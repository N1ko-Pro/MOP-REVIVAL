// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Якорь ручника Сатсумы (переработка стороннего мода BrakeLockFix / "Parking Brake Anchor Only",
// Alex.T). Ванильный баг: с поднятым ручником машина всё равно медленно сползает по уклону (креном,
// по диагонали), потому что тормозного трения UnityCar не хватает, чтобы удержать её намертво.
//
// Как держим: НЕ телепортом transform (это дорогая пересинхронизация всех коллайдеров Сатсумы каждый
// физический тик — просадка ФПС — плюс видимые скачки), а управлением скоростью. Каждый тик задаём
// velocity/angularVelocity так, чтобы физика сама плавно вернула тело в запомненную позу за шаг. Снос
// компенсируется непрерывно, поэтому не накапливается и не заметен (доли миллиметра).
//
// Домкрат: если рядом с машиной (в радиусе) есть ПОДНЯТЫЙ домкрат — фикс полностью отключается, чтобы
// не мешать физике подъёма. «Поднятость» определяем по двум сигналам (какого-то одного мало для обоих
// типов домкратов, см. дамп сцены References/Game):
//   • car jack(itemx): у него есть FsmFloat "Y" на дочернем Trigger — растёт при подъёме (>= 0.15).
//   • floor jack(itemx): поднимается физической площадкой "Lifter/lift" (у неё свой Rigidbody), а Y
//     триггера подъём не отражает. Поэтому смотрим высоту площадки "lift" относительно корпуса
//     домкрата: как только она поднялась выше уровня покоя — домкрат считается работающим.
// Мини-домкрат, который возят в машине, не поднят (оба сигнала в покое), поэтому якорю не мешает.
//
// Отличия от оригинала: нет просадки ФПС (ссылки кэшируются, в цикле только арифметика; оригинал каждый
// тик искал объекты через FindObjectsOfTypeAll и телепортировал тело), корректная работа с домкратом,
// и якорь встаёт только после остановки.
//
// Компонент висит на корне Сатсумы. Работает только при активной физике (не kinematic) и при включённой
// настройке.

using System.Collections.Generic;
using UnityEngine;
using HutongGames.PlayMaker;

using MOPR.Common;
using MOPR.FSM;

namespace MOPR.Vehicles.Managers.SatsumaManagers
{
    internal sealed class SatsumaParkingBrakeAnchor : MonoBehaviour
    {
        // Ручник считается поднятым при FsmFloat "Brake" >= порога (как в оригинале — 0.9).
        private const float EngageThreshold = 0.9f;

        // Полная скорость (м/с), ниже которой машина считается остановившейся и её можно якорить.
        private const float StopSpeed = 0.4f;

        // Ограничение скорости коррекции: сглаживает возврат, если машину сильно сместили (толкнули).
        private const float MaxCorrectionSpeed = 5f;      // м/с
        private const float MaxCorrectionAngSpeed = 5f;   // рад/с

        // Порог педали газа, выше которого считаем, что игрок пытается ехать (а не просто держит газ).
        private const float ThrottleThreshold = 0.1f;
        // Крутящий момент на колёсах: 0 на нейтрали/выжатом сцеплении, ненулевой — когда передача
        // реально тянет (та же семантика, что в Satsuma.IsOnGround).
        private const float DriveTorqueEpsilon = 0.01f;

        // Радиус вокруг машины, в котором учитываем домкрат (квадрат — для дешёвого сравнения).
        private const float JackRadius = 8f;
        private const float JackRadiusSqr = JackRadius * JackRadius;
        // Подъём FsmFloat "Y" триггера домкрата, выше которого он реально держит (как в MOPR: 0.15).
        private const float JackTriggerYThreshold = 0.15f;
        // Подъём физической площадки домкрата (м) над уровнем покоя, трактуемый как «домкрат поднят».
        private const float JackLiftDelta = 0.1f;
        // Как часто пытаться найти домкраты, пока не нашли (сек).
        private const float JackRetrySeconds = 3f;

        // Минимум колёс на земле, чтобы захватить позу (страховка: не якорим машину, поднятую на домкрате,
        // даже если конкретный домкрат почему-то не распознан).
        private const int MinWheelsOnGround = 3;

        // Пауза после включения физики (переход kinematic→dynamic при подгрузке/возврате к машине),
        // прежде чем снова якорить. Даёт MOPR восстановить машину, а петлям дверей/багажника (капота)
        // переинициализироваться. Если в этот момент дёргать тело скоростью/поворотом — петли могут
        // «сломаться»: звук открытия есть, а движения нет.
        private const float ReengageGrace = 2f;

        // Домкрат: корпус, сигнал Y триггера (может отсутствовать), физическая площадка подъёма и
        // авто-калибруемый «уровень покоя» площадки (бегущий минимум высоты относительно корпуса).
        private sealed class Jack
        {
            public Transform Root;
            public FsmFloat TriggerY;
            public Transform Lift;
            public float RestLiftDelta = float.MaxValue;
        }

