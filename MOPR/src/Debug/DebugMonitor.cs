// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Экранный монитор диагностики (верхний правый угол), OnGUI. Страницы переключаются стрелками ←/→:
// Performance (FPS/кадр/спайки/GC), MOP (тик, память, счётчики менеджеров), Satsuma (скорость, крен
// блока/головы, зоны), Player (позиция, сектор и его дальность).

using System;
using System.Text;
using UnityEngine;
using MOPR.Common;
using MOPR.Common.Interfaces;
using MOPR.Managers;
using MOPR.Vehicles.Cases;

namespace MOPR.DebugTools
{
    internal sealed class DebugMonitor : MonoBehaviour
    {
        private enum Page { Performance, Mop, Satsuma, Player }

        private const float Width = 320f;
        private const float Margin = 10f;
        private static readonly int PageCount = Enum.GetValues(typeof(Page)).Length;

        private Page page = Page.Performance;
        private GUIStyle style;
        private readonly StringBuilder sb = new StringBuilder(256);
        private readonly PerformanceMonitor perf = new PerformanceMonitor();
        private readonly MemoryTracker memory = new MemoryTracker();

        private Transform satsuma, block, driverHeadPivot;
        private Vector3 lastSatsumaPosition, blockInitRotation, driverHeadPivotRotation;
        private float satsumaVelocity;

        #region Жизненный цикл Unity

        private void Start()
        {
            CacheSatsumaNodes();
        }

        private void Update()
        {
            if (!MoprSettings.IsModActive || !MoprSettings.ShowOverlayOn)
                return;

            perf.Sample(Time.unscaledDeltaTime);
            HandlePageInput();
            TrackSatsumaVelocity();
        }

        private void OnGUI()
        {
            if (!MoprSettings.IsModActive || !MoprSettings.ShowOverlayOn || Core.Instance == null)
                return;

            EnsureStyle();

            string text = BuildText();
            float height = 26f + style.CalcHeight(new GUIContent(text), Width - 16f);
            float x = Screen.width - Width - Margin;

            GUI.Box(new Rect(x, Margin, Width, height), GUIContent.none);
            GUI.Label(new Rect(x + 8f, Margin + 4f, Width - 16f, height - 8f), text, style);
        }

        #endregion

        #region Ввод и сбор данных

        private void HandlePageInput()
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
                page = (Page)(((int)page + 1) % PageCount);
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
                page = (Page)(((int)page + PageCount - 1) % PageCount);
        }

        private void TrackSatsumaVelocity()
        {
            if (satsuma == null || Time.deltaTime <= 0f)
                return;

            satsumaVelocity = (lastSatsumaPosition - satsuma.position).magnitude / Time.deltaTime;
            lastSatsumaPosition = satsuma.position;
        }

        private void CacheSatsumaNodes()
        {
            satsuma = GameObject.Find("SATSUMA(557kg, 248)")?.transform;
            if (satsuma == null)
                return;

            lastSatsumaPosition = satsuma.position;

            block = satsuma.Find("Chassis/sub frame(xxxxx)/CarMotorPivot/block(Clone)");
            if (block != null)
                blockInitRotation = block.localEulerAngles;

            driverHeadPivot = satsuma.Find("DriverHeadPivot");
            if (driverHeadPivot != null)
                driverHeadPivotRotation = driverHeadPivot.localEulerAngles;
        }

        #endregion

        #region Страницы

        private string BuildText()
        {
            sb.Length = 0;
            sb.Append("<b><color=#FF7A1A>[MOPR]</color></b>  ← ").Append(page).AppendLine(" →");

            switch (page)
            {
                case Page.Performance: BuildPerformancePage(); break;
                case Page.Mop: BuildMopPage(); break;
                case Page.Satsuma: BuildSatsumaPage(); break;
                case Page.Player: BuildPlayerPage(); break;
            }

            return sb.ToString().TrimEnd();
        }

        private void BuildPerformancePage()
        {
            Line("FPS", Mathf.RoundToInt(perf.Fps));
            Line("Frame", string.Format("{0:F1} / {1:F1} ms (avg/worst)", perf.AverageMs, perf.WorstMs));
            Line("Spikes", perf.SpikeCount);
            Line("GC collections", perf.GcCollections);
        }

        private void BuildMopPage()
        {
            long gcUsage = GC.GetTotalMemory(false);
            Line("Tick", Core.Instance.Tick);
            Line("GC", gcUsage + " (Δ " + memory.AverageDelta(gcUsage) + ")");
            Line("Items", Describe(ItemsManager.Instance));
            Line("Vehicles", Describe(VehicleManager.Instance));
            Line("WorldObj", Describe(WorldObjectManager.Instance));
            Line("Places", Describe(PlaceManager.Instance));
            Line("Sectors", Describe(SectorManager.Instance));
        }

        private void BuildSatsumaPage()
        {
            if (satsuma == null)
            {
                sb.AppendLine("Satsuma not found.");
                return;
            }

            Line("Velocity", satsumaVelocity.ToString("F1"));
            if (block != null)
                Line("BlockRot", Difference(blockInitRotation, block.localEulerAngles));
            Line("InspectionArea", Satsuma.Instance.IsSatsumaInInspectionArea);
            Line("ParcFerme", Satsuma.Instance.IsSatsumaInParcFerme);
            if (driverHeadPivot != null)
                Line("DriverHeadPivotRot", Difference(driverHeadPivotRotation, driverHeadPivot.localEulerAngles));
        }

        private void BuildPlayerPage()
        {
            Line("PlayerPos", Core.Instance.GetPlayer().position);
            Line("InSector", Core.Instance.IsInSector());
            Line("SectorDrawDistance", SectorManager.Instance.IsPlayerInSector()
                ? SectorManager.Instance.GetCurrentSectorDrawDistance().ToString()
                : "-");
        }

        #endregion

        #region Хелперы

        /// <summary>Добавляет строку «<жёлтый label> value».</summary>
        private void Line(string label, object value)
        {
            sb.Append("<color=yellow>").Append(label).Append("</color> ").Append(value).AppendLine();
        }

        /// <summary>«включено / всего» для менеджера (или «-», если он ещё не создан).</summary>
        private static string Describe<T>(IManager<T> manager)
        {
            return manager == null ? "-" : manager.EnabledCount + " / " + manager.Count;
        }

        private void EnsureStyle()
        {
            if (style != null)
                return;

            style = new GUIStyle(GUI.skin.label) { fontSize = 13, richText = true };
            style.normal.textColor = Color.white;
        }

        private static Vector3 Difference(Vector3 a, Vector3 b)
        {
            return new Vector3(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y), Mathf.Abs(a.z - b.z));
        }

        #endregion

        /// <summary>Скользящее среднее положительного прироста памяти GC между вызовами.</summary>
        private sealed class MemoryTracker
        {
            private readonly long[] deltas = new long[128];
            private int index;
            private long last;

            public long AverageDelta(long current)
            {
                deltas[index] = current - last;
                last = current;
                index = (index + 1) % deltas.Length;

                long sum = 0;
                int divisor = 0;
                foreach (long delta in deltas)
                {
                    if (delta <= 0)
                        continue;
                    sum += delta;
                    divisor++;
                }

                return divisor > 0 ? sum / divisor : 0;
            }
        }
    }
}
