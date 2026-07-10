// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Режим оптимизации одного предмета, выбираемый один раз при регистрации (см. ItemModeDecider).

namespace MOPR.Items
{
    internal enum ItemToggleMode
    {
        /// <summary>Полное выключение GameObject — максимум экономии, для инертного хлама.</summary>
        Full,

        /// <summary>Только физика/рендер (GameObject активен) — безопасно для интеракций/FSM.</summary>
        PhysicsOnly
    }
}
