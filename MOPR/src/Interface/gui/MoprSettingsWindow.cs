// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Строит окно настроек мода (панель MSCLoader) на Settings-API с локализацией. Контролы сохраняются
// в MoprSettings. 4 профиля производительности двигают ползунок дистанции; языковой список мгновенно
// переводит всё окно.

using MSCLoader;
using MOPR.Common;
using MOPR.Saves;
using MOPR.Rules;
using MOPR.Localization;
using MOPR.Interface.Helpers;

namespace MOPR.Interface.Gui
{
    internal static class MoprSettingsWindow
    {
        // Ползунок двигается по профилю только после загрузки настроек — иначе первичный клик сбивает значение.
        private static bool settingsInteractive;

        /// <summary>Строит всю панель настроек на сохранённом языке.</summary>
        public static void Build(Mod mod)
        {
            LocalizationConfig.Initialize(mod);
            LocalizedUi.Clear();
            settingsInteractive = false;

            BuildLanguage();
            BuildOptimization();
            BuildGraphics();
            BuildGame();
            BuildFixes();
            BuildSaves();
            BuildRulesServer();
            BuildDiagnostics();
            BuildInfo(mod);
        }

        /// <summary>После загрузки настроек: применяет графику и разрешает пресеты.</summary>
        public static void OnSettingsLoaded()
        {
            settingsInteractive = true;
            ApplyGraphics();
        }

        #region Секции

        private static void BuildLanguage()
        {
            Settings.AddHeader(LocalizationCore.Get("settings.language"));
            MoprSettings.Language = Settings.AddDropDownList(
                "mopr_language", " ",
                new[] { "English", "Русский" }, (int)LocalizationCore.Current, OnLanguageChanged);
        }

        private static void BuildOptimization()
        {
            LocalizedUi.Header(Settings.AddHeader(LocalizationCore.Get("settings.despawn_header")), "settings.despawn_header");
            LocalizedUi.Text(Settings.AddText(LocalizationCore.Get("settings.mode_header")), "settings.mode_header");

            MoprSettings.ModeUltra = Settings.AddCheckBoxGroup("mopr_mode_ultra", LocalizationCore.Get("settings.mode.ultra"), false, "mopr_mode", OnModeChanged);
            LocalizedUi.Label(MoprSettings.ModeUltra, "settings.mode.ultra");
            MoprSettings.ModeQuality = Settings.AddCheckBoxGroup("mopr_mode_quality", LocalizationCore.Get("settings.mode.quality"), false, "mopr_mode", OnModeChanged);
            LocalizedUi.Label(MoprSettings.ModeQuality, "settings.mode.quality");
            MoprSettings.ModeBalanced = Settings.AddCheckBoxGroup("mopr_mode_balanced", LocalizationCore.Get("settings.mode.balanced"), true, "mopr_mode", OnModeChanged);
            LocalizedUi.Label(MoprSettings.ModeBalanced, "settings.mode.balanced");
            MoprSettings.ModePerformance = Settings.AddCheckBoxGroup("mopr_mode_perf", LocalizationCore.Get("settings.mode.performance"), false, "mopr_mode", OnModeChanged);
            LocalizedUi.Label(MoprSettings.ModePerformance, "settings.mode.performance");

            // Пер-модульные тумблеры оптимизации (по умолчанию включены).
            LocalizedUi.Text(Settings.AddText(LocalizationCore.Get("settings.subsystems_header")), "settings.subsystems_header");
            MoprSettings.OptimizeItems = Settings.AddCheckBox("mopr_opt_items", LocalizationCore.Get("settings.optimize_items"), true);
            LocalizedUi.Label(MoprSettings.OptimizeItems, "settings.optimize_items");
            MoprSettings.OptimizePlaces = Settings.AddCheckBox("mopr_opt_places", LocalizationCore.Get("settings.optimize_places"), true);
            LocalizedUi.Label(MoprSettings.OptimizePlaces, "settings.optimize_places");
            MoprSettings.OptimizeVehicles = Settings.AddCheckBox("mopr_opt_vehicles", LocalizationCore.Get("settings.optimize_vehicles"), true);
            LocalizedUi.Label(MoprSettings.OptimizeVehicles, "settings.optimize_vehicles");

            MoprSettings.ActiveDistance = Settings.AddSlider(
                "mopr_active_distance", LocalizationCore.Get("settings.active_distance"),
                0, 4, 2, textValues: BuildDistanceLabels());
            LocalizedUi.Label(MoprSettings.ActiveDistance, "settings.active_distance");
            LocalizedUi.DistanceSlider(MoprSettings.ActiveDistance, BuildDistanceLabels);

            MoprSettings.Description = Settings.AddText(LocalizationCore.Get("settings.desc"));
            LocalizedUi.Text(MoprSettings.Description, "settings.desc");

            MoprSettings.SatsumaDrivingMode = Settings.AddCheckBox("mopr_satsuma_driving", LocalizationCore.Get("settings.satsuma_driving"), true);
            LocalizedUi.Label(MoprSettings.SatsumaDrivingMode, "settings.satsuma_driving");
            LocalizedUi.Text(Settings.AddText(LocalizationCore.Get("settings.satsuma_driving_hint")), "settings.satsuma_driving_hint");
        }

