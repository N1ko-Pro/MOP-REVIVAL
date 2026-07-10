// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// База оптимизации транспорта. Разрешает системы UnityCar (CarDynamics/Axles/Drivetrain/rb),
// поддерживает несколько стратегий переключения (весь объект / только физика / игнор — по правилам),
// прячет «неотключаемые» узлы (аудио, коллайдеры) под временного родителя на время выгрузки, ставит
// LOD-подставку и уважает крюки троса/посадку игрока. Специфика машин — в наследниках (Cases).

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using MOPR.Common;
using MOPR.Common.Enumerations;
using MOPR.FSM;
using MOPR.LOD;
using MOPR.Items;
using MOPR.Rules;
using MOPR.Rules.Types;
using MOPR.Vehicles.Managers;
using MOPR.Managers;
using MOPR.Common.Interfaces;

namespace MOPR.Vehicles
{
    internal class Vehicle : ILod
    {
        public bool IsActive = true;

        public GameObject gameObject { get; private set; }

        // Сохраняемые/загружаемые значения.
        protected Vector3 Position { get; set; }
        protected Quaternion Rotation { get; set; }
        protected VehiclesTypes vehicleType;
        public VehiclesTypes VehicleType => vehicleType;

        // Объекты, которые нельзя выгружать (иначе баги), уезжают под этого временного родителя.
        protected Transform temporaryParent;
        protected List<PreventToggleOnObject> preventToggleOnObjects = new List<PreventToggleOnObject>();

        // Системы UnityCar и rigidbody.
        public Transform transform => gameObject.transform;
        protected Rigidbody rb;
        protected CarDynamics carDynamics;
        protected Axles axles;
        protected Drivetrain drivetrain;
        protected Wheel wheel;

        // Не даём выключать физику, пока к машине привязан трос.
        protected PlayMakerFSM fsmHookFront, fsmHookRear;

        // Пока используется только Сатсумой.
        private EventSounds eventSounds;

        // Коллайдеры и их позиция.
        protected Transform colliders;
        protected Vector3 colliderPosition;

        protected LodObject dummyCar;

        public Vehicle(string gameObjectName)
        {
            gameObject = GameObject.Find(gameObjectName);
            if (gameObject == null)
                gameObject = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(g => g.name == gameObjectName);

            if (gameObject == null)
            {
                ModConsole.LogError("[MOPR] Could not find " + gameObjectName + " vehicle.");
                return;
            }

            Position = gameObject.transform.localPosition;
            Rotation = gameObject.transform.localRotation;

            vehicleType = VehiclesTypes.Generic;

            // Временный родитель для «неотключаемых» узлов (например, "MOPR_SATSUMA(557kg, 248)").
            temporaryParent = new GameObject("MOPR_" + gameObject.name).transform;

            // Фикс проваливания предметов внутри транспорта сквозь него.
            PlayMakerFSM lodFSM = gameObject.GetPlayMaker("LOD");
            if (lodFSM != null)
            {
                lodFSM.Fsm.RestartOnEnable = false;
                if (lodFSM.FsmStates.FirstOrDefault(s => s.Name == "Fix Collider") != null)
                    lodFSM.GetState("Fix Collider").ClearActions();
            }

            LoadColliders();
            LoadCarElements();
            LoadRules();

            eventSounds = gameObject.GetComponent<EventSounds>();
        }

        protected virtual void LoadCarElements()
        {
            // Аудио-узлы прячем под временного родителя при выгрузке.
            foreach (Transform audioObject in FindAudioObjects())
                preventToggleOnObjects.Add(new PreventToggleOnObject(audioObject));

            // Уровень топлива сбрасывался после респавна.
            Transform fuelTank = gameObject.transform.Find("FuelTank");
            if (fuelTank != null)
            {
                PlayMakerFSM fuelTankFSM = fuelTank.GetComponent<PlayMakerFSM>();
                if (fuelTankFSM)
                    fuelTankFSM.Fsm.RestartOnEnable = false;
            }

            carDynamics = gameObject.GetComponent<CarDynamics>();
            axles = gameObject.GetComponent<Axles>();
            rb = gameObject.GetComponent<Rigidbody>();

            DisableHooksResetting();

            // По умолчанию переключаем весь объект; правило «только физика» переопределяет.
            Toggle = ToggleActive;
            if (RulesManager.Instance.SpecialRules.ToggleAllVehiclesPhysicsOnly)
                Toggle = IgnoreToggle;

            ApplyHingeManager();

            wheel = axles.allWheels[0];
            drivetrain = gameObject.GetComponent<Drivetrain>();

            if (VehicleManager.Instance.IsInVanilaGame(this))
                dummyCar = new LodObject(gameObject);
        }

