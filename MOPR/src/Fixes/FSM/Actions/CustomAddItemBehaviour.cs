// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Экшен: при спавне предмета игровым FSM навешивает на него DelayedItemBehaviour, чтобы предмет
// попал под управление MOPR (с задержкой на кадр — иначе часть переменных предмета ещё не готова).

using HutongGames.PlayMaker;

namespace MOPR.FSM.Actions
{
    internal class CustomAddItemBehaviour : FsmStateAction
    {
        private readonly FsmGameObject go;

        public CustomAddItemBehaviour(FsmGameObject go)
        {
            this.go = go;
        }

        public override void OnEnter()
        {
            go.Value.AddComponent<DelayedItemBehaviour>();
        }
    }
}
