// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Игровой фикс radiator hose3: у этого шланга особый спавн через игровую БД. Держим ссылку на
// «настоящий» шланг и подставляем её в переменную SpawnThis игрового FSM (при загрузке и перед
// сохранением), иначе после отсоединения шланг спавнится неправильно.

using System.Linq;
using UnityEngine;
using HutongGames.PlayMaker;

using MOPR.Common;
using MOPR.FSM;
using MOPR.Helpers;
using MOPR.Managers;

namespace MOPR.Items.Fixes
{
    internal static class RadiatorHose3Fix
    {
        private static PlayMakerFSM database;
        private static GameObject realHose;

        /// <summary>Текущий «настоящий» radiator hose3 (используется фиксом Сатсумы и спавном).</summary>
        public static GameObject RealHose => realHose;

        public static void SetRealHose(GameObject hose) => realHose = hose;

        /// <summary>Разовая настройка при инициализации предметов.</summary>
        public static void Create()
        {
            database = GameObject.Find("Database/DatabaseMechanics/RadiatorHose3").GetPlayMaker("Data");
            GameObject attachedHose = Resources.FindObjectsOfTypeAll<GameObject>().First(g => g.name == "radiator hose3(xxxxx)");
            realHose = Resources.FindObjectsOfTypeAll<GameObject>().First(g => g.name == "radiator hose3(Clone)");

            // Дубликат для спавна после отсоединения.
            GameObject dummy = Object.Instantiate(realHose);
            Object.Destroy(dummy.GetComponent<ItemBehaviour>());
            dummy.SetActive(false);
            dummy.name = dummy.name.Replace("(Clone)(Clone)", "(Clone)");

            Transform t = SaveManager.GetRadiatorHose3Transform();
            if (!attachedHose.activeSelf)
            {
                realHose.transform.position = t.position;
                realHose.transform.rotation = t.rotation;
                realHose.SetActive(true);
            }

            database.FsmVariables.GameObjectVariables.First(g => g.Name == "SpawnThis").Value = realHose;
        }

        /// <summary>Перед сохранением фиксируем актуальный шланг в игровой БД.</summary>
        public static void OnSave()
        {
            try
            {
                if (database)
                    database.FsmVariables.GameObjectVariables.First(g => g.Name == "SpawnThis").Value = realHose;
            }
            catch (System.Exception ex)
            {
                ExceptionManager.New(ex, true, "RADIATOR_HOSE_3_DB_ERROR");
            }
        }
    }
}
