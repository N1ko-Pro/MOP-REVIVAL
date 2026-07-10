// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Фикс автобуса, заваливающегося на бок: раз в 5с проверяет крен по Z и, если игрок достаточно
// далеко (>300 м), выравнивает угол обратно в 0.

using System.Collections;
using UnityEngine;
using MOPR.Common;

namespace MOPR.Vehicles.Managers
{
    internal class BusRollFix : MonoBehaviour
    {
        private IEnumerator currentPositionFixRoutine;

        private void OnEnable()
        {
            if (currentPositionFixRoutine != null)
            {
                StopCoroutine(currentPositionFixRoutine);
                currentPositionFixRoutine = null;
            }

            currentPositionFixRoutine = PositionFixRoutine();
            StartCoroutine(currentPositionFixRoutine);
        }

        private IEnumerator PositionFixRoutine()
        {
            while (MoprSettings.IsModActive)
            {
                yield return new WaitForSeconds(5);

                if (transform.localEulerAngles.z > 20 && transform.localEulerAngles.z < 340)
                {
                    // Не выравниваем, если игрок близко.
                    if (Vector3.Distance(Core.Instance.GetPlayer().position, transform.position) < 300)
                        continue;

                    Vector3 fixedPosition = transform.localEulerAngles;
                    fixedPosition.z = 0;
                    transform.localEulerAngles = fixedPosition;
                }
            }
        }
    }
}
