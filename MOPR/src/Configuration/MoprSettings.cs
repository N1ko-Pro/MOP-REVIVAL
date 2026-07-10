// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Единый источник настроек. Владеет контролами MSCLoader (их наполняет окно настроек),
// предоставляет null-safe типизированные геттеры, вычисляет профиль производительности и
// множитель дистанции, хранит рантайм-состояние и читает/пишет JSON-конфиг мода.

using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using MSCLoader;
using UnityEngine;

using MOPR.Common.Enumerations;

namespace MOPR.Common
{
    internal static class MoprSettings
    {
        #region Рантайм-состояние

        public static bool IsModActive { get; set; }          // Мастер-переключатель: цикл замирает при false.
        public const int UnityCarActiveDistance = 5;          // Дистанция переключения физики транспорта.
        public static bool GenerateToggledItemsListDebug;
        public static bool LoadedOnce;                        // Была ли игра полностью загружена хотя бы раз.

        public static GameFixStatus GameFixStatus;
        public static bool ForceLoadRestart;

        internal static int Restarts = 0;
        internal const int MaxRestarts = 5;
        internal static bool RestartWarningShown = false;

        private static float shadowDistanceOriginalValue;
        private static int vsyncCount = -1;

        #endregion

        #region Контролы (создаются окном настроек)

        // Профиль (радиогруппа) и дистанция.
        public static SettingsCheckBoxGroup ModeUltra, ModeQuality, ModeBalanced, ModePerformance;
        public static SettingsSliderInt ActiveDistance;
        public static SettingsText Description;

        // Модули оптимизации.
        public static SettingsCheckBox OptimizeItems, OptimizePlaces, OptimizeVehicles, OptimizeSatsuma;

        // Глубокая оптимизация Сатсумы за рулём (гасит триггеры/пивоты/косметику, пока игрок в машине).
        public static SettingsCheckBox SatsumaDrivingMode;

        // Движковые Harmony-патчи игрового кода (Performance/Engine).
        public static SettingsCheckBox EnginePatches;

        // Глобальные системные оптимизации: сон дальних тел и адаптивная сборка мусора.
        public static SettingsCheckBox SleepDistantBodies, AdaptiveGc;

        // Графика.
        public static SettingsCheckBox AdjustShadows, DynamicDrawDistanceEnabled, SectorCulling, LimitFramerate, RunInBackground;
        public static SettingsSliderInt ShadowDistance, FramerateLimit;

        // Исправления / прочее.
        public static SettingsCheckBox DisableSkidmarks, DisableEmptyItems, DestroyEmptyBottles;

        // Отладка / аварийный режим.
        public static SettingsCheckBox ShowOverlay, ShowLogMessages, DisableOptimization;

        // Сохранения (окно контролы пока не создаёт — действуют дефолты «включено»).
        public static SettingsCheckBox SaveProtect, SaveBackup, SaveVerify, RestoreBolts;

        // Язык.
        public static SettingsDropDownList Language;

        #endregion

        #region Профиль / дистанция

        /// <summary>Текущий профиль. Вычисляется из радиогруппы, по умолчанию Balanced.</summary>
        public static PerformanceMode Mode
        {
            get
            {
                if (ModeUltra != null && ModeUltra.GetValue()) return PerformanceMode.UltraQuality;
                if (ModeQuality != null && ModeQuality.GetValue()) return PerformanceMode.Quality;
                if (ModePerformance != null && ModePerformance.GetValue()) return PerformanceMode.Performance;
                return PerformanceMode.Balanced;
            }
        }

        /// <summary>Индекс слайдера дистанции, выставляемый при выборе профиля.</summary>
        public static int ModeSliderPreset
        {
            get
            {
                switch (Mode)
                {
                    case PerformanceMode.Performance: return 1;  // Близко (0.75x)
                    case PerformanceMode.Quality: return 3;      // Далеко (2x)
                    case PerformanceMode.UltraQuality: return 4; // Очень далеко (4x)
                    default: return 2;                           // 1x — Balanced
                }
            }
        }

        /// <summary>Индекс слайдера Active Distance (0..4). До построения UI — 2 (1x).</summary>
        public static int ActiveDistanceValue => ActiveDistance != null ? ActiveDistance.GetValue() : 2;

        /// <summary>Множитель базовой дистанции объектов (например, 200 * 0.5 = 100).</summary>
        public static float ActiveDistanceMultiplicationValue
        {
            get
            {
                switch (ActiveDistanceValue)
                {
                    case 0: return 0.5f;
                    case 1: return 0.75f;
                    case 3: return 2f;
                    case 4: return 4f;
                    default: return 1f;
                }
            }
        }

        #endregion

        #region Типизированные геттеры (null-safe до построения окна)

