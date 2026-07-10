// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Ручной газ Kekmet: включается/выключается по запуску двигателя (инъекции в FSM стартёра), также
// подстраивает минимальные обороты по положению ручки. Удаляет конфликтующие штатные экшены стартёра.

using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using HutongGames.PlayMaker;

using MOPR.FSM;

namespace MOPR.Vehicles.Managers
{
    internal class KekmetHandThrottle : HandThrottle
    {
        private const float MaxMinimumRPM = 500;

        public KekmetHandThrottle() : base("LOD/Dashboard/Throttle")
        {
            try
            {
                GameObject starter = transform.Find("Simulation/Starter").gameObject;

                starter.FsmInject("Start engine", Invoke);
                starter.FsmInject("State 1", CancelInvoke);
                Invoke();

                PlayMakerFSM starterFSM = starter.GetPlayMaker("Starter");

                List<FsmStateAction> startEngineActions = starterFSM.GetState("Start engine").Actions.ToList();
                startEngineActions.RemoveAt(3);
                starterFSM.GetState("Start engine").Actions = startEngineActions.ToArray();

                List<FsmStateAction> state1Actions = starterFSM.GetState("State 1").Actions.ToList();
                state1Actions.RemoveAt(1);
                starterFSM.GetState("State 1").Actions = state1Actions.ToArray();
            }
            catch
            {
                throw new Exception("KekmetHandThrottle: Invokers error.");
            }
        }

        protected override void ThrottleUpdate()
        {
            base.ThrottleUpdate();
            MinRPM = IdleThrottle * 2500;
        }

        private float MinRPM
        {
            set => drivetrain.minRPM = value > MaxMinimumRPM ? MaxMinimumRPM : value;
        }
    }
}