        private static void BuildGraphics()
        {
            LocalizedUi.Header(Settings.AddHeader(LocalizationCore.Get("settings.graphics_header")), "settings.graphics_header");

            MoprSettings.AdjustShadows = Settings.AddCheckBox("mopr_adjust_shadows", LocalizationCore.Get("settings.adjust_shadows"), false, ApplyGraphics);
            LocalizedUi.Label(MoprSettings.AdjustShadows, "settings.adjust_shadows");
            MoprSettings.ShadowDistance = Settings.AddSlider("mopr_shadow_distance", LocalizationCore.Get("settings.shadow_distance"), 0, 2000, 200, ApplyGraphics);
            LocalizedUi.Label(MoprSettings.ShadowDistance, "settings.shadow_distance");

            MoprSettings.DynamicDrawDistanceEnabled = Settings.AddCheckBox("mopr_ddd", LocalizationCore.Get("settings.dynamic_draw"), true);
            LocalizedUi.Label(MoprSettings.DynamicDrawDistanceEnabled, "settings.dynamic_draw");
            LocalizedUi.Text(Settings.AddText(LocalizationCore.Get("settings.dynamic_draw_hint")), "settings.dynamic_draw_hint");

            MoprSettings.SectorCulling = Settings.AddCheckBox("mopr_sector_cull", LocalizationCore.Get("settings.sector_cull"), true);
            LocalizedUi.Label(MoprSettings.SectorCulling, "settings.sector_cull");
            LocalizedUi.Text(Settings.AddText(LocalizationCore.Get("settings.sector_cull_hint")), "settings.sector_cull_hint");

            MoprSettings.EnginePatches = Settings.AddCheckBox("mopr_engine_patches", LocalizationCore.Get("settings.engine_patches"), true);
            LocalizedUi.Label(MoprSettings.EnginePatches, "settings.engine_patches");
            LocalizedUi.Text(Settings.AddText(LocalizationCore.Get("settings.engine_patches_hint")), "settings.engine_patches_hint");
        }

        private static void BuildGame()
        {
            LocalizedUi.Header(Settings.AddHeader(LocalizationCore.Get("settings.game_header")), "settings.game_header");

            MoprSettings.LimitFramerate = Settings.AddCheckBox("mopr_limit_fps", LocalizationCore.Get("settings.limit_fps"), false, ApplyGraphics);
            LocalizedUi.Label(MoprSettings.LimitFramerate, "settings.limit_fps");
            MoprSettings.FramerateLimit = Settings.AddSlider("mopr_fps_limit", LocalizationCore.Get("settings.fps_limit"), 20, 200, 60, ApplyGraphics);
            LocalizedUi.Label(MoprSettings.FramerateLimit, "settings.fps_limit");

            MoprSettings.RunInBackground = Settings.AddCheckBox("mopr_run_background", LocalizationCore.Get("settings.run_background"), true, ApplyGraphics);
            LocalizedUi.Label(MoprSettings.RunInBackground, "settings.run_background");

            MoprSettings.SleepDistantBodies = Settings.AddCheckBox("mopr_sleep_bodies", LocalizationCore.Get("settings.sleep_bodies"), true);
            LocalizedUi.Label(MoprSettings.SleepDistantBodies, "settings.sleep_bodies");
            LocalizedUi.Text(Settings.AddText(LocalizationCore.Get("settings.sleep_bodies_hint")), "settings.sleep_bodies_hint");

            MoprSettings.AdaptiveGc = Settings.AddCheckBox("mopr_adaptive_gc", LocalizationCore.Get("settings.gc_adaptive"), true);
            LocalizedUi.Label(MoprSettings.AdaptiveGc, "settings.gc_adaptive");
            LocalizedUi.Text(Settings.AddText(LocalizationCore.Get("settings.gc_adaptive_hint")), "settings.gc_adaptive_hint");
        }

