// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Ферма (Farm): переключение безопасных частей по чёрному списку.

namespace MOPR.Places
{
    internal class Farm : Place
    {
        private readonly string[] blackList =
        {
            "Farm", "Job", "combine", "pile", "MachineHall", "TargetHaybales",
            "TargetCombine", "collider", "floor_coll", "hall_base", "SpawnToFarm"
        };

        public Farm() : base("Farm", 300)
        {
            GameObjectBlackList.AddRange(blackList);
            DisableableChilds = GetDisableableChilds();
            Compress();
        }
    }
}
