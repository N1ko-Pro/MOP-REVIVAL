// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Отложенное навешивание ItemBehaviour: ждёт ~60 кадров после спавна предмета (чтобы игровые FSM
// успели проинициализировать его переменные), затем берёт предмет под управление и самоуничтожается.

using UnityEngine;
using System.Collections;

using MOPR.Items;

namespace MOPR.FSM.Actions
{
    internal class DelayedItemBehaviour : MonoBehaviour
    {
        private void Start()
        {
            StartCoroutine(DelayAdd());
        }

        private IEnumerator DelayAdd()
        {
            for (int i = 0; i < 60; i++)
                yield return null;

            gameObject.AddComponent<ItemBehaviour>();
            Destroy(this);
        }
    }
}
