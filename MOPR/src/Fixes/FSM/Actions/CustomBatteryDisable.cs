// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Замена экшенов состояния «Disable battery wires» в SATSUMA/Wiring (FSM "Status"). Оригинальный
// экшен иногда навсегда отключал минусовую клемму, ломая сейв; здесь клемма включается строго по
// флагу Installed из базы проводки.

using HutongGames.PlayMaker;
using UnityEngine;
using MOPR.FSM;

namespace MOPR.FSM.Actions
{
    public class CustomBatteryDisable : FsmStateAction
    {
        private readonly FsmBool fsmBoolInstalled;
        private readonly GameObject batteryTerminalMinus;

        public CustomBatteryDisable()
        {
            fsmBoolInstalled = GameObject.Find("Database/DatabaseWiring/WiringBatteryMinus").GetPlayMaker("Data").FsmVariables.FindFsmBool("Installed");
            batteryTerminalMinus = GameObject.Find("SATSUMA(557kg, 248)").transform.Find("Wiring/Parts/battery_terminal_minus(xxxxx)").gameObject;
        }

        public override void OnEnter()
        {
            batteryTerminalMinus.SetActive(fsmBoolInstalled.Value);
            Finish();
        }
    }
}