        /// <summary>Применяет правила из файлов .mopconfig (ignore / prevent-toggle-on-object).</summary>
        private void LoadRules()
        {
            IgnoreRule vehicleRule = RulesManager.Instance.GetList<IgnoreRule>().Find(v => v.ObjectName == gameObject.name);
            if (vehicleRule != null)
            {
                Toggle = IgnoreToggle;
                if (vehicleRule.TotalIgnore)
                    IsActive = false;
            }

            IgnoreRuleAtPlace[] preventToggleRules = RulesManager.Instance.GetList<IgnoreRuleAtPlace>()
                .Where(v => v.Place == gameObject.name).ToArray();

            foreach (IgnoreRuleAtPlace p in preventToggleRules)
            {
                Transform t = transform.FindRecursive(p.ObjectName);
                if (t == null)
                {
                    ModConsole.LogError("[MOPR] Couldn't find " + p.ObjectName + " in " + p.Place + ".");
                    continue;
                }

                preventToggleOnObjects.Add(new PreventToggleOnObject(t));
            }
        }

        /// <summary>Навешивает HingeManager на суставы, если машина не переключается через ToggleUnityCar (или это Hayosiko).</summary>
        protected virtual void ApplyHingeManager()
        {
            if (Toggle != ToggleUnityCar || vehicleType == VehiclesTypes.Hayosiko)
            {
                foreach (HingeJoint joint in gameObject.transform.GetComponentsInChildren<HingeJoint>())
                    joint.gameObject.AddComponent<HingeManager>();
            }
        }

        public delegate void ToggleHandler(bool enabled);
        public ToggleHandler Toggle;

        /// <summary>Включает или выключает машину целиком.</summary>
        internal virtual void ToggleActive(bool enabled)
        {
            if (gameObject == null || gameObject.activeSelf == enabled || !IsActive)
                return;

            // Перед выключением: прячем неотключаемые узлы и коллайдеры, сохраняем позу.
            if (!enabled)
            {
                MoveNonDisableableObjects(temporaryParent);
                Position = transform.localPosition;
                Rotation = transform.localRotation;

                if (colliders)
                    colliders.parent = temporaryParent;
            }

            gameObject.SetActive(enabled);

            // После включения: возвращаем узлы и коллайдеры на место.
            if (enabled)
            {
                MoveNonDisableableObjects(null);

                if (colliders)
                {
                    colliders.parent = transform;
                    colliders.localPosition = colliderPosition;
                }
            }
        }

        /// <summary>Переключает только физику машины (UnityCar).</summary>
        public virtual void ToggleUnityCar(bool enabled)
        {
            if (gameObject == null || !IsActive)
                return;

            if (rb.isKinematic == !enabled && carDynamics.enabled == enabled && rb.useGravity)
                return;

            // Не выключаем физику, пока машина движется или не на земле.
            if ((IsMoving() || !IsOnGround()) && !enabled)
                return;

            // Игрок сидит в этой машине — НИКОГДА не выключаем.
            if (IsPlayerInThisCar())
                enabled = true;

            // Не выключаем, пока привязан трос.
            if (!enabled && gameObject.activeSelf && IsRopeHooked())
                enabled = true;

            carDynamics.enabled = enabled;
            axles.enabled = enabled;
            rb.isKinematic = !enabled;
            rb.useGravity = enabled;

            // Будим тело при включении физики: после загрузки оно оживает «спящим», и подвеска не
            // оседает (висит колесо), пока машину не тряхнёт поездка. WakeUp заставляет физику
            // просчитаться сразу — машина садится на колёса без принудительной езды.
            if (enabled)
                rb.WakeUp();
        }

        public virtual void ForceToggleUnityCar(bool enabled)
        {
            if (gameObject == null || carDynamics.enabled == enabled || !IsActive)
                return;

            if (IsPlayerInThisCar())
                enabled = true;

            carDynamics.enabled = enabled;
            axles.enabled = enabled;
            rb.isKinematic = !enabled;
            rb.useGravity = enabled;

            if (enabled)
                rb.WakeUp();
        }

