// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Способ, которым конкретный объект скрывается/показывается при оптимизации.

namespace MOPR.Common.Enumerations
{
    public enum ToggleModes
    {
        /// <summary>Полное включение/выключение через GameObject.SetActive.</summary>
        Simple,
        /// <summary>Скрытие единственного Renderer (объект остаётся активным).</summary>
        Renderer,
        /// <summary>Скрытие нескольких Renderer'ов.</summary>
        MultipleRenderers,
        /// <summary>Обрабатывается как предмет (ItemBehaviour).</summary>
        Item,
        /// <summary>Обрабатывается как транспорт целиком.</summary>
        Vehicle,
        /// <summary>Обрабатывается только физика транспорта.</summary>
        VehiclePhysics
    }
}
