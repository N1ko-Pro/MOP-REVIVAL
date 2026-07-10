// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Лодка. Формально это не UnityCar-транспорт, но переключаем её как целый объект (ToggleActive),
// чтобы лодка не телепортировалась к точке спавна. По правилу ignore — только физика.

using UnityEngine;

using MOPR.Common.Enumerations;
using MOPR.Rules;
using MOPR.Rules.Types;

namespace MOPR.Vehicles.Cases
{
    internal class Boat : Vehicle
    {
        private Transform collidersParent;

        public Boat(string gameObject) : base(gameObject)
        {
            vehicleType = VehiclesTypes.Boat;
            Toggle = ToggleActive;

            transform.Find("GFX/Motor/Pivot/FuelTank").gameObject.GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;
            rb = this.gameObject.GetComponent<Rigidbody>();

            IgnoreRule vehicleRule = RulesManager.Instance.GetList<IgnoreRule>().Find(v => v.ObjectName == this.gameObject.name);
            if (vehicleRule != null)
            {
                if (vehicleRule.TotalIgnore)
                {
                    IsActive = false;
                    return;
                }

                Toggle = ToggleBoatPhysics;
            }

            dummyCar = new LOD.LodObject(this.gameObject);
        }

        internal override void ToggleActive(bool enabled)
        {
            if (gameObject == null || gameObject.activeSelf == enabled || !IsActive)
                return;

            if (!enabled)
            {
                MoveNonDisableableObjects(temporaryParent);
                colliders.parent = temporaryParent;
                dummyCar?.ToggleActive(true, transform);
            }

            gameObject.SetActive(enabled);

            if (enabled)
            {
                MoveNonDisableableObjects(null);
                colliders.parent = collidersParent;
                colliders.localPosition = colliderPosition;
            }
        }

        private void ToggleBoatPhysics(bool enabled)
        {
            if (gameObject == null || rb.detectCollisions == enabled || !IsActive)
                return;

            rb.detectCollisions = enabled;
            rb.isKinematic = !enabled;
        }

        // У лодки нет UnityCar — физику через ToggleUnityCar не трогаем.
        public override void ToggleUnityCar(bool enabled) { }
        public override void ForceToggleUnityCar(bool enabled) { }
        protected override void LoadCarElements() { }

        protected override void LoadColliders()
        {
            colliders = transform.Find("GFX/Colliders");
            colliderPosition = colliders.localPosition;
            collidersParent = colliders.parent;
        }
    }
}
