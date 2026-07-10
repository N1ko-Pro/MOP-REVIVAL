// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Выбор режима оптимизации предмета один раз при регистрации. Полностью выключаем ТОЛЬКО инертный
// хлам (нет интеракции Use/Open и нет в forced-списке); всё интерактивное/чувствительное держим в
// режиме «только физика» — GameObject активен, интеракция и состояние целы. Учитываются правила
// ignore/fullignore.

using UnityEngine;
using HutongGames.PlayMaker;

using MOPR.Rules;
using MOPR.Rules.Types;

namespace MOPR.Items
{
    internal struct ItemModeDecision
    {
        public ItemToggleMode Mode;
        public bool Destroy;       // fullignore: предмет не трогаем вообще (удаляем компонент).
        public bool KeepRenderer;  // false для ignore-предметов — рендер не трогаем.
    }

    internal static class ItemModeDecider
    {
        public static ItemModeDecision Decide(GameObject go)
        {
            ItemModeDecision d = new ItemModeDecision { Mode = ItemToggleMode.Full, KeepRenderer = true };

            string name = go.name;

            // Правило ignore → только физика; fullignore → не трогаем совсем.
            IgnoreRule rule = RulesManager.Instance.GetList<IgnoreRule>().Find(f => f.ObjectName == name);
            if (rule != null)
            {
                d.Mode = ItemToggleMode.PhysicsOnly;
                d.KeepRenderer = false;
                if (rule.TotalIgnore)
                    d.Destroy = true;
                return d;
            }

            // Точный список неотключаемых, forced-подстроки и интерактивные FSM → только физика.
            if (EqualsAnyExact(name, ItemNameList.CannotFullyDisable)
                || ItemNameList.IsPhysicsOnlyForced(name)
                || HasInteractionFsm(go))
            {
                d.Mode = ItemToggleMode.PhysicsOnly;
            }

            return d;
        }

        /// <summary>Есть ли у предмета игровая интеракция (FSM "Use"/"Open") — тогда полное выключение запрещено.</summary>
        private static bool HasInteractionFsm(GameObject go)
        {
            try
            {
                foreach (PlayMakerFSM fsm in go.GetComponentsInChildren<PlayMakerFSM>(true))
                {
                    if (fsm == null)
                        continue;

                    if (fsm.FsmName == "Use" || fsm.FsmName == "Open")
                        return true;
                }
            }
            catch
            {
                // Не удалось определить — считаем интерактивным (безопасный вариант).
                return true;
            }

            return false;
        }

        private static bool EqualsAnyExact(string name, string[] list)
        {
            for (int i = 0; i < list.Length; i++)
                if (name == list[i])
                    return true;

            return false;
        }
    }
}
