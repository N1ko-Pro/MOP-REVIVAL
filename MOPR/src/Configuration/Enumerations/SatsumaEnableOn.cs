// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Условие включения элемента Сатсумы в зависимости от уровня детализации.

namespace MOPR.Common.Enumerations
{
    public enum SatsumaEnableOn
    {
        /// <summary>Включать при заведённом двигателе.</summary>
        OnEngine,
        /// <summary>Включать, когда игрок рядом.</summary>
        OnPlayerClose,
        /// <summary>Включать, когда игрок далеко.</summary>
        OnPlayerFar
    }
}
