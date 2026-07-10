// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Трактор Kekmet. При переключении тянет за собой прицеплённый Флэтбед (ToggleByKekmet). Гасит
// рестарт FSM счётчика моточасов и сцепки прицепа, добавляет ручной газ. Передний крюк у Кекмета
// находится в другом месте иерархии — переопределяем поиск крюков.

using UnityEngine;

using MOPR.Managers;
using MOPR.Common.Enumerations;
using MOPR.FSM;
using MOPR.Vehicles.Managers;

namespace MOPR.Vehicles.Cases
{
    internal class Kekmet : Vehicle
    {
        private readonly Flatbed flatbed;

        public Kekmet(string gameObjectName) : base(gameObjectName)
        {
            vehicleType = VehiclesTypes.Kekmet;

            flatbed = VehicleManager.Instance.GetVehicle(VehiclesTypes.Flatbed) as Flatbed;

            transform.Find("Dashboard/HourMeter").gameObject.GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;

            gameObject.AddComponent<KekmetHandThrottle>();

            transform.Find("Trailer/Hook").GetPlayMaker("Distance").Fsm.RestartOnEnable = false;
        }

        internal override void ToggleActive(bool enabled)
        {
            if (gameObject == null || gameObject.activeSelf == enabled || !IsActive)
                return;

            flatbed.IsAttached = FsmManager.IsTrailerAttached();

            if (!enabled)
            {
                MoveNonDisableableObjects(temporaryParent);
                Position = transform.localPosition;
                Rotation = transform.localRotation;
                colliders.parent = temporaryParent;
            }

            gameObject.SetActive(enabled);

            if (flatbed.IsAttached)
                flatbed.ToggleByKekmet(enabled);

            if (enabled)
            {
                MoveNonDisableableObjects(null);
                colliders.parent = transform;
                colliders.localPosition = colliderPosition;
            }
        }

        protected override void DisableHooksResetting()
        {
            Transform hookFront = transform.Find("Frontloader/ArmPivot/Arm/LoaderPivot/Loader/RopePoint/HookFront");
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
    }
}
