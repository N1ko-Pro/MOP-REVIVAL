// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Фикс триггера глушителя: когда с пивота глушителя сняты все детали, через пару секунд включает
// обратно «Triggers Mufflers», иначе глушитель нельзя будет установить снова.

using System.Collections;
using UnityEngine;

namespace MOPR.Vehicles.Managers.SatsumaManagers
{
    internal class SatsumaExhaustMufflerFix : MonoBehaviour
    {
        private GameObject triggerMufflers;
        private IEnumerator currentWaitTrigger;

        private void Awake()
        {
            triggerMufflers = transform.root.Find("MiscParts/Triggers Mufflers").gameObject;

            if (transform.childCount > 0)
                StartEnumerator();
        }

        private void OnTransformChildrenChanged()
        {
            StartEnumerator();
        }

        private void StartEnumerator()
        {
            if (currentWaitTrigger == null)
            {
                currentWaitTrigger = WaitTriggerCheck();
                StartCoroutine(currentWaitTrigger);
            }
        }

        private IEnumerator WaitTriggerCheck()
        {
            while (transform.childCount > 0)
                yield return new WaitForSeconds(2);

            yield return new WaitForSeconds(2);
            if (transform.childCount == 0)
                triggerMufflers.SetActive(true);

            currentWaitTrigger = null;
        }
    }
}
