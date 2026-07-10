// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Экшен присоединения заднего бампера Сатсумы: уведомляет RearBumperBehaviour о повторной привязке.

using MOPR.Items.Helpers;

namespace MOPR.FSM.Actions
{
    internal class CustomSatsumaBumperAttach : CustomSatsumaBumperDetach
    {
        public CustomSatsumaBumperAttach(RearBumperBehaviour behaviour) : base(behaviour) { }

        public override void OnEnter()
        {
            behaviour.OnAttach();
            Finish();
        }
    }
}
