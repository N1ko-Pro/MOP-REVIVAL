// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Микроавтобус Hayosiko. Пока у игрока нет ключей (машина ещё «дядина»), используем частичное
// отключение отдельных узлов — чтобы не конфликтовать со скриптами MSC про дядю. После получения
// машины навсегда переходим на обычное полное переключение.

using System;
using System.Collections.Generic;
using UnityEngine;

using MOPR.FSM;
using MOPR.Common.Enumerations;

namespace MOPR.Vehicles.Cases
{
    internal class Hayosiko : Vehicle
    {
        private readonly string[] partialDisableItemNames =
        {
            "CoG", "FuelTank", "HookFront", "HookRear", "RadioPivot",
            "Odometer", "LOD", "wheelFL", "wheelFR", "wheelRL",
            "wheelRR", "StagingWheel", "DriverDoors", "RearDoor",
            "SideDoor", "body", "GetInPivot", "Colliders", "Starter"
        };

        private readonly GameObject[] partialDisableItems;

        public Hayosiko(string gameObjectName = "HAYOSIKO(1500kg, 250)") : base(gameObjectName)
        {
            vehicleType = VehiclesTypes.Hayosiko;

            transform.Find("Odometer").gameObject.GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;

            try
            {
                List<GameObject> gms = new List<GameObject>();
                foreach (string f in partialDisableItemNames)
                {
                    Transform t = transform.Find(f);
                    if (t)
                        gms.Add(t.gameObject);
                }

                partialDisableItems = gms.ToArray();
            }
            catch
            {
                throw new Exception("Couldn't find partial disable items.");
            }

            Toggle = ToggleFull;
        }

        public void ToggleFull(bool enabled)
        {
            if (!FsmManager.PlayerHasHayosikoKey())
            {
                // Частичное отключение — чтобы не ломать скрипты MSC про дядю.
                TogglePartial(enabled);
                return;
            }

            // Машина получена навсегда — частичное отключение больше не нужно.
            TogglePartial(true);

            if (!enabled)
            {
                MoveNonDisableableObjects(temporaryParent);
                colliders.parent = temporaryParent;
            }

            gameObject.SetActive(enabled);

            if (enabled)
            {
                MoveNonDisableableObjects(null);
                colliders.parent = transform;
                colliders.localPosition = colliderPosition;
            }
        }

        public void TogglePartial(bool enabled)
        {
            if (partialDisableItems[0].activeSelf == enabled)
                return;

            for (int i = 0; i < partialDisableItems.Length; i++)
                partialDisableItems[i].SetActive(enabled);
        }
    }
}
