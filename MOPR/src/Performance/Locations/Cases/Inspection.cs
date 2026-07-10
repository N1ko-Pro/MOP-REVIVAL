// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Пункт техосмотра (INSPECTION): переключение безопасных частей по чёрному списку.

namespace MOPR.Places
{
    internal class Inspection : Place
    {
        private readonly string[] blackList =
        {
            "INSPECTION", "BoozeJobTrigger", "Building", "inspection_concrete", "inspection_floor",
            "garage_doors", "glass", "Light", "register plate", "InspectionProcess", "Recipiet", "Order",
            "Audio", "Functions", "DoorWhite", "LOD", "Lifter", "Coll", "Platform"
        };

        public Inspection() : base("INSPECTION")
        {
            GameObjectBlackList.AddRange(blackList);
            DisableableChilds = GetDisableableChilds();
            Compress();
        }
    }
}
