// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Полноэкранный загрузочный оверлей, пока MOP инвентаризует сцену. Показом/скрытием управляет Core
// (Activate/Deactivate). Рисуется через IMGUI и не требует ассет-бандла: анимированный логотип — два
// кадра, вшитые в DLL как ресурсы. Игрока замораживает сам Core; здесь только отрисовка и курсор.

using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using MOPR.Localization;

namespace MOPR.Common
{
    internal class LoadScreen : MonoBehaviour
    {
        private const float DotInterval = 0.4f;    // темп анимации точек «...»
        private const float FrameInterval = 0.45f; // темп смены кадров логотипа

        private Texture2D background;
        private Texture2D[] frames;
        private GUIStyle titleStyle;
        private GUIStyle textStyle;
        private GUIStyle brandStyle;

        private bool doDisplay;
        private float dotTimer;
        private int dots;
        private float frameTimer;
        private int frame;

        private string subtitle;
        private bool animateDots;

        // Первоапрельская абракадабра (намеренная бессмыслица, не локализуется).
        private static readonly string[] AprilFools =
        {
            "MOADING LOP...", "LOADINP MOG...", "...POM GNIDAOL", "LDNG MP...",
            "WOADING MOPR UWU", "MOPR?", "REVIVING MOPR...", "UNLOADING MOPR...",
        };

        private void Awake()
        {
            background = new Texture2D(1, 1);
            // Почти непрозрачный чёрный — сильнее затемняет сцену под загрузочным экраном.
            background.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.99f));
            background.Apply();

            frames = new[]
            {
                LoadEmbedded("MOPR.loadScreen1.png"),
                LoadEmbedded("MOPR.loadScreen2.png"),
            };

            ChooseSubtitle();
        }

        /// <summary>Показать загрузочный экран.</summary>
        public void Activate()
        {
            doDisplay = true;
            enabled = true;
            gameObject.SetActive(true);
            Cursor.visible = false;
        }

        /// <summary>Скрыть загрузочный экран и освободить объект.</summary>
        public void Deactivate()
        {
            doDisplay = false;
            enabled = false;
            Cursor.visible = true;
            Destroy(gameObject);
        }

        private void Update()
        {
            if (!doDisplay)
                return;

            dotTimer += Time.unscaledDeltaTime;
            if (dotTimer >= DotInterval)
            {
                dotTimer = 0f;
                dots = (dots + 1) % 4;
            }

            frameTimer += Time.unscaledDeltaTime;
            if (frameTimer >= FrameInterval)
            {
                frameTimer = 0f;
                frame ^= 1;
            }
        }

        private void OnGUI()
        {
            if (!doDisplay)
                return;

            GUI.depth = -1000; // рисуемся поверх всего остального

            EnsureStyles();

            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), background, ScaleMode.StretchToFill);

            // Крупная надпись бренда сверху экрана.
            brandStyle.fontSize = Mathf.Clamp(Mathf.RoundToInt(Screen.height * 0.06f), 28, 96);
            GUI.Label(new Rect(0f, Screen.height * 0.10f, Screen.width, Screen.height * 0.14f),
                "<b><color=" + MoprColors.SettingsBrand + ">MOP</color> - REVIVAL</b>", brandStyle);

            string caption = subtitle;
            if (animateDots)
                caption += new string('.', dots);

            Texture2D logo = frames != null ? frames[frame] : null;
            if (logo == null && frames != null)
                logo = frames[frame ^ 1]; // если один кадр не загрузился — берём другой

            if (logo != null)
            {
                float logoSize = Mathf.Min(Screen.height * 0.42f, Screen.width * 0.6f);
                float logoX = (Screen.width - logoSize) * 0.5f;
                float logoY = Screen.height * 0.5f - logoSize * 0.62f;
                GUI.DrawTexture(new Rect(logoX, logoY, logoSize, logoSize), logo, ScaleMode.ScaleToFit);

                GUI.Label(new Rect(0f, logoY + logoSize + 6f, Screen.width, 60f), caption, textStyle);
            }
            else
            {
                float centreY = Screen.height * 0.5f;
                GUI.Label(new Rect(0f, centreY - 60f, Screen.width, 60f), "<b><color=#FF7A1A>MOP</color> Revival</b>", titleStyle);
                GUI.Label(new Rect(0f, centreY + 12f, Screen.width, 60f), caption, textStyle);
            }
        }

        private void OnDestroy()
        {
            if (background != null)
                Destroy(background);

            if (frames != null)
            {
                for (int i = 0; i < frames.Length; i++)
                    if (frames[i] != null)
                        Destroy(frames[i]);
            }
        }

        /// <summary>Грузит PNG, вшитый в DLL мода, как Texture2D (null при ошибке).</summary>
        private static Texture2D LoadEmbedded(string resourceName)
        {
            try
            {
                Assembly asm = Assembly.GetExecutingAssembly();
                using (Stream stream = asm.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                        return null;

                    byte[] data = new byte[stream.Length];
                    int offset = 0;
                    int read;
                    while (offset < data.Length && (read = stream.Read(data, offset, data.Length - offset)) > 0)
                        offset += read;

                    Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    texture.LoadImage(data);
                    texture.filterMode = FilterMode.Bilinear;
                    return texture;
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>Выбирает подпись загрузки: обычно «оптимизация», редко — пасхалка, 1 апреля — абракадабра.</summary>
        private void ChooseSubtitle()
        {
            DateTime today = DateTime.Today;
            if (today.Day == 1 && today.Month == 4)
            {
                subtitle = AprilFools[UnityEngine.Random.Range(0, AprilFools.Length)];
                animateDots = false;
                return;
            }

            if (UnityEngine.Random.Range(0, 100) == 0)
            {
                subtitle = LocalizationCore.Get("load.easter"); // 1% добрая пасхалка
                animateDots = false;
                return;
            }

            subtitle = LocalizationCore.Get("load.optimizing");
            animateDots = true;
        }

        private void EnsureStyles()
        {
            if (textStyle != null)
                return;

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 44,
                richText = true,
            };
            titleStyle.normal.textColor = Color.white;

            brandStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 56,
                fontStyle = FontStyle.Bold,
                richText = true,
            };
            brandStyle.normal.textColor = Color.white;

            textStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 32,
                fontStyle = FontStyle.Bold,
                richText = true,
            };
            textStyle.normal.textColor = new Color(0.85f, 0.88f, 0.92f, 1f);
        }
    }
}
