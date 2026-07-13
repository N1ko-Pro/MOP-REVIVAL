// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Оптимизация Сатсумы (ядро). Самый сложный объект игры: отдельные узлы включаются/выключаются по
// дистанции и состоянию (ключ/двигатель/парк-ферме/инспекция), рендереры кулятся, а физика
// «замораживается» константами, чтобы машина не дёргалась и не переворачивалась. Конструктор лишь
// оркеструет разовые правки/фиксы — их тела вынесены в Satsuma.Setup.cs. Специфические фиксы-узлы
// (болты, петли, стекло и т.д.) живут в Fixes/Satsuma (namespace MOPR.Vehicles.Managers.SatsumaManagers).

using HutongGames.PlayMaker;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using MOPR.FSM;
using MOPR.Common;
using MOPR.Common.Enumerations;
using MOPR.Vehicles.Managers.SatsumaManagers;
using MOPR.Rules;
using MOPR.Rules.Types;

namespace MOPR.Vehicles.Cases
{
    internal partial class Satsuma : Vehicle
    {
        private const int MinimumBolts = 163;

        private static Satsuma instance;
        public static Satsuma Instance => instance;

        // Блокируют выключение физики Сатсумы в соответствующих зонах.
        public bool IsSatsumaInInspectionArea;
        public bool IsSatsumaInParcFerme;

        // Ложь до первого включения (нужно фиксу капота).
        public bool AfterFirstEnable;

        // Узлы, которые можно выключать (по whitelist).
        private readonly Transform[] disableableObjects;

        private readonly string[] whiteList =
        {
            "DriveTrigger", "SpawnPlayer", "CarShadowProjector", "Hooks", "CameraPivot", "PivotSeatR", "HookRear", "BeerCaseTarget",
            "HookFront", "GetInPivot", "TrafficTrigger", "Wipers", "WiperLeftPivot", "WiperRightPivot", "wipers_tap", "wipers_rod",
            "generalPivot", "CarRearMirrorPivot", "StagingWheel", "shadow_body", "NormalFront", "NormalSide", "NormalRear", "WindSide",
            "WindFront", "WindRear", "CoG", "Interior", "Body", "MiscParts", "Dashboard"
        };
        private readonly string[] ignoredRendererNames = { "Sphere", "Capsule", "Cube", "Mokia" };

        // «Masked» объекты — их болты скрыты, пока перекрывающая деталь не снята.
        private readonly string[] maskedObjectNames =
        {
            "MaskedClutchCover", "MaskedBearing2", "MaskedBearing3", "MaskedFlywheel",
            "MaskedFlywheelRacing", "MaskedPiston2", "MaskedPiston3", "MaskedPiston4"
        };

        private readonly List<Renderer> renderers;
        private bool renderersOnCloseEnabled;

        private readonly Transform pivotHood;
        // Поля ниже инициализируются в setup-методах (Satsuma.Setup.cs), а не в конструкторе,
        // поэтому не могут быть readonly.
        private GameObject key;

        private List<SatsumaOnActionObjects> satsumaOnActionObjects;

        private List<SatsumaBoltsAntiReload> satsumaBoltsAntiReloads;
        private bool partsUnglued;

        private Dictionary<GameObject, bool> maskedElements;
        private int maskedFixStages;

        private List<Material> dashboardMaterials;
        private FsmInt lightSelection;

        private GameObject cooldownTick;
        private PlayMakerFSM drivingAI;

        // Пока Флеетари перегоняет машину — не телепортируем её обратно к последней «хорошей» позе.
        private bool hasBeenMovedByFleetari;

        internal Quaternion lastGoodRotation;
        internal Vector3 lastGoodPosition;
        private bool lastGoodRotationSaved;

        // Проверка «в воздухе»: сколько кадров ещё считать машину летящей.
        private int framesLeftToSkipForGroundCheckDueToFlight;
        private const int FramesToSkip = 30;
        private const float VelocityCheckDeadzone = 0.2f;

        // Сглаживание микрофризов при подходе: переключение узлов «по состоянию» (CarSimulation и пр.)
        // не выполняем разом в одном тике цикла, а копим и применяем по нескольку за кадр (TickPacedWork
        // вызывается каждый кадр из Core.Update). Итоговое состояние идентично — только размазано.
        private const int PacedTogglesPerFrame = 2;
        private readonly Queue<PacedToggle> pacedToggles = new Queue<PacedToggle>();

