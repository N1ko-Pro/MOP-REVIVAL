// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Единый источник цветов мода. Меняйте значения ТОЛЬКО здесь — они применяются везде (консоль/логи,
// окно настроек). Цвета в формате HEX ("#RRGGBB"). Ниже — хелперы, оборачивающие текст в rich-text
// теги Unity (<color>, <b>), которыми пользуются ModConsole и окно настроек.

namespace MOPR.Common
{
    internal static class MoprColors
    {
        // ============================================================
        //  БЛОК: ЗАГОЛОВКИ БЛОКОВ НАСТРОЕК (фон/текст, UnityEngine.Color)
        // ============================================================

        /// <summary>Фон заголовка «Информация» — чёрный.</summary>
        public static readonly UnityEngine.Color HeaderInfoBackground = Rgb(0, 0, 0);
        /// <summary>Фон заголовка «Язык» — зелёный.</summary>
        public static readonly UnityEngine.Color HeaderLanguageBackground = Rgb(0x2E, 0x7D, 0x32);
        /// <summary>Текст заголовков блоков — жёлтый.</summary>
        public static readonly UnityEngine.Color HeaderTitleText = Rgb(0xF5, 0xC5, 0x18);

        private static UnityEngine.Color Rgb(int r, int g, int b) => new UnityEngine.Color(r / 255f, g / 255f, b / 255f);

        // ============================================================
        //  БЛОК: ЛОГИ / КОНСОЛЬ
        // ============================================================

        /// <summary>Тег [MOPR] — оранжевый, жирный.</summary>
        public const string LogTag = "#FF7A1A";
        /// <summary>Обычный текст в консоли — нежно-бежевый.</summary>
        public const string LogText = "#f0c562ff";
        /// <summary>Версия мода — бирюзовый (как в MSCLoader).</summary>
        public const string LogVersion = "#35E0E0";
        /// <summary>Успех / «доступно» — зелёный.</summary>
        public const string LogSuccess = "#3FB950";
        /// <summary>Предупреждение — жёлтый.</summary>
        public const string LogWarning = "#F5C518";
        /// <summary>Ошибка / «недоступно» — красный.</summary>
        public const string LogError = "#FF5151";
        /// <summary>Акцент (названия модулей и т.п.) — бирюзовый.</summary>
        public const string LogAccent = "#35E0E0";
        /// <summary>Приглушённый текст (например, «отключён») — серый.</summary>
        public const string LogMuted = "#9AA6B2";

        // ============================================================
        //  БЛОК: ОКНО НАСТРОЕК
        // ============================================================

        /// <summary>Название мода в шапке.</summary>
        public const string SettingsBrand = "#FF7A1A";
        /// <summary>Заголовки секций.</summary>
        public const string SettingsHeader = "#35E0E0";
        /// <summary>Заголовок секции «Прочее».</summary>
        public const string SettingsHeaderOther = "#F5C518";
        /// <summary>Заголовок аварийной секции.</summary>
        public const string SettingsEmergency = "#FF5151";
        /// <summary>Подсказки под пунктами.</summary>
        public const string SettingsHint = "#9AA6B2";
        /// <summary>Версия в информации.</summary>
        public const string SettingsVersion = "#35E0E0";
        /// <summary>Автор в информации.</summary>
        public const string SettingsAuthor = "#F5A623";
        /// <summary>Статус сервера: доступен.</summary>
        public const string SettingsServerOk = "#3FB950";
        /// <summary>Статус сервера: проверка.</summary>
        public const string SettingsServerChecking = "#F5C518";
        /// <summary>Статус сервера: недоступен.</summary>
        public const string SettingsServerOffline = "#FF5151";

        // ============================================================
        //  ХЕЛПЕРЫ
        // ============================================================

        /// <summary>Оборачивает текст в цвет: &lt;color=hex&gt;text&lt;/color&gt;.</summary>
        public static string Colorize(string hex, string text) => "<color=" + hex + ">" + text + "</color>";

        /// <summary>Делает текст жирным.</summary>
        public static string Bold(string text) => "<b>" + text + "</b>";

        // Короткие обёртки под цвета лог-блока.
        public static string Text(string s) => Colorize(LogText, s);
        public static string Version(string s) => Bold(Colorize(LogVersion, s));
        public static string Success(string s) => Colorize(LogSuccess, s);
        public static string Warning(string s) => Colorize(LogWarning, s);
        public static string Error(string s) => Colorize(LogError, s);
        public static string Accent(string s) => Colorize(LogAccent, s);
        public static string Muted(string s) => Colorize(LogMuted, s);

        /// <summary>Подзаголовок секции настроек: жирный, заданного цвета.</summary>
        public static string Section(string hex, string text) => Bold(Colorize(hex, text));

        // ============================================================
        //  БЛОК: РАЗДЕЛИТЕЛИ / АКЦЕНТЫ МЕНЮ НАСТРОЕК
        // ============================================================

        /// <summary>Акцент-маркер перед подзаголовком. При отсутствии глифа замените на '»' или '•'.</summary>
        public const string SubheaderMarker = "▍";

        /// <summary>Текст подзаголовка: акцент-маркер + ЗАГЛАВНЫЙ текст (без цвета — цвет добавит Section).</summary>
        public static string SubheaderContent(string rawText) => SubheaderMarker + " " + rawText.ToUpper();

        /// <summary>Парсит "#RRGGBB" в UnityEngine.Color (без зависимости от версии ColorUtility).</summary>
        public static UnityEngine.Color ToColor(string hex)
        {
            hex = hex.TrimStart('#');
            int r = System.Convert.ToInt32(hex.Substring(0, 2), 16);
            int g = System.Convert.ToInt32(hex.Substring(2, 2), 16);
            int b = System.Convert.ToInt32(hex.Substring(4, 2), 16);
            return Rgb(r, g, b);
        }

        /// <summary>Стилизованный тег [MOPR] (жирный оранжевый).</summary>
        public static string Tag() => Bold(Colorize(LogTag, "[MOPR]"));

        /// <summary>Заменяет обычный «[MOPR]» на стилизованный тег (для технических логов).</summary>
        public static string StyleTag(string raw)
        {
            return raw == null ? null : raw.Replace("[MOPR]", Tag());
        }

        /// <summary>
        /// Готовит статусную строку для консоли: стилизованный тег + тело обычного (бежевого) цвета.
        /// Вложенные цветовые теги в теле (версия/успех/ошибка) перекрывают бежевый на своих участках.
        /// </summary>
        public static string FormatStatus(string raw)
        {
            if (string.IsNullOrEmpty(raw))
                return raw;

            const string tag = "[MOPR]";
            int idx = raw.IndexOf(tag);
            if (idx < 0)
                return Text(raw);

            string before = raw.Substring(0, idx);
            string after = raw.Substring(idx + tag.Length);
            return before + Tag() + Text(after);
        }
    }
}
