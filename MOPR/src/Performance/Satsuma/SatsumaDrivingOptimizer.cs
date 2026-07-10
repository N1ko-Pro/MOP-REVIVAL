// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Глубокая оптимизация Сатсумы во время езды. Пока игрок сидит за рулём Сатсумы, капот закрыт и
// внутренности двигателя/триггеры установки деталей/внешняя косметика не видны и не нужны — гасим
// их и возвращаем при выходе. Это заполняет пробел: штатная оптимизация держит Сатсуму полностью
// активной, когда игрок рядом/в ней. Идея заимствована у SatsumaFpsOptimization (TeimoBR), но
// список выверен: НЕ трогаем объекты симуляции двигателя (CooldownTick / New / Damaged /
// RotateFlywheel) — только триггеры, пивоты и косметические меши, чтобы не менять игровое поведение
// (перегрев, износ, состояние блока). Триггер — строго «игрок в Сатсуме», а не в любой машине.

using System.Collections.Generic;
using UnityEngine;

using MOPR.Common;
using MOPR.FSM;

namespace MOPR.Vehicles.Managers.SatsumaManagers
{
    internal sealed class SatsumaDrivingOptimizer : MonoBehaviour
    {
        // Относительные пути (от корня Сатсумы) объектов, безопасных к отключению за рулём.
        // Только install-триггеры, пивоты деталей и косметика. Никакой логики симуляции.
        private static readonly string[] Paths =
        {
            // MiscParts: триггеры установки навесного оборудования.
            "MiscParts/trigger_brake_lining", "MiscParts/trigger_brake_master", "MiscParts/trigger_clutch_master",
            "MiscParts/trigger_clutch_lining", "MiscParts/trigger_gear_linkage", "MiscParts/trigger_fuel_strainer",
            "MiscParts/trigger_fuel_tank", "MiscParts/trigger_headlight_left", "MiscParts/trigger_headlight_right",
            "MiscParts/trigger_rearlight_left", "MiscParts/trigger_rearlight_right", "MiscParts/trigger_radiator",
            "MiscParts/trigger_radiator_hose1", "MiscParts/trigger_radiator_hose3", "MiscParts/trigger_fuel_tank_pipe",
            "MiscParts/trigger_battery", "MiscParts/trigger_electrics", "MiscParts/trigger_mudflap_rl",
            "MiscParts/trigger_mudflap_rr", "MiscParts/trigger_marker_left", "MiscParts/trigger_marker_right",
            "MiscParts/trigger_antenna_left", "MiscParts/trigger_antenna_right", "MiscParts/trigger_exhaust_dual_tip",
            "MiscParts/Triggers Exhaust Pipes", "MiscParts/Triggers Mufflers",

            // MiscParts: пивоты навесного оборудования (держат установленные детали).
            "MiscParts/pivot_headlight_left", "MiscParts/pivot_headlight_right", "MiscParts/pivot_electrics",
            "MiscParts/pivot_exhaust pipe", "MiscParts/pivot_exhaust_dual_tip", "MiscParts/pivot_exhaust_muffler",

            // MiscParts: косметические/внешние меши, не видимые из кабины.
            "MiscParts/fuel line(xxxxx)", "MiscParts/rearlight(leftx)", "MiscParts/rearlight(right)",
            "MiscParts/brake lining(xxxxx)", "MiscParts/brake master cylinder(xxxxx)",
            "MiscParts/clutch master cylinder(xxxxx)", "MiscParts/clutch lining(xxxxx)",
            "MiscParts/fuel tank pipe(xxxxx)", "MiscParts/fuel strainer(xxxxx)", "MiscParts/gear linkage(xxxxx)",
            "MiscParts/radiator hose1(xxxxx)", "MiscParts/radiator hose3(xxxxx)",
            "MiscParts/mudflap rl(xxxxx)", "MiscParts/mudflap rr(xxxxx)",
            "MiscParts/marker light left(xxxxx)", "MiscParts/marker light right(xxxxx)",
            "MiscParts/antenna(leftx)", "MiscParts/antenna(right)",

            // Блок двигателя: пивоты внутренних деталей (скрыты под капотом/за спиной).
            "Chassis/sub frame(xxxxx)/CarMotorPivot/block(Clone)/pivot_alternator",
            "Chassis/sub frame(xxxxx)/CarMotorPivot/block(Clone)/pivot_radiator hose2",
            "Chassis/sub frame(xxxxx)/CarMotorPivot/block(Clone)/pivot_camshaft",
            "Chassis/sub frame(xxxxx)/CarMotorPivot/block(Clone)/pivot_crankshaft",
            "Chassis/sub frame(xxxxx)/CarMotorPivot/block(Clone)/pivot_cylinder head",
            "Chassis/sub frame(xxxxx)/CarMotorPivot/block(Clone)/pivot_distributor",
            "Chassis/sub frame(xxxxx)/CarMotorPivot/block(Clone)/pivot_engine plate",
            "Chassis/sub frame(xxxxx)/CarMotorPivot/block(Clone)/pivot_alternator belt",
            "Chassis/sub frame(xxxxx)/CarMotorPivot/block(Clone)/pivot_fuel pump",
            "Chassis/sub frame(xxxxx)/CarMotorPivot/block(Clone)/pivot_gearbox",
            "Chassis/sub frame(xxxxx)/CarMotorPivot/block(Clone)/pivot_head gasket",
            "Chassis/sub frame(xxxxx)/CarMotorPivot/block(Clone)/pivot_main bearing1",
            "Chassis/sub frame(xxxxx)/CarMotorPivot/block(Clone)/pivot_main bearing2",
            "Chassis/sub frame(xxxxx)/CarMotorPivot/block(Clone)/pivot_main bearing3",
            "Chassis/sub frame(xxxxx)/CarMotorPivot/block(Clone)/pivot_oil filter",
            "Chassis/sub frame(xxxxx)/CarMotorPivot/block(Clone)/pivot_oilpan",
            "Chassis/sub frame(xxxxx)/CarMotorPivot/block(Clone)/pivot_piston1",
            "Chassis/sub frame(xxxxx)/CarMotorPivot/block(Clone)/pivot_piston2",
            "Chassis/sub frame(xxxxx)/CarMotorPivot/block(Clone)/pivot_piston3",
            "Chassis/sub frame(xxxxx)/CarMotorPivot/block(Clone)/pivot_piston4",
            "Chassis/sub frame(xxxxx)/CarMotorPivot/block(Clone)/pivot_timing cover",

            // Блок двигателя: install-триггеры внутренних деталей.
            "Chassis/sub frame(xxxxx)/CarMotorPivot/block(Clone)/_Triggers",
            "Chassis/sub frame(xxxxx)/CarMotorPivot/block(Clone)/trigger_radiator hose2",
            "Chassis/sub frame(xxxxx)/CarMotorPivot/block(Clone)/trigger_timing cover",
            "Chassis/sub frame(xxxxx)/CarMotorPivot/block(Clone)/trigger_piston1",
            "Chassis/sub frame(xxxxx)/CarMotorPivot/block(Clone)/trigger_piston2",
            "Chassis/sub frame(xxxxx)/CarMotorPivot/block(Clone)/trigger_piston3",
            "Chassis/sub frame(xxxxx)/CarMotorPivot/block(Clone)/trigger_piston4",
            "Chassis/sub frame(xxxxx)/CarMotorPivot/block(Clone)/trigger_oilpan",
            "Chassis/sub frame(xxxxx)/CarMotorPivot/block(Clone)/trigger_oil filter",
            "Chassis/sub frame(xxxxx)/CarMotorPivot/block(Clone)/trigger_main bearing1",
            "Chassis/sub frame(xxxxx)/CarMotorPivot/block(Clone)/trigger_main bearing2",
            "Chassis/sub frame(xxxxx)/CarMotorPivot/block(Clone)/trigger_main bearing3",
            "Chassis/sub frame(xxxxx)/CarMotorPivot/block(Clone)/trigger_head gasket",
            "Chassis/sub frame(xxxxx)/CarMotorPivot/block(Clone)/trigger_gearbox",
            "Chassis/sub frame(xxxxx)/CarMotorPivot/block(Clone)/trigger_fuel pump",
            "Chassis/sub frame(xxxxx)/CarMotorPivot/block(Clone)/trigger_alternator belt",
            "Chassis/sub frame(xxxxx)/CarMotorPivot/block(Clone)/trigger_engine plate",
            "Chassis/sub frame(xxxxx)/CarMotorPivot/block(Clone)/trigger_distributor",
            "Chassis/sub frame(xxxxx)/CarMotorPivot/block(Clone)/trigger_cylinder head",
            "Chassis/sub frame(xxxxx)/CarMotorPivot/block(Clone)/trigger_racing head",
            "Chassis/sub frame(xxxxx)/CarMotorPivot/block(Clone)/trigger_crankshaft",
            "Chassis/sub frame(xxxxx)/CarMotorPivot/block(Clone)/trigger_alternator",
            "Chassis/sub frame(xxxxx)/CarMotorPivot/block(Clone)/trigger_camshaft",
        };

