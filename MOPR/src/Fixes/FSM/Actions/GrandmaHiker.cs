// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Экшен: включает/выключает аниматор бабки-автостопщицы при посадке/высадке из машины (иначе её
// анимация конфликтует с оптимизацией транспорта).

using HutongGames.PlayMaker;
using UnityEngine;

namespace MOPR.FSM.Actions
{
    internal class GrandmaHiker : FsmStateAction
    {
        private readonly Animator animator;
        private readonly bool toEnable;

        public GrandmaHiker(GameObject skeleton, bool toEnable)
        {
            animator = skeleton.GetComponent<Animator>();
            this.toEnable = toEnable;
        }

        public override void OnEnter()
        {
            // При посадке в машину аниматор выключается, при высадке — включается.
            animator.enabled = toEnable;
        }
    }
}
