// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Экшен создания шланга радиатора: как CustomCreateObject, но заранее готовит чистый префаб (без
// ItemBehaviour, неактивный) и после инстанцирования регистрирует созданный шланг как текущий
// radiator hose3 в ItemsManager.

using UnityEngine;

using MOPR.Items;
using MOPR.Managers;

namespace MOPR.FSM.Actions
{
    internal class CustomCreateHose : CustomCreateObject
    {
        public CustomCreateHose(GameObject parent, GameObject prefab) : base(parent, prefab)
        {
            GameObject newPrefab = GameObject.Instantiate(prefab);
            newPrefab.name = newPrefab.name.Replace("(Clone)(Clone)", "(Clone)");
            this.prefab = newPrefab;
            Object.Destroy(this.prefab.GetComponent<ItemBehaviour>());
            this.prefab.SetActive(false);
        }

        public override void OnEnter()
        {
            base.OnEnter();
            ItemsManager.Instance.SetCurrentRadiatorHose(newObject);
        }
    }
}