        private struct PacedToggle
        {
            public PlayMakerFSM Fsm;
            public GameObject GameObject;
            public bool State;
        }

        public Satsuma(string gameObject) : base(gameObject)
        {
            instance = this;
            vehicleType = VehiclesTypes.Satsuma;

            disableableObjects = GetDisableableChilds();
            Toggle = ToggleActive;

            pivotHood = this.gameObject.transform.Find("Body/pivot_hood");
            renderers = GetRenderersToDisable();

            IgnoreRule vehicleRule = RulesManager.Instance.GetList<IgnoreRule>().Find(v => v.ObjectName == this.gameObject.name);
            if (vehicleRule != null)
            {
                Toggle = IgnoreToggle;
                if (vehicleRule.TotalIgnore)
                    IsActive = false;
            }

            lastGoodRotation = transform.localRotation;
            lastGoodPosition = transform.localPosition;

            // Полный отказ от управления Сатсумой (правило satsuma_ignore): для модов, которые
            // целиком переделывают Сатсуму (напр. Satsuma LX). Мы НЕ вешаем фиксы/компоненты и не
            // собираем списки болтов/узлов — иначе наши операции по заменённым модом деталям валят
            // игру при сохранении (OnSaveGlueAll/ToggleElements). Коллекции инициализируем пустыми,
            // чтобы весь остальной код (циклы по спискам) оставался безопасным.
            if (RulesManager.Instance.SpecialRules.SatsumaIgnore)
            {
                Toggle = IgnoreToggle;
                IsActive = false;
                satsumaBoltsAntiReloads = new List<SatsumaBoltsAntiReload>();
                satsumaOnActionObjects = new List<SatsumaOnActionObjects>();
                maskedElements = new Dictionary<GameObject, bool>();
                dashboardMaterials = new List<Material>();
                rb.isKinematic = true;
                return;
            }

            // Разовые правки/фиксы (тела — в Satsuma.Setup.cs). Каждый шаг изолирован своим try/catch.
            SetupSeats();
            SetupMechanicalWearAndHandbrake();
            SetupBatteryTerminal();
            BoltsFix();
            SetupBodyPaint();
            WipeLoadStates();
            SetupRadioAndCd();
            SetupWindowGrille();
            SetupHoodFix();
            SetupRearBumper();
            SetupPartMassManagers();
            SetupKey();
            SetupCooldownTick();
            SetupRegPlatesAndDashboard();
            SetupDoors();
            SetupOnActionObjects();
            SetupAssembleAudio();
            SetupColliderSetup();
            SetupDriverHead();
            SetupMaskedElements();
            SetupDrivingAI();
            SetupRadiatorHose();
            SetupWindscreen();
            SetupFireExtinguisherHolder();
            SetupOdometer();
            SetupSubframeBolts();
            SetupExhaustMuffler();
            SetupBlockHinge();
            SetupDriverHeadHinge();
            SetupDrivingOptimizer();
            SetupParkingBrakeAnchor();

            if (IsActive)
                ForceToggleUnityCar(false);

            if (MoprSettings.GenerateToggledItemsListDebug)
                ToggledItemsListGenerator.CreateSatsumaList(disableableObjects);

            rb.isKinematic = true;
        }

        #region Переключение

        internal override void ToggleActive(bool enabled)
        {
            if (IsPlayerInThisCar())
                enabled = true;

            if (!AfterFirstEnable && enabled)
                AfterFirstEnable = true;

            // ИИ ведёт машину — принудительно включаем.
            if (drivingAI != null && drivingAI.enabled)
                enabled = true;

            // В парк-ферме рендереры могут не включиться обратно — форсируем.
            if (IsSatsumaInParcFerme && !renderers[0].enabled)
                RenderersCulling(enabled);

            if (!RulesManager.Instance.SpecialRules.SatsumaIgnoreRenderers)
            {
                bool dashIllumination = false;
                if (IsKeyInserted())
                    dashIllumination = lightSelection != null && lightSelection.Value > 0;

                ToggleDashIllumination(dashIllumination);
            }

            // Состояние уже нужное — выходим.
            if (gameObject == null || disableableObjects[0].gameObject.activeSelf == enabled)
                return;

            if (!enabled)
            {
                lastGoodRotation = transform.rotation;
                lastGoodPosition = transform.position;
            }

            // Пока фикс капота не завершён — держим машину включённой.
            if (!GameFixes.Instance.HoodFixDone)
                enabled = true;

            for (int i = 0; i < disableableObjects.Length; i++)
            {
                if (disableableObjects[i] == null)
                    continue;

                bool isElementEnabled = IsSatsumaInParcFerme || enabled;
                disableableObjects[i].gameObject.SetActive(isElementEnabled);
            }

            RenderersCulling(enabled);
        }

