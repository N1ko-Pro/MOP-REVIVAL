// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Вращает сосиску/картошку в микроволновке Тэймо: после короткой паузы крутит объект по Z в течение
// заданного времени. Таймер сбрасывается при каждом включении.

using UnityEngine;

namespace MOPR.Places.Cases.Misc
{
    internal class SausageInMicrowaveBehaviour : MonoBehaviour
    {
        private const float RotateSpeed = 0.5f;
        private const float WaitTime = 4f;
        private const float RunTime = 75f;

        private float timer;

        private void FixedUpdate()
        {
            timer += Time.deltaTime;
            if (timer < WaitTime)
                return;

            if (timer < RunTime)
            {
                Vector3 euler = transform.localEulerAngles;
                euler.z += RotateSpeed;
                transform.localEulerAngles = euler;
            }
        }

        private void OnEnable()
        {
            timer = 0;
        }
    }
}
