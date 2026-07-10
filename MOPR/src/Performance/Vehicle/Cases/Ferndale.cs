// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Ferndale — стандартное поведение базового Vehicle, задаётся только тип.

using MOPR.Common.Enumerations;

namespace MOPR.Vehicles.Cases
{
    internal class Ferndale : Vehicle
    {
        public Ferndale(string gameObjectName) : base(gameObjectName)
        {
            vehicleType = VehiclesTypes.Ferndale;
        }
    }
}