        /// <summary>Пустой обработчик для случая «переключение игнорируется».</summary>
        internal void IgnoreToggle(bool enabled)
        {
        }

        /// <summary>Дочерние аудио-объекты машины (по вхождению "audio"/"SoundSrc" в имени).</summary>
        private Transform[] FindAudioObjects()
        {
            return gameObject.transform.GetComponentsInChildren<Transform>()
                .Where(obj => obj.gameObject.name.Contains("audio") || obj.gameObject.name.Contains("SoundSrc"))
                .ToArray();
        }

        /// <summary>Привязан ли к машине трос (передний или задний крюк).</summary>
        protected bool IsRopeHooked()
        {
            bool front = fsmHookFront && fsmHookFront.FsmVariables.GetFsmBool("Attached").Value;
            bool rear = fsmHookRear && fsmHookRear.FsmVariables.GetFsmBool("Attached").Value;
            return front || rear;
        }

        /// <summary>Стоит ли машина на земле (по одному из колёс).</summary>
        public virtual bool IsOnGround() => wheel.onGroundDown;

        /// <summary>Движется ли машина.</summary>
        internal bool IsMoving() => rb.velocity.magnitude > 0.5f;

        /// <summary>Включает/выключает компонент EventSounds.</summary>
        public void ToggleEventSounds(bool enabled)
        {
            if (eventSounds.disableSounds == !enabled)
                return;
            if (!enabled && rb.velocity.magnitude > 1)
                enabled = true;
            eventSounds.disableSounds = !enabled;
        }

        /// <summary>Полностью замораживает машину (ItemFreezer) — на время сохранения.</summary>
        public void Freeze()
        {
            gameObject.AddComponent<ItemFreezer>();
        }

        /// <summary>Является ли эта машина той, в которой сидит игрок.</summary>
        protected bool IsPlayerInThisCar()
        {
            return Core.Instance.GetPlayer().transform.root == gameObject.transform;
        }

        /// <summary>Есть ли рядом трафик (NPC_CARS/TRAFFIC) — тогда физику держим включённой.</summary>
        public bool IsTrafficCarInArea()
        {
            if (IsPlayerInThisCar())
                return false;
            if (rb.useGravity)
                return false;

            Collider[] hitColliders = Physics.OverlapSphere(transform.position, 7, 17);
            foreach (Collider hitCollider in hitColliders)
            {
                if (hitCollider.transform.root != null && hitCollider.transform.root.gameObject.name.EqualsAny(Core.Instance.TrafficVehicleRoots))
                    return true;
            }

            return false;
        }

        /// <summary>Переносит неотключаемые узлы под parent (null — вернуть к исходному родителю).</summary>
        protected void MoveNonDisableableObjects(Transform parent)
        {
            for (int i = 0; i < preventToggleOnObjects.Count; i++)
            {
                PreventToggleOnObject p = preventToggleOnObjects[i];
                if (p.ObjectTransform == null)
                    continue;

                p.ObjectTransform.parent = parent ?? p.OriginalParent;
            }
        }

        protected virtual void DisableHooksResetting()
        {
            Transform hookFront = transform.Find("HookFront");
            Transform hookRear = transform.Find("HookRear");

            if (hookFront != null)
            {
                fsmHookFront = hookFront.GetComponent<PlayMakerFSM>();
                fsmHookFront.Fsm.RestartOnEnable = false;
            }

            if (hookRear != null)
            {
                fsmHookRear = hookRear.GetComponent<PlayMakerFSM>();
                fsmHookRear.Fsm.RestartOnEnable = false;
            }
        }

        protected virtual void LoadColliders()
        {
            colliders = transform.Find("Colliders");
            if (colliders == null)
            {
                ModConsole.Log("[MOPR] Could not locate the colliders of vehicle " + gameObject.name);
                return;
            }

            colliderPosition = colliders.localPosition;
        }

        /// <summary>Включает/выключает LOD-подставку машины.</summary>
        public void ToggleLOD(bool enabled)
        {
            dummyCar?.ToggleActive(enabled, transform);
        }

        public LodObject LodObject => dummyCar;
    }
}
