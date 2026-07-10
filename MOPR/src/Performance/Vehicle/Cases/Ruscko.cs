// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Ruscko — стандартное поведение базового Vehicle, задаётся только тип.

using MOPR.Common.Enumerations;

namespace MOPR.Vehicles.Cases
{
    internal class Ruscko : Vehicle
    {
        public Ruscko(string gameObjectName) : base(gameObjectName)
        {
            vehicleType = VehiclesTypes.Ruscko;
        }
    }
}
