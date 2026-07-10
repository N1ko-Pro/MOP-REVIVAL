// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Общий контракт менеджера коллекции управляемых объектов (предметы, транспорт, локации и т.д.).

using System.Collections.Generic;

namespace MOPR.Common.Interfaces
{
    internal interface IManager<T>
    {
        T Add(T obj);
        void Remove(T obj);
        int Count { get; }
        void RemoveAt(int index);
        List<T> GetAll { get; }
        int EnabledCount { get; }
    }
}
