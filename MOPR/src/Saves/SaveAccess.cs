// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Null-безопасная обёртка над ES2-сериализацией игры для чтения/записи тегов основного сейва
// (defaultES2File.txt) и сейва предметов (items.txt). Каждое чтение — Try*, возвращает false
// вместо исключения, если тега нет или тип не совпал.

using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MOPR.Saves
{
    internal static class SaveAccess
    {
        private static readonly ES2Settings Settings = new ES2Settings();

        public static string SavePath => Path.Combine(Application.persistentDataPath, "defaultES2File.txt").Replace('\\', '/');
        public static string ItemsPath => Path.Combine(Application.persistentDataPath, "items.txt").Replace('\\', '/');

        public static bool SaveExists => File.Exists(SavePath);

        public static bool Exists(string tag)
        {
            try
            {
                return ES2.Exists(SavePath + "?tag=" + tag, Settings);
            }
            catch
            {
                return false;
            }
        }

        public static bool ItemExists(string tag)
        {
            if (string.IsNullOrEmpty(tag))
                return false;

            try
            {
                return ES2.Exists(ItemsPath + "?tag=" + tag);
            }
            catch
            {
                return false;
            }
        }

        public static bool TryReadBool(string tag, out bool value)
        {
            value = false;
            try
            {
                if (!Exists(tag))
                    return false;

                value = ES2.Load<bool>(SavePath + "?tag=" + tag, Settings);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool TryReadFloat(string tag, out float value)
        {
            value = 0f;
            try
            {
                if (!Exists(tag))
                    return false;

                value = ES2.Load<float>(SavePath + "?tag=" + tag, Settings);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool TryReadStringList(string tag, out List<string> value)
        {
            value = null;
            try
            {
                if (!Exists(tag))
                    return false;

                value = ES2.LoadList<string>(SavePath + "?tag=" + tag, Settings);
                return value != null;
            }
            catch
            {
                return false;
            }
        }

        public static bool TryReadTransform(string tag, out Transform value)
        {
            value = null;
            try
            {
                if (!Exists(tag))
                    return false;

                value = ES2.Load<Transform>(SavePath + "?tag=" + tag, Settings);
                return value != null;
            }
            catch
            {
                return false;
            }
        }

        public static bool TryReadItemTransform(string tag, out Transform value)
        {
            value = null;
            try
            {
                if (!ItemExists(tag))
                    return false;

                value = ES2.Load<Transform>(ItemsPath + "?tag=" + tag);
                return value != null;
            }
            catch
            {
                return false;
            }
        }

        public static bool TryReadItemInt(string tag, out int value)
        {
            value = 0;
            try
            {
                if (!ItemExists(tag))
                    return false;

                value = ES2.Load<int>(ItemsPath + "?tag=" + tag);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void WriteSave<T>(string tag, T value)
        {
            ES2.Save(value, SavePath + "?tag=" + tag, Settings);
        }

        /// <summary>
        /// Записывает список строк. ОБЯЗАТЕЛЬНО отдельно от обобщённого <see cref="WriteSave{T}"/>:
        /// вызов ES2.Save через обобщённый параметр связывается с перегрузкой для одного объекта
        /// (она не сериализует список), а статически типизированный List — с перегрузкой для списков.
        /// </summary>
        public static void WriteSaveList(string tag, List<string> value)
        {
            ES2.Save(value, SavePath + "?tag=" + tag, Settings);
        }

        public static void WriteItem<T>(string tag, T value)
        {
            ES2.Save(value, ItemsPath + "?tag=" + tag);
        }
    }
}
