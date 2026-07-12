// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Все строки, видимые игроку, КРОМЕ вывода в лог/консоль (те — в LocTextLog): меню настроек,
// внутриигровые диалоги, запросы сохранения, загрузочный экран. Оба словаря держат ОДНИ И ТЕ ЖЕ
// ключи — при добавлении строки правьте обе таблицы.

using System.Collections.Generic;

namespace MOPR.Localization
{
    internal static class LocTextCore
    {
        /// <summary>Таблица строк для запрошенного языка.</summary>
        public static Dictionary<string, string> For(Language language)
        {
            return language == Language.Russian ? Russian : English;
        }

        private static readonly Dictionary<string, string> English = new Dictionary<string, string>
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
        };

        private static readonly Dictionary<string, string> Russian = new Dictionary<string, string>
        {
            { "mod.description", "Лучший проект оптимизации My Summer Car в 2026 году." },

            // === Settings: Language ===
            { "settings.language", "Язык" },

            // === Settings: Performance / Optimization ===
            { "settings.perf_header", "Быстродействие" },
            { "settings.despawn_header", "Оптимизация" },
            { "settings.mode_header", "Пресет производительности" },
            { "settings.mode.ultra", "Ультра-качество" },
            { "settings.mode.quality", "Качество" },
            { "settings.mode.balanced", "Баланс" },
            { "settings.mode.performance", "Производительность" },
            { "settings.subsystems_header", "Модули оптимизации" },
            { "settings.opt_basic", "Базовые" },
            { "settings.opt_advanced", "Продвинутые" },
            { "settings.opt_graphics", "Графические" },
            { "settings.optimize_items", "Оптимизация предметов" },
            { "settings.optimize_places", "Оптимизация локаций" },
            { "settings.optimize_vehicles", "Оптимизация транспорта" },
            { "settings.optimize_satsuma", "Оптимизация Satsuma (облегчённая)" },
            { "settings.satsuma_driving", "Оптимизация Satsuma в режиме вождения" },
            { "settings.satsuma_driving_hint", "<color=#9AA6B2>За рулём скрывает невидимые детали двигателя и косметику ради FPS.</color>" },
            { "settings.other_header", "Другое" },
            { "settings.active_distance", "Дистанция прорисовки" },
            { "settings.desc", "<color=#9AA6B2>На каком расстоянии вокруг вас объекты остаются загруженными. Больше значение — дальше видно объекты.</color>" },
            { "settings.dist.0", "Очень близко (0.5x)" },
            { "settings.dist.1", "Близко (0.75x)" },
            { "settings.dist.2", "По умолчанию (1x)" },
            { "settings.dist.3", "Далеко (2x)" },
            { "settings.dist.4", "Очень далеко (4x)" },

            // === Settings: Graphics ===
            { "settings.graphics_header", "Графика" },
            { "settings.gfx_shadows", "Тени" },
            { "settings.gfx_framerate", "Частота кадров" },
            { "settings.adjust_shadows", "Настроить дальность теней" },
            { "settings.shadow_distance", "Дальность теней (м)" },
            { "settings.dynamic_draw", "Динамическая дальность прорисовки" },
            { "settings.dynamic_draw_hint", "<color=#9AA6B2>Автоматически снижает дальность прорисовки в помещениях и у дома.</color>" },
            { "settings.sector_cull", "Отсечение сцены в помещениях" },
            { "settings.sector_cull_hint", "<color=#9AA6B2>В помещениях скрывает ненужный внешний пейзаж.</color>" },
            { "settings.engine_patches", "Оптимизация кода движка игры" },
            { "settings.engine_patches_hint", "<color=#9AA6B2>Ускоряет внутренний код игры. Действует со следующей загрузки.</color>" },
            { "settings.sleep_bodies", "Оптимизация физики" },
            { "settings.sleep_bodies_hint", "<color=#9AA6B2>Усыпляет дальние неподвижные объекты, снижая нагрузку на физику.</color>" },
            { "settings.lod", "Дальние модели (LOD)" },
            { "settings.lod_hint", "<color=#9AA6B2>Показывает упрощённые модели дальних объектов.</color>" },
            { "settings.disable_skidmarks", "Скрывать следы от шин" },
            { "settings.disable_skidmarks_hint", "<color=#9AA6B2>Убирает следы шин, которые со временем расходуют память.</color>" },

            // === Settings: Game ===
            { "settings.game_header", "Игра" },
            { "settings.limit_fps", "Ограничение FPS" },
            { "settings.fps_limit", "Лимит FPS" },
            { "settings.run_background", "Работать в фоне" },
            { "settings.adaptive", "Адаптивное распределение нагрузки" },
            { "settings.adaptive_hint", "<color=#9AA6B2>Автоматически подстраивает объём работы MOP за кадр ради плавности.</color>" },
            { "settings.gc_adaptive", "Адаптивная чистка мусора" },
            { "settings.gc_adaptive_hint", "<color=#9AA6B2>Периодически освобождает память. Возможны короткие подлагивания.</color>" },

            // === Settings: Diagnostics ===
            { "settings.diag_header", "Отладка" },
            { "settings.overlay", "Монитор отладки" },
            { "settings.overlay_hint", "<color=#9AA6B2>Панель диагностики на экране. Страницы — стрелками.</color>" },
            { "settings.show_log", "Подробные лог-сообщения" },
            { "settings.show_log_hint", "<color=#9AA6B2>Выводит подробные логи MOPR в консоль игры.</color>" },
            { "settings.emergency_header", "Аварийный режим" },
            { "settings.disable_opt", "Отключить оптимизацию" },
            { "settings.disable_opt_hint", "<color=#9AA6B2>Полностью выключает оптимизацию и возвращает всё обратно.</color>" },

            // === Monitor (on-screen debug panel) ===
            { "monitor.title.perf", "Производительность" },
            { "monitor.title.world", "Мир" },
            { "monitor.title.system", "Система" },

            // === Settings: Fixes ===
            { "settings.fixes_header", "Исправления" },
            { "settings.fix_lake_weed", "Исправить водную ряску" },
            { "settings.fix_lake_weed_hint", "<color=#9AA6B2>Скрывает ряску на мелководье озера, которая мерцает под определённым углом.</color>" },
            { "settings.parking_brake_anchor", "Исправить ручник Satsuma" },
            { "settings.parking_brake_anchor_hint", "<color=#9AA6B2>Не позволяет Satsuma сползать с холма с поднятым ручником.</color>" },
            { "settings.disable_empty_items", "Отключать пустые бутылки" },
            { "settings.disable_empty_items_hint", "<color=#9AA6B2>Деактивирует пустые бутылки/тару ради экономии ресурсов.</color>" },
            { "settings.destroy_empty_bottles", "Уничтожать пустые бутылки" },
            { "settings.destroy_empty_bottles_hint", "<color=#9AA6B2>Полностью удаляет пустые бутылки, оставшиеся после питья, чтобы они не накапливались.</color>" },

            // === Settings: Information ===
            { "settings.info_header", "Информация" },
            { "settings.info", "<b><color=#FF7A1A>MOP - REVIVAL</color></b> <color=#35E0E0>v{0}</color>\nАвтор <color=#F5A623>{1}</color>\n<color=#9AA6B2>На основе оригинального MOP от Athlon - GPLv3</color>" },

            // === Save integrity ===
            { "issue.bucket_seats", "Ковшеобразные сиденья: куплено только одно из пары" },
            { "issue.flatbed", "Прицеп отмечен как присоединённый, но стоит далеко от трактора" },
            { "issue.fuel_line", "Затяжка топливной магистрали ({0}) не совпадает с болтом ({1})" },
            { "issue.item_count", "Счётчик предмета «{0}»: сохранено {1}, а присутствует {2}" },
            { "issue.bolt_tightness", "{0}: затяжка болта изменилась (сейчас {1}, восстановить {2})" },
            { "bolt.bumper_front", "Передний бампер" },
            { "bolt.bumper_rear", "Задний бампер" },
            { "bolt.halfshaft_fl", "Передний левый привод" },
            { "bolt.halfshaft_fr", "Передний правый привод" },
            { "bolt.battery_minus", "Минусовая клемма АКБ" },
            { "save.verify_title", "MOP Revival — Целостность сейва" },
            { "save.verify_found_msg", "MOP нашёл проблемы с вашим сохранением ({0}):\n\n{1}\nЭто известные баги игры/сейва (например, самопроизвольный сброс затяжки болтов). MOP может вернуть последние корректные значения.\n\nИсправить сейчас? Сначала делается бэкап; после — перезагрузите сейв, чтобы применить." },
            { "save.verify_more", "...и ещё {0}" },
            { "save.verify_result", "Исправлено: {0}. Не удалось: {1}.\n\nБэкап сохранён в MOP_Backups.\nПерезагрузите сейв, чтобы применить исправления." },
            { "save.corrupt_msg", "Ваше текущее сохранение выглядит повреждённым (сейчас {0} байт против {1} байт в последнем бэкапе).\n\nВосстановить самый свежий бэкап сейчас?" },
            { "save.restored_msg", "Последний бэкап восстановлен. Теперь можно продолжить игру." },
            { "save.restore_failed", "Не удалось восстановить бэкап автоматически. Проверьте папку MOP_Backups вручную." },
            { "save.backup_done", "Бэкап создан в MOP_Backups." },
            { "save.backup_failed", "Не удалось сделать бэкап. Подробности в логе." },
            { "save.no_backups", "Бэкапов пока нет." },

            // === Settings: Save protection ===
            { "settings.saves_header", "Защита сохранений" },
            { "settings.save_protect", "Защита файлов сохранения" },
            { "settings.save_protect_hint", "<color=#9AA6B2>Снимает блокировку «только чтение», мешающую сохранению.</color>" },
            { "settings.save_backup", "Автобэкап сохранений" },
            { "settings.save_backup_hint", "<color=#9AA6B2>Автоматически создаёт и хранит бэкапы сохранений.</color>" },
            { "settings.save_verify", "Проверка целостности сейва" },
            { "settings.save_verify_hint", "<color=#9AA6B2>Ищет известные баги сейва и предлагает их исправить.</color>" },
            { "settings.restore_bolts", "Восстановление затяжки болтов" },
            { "settings.restore_bolts_hint", "<color=#9AA6B2>Возвращает затяжку болтов, которую игра сбрасывает сама.</color>" },
            { "settings.backup_now", "Сделать бэкап сейчас" },
            { "settings.restore_latest", "Восстановить последний бэкап" },

            // === Settings: Rules server ===
            { "settings.rules_header", "Сервер правил" },
            { "settings.rules_refresh", "Проверить обновления правил" },
            { "settings.rules_open_site", "Открыть сайт правил" },
            { "settings.server.checking", "<color=#F5C518>Сервер правил: проверяем…</color>" },
            { "settings.server.available", "<color=#3FB950>Сервер правил: доступен</color>" },
            { "settings.server.offline", "<color=#FF5151>Сервер правил: недоступен</color>" },

            // === Rules: download prompt ===
            { "rules.prompt_title", "MOP Revival — Обновления правил" },
            { "rules.prompt_msg", "MOP нашёл новые/обновлённые файлы правил ({0}) для ваших установленных модов:\n\n{1}\nФайлы правил улучшают совместимость и оптимизацию конкретных модов. Скачать сейчас?" },
            { "rules.download_btn", "Скачать" },
            { "rules.later_btn", "Позже" },
            { "rules.later_msg", "Хорошо. Скачать их можно в любой момент из настроек мода: «Сервер правил» → «Проверить обновления правил»." },
            { "rules.downloaded_msg", "Скачано файлов правил: {0}\n\n{1}\nПерезапустите игру, чтобы они вступили в силу." },

            // === Предупреждение о совместимости ===
            { "compat.title", "MOP Revival — Обнаружен несовместимый мод" },
            { "compat.msg", "MOP Revival обнаружил несовместимый мод:\n\n{0}\n\nОн выполняет ту же работу, что и MOP Revival, поэтому вместе они вызывают конфликты, вылеты и сломанную оптимизацию. Отключите его в загрузчике модов и перезапустите игру.\n\nПока он установлен, MOP Revival остаётся неактивным." },
            { "compat.msg_plural", "MOP Revival обнаружил несовместимые моды:\n\n{0}\n\nОни выполняют ту же работу, что и MOP Revival, поэтому вместе они вызывают конфликты, вылеты и сломанную оптимизацию. Отключите их в загрузчике модов и перезапустите игру.\n\nПока они установлены, MOP Revival остаётся неактивным." },

            // === Common ===
            { "common.yes", "Да" },
            { "common.no", "Нет" },
            { "common.ok", "OK" },

            // === Load screen ===
            { "load.optimizing", "Оптимизация мира" },
            { "load.final_touches", "Последние штрихи" },
            { "load.easter", "Хорошего дня :)" },
        };
    }
}
