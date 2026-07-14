// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Английский языковой пакет: ВСЕ строки мода (UI + логи/консоль/монитор) в одной таблице. Английский —
// эталон: он содержит полный набор ключей, остальные языки при отсутствии ключа откатываются на него
// (см. LocalizationCore.Resolve). Диспетчер языков — LocTextCore.

using System.Collections.Generic;

namespace MOPR.Localization
{
    internal static class LocEN
    {
        public static readonly Dictionary<string, string> Strings = new Dictionary<string, string>
        {
            { "mod.description", "The ultimate My Summer Car optimization project, rebuilt for 2026." },

            // === Settings: Language ===
            { "settings.language", "Language" },

            // === Settings: Performance / Optimization ===
            { "settings.perf_header", "Performance" },
            { "settings.despawn_header", "Optimization" },
            { "settings.mode_header", "Performance preset" },
            { "settings.mode.ultra", "Ultra Quality" },
            { "settings.mode.quality", "Quality" },
            { "settings.mode.balanced", "Balanced" },
            { "settings.mode.performance", "Performance" },
            { "settings.subsystems_header", "Optimization modules" },
            { "settings.opt_basic", "Basic" },
            { "settings.opt_advanced", "Advanced" },
            { "settings.opt_graphics", "Graphical" },
            { "settings.optimize_items", "Optimize items" },
            { "settings.optimize_places", "Optimize locations" },
            { "settings.optimize_vehicles", "Optimize vehicles" },
            { "settings.optimize_satsuma", "Optimize the Satsuma (light)" },
            { "settings.satsuma_driving", "Optimize the Satsuma while driving" },
            { "settings.satsuma_driving_hint", "<color=#9AA6B2>While driving, hides unseen engine parts and cosmetics for extra FPS.</color>" },
            { "settings.other_header", "Other" },
            { "settings.active_distance", "Draw distance" },
            { "settings.desc", "<color=#9AA6B2>How far away objects stay loaded. Higher values keep them visible from further.</color>" },
            { "settings.dist.0", "Very Close (0.5x)" },
            { "settings.dist.1", "Close (0.75x)" },
            { "settings.dist.2", "Default (1x)" },
            { "settings.dist.3", "Far (2x)" },
            { "settings.dist.4", "Very Far (4x)" },

            // === Settings: Graphics ===
            { "settings.graphics_header", "Graphics" },
            { "settings.gfx_shadows", "Shadows" },
            { "settings.gfx_framerate", "Frame rate" },
            { "settings.adjust_shadows", "Adjust shadow distance" },
            { "settings.shadow_distance", "Shadow distance (m)" },
            { "settings.dynamic_draw", "Dynamic draw distance" },
            { "settings.dynamic_draw_hint", "<color=#9AA6B2>Automatically lowers draw distance indoors and near home.</color>" },
            { "settings.sector_cull", "Indoor scene culling" },
            { "settings.sector_cull_hint", "<color=#9AA6B2>Hides unnecessary outdoor scenery while you're indoors.</color>" },
            { "settings.engine_patches", "Optimize game engine code" },
            { "settings.engine_patches_hint", "<color=#9AA6B2>Speeds up the game's internal code. Applies on the next load.</color>" },
            { "settings.sleep_bodies", "Physics optimization" },
            { "settings.sleep_bodies_hint", "<color=#9AA6B2>Puts distant, motionless objects to sleep to ease the physics load.</color>" },
            { "settings.lod", "Distant models (LOD)" },
            { "settings.lod_hint", "<color=#9AA6B2>Shows cheap stand-in models for far objects.</color>" },
            { "settings.disable_skidmarks", "Hide tire skidmarks" },
            { "settings.disable_skidmarks_hint", "<color=#9AA6B2>Removes tire marks that can leak memory over time.</color>" },

            // === Settings: Game ===
            { "settings.game_header", "Game" },
            { "settings.limit_fps", "Limit FPS" },
            { "settings.fps_limit", "FPS limit" },
            { "settings.run_background", "Run in background" },
            { "settings.adaptive", "Adaptive load balancing" },
            { "settings.adaptive_hint", "<color=#9AA6B2>Automatically tunes how much work MOP does per frame to keep the game smooth.</color>" },
            { "settings.gc_adaptive", "Adaptive garbage cleanup" },
            { "settings.gc_adaptive_hint", "<color=#9AA6B2>Periodically frees memory. May cause brief hitches.</color>" },

            // === Settings: Diagnostics ===
            { "settings.diag_header", "Debug" },
            { "settings.overlay", "Debug monitor" },
            { "settings.overlay_hint", "<color=#9AA6B2>On-screen diagnostics panel. Arrow keys switch pages.</color>" },
            { "settings.show_log", "Detailed log messages" },
            { "settings.show_log_hint", "<color=#9AA6B2>Prints MOPR's detailed logs to the game console.</color>" },
            { "settings.emergency_header", "Emergency mode" },
            { "settings.disable_opt", "Disable optimization" },
            { "settings.disable_opt_hint", "<color=#9AA6B2>Fully turns off optimization and re-enables everything.</color>" },

            // === Monitor (on-screen debug panel) ===
            { "monitor.title.perf", "Performance" },
            { "monitor.title.world", "World" },
            { "monitor.title.system", "System" },

            // === Settings: Fixes ===
            { "settings.fixes_header", "Fixes" },
            { "settings.fix_lake_weed", "Fix lake weed" },
            { "settings.fix_lake_weed_hint", "<color=#9AA6B2>Hides the shallow-water weed that flickers at certain angles.</color>" },
            { "settings.parking_brake_anchor", "Fix the Satsuma handbrake" },
            { "settings.parking_brake_anchor_hint", "<color=#9AA6B2>Stops the Satsuma from sliding downhill with the handbrake on.</color>" },
            { "settings.disable_empty_items", "Disable empty bottles" },
            { "settings.disable_empty_items_hint", "<color=#9AA6B2>Deactivates empty bottles/containers to save resources.</color>" },
            { "settings.destroy_empty_bottles", "Destroy empty bottles" },
            { "settings.destroy_empty_bottles_hint", "<color=#9AA6B2>Fully removes empty bottles left after drinking so they don't pile up.</color>" },

            // === Settings: Information ===
            { "settings.info_header", "Information" },
            { "settings.info", "<b><color=#FF7A1A>MOP - REVIVAL</color></b> <color=#35E0E0>v{0}</color>\nby <color=#F5A623>{1}</color>\n<color=#9AA6B2>Based on the original MOP by Athlon - GPLv3</color>" },

            // === Save integrity ===
            { "issue.bucket_seats", "Bucket seats: only one of the pair is marked purchased" },
            { "issue.flatbed", "Flatbed trailer marked attached but parked away from the tractor" },
            { "issue.fuel_line", "Fuel line tightness ({0}) doesn't match its bolt ({1})" },
            { "issue.item_count", "Item count '{0}': saved {1}, but {2} present" },
            { "issue.bolt_tightness", "{0}: bolt tightness drifted (now {1}, restore {2})" },
            { "bolt.bumper_front", "Front bumper" },
            { "bolt.bumper_rear", "Rear bumper" },
            { "bolt.halfshaft_fl", "Front-left halfshaft" },
            { "bolt.halfshaft_fr", "Front-right halfshaft" },
            { "bolt.battery_minus", "Battery minus terminal" },
            { "save.verify_title", "MOP Revival — Save Integrity" },
            { "save.verify_found_msg", "MOP found {0} problem(s) with your save:\n\n{1}\nThese are known game/save bugs (for example, bolt tightness resetting itself). MOP can restore the last known-good values.\n\nFix them now? A backup is made first; reload the save afterwards to apply." },
            { "save.verify_more", "...and {0} more" },
            { "save.verify_result", "Fixed: {0}. Failed: {1}.\n\nA backup was saved in MOP_Backups.\nReload your save to apply the fixes." },
            { "save.corrupt_msg", "Your current save looks damaged (now {0} bytes vs {1} bytes in the last backup).\n\nRestore the most recent backup now?" },
            { "save.restored_msg", "The latest backup was restored. You can continue your game now." },
            { "save.restore_failed", "Could not restore a backup automatically. Check the MOP_Backups folder manually." },
            { "save.backup_done", "Backup created in MOP_Backups." },
            { "save.backup_failed", "Backup failed. See the log for details." },
            { "save.no_backups", "No backups available yet." },

            // === Settings: Save protection ===
            { "settings.saves_header", "Save protection" },
            { "settings.save_protect", "Protect save files" },
            { "settings.save_protect_hint", "<color=#9AA6B2>Clears the read-only lock that can block saving.</color>" },
            { "settings.save_backup", "Auto-backup saves" },
            { "settings.save_backup_hint", "<color=#9AA6B2>Automatically creates and keeps save backups.</color>" },
            { "settings.save_verify", "Verify save integrity" },
            { "settings.save_verify_hint", "<color=#9AA6B2>Checks the save for known bugs and offers to fix them.</color>" },
            { "settings.restore_bolts", "Restore bolt tightness" },
            { "settings.restore_bolts_hint", "<color=#9AA6B2>Restores bolt tightness that the game resets by itself.</color>" },
            { "settings.backup_now", "Back up save now" },
            { "settings.restore_latest", "Restore latest backup" },

            // === Settings: Rules server ===
            { "settings.rules_header", "Rules server" },
            { "settings.rules_refresh", "Check for rule updates" },
            { "settings.rules_open_site", "Open the rules website" },
            { "settings.server.checking", "<color=#F5C518>Rules server: checking…</color>" },
            { "settings.server.available", "<color=#3FB950>Rules server: available</color>" },
            { "settings.server.offline", "<color=#FF5151>Rules server: unavailable</color>" },

            // === Rules: download prompt ===
            { "rules.prompt_title", "MOP Revival — Rule Updates" },
            { "rules.prompt_msg", "MOP found {0} new/updated rule file(s) for your installed mods:\n\n{1}\nRule files improve compatibility and optimization for specific mods. Download them now?" },
            { "rules.download_btn", "Download" },
            { "rules.later_btn", "Later" },
            { "rules.later_msg", "No problem. You can download them anytime from the mod settings: \"Rules server\" → \"Check for rule updates\"." },
            { "rules.downloaded_msg", "Downloaded {0} rule file(s):\n\n{1}\nReload the game for them to take effect." },

            // === Compatibility warning ===
            { "compat.title", "MOP Revival — Incompatible mod detected" },
            { "compat.msg", "MOP Revival detected an incompatible mod:\n\n{0}\n\nIt does the same job as MOP Revival, so running both causes conflicts, crashes and broken optimization. Please disable it in your mod loader and restart the game.\n\nMOP Revival stays inactive while it is installed." },
            { "compat.msg_plural", "MOP Revival detected incompatible mods:\n\n{0}\n\nThey do the same job as MOP Revival, so running them together causes conflicts, crashes and broken optimization. Please disable them in your mod loader and restart the game.\n\nMOP Revival stays inactive while they are installed." },

            // === Common ===
            { "common.yes", "Yes" },
            { "common.no", "No" },
            { "common.ok", "OK" },

            // === Load screen ===
            { "load.optimizing", "Optimizing the world" },
            { "load.final_touches", "Final touches" },
            { "load.easter", "Have a nice day :)" },

            // ================================================================================
            // Логи / консоль (команда mopr) / монитор диагностики — не видно в обычном UI мода.
            // ================================================================================

            // === Logs ===
            { "log.menu_init", "{0} loaded successfully!" },
            { "log.mod_initialized", "Successfully initialized! Version {0}" },
            { "log.disabled", "Optimization disabled in settings; all managed objects re-enabled." },
            { "log.lang_changed", "Language set to English." },
            { "log.catalog_summary", "Catalog: {0} object(s) registered ({1} log wall(s))." },
            { "log.catalog_missing", "Not found in this game version ({0}): {1}" },
            { "log.mode_reapplied", "Mode changed; now managing {0} object(s)." },
            { "log.no_player", "PLAYER object not found; distance optimization is paused." },
            { "log.failsafe", "Repeated errors detected; optimization paused (failsafe). Toggle the mod off/on or restart the game to resume." },
            { "log.save_readonly_cleared", "Cleared the read-only lock on {0} save file(s)." },
            { "log.save_backup_ok", "Save backed up ({0} file(s)) -> {1}" },
            { "log.save_restored", "Save restored from backup {0}." },
            { "log.verify_start", "Verifying save integrity..." },
            { "log.verify_skipped", "Save verification skipped (post-permadeath save)." },
            { "log.verify_clean", "Save integrity OK - no problems found." },
            { "log.verify_found", "Save integrity: {0} problem(s) found." },
            { "log.verify_done", "Save fixes applied: {0} fixed, {1} failed." },
            { "log.bolt_snapshot_saved", "Bolt snapshot captured ({0} part(s))." },
            { "log.save_corrupt", "Save looks corrupt: {0} bytes now vs {1} bytes in the last backup." },
            { "log.continue_restored", "Re-enabled the main menu Continue button." },
            { "log.rules_sync_start", "Checking the rules server availability..." },
            { "log.rules_sync_ok", "Rules server - {0}\nAvailable - {1}; Active - {2}" },
            { "log.rules_sync_offline", "Rules server - {0}" },
            { "log.rules_sync_error", "Rules server error: {0}" },
            { "log.server_status_available", "AVAILABLE" },
            { "log.server_status_unavailable", "UNAVAILABLE" },
            { "log.rules_downloaded", "Downloaded {0} rule file(s) from the server." },

            // === Performance overlay / report ===
            { "perf.fps", "FPS: {0}  (avg {1} ms, worst {2} ms)" },
            { "perf.spikes", "Spikes: {0}  (GC-related: {1})" },
            { "perf.objects", "Objects: {0} managed, {1} active" },

            // === Status (always shown on load) ===
            { "status.core_init", "Core initialized" },
            { "status.managed", "Managed objects: {0}" },
            { "status.core_loaded", "Core loaded" },
            { "status.module_loaded", "{0} module - Loaded ({1})" },
            { "status.module_off", "{0} module - Disabled" },
            { "status.mod.world", "World objects" },
            { "status.mod.items", "Items" },
            { "status.mod.vehicles", "Vehicles" },
            { "status.mod.places", "Locations" },
            { "status.feature", "{0} - {1}" },
            { "status.word.enabled", "Enabled" },
            { "status.word.disabled", "Disabled" },
            { "status.word.applied", "Applied" },
            { "status.feat.fixes", "Game fixes" },
            { "status.feat.engine", "Engine patches" },
            { "status.feat.sector", "Indoor culling" },
            { "status.feat.ddd", "Dynamic draw distance" },
            { "status.feat.sleep", "Sleep distant bodies" },
            { "status.feat.gc", "Adaptive GC" },
            { "status.feat.saves", "Save protection" },
            { "status.ready", "Ready to go!" },

            // === Console command (mopr) ===
            { "cmd.help", "MOP Revival diagnostics. Usage: mopr help | version | status | monitor | presave | stop | start | config | logs | report" },
            { "cmd.not_running", "Core is not running. Load a save first." },
            { "cmd.not_ready", "Core is not ready yet." },
            { "cmd.status", "enabled={0}, distance={1}x, managed={2}, active={3}." },
            { "cmd.unknown", "Unknown subcommand. Type \"mopr help\" for the list." },

            // === Object states ===
            { "state.active", "active" },
            { "state.disabled", "disabled" },
            { "state.destroyed", "destroyed" },
        };
    }
}
