// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Прицеп-платформа Флэтбед. Пока прицеплен к Кекмету — им управляет Кекмет (ToggleByKekmet), а
// собственное переключение блокируется (IsAttached). Гасит рестарт FSM триггера брёвен и убирает
// FixedJoint отцепки, добавляет фикс брёвен, проваливающихся под пол.

using UnityEngine;
using MOPR.Common.Enumerations;
using MOPR.FSM;
using MOPR.Vehicles.Managers;

namespace MOPR.Vehicles.Cases
{
    internal class Flatbed : Vehicle
    {
        public bool IsAttached { get; set; }

        public Flatbed(string gameObjectName) : base(gameObjectName)
        {
            vehicleType = VehiclesTypes.Flatbed;

            transform.Find("Bed/LogTrigger").gameObject.GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;

            GameObject trailerLogUnderFloorCheck = new GameObject("MOPR_TrailerLogUnderFloorFix");
            trailerLogUnderFloorCheck.transform.parent = gameObject.transform;
            trailerLogUnderFloorCheck.AddComponent<TrailerLogUnderFloor>();

            // Сцепка с трактором.
            PlayMakerFSM detach = gameObject.GetPlayMaker("Detach");
            detach.Fsm.RestartOnEnable = false;
            Object.Destroy(detach.FsmVariables.GetFsmGameObject("DetachPivot").Value.gameObject.GetComponent<FixedJoint>());
        }

        internal override void ToggleActive(bool enabled)
        {
            if (IsAttached)
                return;
            if (gameObject == null || gameObject.activeSelf == enabled || !IsActive)
                return;

            if (!enabled)
            {
                MoveNonDisableableObjects(temporaryParent);
                Position = transform.localPosition;
                Rotation = transform.localRotation;
            }

            gameObject.SetActive(enabled);

            if (enabled)
                MoveNonDisableableObjects(null);
        }

        /// <summary>Переключение прицепа заодно с Кекметом, когда он прицеплен.</summary>
        public void ToggleByKekmet(bool enabled)
        {
            if (!enabled)
            {
                MoveNonDisableableObjects(temporaryParent);
                Position = transform.localPosition;
                Rotation = transform.localRotation;
                colliders.parent = temporaryParent;
            }

            gameObject.SetActive(enabled);

            if (enabled)
            {
                MoveNonDisableableObjects(null);
                colliders.parent = transform;
                colliders.localPosition = colliderPosition;
            }
        }
    }
}
