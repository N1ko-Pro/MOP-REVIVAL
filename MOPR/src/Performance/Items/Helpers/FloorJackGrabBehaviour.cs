// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Помощник напольного домкрата: пока игрок несёт домкрат рядом с Сатсумой, временно «ужимает»
// коллайдер подъёмника по вертикали, чтобы он не цеплялся за машину при подведении. Возвращает
// исходный размер, когда домкрат отпущен/отведён.

using UnityEngine;
using MOPR.Vehicles.Cases;

namespace MOPR.Items.Helpers
{
    internal class FloorJackGrabBehaviour : MonoBehaviour
    {
        private const float MinimalDistanceToSatsuma = 2.5f; // ближе — считаем «у машины».
        private const float NewLifterY = 0.01f;              // ужатая высота коллайдера подъёмника.

        private Rigidbody rb;
        private BoxCollider lifterCollider;
        private Vector3 lifterDefaultSize;
        private bool isActionApplied;

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            lifterCollider = transform.Find("Lifter/lift").GetComponent<BoxCollider>();
            lifterDefaultSize = lifterCollider.size;
        }

        private void Update()
        {
            if (IsGrabbed() && IsCloseToSatsuma())
                OnMove();
            else
                OnMoveStop();
        }

        /// <summary>Домкрат держат в руках (у него есть скорость).</summary>
        private bool IsGrabbed()
        {
            return rb.velocity.magnitude > 0;
        }

        /// <summary>Домкрат рядом с Сатсумой.</summary>
        private bool IsCloseToSatsuma()
        {
            return Vector3.Distance(Satsuma.Instance.transform.position, transform.position) < MinimalDistanceToSatsuma;
        }

        private void OnMove()
        {
            if (isActionApplied)
                return;

            isActionApplied = true;

            // Ужимаем высоту коллайдера подъёмника, оставляя остальные размеры прежними.
            Vector3 small = lifterDefaultSize;
            small.y = NewLifterY;
            lifterCollider.size = small;
        }

        private void OnMoveStop()
        {
            if (!isActionApplied)
                return;

            isActionApplied = false;
            lifterCollider.size = lifterDefaultSize;
        }
    }
}
