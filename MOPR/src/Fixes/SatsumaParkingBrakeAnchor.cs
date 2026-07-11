// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Необязательный фикс: «якорь ручника» для Сатсумы. В ваниле машина с полностью поднятым ручником
// всё равно медленно скатывается/«плывёт» на склонах. Пока ручник поднят и машина уже стоит,
// компонент подмораживает её rigidbody, а при опускании ручника — сразу отпускает. Идея взята из
// мода "Parking Brake Anchor Only" (Alex.T), но переписана заново.
//
// Почему так, а не как в оригинале (тот телепортировал transform и обнулял скорости КАЖДЫЙ
// FixedUpdate, а объекты искал через Resources.FindObjectsOfTypeAll + LINQ каждый тик — просадки и
// джиттер):
//   * ссылки берутся один раз (с троттлингом) из Satsuma.Instance и по фиксированному пути рычага —
//     без глобального перебора объектов;
//   * FsmFloat кэшируется, а не ищется по имени каждый тик;
//   * якорь — это переключение rigidbody.constraints ТОЛЬКО на смене состояния (нативно, без борьбы
//     с физикой и без джиттера), стоимость на кадр практически нулевая;
//   * якорим лишь когда машина реально почти неподвижна, поэтому нет резкого «щелчка» на ходу.
//
// Домкрат и физика: замораживаем ТОЛЬКО горизонтальное перемещение (X, Z). Это гасит скатывание по
// склону, но оставляет свободными вертикаль (Y) и вращение — поэтому машину можно спокойно поднять
// домкратом (подъём идёт по Y, наклон — это вращение) и она не соскальзывает с него. Полная заморозка
// (FreezeAll) блокировала бы домкрат, а заморозка всей позиции — подъём по Y, поэтому только X/Z.
//
// Сохранение/загрузка: rigidbody.constraints — рантайм-состояние, оно НЕ пишется в сейв (ES2 хранит
// только трансформ). Компонент завязан на MoprSettings.IsModActive, который MOP выставляет в false в
// PreSaveGame до записи сейва — значит на момент сохранения машина уже в естественном состоянии, а
// после возобновления цикла (в т.ч. после загрузки) якорь включается заново по состоянию ручника.

using UnityEngine;
using HutongGames.PlayMaker;
using MOPR.Common;
using MOPR.Vehicles.Cases;

namespace MOPR.Fixes
{
    internal sealed class SatsumaParkingBrakeAnchor : MonoBehaviour
    {
        // Путь к рычагу ручника от корня Сатсумы (совпадает с тем, что использует Satsuma.Setup).
        private const string LeverPath = "MiscParts/HandBrake/handbrake(xxxxx)/handbrake lever";

        private const float EngageThreshold = 0.9f;  // Рычаг поднят почти полностью (как в оригинале).
        private const float StillSqrSpeed = 0.04f;   // (0.2 м/с)^2 — на большей скорости не якорим.
        private const float RetrySeconds = 2f;        // Как часто пробуем найти ссылки, пока не нашли.

        // Гасим горизонтальный «уезд» (X, Z) и разворот вокруг вертикали (yaw), чтобы сильный боковой
        // удар не закручивал запаркованную машину на месте. Подъём по Y и наклон (pitch/roll) остаются
        // свободными — поэтому её всё ещё можно домкратить.
        private const RigidbodyConstraints AnchorConstraints =
            RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationY;

        private Rigidbody rb;
        private FsmFloat brake;
        private RigidbodyConstraints originalConstraints;
        private bool acquired;
        private bool anchored;
        private float retryTimer;

        private void FixedUpdate()
        {
            // Пока MOP «спит» (в т.ч. предсохранение/аварийный стоп) или фикс выключен — не якорим и
            // снимаем якорь, если он был. Это же обеспечивает корректный сейв (машина без constraints).
            if (!MoprSettings.IsModActive || !MoprSettings.ParkingBrakeAnchorOn)
            {
                if (anchored)
                    Release();
                return;
            }

            if (!acquired)
            {
                retryTimer -= Time.unscaledDeltaTime;
                if (retryTimer > 0f)
                    return;

                retryTimer = RetrySeconds;
                TryAcquire();
                if (!acquired)
                    return;
            }

            // Ссылки могли инвалидироваться (сброс/пересоздание объекта) — переакквайрим.
            if (rb == null || brake == null)
            {
                acquired = false;
                anchored = false;
                return;
            }

            bool engaged = brake.Value >= EngageThreshold;

            if (engaged && !anchored)
            {
                // Не якорим машину на ходу — ждём, пока она почти остановится.
                if (!rb.isKinematic && rb.velocity.sqrMagnitude > StillSqrSpeed)
                    return;

                Anchor();
            }
            else if (!engaged && anchored)
            {
                Release();
            }
        }

        private void TryAcquire()
        {
            Satsuma satsuma = Satsuma.Instance;
            if (satsuma == null)
                return;

            Transform root = satsuma.transform;
            if (root == null)
                return;

            if (rb == null)
            {
                rb = root.GetComponent<Rigidbody>();
                if (rb == null && root.root != null)
                    rb = root.root.GetComponent<Rigidbody>();
                if (rb == null)
                    return;

                originalConstraints = rb.constraints;
            }

            if (brake == null)
            {
                Transform lever = root.Find(LeverPath);
                if (lever == null)
                    return;

                foreach (PlayMakerFSM fsm in lever.GetComponents<PlayMakerFSM>())
                {
                    FsmFloat f = fsm.FsmVariables.GetFsmFloat("Brake");
                    if (f != null)
                    {
                        brake = f;
                        break;
                    }
                }

                if (brake == null)
                    return;
            }

            acquired = true;
        }

        private void Anchor()
        {
            // Гасим остаточную скорость один раз, чтобы при снятии якоря машину не «дёрнуло».
            if (!rb.isKinematic)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            // Добавляем свои оси к тем, что уже могли быть выставлены (не затираем чужие constraints).
            rb.constraints = originalConstraints | AnchorConstraints;
            anchored = true;
        }

        private void Release()
        {
            if (rb != null)
                rb.constraints = originalConstraints;

            anchored = false;
        }

        // Страховка: не оставляем машину подмороженной, если компонент выключили/уничтожили.
        private void OnDisable()
        {
            if (anchored)
                Release();
        }
    }
}
