// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Ядро языковых пакетов: единая точка, которая по языку отдаёт нужную таблицу строк. Сами строки
// разложены по одному файлу на язык (LocEN / LocRU / LocPL) — там для каждого языка лежат ВСЕ ключи
// (UI + логи) сразу, что удобно переводчикам. Добавить язык = добавить LocXX.cs и ветку в For().
// Разрешение ключей и откат на английский — в LocalizationCore.

using System.Collections.Generic;

namespace MOPR.Localization
{
    internal static class LocTextCore
    {
        /// <summary>Полная таблица строк запрошенного языка (по умолчанию — английский).</summary>
        public static Dictionary<string, string> For(Language language)
        {
            switch (language)
            {
                case Language.Russian: return LocRU.Strings;
                case Language.Polish: return LocPL.Strings;
                default: return LocEN.Strings;
            }
        }
    }
}
