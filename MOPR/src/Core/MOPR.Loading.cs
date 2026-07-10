// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Загрузка окружения: отложенный старт (даём физике устояться), проверка полной догрузки Сатсумы с
// разовым рестартом сцены при необходимости, завершение загрузки и фейл-сейф загрузочного экрана.

using System;
using System.Collections;
using UnityEngine;
using MOPR.Common;
using MOPR.Common.Enumerations;
using MOPR.FSM;
using MOPR.Helpers;
using MOPR.Vehicles.Cases;

namespace MOPR
{
    internal partial class Core : MonoBehaviour
    {
        #region Загрузка и инициализация окружения

        /// <summary>Кадры ожидания перед инициализацией, чтобы сцена успела расставиться.</summary>
        private const int InitializationDelayFrames = 200;

        private readonly IEnumerator loadScreenWorkaround;

        /// <summary>Ждёт стабилизации сцены, проверяет Сатсуму и запускает инициализацию.</summary>
        private IEnumerator DelayedInitializationRoutine()
        {
            for (int i = 0; i < InitializationDelayFrames; i++)
                yield return null;

            CheckIfSatsumaIsLoaded();

            // Ждём завершения проверки Сатсумы, но не дольше лимита.
            for (int i = 0; i < WaitForSatsumaCheckTime; ++i)
            {
                if (isFinishedCheckingSatsuma)
                    break;

                yield return new WaitForSeconds(1);
            }

            Initialize();
        }

        /// <summary>Проверяет, что игра полностью загрузила Сатсуму; иначе — разовый рестарт сцены.</summary>
        private void CheckIfSatsumaIsLoaded()
        {
            bool satsumaIsLoaded = false;

            if (SaveManager.IsCarAssembledWithMSCEditor())
            {
                MSCLoader.ModUI.ShowMessage("MSCEditor (or another third-party software) has been used to assemble the car. " +
                    "This might cause MOPR to not work as intended.", "MOPR - Warning");
            }
            else
            {
                try
                {
                    satsumaIsLoaded = SaveManager.IsSatsumaLoadedCompletely();
                }
                catch (Exception ex)
                {
                    ExceptionManager.New(ex, true, "SATSUMA_IS_LOADED_ERROR");
                }

                if (MoprSettings.ForceLoadRestart)
                    satsumaIsLoaded = false;

                if (!satsumaIsLoaded)
                {
                    if (MoprSettings.GameFixStatus == GameFixStatus.None)
                    {
                        StartCoroutine(GameRestartCoroutine());
                        return;
                    }

                    // Фикс уже пробовали — просто предупреждаем игрока.
                    MSCLoader.ModUI.ShowMessage("Satsuma has not been fully loaded by the game!\n\n" +
                                                "Consider restarting the game in order to avoid any issues.",
                                                "MOPR");
                }
            }

            isFinishedCheckingSatsuma = true;
        }

        /// <summary>Перезапускает сцену, когда Сатсума не догрузилась.</summary>
        private IEnumerator GameRestartCoroutine()
        {
            // На MSCLoader ждём завершения загрузчика, иначе сломается.
            GameObject mscloaderLoadscreen = GameObject.Find("MSCLoader Canvas loading").transform.Find("MSCLoader loading dialog").gameObject;
            ModConsole.Log("[MOPR] Waiting for the MSCLoader to finish to load...");

            while (mscloaderLoadscreen.activeSelf)
                yield return null;

            yield return null;

            MoprSettings.GameFixStatus = GameFixStatus.DoFix;
            ModConsole.Log("[MOPR] Attempting to restart the scene...");
            Application.LoadLevel(1);
        }

        /// <summary>Завершает загрузку: снимает экран, возвращает управление игроку.</summary>
        private void FinishLoading()
        {
            if (loadScreenWorkaround != null)
                StopCoroutine(loadScreenWorkaround);

            itemInitializationDelayDone = true;
            loadScreen?.Deactivate();
            playerController.enabled = true;
            FsmManager.PlayerInMenu = false;

            GameObject vh = GameObject.Find("VehiclesHighway");
            if (vh)
                vh.SetActive(false);
        }

        /// <summary>Фейл-сейф: если загрузка не завершилась вовремя — снимаем экран принудительно.</summary>
        private IEnumerator InfiniteLoadscreenWorkaround()
        {
            yield return new WaitForSeconds(LoadScreenWorkaroundTime);
            if (FsmManager.PlayerInMenu)
            {
                ModConsole.LogError("[MOPR] MOPR failed to load in time. Please go into MOPR settings and use \"I found a bug\" button.");
                FinishLoading();
                Satsuma.Instance?.ToggleActive(true);
                Satsuma.Instance?.ForceToggleUnityCar(true);
                FsmManager.PlayerInMenu = false;
            }
        }

        #endregion
    }
}
