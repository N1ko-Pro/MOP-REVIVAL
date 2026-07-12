// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Модуль переключения предмета: режим (Full/PhysicsOnly), гварды и оптимизационные тактики.
//   * Free-standing guard — трогаем только свободно лежащий предмет (родитель null или ITEMS).
//   * Settle-окно — после включения держим тело кинематическим ~3с (не проваливается сквозь опору).
//   * Moving-порог — движущийся предмет не выключаем.
//   * Recovery — провалившийся сквозь мир возвращаем к LostSpawner.
// Частные гварды (домкрат, батарея, гриль, рюкзак, шлем, Сатсума) — в HardGuardKeepsActive.

using UnityEngine;

using MOPR.Common;
using MOPR.FSM;
using MOPR.Helpers;
using MOPR.Managers;
using MOPR.Vehicles.Cases;

namespace MOPR.Items
{
    internal partial class ItemBehaviour
    {
        #region Переключение (вызывается циклом / масс-тогглом)

        /// <summary>Включает (восстанавливает) или гасит предмет согласно его режиму и гвардам.</summary>
        public void ApplyToggle(bool enable)
        {
            try
            {
                if (enable)
                    Restore();
                else
                    Suspend();
            }
            catch
            {
                // Один проблемный предмет не должен ронять весь проход цикла.
            }
        }

        private void Restore()
        {
            // Режим «прятать пустые предметы»: держим выключенным.
            if (ShouldForceHideEmpty())
            {
                Suspend();
                return;
            }

            active = true;

            if (mode == ItemToggleMode.Full)
            {
                if (!gameObject.activeSelf)
                    gameObject.SetActive(true);

                if (rb != null && IsFreeStanding())
                    ReturnToRest(useGravity: false);
            }
            else // PhysicsOnly
            {
                if (renderer != null && !ignoreRenderer)
                    renderer.enabled = true;

                if (rb != null && IsFreeStanding())
                    ReturnToRest(useGravity: true);
            }

            if (eventSound != null)
                StartCoroutine(ToggleEventSound(true));
        }

        /// <summary>
        /// Возвращает свободный предмет в сохранённую позу и держит тело кинематическим до конца
        /// settle-окна (живую физику включит FinalizePhysics, когда опора уже реактивирована).
        /// </summary>
        private void ReturnToRest(bool useGravity)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            // Предметы на фиксированных местах — их позу не трогаем.
            if (PinsPosition)
                transform.position = savedPosition;

            rb.detectCollisions = true;
            if (useGravity)
                rb.useGravity = true;
            rb.isKinematic = true;
            BeginSettle();
        }

        private void Suspend()
        {
            // Не трогаем: спец-гвард, не свободно лежит, ещё движется.
            if (HardGuardKeepsActive() || !IsFreeStanding() || IsMoving)
                return;

            active = false;
            savedPosition = transform.position;

            OnSuspendSpecial();

            if (mode == ItemToggleMode.Full)
            {
                timeDisabled = Time.timeSinceLevelLoad;
                if (eventSound != null)
                    StartCoroutine(ToggleEventSound(false));

                if (gameObject.activeSelf)
                    gameObject.SetActive(false);
            }
            else // PhysicsOnly
            {
                if (rb != null)
                {
                    rb.detectCollisions = false;
                    rb.useGravity = false;
                    rb.isKinematic = true;
                }

                if (renderer != null && !ignoreRenderer && !GrillIsOnFire())
                    renderer.enabled = false;
            }
        }

        /// <summary>
        /// Покадровое сопровождение: анти-дрейф при загрузке, settle, усыпление, recovery.
        /// <paramref name="freezeSleepers"/> = false для активных предметов, которые остаются
        /// включёнными (близко к игроку): их спящие тела НЕ переводим в кинематику, иначе близкие
        /// интерактивные детали (двери/капот/багажник Сатсумы) замерзают.
        /// </summary>
        public void MaintainPhysics(bool freezeSleepers = true)
        {
            if (rb == null)
                return;

            // Во время загрузки пиним свободные предметы на месте (анти-дрейф), даже «выключенные».
            if (!Core.Instance.IsItemInitializationDone())
            {
                if (PinsPosition && IsFreeStanding() && transform.root != Satsuma.Instance.transform)
                    transform.position = savedPosition;
                return;
            }

            // Выключенный предмет физику не считает.
            if (!active)
                return;

            // Завершаем settle-окно: возвращаем живую физику на уже реактивированную опору.
            if (settling && Time.timeSinceLevelLoad >= settleUntil)
                FinalizePhysics();

            // Провалился сквозь мир — возвращаем к спавнеру потеряшек.
            if (transform.position.y < FallThroughY && !CompatibilityManager.IsInBackpack(this))
            {
                transform.position = ItemsManager.Instance.LostSpawner.position;
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                savedPosition = transform.position;
                return;
            }

            // Спящее тело делаем кинематическим — не считаем его физику (не во время settle и только
            // для предметов, которые сейчас гасятся/выгружаются; активные близкие детали не трогаем).
            //
            // НО: hard-guard-предметы (детали Сатсумы — двери/капот/багажник, поднятый домкрат, батарея
            // на зарядке и т.д.) должны оставаться интерактивными и НЕ замораживаются. Иначе их тела
            // застревают кинематическими: Suspend() для них пропущен (HardGuardKeepsActive), а обратный
            // сброс кинематики был только в ветке Full режима ToggleChangeFix — для PhysicsOnly-деталей
            // (двери/капот/багажник) он не срабатывал, и после дальнего отъезда/телепорта они переставали
            // открываться, а сама машина «зависала» (кинематические дети через суставы держат кузов).
            if (freezeSleepers && !settling && rb.IsSleeping() && !rb.isKinematic
                && !CompatibilityManager.IsInBackpack(this) && !HardGuardKeepsActive())
                rb.isKinematic = true;
        }

