// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Игровые FSM-фиксы одного предмета: гасит перезапуск FSM при активации (Use/Data/Paint/триггеры
// жидкостей), внедряет самоудаление в состояния Destroy и «Is garbage» у пакетов, навешивает
// частные поведения (крышка, домкрат, радио, гриль, чемодан) и связывает игровые переменные
// (порча, заряд батареи, гриль) обратно в предмет через биндеры. Вызывается один раз при создании.

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HutongGames.PlayMaker;

using MOPR.Common;
using MOPR.FSM;
using MOPR.FSM.Actions;
using MOPR.Helpers;
using MOPR.Managers;
using MOPR.Items.Helpers;

namespace MOPR.Items.Fixes
{
    internal static class ItemFsmFixes
    {
        public static void Apply(ItemBehaviour item)
        {
            GameObject gameObject = item.gameObject;

            // Внедряем самоудаление в состояния уничтожения.
            PlayMakerFSM fsm = gameObject.GetComponent<PlayMakerFSM>();
            if (fsm != null)
            {
                foreach (FsmState st in fsm.FsmStates)
                {
                    switch (st.Name)
                    {
                        case "Destroy self":
                            gameObject.FsmInject("Destroy self", item.RemoveSelf);
                            break;
                        case "Destroy":
                            gameObject.FsmInject("Destroy", item.RemoveSelf);
                            break;
                        case "Destroy 2":
                            gameObject.FsmInject("Destroy 2", item.RemoveSelf);
                            break;
                    }
                }
            }

            // Пакеты/сумки: самоудаление на «Is garbage» + удаление пустых.
            if (gameObject.name.Contains("shopping bag"))
            {
                gameObject.FsmInject("Is garbage", item.RemoveSelf);

                PlayMakerArrayListProxy list = gameObject.GetComponent<PlayMakerArrayListProxy>();
                if (list.arrayList.Count == 0)
                {
                    ItemsManager.Instance.Remove(item);
                    Object.Destroy(gameObject);
                }
            }

            try
            {
                PlayMakerFSM useFsm = gameObject.GetPlayMaker("Use");
                if (useFsm != null)
                {
                    useFsm.Fsm.RestartOnEnable = false;
                    if (gameObject.name.StartsWith("door ") || gameObject.name.EqualsAny("amis-auto ky package(xxxxx)", "lottery ticket(xxxxx)"))
                        return;

                    NeutralizeState(useFsm, "State 1");
                    NeutralizeState(useFsm, "Load");

                    if (gameObject.name == "battery(Clone)")
                        item.BindBattery(useFsm.FsmVariables.GetFsmBool("OnCharged"));

                    if (useFsm.FsmVariables.GetFsmFloat("SpoilingRate") != null)
                        item.BindSpoilage(useFsm.FsmVariables.GetFsmFloat("SpoilingRate"),
                                          useFsm.FsmVariables.GetFsmFloat("Condition"),
                                          useFsm.FsmVariables.GetFsmFloat("SpoilingRateFridge"));
                }

                ApplyPerItem(item, gameObject);

                PlayMakerFSM dataFsm = gameObject.GetPlayMaker("Data");
                if (dataFsm != null)
                    dataFsm.Fsm.RestartOnEnable = false;

                PlayMakerFSM paintFSM = gameObject.GetPlayMaker("Paint");
                if (paintFSM != null)
                    paintFSM.Fsm.RestartOnEnable = false;
            }
            catch (System.Exception ex)
            {
                ExceptionManager.New(ex, false, "FSM_FIXES | " + gameObject.Path());
            }
        }

        /// <summary>Вставляет CustomStop в начало состояния, чтобы оно не сбрасывало переменные предмета.</summary>
        private static void NeutralizeState(PlayMakerFSM fsm, string stateName)
        {
            FsmState state = fsm.GetState(stateName);
            if (state == null)
                return;

            List<FsmStateAction> actions = state.Actions.ToList();
            actions.Insert(0, new CustomStop());
            state.Actions = actions.ToArray();
            state.SaveActions();
        }

        /// <summary>Частные фиксы конкретных предметов: гашение рестарта триггеров и навеска поведений.</summary>
        private static void ApplyPerItem(ItemBehaviour item, GameObject gameObject)
        {
            switch (gameObject.name)
            {
                case "diesel(itemx)":
                    DisableRestart(gameObject, "FluidTrigger");
                    break;
                case "gasoline(itemx)":
                    DisableRestart(gameObject, "FluidTrigger");
                    break;
                case "motor oil(itemx)":
                    DisableRestart(gameObject, "MotorOilTrigger");
                    break;
                case "coolant(itemx)":
                    DisableRestart(gameObject, "CoolantTrigger");
                    break;
                case "brake fluid(itemx)":
                    DisableRestart(gameObject, "BrakeFluidTrigger");
                    break;
                case "wood carrier(itemx)":
                    DisableRestart(gameObject, "WoodTrigger");
                    break;
                case "suitcase(itemx)":
                    gameObject.transform.Find("Money").gameObject.AddComponent<SuitcaseMoneyBehaviour>();
                    break;
                case "radio(Clone)":
                    if (gameObject.transform.Find("Channel") != null)
                        gameObject.transform.Find("Channel").gameObject.AddComponent<RadioDisable>();
                    break;
                case "fuel tank(Clone)":
                    gameObject.transform.Find("Bolts").GetChild(7).GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;
                    break;
                case "spark plug(Clone)":
                    gameObject.GetPlayMaker("Screw").Fsm.RestartOnEnable = false;
                    break;
                case "grill(itemx)":
                    GameObject flame = gameObject.transform.Find("Fireplace/Flame/FireEffects").gameObject;
                    GameObject trigger = gameObject.transform.Find("Fireplace/GrillTrigger").gameObject;
                    item.BindGrill(flame, trigger);
                    gameObject.transform.Find("Fireplace/SausageTrigger").gameObject.AddComponent<GrillTriggerBehaviour>();
                    break;
                case "floor jack(itemx)":
                    gameObject.AddComponent<FloorJackGrabBehaviour>();
                    break;
            }
        }

        private static void DisableRestart(GameObject gameObject, string child)
        {
            gameObject.transform.Find(child).gameObject.GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;
        }
    }
}