        private FsmFloat brake;
        private Rigidbody body;
        private Transform bodyTransform;
        private Axles axles;
        private Drivetrain drivetrain;
        private AxisCarController axisController;

        private bool anchored;
        private Vector3 parkedPos;
        private Quaternion parkedRot;

        private bool wasKinematic = true;   // старт считаем «из выключенной физики» → выдержим паузу.
        private float graceTimer;

        private readonly List<Jack> jacks = new List<Jack>();
        private bool jacksResolved;
        private float jackRetryTimer;

        private void Start()
        {
            try
            {
                Transform lever = transform.Find("MiscParts/HandBrake/handbrake(xxxxx)/handbrake lever");
                PlayMakerFSM leverFsm = lever != null ? lever.GetComponent<PlayMakerFSM>() : null;
                brake = leverFsm != null ? leverFsm.FsmVariables.GetFsmFloat("Brake") : null;
                body = GetComponent<Rigidbody>();
                bodyTransform = body != null ? body.transform : null;
                axles = GetComponent<Axles>();
                drivetrain = GetComponent<Drivetrain>();
                axisController = GetComponent<AxisCarController>();
            }
            catch (System.Exception ex)
            {
                ExceptionManager.New(ex, false, "SATSUMA_PARKING_BRAKE_ANCHOR_INIT_ERROR");
            }

            // Не удалось разрешить ссылки — компонент бесполезен, отключаемся (никакой работы в цикле).
            if (brake == null || body == null)
            {
                ModConsole.Log("[MOPR] Parking brake anchor: handbrake FSM float or rigidbody not found, disabling.");
                enabled = false;
            }
        }

        private void FixedUpdate()
        {
            // Физика выключена (машина kinematic — далеко/выгружена): позу держит сам MOPR. Запоминаем
            // это, чтобы после включения физики выдержать паузу.
            if (body.isKinematic)
            {
                anchored = false;
                wasKinematic = true;
                return;
            }

            // Только что включилась физика (возврат к машине/подгрузка) — запускаем паузу, за которую
            // MOPR восстановит машину, а петли дверей/багажника переинициализируются.
            if (wasKinematic)
            {
                wasKinematic = false;
                graceTimer = ReengageGrace;
            }

            if (graceTimer > 0f)
            {
                graceTimer -= Time.fixedDeltaTime;
                anchored = false;
                return;
            }

            // Якорь не действует: настройка выкл / мод неактивен / ручник опущен.
            if (!MoprSettings.IsModActive || !MoprSettings.ParkingBrakeAnchorOn
                || brake.Value < EngageThreshold)
            {
                anchored = false;
                return;
            }

            // Игрок газует на передаче — отпускаем якорь, чтобы вернуть ванильные рывки: машина
            // дёргается/буксует о поднятый ручник, а не «газует как на нейтрали». Когда газ отпустят
            // и машина остановится — якорь снова возьмётся за текущую позу (без отскока назад).
            if (IsDriveAttempt())
            {
                anchored = false;
                return;
            }

            // Рядом работает поднятый домкрат — ПОЛНОСТЬЮ отключаем фикс, чтобы не мешать физике подъёма
            // (и не заякорить машину «в воздухе»). Мини-домкрат в машине не поднят — игнорируется.
            if (IsJackActiveNear())
            {
                anchored = false;
                return;
            }

            // Захватываем позу только когда машина стоит на земле (не на домкрате) и остановилась.
            if (!anchored)
            {
                if (!IsGrounded())
                    return;

                if (body.velocity.sqrMagnitude > StopSpeed * StopSpeed)
                    return;

                parkedPos = bodyTransform.position;
                parkedRot = bodyTransform.rotation;
                anchored = true;
                return;
            }

            // Плавно тянем позицию и поворот обратно в запомненную позу (через скорость, без телепорта).
            Vector3 posError = parkedPos - bodyTransform.position;
            body.velocity = Vector3.ClampMagnitude(posError / Time.fixedDeltaTime, MaxCorrectionSpeed);
            body.angularVelocity = RotationCorrection();
        }

        /// <summary>Угловая скорость, доводящая поворот тела до parkedRot за один шаг (с ограничением).</summary>
        private Vector3 RotationCorrection()
        {
            Quaternion rotError = parkedRot * Quaternion.Inverse(bodyTransform.rotation);

            float angleDeg;
            Vector3 axis;
            rotError.ToAngleAxis(out angleDeg, out axis);

            // Ось не определена (поворот ~0) или значение вырождено — гасим вращение.
            if (float.IsNaN(axis.x) || float.IsInfinity(axis.x) || angleDeg < 0.001f)
                return Vector3.zero;

            if (angleDeg > 180f)
                angleDeg -= 360f;

            Vector3 angVel = axis.normalized * (angleDeg * Mathf.Deg2Rad / Time.fixedDeltaTime);
            return Vector3.ClampMagnitude(angVel, MaxCorrectionAngSpeed);
        }