        private void BeginSettle()
        {
            settling = true;
            settleUntil = Time.timeSinceLevelLoad + SettleSeconds;
        }

        private void FinalizePhysics()
        {
            settling = false;

            if (rb == null || !IsFreeStanding())
                return;

            if (PinsPosition)
                transform.position = savedPosition;

            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.detectCollisions = true;
            rb.useGravity = true;
            rb.isKinematic = false;
        }

        #endregion

        #region Гварды

        /// <summary>Свободно лежит в мире: родитель — null или контейнер ITEMS.</summary>
        private bool IsFreeStanding()
        {
            Transform parent = transform.parent;
            return parent == null || parent.name == "ITEMS";
        }

        /// <summary>Предмет должен оставаться активным несмотря на дистанцию (частные гварды).</summary>
        private bool HardGuardKeepsActive()
        {
            if (dontDisable || isObjectOnGrill || GrillIsOnFire())
                return true;

            if (transform.root == Satsuma.Instance.transform)
                return true;

            if (CompatibilityManager.IsInBackpack(this))
                return true;

            // Домкрат поднят (держит машину) — не выключаем.
            if (floorJackTriggerY != null && floorJackTriggerY.Value >= 0.15f)
                return true;

            // Батарея на зарядке — не выключаем.
            if (batteryOnCharged != null && batteryOnCharged.Value)
                return true;

            // Надетый шлем рядом с игроком — не выключаем.
            if (gameObject.name == "helmet(itemx)" && Vector3.Distance(transform.position, Core.Instance.GetPlayer().position) < 5)
                return true;

            return false;
        }

        /// <summary>Предмет ещё движется/катится/падает — выключать рано.</summary>
        private bool IsMoving => rb != null && !rb.isKinematic && rb.velocity.sqrMagnitude > MovingThresholdSqr;

        /// <summary>Настройка «прятать пустые предметы»: пустую тару держим выключенной.</summary>
        private bool ShouldForceHideEmpty()
        {
            return MoprSettings.DisableEmptyItemsOn && gameObject.name == "empty(itemx)" && transform.parent == null;
        }

        /// <summary>Предметы на фиксированных местах — их позу при восстановлении не трогаем.</summary>
        private bool PinsPosition
            => !gameObject.name.EqualsAny("lottery ticket(xxxxx)", "envelope(xxxxx)", "teimo advert pile(itemx)");

        /// <summary>Частные действия при выключении конкретных предметов.</summary>
        private void OnSuspendSpecial()
        {
            switch (gameObject.name)
            {
                // Гасим бесконечный огонь мусорной бочки.
                case "garbage barrel(itemx)":
                    Transform fire = transform.Find("Fire");
                    if (fire != null)
                        fire.gameObject.SetActive(false);
                    break;
                // Крышка ведра: отключаем removalFSM, чтобы не отсоединялась, пока игрок далеко.
                case "bucket lid(itemx)":
                    if (removalFSM != null)
                        removalFSM.enabled = false;
                    break;
                // Пустая канистра у свалки: переименовываем и делаем подбираемой.
                case "emptyca":
                    if (ItemsManager.Instance.LandfillSpawn != null
                        && Vector3.Distance(transform.position, ItemsManager.Instance.LandfillSpawn.position) < 5)
                    {
                        gameObject.name = "empty plastic can(itemx)";
                        gameObject.MakePickable();
                    }
                    break;
            }
        }

        #endregion

        #region Запросы состояния для цикла

        /// <summary>Активен ли предмет с точки зрения оптимизации (в PhysicsOnly — по detectCollisions).</summary>
        public bool ActiveSelf => (mode == ItemToggleMode.PhysicsOnly && rb != null) ? rb.detectCollisions : gameObject.activeSelf;

        /// <summary>Чинит крышку ведра, восстанавливает рендер и «живую» физику при повторном включении.</summary>
        internal void ToggleChangeFix()
        {
            // Фикс: крышка ведра отсоединяется, пока игрок далеко.
            if (gameObject.name == "bucket lid(itemx)")
            {
                if (transform.parent != null && transform.parent.gameObject.name == "PivotLid" && removalFSM != null)
                {
                    removalFSM.enabled = true;
                    transform.localEulerAngles = Vector3.zero;
                }
            }

            // Дальнейшее — только для предметов в режиме полного переключения.
            if (mode != ItemToggleMode.Full)
                return;

            // Восстанавливаем рендер, если он остался выключенным.
            if (renderer != null && !renderer.enabled && !ignoreRenderer)
                renderer.enabled = true;

            // КЛЮЧЕВОЙ ФИКС: возвращаем «живую» физику телам, замороженным усыплением. MaintainPhysics
            // ставит спящим телам isKinematic=true; без обратного сброса детали Сатсумы (двери/капот/
            // багажник — это тоже ItemBehaviour) остаются кинематическими и не открываются: играет звук,
            // но движения нет, а сама машина «висит в воздухе».
            if (rb != null && (rb.isKinematic || !rb.useGravity || !rb.detectCollisions))
            {
                rb.isKinematic = false;
                rb.useGravity = true;
                rb.detectCollisions = true;
            }
        }

        #endregion
    }
}
