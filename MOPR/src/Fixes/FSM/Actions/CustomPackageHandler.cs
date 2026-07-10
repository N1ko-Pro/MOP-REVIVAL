// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Экшен обработки посылки деталей: при распаковке включает все детали из подобъекта Parts, берёт
// каждую под управление MOPR (ItemBehaviour) и снова гасит — чтобы они появились уже управляемыми.

using System.Linq;
using UnityEngine;

using MOPR.Items;

namespace MOPR.FSM.Actions
{
    internal class CustomPackageHandler : HutongGames.PlayMaker.FsmStateAction
    {
        private readonly Transform[] items;

        public CustomPackageHandler(GameObject gm)
        {
            Transform parts = gm.transform.Find("Parts");
            items = parts.GetComponentsInChildren<Transform>(true).Where(t => t.parent == parts).ToArray();
            MSCLoader.ModConsole.Log(string.Join(", ", items.Select(g => g.name).ToArray()));
        }

        public override void OnEnter()
        {
            for (int j = 0; j < items.Length; j++)
            {
                items[j].gameObject.SetActive(true);
                items[j].gameObject.AddComponent<ItemBehaviour>();
                items[j].gameObject.SetActive(false);
            }

            Finish();
        }
    }
}
