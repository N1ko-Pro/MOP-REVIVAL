// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Режим массового переключения всех управляемых объектов сцены.

namespace MOPR.Common.Enumerations
{
    public enum ToggleAllMode
    {
        /// <summary>Обычное переключение по текущим условиям.</summary>
        Default,
        /// <summary>Перед сохранением: всё включается и замораживается.</summary>
        OnSave,
        /// <summary>Во время загрузки сцены.</summary>
        OnLoad
    }
}
