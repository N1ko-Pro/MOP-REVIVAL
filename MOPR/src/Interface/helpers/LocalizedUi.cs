// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Реестр локализуемых элементов настроек: для каждого зарегистрированного контрола хранит способ
// заново применить его текст на текущем языке, чтобы всё меню мгновенно перевелось при смене языка.

using System;
using System.Collections.Generic;
using MSCLoader;
using MOPR.Common;
using MOPR.Localization;

namespace MOPR.Interface.Helpers
{
    internal static class LocalizedUi
    {
        private static readonly List<Action> refreshers = new List<Action>();

        public static void Clear() => refreshers.Clear();

        public static void Header(SettingsHeader header, string key)
        {
            refreshers.Add(() => SettingsReflection.SetHeaderTitle(header, LocalizationCore.Get(key)));
        }

        /// <summary>Цветной жирный подзаголовок секции с акцент-маркером (текст локализуется, цвет — из MoprColors).</summary>
        public static void Subheader(SettingsText text, string key, string hex)
        {
            refreshers.Add(() => text.SetValue(MoprColors.Section(hex, MoprColors.SubheaderContent(LocalizationCore.Get(key)))));
        }

        public static void Label(ModSetting setting, string key)
        {
            refreshers.Add(() => SettingsReflection.SetLabel(setting, LocalizationCore.Get(key)));
        }

        public static void LabelRed(ModSetting setting, string key)
        {
            refreshers.Add(() => SettingsReflection.SetLabel(setting, "<color=red>" + LocalizationCore.Get(key) + "</color>"));
        }

        public static void Button(SettingsButton button, string key)
        {
            refreshers.Add(() => SettingsReflection.SetButtonText(button, LocalizationCore.Get(key)));
        }

        public static void Text(SettingsText text, string key)
        {
            refreshers.Add(() => text.SetValue(LocalizationCore.Get(key)));
        }

        /// <summary>Текст с ключом, вычисляемым заново при каждом обновлении (например, статус сервера).</summary>
        public static void DynamicText(SettingsText text, Func<string> keyFactory)
        {
            refreshers.Add(() => text.SetValue(LocalizationCore.Get(keyFactory())));
        }

        public static void TextArgs(SettingsText text, string key, Func<object[]> argsFactory)
        {
            refreshers.Add(() => text.SetValue(LocalizationCore.Get(key, argsFactory())));
        }

        public static void DistanceSlider(SettingsSliderInt slider, Func<string[]> labelsFactory)
        {
            refreshers.Add(() => SettingsReflection.UpdateSliderTextValues(slider, labelsFactory()));
        }

        public static void RefreshAll()
        {
            for (int i = 0; i < refreshers.Count; i++)
            {
                try
                {
                    refreshers[i]();
                }
                catch
                {
                }
            }
        }
    }
}
