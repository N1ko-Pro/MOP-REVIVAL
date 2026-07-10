// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Декларативный каталог статических объектов сцены, которыми управляет MOPR. Вместо длинной
// ручной регистрации сцена описана таблицами, а регистрация выполняется единым проходом.
// Само переключение видимости остаётся за менеджерами и ядром.

using MOPR.Common.Enumerations;
using MOPR.Managers;
using MOPR.WorldObjects;

namespace MOPR.Common
{
    internal static class WorldObjectCatalog
    {
        /// <summary>Табличное описание одного объекта мира.</summary>
        internal struct WorldObjectDef
        {
            public readonly string Name;
            public readonly DisableOn DisableOn;
            public readonly int Distance;
            public readonly int MinimumToggle; // 0 — персональный минимум не задаётся.
            public readonly bool Silent;

            public WorldObjectDef(string name, DisableOn disableOn, int distance = 200, int minimumToggle = 0, bool silent = false)
            {
                Name = name;
                DisableOn = disableOn;
                Distance = distance;
                MinimumToggle = minimumToggle;
                Silent = silent;
            }
        }

        /// <summary>Здания и природа, переключаемые по дистанции до игрока.</summary>
        internal static readonly WorldObjectDef[] Primary =
        {
            new WorldObjectDef("CABIN", DisableOn.Distance, minimumToggle: 300),
            new WorldObjectDef("COTTAGE", DisableOn.Distance, 400, 400),
            new WorldObjectDef("DANCEHALL", DisableOn.Distance, 500),
            new WorldObjectDef("PERAJARVI", DisableOn.Distance | DisableOn.IgnoreInQualityMode, 600, 600),
            new WorldObjectDef("SOCCER", DisableOn.Distance),
            new WorldObjectDef("WATERFACILITY", DisableOn.Distance, 300, 300),
            new WorldObjectDef("StrawberryField", DisableOn.Distance, 400),
            new WorldObjectDef("MAP/Buildings/DINGONBIISI", DisableOn.Distance | DisableOn.IgnoreInBalancedAndAbove, 400),
            new WorldObjectDef("RALLY/PartsSalesman", DisableOn.Distance, 400),
            new WorldObjectDef("LakeSmallBottom1", DisableOn.Distance, 500),
            new WorldObjectDef("machine", DisableOn.Distance, 200, silent: true),
        };

        /// <summary>Объекты, включаемые когда игрок вдали от дома, и гасимые дома.</summary>
        internal static readonly WorldObjectDef[] HomeDependent =
        {
            new WorldObjectDef("NPC_CARS", DisableOn.PlayerInHome),
            new WorldObjectDef("TRAFFIC", DisableOn.PlayerInHome),
            new WorldObjectDef("VehiclesHighway", DisableOn.PlayerInHome | DisableOn.DoNotEnableWhenLeavingHome),
            new WorldObjectDef("VehiclesDirtRoad", DisableOn.PlayerInHome),
            new WorldObjectDef("TRAIN", DisableOn.PlayerInHome | DisableOn.IgnoreInQualityMode),
            new WorldObjectDef("Buildings", DisableOn.PlayerInHome),
            new WorldObjectDef("TrafficSigns", DisableOn.PlayerInHome),
            new WorldObjectDef("StreetLights", DisableOn.PlayerInHome),
            new WorldObjectDef("HUMANS", DisableOn.PlayerInHome),
            new WorldObjectDef("TRACKFIELD", DisableOn.PlayerInHome),
            new WorldObjectDef("SkijumpHill", DisableOn.PlayerInHome | DisableOn.IgnoreInQualityMode),
            new WorldObjectDef("Factory", DisableOn.PlayerInHome),
            new WorldObjectDef("WHEAT", DisableOn.PlayerInHome),
            new WorldObjectDef("RAILROAD", DisableOn.PlayerInHome),
            new WorldObjectDef("AIRPORT", DisableOn.PlayerInHome),
            new WorldObjectDef("RAILROAD_TUNNEL", DisableOn.PlayerInHome),
            new WorldObjectDef("PierDancehall", DisableOn.PlayerInHome),
            new WorldObjectDef("PierRiver", DisableOn.PlayerInHome),
            new WorldObjectDef("PierStore", DisableOn.PlayerInHome),
            new WorldObjectDef("BRIDGE_dirt", DisableOn.PlayerInHome),
            new WorldObjectDef("BRIDGE_highway", DisableOn.PlayerInHome),
            new WorldObjectDef("BirdTower", DisableOn.Distance, 400),
            new WorldObjectDef("RYKIPOHJA", DisableOn.PlayerInHome),
            new WorldObjectDef("COMPUTER", DisableOn.PlayerAwayFromHome, silent: true),
        };

        /// <summary>Мебель Jokke — у неё переключаются только рендереры.</summary>
        internal static readonly string[] JokkeFurniture =
        {
            "tv(Clo01)", "chair(Clo02)", "chair(Clo05)", "bench(Clo01)", "bench(Clo02)",
            "table(Clo02)", "table(Clo03)", "table(Clo04)", "table(Clo05)", "desk(Clo01)", "arm chair(Clo01)"
        };

        /// <summary>Регистрирует все объекты таблицы, при необходимости задавая персональный минимум.</summary>
        internal static void Register(WorldObjectManager manager, WorldObjectDef[] defs)
        {
            foreach (WorldObjectDef def in defs)
            {
                GenericObject obj = manager.Add(def.Name, def.DisableOn, def.Distance, silent: def.Silent);
                if (obj != null && def.MinimumToggle > 0)
                    obj.MinimumToggleDistance = def.MinimumToggle;
            }
        }
    }
}
