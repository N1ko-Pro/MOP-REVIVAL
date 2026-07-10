// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Хуки префабов дров и брёвен: у них нет FSM (это «сырые» префабы), поэтому оптимизацию на них
// вешаем отдельно, находя их среди всех загруженных объектов.

using System.Linq;
using UnityEngine;
using HutongGames.PlayMaker;

using MOPR.Common;
using MOPR.Helpers;

namespace MOPR.Items.Hooks
{
    internal static class ItemPrefabHooks
    {
        public static void HookFirewood()
        {
            // Ищем префаб дров без FSM и навешиваем на него оптимизацию.
            GameObject firewoodPrefab = Resources.FindObjectsOfTypeAll<GameObject>()
                .FirstOrDefault(f => f.name.EqualsAny("firewood") && f.GetComponent<PlayMakerFSM>() == null)?.gameObject;
            firewoodPrefab?.AddComponent<ItemBehaviour>();
        }

        public static void HookLog()
        {
            // Префаб бревна без FSM — вместе с вложенным клоном.
            GameObject logPrefab = Resources.FindObjectsOfTypeAll<GameObject>()
                .FirstOrDefault(f => f.name.EqualsAny("log") && f.GetComponent<PlayMakerFSM>() == null)?.gameObject;
            if (logPrefab != null)
            {
                logPrefab.AddComponent<ItemBehaviour>();
                logPrefab.transform.Find("log(Clone)").gameObject.AddComponent<ItemBehaviour>();
            }

            // Все уже присутствующие на сцене клоны брёвен тоже оптимизируем.
            foreach (GameObject f in Resources.FindObjectsOfTypeAll<GameObject>().Where(g => g.name == "log(Clone)"))
                f.AddComponent<ItemBehaviour>();
        }
    }
}