        public static bool LimitFramerateOn => LimitFramerate != null && LimitFramerate.GetValue();
        public static int FramerateLimitValue => FramerateLimit != null ? FramerateLimit.GetValue() : 60;
        public static bool AdjustShadowsOn => AdjustShadows != null && AdjustShadows.GetValue();
        public static int ShadowDistanceValue => ShadowDistance != null ? ShadowDistance.GetValue() : 200;
        public static bool RunInBackgroundOn => RunInBackground == null || RunInBackground.GetValue();
        public static bool DynamicDrawDistanceOn => DynamicDrawDistanceEnabled == null || DynamicDrawDistanceEnabled.GetValue();
        public static bool SectorCullingOn => SectorCulling == null || SectorCulling.GetValue();

        // Глубокая оптимизация Сатсумы за рулём (по умолчанию включена).
        public static bool SatsumaDrivingModeOn => SatsumaDrivingMode == null || SatsumaDrivingMode.GetValue();

        // Движковые патчи (по умолчанию включены).
        public static bool EnginePatchesOn => EnginePatches == null || EnginePatches.GetValue();

        // Системные оптимизации (по умолчанию включены).
        public static bool SleepDistantBodiesOn => SleepDistantBodies == null || SleepDistantBodies.GetValue();
        public static bool AdaptiveGcOn => AdaptiveGc == null || AdaptiveGc.GetValue();

        // Модули оптимизации (по умолчанию включены). Отключение оставляет соответствующие
        // объекты активными и пропускает их обработку в цикле.
        public static bool OptimizeItemsOn => OptimizeItems == null || OptimizeItems.GetValue();
        public static bool OptimizePlacesOn => OptimizePlaces == null || OptimizePlaces.GetValue();
        public static bool OptimizeVehiclesOn => OptimizeVehicles == null || OptimizeVehicles.GetValue();

        public static bool DisableSkidmarksOn => DisableSkidmarks != null && DisableSkidmarks.GetValue();
        public static bool DisableEmptyItemsOn => DisableEmptyItems != null && DisableEmptyItems.GetValue();
        public static bool DestroyEmptyBottlesOn => DestroyEmptyBottles != null && DestroyEmptyBottles.GetValue();

        public static bool ShowOverlayOn => ShowOverlay != null && ShowOverlay.GetValue();
        public static bool ShowLogMessagesOn => ShowLogMessages != null && ShowLogMessages.GetValue();
        public static bool DisableOptimizationOn => DisableOptimization != null && DisableOptimization.GetValue();

        public static bool SaveProtectionOn => SaveProtect == null || SaveProtect.GetValue();
        public static bool SaveBackupOn => SaveBackup == null || SaveBackup.GetValue();
        public static bool SaveVerifyOn => SaveVerify == null || SaveVerify.GetValue();
        public static bool RestoreBoltsOn => RestoreBolts == null || RestoreBolts.GetValue();

        #endregion

        #region Применение к Unity (колбэки контролов)

        /// <summary>Профиль вычисляется на лету; метод оставлен хуком для колбэков UI.</summary>
        internal static void UpdatePerformanceMode()
        {
        }

        internal static void UpdateFramerateLimiter()
        {
            Application.targetFrameRate = LimitFramerateOn ? FramerateLimitValue : -1;
        }

        internal static void UpdateShadows()
        {
            if (shadowDistanceOriginalValue == 0)
                shadowDistanceOriginalValue = QualitySettings.shadowDistance;

            QualitySettings.shadowDistance = AdjustShadowsOn ? ShadowDistanceValue : shadowDistanceOriginalValue;
        }

        public static void UpdateMiscSettings()
        {
            ToggleBackgroundRunning();

            // Запоминаем исходный vsync и восстанавливаем его (игра иногда сбрасывает значение).
            if (vsyncCount == -1)
                vsyncCount = QualitySettings.vSyncCount;
            else
                QualitySettings.vSyncCount = vsyncCount;
        }

        internal static void ToggleBackgroundRunning()
        {
            Application.runInBackground = RunInBackgroundOn;
        }

        #endregion

        #region Сохранение конфига

        public static string DataFile => Path.Combine(MOPR.ModConfigPath, "MopData.json");
        private static MoprData loadedData;

        private static JsonSerializerSettings GetNewSettings()
        {
            return new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Utc };
        }

        public static void WriteData(MoprData data)
        {
            string json = JsonConvert.SerializeObject(data, GetNewSettings());
            using (StreamWriter writer = new StreamWriter(DataFile))
                writer.Write(json);
        }

        private static MoprData ReadData()
        {
            if (!File.Exists(DataFile))
                return new MoprData { LastModList = new List<string>() };

            string content;
            using (StreamReader reader = new StreamReader(DataFile))
                content = reader.ReadToEnd();

            return JsonConvert.DeserializeObject<MoprData>(content, GetNewSettings());
        }

        public static MoprData Data => loadedData ?? (loadedData = ReadData());

        #endregion
    }
}
