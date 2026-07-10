// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Хук брошенной тары: на префабы «Fly»-объектов (бутылки/стаканы/пачки) вешаем
// ThrowableJunkBehaviour, который либо уничтожает пустую тару (если включена настройка), либо
// навешивает ItemBehaviour.

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HutongGames.PlayMaker;

using MOPR.Items.Helpers;

namespace MOPR.Items.Hooks
{
    internal static class ThrowableBottleHook
    {
        // Имена «Fly»-префабов и соответствующая им пустая тара.
        private static readonly Dictionary<string, string> Names = new Dictionary<string, string>
        {
            { "BottleBeerFly", "empty bottle(Clone)" },
            { "BottleBoozeFly", "empty bottle(Clone)" },
            { "BottleSpiritFly", "empty bottle(Clone)" },
            { "CoffeeFly", "empty cup(Clone)" },
            { "MilkFly", "empty pack(Clone)" },
            { "VodkaShotFly", "empty glass(Clone)" }
        };

        public static void Install()
        {
            // Берём только «Fly»-префабы, у которых есть FSM, и навешиваем на них помощника.
            foreach (GameObject go in Resources.FindObjectsOfTypeAll<GameObject>()
                .Where(obj => Names.ContainsKey(obj.name) && obj.GetComponent<PlayMakerFSM>() != null))
            {
                go.AddComponent<ThrowableJunkBehaviour>();
            }
        }
    }
}
