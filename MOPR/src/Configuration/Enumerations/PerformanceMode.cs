// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Профиль агрессивности оптимизации. Порядок значений идёт по возрастанию качества картинки
// (и убыванию FPS), поэтому сравнения вида (Mode >= Quality) корректны.

namespace MOPR.Common.Enumerations
{
    public enum PerformanceMode
    {
        /// <summary>Максимум FPS: объекты выгружаются на минимальной дистанции.</summary>
        Performance,
        /// <summary>Компромисс между FPS и дальностью прорисовки.</summary>
        Balanced,
        /// <summary>Больше дальность прорисовки, меньше «всплывания» объектов.</summary>
        Quality,
        /// <summary>Максимальная дальность — минимум выгрузок.</summary>
        UltraQuality
    }
}
