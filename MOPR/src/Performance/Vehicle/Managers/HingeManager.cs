// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Сбрасывает состояние HingeJoint при выгрузке машины: Unity-баг ломает шарнир после disable/enable,
// поэтому на выключении возвращаем исходную позу, а на следующем кадре после включения — ту, что
// была на момент выключения. (Идея: answers.unity.com про HingeJoint disable/enable.)

using UnityEngine;

namespace MOPR.Vehicles.Managers
{
    internal class HingeManager : MonoBehaviour
    {
        private Quaternion initialLocalRotation;
        private Vector3 initialLocalPosition;

        private Quaternion localRotationOnDisable;
        private Vector3 localPositionOnDisable;

        private bool hasDisabled;

        private void Awake()
        {
            initialLocalRotation = transform.localRotation;
            initialLocalPosition = transform.localPosition;
        }

        private void OnDisable()
        {
            if (!IsAttachedToVehicle())
                return;

            localRotationOnDisable = transform.localRotation;
            transform.localRotation = initialLocalRotation;

            localPositionOnDisable = transform.localPosition;
            transform.localPosition = initialLocalPosition;

            hasDisabled = true;
        }

        private void Update()
        {
            if (!IsAttachedToVehicle())
                return;

            if (hasDisabled)
            {
                hasDisabled = false;
                transform.localRotation = localRotationOnDisable;
                transform.localPosition = localPositionOnDisable;
            }
        }

        protected bool IsAttachedToVehicle()
        {
            Transform root = transform.root;
            return root != null && root.gameObject.name != "CARPARTS";
        }
    }
}
