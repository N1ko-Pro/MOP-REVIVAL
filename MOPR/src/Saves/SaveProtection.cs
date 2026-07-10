// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Защита файлов сохранения: снимает атрибут «только чтение» (иногда мешающий игре сохраняться) и
// делает тайм-штампованные бэкапы перед каждым сохранением, храня несколько последних как точки
// отката. Все операции best-effort и никогда не бросают исключение в игру.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using MOPR.Common;
using MOPR.Localization;

namespace MOPR.Saves
{
    internal static class SaveProtection
    {
        private const int MaxBackups = 5;
        private const string MainSaveFile = "defaultES2File.txt";

        private static string SaveDir => Application.persistentDataPath;
        public static string BackupFolder => Path.Combine(SaveDir, "MOP_Backups");

        /// <summary>Все файлы сохранения игры (*.txt прямо в папке сейвов).</summary>
        private static string[] GetSaveFiles()
        {
            try
            {
                return Directory.Exists(SaveDir) ? Directory.GetFiles(SaveDir, "*.txt") : new string[0];
            }
            catch
            {
                return new string[0];
            }
        }

        /// <summary>При загрузке сейва: делаем файлы записываемыми на всю сессию.</summary>
        public static void OnGameLoad()
        {
            if (MoprSettings.SaveProtectionOn)
                RemoveReadOnly();
        }

        /// <summary>При сохранении: снять read-only, затем сделать бэкап.</summary>
        public static void OnGameSave()
        {
            if (MoprSettings.SaveProtectionOn)
                RemoveReadOnly();

            if (MoprSettings.SaveBackupOn)
                Backup();
        }

        /// <summary>Снимает атрибут «только чтение» с файлов сохранения (best-effort).</summary>
        public static void RemoveReadOnly()
        {
            int cleared = 0;

            foreach (string path in GetSaveFiles())
            {
                try
                {
                    FileInfo info = new FileInfo(path);
                    if (info.IsReadOnly)
                    {
                        info.IsReadOnly = false;
                        cleared++;
                    }
                }
                catch (Exception ex)
                {
                    ExceptionManager.New(ex, false, "SAVE_READONLY_ERROR");
                }
            }

            if (cleared > 0)
                ModConsole.Log("[MOPR] " + LocalizationCore.Get("log.save_readonly_cleared", cleared));
        }

        /// <summary>Копирует текущие файлы сейва в свежую тайм-штампованную папку бэкапа.</summary>
        public static bool Backup()
        {
            try
            {
                string[] files = GetSaveFiles();
                if (files.Length == 0)
                    return false;

                string stamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                string target = Path.Combine(BackupFolder, stamp);
                Directory.CreateDirectory(target);

                foreach (string path in files)
                    File.Copy(path, Path.Combine(target, Path.GetFileName(path)), true);

                Prune();
                ModConsole.Log("[MOPR] " + LocalizationCore.Get("log.save_backup_ok", files.Length, stamp));
                return true;
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, true, "SAVE_BACKUP_ERROR");
                return false;
            }
        }

        /// <summary>Оставляет только самые свежие <see cref="MaxBackups"/> папок бэкапа.</summary>
        private static void Prune()
        {
            try
            {
                if (!Directory.Exists(BackupFolder))
                    return;

                string[] folders = Directory.GetDirectories(BackupFolder);
                if (folders.Length <= MaxBackups)
                    return;

                // Тайм-штампы сортируются хронологически как строки (старые первыми).
                Array.Sort(folders, StringComparer.Ordinal);
                for (int i = 0; i < folders.Length - MaxBackups; i++)
                    Directory.Delete(folders[i], true);
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "SAVE_PRUNE_ERROR");
            }
        }

        /// <summary>Размер (байт) текущего основного файла сейва, или -1 если не читается.</summary>
        public static long CurrentSaveSize()
        {
            try
            {
                string path = Path.Combine(SaveDir, MainSaveFile);
                return File.Exists(path) ? new FileInfo(path).Length : 0;
            }
            catch
            {
                return -1;
            }
        }

        /// <summary>Список бэкапов, свежие первыми.</summary>
        public static List<BackupInfo> ListBackups()
        {
            List<BackupInfo> list = new List<BackupInfo>();
            try
            {
                if (!Directory.Exists(BackupFolder))
                    return list;

                foreach (string dir in Directory.GetDirectories(BackupFolder))
                {
                    long size = 0;
                    string main = Path.Combine(dir, MainSaveFile);
                    if (File.Exists(main))
                        size = new FileInfo(main).Length;

                    string name = Path.GetFileName(dir);
                    if (!DateTime.TryParseExact(name, "yyyy-MM-dd_HH-mm-ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime time))
                        time = Directory.GetCreationTime(dir);

                    list.Add(new BackupInfo(name, dir, size, time));
                }

                list.Sort((a, b) => b.Time.CompareTo(a.Time)); // свежие первыми
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "SAVE_LIST_BACKUPS_ERROR");
            }

            return list;
        }

        /// <summary>Восстанавливает файлы сейва из указанного бэкапа, перезаписывая текущие.</summary>
        public static bool Restore(BackupInfo backup)
        {
            try
            {
                if (backup == null || !Directory.Exists(backup.Folder))
                    return false;

                RemoveReadOnly(); // текущие файлы должны быть записываемыми

                bool any = false;
                foreach (string file in Directory.GetFiles(backup.Folder, "*.txt"))
                {
                    File.Copy(file, Path.Combine(SaveDir, Path.GetFileName(file)), true);
                    any = true;
                }

                if (any)
                    ModConsole.Log("[MOPR] " + LocalizationCore.Get("log.save_restored", backup.Name));

                return any;
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, true, "SAVE_RESTORE_ERROR");
                return false;
            }
        }

        /// <summary>Восстанавливает самый свежий бэкап. false, если бэкапов нет.</summary>
        public static bool RestoreLatest()
        {
            List<BackupInfo> list = ListBackups();
            return list.Count > 0 && Restore(list[0]);
        }

        /// <summary>
        /// Эвристика порчи: true, если текущий сейв пуст или резко уменьшился относительно свежего
        /// бэкапа (и есть что восстанавливать). Настроена так, чтобы не срабатывать на нормальный
        /// рост файла сейва.
        /// </summary>
        public static bool LooksCorrupt(out long currentSize, out long latestBackupSize)
        {
            currentSize = CurrentSaveSize();
            latestBackupSize = 0;

            List<BackupInfo> list = ListBackups();
            if (list.Count > 0)
                latestBackupSize = list[0].SaveSize;

            if (currentSize < 0 || latestBackupSize <= 0)
                return false; // нечего читать или не с чем сравнивать/восстанавливать

            if (currentSize == 0)
                return true;

            // Ужался более чем вдвое относительно последнего бэкапа, с ощутимым запасом.
            return currentSize < latestBackupSize / 2 && (latestBackupSize - currentSize) > 4096;
        }

        /// <summary>Метаданные одной папки бэкапа.</summary>
        public sealed class BackupInfo
        {
            public readonly string Name;
            public readonly string Folder;
            public readonly long SaveSize;
            public readonly DateTime Time;

            public BackupInfo(string name, string folder, long saveSize, DateTime time)
            {
                Name = name;
                Folder = folder;
                SaveSize = saveSize;
                Time = time;
            }
        }
    }
}
