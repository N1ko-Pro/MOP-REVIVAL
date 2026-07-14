// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Ядро локализации: хранит активный язык, собирает таблицы строк (UI + логи) в единый словарь на
// язык и разрешает ключи по цепочке «текущий язык → английский → сам ключ». При смене языка
// поднимает событие LanguageChanged, чтобы живой UI мог перерисоваться.

using System;
using System.Collections.Generic;

namespace MOPR.Localization
{
    /// <summary>Поддерживаемые языки. Значения совпадают с индексами выпадающего списка.</summary>
    internal enum Language
    {
        English = 0,
        Russian = 1,
        Polish = 2
    }

    internal static class LocalizationCore
    {
        /// <summary>
        /// Невидимый маркер «не переводить» (U+200B, zero-width space): им помечается ЛЮБАЯ строка MOPR.
        /// Имеет нулевую ширину (не отображается) и не ломает форматирование, т.к. стоит в начале строки.
        /// Сторонний мод-переводчик должен пропускать текст, содержащий этот символ:
        /// <c>if (uiText.text.Contains('\u200B')) return;</c> — тогда наш UI не переводится.
        /// Чтобы отключить пометку — поставьте пустую строку.
        /// </summary>
        public const string NoTranslateMarker = "\u200B";

        /// <summary>Текущий активный язык.</summary>
        public static Language Current = Language.English;

        /// <summary>Поднимается после смены языка.</summary>
        public static event Action LanguageChanged;

        private static readonly Dictionary<string, string> English = BuildTable(Language.English);
        private static readonly Dictionary<string, string> Russian = BuildTable(Language.Russian);
        private static readonly Dictionary<string, string> Polish = BuildTable(Language.Polish);

        /// <summary>Меняет язык (если отличается) и уведомляет слушателей.</summary>
        public static void SetCurrent(Language language)
        {
            if (Current == language)
                return;

            Current = language;
            if (LanguageChanged != null)
                LanguageChanged();
        }

        /// <summary>Переведённая строка для текущего языка (с невидимым маркером «не переводить»).</summary>
        public static string Get(string key)
        {
            return NoTranslateMarker + Resolve(key);
        }

        /// <summary>Разрешает ключ по цепочке «текущий язык → английский → сам ключ».</summary>
        private static string Resolve(string key)
        {
            Dictionary<string, string> table =
                Current == Language.Russian ? Russian :
                Current == Language.Polish ? Polish :
                English;
            if (table.TryGetValue(key, out string value))
                return value;

            // Запасной вариант: английский, затем сам ключ — пусто быть не должно.
            if (Current != Language.English && English.TryGetValue(key, out string fallback))
                return fallback;

            return key;
        }

        /// <summary>Переведённая и отформатированная строка.</summary>
        public static string Get(string key, params object[] args)
        {
            string format = Get(key);
            try
            {
                return string.Format(format, args);
            }
            catch (FormatException)
            {
                // Некорректный плейсхолдер не должен ронять игру.
                return format;
            }
        }

        private static Dictionary<string, string> BuildTable(Language language)
        {
            // Строки разложены по одному файлу на язык (LocEN/LocRU/LocPL) и берутся через LocTextCore;
            // копируем в свой словарь с Ordinal-сравнением, чтобы движок владел таблицей.
            Dictionary<string, string> table = new Dictionary<string, string>(StringComparer.Ordinal);
            Merge(table, LocTextCore.For(language));
            return table;
        }

        private static void Merge(Dictionary<string, string> target, Dictionary<string, string> source)
        {
            foreach (KeyValuePair<string, string> pair in source)
                target[pair.Key] = pair.Value;
        }
    }
}
