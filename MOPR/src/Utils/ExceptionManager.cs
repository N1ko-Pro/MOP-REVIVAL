// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Централизованная обработка ошибок и генерация диагностических отчётов. Записывает крэш-дампы
// в папку логов (по одному файлу на сессию), формирует подробный отчёт о системе, настройках
// мода, состоянии игры и списке установленных модов для передачи разработчику.

using MSCLoader;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEngine;

namespace MOPR.Common
{
    internal static class ExceptionManager
    {
        private static readonly List<string> reportedErrors = new List<string>();
        private static string currentLogFile;

        public static DateTime SessionTimeStart { get; set; }

        /// <summary>Записывает ошибку в лог-файл текущей сессии и уведомляет игрока.</summary>
        public static void New(Exception ex, bool isCritical, string message)
        {
            // Одну и ту же ошибку не дублируем.
            if (reportedErrors.Contains(message))
                return;
            reportedErrors.Add(message);

            bool isNewFile = false;
            if (string.IsNullOrEmpty(currentLogFile))
            {
                string fileName = string.Format("{0}_{1:yyyy-MM-dd-HH-mm}", Paths.DefaultErrorLogName, DateTime.Now);
                if (File.Exists(Paths.LogFolder + "/" + fileName + ".txt"))
                {
                    int index = 0;
                    while (File.Exists(Paths.LogFolder + "/" + fileName + "_" + index + ".txt"))
                        index++;
                    fileName += "_" + index;
                }

                isNewFile = true;
                currentLogFile = fileName + ".txt";
            }

            string logFilePath = Path.Combine(Paths.LogFolder, currentLogFile).Replace("\\", "/");
            string errorInfo = string.Format(
                "({0:HH:mm:ss.fff})\n  Code: {1}\n  Type: {2}\n  Message: {3}{4}\n  Target Site: {5}",
                DateTime.Now, message, ex.GetType().Name, ex.Message, ex.StackTrace, ex.TargetSite);

            if (isNewFile)
                errorInfo = "MOPR " + MOPR.ModVersion + "\n" + errorInfo;

            using (StreamWriter sw = new StreamWriter(logFilePath, true))
                sw.Write(errorInfo + "\n\n");

            string userMessage =
                "[MOPR] An error has occured. The log file has been saved into:\n\n" +
                logFilePath + "\nMessage: " + message + "\n\n" +
                "Please go into MOPR Settings, and click \"<b>I FOUND A BUG</b>\" button, in order to generate the bug report. " +
                "Then please follow the provided instructions.\n";

            if (isCritical)
                ModConsole.LogError(userMessage);
            else
                ModConsole.LogWarning(userMessage + "\n<b>You can continue playing.</b>");
        }

        public static void OpenCurrentSessionLogFolder()
        {
            if (Paths.LogDirectoryExists)
                Process.Start(Paths.LogFolder);
            else
                ModUI.ShowMessage("Logs folder doesn't exist.", "MOPR");
        }

        public static void OpenOutputLog()
        {
            if (File.Exists(Paths.OutputLogPath))
                Process.Start(Paths.OutputLogPath);
            else
                ModUI.ShowMessage("File \"output_log.txt\" doesn't exist.", "MOPR");
        }

        /// <summary>Сохраняет отчёт с информацией о моде и списком установленных модов.</summary>
        public static void GenerateReport()
        {
            string gameInfo = GetGameInfo();

            int index = 0;
            while (File.Exists(string.Format("{0}/{1}_{2}.txt", Paths.LogFolder, Paths.DefaultReportLogName, index)))
                index++;

            string path = string.Format("{0}/{1}_{2}.txt", Paths.LogFolder, Paths.DefaultReportLogName, index);
            using (StreamWriter sw = new StreamWriter(path))
                sw.Write(gameInfo);

            ModConsole.Log("[MOPR] Mod report has been successfully generated.");
            Process.Start(path);
        }

