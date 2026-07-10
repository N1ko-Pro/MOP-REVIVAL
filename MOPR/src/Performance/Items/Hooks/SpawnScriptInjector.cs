// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Хук спавнеров: в игровые FSM спавна предметов (Spawner/CreateItems и т.п.) внедряем действие
// CustomAddItemBehaviour, чтобы каждый только что заспавненный предмет получал ItemBehaviour и
// попадал под оптимизацию.

using UnityEngine;
using HutongGames.PlayMaker;

using MOPR.Common;
using MOPR.FSM;
using MOPR.FSM.Actions;

namespace MOPR.Items.Hooks
{
    internal static class SpawnScriptInjector
    {
        public static void Inject(GameObject gm)
        {
            PlayMakerFSM[] createFSMs = gm.GetComponents<PlayMakerFSM>();
            for (int i = 0; i < createFSMs.Length; i++)
            {
                try
                {
                    PlayMakerFSM fsm = createFSMs[i];

                    // У разных спавнеров состояние создания называется по-разному.
                    FsmState state = fsm.GetState("Create") ?? fsm.GetState("Create product");
                    if (state == null)
                    {
                        ModConsole.LogError($"[MOPR] FSM {i} has no Create or Create product");
                        continue;
                    }

                    // Переменная «New» хранит только что созданный предмет.
                    FsmGameObject go = fsm.FsmVariables.GetFsmGameObject("New");
                    state.AddAction(new CustomAddItemBehaviour(go));
                }
                catch (System.Exception ex)
                {
                    ExceptionManager.New(ex, false, "INJECT_SPAWN_SCRIPTS_ERROR");
                }
            }
        }
    }
}