        private readonly List<GameObject> targets = new List<GameObject>();
        private readonly Dictionary<GameObject, bool> originalStates = new Dictionary<GameObject, bool>();

        private bool suppressed;   // true — сейчас в «режиме вождения» (объекты погашены)
        private bool resolved;

        private void Awake()
        {
            // Резолвим пути, пока Сатсума и её внутренности ещё активны (до общего ToggleAll).
            Resolve();
        }

        private void Resolve()
        {
            targets.Clear();
            for (int i = 0; i < Paths.Length; i++)
            {
                Transform t = transform.Find(Paths[i]);
                if (t != null)
                    targets.Add(t.gameObject);
            }

            resolved = true;
        }

        private void Update()
        {
            // Фича/мод выключены — всё должно быть в исходном состоянии.
            if (!MoprSettings.IsModActive || !MoprSettings.SatsumaDrivingModeOn)
            {
                if (suppressed)
                    Restore();
                return;
            }

            bool inSatsuma = FsmManager.IsPlayerInSatsuma();
            if (inSatsuma == suppressed)
                return; // состояние не изменилось

            if (inSatsuma)
                Suppress();
            else
                Restore();
        }

        /// <summary>Гасит целевые объекты, запомнив их исходное состояние.</summary>
        private void Suppress()
        {
            if (!resolved)
                Resolve();

            originalStates.Clear();
            for (int i = 0; i < targets.Count; i++)
            {
                GameObject go = targets[i];
                if (go == null)
                    continue;

                originalStates[go] = go.activeSelf;
                if (go.activeSelf)
                    go.SetActive(false);
            }

            suppressed = true;
        }

        /// <summary>Возвращает объекты в состояние, которое было до отключения.</summary>
        private void Restore()
        {
            foreach (KeyValuePair<GameObject, bool> pair in originalStates)
            {
                if (pair.Key != null && pair.Value && !pair.Key.activeSelf)
                    pair.Key.SetActive(true);
            }

            originalStates.Clear();
            suppressed = false;
        }

        private void OnDisable() => Restore();
        private void OnDestroy() => Restore();
    }
}
