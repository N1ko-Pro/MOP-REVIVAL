// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Экшен отсоединения заднего бампера Сатсумы: уведомляет RearBumperBehaviour, чтобы тот корректно
// снял оптимизационную привязку бампера. База для CustomSatsumaBumperAttach.

using HutongGames.PlayMaker;
using MOPR.Items.Helpers;

namespace MOPR.FSM.Actions
{
    internal class CustomSatsumaBumperDetach : FsmStateAction
    {
        protected RearBumperBehaviour behaviour;

        public CustomSatsumaBumperDetach(RearBumperBehaviour behaviour)
        {
            this.behaviour = behaviour;
        }

        public override void OnEnter()
        {
            behaviour.OnDetach();
            Finish();
        }
    }
}