        /// <summary>Собирает полный текстовый отчёт: система, настройки, данные игры, моды.</summary>
        internal static string GetGameInfo()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Modern Optimization Plugin");
            sb.Append("Version: ").AppendLine(MOPR.ModVersion);
            sb.Append("MSC Mod Loader Version: ").AppendLine(ModLoader.MSCLoader_Ver);
            sb.Append("Date and Time: ").AppendLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            sb.AppendLine(GetSystemInfo());
            sb.Append("Game restarts: ").Append(MoprSettings.Restarts).AppendLine();
            sb.Append("Game resolution: ").Append(Screen.width).Append("x").Append(Screen.height).AppendLine();
            sb.Append("Screen resolution: ").Append(Screen.currentResolution.width).Append("x").Append(Screen.currentResolution.height).AppendLine();
            if (ModLoader.CurrentScene == CurrentScene.Game)
            {
                TimeSpan elapsed = DateTime.Now.Subtract(SessionTimeStart);
                sb.Append("Session time: ").Append(elapsed.Hours).Append(" Hours ").Append(elapsed.Minutes).Append(" Minutes ").Append(elapsed.Seconds).AppendLine(" Seconds");
            }
            sb.Append("CPU: ").Append(SystemInfo.processorType).Append(" (").Append(SystemInfo.processorCount).AppendLine(" cores)");
            sb.Append("RAM: ").Append(SystemInfo.systemMemorySize).AppendLine(" MB");
            sb.Append("GPU: ").Append(SystemInfo.graphicsDeviceName).Append(" (").Append(SystemInfo.graphicsMemorySize).AppendLine(" MB VRAM)");

            sb.AppendLine();
            sb.AppendLine("=== MOPR SETTINGS ===");
            sb.AppendLine();
            sb.Append("ActiveDistance: ").Append(MoprSettings.ActiveDistanceValue).AppendLine();
            sb.Append("ActiveDistanceMultiplier: ").Append(MoprSettings.ActiveDistanceMultiplicationValue).AppendLine();
            sb.Append("PerformanceMode: ").Append(MoprSettings.Mode).AppendLine();
            sb.Append("LimitFramerate: ").Append(MoprSettings.LimitFramerateOn).AppendLine();
            sb.Append("FramerateLimiter: ").Append(MoprSettings.FramerateLimitValue).AppendLine();
            sb.Append("ShadowDistance: ").Append(MoprSettings.ShadowDistanceValue).AppendLine();
            sb.Append("RunInBackground: ").Append(MoprSettings.RunInBackgroundOn).AppendLine();
            sb.Append("DynamicDrawDistance: ").Append(MoprSettings.DynamicDrawDistanceOn).AppendLine();
            sb.Append("DestroyEmptyBottles: ").Append(MoprSettings.DestroyEmptyBottlesOn).AppendLine();
            sb.Append("DisableEmptyItems: ").Append(MoprSettings.DisableEmptyItemsOn).AppendLine();
            sb.Append("NoSkidmarks: ").Append(MoprSettings.DisableSkidmarksOn).AppendLine();
            sb.Append("HideLakeVegetation: ").Append(MoprSettings.HideLakeVegetationOn).AppendLine();
            sb.Append("ParkingBrakeAnchor: ").Append(MoprSettings.ParkingBrakeAnchorOn).AppendLine();

            if (ModLoader.CurrentScene == CurrentScene.Game)
            {
                sb.AppendLine();
                sb.AppendLine("=== GAME DATA ===");
                sb.AppendLine();
                sb.AppendLine(GetGameData("PlayerPosition"));
                sb.AppendLine(GetGameData("PlayerHasHayosikoKey"));
                sb.AppendLine(GetGameData("IsPlayerInCar"));
                sb.AppendLine(GetGameData("IsPlayerInSatsuma"));
                sb.AppendLine(GetGameData("DrawDistance"));
                sb.AppendLine(GetGameData("CanTriggerStatus"));
                sb.AppendLine(GetGameData("IsTrailerAttached"));
            }

            sb.AppendLine();
            sb.AppendLine("=== MESSAGES ===");
            sb.AppendLine();
            sb.AppendLine(string.Join("\n", ModConsole.GetMessages().ToArray()));

