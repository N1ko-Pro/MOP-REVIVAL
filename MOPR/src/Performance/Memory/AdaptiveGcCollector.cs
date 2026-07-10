// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Адаптивная сборка мусора (идея — GarbageCollectionOptimizer из MSWCOptimization). Раз в
// CheckInterval проверяет прирост управляемой памяти и вызывает GC.Collect ТОЛЬКО при заметном
// давлении (> порога) — это сглаживает микрофризы от стихийных сборок, не собирая мусор без нужды.
// Пропускается, если установлен MSWCOptimization (делает то же).

using System;
using UnityEngine;
using MSCLoader;
using MOPR.Common;

namespace MOPR.Performance.Optimizers
{
    internal sealed class AdaptiveGcCollector : MonoBehaviour
    {
        private const float CheckIntervalSeconds = 30f;
        private const long PressureThresholdBytes = 48L * 1024L * 1024L; // 48 МБ прироста

        private readonly ModuleFailsafe guard = new ModuleFailsafe("ADAPTIVE_GC_ERROR");
        private float timer;
        private long lastMemory;
        private bool skip;

        private void Awake()
        {
            skip = ModLoader.IsModPresent("MSWCOptimization");
            if (skip)
                ModConsole.Log("[MOPR] Adaptive GC skipped: MSWCOptimization is installed.");

            lastMemory = GC.GetTotalMemory(false);
        }

        private void Update()
        {
            if (skip || !MoprSettings.IsModActive || !MoprSettings.AdaptiveGcOn)
                return;

            timer += Time.unscaledDeltaTime;
            if (timer < CheckIntervalSeconds)
                return;

            timer = 0f;
            guard.Run(Check);
        }

        private void Check()
        {
            long memory = GC.GetTotalMemory(false);
            long delta = memory - lastMemory;
            lastMemory = memory;

            if (delta <= PressureThresholdBytes)
                return;

            GC.Collect();
            lastMemory = GC.GetTotalMemory(false);
        }
    }
}
