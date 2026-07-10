// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Экшен массового включения/выключения болтов узла: собирает родителя и всех его потомков и
// разом переключает их активность (используется для скрытия болтов, пока деталь не в фокусе).

using System.Collections.Generic;
using HutongGames.PlayMaker;
using UnityEngine;

namespace MOPR.FSM.Actions
{
    internal class CustomToggleAllBoltsAction : FsmStateAction
    {
        private readonly List<GameObject> bolts;
        private readonly bool isEnabled;

        public CustomToggleAllBoltsAction(GameObject boltsParent, bool isEnabled)
        {
            this.isEnabled = isEnabled;
            bolts = new List<GameObject> { boltsParent.gameObject };
            foreach (Transform child in boltsParent.GetComponentsInChildren<Transform>())
                bolts.Add(child.gameObject);
        }

        public override void OnEnter()
        {
            foreach (GameObject obj in bolts)
                obj.SetActive(isEnabled);

            Finish();
        }
    }
}