            sb.AppendLine();
            sb.Append("=== MODS (").Append(ModLoader.LoadedMods.Count - 1).AppendLine(") ===");
            sb.AppendLine();
            foreach (Mod mod in ModLoader.LoadedMods)
            {
                if (mod.ID.Equals("MOPR"))
                    continue;

                sb.AppendLine(mod.Name);
                sb.Append("  ID: ").AppendLine(mod.ID);
                sb.Append("  Version: ").AppendLine(mod.Version);
                sb.Append("  Author: ").AppendLine(mod.Author);
                sb.AppendLine();
            }

            // Только MOPR + два модуля загрузчика — значит других модов нет.
            if (ModLoader.LoadedMods.Count <= 3)
                sb.AppendLine("No other mods found");

            return sb.ToString();
        }

        private static string GetGameData(string name)
        {
            string output = name + ": ";
            try
            {
                switch (name)
                {
                    default:
                        output += "null";
                        break;
                    case "PlayerPosition":
                        output += GameObject.Find("PLAYER").transform.position;
                        break;
                    case "PlayerHasHayosikoKey":
                        output += FSM.FsmManager.PlayerHasHayosikoKey();
                        break;
                    case "IsPlayerInCar":
                        output += FSM.FsmManager.IsPlayerInCar();
                        break;
                    case "IsPlayerInSatsuma":
                        output += FSM.FsmManager.IsPlayerInSatsuma();
                        break;
                    case "DrawDistance":
                        output += FSM.FsmManager.GetDrawDistance();
                        break;
                    case "CanTriggerStatus":
                        output += Managers.ItemsManager.Instance == null
                            ? "manager_null"
                            : Managers.ItemsManager.Instance.GetCanTrigger() == null
                                ? "null"
                                : "Found (" + Managers.ItemsManager.Instance.GetCanTrigger().Path() + ")";
                        break;
                    case "IsTrailerAttached":
                        output += FSM.FsmManager.IsTrailerAttached();
                        break;
                }
            }
            catch
            {
                output += "error";
            }

            return output;
        }

        public static string GetSystemInfo()
        {
            string fullOs = SystemInfo.operatingSystem;
            string windowsVersion = GetWindowsName(fullOs);

            // Wine оставляет характерный лог — значит игра запущена под Linux/macOS.
            if (File.Exists("Z:/var/log/syslog"))
                return string.Format("{0} [Wine, {1}]", GetLinuxName(), windowsVersion);

            return windowsVersion;
        }

        private static string GetWindowsName(string fullOs)
        {
            int build = int.Parse(fullOs.Split('(')[1].Split(')')[0].Split('.')[2]);
            if (build > 9600)
            {
                string realOs = build >= 22000
                    ? "Windows 11 (10.0." + build + ")"
                    : "Windows 10 (10.0." + build + ")";

                if (Directory.Exists("C:\\Program Files (Arm)"))
                    realOs += " ARM";
                else
                    realOs += SystemInfo.operatingSystem.Contains("64bit") ? " 64bit" : " 32bit";

                return realOs;
            }

            return fullOs;
        }

        private static string GetLinuxName()
        {
            const string osReleaseFile = "Z:/etc/os-release";
            string output = "Linux";

            if (File.Exists(osReleaseFile))
            {
                using (StreamReader reader = new StreamReader(osReleaseFile))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        if (line.Contains("PRETTY_NAME"))
                        {
                            output = line.Replace("PRETTY_NAME=", "").Replace("\"", "");
                            break;
                        }
                    }
                }
            }
            else if (Directory.Exists("Z:/Applications"))
            {
                output = "macOS";
            }

            return output;
        }

        /// <summary>Удаляет все лог-файлы после подтверждения игроком.</summary>
        public static void DeleteAllLogs()
        {
            string[] files = Directory.GetFiles(Paths.LogFolder, "*.txt");
            if (files.Length == 0)
            {
                ModUI.ShowMessage("No logs exist.", "MOPR");
                return;
            }

            ModUI.ShowYesNoMessage(
                string.Format("Are you sure you want to delete <color=yellow>{0}</color> log{1}?", files.Length, files.Length > 1 ? "s" : ""),
                "MOPR",
                () => DeleteLogFiles(files));
        }

        private static void DeleteLogFiles(string[] files)
        {
            foreach (string file in files)
                File.Delete(file);
        }
    }
}
