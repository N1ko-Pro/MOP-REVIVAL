// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Оркестратор движковых Harmony-патчей игрового кода. Применяется один раз за процесс, за тумблером
// настройки и с проверкой совместимости: если установлен мод Reharmonization (делает то же и больше),
// свои патчи НЕ накладываем, чтобы не патчить один и тот же код дважды. Сами патчи — в GamePatches.

using System;
using System.Reflection;
using Harmony;
using HutongGames.PlayMaker;
using UnityEngine;
using MSCLoader;
using MOPR.Common;

namespace MOPR.Performance.Engine
{
    internal static class EnginePatches
    {
        private const string HarmonyId = "MOPR.EnginePatches";

        private static HarmonyInstance harmony;
        private static bool applied;

        /// <summary>Накладывает движковые патчи (идемпотентно, один раз за процесс).</summary>
        public static void Apply()
        {
            if (applied)
                return;

            if (!MoprSettings.EnginePatchesOn)
                return;

            // Reharmonization уже патчит этот код — не дублируем.
            if (ModLoader.IsModPresent("Reharmonization"))
            {
                ModConsole.Log("[MOPR] Engine patches skipped: Reharmonization is installed.");
                return;
            }

            try
            {
                harmony = HarmonyInstance.Create(HarmonyId);

                // Transform-сеттеры: no-op при неизменном значении.
                Patch(typeof(Transform), "set_position", new[] { typeof(Vector3) });
                Patch(typeof(Transform), "set_localPosition", new[] { typeof(Vector3) });
                Patch(typeof(Transform), "set_eulerAngles", new[] { typeof(Vector3) });
                Patch(typeof(Transform), "set_localEulerAngles", new[] { typeof(Vector3) });

                // Кэш «мышиных» рейкастов по маске.
                Patch(typeof(ActionHelpers), "MousePick", new[] { typeof(float), typeof(int) });
                Patch(typeof(ActionHelpers), "IsMouseOver", new[] { typeof(GameObject), typeof(float), typeof(int) });

                // Исправление удаления отложенных событий FSM.
                Patch(typeof(Fsm), "UpdateDelayedEvents", Type.EmptyTypes);

                applied = true;
                ModConsole.Log("<color=green>[MOPR] Engine patches applied.</color>");
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "ENGINE_PATCHES_ERROR");
            }
        }

        /// <summary>Патчит метод префиксом из GamePatches с именем "{Тип}_{Метод}".</summary>
        private static void Patch(Type type, string method, Type[] parameters)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

            MethodBase original = type.GetMethod(method, flags, null, parameters, null);
            if (original == null)
            {
                ModConsole.LogWarning("[MOPR] Engine patch target not found: " + type.Name + "." + method);
                return;
            }

            string prefixName = type.Name + "_" + method;
            MethodInfo prefix = typeof(GamePatches).GetMethod(prefixName, flags);
            if (prefix == null)
            {
                ModConsole.LogWarning("[MOPR] Engine patch prefix not found: " + prefixName);
                return;
            }

            harmony.Patch(original, new HarmonyMethod(prefix), null, null);
        }
    }
}
