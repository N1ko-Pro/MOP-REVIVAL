// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Ручной газ Gifu: активен, только пока в замке зажигания есть ключ. Ставит стартовое значение
// ручки и включает/выключает обновление газа по наличию ключа.

using UnityEngine;
using System;

namespace MOPR.Vehicles.Managers
{
    internal class GifuHandThrottle : HandThrottle
    {
        private readonly GameObject key;
        private bool isInvoked;

        public GifuHandThrottle() : base("LOD/Dashboard/ButtonHandThrottle")
        {
            try
            {
                key = transform.Find("LOD/Dashboard/KeyHole/Keys/Key").gameObject;
                handThrottleValue.Value = 0.13f;
            }
            catch
            {
                throw new Exception("GifuHandThrottle: Key not found.");
            }
        }

        private void Update()
        {
            if (isInvoked && !key.activeSelf)
            {
                isInvoked = false;
                CancelInvoke();
            }
            else if (!isInvoked && key.activeSelf)
            {
                isInvoked = true;
                Invoke();
            }
        }
    }
}
