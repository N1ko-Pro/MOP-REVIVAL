// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Польский языковой пакет: ВСЕ строки мода в одной таблице. UI переведён на польский; строки логов/
// консоли/монитора пока НЕ переведены и держатся английскими плейсхолдерами (это сохраняет прежнее
// поведение: раньше польские логи откатывались на английский). Переводчику: раздел «Logs / console /
// monitor» ниже — на перевод. Отсутствующие ключи всё равно откатятся на английский (LocalizationCore).

using System.Collections.Generic;

namespace MOPR.Localization
{
    internal static class LocPL
    {
        public static readonly Dictionary<string, string> Strings = new Dictionary<string, string>
        {
            { "mod.description", "Najlepszy projekt optymalizacji My Summer Car, odbudowany na rok 2026." },

            // === Settings: Language ===
            { "settings.language", "Język" },

            // === Settings: Performance / Optimization ===
            { "settings.perf_header", "Wydajność" },
            { "settings.despawn_header", "Optymalizacja" },
            { "settings.mode_header", "Ustawienia wydajności" },
            { "settings.mode.ultra", "Bardzo wysoka" },
            { "settings.mode.quality", "Wysoka" },
            { "settings.mode.balanced", "Zrównoważona" },
            { "settings.mode.performance", "Wydajność" },
            { "settings.subsystems_header", "Moduły optymalizacji" },
            { "settings.opt_basic", "Podstawowa" },
            { "settings.opt_advanced", "Zaawansowana" },
            { "settings.opt_graphics", "Graficzna" },
            { "settings.optimize_items", "Optymalizuj przedmioty" },
            { "settings.optimize_places", "Optymalizuj lokacje" },
            { "settings.optimize_vehicles", "Optymalizuj pojazdy" },
            { "settings.optimize_satsuma", "Optymalizuj Satsumę (lekko)" },
            { "settings.satsuma_driving", "Optymalizuj Satsumę podczas jazdy" },
            { "settings.satsuma_driving_hint", "<color=#9AA6B2>Ukrywa niewidoczne części silnika i elementy ozdobne dla zwiększenia liczby klatek.</color>" },
            { "settings.other_header", "Inne" },
            { "settings.active_distance", "Odległość rysowania" },
            { "settings.desc", "<color=#9AA6B2>Określa, jak daleko obiekty pozostają załadowane. Wyższe wartości zwiększają odległość widoczności.</color>" },
            { "settings.dist.0", "Bardzo bliska (0.5x)" },
            { "settings.dist.1", "Bliska (0.75x)" },
            { "settings.dist.2", "Domyślna (1x)" },
            { "settings.dist.3", "Daleka (2x)" },
            { "settings.dist.4", "Bardzo daleka (4x)" },

            // === Settings: Graphics ===
            { "settings.graphics_header", "Grafika" },
            { "settings.gfx_shadows", "Cienie" },
            { "settings.gfx_framerate", "Liczba klatek" },
            { "settings.adjust_shadows", "Dostosuj odległość cieni" },
            { "settings.shadow_distance", "Odległość cieni (m)" },
            { "settings.dynamic_draw", "Dynamiczna odległość rysowania" },
            { "settings.dynamic_draw_hint", "<color=#9AA6B2>Automatycznie obniża odległość rysowania w środku budynków i blisko domu.</color>" },
            { "settings.sector_cull", "Ukrywanie krajobrazu" },
            { "settings.sector_cull_hint", "<color=#9AA6B2>Chowa zbędny krajobraz, gdy jesteś w środku.</color>" },
            { "settings.water_reflections", "Optymalizuj odbicia wody" },
            { "settings.water_reflections_hint", "<color=#9AA6B2>Usuwa kamerę odbić jeziora. Woda staje się płaska.</color>" },
            { "settings.engine_patches", "Optymalizuj kod silnika gry" },
            { "settings.engine_patches_hint", "<color=#9AA6B2>Przyspiesza wewnętrzny kod gry. Obowiązuje przy kolejnym wczytaniu.</color>" },
            { "settings.sleep_bodies", "Optymalizacja fizyki" },
            { "settings.sleep_bodies_hint", "<color=#9AA6B2>Wyłącza dalekie, nieruchome obiekty aby zmniejszyć obciążenie.</color>" },
            { "settings.lod", "Odległe detale (LOD)" },
            { "settings.lod_hint", "<color=#9AA6B2>Wyświetla uproszczone modele dla odległych obiektów.</color>" },
            { "settings.disable_skidmarks", "Ukryj ślady opon" },
            { "settings.disable_skidmarks_hint", "<color=#9AA6B2>Usuwa ślady opon, które z czasem mogą prowadzić do wycieków pamięci.</color>" },

            // === Settings: Game ===
            { "settings.game_header", "Gra" },
            { "settings.limit_fps", "Ograniczenie FPS" },
            { "settings.fps_limit", "Limit FPS" },
            { "settings.run_background", "Działaj w tle" },
            { "settings.adaptive", "Adaptacyjne równoważenie obciążenia" },
            { "settings.adaptive_hint", "<color=#9AA6B2>Automatycznie reguluje jak dużo pracy wykonuje MOPR na klatkę aby zachować płynność gry.</color>" },
            { "settings.gc_adaptive", "Adaptacyjne usuwanie śmieci" },
            { "settings.gc_adaptive_hint", "<color=#9AA6B2>Okresowo zwalnia pamięć. Może powodować chwilowe przycięcia.</color>" },

            // === Settings: Diagnostics ===
            { "settings.diag_header", "Debugowanie" },
            { "settings.overlay", "Monitor debugowania" },
            { "settings.overlay_hint", "<color=#9AA6B2>Panel diagnostyczny dostępny na ekranie. Strzałki zmieniają strony.</color>" },
            { "settings.show_log", "Szczegółowe komunikaty dziennika" },
            { "settings.show_log_hint", "<color=#9AA6B2>Wypisuje szczegółowe logi MOPR do konsoli gry.</color>" },
            { "settings.emergency_header", "Tryb awaryjny" },
            { "settings.disable_opt", "Wyłącz optymalizacje" },
            { "settings.disable_opt_hint", "<color=#9AA6B2>Całkowicie wyłącza optymalizacje i ponownie wszystko włącza.</color>" },

            // === Monitor (on-screen debug panel) ===
            { "monitor.title.perf", "Wydajność" },
            { "monitor.title.world", "Świat" },
            { "monitor.title.system", "System" },

            // === Settings: Fixes ===
            { "settings.fixes_header", "Poprawki" },
            { "settings.fix_lake_weed", "Napraw roślinność wodną" },
            { "settings.fix_lake_weed_hint", "<color=#9AA6B2>Naprawia roślinność wodną na płytkiej wodzie migoczącą pod pewnymi kątami.</color>" },
            { "settings.parking_brake_anchor", "Napraw hamulec ręczny w Satsumie" },
            { "settings.parking_brake_anchor_hint", "<color=#9AA6B2>Zapobiega staczaniu się Satsumy ze wzniesienia przy zaciągniętym hamulcu ręcznym.</color>" },
            { "settings.disable_empty_items", "Wyłącz puste butelki i opakowania" },
            { "settings.disable_empty_items_hint", "<color=#9AA6B2>Wyłącza puste butelki i opakowania aby zaoszczędzić zasoby.</color>" },
            { "settings.destroy_empty_bottles", "Usuń puste butelki" },
            { "settings.destroy_empty_bottles_hint", "<color=#9AA6B2>Całkowicie usuwa puste butelki pozostawione po piciu aby się nie gromadziły.</color>" },

            // === Settings: Information ===
            { "settings.info_header", "Informacje" },
            { "settings.info", "<b><color=#FF7A1A>MOP - REVIVAL</color></b> <color=#35E0E0>v{0}</color>\nstworzony przez <color=#F5A623>{1}</color>\n<color=#9AA6B2>Na podstawie oryginalnego MOPa autorstwa Athlon - GPLv3</color>" },

            // === Save integrity ===
            { "issue.bucket_seats", "Fotele kubełkowe: Tylko jeden z pary jest oznaczony jako zakupiony" },
            { "issue.flatbed", "Naczepa oznaczona jako podpięta ale zaparkowana z dala od traktora" },
            { "issue.fuel_line", "Szczelność przewodu paliwowego ({0}) nie pasuje do swojej śruby ({1})" },
            { "issue.item_count", "Ilość przedmiotów '{0}': zapisanych {1}, ale {2} jest obecnych" },
            { "issue.bolt_tightness", "{0}: Zmieniło się dokręcenie śrub (aktualnie {1}, przywróć {2})" },
            { "bolt.bumper_front", "Przedni zderzak" },
            { "bolt.bumper_rear", "Tylny zderzak" },
            { "bolt.halfshaft_fl", "Przednia lewa półoś" },
            { "bolt.halfshaft_fr", "Przednia prawa półoś" },
            { "bolt.battery_minus", "Ujemny biegun akumulatora" },
            { "save.verify_title", "MOP Revival — Integralność zapisu" },
            { "save.verify_found_msg", "MOPR odnalazł {0} problemów z twoim zapisem gry:\n\n{1}\nSą to znane błędy gry/zapisu (na przykład, dokręcenie śrub samoczynnie się resetuje). MOPR może przywrócić ostatnie znane i dobre wartości.\n\nPrzywrócić je teraz? Najpierw wykonywana jest kopia zapasowa; następnie wczytaj ponownie zapis gry aby zastosować." },
            { "save.verify_more", "...i {0} więcej" },
            { "save.verify_result", "Naprawiono: {0}. Nie naprawiono: {1}.\n\nKopia zapasowa została zapisana w MOP_Backups.\nZaładuj swój zapis gry aby zastosować poprawki." },
            { "save.corrupt_msg", "Twój aktualny zapis gry wygląda na uszkodzony (aktualnie {0} bitów vs {1} bitów w ostatniej kopii zapasowej).\n\nPrzywrócić najnowszą kopię zapasową?" },
            { "save.restored_msg", "Przywrócono najnowszą kopię zapasową. Możesz kontynuować swoją rozgrywkę." },
            { "save.restore_failed", "Nie udało się automatycznie przywrócić kopii zapasowej. Sprawdź folder MOP_Backups." },
            { "save.backup_done", "Kopia zapasowa stworzona w MOP_Backups." },
            { "save.backup_failed", "Tworzenie kopii zapasowej nie powiodło się. Szczegóły znajdują się w dzienniku." },
            { "save.no_backups", "Brak dostępnych kopii zapasowych." },

            // === Settings: Save protection ===
            { "settings.saves_header", "Ochrona zapisu" },
            { "settings.save_protect", "Chroń pliki zapisu" },
            { "settings.save_protect_hint", "<color=#9AA6B2>Usuwa blokadę trybu tylko do odczytu, która może uniemożliwiać zapisywanie.</color>" },
            { "settings.save_backup", "Automatycznie twórz kopie zapasowe zapisu gry" },
            { "settings.save_backup_hint", "<color=#9AA6B2>Automatycznie tworzy i zapisuje kopie zapasowe zapisu gry.</color>" },
            { "settings.save_verify", "Zweryfikuj integralność zapisu gry" },
            { "settings.save_verify_hint", "<color=#9AA6B2>Sprawdza zapis gry pod kątem znanych błędów i oferuje ich naprawę.</color>" },
            { "settings.restore_bolts", "Przywróć dokręcenie śrub" },
            { "settings.restore_bolts_hint", "<color=#9AA6B2>Przywraca dokręcenie śrub, które gra samoczynnie resetuje.</color>" },
            { "settings.backup_now", "Stwórz kopię zapasową zapisu gry" },
            { "settings.restore_latest", "Przywróć najnowszą kopię zapasową" },

            // === Settings: Rules server ===
            { "settings.rules_header", "Serwer reguł" },
            { "settings.rules_refresh", "Sprawdź aktualizację reguł" },
            { "settings.rules_open_site", "Otwórz stronę reguł" },
            { "settings.server.checking", "<color=#F5C518>Serwer reguł: sprawdzanie.</color>" },
            { "settings.server.available", "<color=#3FB950>Serwer reguł: dostępny</color>" },
            { "settings.server.offline", "<color=#FF5151>Serwer reguł: niedostępny</color>" },

            // === Rules: download prompt ===
            { "rules.prompt_title", "MOP Revival — Aktualizacje reguł" },
            { "rules.prompt_msg", "MOP znalazł {0} nowych reguł dla Twoich zainstalowanych modyfikacji:\n\n{1}\nPliki reguł poprawiają kompatybilność i optymalizację dla konkretnych modyfikacji. Pobrać je teraz?" },
            { "rules.download_btn", "Pobierz" },
            { "rules.later_btn", "Nie teraz" },
            { "rules.later_msg", "Nie ma problemu. Możesz je pobrać w dowolnym momencie z poziomu ustawień modyfikacji: \"Serwer reguł\" → \"Sprawdź aktualizację reguł\"." },
            { "rules.downloaded_msg", "Pobrano {0} reguł:\n\n{1}\nZaładuj ponownie grę, aby zostały zastosowane." },

            // === Compatibility warning ===
            { "compat.title", "MOP Revival — Wykryto niekompatybilną modyfikację" },
            { "compat.msg", "MOP Revival wykrył niekompatybilną modyfikację:\n\n{0}\n\nWykonuje tę samą pracę co MOP Revival, więc jednoczesne ich uruchomienie powoduje konflikty, awarie i problemy z optymalizacją. Proszę ją wyłączyć w swoim mod loaderze i zrestartować grę.\n\nMOP Revival pozostaje nieaktywny gdy jest zainstalowana." },
            { "compat.msg_plural", "MOP Revival wykrył niekompatybilne modyfikacje:\n\n{0}\n\nWykonują tę samą pracę co MOP Revival, więc jednoczesne ich uruchomienie powoduje konflikty, awarie i problemy z optymalizacją. Proszę wyłączyć je w swoim mod loaderze i zrestartować grę.\n\nMOP Revival pozostaje nieaktywny gdy są zainstalowane." },

            // === Common ===
            { "common.yes", "Tak" },
            { "common.no", "Nie" },
            { "common.ok", "OK" },

            // === Load screen ===
            { "load.optimizing", "Optymalizowanie świata" },
            { "load.final_touches", "Ostatnie szlify" },
            { "load.easter", "Miłego dnia :)" },

            // ================================================================================
            // Logs / console (команда mopr) / monitor — ЕЩЁ НЕ ПЕРЕВЕДЕНО НА ПОЛЬСКИЙ.
            // Пока держим английский текст как плейсхолдер (как и раньше — польские логи откатывались
            // на английский). TODO(pl): перевести строки ниже.
            // ================================================================================

            // === Logs ===
            { "log.menu_init", "{0} loaded successfully!" },
            { "log.mod_initialized", "Successfully initialized! Version {0}" },
            { "log.disabled", "Optimization disabled in settings; all managed objects re-enabled." },
            { "log.lang_changed", "Language set to Polish." },
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
