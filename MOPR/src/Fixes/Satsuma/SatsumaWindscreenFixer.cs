// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Фикс ремонта лобового стекла: помечает стекло на ремонт (FixWindscreen) и при следующем включении
// объекта посылает событие REPAIR его FSM.

using UnityEngine;

namespace MOPR.Vehicles.Managers.SatsumaManagers
{
    internal class SatsumaWindscreenFixer : MonoBehaviour
    {
        private bool fixWindscreen;
        private PlayMakerFSM fsm;

        private void Awake()
        {
            fsm = GetComponent<PlayMakerFSM>();
        }

        private void OnEnable()
        {
            if (fixWindscreen)
            {
                fsm.SendEvent("REPAIR");
                fixWindscreen = false;
            }
        }

        internal void FixWindscreen()
        {
            fixWindscreen = true;
        }
    }
}
