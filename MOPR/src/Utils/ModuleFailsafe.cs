// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Пер-модульный предохранитель (идея — DiagnosticGuard из MSWCOptimization). Оборачивает
// периодическую работу подсистемы в try/catch и, если модуль подряд падает N раз, отключает его,
// чтобы один сбойный оптимизатор не спамил ошибками и не мешал остальным. Дополняет глобальный
// watchdog ядра точечной изоляцией отдельных модулей.

using System;

namespace MOPR.Common
{
    internal sealed class ModuleFailsafe
    {
        private readonly string tag;
        private readonly int threshold;
        private int consecutiveErrors;

        public bool Disabled { get; private set; }

        public ModuleFailsafe(string tag, int threshold = 5)
        {
            this.tag = tag;
            this.threshold = threshold;
        }

        /// <summary>Выполняет работу модуля; при повторных сбоях гасит модуль.</summary>
        public void Run(Action work)
        {
            if (Disabled)
                return;

            try
            {
                work();
                consecutiveErrors = 0;
            }
            catch (Exception ex)
            {
                consecutiveErrors++;
                ExceptionManager.New(ex, false, tag);

                if (consecutiveErrors >= threshold)
                {
                    Disabled = true;
                    ModConsole.LogWarning("[MOPR] Module '" + tag + "' disabled after repeated errors (failsafe).");
                }
            }
        }
    }
}