        public override void ToggleUnityCar(bool enabled)
        {
            // В зоне инспекции / на тросе / не на земле — физику не выключаем.
            if ((!enabled && IsSatsumaInInspectionArea) || IsRopeHooked() || !IsOnGround())
                enabled = true;

            base.ToggleUnityCar(enabled);

            // Полностью замораживаем позицию, чтобы Сатсума не переворачивалась.
            SaveCarPosition(enabled);
        }

        public override void ForceToggleUnityCar(bool enabled)
        {
            base.ForceToggleUnityCar(enabled);
            SaveCarPosition(enabled);
        }

        private void SaveCarPosition(bool enabled)
        {
            if (!enabled && !lastGoodRotationSaved)
            {
                lastGoodRotationSaved = true;
                lastGoodRotation = transform.localRotation;
                lastGoodPosition = transform.localPosition;
            }

            if (enabled)
                lastGoodRotationSaved = false;

            rb.constraints = enabled ? RigidbodyConstraints.None : RigidbodyConstraints.FreezePosition;
        }

        public override bool IsOnGround()
        {
            // Колесо не прикреплено — судим по крутящему моменту и скорости кузова.
            if (!wheel.enabled)
            {
                if (rb.velocity.magnitude > VelocityCheckDeadzone)
                    framesLeftToSkipForGroundCheckDueToFlight = FramesToSkip;

                if (framesLeftToSkipForGroundCheckDueToFlight > 0)
                {
                    framesLeftToSkipForGroundCheckDueToFlight--;
                    return false;
                }

                return drivetrain.torque == 0 && rb.velocity.sqrMagnitude <= VelocityCheckDeadzone;
            }

            return wheel.onGroundDown;
        }

        #endregion

        #region Рендереры / элементы / поза

        /// <summary>Список переключаемых дочерних узлов (по whitelist, с учётом ignore-правил).</summary>
        internal Transform[] GetDisableableChilds()
        {
            Transform[] childs = gameObject.GetComponentsInChildren<Transform>(true)
                .Where(t => t.gameObject.name.ContainsAny(whiteList)).ToArray();

            if (RulesManager.Instance.GetList<IgnoreRule>().Count > 0)
                childs = childs.Where(t => !RulesManager.Instance.IsObjectInIgnoreList(t.gameObject)).ToArray();

            return childs;
        }

        private bool IsHoodAttached() => pivotHood.childCount > 0;

        /// <summary>Включает/выключает все рендереры Сатсумы (кроме её корня и по ignore-правилам).</summary>
        private void RenderersCulling(bool enabled)
        {
            // Правило satsuma_ignore: рендереры Сатсумы не трогаем вовсе.
            if (RulesManager.Instance.SpecialRules.SatsumaIgnore)
                return;

            if (RulesManager.Instance.SpecialRules.SatsumaIgnoreRenderers)
                enabled = true;

            for (int i = 0; i < renderers.Count; i++)
            {
                try
                {
                    if (renderers[i] == null)
                        continue;

                    // Пропускаем рендерер, если его корень — не Сатсума.
                    if (renderers[i].transform.root.gameObject != gameObject)
                        continue;

                    renderers[i].enabled = enabled;
                }
                catch (System.Exception ex)
                {
                    ExceptionManager.New(ex, true, "SATSUMA_RENDERER_TOGGLE_ISSUE");
                }
            }
        }

        /// <summary>Удерживает позу Сатсумы при выключенной физике (иначе она дрожит).</summary>
        public void ForceRotation()
        {
            if (carDynamics == null || IsPlayerInThisCar() || IsRopeHooked())
                return;

            if (!hasBeenMovedByFleetari && !carDynamics.enabled)
            {
                transform.localRotation = lastGoodRotation;
                transform.localPosition = lastGoodPosition;
            }
        }

