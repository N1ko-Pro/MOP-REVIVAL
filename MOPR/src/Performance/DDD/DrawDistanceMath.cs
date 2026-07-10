// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Чистый расчёт целевой дальности прорисовки (far clip) без зависимостей от Unity. Базой всегда
// служит выбранная игроком дистанция: её лишь модулируют — режут в секторах и (осознанно) поднимают
// на высоте. Выше игровой настройки поднимаемся ТОЛЬКО высотным бустом.

namespace MOPR.Helpers
{
    internal static class DrawDistanceMath
    {
        /// <summary>Вычисляет целевой far clip.</summary>
        /// <param name="gameDrawDistance">Дистанция прорисовки из настроек игры (потолок игрока).</param>
        /// <param name="farBoostApplicable">Применим ли высотный буст (высоко над землёй + пресет Balanced+).</param>
        /// <param name="maxDrawDistance">Максимальная дистанция при высотном бусте.</param>
        /// <param name="inSector">Игрок в замкнутом секторе (помещение).</param>
        /// <param name="sectorDrawDistance">Собственная дистанция активного сектора.</param>
        public static float ComputeTarget(
            float gameDrawDistance,
            bool farBoostApplicable,
            float maxDrawDistance,
            bool inSector,
            float sectorDrawDistance)
        {
            // Высоко над землёй — рисуем далеко, чтобы не срезать горизонт (проверяется первым).
            if (farBoostApplicable)
                return maxDrawDistance > gameDrawDistance ? maxDrawDistance : gameDrawDistance;

            // В секторе вид перекрыт стенами — режем до дистанции сектора, но не выше игровой.
            if (inSector)
                return sectorDrawDistance < gameDrawDistance ? sectorDrawDistance : gameDrawDistance;

            // Обычный случай: ровно то, что выбрал игрок.
            return gameDrawDistance;
        }
    }
}
