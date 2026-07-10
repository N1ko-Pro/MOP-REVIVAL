// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Вешается на меню паузы: собирает мусор при открытии и закрытии меню (удобный момент — игра и так
// «замерла», лишний фриз незаметен).

using System;
using UnityEngine;

namespace MOPR.Helpers
{
    internal class MopPauseMenuHandler : MonoBehaviour
    {
        private void OnEnable() => GC.Collect();
        private void OnDisable() => GC.Collect();
    }
}
