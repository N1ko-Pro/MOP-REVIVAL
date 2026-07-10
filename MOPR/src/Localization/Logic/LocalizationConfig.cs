// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Хранит выбранный язык в отдельном файле в папке настроек мода. Загружается ДО построения окна
// настроек, потому что MSCLoader восстанавливает значения контролов только ПОСЛЕ вызова функции
// настроек — полагаться на сохранённое значение выпадающего списка для языка меню нельзя.

using System;
using System.IO;
using MSCLoader;
using MOPR.Common;

namespace MOPR.Localization
{
    internal static class LocalizationConfig
    {
        private const string FileName = "language.txt";
        private const string RussianToken = "Russian";
        private const string EnglishToken = "English";

        private static string filePath;
        private static bool loaded;

        /// <summary>Разово загружает сохранённый язык. Безопасно вызывать из нескольких хуков.</summary>
        public static void Initialize(Mod mod)
        {
            if (loaded)
                return;
            loaded = true;

            try
            {
                filePath = Path.Combine(ModLoader.GetModSettingsFolder(mod), FileName);
                if (File.Exists(filePath))
                {
                    string token = File.ReadAllText(filePath).Trim();
                    LocalizationCore.Current = token.Equals(RussianToken, StringComparison.OrdinalIgnoreCase)
                        ? Language.Russian
                        : Language.English;
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "LOCALIZATION_LOAD_ERROR");
            }
        }

        /// <summary>Записывает текущий выбор языка на диск.</summary>
        public static void Save()
        {
            if (filePath == null)
                return;

            try
            {
                string directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                File.WriteAllText(filePath, LocalizationCore.Current == Language.Russian ? RussianToken : EnglishToken);
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "LOCALIZATION_SAVE_ERROR");
            }
        }
    }
}
