// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Помощник совместимости с модом Item Shop / CD Player Enhanced: добавляет триггер-коллайдер на
// стол магазина Тэймо и навешивает ItemBehaviour на купленные CD/кейсы, когда они там появляются.

using UnityEngine;
using MOPR.Common;

namespace MOPR.Items.Helpers
{
    internal class ShopModItemSpawnCheck : MonoBehaviour
    {
        private readonly string[] items = { "cd case(itemy)", "CD Rack(itemy)", "cd(itemy)" };

        public ShopModItemSpawnCheck()
        {
            gameObject.layer = 2; // Ignore Raycast — чтобы «рука» игрока не цеплялась.

            BoxCollider col = gameObject.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(2, 2, 2);
        }

        private void OnTriggerEnter(Collider other)
        {
            GameObject enteredObject = other.gameObject;

            if (enteredObject.name.ContainsAny(items) && enteredObject.GetComponent<ItemBehaviour>() == null)
            {
                enteredObject.AddComponent<ItemBehaviour>();

                // Внутри кейса CD — тоже хукаем вложенные диски.
                if (enteredObject.name == "cd case(itemy)")
                {
                    foreach (Transform child in enteredObject.GetComponentsInChildren<Transform>(true))
                    {
                        if (child.gameObject.name == "cd(itemy)")
                            child.gameObject.AddComponent<ItemBehaviour>();
                    }
                }
            }
        }
    }
}
