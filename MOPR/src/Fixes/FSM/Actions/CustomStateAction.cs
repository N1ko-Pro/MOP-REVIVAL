// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Экшен-обёртка: выполняет переданный C#-колбэк при входе в состояние и завершается. Используется
// FsmInject для внедрения кода мода в игровые FSM.

using HutongGames.PlayMaker;
using System;

namespace MOPR.FSM.Actions
{
    internal class CustomStateAction : FsmStateAction
    {
        private readonly Action action;

        public CustomStateAction(Action action)
        {
            this.action = action;
        }

        public override void OnEnter()
        {
            action?.Invoke();
            Finish();
        }
    }
}
