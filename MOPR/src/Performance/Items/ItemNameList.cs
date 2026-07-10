// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Белый список подбираемых предметов (перенос из оригинального MOP) и маркеры экземпляров. Единый
// источник имён для регистратора и сканера: имя должно быть в списке И нести суффикс экземпляра
// ((itemx)/(Clone)/(xxxxx)), либо быть одним из «безмаркерных» колёс.

namespace MOPR.Items
{
    internal static class ItemNameList
    {
        /// <summary>Суффиксы, которые MSC вешает на экземпляры предметов.</summary>
        public static readonly string[] Markers = { "(itemx)", "(Clone)", "(xxxxx)" };

        /// <summary>Подбираемые предметы без суффикса, которыми всё равно управляем (колёса).</summary>
        public static readonly string[] Markerless = { "wheel_regula", "wheel_offset" };

        /// <summary>Белый список базовых имён предметов (перенос из оригинального MOP).</summary>
        public static readonly string[] Names =
        {
            "airfilter", "alternator", "alternator belt", "amplifier", "antenna", "ax",
            "back panel", "basketball", "battery", "beer case", "berry box",
            "block", "bootlid", "booze", "box", "brake fluid",
            "brake lining", "brake master cylinder", "bucket", "bucket lid", "bucket seat driver",
            "bucket seat passenger", "bumper front", "bumper rear", "camera", "camshaft",
            "camshaft gear", "car jack", "carburator", "cd player", "center console gt",
            "cigarettes", "clock gauge", "clutch cover plate", "clutch disc", "clutch lining",
            "clutch master cylinder", "clutch pressure plate", "coffee cup", "coffee pan", "coil spring",
            "coolant", "crankshaft", "crankshaft pulley", "cylinder head", "dash cover leopard",
            "dash cover plush", "dash cover suomi", "dash cover zebra", "dashboard", "dashboard meters",
            "diesel", "digging bar", "dipper", "disc brake", "diskette",
            "distributor", "door left", "door right", "drive gear", "drum brake",
            "electrics", "empty", "empty plastic can", "engine plate", "exhaust dual tip",
            "exhaust muffler", "exhaust pipe", "extra gauges", "fender flare fl", "fender flare fr",
            "fender flare rl", "fender flare rr", "fender flare spoiler", "fender flares", "fender left",
            "fender right", "fiberglass hood", "fire extinguisher", "fire extinguisher holder", "firewood",
            "fireworks bag", "fish trap", "flashlight", "floor jack", "flywheel",
            "football", "front spoiler", "fuel mixture gauge", "fuel pump", "fuel strainer",
            "fuel tank", "fuel tank pipe", "fur dices", "garbage barrel", "gasoline",
            "gear linkage", "gear stick", "gearbox", "grill", "grill charcoal",
            "grille", "grille gt", "grilled pike", "ground coffee", "gt steering wheel",
            "halfshaft", "handbrake", "head gasket", "headers", "headlight left",
            "headlight right", "helmet", "hood", "hubcap", "inspection cover",
            "juice", "lantern", "light bulb",
            "log", "long coil spring", "main bearing1", "main bearing2", "main bearing3",
            "marker light left", "marker light right", "marker lights", "mosquito spray", "motor hoist",
            "motor oil", "mudflap fl", "mudflap fr", "mudflap rl", "mudflap rr",
            "n2o bottle", "n2o bottle holder", "n2o button panel", "n2o injectors", "n2o kit",
            "notepad", "oil filter", "oilpan", "parts magazine", "piston1",
            "piston2", "piston3", "piston4", "racing carburators", "racing exhaust",
            "racing flywheel", "racing harness", "racing muffler", "racing radiator", "radar buster",
            "radiator", "radiator hose1", "radiator hose2", "radiator hose3", "radio",
            "rally coil spring", "rally shock absorber", "rally steering wheel", "rally strut fl", "rally strut fr",
            "rally suspension kit", "ratchet set", "rear light left", "rear light right", "rear spoiler",
            "rear spoiler2", "register plate", "rocker cover", "rocker cover gt", "rocker shaft",
            "rpm gauge", "ruler", "sausages", "screwdriver", "seat cover leopard",
            "seat cover plush", "seat cover suomi", "seat cover zebra", "seat driver", "seat passenger",
            "seat rear", "shock absorber", "shopping bag", "side skirt left", "side skirt right",
            "sledgehammer", "sofa", "spanner set",
            "spark plug", "spark plug box", "sparkplug socket", "sparkplug wrench", "spindle fl",
            "spindle fr", "spirit", "sport steering wheel", "spray can", "starter",
            "steel headers", "steering column", "steering rack", "steering rod fl", "steering rod fr",
            "stock steering wheel", "strut fl", "strut fr", "sub frame", "subwoofer left",
            "subwoofer panel", "subwoofer right", "subwoofers", "sugar", "suitcase",
            "table", "tachometer", "teimo advert pile", "timing chain", "timing cover",
            "trail arm rl", "trail arm rr", "turbo", "tv remote control", "twin carburators",
            "two stroke fuel", "warning triangle", "water bucket", "water pump", "water pump pulley",
            "wheel cover leopard", "wheel cover plush", "wheel cover suomi", "wheel cover zebra", "wheelset hayosiko",
            "wheelset octo", "wheelset racing", "wheelset rally", "wheelset slot", "wheelset spoke",
            "wheelset steelwide", "wheelset turbine", "window grille", "windows black wrap", "wiring mess",
            "wishbone fl", "wishbone fr", "wood carrier", "xmas lights", "yeast", "pike", "macaron box", "milk", "potato chips",
            "pizza", "kilju", "fuse holder", "fuse package", "fuse"
        };