        /// <summary>
        /// Игрок пытается ехать на передаче: педаль газа нажата и крутящий момент реально идёт на колёса
        /// (torque != 0 только на включённой передаче со сцеплением; на нейтрали/выжатом сцеплении — 0).
        /// В этот момент якорь отпускаем, чтобы двигатель мог дёргать машину о ручник, как в ванили.
        /// </summary>
        private bool IsDriveAttempt()
        {
            return drivetrain != null && axisController != null
                && axisController.throttle > ThrottleThreshold
                && Mathf.Abs(drivetrain.torque) > DriveTorqueEpsilon;
        }

        /// <summary>Есть ли рядом с машиной поднятый домкрат (в радиусе и в поднятом состоянии).</summary>
        private bool IsJackActiveNear()
        {
            ResolveJacks();

            for (int i = 0; i < jacks.Count; i++)
            {
                Jack jack = jacks[i];
                if (jack.Root == null)
                    continue;

                if (!IsJackRaised(jack))
                    continue;

                if ((jack.Root.position - bodyTransform.position).sqrMagnitude <= JackRadiusSqr)
                    return true;
            }

            return false;
        }

        /// <summary>Поднят ли домкрат: по Y триггера (car jack) или по высоте площадки (floor jack).</summary>
        private static bool IsJackRaised(Jack jack)
        {
            // Сигнал 1: FsmFloat "Y" триггера (надёжен для car jack).
            if (jack.TriggerY != null && jack.TriggerY.Value >= JackTriggerYThreshold)
                return true;

            // Сигнал 2: физическая площадка подъёма поднялась над уровнем покоя (нужно для floor jack).
            if (jack.Lift != null)
            {
                float delta = jack.Lift.position.y - jack.Root.position.y;

                // Авто-калибровка уровня покоя: держим бегущий минимум высоты площадки.
                if (delta < jack.RestLiftDelta)
                    jack.RestLiftDelta = delta;

                if (delta - jack.RestLiftDelta > JackLiftDelta)
                    return true;
            }

            return false;
        }

        /// <summary>Стоит ли машина на земле (достаточно колёс с контактом) — а не поднята на домкрате.</summary>
        private bool IsGrounded()
        {
            // Не можем проверить колёса — считаем, что на земле (безопаснее, чем не якорить никогда).
            if (axles == null || axles.allWheels == null)
                return true;

            int onGround = 0;
            Wheel[] wheels = axles.allWheels;
            for (int i = 0; i < wheels.Length; i++)
            {
                if (wheels[i] != null && wheels[i].onGroundDown)
                    onGround++;
            }

            return onGround >= MinWheelsOnGround;
        }

        /// <summary>
        /// Единожды находит все домкраты сцены (floor/car jack): корпус, сигнал Y триггера и физическую
        /// площадку подъёма ("Lifter/lift" у floor jack, "Lift/lift" у car jack). Ищем через
        /// FindObjectsOfTypeAll (видит и неактивные), с редкими повторами, пока не найдём хотя бы один.
        /// После успеха больше не сканируем — в цикле остаётся лишь дешёвая проверка.
        /// </summary>
        private void ResolveJacks()
        {
            if (jacksResolved)
                return;

            jackRetryTimer -= Time.fixedDeltaTime;
            if (jackRetryTimer > 0f)
                return;

            jackRetryTimer = JackRetrySeconds;

            try
            {
                jacks.Clear();
                GameObject[] all = Resources.FindObjectsOfTypeAll<GameObject>();
                for (int i = 0; i < all.Length; i++)
                {
                    GameObject g = all[i];
                    if (g.name != "floor jack(itemx)" && g.name != "car jack(itemx)")
                        continue;

                    Transform trigger = g.transform.Find("Trigger");
                    PlayMakerFSM triggerFsm = trigger != null ? trigger.GetComponent<PlayMakerFSM>() : null;
                    FsmFloat y = triggerFsm != null ? triggerFsm.FsmVariables.GetFsmFloat("Y") : null;

                    // Физическая площадка подъёма (у floor jack и car jack разные пути).
                    Transform lift = g.transform.Find("Lifter/lift") ?? g.transform.Find("Lift/lift");

                    if (y != null || lift != null)
                        jacks.Add(new Jack { Root = g.transform, TriggerY = y, Lift = lift });
                }

                if (jacks.Count > 0)
                    jacksResolved = true;
            }
            catch (System.Exception ex)
            {
                ExceptionManager.New(ex, false, "SATSUMA_PARKING_BRAKE_ANCHOR_JACK_ERROR");
            }
        }
    }
}
