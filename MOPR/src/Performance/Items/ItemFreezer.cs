// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Замораживает предмет на месте на время записи сохранения, чтобы он не сместился и не улетел
// (kinematic + FreezeAll + фиксация позы). Если предмет двигался, обновляет сохранённую позу.

using UnityEngine;

namespace MOPR.Items
{
    internal class ItemFreezer : MonoBehaviour
    {
        private Vector3 position;
        private Quaternion rotation;
        private readonly Rigidbody rb;

        // Предметы, которые двигались, останавливаясь, иначе телепортировались бы в исходную позу.
        private bool hasBeenMoving;

        public ItemFreezer()
        {
            position = transform.position;
            rotation = transform.rotation;
            rb = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            if (rb != null && rb.velocity.magnitude > 0.1f)
            {
                hasBeenMoving = true;
                return;
            }

            if (hasBeenMoving)
            {
                position = transform.position;
                rotation = transform.rotation;
            }

            if (rb != null)
            {
                rb.isKinematic = true;
                rb.constraints = RigidbodyConstraints.FreezeAll;
            }

            transform.position = position;
            transform.rotation = rotation;
        }
    }
}
