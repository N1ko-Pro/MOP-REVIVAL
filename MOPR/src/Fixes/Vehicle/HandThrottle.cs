// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Базовый ручной газ: переопределяет штатные системы ручного газа Кекмета и Гифу. Каждый тик
// складывает педальный газ с положением ручки и пишет в drivetrain (с ограничением по максимуму).

using HutongGames.PlayMaker;
using UnityEngine;
using System;
using MOPR.FSM;

namespace MOPR.Vehicles.Managers
{
    internal class HandThrottle : MonoBehaviour
    {
        protected Drivetrain drivetrain;
        protected AxisCarController axisCarController;
        protected FsmFloat handThrottleValue;

        private const float ThrottleMax = 1;

        public HandThrottle(string throttlePath)
        {
            try
            {
                drivetrain = gameObject.GetComponent<Drivetrain>();
                axisCarController = gameObject.GetComponent<AxisCarController>();
            }
            catch
            {
                throw new Exception("HandThrottle: Components not found.");
            }

            try
            {
                Transform handThrottle = transform.Find(throttlePath);
                handThrottleValue = handThrottle.GetPlayMaker("Use").FsmVariables.GetFsmFloat("Throttle");
                handThrottle.GetPlayMaker("Throttle").enabled = false;
            }
            catch
            {
                throw new Exception("HandThrottle: FSM tweaks issue.");
            }
        }

        protected void Invoke()
        {
            InvokeRepeating("ThrottleUpdate", 0, 0.000015f);
        }

        protected virtual void ThrottleUpdate()
        {
            IdleThrottle = handThrottleValue.Value;
            Throttle = axisCarController.throttle + IdleThrottle;
        }

        protected float Throttle
        {
            get => drivetrain.throttle;
            set => drivetrain.throttle = value > ThrottleMax ? ThrottleMax : value;
        }

        protected float IdleThrottle
        {
            get => drivetrain.idlethrottle;
            set => drivetrain.idlethrottle = value;
        }
    }
}
