// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Комбайн. Переключается как целый объект, но только когда становится доступен игроку (3-я стадия
// фермерской работы) — до этого его не трогаем. По правилу ignore — игнор/полное отключение.

using MOPR.Common.Enumerations;
using MOPR.FSM;
using MOPR.Rules;
using MOPR.Rules.Types;

namespace MOPR.Vehicles.Cases
{
    internal class Combine : Vehicle
    {
        public Combine(string gameObjectName) : base(gameObjectName)
        {
            vehicleType = VehiclesTypes.Combine;

            Toggle = ToggleCombineActive;

            IgnoreRule vehicleRule = RulesManager.Instance.GetList<IgnoreRule>().Find(v => v.ObjectName == this.gameObject.name);
            if (vehicleRule != null)
            {
                Toggle = IgnoreToggle;
                if (vehicleRule.TotalIgnore)
                    IsActive = false;
            }
        }

        public void ToggleCombineActive(bool enabled)
        {
            // Комбайн ещё недоступен — не трогаем.
            if (!FsmManager.IsCombineAvailable())
                return;

            ToggleActive(enabled);
        }
    }
}
