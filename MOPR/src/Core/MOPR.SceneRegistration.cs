// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Регистрация сцены: разовая инициализация при загрузке. Создаёт менеджеры, регистрирует объекты
// мира и локации, триггеры зон Сатсумы, применяет фиксы и правила и запускает главный цикл. Каждый
// блок изолирован через Guard, чтобы сбой одного объекта не ломал всю инициализацию.

using System;
using System.Linq;
using UnityEngine;
using MOPR.Common;
using MOPR.Common.Enumerations;
using MOPR.FSM;
using MOPR.Helpers;
using MOPR.Items;
using MOPR.Managers;

using MOPR.Vehicles;
using MOPR.Vehicles.Cases;
using MOPR.WorldObjects;
using MOPR.Rules;
using MOPR.Rules.Types;
using MOPR.Indoors;
using HutongGames.PlayMaker;

namespace MOPR
{
    internal partial class Core : MonoBehaviour
    {
        #region Инициализация сцены

        private void Initialize()
        {
            ModConsole.Log("[MOPR] Loading MOPR...");

            // Парсим локальные правила (их синхронизирует RemoteRuleSync) до регистрации объектов.
            Guard("RULES_LOAD_ERROR", RulesManager.LoadActive);

            // Движковые Harmony-патчи игрового кода (один раз за процесс, за тумблером).
            Guard("ENGINE_PATCHES_APPLY_ERROR", Performance.Engine.EnginePatches.Apply);

            CreateManagersAndFindPlayer();
            RegisterPrimaryWorldObjects();

            placeManager = new PlaceManager();
            GameFixes.Instance.MainFixes();

            RegisterHomeDependentWorldObjects();
            RegisterSatsumaAreaTriggers();
            RegisterDecorativeObjects();

            InitializeItems();
            HookPreSaveGame();

            LoadRules();
            AttachAuxiliaryManagers();

            // Начальное состояние сцены: всё выключено.
            Guard("TOGGLING_ALL_ERROR", () => ToggleAll(false, ToggleAllMode.OnLoad));

            LocateComputerSystem();
            CacheTrafficReferences();

            Startup();

            // Учитываем аварийный тумблер, если он был включён ещё до загрузки.
            Guard("APPLY_OPTIMIZATION_SETTING_ERROR", ApplyOptimizationSetting);

            GenerateDebugListsIfRequested();
        }

        #endregion

        #region Шаги инициализации

        /// <summary>Создаёт менеджеры и родитель LOD-дублей, находит игрока, вешает базовые фиксы.</summary>
        private void CreateManagersAndFindPlayer()
        {
            dumbObjectsParent = new GameObject("MOPR_DumbObjects");
            dumbObjectsParent.transform.SetParent(transform);

            worldObjectManager = new WorldObjectManager();

            player = GameObject.Find("PLAYER").transform;

            Guard("GAME_FIXES_INITIALIZATION", () => gameObject.AddComponent<GameFixes>());

            VehicleManager.Instance.Initialize();
        }

        /// <summary>Регистрирует основные объекты мира (из каталога) и скидмарки.</summary>
        private void RegisterPrimaryWorldObjects()
        {
            Guard("WORLD_OBJECTS_1_INITIALIZATION_FAIL", () =>
            {
                WorldObjectCatalog.Register(worldObjectManager, WorldObjectCatalog.Primary);
                worldObjectManager.Add(new SkidmarkObject(GameObject.Find("Skidmarks"), 0));
                ModConsole.Log("[MOPR] World objects (1) loaded");
            });
        }

        /// <summary>Регистрирует объекты (из каталога), включаемые, когда игрок вдали от дома.</summary>
        private void RegisterHomeDependentWorldObjects()
        {
            Guard("WORLD_OBJECTS_2_INITIALIZATION_FAIL", () =>
            {
                WorldObjectCatalog.Register(worldObjectManager, WorldObjectCatalog.HomeDependent);
                ModConsole.Log("[MOPR] World objects (2) loaded");
            });
        }

        /// <summary>Вешает триггеры зон Сатсумы: техосмотр, подъёмник ремонтной, parc ferme.</summary>
        private void RegisterSatsumaAreaTriggers()
        {
            Guard("SATSUMA_AREA_CHECK_INSPECTION_FAIL", () =>
                GameObject.Find("INSPECTION").AddComponent<SatsumaInArea>().Initialize(new Vector3(20, 20, 20)));

            Guard("SATSUMA_AREA_CHECK_REPAIRSHOP_FAIL", () =>
                GameObject.Find("REPAIRSHOP/Lifter/Platform").AddComponent<SatsumaInArea>().Initialize(new Vector3(5, 5, 5)));

            Guard("PARC_FERME_TRIGGER_FAIL", () =>
            {
                GameObject parcFermeTrigger = new GameObject("MOPR_ParcFermeTrigger");
                parcFermeTrigger.transform.parent = GameObject.Find("RALLY").transform.Find("Scenery");
                parcFermeTrigger.transform.position = new Vector3(-1383f, 3f, 1260f);
                parcFermeTrigger.AddComponent<SatsumaInArea>().Initialize(new Vector3(41, 12, 35));
            });

            ModConsole.Log("[MOPR] Satsuma triggers loaded");
        }

