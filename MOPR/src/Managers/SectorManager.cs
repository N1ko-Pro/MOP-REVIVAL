// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Менеджер секторов помещений: создаёт триггер-объём на игроке и расставляет по карте секторы
// (гараж/дом/магазин/мастерская/дача/камера и т.д.) с их дистанциями прорисовки и whitelist'ами.
// Отслеживает активные секторы — этим пользуются DDD, оптимизация предметов/мира и IndoorCuller.
// Дополнительные секторы могут приходить из файлов правил.

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using MOPR.Common;
using MOPR.Common.Enumerations;
using MOPR.Rules;
using MOPR.Rules.Types;
using MOPR.Common.Interfaces;

namespace MOPR.Managers
{
    internal class SectorManager : MonoBehaviour, IManager<Sector>
    {
        private static SectorManager instance;
        public static SectorManager Instance => instance;

        private List<Sector> sectors;
        private List<Sector> activeSectors;

        public GameObject PlayerCheck { get; private set; }

        public int Count => sectors.Count;
        public List<Sector> GetAll => sectors;

        public SectorManager()
        {
            instance = this;

            ModConsole.Log("[MOPR] Loading sectors...");

            // Триггер-«щуп» на игроке (слой 20 игнорируется «рукой» игрока).
            PlayerCheck = new GameObject("MOPR_PlayerCheck");
            PlayerCheck.layer = 20;
            PlayerCheck.transform.parent = GameObject.Find("PLAYER").transform;
            PlayerCheck.transform.localPosition = Vector3.zero;
            BoxCollider playerCollider = PlayerCheck.AddComponent<BoxCollider>();
            playerCollider.isTrigger = true;
            playerCollider.size = new Vector3(.1f, 1, .1f);
            Rigidbody rb = PlayerCheck.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = true;

            activeSectors = new List<Sector>();
            sectors = new List<Sector>();

            // Гараж.
            if (MoprSettings.Mode == PerformanceMode.Performance)
                CreateNewSector(new Vector3(-16.77627f, -0.5062422f, 1.559867f), new Vector3(5, 5, 9), 1500, "PierHome");
            else
                CreateNewSector(new Vector3(-16.77627f, -0.5062422f, 1.559867f), new Vector3(5, 5, 9), 1500, "PierHome", "Tile", "LakeNice", "TREES_MEDIUM3", "BUSHES3", "BUSHES6", "RadioMast");

            // Тэймо.
            CreateNewSector(new Vector3(-1547.4f, 4, 1183.35f), new Vector3(9.6f, 5, 5.5f), new Vector3(0, 328, 0), 500,
                "StreetLights", "HUMANS", "TRAFFIC", "NPC_CARS", "PERAJARVI", "TrafficSigns", "StreetLights", "ELEC_POLES", "TREES_SMALL1", "BusStop", "BUSHES3");
            CreateNewSector(new Vector3(-1551.7f, 4, 1185.8f), new Vector3(4.7f, 5, 3.3f), new Vector3(0, 328, 0), 50,
                "StreetLights", "HUMANS", "TRAFFIC", "NPC_CARS", "PERAJARVI", "TrafficSigns", "StreetLights", "ELEC_POLES", "TREES_SMALL1", "BusStop");

            // Мастерская.
            CreateNewSector(new Vector3(1562.49f, 4.8f, 733.8835f), new Vector3(15, 5, 20), new Vector3(0, 335, 0), 500,
                "TRAFFIC", "ELEC_POLES", "Buildings", "HUMANS", "TrafficSigns", "StreetLights", "TREES_MEDIUM3", "BUSHES6");

            // Машинный зал двора.
            CreateNewSector(new Vector3(54.7f, -0.5062422f, -73.9f), new Vector3(6, 5, 5.2f), 1500, "YARD", "MachineHall", "BUSHES3", "BUSHES6", "TREES_SMALL1");

            // Дом.
            CreateNewSector(new Vector3(-7.2f, -0.5062422f, 9.9f), new Vector3(11, 5, 9.5f), 1500, "PierHome", "TREES_SMALL1", "BUSHES7", "Building", "RadioMast");
            CreateNewSector(new Vector3(-12.5f, -0.5062422f, 1.2f), new Vector3(3, 5, 8f), 300, "PierHome", "TREES_SMALL1", "Building");
            CreateNewSector(new Vector3(-13.4f, -0.5062422f, 6.4f), new Vector3(1.4f, 5, 1.7f), 100, "PierHome");

            // Тюрьма.
            CreateNewSector(new Vector3(-655, 5, -1156), new Vector3(5, 5, 9f), 50);

            // Дача.
            CreateNewSector(new Vector3(-848.2f, -2, 505.6f), new Vector3(5, 3, 5.2f), new Vector3(0, 342, -1.07f), 2200,
                "BUSHES7", "TREES_SMALL4", "TREES_MEDIUM3", "LakeNice", "TRAFFIC", "Tile");

            // Хижина.
            CreateNewSector(new Vector3(-165.55f, -3.7f, 1020.7f), new Vector3(5, 4, 3.5f), 1400, "LakeNice", "Tile", "BUSHES7", "TREES_SMALL1");

            // Сектор подъездной дорожки (только в Performance).
            if (MoprSettings.Mode == PerformanceMode.Performance)
                CreateNewSector(new Vector3(-18.5f, -0.5062422f, 11.9f), new Vector3(11f, 5, 9.5f), 3000,
                    "TREES_SMALL1", "BUSHES7", "BUSHES3", "BUSHES6", "TREES_MEDIUM3", "YARD", "LakeNice", "Tile", "PierHome");

            // Секторы из файлов правил.
            if (RulesManager.Instance.GetList<NewSector>().Count > 0)
            {
                foreach (NewSector sector in RulesManager.Instance.GetList<NewSector>())
                {
                    try
                    {
                        CreateNewSector(sector.Position, sector.Scale, sector.Rotation, 300, sector.Whitelist);
                    }
                    catch (System.Exception ex)
                    {
                        ExceptionManager.New(ex, false, "CUSTOM_SECTOR_FAIL");
                    }
                }
            }

            ModConsole.Log("[MOPR] Loaded " + sectors.Count + " sectors");
        }

