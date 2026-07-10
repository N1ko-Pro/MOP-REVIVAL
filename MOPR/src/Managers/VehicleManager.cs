// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Менеджер транспорта: при инициализации находит все ванильные машины MSC и создаёт под них
// подходящий класс-обработчик (Satsuma/Hayosiko/… или базовый Vehicle). Даёт перебор и поиск по типу.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using MOPR.Vehicles;
using MOPR.Vehicles.Cases;
using MOPR.Common;
using MOPR.Common.Enumerations;
using MOPR.Common.Interfaces;

namespace MOPR.Managers
{
    internal class VehicleManager : IManager<Vehicle>
    {
        private static VehicleManager instance;
        public static VehicleManager Instance => instance ?? (instance = new VehicleManager());

        public Vehicle this[int index] => vehicles[index];

        private readonly string[] vehicleArrayMSC =
        {
            "SATSUMA(557kg, 248)",
            "HAYOSIKO(1500kg, 250)",
            "JONNEZ ES(Clone)",
            "FLATBED",
            "KEKMET(350-400psi)",
            "RCO_RUSCKO12(270)",
            "FERNDALE(1630kg)",
            "GIFU(750/450psi)",
            "BOAT",
            "COMBINE(350-400psi)"
        };

        private List<Vehicle> vehicles;

        private VehicleManager() { }

        public void Initialize()
        {
            vehicles = new List<Vehicle>();

            foreach (string vehicle in vehicleArrayMSC)
            {
                try
                {
                    if (GameObject.Find(vehicle) == null)
                    {
                        ModConsole.Log("[MOPR] Unable to locate vehicle " + vehicle);
                        continue;
                    }

                    Vehicle newVehicle;
                    switch (vehicle)
                    {
                        default:
                            newVehicle = new Vehicle(vehicle);
                            break;
                        case "SATSUMA(557kg, 248)":
                            newVehicle = new Satsuma(vehicle);
                            break;
                        case "BOAT":
                            newVehicle = new Boat(vehicle);
                            break;
                        case "COMBINE(350-400psi)":
                            newVehicle = new Combine(vehicle);
                            break;
                        case "HAYOSIKO(1500kg, 250)":
                            newVehicle = new Hayosiko(vehicle);
                            break;
                        case "KEKMET(350-400psi)":
                            newVehicle = new Kekmet(vehicle);
                            break;
                        case "FLATBED":
                            newVehicle = new Flatbed(vehicle);
                            break;
                        case "RCO_RUSCKO12(270)":
                            newVehicle = new Ruscko(vehicle);
                            break;
                        case "FERNDALE(1630kg)":
                            newVehicle = new Ferndale(vehicle);
                            break;
                        case "GIFU(750/450psi)":
                            newVehicle = new Gifu(vehicle);
                            break;
                        case "JONNEZ ES(Clone)":
                            newVehicle = new Jonnez(vehicle);
                            break;
                    }

                    vehicles.Add(newVehicle);
                }
                catch (Exception ex)
                {
                    ExceptionManager.New(ex, false, "VEHICLE_LOAD_ERROR_" + vehicle);
                }
            }

            ModConsole.Log("[MOPR] Vehicles initialized");
        }

        public Vehicle Add(Vehicle vehicle)
        {
            vehicles.Add(vehicle);
            return vehicle;
        }

        public int Count => vehicles.Count;

        public List<Vehicle> GetAll => vehicles;

        public void RemoveAt(int index) => vehicles.RemoveAt(index);

        public Vehicle GetVehicle(VehiclesTypes vehicleType)
        {
            if (vehicleType == VehiclesTypes.Generic)
                return null;

            return vehicles.FirstOrDefault(f => f.VehicleType == vehicleType);
        }

        public void Remove(Vehicle vehicle)
        {
            if (vehicles.Contains(vehicle))
                vehicles.Remove(vehicle);
        }

        public int EnabledCount
        {
            get
            {
                int enabled = 0;
                foreach (Vehicle veh in vehicles)
                    if (veh.gameObject.activeSelf)
                        enabled++;
                return enabled;
            }
        }

        public bool IsInVanilaGame(Vehicle veh) => vehicleArrayMSC.Contains(veh.gameObject.name);
    }
}
