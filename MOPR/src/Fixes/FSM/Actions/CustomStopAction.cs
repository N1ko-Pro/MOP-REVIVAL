// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// «Пустой» экшен: сразу шлёт FINISHED и завершается. Ставится вместо вырезанных экшенов состояния,
// чтобы FSM продолжал переход, но ничего лишнего не выполнял.

using HutongGames.PlayMaker;

namespace MOPR.FSM.Actions
{
    public class CustomStop : FsmStateAction
    {
        public override void OnEnter()
        {
            Fsm.Event("FINISHED");
            Finish();
        }
    }
}
