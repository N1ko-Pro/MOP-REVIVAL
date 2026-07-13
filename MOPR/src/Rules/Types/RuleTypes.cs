// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Модель правил совместимости: типы, в которые парсятся .mopconfig, и набор «особых» флагов на весь
// сейв. Потребители читают их через RulesManager.

using UnityEngine;
using MOPR.Common.Enumerations;

namespace MOPR.Rules.Types
{
    /// <summary>Базовое правило. SourceMod — ID мода (имя файла), из которого правило пришло.</summary>
    internal abstract class Rule
    {
        public string SourceMod;

        public override string ToString()
        {
            return $"{GetType().Name} (from {SourceMod})";
        }
    }

    /// <summary>Объект не оптимизируется. TotalIgnore — не трогать вообще (fullignore).</summary>
    internal sealed class IgnoreRule : Rule
    {
        public string ObjectName;
        public bool TotalIgnore;
    }

    /// <summary>Объект внутри конкретной локации не трогать (ignore: Place Object).</summary>
    internal sealed class IgnoreRuleAtPlace : Rule
    {
        public string Place;
        public string ObjectName;
    }

    /// <summary>Пользовательский сектор (sector: pos scale rot whitelist...).</summary>
    internal sealed class NewSector : Rule
    {
        public Vector3 Position;
        public Vector3 Scale;
        public Vector3 Rotation;
        public string[] Whitelist;
    }

    /// <summary>Не создавать LOD-заглушку для объекта (no_lod: Object).</summary>
    internal sealed class NoLod : Rule
    {
        public string ObjectName;
    }

    /// <summary>Дополнительный объект под управление оптимизацией (toggle: Object mode).</summary>
    internal sealed class ToggleRule : Rule
    {
        public string ObjectName;
        public ToggleModes ToggleMode;
    }

    /// <summary>Сменить родителя объекта (change_parent: Object NewParent|null).</summary>
    internal sealed class ChangeParentRule : Rule
    {
        public string ObjectName;
        public string NewParentName;
    }

    /// <summary>Флаги-переключатели на весь сейв (standalone-директивы).</summary>
    internal struct SpecialRules
    {
        public bool SatsumaIgnoreRenderers;
        public bool SatsumaIgnore;
        public bool DontDestroyEmptyBeerBottles;
        public bool SkipFuryColliderFix;
        public bool IgnoreModVehicles;
        public bool ToggleAllVehiclesPhysicsOnly;
        public bool NoLods;
    }
}
