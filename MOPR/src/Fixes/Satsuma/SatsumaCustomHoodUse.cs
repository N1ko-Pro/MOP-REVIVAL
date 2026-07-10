// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Фикс невозможности закрыть капот Сатсумы: следит за состояниями FSM капота и, когда капот открыт
// и его угол превышает порог, принудительно шлёт CLOSE.

using UnityEngine;
using MOPR.FSM;

namespace MOPR.Vehicles.Managers.SatsumaManagers
{
    internal class SatsumaCustomHoodUse : MonoBehaviour
    {
        public bool isActive;
        public bool isHoodOpen;

        private PlayMakerFSM useFsm;

        private void Start()
        {
            gameObject.FsmInject("State 2", () => isActive = true);
            gameObject.FsmInject("Mouse off", () => isActive = false);
            gameObject.FsmInject("Open hood 2", () => isHoodOpen = true);
            gameObject.FsmInject("Close hood 2", () => isHoodOpen = false);

            useFsm = gameObject.GetPlayMaker("Use");
        }

        private void FixedUpdate()
        {
            if (!IsHoodAttached() || (!isHoodOpen && !isActive))
                return;

            if (transform.localEulerAngles.x > 340)
                useFsm.SendEvent("CLOSE");
        }

        private void OnEnable()
        {
            isHoodOpen = false;
            isActive = false;
        }

        private bool IsHoodAttached()
        {
            return transform.parent != null && transform.parent.gameObject.name == "pivot_hood";
        }
    }
}
