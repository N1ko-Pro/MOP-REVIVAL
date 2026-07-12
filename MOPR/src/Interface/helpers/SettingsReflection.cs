// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Доступ через рефлексию к внутренностям UI настроек MSCLoader для того, чего нет в публичном API:
// сдвинуть ползунок с корректной подписью и живьём переименовать контролы/заголовки при смене языка.
// Каждый вызов защищён try/catch и превращается в no-op, если UI ещё не построен.

using System.Reflection;
using MSCLoader;
using UnityEngine;
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

        /// <summary>
        /// Прячет подпись контрола (settingName) и схлопывает её строку — убирает пустой отступ
        /// (например, у дропдауна с пустым именем). Возвращает false, если UI-элемент ещё не создан
        /// (нужно повторить позже — MSCLoader инстанцирует элементы при открытии страницы настроек).
        /// </summary>
        public static bool HideLabel(ModSetting setting)
        {
            try
            {
                object element = GetSettingsElement(setting);
                if (element == null)
                    return false;

                Text label = GetUiField<Text>(element, "settingName");
                if (label == null)
                    return false;

                GameObject go = label.gameObject;

                // Схлопываем строку: нулевая высота через LayoutElement (на случай layout-группы) и
                // отключение объекта (исключает из компоновки родителя).
                RectTransform rt = go.GetComponent<RectTransform>();
                if (rt != null)
                    rt.sizeDelta = new Vector2(rt.sizeDelta.x, 0f);

                LayoutElement le = go.GetComponent<LayoutElement>();
                if (le == null)
                    le = go.AddComponent<LayoutElement>();
                le.minHeight = 0f;
                le.preferredHeight = 0f;

                go.SetActive(false);
                return true;
            }
            catch
            {
                return true; // не зацикливаемся на ошибке
            }
        }

        /// <summary>
        /// Размещает две кнопки на одной строке: оборачивает их в горизонтальный layout и делит ширину
        /// поровну. Возвращает false, если UI-элементы ещё не созданы (нужно повторить позже).
        /// </summary>
        public static bool PairButtons(ModSetting a, ModSetting b)
        {
            try
            {
                Component ea = GetSettingsElement(a) as Component;
                Component eb = GetSettingsElement(b) as Component;
                if (ea == null || eb == null)
                    return false;

                RectTransform ra = ea.GetComponent<RectTransform>();
                RectTransform rb = eb.GetComponent<RectTransform>();
                if (ra == null || rb == null)
                    return false;

                // Уже объединены (кнопка перенесена в строку) — выходим.
                if (ra.parent != null && ra.parent.name == "MOPR_ButtonRow")
                    return true;

                Transform content = ra.parent;
                if (content == null)
                    return false;

                float height = Mathf.Max(ra.rect.height, 30f);
                int index = ra.GetSiblingIndex();

                GameObject rowObj = new GameObject("MOPR_ButtonRow", typeof(RectTransform));
                RectTransform row = (RectTransform)rowObj.transform;
                row.SetParent(content, false);
                row.SetSiblingIndex(index);

                HorizontalLayoutGroup hlg = rowObj.AddComponent<HorizontalLayoutGroup>();
                hlg.childForceExpandWidth = true;
                hlg.childForceExpandHeight = true;
                hlg.spacing = 6f;

                LayoutElement le = rowObj.AddComponent<LayoutElement>();
                le.minHeight = height;
                le.preferredHeight = height;

                ra.SetParent(row, false);
                rb.SetParent(row, false);
                return true;
            }
            catch
            {
                return true; // не зацикливаемся на ошибке
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
