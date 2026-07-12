// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Внутриигровая консольная команда «mopr»: диагностика и управление модом (статус, генерация
// списков, монитор, пред-сохранение, старт/стоп, конфиг, логи, отчёт). Ответы печатаются всегда
// (LogAlways), независимо от настройки «Показывать лог-сообщения».

using System;
using System.Diagnostics;
using MSCLoader;
using MOPR.Common;
using MOPR.Common.Enumerations;
using MOPR.Managers;

namespace MOPR.Commands
{
    internal sealed class MoprCommand : ConsoleCommand
    {
        public override string Name => "mopr";
        public override string Help => "Use \"mopr help\" for the command list.";

        public override void Run(string[] args)
        {
            string sub = args.Length > 0 ? args[0].ToUpperInvariant() : "STATUS";

            switch (sub)
            {
                case "?":
                case "H":
                case "HELP": PrintHelp(); break;
                case "VERSION": PrintVersion(); break;
                case "STATUS": PrintStatus(); break;
                case "GENERATE-LIST": ToggleGenerateList(args); break;
                case "DUMP": DumpObject(args); break;
                case "MONITOR": ToggleMonitor(); break;
                case "PRESAVE": RunPreSave(); break;
                case "STOP": StopMod(); break;
                case "START": StartMod(); break;
                case "CONFIG": OpenInShell(MOPR.ModConfigPath); break;
                case "LOGS": ExceptionManager.OpenCurrentSessionLogFolder(); break;
                case "REPORT": ExceptionManager.GenerateReport(); break;
                default:
                    ModConsole.LogAlways("Unknown command. Type \"mopr help\" for the list.");
                    break;
            }
        }

        private static void PrintHelp()
        {
            ModConsole.LogAlways(
                "<color=yellow>help</color> - show this list\n" +
                "<color=yellow>version</color> - print MOPR version\n" +
                "<color=yellow>status</color> - mod state, mode and managed object counts\n" +
                "<color=yellow>generate-list [true/false]</color> - dump lists of toggled objects\n" +
                "<color=yellow>dump <ObjectName></color> - dump an object's full hierarchy + components (in-game)\n" +
                "<color=yellow>monitor</color> - toggle the on-screen debug monitor (in-game)\n" +
                "<color=yellow>presave</color> - run the pre-save routine (in-game)\n" +
                "<color=yellow>stop</color> - stop MOPR (in-game, may break things)\n" +
                "<color=yellow>start</color> - start MOPR back (in-game)\n" +
                "<color=yellow>config</color> - open the MOPR config folder\n" +
                "<color=yellow>logs</color> - open the current session log folder\n" +
                "<color=yellow>report</color> - generate a diagnostics report");
        }

        private static void PrintVersion()
        {
            ModConsole.LogAlways("[MOPR] " + MOPR.ModVersion + " for " + MOPR.Edition);
        }

        private static void PrintStatus()
        {
            if (Core.Instance == null)
            {
                ModConsole.LogAlways("[MOPR] Not running. Load a save first.");
                return;
            }

            ModConsole.LogAlways(
                "[MOPR] " + MOPR.ModVersion + " for " + MOPR.Edition + "\n" +
                "Active: <color=" + (MoprSettings.IsModActive ? "green" : "red") + ">" + MoprSettings.IsModActive + "</color>\n" +
                "Mode: " + MoprSettings.Mode + "\n" +
                "Active distance: " + MoprSettings.ActiveDistanceMultiplicationValue + "x\n" +
                "World objects: " + DescribeManager(WorldObjectManager.Instance) + "\n" +
                "Items: " + DescribeManager(ItemsManager.Instance) + "\n" +
                "Vehicles: " + DescribeManager(VehicleManager.Instance) + "\n" +
                "Places: " + DescribeManager(PlaceManager.Instance));
        }

        /// <summary>Формирует «включено/всего» для менеджера (или «-», если он ещё не создан).</summary>
        private static string DescribeManager<T>(Common.Interfaces.IManager<T> manager)
        {
            return manager == null ? "-" : manager.EnabledCount + "/" + manager.Count;
        }

        private static void ToggleGenerateList(string[] args)
        {
            if (args.Length > 1)
                MoprSettings.GenerateToggledItemsListDebug = args[1].ToUpperInvariant() == "TRUE";

            ModConsole.LogAlways("[MOPR] Toggled-list generation: <color=" +
                (MoprSettings.GenerateToggledItemsListDebug ? "green" : "red") + ">" + MoprSettings.GenerateToggledItemsListDebug + "</color>");
        }

        private static void DumpObject(string[] args)
        {
            if (!RequireInGame())
                return;

            if (args.Length < 2)
            {
                ModConsole.LogAlways("[MOPR] Usage: mopr dump <ObjectName>  (e.g. mopr dump SVOBODA(855kg))");
                return;
            }

            // Имена объектов могут содержать пробелы — склеиваем всё после "dump" обратно.
            string name = string.Join(" ", args, 1, args.Length - 1);
            ObjectDumper.Dump(name);
        }

        private static void ToggleMonitor()
        {
            if (RequireInGame())
                Core.Instance.ToggleDebugMode();
        }

        private static void RunPreSave()
        {
            if (!RequireInGame())
                return;

            ModConsole.LogAlways("[MOPR] Running pre-save...");
            Core.Instance.PreSaveGame();
        }

        private static void StopMod()
        {
            if (!RequireInGame())
                return;

            if (!MoprSettings.IsModActive)
            {
                ModConsole.LogAlways("[MOPR] MOPR is not running.");
                return;
            }

            Core.Instance.StopAllCoroutines();
            Core.Instance.ToggleAll(true);
            MoprSettings.IsModActive = false;
            ModConsole.LogAlways("[MOPR] Stopped. Saving in this state may break your game!");
        }

        private static void StartMod()
        {
            if (!RequireInGame())
                return;

            if (MoprSettings.IsModActive)
            {
                ModConsole.LogAlways("[MOPR] MOPR is already running.");
                return;
            }

            Core.Instance.ToggleAll(false, ToggleAllMode.OnLoad);
            Core.Instance.Startup();
            ModConsole.LogAlways("[MOPR] Started.");
        }

        /// <summary>Проверяет, что мы в игре и ядро создано; иначе пишет подсказку.</summary>
        private static bool RequireInGame()
        {
            if (ModLoader.CurrentScene != CurrentScene.Game || Core.Instance == null)
            {
                ModConsole.LogAlways("[MOPR] This command can only be used in-game.");
                return false;
            }

            return true;
        }

        /// <summary>Открывает файл или папку в системной оболочке, логируя ошибку вместо исключения.</summary>
        private static void OpenInShell(string path)
        {
            try
            {
                Process.Start(path);
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "OPEN_IN_SHELL_ERROR");
            }
        }
    }
}