        /// <summary>Предметы, которые нельзя выключать полностью (только физика/рендер) — точное имя.</summary>
        public static readonly string[] CannotFullyDisable =
        {
            "fish trap(itemx)", "bucket(itemx)", "pike(itemx)", "envelope(xxxxx)",
            "lottery ticket(xxxxx)", "fuel tank(Clone)"
        };

        /// <summary>
        /// Интерактивные/чувствительные предметы, которым принудительно оставляем «только физика» даже
        /// при разрешённом полном выключении. Сравнение по вхождению подстроки.
        /// </summary>
        public static readonly string[] PhysicsOnlyForced =
        {
            "fish trap", "bucket", "pike", "envelope", "lottery ticket", "fuel tank",
            "battery", "helmet", "floor jack", "car jack", "grill", "spark plug",
            "wheel_regula", "wheel_offset", "radiator hose", "kilju", "empty plastic can",
            "emptyca", "diesel", "gasoline", "motor oil", "coolant", "brake fluid",
            "two stroke", "suitcase", "garbage barrel", "beer case", "shopping bag",
            "cd player", "cd case", "cd(", "fireworks", "wood carrier"
        };

        /// <summary>Имя проходит в белый список: безмаркерное колесо, либо (маркер + имя из Names).</summary>
        public static bool IsItemName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;

            for (int i = 0; i < Markerless.Length; i++)
                if (name == Markerless[i])
                    return true;

            if (!HasMarker(name))
                return false;

            for (int i = 0; i < Names.Length; i++)
                if (name.Contains(Names[i]))
                    return true;

            return false;
        }

        public static bool HasMarker(string name)
        {
            for (int i = 0; i < Markers.Length; i++)
                if (name.Contains(Markers[i]))
                    return true;

            return false;
        }

        /// <summary>Принудительный режим «только физика» по подстроке имени.</summary>
        public static bool IsPhysicsOnlyForced(string name)
        {
            string lower = name.ToLower();
            for (int i = 0; i < PhysicsOnlyForced.Length; i++)
                if (lower.Contains(PhysicsOnlyForced[i]))
                    return true;

            return false;
        }
    }
}
