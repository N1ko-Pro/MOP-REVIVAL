// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Защита от катастрофической порчи сейва прямо из главного меню (до загрузки сохранения). Если
// текущий сейв выглядит обнулённым/обрезанным, а исправный бэкап есть — сразу предлагает
// восстановить самый свежий бэкап, спасая 0-байтовый сейв ещё до попытки загрузки.

using System;
using UnityEngine;
using MSCLoader;
using MOPR.Common;
using MOPR.Localization;

namespace MOPR.Saves
{
    internal static class SaveGuard
    {
        private static bool checkedThisSession;

        /// <summary>Проверка порчи из главного меню. Выполняется один раз за сессию игры.</summary>
        public static void CheckOnMenu()
        {
            if (checkedThisSession)
                return;
            checkedThisSession = true;

            try
            {
                if (!MoprSettings.SaveProtectionOn)
                    return;

                if (!SaveProtection.LooksCorrupt(out long current, out long backup))
                    return;

                ModConsole.LogWarning("[MOPR] " + LocalizationCore.Get("log.save_corrupt", current, backup));

                string message = LocalizationCore.Get("save.corrupt_msg", current, backup);
                ModUI.ShowCustomMessage(
                    message,
                    LocalizationCore.Get("save.verify_title"),
                    new[]
                    {
                        ModUI.CreateMessageBoxBtn(LocalizationCore.Get("common.yes"), RestoreLatest),
                        ModUI.CreateMessageBoxBtn(LocalizationCore.Get("common.no"))
                    },
                    new MsgBoxBtn[] { });
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "SAVE_GUARD_CHECK_ERROR");
            }
        }

        private static void RestoreLatest()
        {
            bool ok = SaveProtection.RestoreLatest();
            if (ok)
                TryShowContinueButton();

            ModUI.ShowMessage(
                LocalizationCore.Get(ok ? "save.restored_msg" : "save.restore_failed"),
                LocalizationCore.Get("save.verify_title"));
        }

        /// <summary>
        /// Возвращает кнопку «Продолжить» в главном меню после восстановления. Игра решает, показывать
        /// ли её при построении меню (когда сейв был ещё сломан), поэтому включаем через родителя
        /// (GameObject.Find не находит неактивную кнопку напрямую). Вне главного меню — no-op.
        /// </summary>
        public static void TryShowContinueButton()
        {
            try
            {
                GameObject ui = GameObject.Find("Interface");
                if (ui == null)
                    return;

                Transform continueButton = ui.transform.Find("Buttons/ButtonContinue");
                if (continueButton != null && !continueButton.gameObject.activeSelf)
                {
                    continueButton.gameObject.SetActive(true);
                    ModConsole.Log("[MOPR] " + LocalizationCore.Get("log.continue_restored"));
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "SAVE_GUARD_CONTINUE_ERROR");
            }
        }
    }
}
