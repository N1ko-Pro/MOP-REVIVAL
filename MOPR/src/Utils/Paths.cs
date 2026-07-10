// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Централизованные пути к файлам мода: корень игры, папка логов, отчёты об ошибках, файл настроек.

using System.IO;
using UnityEngine;

namespace MOPR.Common
{
    internal static class Paths
    {
        public const string LogFolderName = "MOPR_Logs";
        public const string DefaultErrorLogName = "MOPR_Crash";
        public const string DefaultReportLogName = "MOPR_Report";
        public const string BugReportFileName = "MOPR Bug Report";
        public const string SaveFileBugsReport = "SaveFileBugsReport";

        /// <summary>Путь к файлу настроек мода (в папке настроек MSCLoader).</summary>
        public static string MopSettingsFile => Path.Combine(MOPR.ModConfigPath, "settings.json");

        /// <summary>Корень установки игры (без подпапки mysummercar_Data).</summary>
        public static string RootPath => Application.dataPath.Replace("/mysummercar_Data", "");

        public static string OutputLogPath => RootPath + "/output_log.txt";
        public static string BugReportPath => RootPath + "/MOPR_bugreport";

        /// <summary>Папка логов мода. Создаётся при первом обращении.</summary>
        public static string LogFolder
        {
            get
            {
                string path = Path.Combine(RootPath, LogFolderName);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                return path;
            }
        }

        public static bool LogDirectoryExists => Directory.Exists(LogFolder);
    }
}