        public bool IsKeyInserted() => key == null || key.activeSelf;

        /// <summary>Переключает часть узлов Сатсумы в зависимости от дистанции игрока.</summary>
        public void ToggleElements(float distance)
        {
            // Правило satsuma_ignore: Сатсума полностью на попечении другого мода — не трогаем её узлы.
            if (RulesManager.Instance.SpecialRules.SatsumaIgnore)
                return;

            try
            {
                bool onEngine = distance < 2;
                bool onClose = distance <= 10 * MoprSettings.ActiveDistanceMultiplicationValue;
                bool onFar = distance <= 20 * MoprSettings.ActiveDistanceMultiplicationValue;

                // Ряд условий держит все узлы включёнными.
                if (Toggle == IgnoreToggle || IsKeyInserted() || IsSatsumaInInspectionArea || IsMoving() || (drivingAI != null && drivingAI.enabled))
                {
                    onEngine = true;
                    onClose = true;
                    onFar = true;
                }

                for (int i = 0; i < satsumaOnActionObjects.Count; i++)
                {
                    SatsumaOnActionObjects sao = satsumaOnActionObjects[i];
                    if (sao == null)
                        continue;

                    bool state = StateFor(sao.EnableOn, onEngine, onClose, onFar);

                    // Копим изменение состояния (применит TickPacedWork по кадрам) — сглаживает
                    // микрофриз при подходе, когда за один тик включается CarSimulation и др.
                    EnqueuePacedToggle(sao.FSM, sao.GameObject, state);
                }

                if (onEngine)
                {
                    // Фикс: болты остаются «незакрученными», хотя деталь внутри полностью прикручена.
                    if (maskedFixStages < 2)
                    {
                        for (int i = 0; i < maskedElements.Count; i++)
                        {
                            GameObject gm = maskedElements.ElementAt(i).Key;
                            if (gm == null)
                                continue;

                            if (maskedFixStages == 0)
                                gm.SetActive(true);
                            else if (maskedFixStages == 1)
                                gm.SetActive(maskedElements.ElementAt(i).Value);
                        }

                        maskedFixStages++;
                    }
                }
                else
                {
                    cooldownTick.SetActive(false);
                }

                if (onClose)
                {
                    if (!renderersOnCloseEnabled)
                    {
                        renderersOnCloseEnabled = true;
                        RenderersCulling(true);
                    }
                }
                else
                {
                    renderersOnCloseEnabled = false;
                }

                if (onFar)
                {
                    hasBeenMovedByFleetari = false;
                    UnglueAll();
                }
            }
            catch (System.Exception ex)
            {
                ExceptionManager.New(ex, true, "SATSUMA_TOGGLE_ELEMENTS_ERROR");
            }
        }

        private static bool StateFor(SatsumaEnableOn enableOn, bool onEngine, bool onClose, bool onFar)
        {
            switch (enableOn)
            {
                case SatsumaEnableOn.OnEngine: return onEngine;
                case SatsumaEnableOn.OnPlayerClose: return onClose;
                case SatsumaEnableOn.OnPlayerFar: return onFar;
                default: return true;
            }
        }

        /// <summary>Ставит переключение узла в очередь, только если целевое состояние отличается от текущего.</summary>
        private void EnqueuePacedToggle(PlayMakerFSM fsm, GameObject go, bool state)
        {
            bool needsFsm = fsm != null && fsm.enabled != state;
            bool needsGo = go != null && go.activeSelf != state;
            if (!needsFsm && !needsGo)
                return;

            pacedToggles.Enqueue(new PacedToggle { Fsm = fsm, GameObject = go, State = state });
        }

        /// <summary>Применяет несколько отложенных переключений за кадр (вызывается из Core.Update).</summary>
        public void TickPacedWork()
        {
            ApplyPacedToggles(PacedTogglesPerFrame);
        }

        /// <summary>
        /// Немедленно применяет ВСЕ отложенные переключения. Нужно перед сохранением/загрузкой
        /// (ToggleAll), где узлы обязаны прийти в целевое состояние синхронно, до сериализации.
        /// </summary>
        public void FlushPacedToggles()
        {
            ApplyPacedToggles(int.MaxValue);
        }

