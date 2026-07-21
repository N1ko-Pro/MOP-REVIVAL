// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Префикс-методы Harmony для движковых оптимизаций (идея — Reharmonization, Horsey4):
//   * Transform-сеттеры (position/localPosition/eulerAngles/localEulerAngles) — no-op, если значение
//     не изменилось: игра и её FSM выставляют трансформы каждый кадр, часто тем же значением, а это
//     каждый раз «пачкает» иерархию и синхронизацию физики.
//   * ActionHelpers.MousePick/IsMouseOver — кэш рейкастов по КАЖДОЙ маске за кадр. Встроенный кэш
//     игры держит лишь один (distance, layerMask) и «пробуксовывает» при чередовании масок.
//   * Fsm.UpdateDelayedEvents — точная замена оригинала (снимок delayedEvents перед тиком: Update()
//     может отправить событие и реентрантно изменить список, поэтому удаляем завершённые после прохода).
// Все префиксы возвращают false, чтобы заменить оригинал, сохраняя его наблюдаемое поведение.

using System.Collections.Generic;
using UnityEngine;
using HutongGames.PlayMaker;

namespace MOPR.Performance.Engine
{
    internal static class GamePatches
    {
        #region Transform-сеттеры (no-op при неизменном значении)

        private static bool Transform_set_position(Transform __instance, Vector3 value)
        {
            return __instance.position != value; // true → выполнить оригинал; false → пропустить
        }

        private static bool Transform_set_localPosition(Transform __instance, Vector3 value)
        {
            return __instance.localPosition != value;
        }

        private static bool Transform_set_eulerAngles(Transform __instance, Vector3 value)
        {
            // Unity всё равно хранит поворот кватернионом; ставим rotation напрямую и только при отличии.
            Quaternion q = Quaternion.Euler(value);
            if (__instance.rotation != q)
                __instance.rotation = q;
            return false;
        }

        private static bool Transform_set_localEulerAngles(Transform __instance, Vector3 value)
        {
            return __instance.localEulerAngles != value;
        }

        #endregion

        #region MousePick (мульти-маска кэш рейкастов)

        private static bool ActionHelpers_MousePick(ref RaycastHit __result, float distance, int layerMask)
        {
            if (RaycastCache.Raycast(out RaycastHit hit, distance, layerMask))
                __result = hit;
            return false;
        }

        private static bool ActionHelpers_IsMouseOver(ref bool __result, GameObject gameObject, float distance, int layerMask)
        {
            if (RaycastCache.Raycast(out RaycastHit hit, distance, layerMask))
                __result = hit.collider != null && hit.collider.gameObject == gameObject;
            return false;
        }

        #endregion

        #region FSM: обновление отложенных событий

        // Снимок-буферы, чтобы не аллоцировать список каждый кадр. Метод вызывается из главного
        // потока Unity (FsmComponent.Update), поэтому статические буферы безопасны.
        private static readonly List<DelayedEvent> updateBuffer = new List<DelayedEvent>();
        private static readonly List<DelayedEvent> removeBuffer = new List<DelayedEvent>();

        // ВАЖНО: DelayedEvent.Update() не просто тикает таймер — по истечении он ОТПРАВЛЯЕТ событие
        // (Fsm.Event → UpdateStateChanges), а это может СТАРТОВАТЬ новые состояния и добавить/убрать
        // отложенные события ТОГО ЖЕ FSM. Поэтому оригинал PlayMaker снимает копию delayedEvents,
        // тикает по копии, а удаляет завершённые после прохода. Если тикать по «живому» списку и
        // удалять на месте (RemoveAt(i--)), реентрантная мутация внутри Update() приводит к пропуску
        // или потере событий. Практический симптом в MSC: при засыпании отложенное событие «проснуться/
        // вернуть картинку» теряется — экран остаётся чёрным навсегда, HUD/звук/жесты пропадают, игра
        // при этом не падает. Здесь точно воспроизводим безопасный по реентрантности алгоритм оригинала.
        private static bool Fsm_UpdateDelayedEvents(Fsm __instance)
        {
            List<DelayedEvent> delayed = __instance.DelayedEvents;

            updateBuffer.Clear();
            removeBuffer.Clear();
            updateBuffer.AddRange(delayed);

            for (int i = 0; i < updateBuffer.Count; i++)
            {
                DelayedEvent e = updateBuffer[i];
                e.Update();
                if (e.Finished)
                    removeBuffer.Add(e);
            }

            for (int i = 0; i < removeBuffer.Count; i++)
                delayed.Remove(removeBuffer[i]);

            updateBuffer.Clear();
            removeBuffer.Clear();
            return false;
        }

        #endregion
    }

    /// <summary>
    /// Покадровый кэш «мышиных» рейкастов по слою-маске. Луч из камеры пересчитывается один раз за
    /// кадр; результаты хранятся по layerMask. Если ранее уже был промах на меньшую дистанцию —
    /// продолжаем луч дальше вместо полного повторного рейкаста (порт логики Reharmonization).
    /// </summary>
    internal static class RaycastCache
    {
        private static readonly Dictionary<int, RaycastHit> hits = new Dictionary<int, RaycastHit>();
        private static Camera camera;
        private static Ray ray;
        private static bool recalculate = true;
        private static int lastFrame = -1;

        public static bool Raycast(out RaycastHit hit, float distance, int layerMask)
        {
            int frame = Time.frameCount;
            if (frame != lastFrame)
            {
                lastFrame = frame;
                hits.Clear();
                recalculate = true;
            }

            if (recalculate)
            {
                recalculate = false;
                camera = Camera.main;
                if (camera != null)
                    ray = camera.ScreenPointToRay(Input.mousePosition);
            }

            if (hits.TryGetValue(layerMask, out RaycastHit cached))
            {
                if (cached.collider != null || cached.distance >= distance)
                {
                    hit = cached;
                    return hit.collider != null && cached.distance <= distance;
                }

                // Ранее был промах до cached.distance — продолжаем луч дальше.
                Physics.Raycast(ray.origin + ray.direction * cached.distance, ray.direction, out hit, distance - cached.distance, layerMask);
                hit.distance += cached.distance;
                hits[layerMask] = hit;
                return hit.collider != null;
            }

            if (camera != null)
            {
                Physics.Raycast(ray, out hit, distance, layerMask);
                hits[layerMask] = hit;
                return hit.collider != null;
            }

            hit = default(RaycastHit);
            return false;
        }
    }
}