        private void CreateNewSector(Vector3 position, Vector3 size, int renderDistance, params string[] ignoreList)
            => CreateNewSector(position, size, Vector3.zero, renderDistance, ignoreList);

        private void CreateNewSector(Vector3 position, Vector3 size, Vector3 rotation, int renderDistance, params string[] ignoreList)
        {
            GameObject newSector = new GameObject("MOPR_Sector");
            newSector.transform.position = position;
            newSector.transform.localEulerAngles = rotation;

            Sector sectorInfo = newSector.AddComponent<Sector>();
            sectorInfo.Initialize(size, renderDistance, ignoreList ?? new string[0]);
            Add(sectorInfo);
        }

        internal void AddActiveSector(Sector sector)
        {
            if (!activeSectors.Contains(sector))
                activeSectors.Add(sector);
        }

        internal void RemoveActiveSector(Sector sector)
        {
            if (activeSectors.Contains(sector))
                activeSectors.Remove(sector);
        }

        /// <summary>Есть ли среди активных секторов whitelist, содержащий это имя.</summary>
        public bool SectorRulesContains(string name)
        {
            for (int i = 0; i < activeSectors.Count; i++)
                if (activeSectors[i].IgnoreList.Contains(name))
                    return true;
            return false;
        }

        public bool IsPlayerInSector() => activeSectors.Count > 0;

        public int GetCurrentSectorDrawDistance()
        {
            if (activeSectors.Count == 0)
                throw new System.Exception("No sector is active.");

            return activeSectors[0].DrawDistance;
        }

        public bool IsPlayerInSector(Sector sector) => activeSectors.Contains(sector);

        public Sector Add(Sector obj)
        {
            sectors.Add(obj);
            return obj;
        }

        public void Remove(Sector obj)
        {
            if (sectors.Contains(obj))
                sectors.Remove(obj);
        }

        public void RemoveAt(int index) => sectors.RemoveAt(index);

        public int EnabledCount => activeSectors.Count;
    }
}
