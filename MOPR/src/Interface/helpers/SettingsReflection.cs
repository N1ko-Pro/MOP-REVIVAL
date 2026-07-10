// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Доступ через рефлексию к внутренностям UI настроек MSCLoader для того, чего нет в публичном API:
// сдвинуть ползунок с корректной подписью и живьём переименовать контролы/заголовки при смене языка.
// Каждый вызов защищён try/catch и превращается в no-op, если UI ещё не построен.

using System.Reflection;
using MSCLoader;
using UnityEngine.UI;

namespace MOPR.Interface.Helpers
{
    internal static class SettingsReflection
    {
        private const BindingFlags InstanceAny = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private static readonly FieldInfo SettingsElementField = typeof(ModSetting).GetField("SettingsElement", InstanceAny);
        private static readonly FieldInfo HeaderElementField = typeof(ModSetting).GetField("HeaderElement", InstanceAny);
        private static readonly FieldInfo NameField = typeof(ModSetting).GetField("Name", InstanceAny);
        private static readonly FieldInfo TextValuesField = typeof(SettingsSliderInt).GetField("TextValues", InstanceAny);

        /// <summary>
        /// Задаёт подпись контрола. Пишется и в ModSetting.Name, чтобы текст пережил перестройку меню
        /// (MSCLoader пересоздаёт элементы из Name при каждом открытии страницы).
        /// </summary>
        public static void SetLabel(ModSetting setting, string text)
        {
            SetName(setting, text);
            try
            {
                Text label = GetUiField<Text>(GetSettingsElement(setting), "settingName");
                if (label != null)
                    label.text = text;
            }
            catch
            {
            }
        }

        /// <summary>Задаёт подпись кнопки (MSCLoader показывает её в верхнем регистре).</summary>
        public static void SetButtonText(ModSetting button, string text)
        {
            SetName(button, text);
            try
            {
                Text label = GetUiField<Text>(GetSettingsElement(button), "settingName");
                if (label != null)
                    label.text = text.ToUpper();
            }
            catch
            {
            }
        }

        /// <summary>Задаёт заголовок группы (показывается в верхнем регистре).</summary>
        public static void SetHeaderTitle(ModSetting header, string text)
        {
            SetName(header, text);
            try
            {
                object headerGroup = HeaderElementField != null ? HeaderElementField.GetValue(header) : null;
                Text title = GetUiField<Text>(headerGroup, "HeaderTitle");
                if (title != null)
                    title.text = text.ToUpper();
            }
            catch
            {
            }
        }

        private static void SetName(ModSetting setting, string text)
        {
            try
            {
                if (NameField != null)
                    NameField.SetValue(setting, text);
            }
            catch
            {
            }
        }

        /// <summary>Программно ставит значение чекбокса через Unity Toggle (срабатывает слушатель MSCLoader).</summary>
        public static void SetToggle(ModSetting setting, bool value)
        {
            try
            {
                Toggle toggle = GetUiField<Toggle>(GetSettingsElement(setting), "checkBox");
                if (toggle != null)
                    toggle.isOn = value;
            }
            catch
            {
            }
        }

        /// <summary>Двигает ползунок через Unity Slider (срабатывает слушатель MSCLoader — «пресет» ползунка).</summary>
        public static void MoveSlider(ModSetting slider, int value)
        {
            try
            {
                Slider unitySlider = GetUiField<Slider>(GetSettingsElement(slider), "slider");
                if (unitySlider != null)
                    unitySlider.value = value;
            }
            catch
            {
            }
        }

        /// <summary>Меняет текстовые подписи ползунка и обновляет показанное значение.</summary>
        public static void UpdateSliderTextValues(SettingsSliderInt slider, string[] textValues)
        {
            try
            {
                if (TextValuesField != null)
                    TextValuesField.SetValue(slider, textValues);

                Text valueText = GetUiField<Text>(GetSettingsElement(slider), "value");
                int index = slider.GetValue();
                if (valueText != null && textValues != null && index >= 0 && index < textValues.Length)
                    valueText.text = textValues[index];
            }
            catch
            {
            }
        }

        private static object GetSettingsElement(ModSetting setting)
        {
            return SettingsElementField != null ? SettingsElementField.GetValue(setting) : null;
        }

        private static T GetUiField<T>(object instance, string fieldName) where T : class
        {
            if (instance == null)
                return null;

            FieldInfo field = instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return field != null ? field.GetValue(instance) as T : null;
        }
    }
}
