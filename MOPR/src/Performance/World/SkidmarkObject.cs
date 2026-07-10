// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Следы от шин: постоянно накапливаются и со временем дают утечку памяти. Гасим их полностью в
// профиле Performance либо когда игрок явно включил соответствующую настройку.

using UnityEngine;
using MOPR.Common;
using MOPR.Common.Enumerations;
using MOPR.Common.Interfaces;

namespace MOPR.WorldObjects
{
    internal class SkidmarkObject : GenericObject, IDisableUnderCondition
    {
        public SkidmarkObject(GameObject gameObject, int distance = 200, DisableOn disableOn = DisableOn.Distance)
            : base(gameObject, distance, disableOn)
        {
        }

        /// <summary>Следует ли держать следы выключенными.</summary>
        public bool IsConditionMet()
        {
            return MoprSettings.Mode == PerformanceMode.Performance || MoprSettings.DisableSkidmarksOn;
        }

        public override void Toggle(bool enabled)
        {
            gameObject.SetActive(!IsConditionMet());
        }
    }
}
