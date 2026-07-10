// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Правила дистанции: решения «должен ли объект быть включён» по расстоянию до игрока. Базовый порог
// масштабируется пользовательским множителем Active Distance и прижимается к минимальному «полу»,
// ниже которого объекты не выгружаются.

using UnityEngine;
using MOPR.Common;
using MOPR.Common.Enumerations;
using MOPR.WorldObjects;

namespace MOPR
{
    internal partial class Core : MonoBehaviour
    {
        #region Пороги дистанции (в метрах)

        private const int MinimumItemDistance = 10;          // «пол» для предметов в секторе
        private const int MinimumItemOutsideDistance = 30;   // «пол» для предметов вне сектора
        private const int DefaultItemToggleDistance = 75;    // базовый порог включения предмета

        private const int MinimumVehicleDistance = 20;         // «пол» для транспорта в секторе
        private const int MinimumVehicleOutsideDistance = 50;  // «пол» для транспорта вне сектора
        private const int DefaultVehicleDistance = 125;        // базовый порог включения транспорта
        private const int PerformanceModeVehicleDistance = 30; // ужатый порог в режиме Performance

        private const int MinimumPlaceDistance = 175;          // «пол» для локаций

        #endregion

        /// <summary>Текущий множитель дистанции из настроек Active Distance.</summary>
        private static float DistanceMultiplier => MoprSettings.ActiveDistanceMultiplicationValue;

        /// <summary>Применяет множитель к порогу и прижимает результат к минимальному «полу».</summary>
        private static float ScaleAndClamp(float baseDistance, float floor)
        {
            return Mathf.Max(baseDistance * DistanceMultiplier, floor);
        }

        /// <summary>Включён ли предмет: в секторе порог ужимается до 20 м, «пол» ниже.</summary>
        private bool IsEnabled(float distance, float toggleDistance = DefaultItemToggleDistance)
        {
            // Порог 0 означает «никогда не включать».
            if (toggleDistance == 0)
                return false;

            if (inSectorMode)
                toggleDistance = 20;

            float floor = inSectorMode ? MinimumItemDistance : MinimumItemOutsideDistance;
            return distance < ScaleAndClamp(toggleDistance, floor);
        }

        /// <summary>Включён ли объект мира с учётом персональных флагов и минимума.</summary>
        private bool IsGenericObjectEnabled(GenericObject obj)
        {
            // Порог -1 означает «никогда не включать».
            if (obj.Distance == -1)
                return false;

            float toggleDistance = obj.Distance;

            // Коэффициент ужатия вблизи: 0.5x при нулевой настройке Active Distance, иначе 0.1x.
            float squeeze = MoprSettings.ActiveDistanceValue <= 0 ? 0.5f : 0.1f;

            // Флаг AlwaysUse1xDistance игнорирует пользовательский множитель.
            if (obj.DisableOn.HasFlag(DisableOn.AlwaysUse1xDistance))
            {
                toggleDistance *= squeeze;
                return DistanceToPlayer(obj) < toggleDistance;
            }

            if (inSectorMode)
                toggleDistance *= squeeze;

            toggleDistance *= DistanceMultiplier;

            // Вне сектора не опускаемся ниже персонального минимума объекта.
            if (!inSectorMode && toggleDistance < obj.MinimumToggleDistance)
                toggleDistance = obj.MinimumToggleDistance;

            return DistanceToPlayer(obj) < toggleDistance;
        }

        /// <summary>Включён ли транспорт: в секторе — минимум, в Performance — ужатый порог.</summary>
        private bool IsVehicleEnabled(float distance, float toggleDistance = DefaultVehicleDistance)
        {
            if (inSectorMode)
                toggleDistance = MinimumVehicleDistance;
            else if (MoprSettings.Mode == PerformanceMode.Performance)
                toggleDistance = PerformanceModeVehicleDistance;

            float floor = inSectorMode ? MinimumVehicleDistance : MinimumVehicleOutsideDistance;
            return distance < ScaleAndClamp(toggleDistance, floor);
        }

        /// <summary>Включать ли физику транспорта: прямое сравнение с порогом, без множителя.</summary>
        private bool IsVehiclePhysicsEnabled(float distance, float toggleDistance = 200)
        {
            return distance < toggleDistance;
        }

        /// <summary>Включена ли локация: порог масштабируется и прижимается к «полу» 175 м.</summary>
        private bool IsPlaceEnabled(Transform target, float toggleDistance = 200)
        {
            return Vector3.Distance(player.transform.position, target.position)
                   < ScaleAndClamp(toggleDistance, MinimumPlaceDistance);
        }

        /// <summary>Дистанция от игрока до объекта мира.</summary>
        private float DistanceToPlayer(GenericObject obj)
        {
            return Vector3.Distance(player.transform.position, obj.transform.position);
        }
    }
}