        private void ApplyPacedToggles(int budget)
        {
            while (budget-- > 0 && pacedToggles.Count > 0)
            {
                PacedToggle t = pacedToggles.Dequeue();
                try
                {
                    if (t.Fsm != null && t.Fsm.enabled != t.State)
                        t.Fsm.enabled = t.State;

                    if (t.GameObject != null && t.GameObject.activeSelf != t.State)
                        t.GameObject.SetActive(t.State);
                }
                catch (System.Exception ex)
                {
                    ExceptionManager.New(ex, false, "SATSUMA_PACED_TOGGLE_ERROR");
                }
            }
        }

        private void ToggleDashIllumination(bool enabled)
        {
            if (dashboardMaterials == null
                || (dashboardMaterials[0].GetFloat("_Intensity") == 0 && !enabled)
                || (dashboardMaterials[0].GetFloat("_Intensity") == 0.2f && enabled))
                return;

            for (int i = 0; i < dashboardMaterials.Count; i++)
                dashboardMaterials[i].SetFloat("_Intensity", enabled ? 0.2f : 0);
        }

        public GameObject GetCarBody() => transform.Find("Body/car body(xxxxx)").gameObject;

        public void FleetariIsMovingCar() => hasBeenMovedByFleetari = true;

        #endregion

        #region Болты (anti-reload) / рендереры

        internal void AddPart(SatsumaBoltsAntiReload i) => satsumaBoltsAntiReloads.Add(i);

        private void UnglueAll()
        {
            if (partsUnglued)
                return;
            partsUnglued = true;

            List<SatsumaBoltsAntiReload> brokens = new List<SatsumaBoltsAntiReload>();

            for (int i = 0; i < satsumaBoltsAntiReloads.Count; i++)
            {
                if (satsumaBoltsAntiReloads[i] == null)
                {
                    brokens.Add(satsumaBoltsAntiReloads[i]);
                    continue;
                }

                satsumaBoltsAntiReloads[i]?.Unglue();
            }

            ModConsole.Log("[MOPR] Found " + brokens.Count + " broken SatsumaBoltsAntiReload.");
            foreach (SatsumaBoltsAntiReload broken in brokens)
                satsumaBoltsAntiReloads.Remove(broken);
        }

        /// <summary>При сохранении: намертво «приклеивает» все детали к машине.</summary>
        public void OnSaveGlueAll()
        {
            // Правило satsuma_ignore: не склеиваем детали — их болты/джойнты могут быть заменены модом.
            if (RulesManager.Instance.SpecialRules.SatsumaIgnore)
                return;

            for (int i = 0; i < satsumaBoltsAntiReloads.Count; i++)
            {
                if (satsumaBoltsAntiReloads[i] == null)
                    continue;

                satsumaBoltsAntiReloads[i].enabled = true;
                satsumaBoltsAntiReloads[i].Glue();

                PlayMakerFSM removalFSM = satsumaBoltsAntiReloads[i].gameObject.GetPlayMaker("Removal");
                if (removalFSM)
                    Object.Destroy(removalFSM);
            }

            ModConsole.Log("[MOPR] Glued all Satsuma parts.");
        }

        private List<Renderer> GetRenderersToDisable()
        {
            List<Renderer> found = gameObject.transform.GetComponentsInChildren<Renderer>(true)
                .Where(r => !r.gameObject.name.ContainsAny(ignoredRendererNames)).ToList();

            if (RulesManager.Instance.GetList<IgnoreRule>().Count > 0)
                found = found.Where(r => !RulesManager.Instance.IsObjectInIgnoreList(r.gameObject)).ToList();

            List<Renderer> toReturn = new List<Renderer>();
            foreach (Renderer r in found)
            {
                try
                {
                    MeshFilter f = r.GetComponent<MeshFilter>();
                    // Пропускаем примитивный куб (служебная геометрия).
                    if (!(f && f.sharedMesh.name == "Cube" && f.mesh.subMeshCount == 1 && f.mesh.vertexCount == 24))
                        toReturn.Add(r);
                }
                catch
                {
                }
            }

            return toReturn;
        }

        // У Сатсумы петли обрабатываются точечно (SatsumaHingeManager), общий HingeManager не нужен.
        protected override void ApplyHingeManager()
        {
        }

        // Коллайдеры Сатсумы настраиваются в SetupColliderSetup, базовую загрузку отключаем.
        protected override void LoadColliders()
        {
        }

        #endregion
    }
}