        private static void BuildFixes()
        {
            LocalizedUi.Header(Settings.AddHeader(LocalizationCore.Get("settings.fixes_header")), "settings.fixes_header");

            MoprSettings.DisableSkidmarks = Settings.AddCheckBox("mopr_disable_skidmarks", LocalizationCore.Get("settings.disable_skidmarks"), false);
            LocalizedUi.Label(MoprSettings.DisableSkidmarks, "settings.disable_skidmarks");
            LocalizedUi.Text(Settings.AddText(LocalizationCore.Get("settings.disable_skidmarks_hint")), "settings.disable_skidmarks_hint");

            MoprSettings.DisableEmptyItems = Settings.AddCheckBox("mopr_disable_empty_items", LocalizationCore.Get("settings.disable_empty_items"), false);
            LocalizedUi.Label(MoprSettings.DisableEmptyItems, "settings.disable_empty_items");
            LocalizedUi.Text(Settings.AddText(LocalizationCore.Get("settings.disable_empty_items_hint")), "settings.disable_empty_items_hint");

            MoprSettings.DestroyEmptyBottles = Settings.AddCheckBox("mopr_destroy_empty_bottles", LocalizationCore.Get("settings.destroy_empty_bottles"), false);
            LocalizedUi.Label(MoprSettings.DestroyEmptyBottles, "settings.destroy_empty_bottles");
            LocalizedUi.Text(Settings.AddText(LocalizationCore.Get("settings.destroy_empty_bottles_hint")), "settings.destroy_empty_bottles_hint");

            MoprSettings.HideLakeVegetation = Settings.AddCheckBox("mopr_fix_lake_weed", LocalizationCore.Get("settings.fix_lake_weed"), false);
            LocalizedUi.Label(MoprSettings.HideLakeVegetation, "settings.fix_lake_weed");
            LocalizedUi.Text(Settings.AddText(LocalizationCore.Get("settings.fix_lake_weed_hint")), "settings.fix_lake_weed_hint");

            MoprSettings.ParkingBrakeAnchor = Settings.AddCheckBox("mopr_fix_parking_brake_anchor", LocalizationCore.Get("settings.parking_brake_anchor"), false);
            LocalizedUi.Label(MoprSettings.ParkingBrakeAnchor, "settings.parking_brake_anchor");
            LocalizedUi.Text(Settings.AddText(LocalizationCore.Get("settings.parking_brake_anchor_hint")), "settings.parking_brake_anchor_hint");
        }

        private static void BuildSaves()
        {
            LocalizedUi.Header(Settings.AddHeader(LocalizationCore.Get("settings.saves_header")), "settings.saves_header");

            MoprSettings.SaveProtect = Settings.AddCheckBox("mopr_save_protect", LocalizationCore.Get("settings.save_protect"), true);
            LocalizedUi.Label(MoprSettings.SaveProtect, "settings.save_protect");
            LocalizedUi.Text(Settings.AddText(LocalizationCore.Get("settings.save_protect_hint")), "settings.save_protect_hint");

            MoprSettings.SaveBackup = Settings.AddCheckBox("mopr_save_backup", LocalizationCore.Get("settings.save_backup"), true);
            LocalizedUi.Label(MoprSettings.SaveBackup, "settings.save_backup");
            LocalizedUi.Text(Settings.AddText(LocalizationCore.Get("settings.save_backup_hint")), "settings.save_backup_hint");

            MoprSettings.SaveVerify = Settings.AddCheckBox("mopr_save_verify", LocalizationCore.Get("settings.save_verify"), true);
            LocalizedUi.Label(MoprSettings.SaveVerify, "settings.save_verify");
            LocalizedUi.Text(Settings.AddText(LocalizationCore.Get("settings.save_verify_hint")), "settings.save_verify_hint");

            MoprSettings.RestoreBolts = Settings.AddCheckBox("mopr_restore_bolts", LocalizationCore.Get("settings.restore_bolts"), true);
            LocalizedUi.Label(MoprSettings.RestoreBolts, "settings.restore_bolts");
            LocalizedUi.Text(Settings.AddText(LocalizationCore.Get("settings.restore_bolts_hint")), "settings.restore_bolts_hint");

            Settings.AddButton(LocalizationCore.Get("settings.backup_now"), OnBackupNow, true);
            Settings.AddButton(LocalizationCore.Get("settings.restore_latest"), OnRestoreLatest, true);
        }

        private static void BuildRulesServer()
        {
            LocalizedUi.Header(Settings.AddHeader(LocalizationCore.Get("settings.rules_header")), "settings.rules_header");

            SettingsText status = Settings.AddText(LocalizationCore.Get(RemoteRuleSync.StatusKey));
            LocalizedUi.DynamicText(status, () => RemoteRuleSync.StatusKey);

            Settings.AddButton(LocalizationCore.Get("settings.rules_refresh"), OnRefreshRules, true);
            Settings.AddButton(LocalizationCore.Get("settings.rules_open_site"), OnOpenRulesSite, true);
        }

