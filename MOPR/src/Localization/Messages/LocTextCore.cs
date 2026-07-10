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

            // === Settings: Optimization ===
            { "settings.despawn_header", "Optimization" },
            { "settings.mode_header", "<b><color=#35E0E0>PERFORMANCE PRESET</color></b>" },
            { "settings.mode.ultra", "Ultra Quality" },
            { "settings.mode.quality", "Quality" },
            { "settings.mode.balanced", "Balanced" },
            { "settings.mode.performance", "Performance" },
            { "settings.subsystems_header", "<b><color=#35E0E0>OPTIMIZATION MODULES</color></b>" },
            { "settings.optimize_items", "Item optimization" },
            { "settings.optimize_places", "Location optimization" },
            { "settings.optimize_vehicles", "Vehicle optimization" },
            { "settings.optimize_satsuma", "Optimize the Satsuma (light)" },
            { "settings.satsuma_driving", "Deep-optimize the Satsuma while driving" },
            { "settings.satsuma_driving_hint", "<color=#9AA6B2>While you're sitting in the Satsuma, hides engine internals, part-install triggers and external cosmetics you can't see from the seat. Restored when you get out. Engine simulation (wear/overheating) is untouched.</color>" },
            { "settings.other_header", "<b><color=#F5C518>OTHER</color></b>" },
            { "settings.active_distance", "Draw distance" },
            { "settings.desc", "<color=#9AA6B2>How far away objects stay loaded. Higher values keep them visible from further.</color>" },
            { "settings.dist.0", "Very Close (0.5x)" },
            { "settings.dist.1", "Close (0.75x)" },
            { "settings.dist.2", "Default (1x)" },
            { "settings.dist.3", "Far (2x)" },
            { "settings.dist.4", "Very Far (4x)" },

            // === Settings: Graphics ===
            { "settings.graphics_header", "Graphics" },
            { "settings.adjust_shadows", "Adjust shadow distance" },
            { "settings.shadow_distance", "Shadow distance (meters)" },
            { "settings.dynamic_draw", "Dynamic draw distance" },
            { "settings.dynamic_draw_hint", "<color=#9AA6B2>Dynamically adjusts draw distance indoors and near home.</color>" },
            { "settings.sector_cull", "Indoor scenery culling" },
            { "settings.sector_cull_hint", "<color=#9AA6B2>Turns off unnecessary scenery objects while indoors.</color>" },
            { "settings.engine_patches", "Engine code patches" },
            { "settings.engine_patches_hint", "<color=#9AA6B2>Low-level fixes to the game's own code: skips redundant transform updates, caches mouse-hover raycasts per layer, and fixes a slow FSM event loop. Automatically disabled if the Reharmonization mod is installed. Takes effect on the next load.</color>" },
            { "settings.sleep_bodies", "Sleep distant physics objects" },
            { "settings.sleep_bodies_hint", "<color=#9AA6B2>Periodically puts far-away, already-still physics objects to sleep. Disabled automatically if the MSWCOptimization mod is installed.</color>" },
            { "settings.lod", "Distant models (LOD)" },
            { "settings.lod_hint", "<color=#9AA6B2>Shows cheap stand-in models for far objects.</color>" },
            { "settings.disable_skidmarks", "Hide tire skidmarks" },
            { "settings.disable_skidmarks_hint", "<color=#9AA6B2>Removes tire marks, which can cause a memory leak over time.</color>" },

            // === Settings: Game ===
            { "settings.game_header", "Game" },
            { "settings.limit_fps", "Limit framerate" },
            { "settings.fps_limit", "Framerate limit (FPS)" },
            { "settings.run_background", "Run game in background" },
            { "settings.adaptive", "Adaptive load balancing" },
            { "settings.adaptive_hint", "<color=#9AA6B2>Automatically tunes how much work MOP does per frame to keep the game smooth.</color>" },
            { "settings.gc_adaptive", "Adaptive garbage cleanup" },
            { "settings.gc_adaptive_hint", "<color=#9AA6B2>Periodically frees memory. May cause brief hitches during cleanup.</color>" },

            // === Settings: Diagnostics ===
            { "settings.diag_header", "Debug" },
            { "settings.overlay", "Show debug monitor" },
            { "settings.overlay_hint", "<color=#9AA6B2>On-screen diagnostics panel (top-right).\nSwitch pages with the arrow keys.</color>" },
            { "settings.show_log", "Show log messages" },
            { "settings.show_log_hint", "<color=#9AA6B2>Prints MOPR's detailed log messages to the in-game console.</color>" },
            { "settings.emergency_header", "<b><color=#FF5151>EMERGENCY MODE</color></b>" },
            { "settings.disable_opt", "Disable optimization" },
            { "settings.disable_opt_hint", "<color=#9AA6B2>Emergency switch: turns MOP's optimization off entirely and re-enables everything.</color>" },

            // === Monitor (on-screen debug panel) ===
            { "monitor.title.perf", "Performance" },
            { "monitor.title.world", "World" },
            { "monitor.title.system", "System" },

            // === Settings: Fixes ===
            { "settings.fixes_header", "Fixes" },
            { "settings.fix_lake_weed", "Remove lake surface weed" },
            { "settings.fix_lake_weed_hint", "<color=#9AA6B2>Hides the lake's shallow-water vegetation, which vanilla renders as if floating above the water from a distance.</color>" },
            { "settings.disable_empty_items", "Disable empty items" },
            { "settings.disable_empty_items_hint", "<color=#9AA6B2>Deactivates items the game has marked as EMPTY (empty bags/cans) to save resources.</color>" },
            { "settings.destroy_empty_bottles", "Destroy empty bottles" },
            { "settings.destroy_empty_bottles_hint", "<color=#9AA6B2>Fully removes the empty bottles left after drinking so they don't pile up.</color>" },

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
            { "settings.save_protect_hint", "<color=#9AA6B2>Clears the read-only lock that sometimes stops the game from saving.</color>" },
            { "settings.save_backup", "Back up saves automatically" },
            { "settings.save_backup_hint", "<color=#9AA6B2>Keeps the last few save backups in MOP_Backups so you can roll back.</color>" },
            { "settings.save_verify", "Verify save integrity" },
            { "settings.save_verify_hint", "<color=#9AA6B2>Checks the loaded save for known game/save bugs and offers to fix them.</color>" },
            { "settings.restore_bolts", "Restore bolt tightness" },
            { "settings.restore_bolts_hint", "<color=#9AA6B2>Restores bolt tightness that MSC resets by itself between saves.</color>" },
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

            // === Settings: Optimization ===
            { "settings.despawn_header", "Оптимизация" },
            { "settings.mode_header", "<b><color=#35E0E0>ПРЕСЕТ ПРОИЗВОДИТЕЛЬНОСТИ</color></b>" },
            { "settings.mode.ultra", "Ультра-качество" },
            { "settings.mode.quality", "Качество" },
            { "settings.mode.balanced", "Баланс" },
            { "settings.mode.performance", "Производительность" },
            { "settings.subsystems_header", "<b><color=#35E0E0>МОДУЛИ ОПТИМИЗАЦИИ</color></b>" },
            { "settings.optimize_items", "Оптимизация предметов" },
            { "settings.optimize_places", "Оптимизация локаций" },
            { "settings.optimize_vehicles", "Оптимизация транспорта" },
            { "settings.optimize_satsuma", "Оптимизация Satsuma (облегчённая)" },
            { "settings.satsuma_driving", "Глубокая оптимизация Satsuma за рулём" },
            { "settings.satsuma_driving_hint", "<color=#9AA6B2>Пока вы сидите в Satsuma, скрывает внутренности двигателя, триггеры установки деталей и внешнюю косметику, которых не видно с сиденья. Возвращается при выходе. Симуляцию двигателя (износ/перегрев) не трогает.</color>" },
            { "settings.other_header", "<b><color=#F5C518>ДРУГОЕ</color></b>" },
            { "settings.active_distance", "Дистанция прорисовки" },
            { "settings.desc", "<color=#9AA6B2>На каком расстоянии вокруг вас объекты остаются загруженными. Больше значение — дальше видно объекты.</color>" },
            { "settings.dist.0", "Очень близко (0.5x)" },
            { "settings.dist.1", "Близко (0.75x)" },
            { "settings.dist.2", "По умолчанию (1x)" },
            { "settings.dist.3", "Далеко (2x)" },
            { "settings.dist.4", "Очень далеко (4x)" },

            // === Settings: Graphics ===
            { "settings.graphics_header", "Графика" },
            { "settings.adjust_shadows", "Настроить дистанцию теней" },
            { "settings.shadow_distance", "Дистанция теней (метры)" },
            { "settings.dynamic_draw", "Динамическая дальность прорисовки" },
            { "settings.dynamic_draw_hint", "<color=#9AA6B2>Динамически регулирует дальность прорисовки в помещениях и у дома.</color>" },
            { "settings.sector_cull", "Отсечение пейзажа в помещениях" },
            { "settings.sector_cull_hint", "<color=#9AA6B2>Отключает лишние объекты пейзажа при нахождении в помещении.</color>" },
            { "settings.engine_patches", "Патчи кода движка" },
            { "settings.engine_patches_hint", "<color=#9AA6B2>Низкоуровневые правки самого кода игры: пропускает лишние обновления трансформов, кэширует рейкасты наведения мыши по слоям и чинит медленный цикл событий FSM. Автоматически отключается, если установлен мод Reharmonization. Вступает в силу при следующей загрузке.</color>" },
            { "settings.sleep_bodies", "Усыплять дальние физ-объекты" },
            { "settings.sleep_bodies_hint", "<color=#9AA6B2>Периодически усыпляет дальние, уже неподвижные физические тела. Автоматически отключается, если установлен мод MSWCOptimization.</color>" },
            { "settings.lod", "Дальние модели (LOD)" },
            { "settings.lod_hint", "<color=#9AA6B2>Показывает упрощённые модели дальних объектов.</color>" },
            { "settings.disable_skidmarks", "Скрывать следы от шин" },
            { "settings.disable_skidmarks_hint", "<color=#9AA6B2>Убирает следы от шин, которые со временем могут вызывать утечку памяти.</color>" },

            // === Settings: Game ===
            { "settings.game_header", "Игра" },
            { "settings.limit_fps", "Ограничить частоту кадров" },
            { "settings.fps_limit", "Лимит кадров (FPS)" },
            { "settings.run_background", "Не ставить игру на паузу в фоне" },
            { "settings.adaptive", "Адаптивное распределение нагрузки" },
            { "settings.adaptive_hint", "<color=#9AA6B2>Автоматически подстраивает объём работы MOP за кадр ради плавности.</color>" },
            { "settings.gc_adaptive", "Адаптивная чистка мусора" },
            { "settings.gc_adaptive_hint", "<color=#9AA6B2>Периодически освобождает память. Может вызывать кратковременные лаги во время очистки.</color>" },

            // === Settings: Diagnostics ===
            { "settings.diag_header", "Отладка" },
            { "settings.overlay", "Показывать монитор отладки" },
            { "settings.overlay_hint", "<color=#9AA6B2>Панель диагностики на экране (справа сверху).\nСтраницы переключаются стрелками.</color>" },
            { "settings.show_log", "Показывать лог-сообщения" },
            { "settings.show_log_hint", "<color=#9AA6B2>Выводит подробные лог-сообщения MOPR в игровую консоль.</color>" },
            { "settings.emergency_header", "<b><color=#FF5151>АВАРИЙНЫЙ РЕЖИМ</color></b>" },
            { "settings.disable_opt", "Отключить оптимизацию" },
            { "settings.disable_opt_hint", "<color=#9AA6B2>Аварийный выключатель: полностью отключает оптимизацию MOPR и включает всё обратно.</color>" },

            // === Monitor (on-screen debug panel) ===
            { "monitor.title.perf", "Производительность" },
            { "monitor.title.world", "Мир" },
            { "monitor.title.system", "Система" },

            // === Settings: Fixes ===
            { "settings.fixes_header", "Исправления" },
            { "settings.fix_lake_weed", "Убрать водную ряску" },
            { "settings.fix_lake_weed_hint", "<color=#9AA6B2>Скрывает ряску на мелководье озера, которую ванильная игра издалека рисует будто «над» водой.</color>" },
            { "settings.disable_empty_items", "Отключать пустые предметы" },
            { "settings.disable_empty_items_hint", "<color=#9AA6B2>Деактивирует предметы, помеченные игрой как «ПУСТЫЕ» (пустые пакеты/банки), ради экономии ресурсов.</color>" },
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
            { "settings.save_protect", "Защищать файлы сохранения" },
            { "settings.save_protect_hint", "<color=#9AA6B2>Снимает блокировку «только чтение», которая иногда мешает игре сохраняться.</color>" },
            { "settings.save_backup", "Автоматически делать бэкапы" },
            { "settings.save_backup_hint", "<color=#9AA6B2>Хранит несколько последних бэкапов в MOP_Backups, чтобы можно было откатиться.</color>" },
            { "settings.save_verify", "Проверять целостность сейва" },
            { "settings.save_verify_hint", "<color=#9AA6B2>Проверяет загруженный сейв на известные баги игры/сейва и предлагает их исправить.</color>" },
            { "settings.restore_bolts", "Восстанавливать затяжку болтов" },
            { "settings.restore_bolts_hint", "<color=#9AA6B2>Восстанавливает затяжку болтов, которую MSC самопроизвольно сбрасывает между сохранениями.</color>" },
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
