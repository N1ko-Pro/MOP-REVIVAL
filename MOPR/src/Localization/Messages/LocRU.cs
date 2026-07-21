// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Русский языковой пакет: ВСЕ строки мода (UI + логи/консоль/монитор) в одной таблице. Отсутствующие
// ключи откатываются на английский (см. LocalizationCore.Resolve). Диспетчер языков — LocTextCore.

using System.Collections.Generic;

namespace MOPR.Localization
{
    internal static class LocRU
    {
        public static readonly Dictionary<string, string> Strings = new Dictionary<string, string>
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
            { "settings.water_reflections", "Оптимизировать отражения воды" },
            { "settings.water_reflections_hint", "<color=#9AA6B2>В помещениях отсекает камеру отражений озера. Вода становится плоской.</color>" },
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

            // ================================================================================
            // Логи / консоль (команда mopr) / монитор диагностики — не видно в обычном UI мода.
            // ================================================================================

            // === Logs ===
            { "log.menu_init", "{0} успешно загружен!" },
            { "log.mod_initialized", "Успешно инициализирован! Версия {0}" },
            { "log.disabled", "Оптимизация отключена в настройках; все управляемые объекты включены обратно." },
            { "log.lang_changed", "Язык переключён на русский." },
            { "log.catalog_summary", "Каталог: зарегистрировано объектов: {0} (бревенчатых стен: {1})." },
            { "log.catalog_missing", "Не найдено в этой версии игры ({0}): {1}" },
            { "log.mode_reapplied", "Режим изменён; теперь под управлением: {0}." },
            { "log.no_player", "Объект PLAYER не найден; оптимизация по расстоянию приостановлена." },
            { "log.failsafe", "Обнаружены повторяющиеся ошибки; оптимизация приостановлена (аварийный режим). Выключите/включите мод или перезапустите игру." },
            { "log.save_readonly_cleared", "Снята блокировка read-only с {0} файла(ов) сохранения." },
            { "log.save_backup_ok", "Сейв сохранён в бэкап ({0} файл(ов)) -> {1}" },
            { "log.save_restored", "Сейв восстановлен из бэкапа {0}." },
            { "log.verify_start", "Проверка целостности сейва..." },
            { "log.verify_skipped", "Проверка сейва пропущена (сейв после перма-смерти)." },
            { "log.verify_clean", "Целостность сейва в порядке - проблем не найдено." },
            { "log.verify_found", "Целостность сейва: найдено проблем - {0}." },
            { "log.verify_done", "Исправление сейва: успешно {0}, не удалось {1}." },
            { "log.bolt_snapshot_saved", "Снимок затяжки болтов сохранён ({0} шт.)." },
            { "log.save_corrupt", "Сейв выглядит повреждённым: сейчас {0} байт против {1} байт в последнем бэкапе." },
            { "log.continue_restored", "Кнопка «Продолжить» в главном меню включена обратно." },
            { "log.rules_sync_start", "Запуск проверки доступности сервера..." },
            { "log.rules_sync_ok", "Сервер правил - {0}\nДоступно - {1}; Активно - {2}" },
            { "log.rules_sync_offline", "Сервер правил - {0}" },
            { "log.rules_sync_error", "Ошибка сервера правил: {0}" },
            { "log.server_status_available", "ДОСТУПЕН" },
            { "log.server_status_unavailable", "НЕДОСТУПЕН" },
            { "log.rules_downloaded", "Скачано файлов правил с сервера: {0}." },

            // === Performance overlay / report ===
            { "perf.fps", "FPS: {0}  (сред. {1} мс, худш. {2} мс)" },
            { "perf.spikes", "Просадки: {0}  (из-за GC: {1})" },
            { "perf.objects", "Объекты: {0} под управлением, {1} активно" },

            // === Status (always shown on load) ===
            { "status.core_init", "Ядро инициализировано" },
            { "status.managed", "Объектов под управлением: {0}" },
            { "status.core_loaded", "Ядро загружено" },
            { "status.module_loaded", "Модуль {0} - Загружен ({1})" },
            { "status.module_off", "Модуль {0} - Отключён" },
            { "status.mod.world", "Объекты мира" },
            { "status.mod.items", "Предметы" },
            { "status.mod.vehicles", "Транспорт" },
            { "status.mod.places", "Локации" },
            { "status.feature", "{0} - {1}" },
            { "status.word.enabled", "Включено" },
            { "status.word.disabled", "Отключено" },
            { "status.word.applied", "Применены" },
            { "status.feat.fixes", "Игровые фиксы" },
            { "status.feat.engine", "Движковые патчи" },
            { "status.feat.sector", "Куллинг помещений" },
            { "status.feat.ddd", "Динамическая дальность" },
            { "status.feat.sleep", "Сон дальних тел" },
            { "status.feat.gc", "Адаптивный GC" },
            { "status.feat.saves", "Защита сохранений" },
            { "status.ready", "Готово к работе!" },

            // === Console command (mopr) ===
            { "cmd.help", "Диагностика MOP Revival. Использование: mopr help | version | status | monitor | presave | stop | start | config | logs | report" },
            { "cmd.not_running", "Ядро не запущено. Сначала загрузите сохранение." },
            { "cmd.not_ready", "Ядро ещё не готово." },
            { "cmd.status", "включено={0}, дистанция={1}x, под управлением={2}, активно={3}." },
            { "cmd.unknown", "Неизвестная подкоманда. Введите «mopr help» для списка." },

            // === Object states ===
            { "state.active", "активен" },
            { "state.disabled", "отключён" },
            { "state.destroyed", "уничтожен" },
        };
    }
}
