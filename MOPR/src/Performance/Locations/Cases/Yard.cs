// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Двор игрока (YARD): дом, гараж, сауна, холодильник, компьютер и т.д. Помимо переключения по
// дистанции чинит ряд FSM (двери, гаражные ворота, сауна) и умеет отвечать, лежит ли предмет в
// работающем холодильнике (для расчёта порчи) и держит симуляцию сауны, если печь разогрета.

using System;
using HutongGames.PlayMaker;
using System.Linq;
using UnityEngine;

using MOPR.FSM;
using MOPR.Common;

namespace MOPR.Places
{
    internal class Yard : Place
    {
        public static Yard Instance;

        private readonly string[] blackList =
        {
            "YARD", "Spawn", "VenttiPigHouse", "Capsule", "Target", "Pivot", "skeleton", "bodymesh",
            "COPS", "Trigger", "Cop", "Collider", "thig", "pelvis", "knee", "ankle", "spine",
            "PlayerMailBox", "mailbox", "envelope", "Envelope", "Letter", "Ad", "UNCLE",
            "Uncle", "GifuKeys", "key", "anim", "Pos", "GraveYardSpawn", "HOUSEFIRE",
            "Livingroom", "Fire", "0", "1", "2", "Particle", "smoke", "Flame", "livingroom",
            "Kitchen", "kitchen", "Bathroom", "bathroom", "Bedroom", "bedroom", "Sauna",
            "sauna", "COMPUTER", "SYSTEM", "Computer", "TriggerPlayMode", "Meshes", "386",
            "monitor", "mouse", "cord", "Button", "Sound", "led", "DiskDrive", "Sled",
            "disk", "Power", "power", "Floppy", "LCD", "Screen", "Memory", "Mesh",
            "Dynamics", "Light", "Electric", "KWH", "kwh", "Clock", "Program", "program",
            "Buzz", "switch", "Switch", "Fines", "Colliders", "coll", "Shower", "Water", "Tap",
            "Valve", "Telephone", "Cord", "Socket", "Logic", "Phone", "table", "MAP", "Darts", "Booze",
            "Shit", "Wood", "Grandma", "SAVEGAME", "Shelf", "shelf", "Garage", "Building", "LIVINGROOM",
            "BEDROOM1", "Table", "boybed", "KITCHEN", "Fridge", "bench", "wood", "Pantry", "Glass",
            "closet", "Numbers", "Ring", "log", "washingmachine", "lauteet", "MIDDLEROOM", "BeerCamp",
            "Chair", "TablePlastic", "LOD_middleroom", "hotwaterkeeper", "house_roof",
            "WC", "Hallway", "Entry", "ContactPivot", "DoorRight", "DoorLeft", "GarageDoors", "BatteryCharger",
            "Clamps", "ChargerPivot", "Clamp", "BatteryPivot", "battery_charger", "Wire", "cable", "TriggerCharger",
            "tvtable", "VHS_Screen", "tv_table(Clone)", "scart_con", "Haybale", "Combine", "UncleWalking", "LOD", "house_wall_brick",
            "houseuncle_roof", "houseuncle_walls", "fuse holder(Clone)", "SAUNA"
        };

        private const float ChillDistance = .45f;
        private readonly Transform chillPoint;
        private readonly FsmBool fridgeRunning;

        private GameObject sauna;
        private GameObject saumaSimulation;
        private readonly FsmFloat saunaStoveHeat;
        private const float StoveOnSimulationPoint = 35; // порог разогрева печи, выше которого симулируем перегрев

