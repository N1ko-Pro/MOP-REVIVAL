// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Изоляция сбоёв: единая обёртка, выполняющая действие и превращающая любое исключение в запись
// через ExceptionManager, не роняя остальную инициализацию. Пустые catch не используются.

using System;
using MOPR.Common;

namespace MOPR
{
    internal partial class Core
    {
        /// <summary>Выполняет действие, логируя исключение под тегом <paramref name="tag"/>.</summary>
        private static void Guard(string tag, Action action, bool fatal = false)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, fatal, tag);
            }
        }
    }
}
