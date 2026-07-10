// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Помощник кассы магазина: при выкупе заказа деталей (через хук на кассе) навешивает
// ItemBehaviour на приехавшие пакеты «amis-auto ky package» и внедряет в их FSM обработчик
// распаковки (CustomPackageHandler). Ставится из StoreOrderHook.

using System.Collections;
using System.Linq;
using UnityEngine;

using MOPR.FSM;
using MOPR.FSM.Actions;

namespace MOPR.Items.Helpers
{
    internal class CashRegisterBehaviour : MonoBehaviour
    {
        private IEnumerator packagesRoutine;

        /// <summary>Точка входа для хука кассы: перезапускает корутину обработки пакетов.</summary>
        public void Packages()
        {
            // Если корутина уже выполнялась — прерываем её и запускаем заново.
            if (packagesRoutine != null)
                StopCoroutine(packagesRoutine);

            packagesRoutine = PackagesCoroutine();
            StartCoroutine(packagesRoutine);
        }

        private IEnumerator PackagesCoroutine()
        {
            // Ждём, пока пакеты успеют появиться на сцене.
            yield return new WaitForSeconds(2);

            GameObject[] packages = GameObject.FindGameObjectsWithTag("ITEM")
                .Where(g => g.name == "amis-auto ky package(xxxxx)" && g.activeSelf)
                .ToArray();

            for (int i = 0; i < packages.Length; ++i)
            {
                GameObject package = packages[i];
                package.AddComponent<ItemBehaviour>();
                package.GetPlayMaker("Use").GetState("State 1").AddAction(new CustomPackageHandler(package));
            }
        }
    }
}
