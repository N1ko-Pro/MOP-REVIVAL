// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Помощник денег в чемодане: разово гасит перезапуск FSM при активации, чтобы состояние денег
// (сумма) не сбрасывалось, когда чемодан снова включается.

using UnityEngine;

namespace MOPR.Items.Helpers
{
    internal class SuitcaseMoneyBehaviour : MonoBehaviour
    {
        private bool triggered;

        private void OnEnable()
        {
            // Отключаем перезапуск FSM только один раз — при первой активации.
            if (!triggered)
            {
                GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;
                triggered = true;
            }
        }
    }
}
