// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Периодически усыпляет дальние неподвижные rigidbody, до которых не добралась штатная оптимизация
// предметов/транспорта (например, тела, добавленные другими модами). Идея — PhysicsOptimizer из
// MSWCOptimization. Трогаем только НЕ кинематические и уже почти неподвижные тела дальше порога —
// такие Unity и сам бы усыпил; мы лишь ускоряем это. Тела с суставами (Joint) не трогаем — их
// пружина не разбудит спящее тело, и шарнирные детали (двери/капот/багажник) перестают открываться.
// Проход редкий (интервал), поэтому дорогой FindObjectsOfType не бьёт по кадру. Пропускается, если
// установлен MSWCOptimization (делает то же).

using UnityEngine;
using MSCLoader;
using MOPR.Common;
using MOPR.Common.Enumerations;

namespace MOPR.Performance.Optimizers
{
    internal sealed class RigidbodySleeper : MonoBehaviour
    {
        private const float BaseIntervalSeconds = 6f;
        private const float PerformanceIntervalSeconds = 3f;
        private const float BaseFarDistance = 200f;
        private const float MinFarDistance = 120f;
        private const float StillSqrThreshold = 0.01f; // (0.1 м/с)^2

        private readonly ModuleFailsafe guard = new ModuleFailsafe("RIGIDBODY_SLEEPER_ERROR");
        private float timer;
        private bool skip;

        private void Awake()
        {
            skip = ModLoader.IsModPresent("MSWCOptimization");
            if (skip)
                ModConsole.Log("[MOPR] Rigidbody sleeper skipped: MSWCOptimization is installed.");
        }

        private void Update()
        {
            if (skip || !MoprSettings.IsModActive || !MoprSettings.SleepDistantBodiesOn)
                return;

            float interval = MoprSettings.Mode == PerformanceMode.Performance ? PerformanceIntervalSeconds : BaseIntervalSeconds;
            timer += Time.unscaledDeltaTime;
            if (timer < interval)
                return;

            timer = 0f;
            guard.Run(Scan);
        }

        private void Scan()
        {
            Transform player = Core.Instance != null ? Core.Instance.GetPlayer() : null;
            if (player == null)
                return;

            float far = Mathf.Max(BaseFarDistance * MoprSettings.ActiveDistanceMultiplicationValue, MinFarDistance);
            float farSqr = far * far;
            Vector3 origin = player.position;

            Object[] found = Object.FindObjectsOfType(typeof(Rigidbody));
            for (int i = 0; i < found.Length; i++)
            {
                Rigidbody rb = found[i] as Rigidbody;
                if (rb == null || rb.isKinematic || rb.IsSleeping())
                    continue;

                Vector3 delta = rb.transform.position - origin;
                if (delta.sqrMagnitude <= farSqr)
                    continue;

                // Тела с суставами (Joint) не усыпляем: пружина HingeJoint не будит спящее тело,
                // поэтому шарнирные детали (двери/капот/багажник) перестали бы открываться.
                if (rb.GetComponent<Joint>() != null)
                    continue;

                if (rb.velocity.sqrMagnitude < StillSqrThreshold && rb.angularVelocity.sqrMagnitude < StillSqrThreshold)
                    rb.Sleep();
            }
        }
    }
}
