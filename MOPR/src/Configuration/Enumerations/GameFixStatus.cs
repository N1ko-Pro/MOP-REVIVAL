// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Состояние разового перезапуска сцены — применяется, когда игра не догрузила Сатсуму.

namespace MOPR.Common.Enumerations
{
    internal enum GameFixStatus
    {
        /// <summary>Перезапуск не требуется.</summary>
        None,
        /// <summary>Нужно выполнить перезапуск сцены при следующем заходе в меню.</summary>
        DoFix,
        /// <summary>Перезапуск уже был выполнен.</summary>
        Restarted
    }
}
