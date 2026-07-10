// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Контракт объекта, поддерживающего LOD-дубль (упрощённую копию для дальней дистанции).

using MOPR.LOD;

namespace MOPR.Common.Interfaces
{
    internal interface ILod
    {
        void ToggleLOD(bool enabled);
        LodObject LodObject { get; }
    }
}
