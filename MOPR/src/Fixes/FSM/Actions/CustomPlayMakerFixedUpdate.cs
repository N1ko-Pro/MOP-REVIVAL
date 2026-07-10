// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Замена игрового PlayMakerFixedUpdate: даёт моду полный контроль над FixedUpdate FSM объекта —
// его можно включать/выключать по надобности. Тик FSM запускается только после StartFixedUpdate.

using UnityEngine;
using MOPR.Common;

namespace MOPR.FSM.Actions
{
    internal class CustomPlayMakerFixedUpdate : MonoBehaviour
    {
        private readonly PlayMakerFSM[] fsms;
        private bool isRunning;

        public CustomPlayMakerFixedUpdate()
        {
            if (!gameObject.name.Contains("wheel"))
            {
                Component playMakerFixedUpdate = gameObject.GetComponentByName("PlayMakerFixedUpdate");
                if (playMakerFixedUpdate == null)
                {
                    MSCLoader.ModConsole.Error("[MOPR] No PlayMakerFixedUpdate component found in " + gameObject.name + ".");
                    return;
                }

                Object.Destroy(playMakerFixedUpdate);
            }

            fsms = GetComponents<PlayMakerFSM>();
        }

        private void FixedUpdate()
        {
            if (!isRunning)
                return;

            for (int i = 0; i < fsms.Length; i++)
                if (fsms[i].Active && fsms[i].Fsm.HandleFixedUpdate)
                    fsms[i].Fsm.FixedUpdate();
        }

        public void StartFixedUpdate() => isRunning = true;
    }
}
