// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Обнаружение и регистрация подбираемых предметов сцены. Навешивает ItemBehaviour на всё, что
// проходит по белому списку (ItemNameList) + суффиксу экземпляра, на детей контейнера ITEMS, на
// колёса, конверты и стартовый ящик пива. Совместимость с CD Player Enhanced определяется через
// MSCLoader.IsModPresent.

using System;
using System.Linq;
using UnityEngine;
using HutongGames.PlayMaker;
using MSCLoader;

using MOPR.Common;
using MOPR.Helpers;
using MOPR.Items.Helpers;

namespace MOPR.Items
{
    internal static class ItemRegistrar
    {
        /// <summary>Находит и хукает все стартовые предметы мира.</summary>
        public static void RegisterAll()
        {
            RegisterWhitelisted();
            RegisterCds();
            RegisterItemsContainer();
            RegisterWheels();
            CreateWheelRepairTrigger();
            RegisterEnvelopes();
            UnparentCds();
            RegisterInitialBeerCase();
        }

        /// <summary>Предметы из белого списка с суффиксом экземпляра (в т.ч. пакеты покупок).</summary>
        private static void RegisterWhitelisted()
        {
            foreach (GameObject gm in UnityEngine.Object.FindObjectsOfType<GameObject>()
                .Where(g => g.name.ContainsAny(ItemNameList.Names)
                            && g.name.ContainsAny("(itemx)", "(Clone)")
                            && g.GetComponent<ItemBehaviour>() == null))
            {
                gm.AddComponent<ItemBehaviour>();
            }
        }

        /// <summary>CD/кейсы: с модом CD Player Enhanced — свои объекты (itemy) + триггер стола, иначе ваниль.</summary>
        private static void RegisterCds()
        {
            if (ModLoader.IsModPresent("CDPlayer"))
            {
                foreach (GameObject cd in Resources.FindObjectsOfTypeAll<GameObject>()
                    .Where(g => g.name.ContainsAny("cd case(itemy)", "CD Rack(itemy)", "cd(itemy)") && g.activeSelf).ToArray())
                {
                    cd.AddComponent<ItemBehaviour>();
                }

                // Триггер на столе Тэймо — для покупаемых CD.
                GameObject itemCheck = new GameObject("MOPR_ItemAreaCheck");
                itemCheck.transform.localPosition = new Vector3(-1551.303f, 4.883998f, 1182.904f);
                itemCheck.transform.localEulerAngles = new Vector3(0f, 58.00043f, 0f);
                itemCheck.transform.localScale = Vector3.one;
                itemCheck.AddComponent<ShopModItemSpawnCheck>();
            }
            else
            {
                foreach (GameObject cd in Resources.FindObjectsOfTypeAll<GameObject>()
                    .Where(g => g.name.ContainsAny("cd case(item", "cd(item")).ToArray())
                {
                    cd.AddComponent<ItemBehaviour>();
                }
            }
        }

        /// <summary>Дети контейнера ITEMS (кроме CDs) + гасим рестарт его FSM.</summary>
        private static void RegisterItemsContainer()
        {
            GameObject itemsObject = GameObject.Find("ITEMS");
            for (int i = 0; i < itemsObject.transform.childCount; i++)
            {
                GameObject item = itemsObject.transform.GetChild(i).gameObject;
                if (item.name == "CDs")
                    continue;

                try
                {
                    if (item.GetComponent<ItemBehaviour>() == null)
                        item.AddComponent<ItemBehaviour>();
                }
                catch (Exception ex)
                {
                    ExceptionManager.New(ex, false, "ITEM_LIST_AT_ITEMS_LOAD_ERROR");
                }
            }

            // Отключаем перезапуск FSM контейнера при активации.
            itemsObject.GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;
        }

        /// <summary>Колёса без суффикса (wheel_regula/wheel_offset).</summary>
        private static void RegisterWheels()
        {
            foreach (GameObject wheel in UnityEngine.Object.FindObjectsOfType<GameObject>()
                .Where(g => g.name.EqualsAny("wheel_regula", "wheel_offset") && g.activeSelf))
            {
                wheel.AddComponent<ItemBehaviour>();
            }
        }

        /// <summary>Триггер шиномонтажа у Флеетари.</summary>
        private static void CreateWheelRepairTrigger()
        {
            GameObject wheelTrigger = new GameObject("MOPR_WheelTrigger");
            wheelTrigger.transform.localPosition = new Vector3(1555.49f, 4.8f, 737);
            wheelTrigger.transform.localEulerAngles = new Vector3(1.16f, 335, 1.16f);
            wheelTrigger.AddComponent<WheelRepairJobTrigger>();
        }

        /// <summary>Конверты и лотерейные билеты.</summary>
        private static void RegisterEnvelopes()
        {
            foreach (GameObject g in Resources.FindObjectsOfTypeAll<GameObject>()
                .Where(g => g.name.EqualsAny("envelope(xxxxx)", "lottery ticket(xxxxx)")))
            {
                g.AddComponent<ItemBehaviour>();
            }
        }

        /// <summary>Выносим детей объекта CDs из-под родителя (как в оригинале).</summary>
        private static void UnparentCds()
        {
            Transform cds = GameObject.Find("ITEMS").transform.Find("CDs");
            if (cds)
            {
                for (int i = 0; i < cds.childCount; i++)
                    cds.GetChild(i).parent = null;
            }
        }

        /// <summary>Стартовый ящик пива.</summary>
        private static void RegisterInitialBeerCase()
        {
            GameObject beerCaseInitial = Resources.FindObjectsOfTypeAll<GameObject>()
                .FirstOrDefault(g => g.name == "beer case(itemx)" && g.GetComponent<ItemBehaviour>() == null);
            beerCaseInitial?.AddComponent<ItemBehaviour>();
        }

        /// <summary>
        /// Дешёвый повторный проход по контейнеру ITEMS: навешивает ItemBehaviour на новичков
        /// (покупки/спавны), которые ещё не под управлением. Используется сканером ItemScanner.
        /// </summary>
        public static int ScanItemsContainer()
        {
            GameObject itemsObject = GameObject.Find("ITEMS");
            if (itemsObject == null)
                return 0;

            int added = 0;
            Transform root = itemsObject.transform;
            for (int i = 0; i < root.childCount; i++)
            {
                try
                {
                    GameObject item = root.GetChild(i).gameObject;
                    if (item.name == "CDs" || item.GetComponent<ItemBehaviour>() != null)
                        continue;

                    item.AddComponent<ItemBehaviour>();
                    added++;
                }
                catch (Exception ex)
                {
                    ExceptionManager.New(ex, false, "ITEM_RESCAN_ERROR");
                }
            }

            return added;
        }
    }
}
