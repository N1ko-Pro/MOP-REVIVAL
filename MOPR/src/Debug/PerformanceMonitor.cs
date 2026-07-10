// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Лёгкий безаллокационный профайлер кадров: сглаженный FPS, скользящее окно времён кадра, счётчик
// просадок (спайков) и сборок мусора. Используется экранным монитором диагностики.

using System;
using UnityEngine;

namespace MOPR.DebugTools
{
    internal sealed class PerformanceMonitor
    {
        private const int WindowSize = 120;

        private readonly float[] frameMs = new float[WindowSize];
        private int index;
        private int count;

        private float emaFps;
        private int lastGcGen0 = -1;

        // Спайк — кадр медленнее порога (мс) ИЛИ во столько-то раз медленнее среднего.
        private const float SpikeThresholdMs = 50f;
        private const float SpikeFactor = 2.5f;

        public float Fps => emaFps;
        public float AverageMs { get; private set; }
        public float WorstMs { get; private set; }
        public int SpikeCount { get; private set; }
        public int GcCollections { get; private set; }

        /// <summary>Учитывает один кадр по его нескорректированному времени (Time.unscaledDeltaTime).</summary>
        public void Sample(float unscaledDeltaTime)
        {
            float ms = unscaledDeltaTime * 1000f;

            float fps = unscaledDeltaTime > 0f ? 1f / unscaledDeltaTime : 0f;
            emaFps = emaFps <= 0f ? fps : Mathf.Lerp(emaFps, fps, 0.1f);

            frameMs[index] = ms;
            index = (index + 1) % WindowSize;
            if (count < WindowSize)
                count++;

            float sum = 0f;
            float worst = 0f;
            for (int i = 0; i < count; i++)
            {
                float v = frameMs[i];
                sum += v;
                if (v > worst)
                    worst = v;
            }

            AverageMs = count > 0 ? sum / count : ms;
            WorstMs = worst;

            bool spike = ms > SpikeThresholdMs || (count > 5 && ms > AverageMs * SpikeFactor);
            if (spike)
                SpikeCount++;

            // Считаем сборки мусора нулевого поколения (частая причина микрофризов).
            int gc0 = GC.CollectionCount(0);
            if (lastGcGen0 < 0)
                lastGcGen0 = gc0;
            else if (gc0 != lastGcGen0)
            {
                GcCollections += gc0 - lastGcGen0;
                lastGcGen0 = gc0;
            }
        }
    }
}
