// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Мопед Jonnez. Гасит рестарт FSM подножки, колёс и триггера игрока (борьба с тряской). Проверка
// «на земле» переопределена: у мопеда onGroundDown работает иначе, поэтому смотрим на крутящий момент.

using UnityEngine;

using MOPR.Common.Enumerations;

namespace MOPR.Vehicles.Cases
{
    internal class Jonnez : Vehicle
    {
        public Jonnez(string gameObjectName) : base(gameObjectName)
        {
            vehicleType = VehiclesTypes.Jonnez;

            gameObject.transform.Find("Kickstand").GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;

            // Гасим рестарт FSM у колёс.
            Transform wheelsParent = transform.Find("Wheels");
            foreach (Transform wheel in wheelsParent.GetComponentsInChildren<Transform>())
            {
                if (!wheel.gameObject.name.StartsWith("Moped_wheel"))
                    continue;
                wheel.gameObject.GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;
            }

            // Попытка убрать тряску мопеда.
            gameObject.transform.Find("LOD/PlayerTrigger").GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;
        }

        /// <summary>У Jonnez onGroundDown ненадёжен — считаем «на земле» по нулевому крутящему моменту.</summary>
        public override bool IsOnGround()
        {
            return drivetrain.torque == 0;
        }
    }
}
