// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Мастерская Флеетари (REPAIRSHOP): переключает безопасные части, отвязывает ржавые машины, чинит
// коллайдер кассы и Fury, хук «Флеетари перегоняет Сатсуму» (чтобы она не телепортировалась назад),
// и даёт доступ к позиции кассы (нужно фиксу колеса у ItemBehaviour).

using System.Collections.Generic;
using UnityEngine;

using MOPR.FSM;
using MOPR.Vehicles.Cases;
using MOPR.Rules;

namespace MOPR.Places
{
    internal class RepairShop : Place
    {
        private readonly string[] blackList =
        {
            "REPAIRSHOP", "JunkCar", "sats_burn_masse", "TireOld(Clone)", "Order", "JunkYardJob",
            "BoozeJob", "Spawn", "SatsumaSpawns", "SeatPivot", "DistanceTarget", "SpawnToRepair",
            "PartsDistanceTarget", "JunkCarSpawns", "Parts", "wheel_regul", "rpm gauge(Clone)",
            "Hook", "Jobs", "GearRatios", "Fix", "fix", "Job", "Polish", "Wheel", "Fill", "Rollcage",
            "Adjust", "GearLinkage", "Paintjob", "Windshield", "ToeAdjust", "Brakes", "Lifter", "Audio", "roll",
            "TireCatcher", "Ropes", "Note", "note", "inspection_desk 1", "Office", "Furniture",
            "Building", "office_floor", "coll", "wall_base", "JunkYardJob", "PayMoney", "100mk", "GaugeMeshTach",
            "gauge_glass_fbx", "Pivot", "needle", "Bolt", "bolt", "grille", "wheel_steel5", "gear_stick",
            "Platform", "Coll", "Buy", "Product", "Key(Clone)", "LOD", "repair_shop_walls", "repair_shop_roof_metal"
        };

        private Transform cashRegister;

        public RepairShop() : base("REPAIRSHOP", 250)
        {
            // Отвязываем ржавые машины.
            for (int i = 1; transform.Find("JunkCar" + i) != null; i++)
            {
                Transform junk = transform.Find("JunkCar" + i);
                if (junk != null)
                    junk.parent = null;
            }

            GameObjectBlackList.AddRange(blackList);
            DisableableChilds = GetDisableableChilds();

            // Детали Сатсумы на полках.
            List<Transform> productsMesh = DisableableChilds.FindAll(t => t.name == "mesh" && t.parent.name.Contains("Product"));
            foreach (Transform product in productsMesh)
                DisableableChilds.Remove(product);

            // Правим сферический коллайдер кассы.
            SphereCollider registerCollider = transform.Find("LOD/Store/ShopCashRegister/Register").gameObject.GetComponent<SphereCollider>();
            registerCollider.radius = 70;
            Vector3 newBounds = registerCollider.center;
            newBounds.x = 5;
            registerCollider.center = newBounds;

            // Коллайдер для Fury.
            if (!RulesManager.Instance.SpecialRules.SkipFuryColliderFix)
            {
                BoxCollider collBox0 = transform.Find("LOD/Vehicle/FURY").gameObject.AddComponent<BoxCollider>();
                BoxCollider collBox1 = transform.Find("LOD/Vehicle/FURY").gameObject.AddComponent<BoxCollider>();
                collBox0.center = new Vector3(0, 0, -.2f);
                collBox0.size = new Vector3(2.2f, 1.3f, 5);
                collBox1.center = new Vector3(0, 0, -.25f);
                collBox1.size = new Vector3(1.4f, 2.3f, 1.1f);
            }

            // Флеетари перегоняет Сатсуму — не телепортируем её обратно.
            try
            {
                transform.Find("Order").gameObject.FsmInject("Move Satsuma", Satsuma.Instance.FleetariIsMovingCar);
            }
            catch
            {
                throw new System.Exception("Couldn't FsmHook RepairShop/Order object.");
            }

            transform.Find("LOD/Door/Handle").gameObject.GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;

            cashRegister = GameObject.Find("REPAIRSHOP").transform.Find("LOD/Store/ShopCashRegister");

            Compress();
        }

        public Transform GetCashRegister() => cashRegister;
    }
}
