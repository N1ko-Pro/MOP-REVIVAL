// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Обёртка над MSCLoader.ModConsole. Оригинал MOP наследовал старый класс консоли; текущий
// MSCLoader предоставляет только статические Log/Warning/Error. Здесь единый фасад со всеми
// методами, которыми пользуется движок мода, плюс собственный буфер сообщений для отчётов
// (ExceptionManager читает его через GetMessages).

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MOPR.Common;

namespace MOPR
{
    internal static class ModConsole
    {
        private static readonly List<string> messages = new List<string>();
        private static readonly Regex tagStripper = new Regex("<(.*?)>");

        /// <summary>
        /// Информационное сообщение. В консоль печатается только при включённой настройке
        /// «Показывать лог-сообщения», но в буфер отчёта попадает всегда. Ошибки и предупреждения
        /// (LogError/LogWarning) печатаются независимо от настройки.
        /// </summary>
        public static void Log(string message)
        {
            Store(message);
            if (MoprSettings.ShowLogMessagesOn)
                MSCLoader.ModConsole.Log(message);
        }

        /// <summary>Как Log, но печатается всегда (для ответов консольной команды «mopr»).</summary>
        public static void LogAlways(string message)
        {
            Store(message);
            MSCLoader.ModConsole.Log(message);
        }

        public static void LogError(string message)
        {
            Store("[ERROR] " + message);
            MSCLoader.ModConsole.Error(message);
        }

        public static void LogWarning(string message)
        {
            Store("[WARNING] " + message);
            MSCLoader.ModConsole.Warning(message);
        }

        // Синонимы под прямой стиль текущего MSCLoader API.
        public static void Error(string message) => LogError(message);
        public static void Warning(string message) => LogWarning(message);

        public static void LogSilentError(string message)
        {
            Store("[SILENT_ERROR] " + message);
            MSCLoader.ModConsole.Log("<color=red>" + message + "</color>");
        }

        public static List<string> GetMessages() => messages;

        // Нормализует сообщение (убирает разметку, проставляет временную метку) и кладёт в буфер.
        private static void Store(string message)
        {
            message = message
                .Replace("[MOPR] ", "")
                .Replace("<color=yellow>", "[WARNING] ")
                .Replace("<color=green>", "[SYSTEM] ")
                .Replace("<color=red>", "[ERROR] ")
                .Trim();

            message = string.Format("{0:HH:mm:ss.fff}: {1}", DateTime.Now, message);

            string[] lines = message.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                if (i >= 1)
                    lines[i] = new string(' ', 10) + lines[i];
                lines[i] = tagStripper.Replace(lines[i], "");
            }

            messages.Add(string.Join("\n", lines));
        }
    }
}
