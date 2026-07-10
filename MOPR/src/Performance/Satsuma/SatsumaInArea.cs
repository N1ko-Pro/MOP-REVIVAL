// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Триггер-зона: сообщает Сатсуме, находится ли она в области объекта (инспекция / парк-ферме).
// Пока Сатсума в такой зоне, её физику выключать нельзя.

using UnityEngine;
using MOPR.Vehicles.Cases;

namespace MOPR.Helpers
{
    internal class SatsumaInArea : MonoBehaviour
    {
        private GameObject referenceObject;
        private BoxCollider collider;
        private bool isParcFerme;

        public void Initialize(Vector3 size)
        {
            referenceObject = Satsuma.Instance.GetCarBody();

            collider = gameObject.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size = size;
            collider.transform.position = transform.position;

            isParcFerme = gameObject.name == "MOPR_ParcFermeTrigger";
        }

        private void OnTriggerEnter(Collider other) => Toggle(true, other);
        private void OnTriggerExit(Collider other) => Toggle(false, other);

        private void Toggle(bool enabled, Collider other)
        {
            if (other.gameObject != referenceObject)
                return;

            Satsuma.Instance.IsSatsumaInInspectionArea = enabled;
            if (isParcFerme)
                Satsuma.Instance.IsSatsumaInParcFerme = enabled;
        }
    }
}
