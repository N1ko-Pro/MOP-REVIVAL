// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Как HingeManager, но для капота Сатсумы: исходную позу шарнира снимает с задержкой (капот
// пристёгивается позже) и переобновляет её при сборке капота. Также гасит лишние FixedJoint при
// включении.

using MOPR.FSM;
using System.Collections;
using UnityEngine;

namespace MOPR.Vehicles.Managers
{
    internal class DelayedHingeManager : MonoBehaviour
    {
        private Quaternion initialLocalRotation;
        private Vector3 initialLocalPosition;

        private Quaternion localRotationOnDisable;
        private Vector3 localPositionOnDisable;

        private bool hasDisabled;
        private bool initialHookDone;

        private void Awake()
        {
            StartCoroutine(InitializationRoutine());
        }

        private IEnumerator InitializationRoutine(bool noDelay = false)
        {
            if (noDelay)
                yield return null;
            else
                yield return new WaitForSeconds(2);

            initialLocalRotation = transform.localRotation;
            initialLocalPosition = transform.localPosition;

            if (!initialHookDone)
            {
                GameObject.Find("SATSUMA(557kg, 248)").transform.Find("Body/trigger_hood").gameObject.FsmInject("Assemble 2", UpdateInitialRotation);
                initialHookDone = true;
            }
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

        private void UpdateInitialRotation()
        {
            StartCoroutine(InitializationRoutine(true));
        }

        private void OnEnable()
        {
            while (gameObject.GetComponents<FixedJoint>().Length > 1)
                gameObject.GetComponent<FixedJoint>().breakForce = 0;
        }

        protected bool IsAttachedToVehicle()
        {
            Transform root = transform.root;
            return root != null && root.gameObject.name != "CARPARTS";
        }
    }
}
