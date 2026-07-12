// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Ядро мода: MonoBehaviour-синглтон, управляющий всей оптимизацией сцены. Здесь — поля, доступ к
// синглтону, запуск/остановка и общие аксессоры. Остальные обязанности разнесены по partial-файлам
// того же класса (Loop / DistanceRules / MassToggle / Loading / SceneRegistration / SaveHooks /
// Watchdog / Diagnostics).

using System;
using System.Collections.Generic;
using UnityEngine;

using MOPR.Managers;
using MOPR.Common;
using MOPR.FSM;
using MOPR.Items;
using MOPR.Localization;

namespace MOPR
{
    internal partial class Core : MonoBehaviour
    {
        #region Синглтон

        private static Core instance;
        public static Core Instance => instance;

        #endregion

        #region Ссылки сцены и менеджеры

        private Transform player;
        private PlaceManager placeManager;
        private WorldObjectManager worldObjectManager;

        private GameObject computerSystem;
        private GameObject dumbObjectsParent;

        private readonly string[] trafficVehicleRoots = { "NPC_CARS", "TRAFFIC", "RALLY" };
        public string[] TrafficVehicleRoots => trafficVehicleRoots;
        private GameObject traffic, trafficHighway, trafficDirt;

        #endregion

        #region Рантайм-состояние

        private bool isPlayerAtYard;
        private bool inSectorMode;

        // Предметы гасятся до транспорта, включаются — после (см. главный цикл).
        private readonly Queue<ItemBehaviour> itemsToRemove = new Queue<ItemBehaviour>();
        private readonly Queue<ItemBehaviour> itemsToEnable = new Queue<ItemBehaviour>();

        #endregion

        #region Переменные загрузки

        private readonly CharacterController playerController;
        private bool itemInitializationDelayDone;
        private bool isFinishedCheckingSatsuma;

        private const int WaitForPhysicsToSettleTime = 2;  // сек ожидания «устаканивания» физики
        private const int WaitForSatsumaCheckTime = 10;     // сек ожидания проверки догрузки Сатсумы
        private const int LoadScreenWorkaroundTime = 20;    // сек до принудительного снятия загрузэкрана

        private readonly LoadScreen loadScreen;

        #endregion

        #region Конструирование и старт

        private Core()
        {
            instance = this;

            MoprSettings.LoadedOnce = true;

            // Загрузочный экран (IMGUI, кадры вшиты в DLL — ассет-бандл не нужен).
            try
            {
                GameObject loadscreenObj = new GameObject("MOPR_LoadScreen");
                loadScreen = loadscreenObj.AddComponent<LoadScreen>();
                loadScreen.Activate();

                loadScreenWorkaround = InfiniteLoadscreenWorkaround();
                StartCoroutine(loadScreenWorkaround);
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "LOAD_SCREEN_ERROR");
            }

            // Отключаем управление игроком — будто он в главном меню.
            playerController = GameObject.Find("PLAYER").GetComponent<CharacterController>();
            playerController.enabled = false;
            FsmManager.PlayerInMenu = true;

            ExceptionManager.SessionTimeStart = DateTime.Now;

            StartCoroutine(DelayedInitializationRoutine());
        }

        /// <summary>Запускает главный цикл и сторож живучести после инициализации сцены.</summary>
        public void Startup()
        {
            currentLoop = LoopRoutine();
            StartCoroutine(currentLoop);

            currentControlCoroutine = ControlCoroutine();
            StartCoroutine(currentControlCoroutine);

            LogLoadStatus();

            Resources.UnloadUnusedAssets();
            GC.Collect();
        }

        /// <summary>
        /// Всегда видимый (не гейтится «Показывать логи») визуальный статус загрузки при входе в сейв:
        /// ядро + состояние каждого модуля оптимизации. Подробные технические логи остаются в Log().
        /// </summary>
        private void LogLoadStatus()
        {
            ModConsole.Status("[MOPR] " + MoprColors.Success(LocalizationCore.Get("status.core_loaded")));

            // Модули оптимизации: показываем состояние и число объектов под управлением.
            LogModuleStatus("status.mod.world", worldObjectManager != null ? worldObjectManager.Count : 0, true);
            LogModuleStatus("status.mod.vehicles", VehicleManager.Instance.Count, MoprSettings.OptimizeVehiclesOn);
            LogModuleStatus("status.mod.items", ItemsManager.Instance.Count, MoprSettings.OptimizeItemsOn);
            LogModuleStatus("status.mod.places", placeManager != null ? placeManager.Count : 0, MoprSettings.OptimizePlacesOn);

            // Подсистемы: включено/отключено (или «применено» для разовых фиксов).
            LogFeatureStatus("status.feat.fixes", true, FeatureWord.Applied);
            LogFeatureStatus("status.feat.engine", MoprSettings.EnginePatchesOn);
            LogFeatureStatus("status.feat.sector", MoprSettings.SectorCullingOn);
            LogFeatureStatus("status.feat.ddd", MoprSettings.DynamicDrawDistanceOn);
            LogFeatureStatus("status.feat.sleep", MoprSettings.SleepDistantBodiesOn);
            LogFeatureStatus("status.feat.gc", MoprSettings.AdaptiveGcOn);
            LogFeatureStatus("status.feat.saves", MoprSettings.SaveProtectionOn);

            ModConsole.Status("[MOPR] " + MoprColors.Success(LocalizationCore.Get("status.ready")));
        }

        /// <summary>Строка статуса модуля: имя (акцент) + «Загружен (N)» либо приглушённое «Отключён».</summary>
        private static void LogModuleStatus(string moduleKey, int count, bool loaded)
        {
            string name = LocalizationCore.Get(moduleKey);
            if (loaded)
                ModConsole.Status("[MOPR] " + LocalizationCore.Get("status.module_loaded", MoprColors.Accent(name), count));
            else
                ModConsole.Status("[MOPR] " + MoprColors.Muted(LocalizationCore.Get("status.module_off", name)));
        }

        private enum FeatureWord { OnOff, Applied }

        /// <summary>Строка статуса подсистемы: имя (акцент) + цветное слово состояния.</summary>
        private static void LogFeatureStatus(string nameKey, bool on, FeatureWord word = FeatureWord.OnOff)
        {
            string name = MoprColors.Accent(LocalizationCore.Get(nameKey));
            string state = word == FeatureWord.Applied
                ? MoprColors.Success(LocalizationCore.Get("status.word.applied"))
                : on
                    ? MoprColors.Success(LocalizationCore.Get("status.word.enabled"))
                    : MoprColors.Muted(LocalizationCore.Get("status.word.disabled"));

            ModConsole.Status("[MOPR] " + LocalizationCore.Get("status.feature", name, state));
        }

        #endregion

        #region Аксессоры

        public Transform GetPlayer() => player;

        public bool IsInSector() => inSectorMode;

        public bool IsItemInitializationDone() => itemInitializationDelayDone;

        public Transform DumbObjectParent => dumbObjectsParent.transform;

        /// <summary>Переключает чекбокс «Показывать монитор отладки» (монитор читает настройку вживую).</summary>
        internal void ToggleDebugMode()
        {
            if (MoprSettings.ShowOverlay != null)
                Interface.Helpers.SettingsReflection.SetToggle(MoprSettings.ShowOverlay, !MoprSettings.ShowOverlayOn);
        }

        #endregion
    }
}
