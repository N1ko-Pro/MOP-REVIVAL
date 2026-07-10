// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Навешивает SatsumaTrigger на все триггеры сборки кузова и блока и, когда триггер выключается,
// через пару секунд проверяет: если точка сборки осталась пустой — включает триггер обратно (иначе
// деталь нельзя будет приделать).

using System.Collections;
using System.Linq;
using UnityEngine;

using MOPR.Managers;
using MOPR.Common.Enumerations;

namespace MOPR.Vehicles.Managers.SatsumaManagers
{
    internal class SatsumaTriggerFixer : MonoBehaviour
    {
        private static SatsumaTriggerFixer instance;
        public static SatsumaTriggerFixer Instance => instance;

        private void Awake()
        {
            instance = this;

            Transform body = VehicleManager.Instance.GetVehicle(VehiclesTypes.Satsuma).transform.Find("Body");
            Transform block = GameObject.Find("block(Clone)").transform;

            AddTriggerToChildren(body);
            AddTriggerToChildren(block);
        }

        private void AddTriggerToChildren(Transform parent)
        {
            foreach (Transform t in parent.GetComponentsInChildren<Transform>()
                .Where(g => g.gameObject.name.ToLower().StartsWith("trigger_")))
            {
                if (t.GetComponent<PlayMakerFSM>() != null)
                    t.gameObject.AddComponent<SatsumaTrigger>();
            }
        }

        internal void Check(SatsumaTrigger trigger)
        {
            if (trigger == null)
                return;

            try
            {
                StartCoroutine(CheckTriggerChild(trigger));
            }
            catch
            {
            }
        }

        private IEnumerator CheckTriggerChild(SatsumaTrigger trigger)
        {
            yield return new WaitForSeconds(2);

            if (trigger.Pivot.childCount == 0)
                trigger.gameObject.SetActive(true);
        }
    }
}
