// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Помощники для работы с PlayMaker: поиск FSM/состояний по имени, безопасное добавление/вставка/
// удаление/очистка экшенов состояния и инъекция C#-колбэка в состояние (замена FsmHook.FsmInject
// из MSCLoader). Пометка объекта «подбираемым». Это база, на которой строятся все FSM-фиксы.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HutongGames.PlayMaker;
using MOPR.FSM.Actions;

namespace MOPR.FSM
{
    internal static class PlayMakerExtensions
    {
        /// <summary>FSM объекта по имени (или null, если такого нет).</summary>
        public static PlayMakerFSM GetPlayMaker(this GameObject gm, string name)
        {
            return gm?.GetComponents<PlayMakerFSM>()?.FirstOrDefault(fsm => fsm.FsmName == name);
        }

        public static PlayMakerFSM GetPlayMaker(this Transform t, string name)
        {
            return GetPlayMaker(t.gameObject, name);
        }

        /// <summary>Состояние FSM по имени (или null).</summary>
        public static FsmState GetState(this PlayMakerFSM fsm, string name)
        {
            return fsm.FsmStates.FirstOrDefault(s => s.Name == name);
        }

        /// <summary>Делает объект подбираемым (слой Parts, тег PART).</summary>
        public static void MakePickable(this GameObject gm)
        {
            gm.layer = LayerMask.NameToLayer("Parts");
            gm.tag = "PART";
        }

        /// <summary>Удаляет экшен состояния по индексу.</summary>
        public static void RemoveAction(this FsmState state, int index)
        {
            List<FsmStateAction> actions = state.Actions.ToList();
            actions.RemoveAt(index);
            state.Actions = actions.ToArray();
        }

        /// <summary>Внедряет C#-колбэк в состояние FSM (замена MSCLoader FsmHook.FsmInject).</summary>
        public static void FsmInject(this GameObject gm, string name, Action action)
        {
            PlayMakerFSM fsm = gm.GetComponent<PlayMakerFSM>();
            if (!fsm)
                return;

            FsmState state = fsm.GetState(name);
            List<FsmStateAction> actions = state.Actions.ToList();
            actions.Add(new CustomStateAction(action));
            state.Actions = actions.ToArray();
        }

        /// <summary>Добавляет экшен в конец состояния.</summary>
        public static void AddAction(this FsmState state, FsmStateAction action)
        {
            List<FsmStateAction> actions = state.Actions.ToList();
            actions.Add(action);
            state.Actions = actions.ToArray();
        }

        /// <summary>Вставляет экшен в состояние по индексу.</summary>
        public static void InsertAction(this FsmState state, int index, FsmStateAction action)
        {
            List<FsmStateAction> actions = state.Actions.ToList();
            actions.Insert(index, action);
            state.Actions = actions.ToArray();
        }

        /// <summary>Убирает все экшены состояния, оставляя единственный «стоп» (state ничего не делает).</summary>
        public static void ClearActions(this FsmState state)
        {
            state.Actions = new FsmStateAction[] { new CustomStop() };
            state.SaveActions();
        }
    }
}
