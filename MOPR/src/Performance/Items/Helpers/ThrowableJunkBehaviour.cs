// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Помощник брошенной тары (бутылки/стаканы/пачки): навешивается на префабы «Fly»-объектов. При
// появлении либо уничтожает пустую тару (если включена настройка «уничтожать пустые бутылки»),
// либо навешивает на неё ItemBehaviour, чтобы она попала под оптимизацию.

using UnityEngine;
using MOPR.Common;

namespace MOPR.Items.Helpers
{
    internal class ThrowableJunkBehaviour : MonoBehaviour
    {
        private void Start()
        {
            // Если включена настройка — просто убираем пустую тару со сцены.
            if (MoprSettings.DestroyEmptyBottlesOn)
            {
                Destroy(gameObject);
                return;
            }

            // Иначе ставим оптимизацию и отключаемся (работа помощника разовая).
            gameObject.AddComponent<ItemBehaviour>();
            enabled = false;
        }
    }
}
