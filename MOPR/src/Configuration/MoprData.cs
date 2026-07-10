// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Модель конфига MOPR, сериализуемого в JSON: версия мода, момент последней синхронизации
// правил и снимок списка модов на тот момент (для определения, изменился ли набор модов).

using System;
using System.Collections.Generic;

namespace MOPR.Common
{
    internal class MoprData
    {
        public string Version = "1.0";
        public DateTime LastTimeUpdate;
        public List<string> LastModList;
    }
}
