// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Описатель узла Сатсумы, который включается/выключается по ситуации (двигатель заведён / игрок
// близко / игрок далеко). Хранит либо GameObject, либо FSM и условие включения.

using UnityEngine;
using MOPR.Common.Enumerations;

namespace MOPR.Vehicles.Managers.SatsumaManagers
{
    internal class SatsumaOnActionObjects
    {
        public GameObject GameObject;
        public PlayMakerFSM FSM;
        public SatsumaEnableOn EnableOn;

        public SatsumaOnActionObjects(GameObject gameObject, SatsumaEnableOn enableOn)
        {
            GameObject = gameObject;
            EnableOn = enableOn;
        }

        public SatsumaOnActionObjects(PlayMakerFSM fsm, SatsumaEnableOn enableOn)
        {
            FSM = fsm;
            EnableOn = enableOn;
        }
    }
}
