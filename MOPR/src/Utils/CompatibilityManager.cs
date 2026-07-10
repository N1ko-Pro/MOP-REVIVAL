// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Совместимость с другими модами. Здесь описываются только те случаи, что нельзя закрыть
// файлами правил (.mopconfig): рюкзаки CarryMore / AdvancedBackpack (предметы «прячутся» под
// картой либо в спец-точке) и список несовместимых модов-оптимизаторов.

using MSCLoader;
using UnityEngine;
using MOPR.Items;

namespace MOPR.Common
{
    internal static class CompatibilityManager
    {
        // CarryMore прячет предметы глубоко под картой.
        private static bool carryMore;

        // AdvancedBackpack складывает предметы в фиксированную точку.
        private static bool advancedBackpack;
        private static readonly Vector3 AdvancedBackpackPosition = new Vector3(630f, 10f, 1140f);
        private const int AdvancedBackpackDistance = 30;

        private static readonly string[] IncompatibleMods =
        {
            "KruFPS", "ImproveFPS", "OptimizeMSC", "ZZDisableAll", "DisableAll"
        };

        public static bool IsMySummerCar => Application.productName == "My Summer Car";

        public static void Initialize()
        {
            carryMore = ModLoader.IsModPresent("CarryMore");
            advancedBackpack = ModLoader.IsModPresent("AdvancedBackpack");
        }

        /// <summary>Находится ли предмет внутри поддерживаемого мод-рюкзака (тогда его не трогаем).</summary>
        public static bool IsInBackpack(ItemBehaviour behaviour)
        {
            if (carryMore)
                return behaviour.transform.position.y < -900;

            if (advancedBackpack)
                return Vector3.Distance(behaviour.transform.position, AdvancedBackpackPosition) < AdvancedBackpackDistance;

            return false;
        }

        /// <summary>Установлен ли мод, с которым MOPR принципиально несовместим.</summary>
        public static bool IsConfilctingModPresent(out string conflictingModName)
        {
            foreach (string id in IncompatibleMods)
            {
                if (ModLoader.IsModPresent(id))
                {
                    conflictingModName = id;
                    return true;
                }
            }

            conflictingModName = "";
            return false;
        }

        public static bool IsMSCLoader() => GameObject.Find("MSCLoader Canvas menu") != null;
        public static bool IsModLoaderPro() => GameObject.Find("MSCLoader Canvas menu") == null;
    }
}
