// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Строки, появляющиеся ТОЛЬКО в логе, игровой консоли (команда mopr) и мониторе диагностики.
// Всё, что игрок видит в обычном UI мода, — в LocTextCore. Оба словаря держат одни и те же ключи.

using System.Collections.Generic;

namespace MOPR.Localization
{
    internal static class LocTextLog
    {
        /// <summary>Таблица строк для запрошенного языка.</summary>
        public static Dictionary<string, string> For(Language language)
        {
            return language == Language.Russian ? Russian : English;
        }

        private static readonly Dictionary<string, string> English = new Dictionary<string, string>
        {
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

        private static readonly Dictionary<string, string> Russian = new Dictionary<string, string>
        {
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
