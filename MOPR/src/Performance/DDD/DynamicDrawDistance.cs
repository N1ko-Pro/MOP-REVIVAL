// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Динамическая дальность прорисовки: управляет far clip главной камеры, модулируя выбранную игроком
// дистанцию. Обычно — ровно игровая настройка; высоко над землёй (гора/трамплин/крыша/полёт) при
// пресете Balanced+ поднимаем к максимуму, чтобы не срезать горизонт; в замкнутом секторе режем до
// дистанции сектора. Значение доводится до цели плавно (без «попа»), уровень земли отслеживается
// адаптивно. Меняется только Camera.farClipPlane — состояние игры/FSM/сейвы не трогаются.

using System;
using UnityEngine;

using MOPR.Common;
using MOPR.Common.Enumerations;
using MOPR.FSM;
using MOPR.Managers;

namespace MOPR.Helpers
{
    internal class DynamicDrawDistance : MonoBehaviour
    {
        private const float MaxDrawDistance = 5000f;      // предел высотного буста
        private const float AltitudeForBoost = 20f;       // метров над локальной землёй для максимизации
        private const float GroundReclaimPerSecond = 1f;  // скорость «подъёма» отслеживаемого уровня земли
        private const float LerpMetresPerSecond = 2500f;  // скорость сглаживания far clip

        private Camera mainCamera;
        private float capturedFarClip;
        private bool captured;

        private float groundY;
        private bool groundInitialized;

        private bool errorLogged;

        private void Update()
        {
            try
            {
                if (mainCamera == null)
                {
                    mainCamera = Camera.main;
                    if (mainCamera == null)
                        return;
                }

                if (!captured)
                {
                    capturedFarClip = mainCamera.farClipPlane;
                    captured = true;
                }

                float dt = Time.unscaledDeltaTime;
                float game = GetGameDrawDistance();

                // Мод или DDD отключены — плавно возвращаем игровую дистанцию.
                if (!MoprSettings.IsModActive || !MoprSettings.DynamicDrawDistanceOn)
                {
                    EaseFarClip(game, dt);
                    return;
                }

                bool farBoost = MoprSettings.Mode >= PerformanceMode.Balanced && TrackAltitude() > AltitudeForBoost;

                bool inSector = false;
                float sectorDistance = game;
                if (!farBoost && SectorManager.Instance != null && SectorManager.Instance.IsPlayerInSector())
                {
                    inSector = true;
                    sectorDistance = SectorManager.Instance.GetCurrentSectorDrawDistance();
                }

                float target = DrawDistanceMath.ComputeTarget(game, farBoost, MaxDrawDistance, inSector, sectorDistance);
                EaseFarClip(target, dt);
            }
            catch (Exception ex)
            {
                // Не глушим DDD навсегда: логируем один раз и продолжаем.
                if (!errorLogged)
                {
                    errorLogged = true;
                    ExceptionManager.New(ex, false, "DRAW_DISTANCE_ERROR");
                }
            }
        }

        /// <summary>Игровая дистанция прорисовки; при сбое — захваченное исходное значение.</summary>
        private float GetGameDrawDistance()
        {
            try
            {
                float value = FsmManager.GetDrawDistance();
                if (value > 1f)
                    return value;
            }
            catch
            {
            }

            return capturedFarClip;
        }

        /// <summary>
        /// Высота камеры над локальной землёй. Уровень земли отслеживает недавний минимум высоты
        /// (падает мгновенно, поднимается медленно) — адаптируется к рельефу без фикс. «уровня моря».
        /// </summary>
        private float TrackAltitude()
        {
            float y = mainCamera.transform.position.y;

            if (!groundInitialized)
            {
                groundY = y;
                groundInitialized = true;
                return 0f;
            }

            if (y < groundY)
                groundY = y;
            else
                groundY = Mathf.MoveTowards(groundY, y, GroundReclaimPerSecond * Time.unscaledDeltaTime);

            float above = y - groundY;
            return above > 0f ? above : 0f;
        }

        private void EaseFarClip(float target, float dt)
        {
            if (Mathf.Approximately(mainCamera.farClipPlane, target))
                return;

            mainCamera.farClipPlane = Mathf.MoveTowards(mainCamera.farClipPlane, target, LerpMetresPerSecond * dt);
        }

        private void OnDisable() => RestoreFarClip();
        private void OnDestroy() => RestoreFarClip();

        private void RestoreFarClip()
        {
            try
            {
                Camera cam = mainCamera != null ? mainCamera : Camera.main;
                if (cam != null)
                    cam.farClipPlane = GetGameDrawDistance();
            }
            catch
            {
            }
        }
    }
}