        public Yard() : base("YARD")
        {
            Instance = this;

            RemoveComputerFromHome();
            FixGarageDoors();

            try
            {
                foreach (Transform door in transform.GetComponentsInChildren<Transform>()
                    .Where(t => t.root == transform && t.gameObject.name.Contains("Door") && t.Find("Pivot") != null).ToArray())
                {
                    if (door.Find("Pivot/Handle") != null)
                        door.Find("Pivot/Handle").gameObject.GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "FRIDGE_DOORHANDLE_ERROR");
            }

            GameObjectBlackList.AddRange(blackList);
            DisableableChilds = GetDisableableChilds();

            // Меш холодильника не выключаем.
            Transform fridgeMesh = DisableableChilds.Find(w => w.name == "mesh" && w.transform.parent.name == "Fridge");
            DisableableChilds.Remove(fridgeMesh);

            transform.Find("UNCLE").GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;

            Compress();

            FixSaunaSimulation();

            chillPoint = transform.Find("Building/KITCHEN/Fridge/FridgePoint/ChillArea");
            try
            {
                GameObject fridgePoint = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(g => g.name == "FridgePoint");
                if (fridgePoint)
                    fridgeRunning = fridgePoint.GetPlayMaker("Chilling")?.FsmVariables.GetFsmBool("Kitchen");
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "FRIDGE_RUNNING_FAILURE");
            }

            try
            {
                sauna = transform.Find("Building/SAUNA")?.gameObject;
                if (sauna != null)
                {
                    saumaSimulation = sauna.transform.Find("Sauna/Simulation")?.gameObject;
                    if (saumaSimulation != null)
                        saunaStoveHeat = saumaSimulation.GetPlayMaker("Time").FsmVariables.GetFsmFloat("StoveHeat");
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "SAUNA_STOVE_SIMULATION_FAILURE");
            }

            LightSources = GetLightSources();
        }

        private static void FixGarageDoors()
        {
            GameObject garageDoors = GameObject.Find("GarageDoors");
            if (garageDoors)
            {
                garageDoors.transform.Find("DoorLeft/Coll").GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;
                garageDoors.transform.Find("DoorRight/Coll").GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;
            }
        }

        private static void RemoveComputerFromHome()
        {
            GameObject computer = GameObject.Find("COMPUTER");
            if (computer != null)
                computer.transform.parent = null;
        }

        /// <summary>Лежит ли предмет в работающем кухонном холодильнике (влияет на скорость порчи).</summary>
        public bool IsItemInFridge(GameObject item)
        {
            if (fridgeRunning == null || !fridgeRunning.Value)
                return false;

            return Vector3.Distance(item.transform.position, chillPoint.position) < ChillDistance;
        }

        private void FixSaunaSimulation()
        {
            if (transform.Find("Building/SAUNA") != null)
            {
                transform.Find("Building/SAUNA/Sauna/Kiuas/ButtonPower").GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;
                transform.Find("Building/SAUNA/Sauna/Kiuas/ButtonTime").GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;
                transform.Find("Building/SAUNA/Sauna/Kiuas/StoveTrigger").GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;
                transform.Find("Building/SAUNA/Sauna/Simulation").GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;
            }
        }

        public override void ToggleActive(bool enabled)
        {
            if (isActive == enabled)
                return;
            isActive = enabled;

            for (int i = 0; i < DisableableChilds.Count; i++)
            {
                if (DisableableChilds[i] == null)
                    continue;
                DisableableChilds[i].gameObject.SetActive(enabled);
            }

            for (int i = 0; i < PlayMakers.Count; i++)
            {
                if (PlayMakers[i] == null)
                    continue;
                PlayMakers[i].enabled = enabled;
            }

            if (LightSources.Count > 0 && FsmManager.ShadowsHouse)
            {
                for (int i = 0; i < LightSources.Count; i++)
                    LightSources[i].shadows = enabled ? LightShadows.Hard : LightShadows.None;
            }

            // Печь сауны разогрета — держим симуляцию включённой независимо от дистанции.
            if (sauna != null)
            {
                if (saunaStoveHeat.Value > StoveOnSimulationPoint)
                {
                    sauna.SetActive(true);
                    saumaSimulation.SetActive(true);
                }
                else
                {
                    sauna.SetActive(enabled);
                    saumaSimulation.SetActive(enabled);
                }
            }
        }
    }
}