        private static void BuildDiagnostics()
        {
            LocalizedUi.Header(Settings.AddHeader(LocalizationCore.Get("settings.diag_header")), "settings.diag_header");

            MoprSettings.ShowOverlay = Settings.AddCheckBox("mopr_overlay", LocalizationCore.Get("settings.overlay"), false);
            LocalizedUi.Label(MoprSettings.ShowOverlay, "settings.overlay");
            LocalizedUi.Text(Settings.AddText(LocalizationCore.Get("settings.overlay_hint")), "settings.overlay_hint");

            MoprSettings.ShowLogMessages = Settings.AddCheckBox("mopr_show_log", LocalizationCore.Get("settings.show_log"), false);
            LocalizedUi.Label(MoprSettings.ShowLogMessages, "settings.show_log");
            LocalizedUi.Text(Settings.AddText(LocalizationCore.Get("settings.show_log_hint")), "settings.show_log_hint");

            LocalizedUi.Text(Settings.AddText(LocalizationCore.Get("settings.emergency_header")), "settings.emergency_header");
            MoprSettings.DisableOptimization = Settings.AddCheckBox("mopr_disable_opt", LocalizationCore.Get("settings.disable_opt"), false, OnDisableOptimizationChanged);
            LocalizedUi.Label(MoprSettings.DisableOptimization, "settings.disable_opt");
            LocalizedUi.Text(Settings.AddText(LocalizationCore.Get("settings.disable_opt_hint")), "settings.disable_opt_hint");
        }

        private static void BuildInfo(Mod mod)
        {
            LocalizedUi.Header(Settings.AddHeader(LocalizationCore.Get("settings.info_header")), "settings.info_header");
            SettingsText info = Settings.AddText(LocalizationCore.Get("settings.info", mod.Version, mod.Author));
            LocalizedUi.TextArgs(info, "settings.info", () => new object[] { mod.Version, mod.Author });
        }

        #endregion

        #region Колбэки

        private static void OnLanguageChanged()
        {
            int index = MoprSettings.Language != null ? MoprSettings.Language.GetSelectedItemIndex() : 0;
            LocalizationCore.SetCurrent(index == (int)Language.Russian ? Language.Russian : Language.English);
            LocalizationConfig.Save();
            LocalizedUi.RefreshAll();
        }

        /// <summary>Выбор профиля двигает ползунок дистанции.</summary>
        private static void OnModeChanged()
        {
            if (!settingsInteractive || MoprSettings.ActiveDistance == null)
                return;

            SettingsReflection.MoveSlider(MoprSettings.ActiveDistance, MoprSettings.ModeSliderPreset);
        }

        private static void OnBackupNow()
        {
            bool ok = SaveProtection.Backup();
            ModUI.ShowMessage(
                LocalizationCore.Get(ok ? "save.backup_done" : "save.backup_failed"),
                LocalizationCore.Get("save.verify_title"));
        }

        private static void OnRestoreLatest()
        {
            if (SaveProtection.ListBackups().Count == 0)
            {
                ModUI.ShowMessage(LocalizationCore.Get("save.no_backups"), LocalizationCore.Get("save.verify_title"));
                return;
            }

            bool ok = SaveProtection.RestoreLatest();
            ModUI.ShowMessage(
                LocalizationCore.Get(ok ? "save.restored_msg" : "save.restore_failed"),
                LocalizationCore.Get("save.verify_title"));
        }

        private static void OnRefreshRules()
        {
            RuleSyncRunner.Launch(true);
            LocalizedUi.RefreshAll();
        }

        private static void OnOpenRulesSite()
        {
            try
            {
                System.Diagnostics.Process.Start(RemoteRuleSync.SiteUrl);
            }
            catch
            {
            }
        }

        /// <summary>Аварийный тумблер: мгновенно включает/выключает оптимизацию, если игра загружена.</summary>
        private static void OnDisableOptimizationChanged()
        {
            if (Core.Instance != null)
                Core.Instance.ApplyOptimizationSetting();
        }

        private static void ApplyGraphics()
        {
            MoprSettings.UpdateShadows();
            MoprSettings.UpdateFramerateLimiter();
            MoprSettings.ToggleBackgroundRunning();
        }

        private static string[] BuildDistanceLabels()
        {
            return new[]
            {
                LocalizationCore.Get("settings.dist.0"),
                LocalizationCore.Get("settings.dist.1"),
                LocalizationCore.Get("settings.dist.2"),
                LocalizationCore.Get("settings.dist.3"),
                LocalizationCore.Get("settings.dist.4"),
            };
        }

        #endregion
    }
}
