// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Экшен, инстанцирующий объект из префаба в позицию родителя и сразу берущий его под управление
// MOPR (навешивает ItemBehaviour). База для CustomCreateHose.

using HutongGames.PlayMaker;
using UnityEngine;

using MOPR.Items;

namespace MOPR.FSM.Actions
{
    internal class CustomCreateObject : FsmStateAction
    {
        private readonly GameObject parent;
        protected GameObject prefab;

        protected GameObject newObject;

        public CustomCreateObject(GameObject parent, GameObject prefab)
        {
            this.parent = parent;
            this.prefab = prefab;
        }

        public override void OnEnter()
        {
            newObject = GameObject.Instantiate(prefab);
            newObject.transform.position = parent.transform.position;
            newObject.name = newObject.name.Replace("(Clone)(Clone)", "(Clone)");
            newObject.SetActive(true);
            newObject.AddComponent<ItemBehaviour>();
        }
    }
}
