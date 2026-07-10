// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Битовые флаги, описывающие при каких условиях объект мира должен выгружаться и как режим
// производительности модифицирует это поведение.

using System;

namespace MOPR.Common.Enumerations
{
    [Flags]
    public enum DisableOn
    {
        /// <summary>Гасится по дистанции до игрока (значение по умолчанию).</summary>
        Distance = 0,
        /// <summary>Гасится, когда игрок находится дома (во дворе).</summary>
        PlayerInHome = 1,
        /// <summary>Гасится, когда игрок вне дома.</summary>
        PlayerAwayFromHome = 2,
        /// <summary>Не выгружать в режиме Quality и выше — только в Performance/Balanced.</summary>
        IgnoreInQualityMode = 4,
        /// <summary>Всегда использовать базовую (1x) дистанцию, игнорируя множитель профиля.</summary>
        AlwaysUse1xDistance = 8,
        /// <summary>Не включать автоматически при уходе игрока из дома.</summary>
        DoNotEnableWhenLeavingHome = 16,
        /// <summary>Игнорировать в режимах Balanced и выше.</summary>
        IgnoreInBalancedAndAbove = 32
    }
}
