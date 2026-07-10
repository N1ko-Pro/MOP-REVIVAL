// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Грузовик Gifu. Гасит рестарт FSM ручек приборки, бака отходов, одометра и давления воздуха;
// добавляет ручной газ (GifuHandThrottle).

using UnityEngine;

using MOPR.Common.Enumerations;
using MOPR.FSM;
using MOPR.Vehicles.Managers;

namespace MOPR.Vehicles.Cases
{
    internal class Gifu : Vehicle
    {
        public Gifu(string gameObjectName) : base(gameObjectName)
        {
            vehicleType = VehiclesTypes.Gifu;

            Transform knobs = gameObject.transform.Find("Dashboard/Knobs");
            foreach (PlayMakerFSM knobsFSMs in knobs.GetComponentsInChildren<PlayMakerFSM>())
                knobsFSMs.Fsm.RestartOnEnable = false;

            gameObject.transform.Find("ShitTank").gameObject.GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;
            transform.Find("Dashboard/Odometer").gameObject.GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;
            transform.Find("Simulation/Airbrakes").GetPlayMaker("Air Pressure").Fsm.RestartOnEnable = false;

            gameObject.AddComponent<GifuHandThrottle>();
        }
    }
}
