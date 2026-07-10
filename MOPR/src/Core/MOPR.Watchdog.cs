// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Сторож живучести: каждые 10 секунд проверяет, что главный цикл ещё «тикает». Если цикл замер —
// перезапускает его до нескольких раз, после чего включает безопасный режим (всё включает обратно).

using System;
using System.Collections;
using UnityEngine;
using MOPR.Common;

namespace MOPR
{
    internal partial class Core : MonoBehaviour
    {
        #region Контроль системы и защита от зависаний

        private int ticks;
        public int Tick => ticks;
        private int lastTick;
        private int retries;
        private const int MaxRetries = 3;
        private bool restartSucceedMessaged;
        private IEnumerator currentControlCoroutine;

        /// <summary>
        /// Раз в 10 секунд сверяет ticks и lastTick. Равны — цикл остановился, пробуем перезапустить;
        /// после исчерпания попыток включаем безопасный режим.
        /// </summary>
        private IEnumerator ControlCoroutine()
        {
            while (MoprSettings.IsModActive)
            {
                yield return new WaitForSeconds(10);

                if (lastTick == ticks)
                {
                    if (retries >= MaxRetries)
                    {
                        ModConsole.LogError("[MOPR] Restart attempts failed. Pausing optimization (failsafe).");
                        ModConsole.LogError("[MOPR] Please use the \"I Found a Bug\" button in MOPR settings.");

                        // Возвращаем всё во включённое состояние и останавливаем оптимизацию.
                        try
                        {
                            ToggleAll(true);
                        }
                        catch (Exception ex)
                        {
                            ExceptionManager.New(ex, false, "FAILSAFE_TOGGLE_ALL_ERROR");
                        }

                        MoprSettings.IsModActive = false;
                        yield break;
                    }

                    retries++;
                    restartSucceedMessaged = false;
                    ModConsole.LogWarning("[MOPR] MOPR has stopped working! Restart attempt " + retries + "/" + MaxRetries + "...");
                    StopCoroutine(currentLoop);
                    currentLoop = LoopRoutine();
                    StartCoroutine(currentLoop);
                }
                else
                {
                    lastTick = ticks;
                }
            }
        }

        #endregion
    }
}
