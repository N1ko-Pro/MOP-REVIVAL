// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Самый простой способ переключения объекта мира — полное включение/выключение GameObject.
// Учитывает флаги, запрещающие выгрузку в более «качественных» профилях.

using UnityEngine;
using MOPR.Common;
using MOPR.Common.Enumerations;

namespace MOPR.WorldObjects
{
    internal class SimpleObjectToggle : GenericObject
    {
        public SimpleObjectToggle(GameObject gameObject, DisableOn disableOn = DisableOn.Distance, int distance = 200)
            : base(gameObject, distance, disableOn)
        {
        }

        public override void Toggle(bool enabled)
        {
            // В «качественных» профилях часть объектов запрещено гасить — принудительно включаем.
            if (DisableOn.HasFlag(DisableOn.IgnoreInQualityMode) && MoprSettings.Mode == PerformanceMode.Quality)
                enabled = true;

            if (DisableOn.HasFlag(DisableOn.IgnoreInBalancedAndAbove) && MoprSettings.Mode >= PerformanceMode.Balanced)
                enabled = true;

            if (gameObject != null && gameObject.activeSelf != enabled)
                gameObject.SetActive(enabled);
        }
    }
}
