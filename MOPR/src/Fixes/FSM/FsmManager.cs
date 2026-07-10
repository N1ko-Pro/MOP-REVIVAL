// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Единая точка доступа к игровому состоянию через PlayMaker: ленивое кэширование ссылок на FSM-
// переменные MSC (ключи Хаёсико, установлен ли капот/бампер, где сидит игрок, дистанция прорисовки,
// час суток и т.д.). Кэш обнуляется при возврате в меню (ResetAll), чтобы новая игра переразрешила
// ссылки на своей сцене и не держала указатели на уничтоженные объекты прошлой сессии.

using HutongGames.PlayMaker;
using System.Reflection;
using UnityEngine;

using MOPR.Common.Enumerations;
using MOPR.Managers;

namespace MOPR.FSM
{
    internal static class FsmManager
    {
        /// <summary>
        /// Сбрасывает все закэшированные ссылки на FSM-переменные, чтобы следующая загрузка
        /// переразрешила их заново. В оригинале метод был нерабочим (GetFields() без флагов не
        /// видел приватные статик-поля, а target передавался неверно) — здесь исправлено.
        /// </summary>
        public static void ResetAll()
        {
            FieldInfo[] fields = typeof(FsmManager).GetFields(BindingFlags.NonPublic | BindingFlags.Static);
            foreach (FieldInfo field in fields)
            {
                // Обнуляем только ссылочные кэши FSM-переменных/объектов.
                if (!field.FieldType.IsValueType)
                    field.SetValue(null, null);
            }
        }

        private static FsmInt uncleStage;
        /// <summary>Есть ли у игрока ключи от Хаёсико (UncleStage == 5).</summary>
        public static bool PlayerHasHayosikoKey()
        {
            if (uncleStage == null)
                uncleStage = GameObject.Find("UNCLE").GetComponent<PlayMakerFSM>().FsmVariables.GetFsmInt("UncleStage");

            return uncleStage.Value == 5;
        }

        private static FsmBool gtGrilleInstalled;
        /// <summary>Установлена ли GT-решётка.</summary>
        public static bool IsGTGrilleInstalled()
        {
            if (gtGrilleInstalled == null)
                gtGrilleInstalled = GameObject.Find("Database").transform.Find("DatabaseOrders/GrilleGT").gameObject.GetComponent<PlayMakerFSM>().FsmVariables.GetFsmBool("Installed");

            return gtGrilleInstalled.Value;
        }

        private static FsmBool order;
        /// <summary>Заказана ли работа в ремонтной мастерской.</summary>
        public static bool IsRepairshopJobOrdered()
        {
            if (order == null)
                order = GameObject.Find("REPAIRSHOP/Order").GetComponent<PlayMakerFSM>().FsmVariables.GetFsmBool("_Order");

            return order.Value;
        }

        private static FsmString playerCurrentVehicle;
        /// <summary>Сидит ли игрок в Сатсуме (глобальная PlayerCurrentVehicle == "Satsuma").</summary>
        public static bool IsPlayerInSatsuma()
        {
            if (playerCurrentVehicle == null)
                playerCurrentVehicle = PlayMakerGlobals.Instance.Variables.FindFsmString("PlayerCurrentVehicle");

            return playerCurrentVehicle.Value == "Satsuma";
        }

        /// <summary>Сидит ли игрок в каком-либо транспорте.</summary>
        public static bool IsPlayerInCar()
        {
            if (playerCurrentVehicle == null)
                playerCurrentVehicle = PlayMakerGlobals.Instance.Variables.FindFsmString("PlayerCurrentVehicle");

            return playerCurrentVehicle.Value.Length > 0;
        }

        private static FsmInt farmJobStage;
        /// <summary>Доступен ли комбайн (фермерская работа дошла до 3-й стадии).</summary>
        public static bool IsCombineAvailable()
        {
            if (farmJobStage == null)
                farmJobStage = GameObject.Find("JOBS/Farm/Job").GetComponent<PlayMakerFSM>().FsmVariables.GetFsmInt("JobStage");

            return farmJobStage.Value >= 3;
        }

        private static FsmBool hoodBolted;
        public static bool IsStockHoodBolted()
        {
            if (hoodBolted == null)
                hoodBolted = GameObject.Find("Database/DatabaseBody/Hood").GetComponent<PlayMakerFSM>().FsmVariables.GetFsmBool("Bolted");

            return hoodBolted.Value;
        }

