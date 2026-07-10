// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Контракт объекта мира, выгружаемого только при выполнении собственного условия.

namespace MOPR.Common.Interfaces
{
    internal interface IDisableUnderCondition
    {
        bool IsConditionMet();
    }
}
