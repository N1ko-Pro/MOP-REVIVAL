// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Обнаруженная проблема сейва вместе с действием, которое её исправляет.

using System;

namespace MOPR.Saves
{
    internal sealed class SaveIssue
    {
        public readonly string Name;
        public readonly Action Fix;

        public SaveIssue(string name, Action fix)
        {
            Name = name;
            Fix = fix;
        }
    }
}