        /// <summary>Регистрирует декоративные/тяжёлые по рендеру объекты и правки видимости по режиму.</summary>
        private void RegisterDecorativeObjects()
        {
            Guard("JOKKE_FURNITURE_ERROR", RegisterJokkeFurniture);
            Guard("HAYBALES_FIX_ERROR", RegisterHaybales);
            Guard("LOGWALL_LOAD_ERROR", RegisterLogwalls);
            Guard("CHURCH_LOD_ERROR", RegisterPerajarviChurch);

            // Дома у озера: в Quality не выгружаем.
            Guard("LAKE_HOUSE_ERROR", () =>
            {
                if (MoprSettings.Mode == PerformanceMode.Quality)
                    GameObject.Find("PERAJARVI").transform.Find("TerraceHouse").transform.parent = null;
            });

            Guard("TRAFFIC_VEHICLES_ERROR", RegisterHighwayTraffic);

            // Рендереры FITTAN.
            Guard("FITTAN_RENDERERS_ERROR", () =>
            {
                GenericObject fittan = worldObjectManager.Add(GameObject.Find("TRAFFIC").transform.Find("VehiclesDirtRoad/Rally/FITTAN").gameObject, DisableOn.Distance, 600, ToggleModes.MultipleRenderers);
                fittan.MinimumToggleDistance = 400;
            });

            // Cabin: в Quality не выгружаем дом кабины и сгоревший дом.
            Guard("CABIN_DETAILS_QUALITY", () =>
            {
                if (MoprSettings.Mode >= PerformanceMode.Quality)
                {
                    Transform cabin = GameObject.Find("CABIN").transform;
                    cabin.Find("Cabin").parent = null;
                    cabin.Find("BurntHouse").parent = null;
                }
            });

            // Cottage: в Balanced+ не выгружаем основной рендерер.
            Guard("COTTAGE_DETAILS_QUALITY", () =>
            {
                if (MoprSettings.Mode >= PerformanceMode.Balanced)
                    GameObject.Find("COTTAGE").transform.Find("MESH").parent = null;
            });

            // DANCEHALL: видимый издалека в Balanced+.
            Guard("DANCEHALL_BULDINGS_VISIBILITY_ERROR", () =>
            {
                if (MoprSettings.Mode >= PerformanceMode.Balanced)
                    GameObject.Find("DANCEHALL").transform.Find("Building").parent = null;
            });
        }

        private void RegisterJokkeFurniture()
        {
            if (!GameObject.Find("tv(Clo01)"))
                return;

            foreach (string furniture in WorldObjectCatalog.JokkeFurniture)
            {
                GameObject g = GameObject.Find(furniture);
                if (g)
                {
                    g.transform.parent = null;
                    worldObjectManager.Add(g, DisableOn.Distance, 100, ToggleModes.Renderer);
                }
            }

            ModConsole.Log("[MOPR] Jokke's furnitures found and loaded");
        }

        private void RegisterHaybales()
        {
            GameObject haybalesParent = GameObject.Find("JOBS/HayBales");
            if (haybalesParent == null)
                return;

            // Гасим перезагрузку позиций и вешаем поведение предмета на каждый тюк.
            haybalesParent.GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;
            foreach (Transform haybale in haybalesParent.transform.GetComponentInChildren<Transform>())
                haybale.gameObject.AddComponent<ItemBehaviour>();
        }

        private void RegisterLogwalls()
        {
            foreach (GameObject wall in Resources.FindObjectsOfTypeAll<GameObject>().Where(g => g.name == "LogwallLarge"))
            {
                GenericObject logWall = worldObjectManager.Add(wall, DisableOn.Distance, 300);
                logWall.MinimumToggleDistance = 200;
            }
        }

        private void RegisterPerajarviChurch()
        {
            if (MoprSettings.Mode == PerformanceMode.Performance)
                return;

            GameObject church = GameObject.Find("PERAJARVI").transform.Find("CHURCH").gameObject;
            church.transform.parent = null;
            GameObject churchLOD = church.transform.Find("LOD").gameObject;
            church.GetComponent<PlayMakerFSM>().enabled = false;
            GenericObject churchObject = worldObjectManager.Add(churchLOD, DisableOn.Distance, 300);
            churchObject.MinimumToggleDistance = 200;
        }

        private void RegisterHighwayTraffic()
        {
            Transform vehiclesHighway = GameObject.Find("TRAFFIC").transform.Find("VehiclesHighway");
            foreach (Transform child in vehiclesHighway.GetComponentsInChildren<Transform>(true).Where(f => f.parent == vehiclesHighway))
            {
                GenericObject trafficObj = worldObjectManager.Add(child.gameObject, DisableOn.Distance, 600, ToggleModes.MultipleRenderers);
                trafficObj.MinimumToggleDistance = 400;
            }

            // Заодно чиним лаг при первичной загрузке трафика.
            vehiclesHighway.gameObject.SetActive(true);
        }

