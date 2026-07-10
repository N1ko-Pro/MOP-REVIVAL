// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Петли деталей Сатсумы (двери, багажник, блок): при выгрузке возвращает исходную позу, а после
// включения восстанавливает ту, что была на момент выключения. Отслеживает, прикреплена ли деталь
// к машине, и убирает дублирующиеся суставы после включения.

using System.Collections;
using UnityEngine;

using MOPR.Vehicles.Cases;

namespace MOPR.Vehicles.Managers.SatsumaManagers
{
    internal class SatsumaHingeManager : MonoBehaviour
    {
        private Quaternion initialLocalRotation;
        private Vector3 initialLocalPosition;

        private Quaternion localRotationOnDisable;
        private Vector3 localPositionOnDisable;

        private bool isDisabled, isAssembled;

        private void Awake()
        {
            GetDefaultPosition();
            isAssembled = IsAssembledToTheCar();
        }

        private void GetDefaultPosition()
        {
            initialLocalRotation = transform.localRotation;
            initialLocalPosition = transform.localPosition;
        }

        private void OnDisable()
        {
            if (!isAssembled)
                return;

            localRotationOnDisable = transform.localRotation;
            transform.localRotation = initialLocalRotation;

            localPositionOnDisable = transform.localPosition;
            transform.localPosition = initialLocalPosition;

            isDisabled = true;
        }

        private void OnEnable()
        {
            StartCoroutine(HingeFix());
        }

        private void Update()
        {
            if (!IsAssembledToTheCar() && isAssembled)
                isAssembled = false;

            if (IsAssembledToTheCar() && transform.parent.gameObject.name != "ItemPivot" && !isAssembled)
            {
                GetDefaultPosition();
                isAssembled = true;
            }

            if (isAssembled && isDisabled)
            {
                isDisabled = false;
                transform.localRotation = localRotationOnDisable;
                transform.localPosition = localPositionOnDisable;
            }
        }

        private bool IsAssembledToTheCar()
        {
            return transform.root != null && transform.root == Satsuma.Instance.transform;
        }

        private IEnumerator HingeFix()
        {
            yield return new WaitForSeconds(1);

            FixedJoint[] fixedJoints = gameObject.GetComponents<FixedJoint>();
            HingeJoint[] hingeJoints = gameObject.GetComponents<HingeJoint>();

            while (fixedJoints.Length > 1)
                Destroy(fixedJoints[0]);

            while (hingeJoints.Length > 1)
                Destroy(hingeJoints[0]);
        }
    }
}
