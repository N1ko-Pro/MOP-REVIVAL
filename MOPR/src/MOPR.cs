// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Точка входа мода: тонкая оболочка жизненного цикла MSCLoader. Настройки строит MoprSettingsWindow,
// всю оптимизацию ведёт компонент Core. Здесь — только привязка фаз (ModSettings/ModSettingsLoaded/
// OnMenuLoad/PostLoad), путь конфига и создание ядра после полной загрузки игры.

using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using MSCLoader;

using MOPR.Common;
using MOPR.Common.Enumerations;
using MOPR.FSM;
using MOPR.Helpers;
using MOPR.Saves;
using MOPR.Rules;
using MOPR.Localization;
using MOPR.Interface.Gui;

namespace MOPR
{
    public class MOPR : Mod
    {
        public override string ID => "MOPR";
        public override string Name => "MOP - REVIVAL";
        public override string Author => "ANICKON";
        public override string Version => "4.0.0b";
        public const string SubVersion = "";
        public const string Edition = "MSCLoader";
        public override string Description => "The <color=yellow>ultimate</color> My Summer Car optimization project!";

        // Иконка мода (icon.png вшит в DLL). Читается один раз и кэшируется.
        private static byte[] iconBytes;
        public override byte[] Icon => iconBytes ?? (iconBytes = LoadIcon());

        private static byte[] LoadIcon()
        {
            try
            {
                Assembly asm = Assembly.GetExecutingAssembly();
                using (Stream stream = asm.GetManifestResourceStream("MOPR.icon.png"))
                {
                    if (stream == null)
                        return new byte[0];

                    byte[] data = new byte[stream.Length];
                    int offset = 0;
                    int read;
                    while (offset < data.Length && (read = stream.Read(data, offset, data.Length - offset)) > 0)
                        offset += read;

                    return data;
                }
            }
            catch
            {
                return new byte[0];
            }
        }

        private static string modConfigPath;
        public static string ModConfigPath => modConfigPath;

        private static string modVersion;
        public static string ModVersion => modVersion + (SubVersion != "" ? "_" + SubVersion : "");
        public static string ModVersionShort => modVersion;

        public override void ModSetup()
        {
            modVersion = Version;
            modConfigPath = ModLoader.GetModSettingsFolder(this);
            LocalizationConfig.Initialize(this);

            SetupFunction(Setup.ModSettings, () => MoprSettingsWindow.Build(this));
            SetupFunction(Setup.ModSettingsLoaded, ModSettingsLoaded);
            SetupFunction(Setup.OnMenuLoad, MenuOnLoad);
            SetupFunction(Setup.PostLoad, SecondPassOnLoad);
        }

        private void MenuOnLoad()
        {
            RemoveUnusedFiles();

            // Спасение обнулённого сейва из меню (до попытки загрузки).
            SaveGuard.CheckOnMenu();

            // Фоновая синхронизация правил с сервером; при новых правилах покажет окно-приглашение.
            RuleSyncRunner.Launch(false);

            // Разовая запись версии в конфиг при первом запуске новой версии.
            if (!Version.Contains(MoprSettings.Data.Version))
            {
                MoprSettings.Data.Version = Version;
                MoprSettings.WriteData(MoprSettings.Data);
            }

            FsmManager.ResetAll();
            Resources.UnloadUnusedAssets();
            GC.Collect();

            MoprSettings.Restarts++;
            if (MoprSettings.Restarts > MoprSettings.MaxRestarts && !MoprSettings.RestartWarningShown)
            {
                MoprSettings.RestartWarningShown = true;
                ModUI.ShowMessage("The game has been reloaded over " + MoprSettings.MaxRestarts + " times, which may cause issues with the game's physics.", "MOPR");
            }

            // Разовый рестарт сцены, если Сатсума не догрузилась (см. Core.Loading).
            if (MoprSettings.GameFixStatus == GameFixStatus.DoFix)
            {
                MoprSettings.GameFixStatus = GameFixStatus.Restarted;
                Application.LoadLevel(1);
                return;
            }

            MoprSettings.GameFixStatus = GameFixStatus.None;
        }

        private void ModSettingsLoaded()
        {
            if (modConfigPath == null)
                modConfigPath = ModLoader.GetModSettingsFolder(this);

            MoprSettingsWindow.OnSettingsLoaded();
            MoprSettings.UpdateMiscSettings();

            ConsoleCommand.Add(new Commands.MoprCommand());

            CompatibilityManager.ShowConflictWarningIfNeeded();

            SaveManager.VerifySave();

            ModConsole.Log("<color=green>MOPR " + ModVersion + " initialized!</color>");
        }

        // Запускается один раз после полной загрузки игры (PostLoad): здесь создаётся Core, чтобы мод
        // стартовал последним и не выгружал чужие объекты.
        private void SecondPassOnLoad()
        {
            MoprSettings.UpdateFramerateLimiter();
            MoprSettings.UpdateShadows();
            MoprSettings.UpdateMiscSettings();

            if (CompatibilityManager.IsConfilctingModPresent(out string modName))
            {
                ModConsole.LogError("MOPR could not be loaded, because the following mod is present: <color=yellow>" + modName + "</color>");
                return;
            }

            GameObject mop = new GameObject("MOPR");
            CompatibilityManager.Initialize();
            mop.AddComponent<Core>();
            SaveManager.AddSaveFlag();
        }

        private void RemoveUnusedFiles()
        {
            RemoveIfExists(Path.Combine(ModConfigPath, "LastModList.mop"));
            RemoveIfExists(Path.Combine(ModConfigPath, "LastUpdate.mop"));
            RemoveIfExists(Path.Combine(ModConfigPath, "RulesInfo.json"));
        }

        private static void RemoveIfExists(string filename)
        {
            if (File.Exists(filename))
                File.Delete(filename);
        }
    }
}