        /// <summary>Инициализирует менеджер предметов.</summary>
        private void InitializeItems()
        {
            Guard("ITEMS_CLASS_ERROR", () =>
            {
                ItemsManager.Instance.Initialize();
                ModConsole.Log("[MOPR] Items class initialized");
            }, fatal: true);
        }

        /// <summary>Применяет правила из .mopconfig: переключаемые объекты и смену родителя.</summary>
        private void LoadRules()
        {
            foreach (ToggleRule rule in RulesManager.Instance.GetList<ToggleRule>())
                Guard("TOGGLE_RULES_LOAD_ERROR", () => ApplyToggleRule(rule));

            foreach (ChangeParentRule rule in RulesManager.Instance.GetList<ChangeParentRule>())
                Guard("CHANGE_PARENT_RULE_ERROR", () => ApplyChangeParentRule(rule));
        }

        private void ApplyToggleRule(ToggleRule rule)
        {
            GameObject g = GameObject.Find(rule.ObjectName);
            if (g == null)
            {
                ModConsole.LogError("[MOPR] Couldn't find " + rule.ToggleMode + " " + rule.ObjectName);
                return;
            }

            switch (rule.ToggleMode)
            {
                case ToggleModes.Simple:
                case ToggleModes.Renderer:
                    worldObjectManager.Add(g, DisableOn.Distance, 200, rule.ToggleMode);
                    break;
                case ToggleModes.Item:
                    if (g.GetComponent<ItemBehaviour>() == null)
                        g.AddComponent<ItemBehaviour>();
                    break;
                case ToggleModes.Vehicle:
                case ToggleModes.VehiclePhysics:
                    Vehicle veh = new Vehicle(rule.ObjectName);
                    VehicleManager.Instance.Add(veh);
                    if (rule.ToggleMode == ToggleModes.VehiclePhysics)
                        veh.Toggle = veh.ToggleUnityCar;
                    break;
            }
        }

        private void ApplyChangeParentRule(ChangeParentRule rule)
        {
            GameObject obj = GameObject.Find(rule.ObjectName);
            if (obj == null)
                throw new Exception("Object " + rule.ObjectName + " doesn't exist.");

            if (rule.NewParentName.ToLower() == "null")
            {
                obj.transform.parent = null;
                return;
            }

            GameObject parent = GameObject.Find(rule.NewParentName);
            if (parent == null)
                throw new Exception("Parent " + rule.NewParentName + " doesn't exist.");

            obj.transform.parent = parent.transform;
        }

        /// <summary>Добавляет вспомогательные менеджеры: сектора, DDD, куллер помещений, сканер предметов.</summary>
        private void AttachAuxiliaryManagers()
        {
            Guard("SECTOR_MANAGER_ERROR", () => gameObject.AddComponent<SectorManager>(), fatal: true);
            Guard("DYNAMIC_DRAW_DISTANCE_LOAD_ERROR", () => gameObject.AddComponent<DynamicDrawDistance>());
            Guard("INDOOR_CULLER_ERROR", () => gameObject.AddComponent<IndoorCuller>());
            Guard("ITEM_SCANNER_ERROR", () => gameObject.AddComponent<ItemScanner>());
            Guard("RIGIDBODY_SLEEPER_ERROR", () => gameObject.AddComponent<Performance.Optimizers.RigidbodySleeper>());
            Guard("ADAPTIVE_GC_ERROR", () => gameObject.AddComponent<Performance.Optimizers.AdaptiveGcCollector>());
            Guard("DEBUG_MONITOR_ERROR", () => gameObject.AddComponent<DebugTools.DebugMonitor>());
        }

        /// <summary>Находит систему компьютера и вешает обработчик паузы на меню опций.</summary>
        private void LocateComputerSystem()
        {
            Guard("COMPUTER_SYSTEM_ERROR", () =>
            {
                GameObject computer = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(g => g.name == "COMPUTER");
                if (computer != null)
                {
                    computerSystem = computer.transform.Find("SYSTEM").gameObject;
                    GameObject.Find("Systems").transform.Find("OptionsMenu").gameObject.AddComponent<MopPauseMenuHandler>();
                }
            });
        }

        /// <summary>Кэширует ссылки на корни трафика для проверки столкновений в Update.</summary>
        private void CacheTrafficReferences()
        {
            Guard("TRAFFIC_LOOKUP_ERROR", () =>
            {
                traffic = worldObjectManager.Get("TRAFFIC");
                trafficDirt = worldObjectManager.Get("VehiclesDirtRoad");
                trafficHighway = worldObjectManager.Get("VehiclesHighway");
            });
        }

        /// <summary>По debug-настройке выгружает списки переключаемых объектов.</summary>
        private void GenerateDebugListsIfRequested()
        {
            if (!MoprSettings.GenerateToggledItemsListDebug)
                return;

            ToggledItemsListGenerator.CreateWorldList(WorldObjectManager.Instance.GetAll);
            ToggledItemsListGenerator.CreateVehicleList(VehicleManager.Instance.GetAll);
            ToggledItemsListGenerator.CreateItemsList(ItemsManager.Instance.GetAll);
            ToggledItemsListGenerator.CreatePlacesList(PlaceManager.Instance.GetAll);
        }

        #endregion
    }
}