        private static FsmBool fiberHoodBolted;
        public static bool IsFiberHoodBolted()
        {
            if (fiberHoodBolted == null)
                fiberHoodBolted = GameObject.Find("Database/DatabaseOrders/Fiberglass Hood").GetComponent<PlayMakerFSM>().FsmVariables.GetFsmBool("Bolted");

            return fiberHoodBolted.Value;
        }

        private static GameObject triggerHood;
        public static void ForceHoodAssemble()
        {
            if (triggerHood == null)
                triggerHood = VehicleManager.Instance.GetVehicle(VehiclesTypes.Satsuma).transform.Find("Body/trigger_hood").gameObject;

            triggerHood.SetActive(true);
            triggerHood.GetComponent<PlayMakerFSM>().SendEvent("ASSEMBLE");
        }

        private static GameObject triggerBumperRear;
        public static void ForceRearBumperAssemble()
        {
            if (triggerBumperRear == null)
                triggerBumperRear = VehicleManager.Instance.GetVehicle(VehiclesTypes.Satsuma).transform.Find("Body/trigger_bumper_rear").gameObject;

            triggerBumperRear.SetActive(true);
            triggerBumperRear.GetComponent<PlayMakerFSM>().SendEvent("ASSEMBLE");
        }

        private static GameObject kekmetTrailerRemove;
        public static bool IsTrailerAttached()
        {
            if (kekmetTrailerRemove == null)
                kekmetTrailerRemove = VehicleManager.Instance.GetVehicle(VehiclesTypes.Kekmet).transform.Find("Trailer/Remove").gameObject;

            return kekmetTrailerRemove.activeSelf;
        }

        private static FsmFloat battery1;
        private static FsmFloat battery2;
        public static bool IsBatteryInstalled()
        {
            if (battery1 == null)
                battery1 = GameObject.Find("Database/PartsStatus/Battery").GetComponent<PlayMakerFSM>().FsmVariables.GetFsmFloat("Bolt1");

            if (battery2 == null)
                battery2 = GameObject.Find("Database/PartsStatus/Battery").GetComponent<PlayMakerFSM>().FsmVariables.GetFsmFloat("Bolt2");

            return battery1.Value > 0 || battery2.Value > 0;
        }

        private static FsmBool playerHelmet;
        public static bool PlayerHelmetOn()
        {
            if (playerHelmet == null)
                playerHelmet = PlayMakerGlobals.Instance.Variables.GetFsmBool("PlayerHelmet");

            return playerHelmet.Value;
        }

        private static FsmFloat drawDistance;
        public static float GetDrawDistance()
        {
            if (drawDistance == null)
                drawDistance = GameObject.Find("Systems/Options").GetPlayMaker("GFX").FsmVariables.GetFsmFloat("DrawDistance");

            return drawDistance.Value;
        }

        private static FsmBool suskiLarge;
        public static bool IsSuskiLargeCall()
        {
            if (suskiLarge == null)
                suskiLarge = GameObject.Find("Telephone/Logic/UseHandle").GetComponent<PlayMakerFSM>().FsmVariables.GetFsmBool("SuskiLarge");

            return suskiLarge.Value;
        }

        private static FsmBool playerInMenu;
        public static bool PlayerInMenu
        {
            get
            {
                if (playerInMenu == null)
                    playerInMenu = PlayMakerGlobals.Instance.Variables.GetFsmBool("PlayerInMenu");

                return playerInMenu.Value;
            }
            set
            {
                if (playerInMenu == null)
                    playerInMenu = PlayMakerGlobals.Instance.Variables.GetFsmBool("PlayerInMenu");

                playerInMenu.Value = value;
            }
        }

        private static FsmBool playerComputer;
        public static bool PlayerComputer
        {
            get
            {
                if (playerComputer == null)
                    playerComputer = PlayMakerGlobals.Instance.Variables.GetFsmBool("PlayerComputer");

                return playerComputer.Value;
            }
        }

        private static FsmBool shadowsHouse;
        public static bool ShadowsHouse
        {
            get
            {
                if (shadowsHouse == null)
                    shadowsHouse = GameObject.Find("Systems").transform.Find("Options").GetPlayMaker("GFX").FsmVariables.GetFsmBool("ShadowsHouse");

                return shadowsHouse.Value;
            }
        }

        private static FsmFloat hour;
        public static int Hour
        {
            get
            {
                if (hour == null)
                    hour = GameObject.Find("MAP/SUN/Pivot/SUN").GetPlayMaker("Clock").FsmVariables.GetFsmFloat("Hours");

                return (int)hour.Value;
            }
        }
    }
}
