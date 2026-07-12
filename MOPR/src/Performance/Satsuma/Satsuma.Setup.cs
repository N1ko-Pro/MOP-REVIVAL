// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Разовая настройка Сатсумы: множество игровых фиксов и привязок, которые оригинал делал прямо в
// конструкторе. Здесь они разложены по именованным методам (каждый в своём try/catch, чтобы сбой
// одного не срывал остальные). Сами компоненты-фиксы живут в Fixes/Satsuma.

using HutongGames.PlayMaker;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using MOPR.FSM;
using MOPR.FSM.Actions;
using MOPR.Common;
using MOPR.Common.Enumerations;
using MOPR.Items.Helpers;
using MOPR.Vehicles.Managers.SatsumaManagers;
using MOPR.Rules;

namespace MOPR.Vehicles.Cases
{
    internal partial class Satsuma
    {
        private void SetupSeats()
        {
            try
            {
                GameObject.Find("seat driver(Clone)").AddComponent<SatsumaSeatsManager>();
                GameObject.Find("seat passenger(Clone)").AddComponent<SatsumaSeatsManager>();
            }
            catch (System.Exception ex)
            {
                ExceptionManager.New(ex, true, "SATSUMA_SEAT_FIX_ERROR");
            }

            try
            {
                GameObject bucketDriver = GameObject.Find("bucket seat driver(Clone)");
                GameObject bucketPassanger = GameObject.Find("bucket seat passenger(Clone)");
                if (bucketDriver == null)
                {
                    bucketDriver = Resources.FindObjectsOfTypeAll<GameObject>().First(g => g.name == "bucket seat driver(Clone)"
                        && g.transform.parent.gameObject.name == "Parts");
                    bucketPassanger = Resources.FindObjectsOfTypeAll<GameObject>().First(g => g.name == "bucket seat passenger(Clone)"
                        && g.transform.parent.gameObject.name == "Parts");
                }

                bucketDriver?.AddComponent<SatsumaSeatsManager>();
                bucketPassanger?.AddComponent<SatsumaSeatsManager>();
            }
            catch (System.Exception ex)
            {
                ExceptionManager.New(ex, true, "SATSUMA_BUCKET_SEAT_FIX_ERROR");
            }
        }

        private void SetupMechanicalWearAndHandbrake()
        {
            // Механический износ.
            try
            {
                transform.Find("CarSimulation/MechanicalWear").gameObject.GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;
            }
            catch (System.Exception ex)
            {
                ExceptionManager.New(ex, true, "SATSUMA_MECHANICAL_WEAR_FIX_ERROR");
            }

            // Ручник не работал после респавна.
            try
            {
                GameObject.Find("HandBrake").GetComponent<PlayMakerFSM>().enabled = true;
            }
            catch (System.Exception ex)
            {
                ExceptionManager.New(ex, true, "SATSUMA_HANDBRAKE_ERROR");
            }

            // Позиция рычага ручника.
            try
            {
                transform.Find("MiscParts/HandBrake/handbrake(xxxxx)/handbrake lever").GetPlayMaker("Use").Fsm.RestartOnEnable = false;
            }
            catch
            {
                ExceptionManager.New(new System.Exception("handbrake lever fix"), false, "Unable to fix handbrake lever.");
            }
        }

        private void SetupBatteryTerminal()
        {
            // Минусовая клемма иногда отключалась навсегда — своя реализация отключения проводов.
            try
            {
                transform.Find("Wiring").GetPlayMaker("Status").GetState("Disable battery wires").AddAction(new CustomBatteryDisable());
            }
            catch
            {
                ExceptionManager.New(new System.Exception("battery terminal fix"), false, "Unable to fix battery terminal wire.");
            }
        }

        private void SetupBodyPaint()
        {
            try
            {
                transform.Find("Body/car body(xxxxx)").GetPlayMaker("Paint").Fsm.RestartOnEnable = false;
            }
            catch (System.Exception ex)
            {
                ExceptionManager.New(ex, true, "SATSUMA_BODY_PAINT_FIX_ERROR");
            }
        }

        /// <summary>Гасит State 1/Load у ремней/фильтров/свечей/батарей, чтобы не сбрасывали состояние.</summary>
        private void WipeLoadStates()
        {
            GameObject[] parts = Resources.FindObjectsOfTypeAll<GameObject>()
                .Where(obj => obj.name.ContainsAny("alternator belt(Clone)", "oil filter(Clone)", "spark plug(Clone)", "battery(Clone)"))
                .ToArray();

            foreach (GameObject part in parts)
            {
                try
                {
                    if (part.transform.root.gameObject.name == "GAZ24(1420kg)")
                        continue;

                    PlayMakerFSM useFsm = part.GetPlayMaker("Use");
                    FsmState state1 = useFsm.GetState("State 1");
                    List<FsmStateAction> emptyState1 = state1.Actions.ToList();
                    emptyState1.Insert(0, new CustomStop());
                    state1.Actions = emptyState1.ToArray();
                    state1.SaveActions();

                    useFsm.GetState("Load").Fsm.RestartOnEnable = false;
                }
                catch (System.Exception ex)
                {
                    ExceptionManager.New(ex, true, "SATSUMA_STATE1_WIPE_ERROR");
                }
            }
        }

        private void SetupRadioAndCd()
        {
            // CD-плеер глушил радио других машин.
            try
            {
                Resources.FindObjectsOfTypeAll<GameObject>()
                    .First(g => g.name == "cd player(Clone)" && g.GetComponent<PlayMakerFSM>() != null && g.GetComponent<MeshRenderer>() != null)
                    .transform.Find("ButtonsCD/RadioVolume").GetPlayMaker("Knob").Fsm.RestartOnEnable = false;
            }
            catch
            {
                ExceptionManager.New(new System.Exception("FSM RadioCD Fix"), false, "Unable to fix cd player(Clone).");
            }

            // Радио.
            try
            {
                Resources.FindObjectsOfTypeAll<GameObject>().First(g => g.name == "radio(Clone)" && g.transform.root.gameObject.name != "JAIL")
                    .transform.Find("ButtonsRadio/RadioVolume").GetPlayMaker("Knob").Fsm.RestartOnEnable = false;
            }
            catch
            {
                ExceptionManager.New(new System.Exception("FSM Radio Fix"), false, "Unable to fix radio(Clone).");
            }
        }

        private void SetupWindowGrille()
        {
            try
            {
                Resources.FindObjectsOfTypeAll<GameObject>().First(g => g.name == "window grille(Clone)" && g.GetPlayMaker("Paint") != null)
                    .GetPlayMaker("Paint").Fsm.RestartOnEnable = false;
            }
            catch
            {
                ExceptionManager.New(new System.Exception("window grille fix"), false, "Unable to fix window grille(Clone).");
            }
        }

        private void SetupHoodFix()
        {
            try
            {
                GameFixes.Instance.HoodFix(transform.Find("Body/pivot_hood"),
                                           transform.Find("MiscParts/trigger_battery"),
                                           transform.Find("MiscParts/pivot_battery"));
            }
            catch (System.Exception ex)
            {
                ExceptionManager.New(ex, true, "SATSUMA_HOOD_FIX_ERROR");
            }
        }

        private void SetupRearBumper()
        {
            try
            {
                GameObject rearBumper = GameObject.Find("bumper rear(Clone)");
                GameObject databaseBumper = GameObject.Find("Database/DatabaseBody/Bumper_Rear");
                databaseBumper.SetActive(false);
                databaseBumper.SetActive(true);

                RearBumperBehaviour behaviour = rearBumper.AddComponent<RearBumperBehaviour>();
                rearBumper.GetPlayMaker("Removal").GetState("Remove part").AddAction(new CustomSatsumaBumperDetach(behaviour));
                transform.Find("Body/trigger_bumper_rear").GetPlayMaker("Assembly").GetState("Assemble 2").AddAction(new CustomSatsumaBumperAttach(behaviour));
            }
            catch (System.Exception ex)
            {
                ExceptionManager.New(ex, true, "SATSUMA_REAR_BUMPER_DETACH_FIX_ERROR");
            }
        }

        /// <summary>Компенсирует «набор массы» подвески и ряда деталей при каждом респавне машины.</summary>
        private void SetupPartMassManagers()
        {
            IEnumerable<GameObject> suspensionParts = Resources.FindObjectsOfTypeAll<GameObject>()
                .Where(g => g.name.ContainsAny("strut", "coil spring", "shock absorber")
                            && g.transform.root != null
                            && g.transform.root == gameObject.transform);

            foreach (GameObject part in suspensionParts)
            {
                try
                {
                    part.AddComponent<SatsumaPartMassManager>();
                }
                catch
                {
                    ExceptionManager.New(new System.Exception("SatsumaPartMassManager: Suspension"), false, "SatsumaPartsMassManager: Adding to suspension parts");
                }
            }

            GameObject[] others = Resources.FindObjectsOfTypeAll<GameObject>()
                .Where(g => g.name.EqualsAny("grille gt(Clone)", "grille(Clone)", "subwoofer panel(Clone)",
                    "seat rear(Clone)", "amplifier(Clone)", "rearlight(leftx)", "rearlight(right)", "racing radiator(xxxxx)", "radiator(xxxxx)",
                    "radiator hose1(xxxxx)", "radiator hose3(xxxxx)", "marker light left(xxxxx)", "marker light right(xxxxx)", "antenna(leftx)",
                    "antenna(right)", "dashboard(Clone)")).ToArray();

            foreach (GameObject other in others)
            {
                try
                {
                    other.AddComponent<SatsumaPartMassManager>();
                }
                catch
                {
                    ExceptionManager.New(new System.Exception("SatsumaPartMassManager: Others"), false, "SatsumaPartsMassManager: Others adding.");
                }
            }
        }

        private void SetupKey()
        {
            try
            {
                key = Resources.FindObjectsOfTypeAll<GameObject>()
                    .First(g => g.name == "steering_column2" && g.transform.root == transform)
                    .transform.Find("Ignition/Keys/Key").gameObject;
            }
            catch
            {
                ExceptionManager.New(new System.Exception("SatsumKey: Cannot Locate Key"), false, "SatsumaPartsMassManager: Key Error");
            }
        }

        private void SetupCooldownTick()
        {
            try
            {
                cooldownTick = GameObject.Find("block(Clone)").transform.Find("CooldownTick").gameObject;
            }
            catch (System.Exception ex)
            {
                ExceptionManager.New(ex, true, "SATSUMA_COOLDOWN_TICK_ERROR");
            }
        }

        private void SetupRegPlatesAndDashboard()
        {
            try
            {
                GameObject.Find("bootlid(Clone)").transform.Find("RegPlateRear").gameObject.GetComponent<Renderer>().material.renderQueue = 100;
                GameObject.Find("bumper front(Clone)").transform.Find("RegPlateFront").gameObject.GetComponent<Renderer>().material.renderQueue = 100;

                // Z-fighting стрелок приборки Сатсумы.
                if (!RulesManager.Instance.SpecialRules.SatsumaIgnoreRenderers)
                {
                    dashboardMaterials = new List<Material>
                    {
                        GameObject.Find("dashboard meters(Clone)").transform.Find("Gauges/Fuel/needle_small").gameObject.GetComponent<Renderer>().material,
                        GameObject.Find("dashboard meters(Clone)").transform.Find("Gauges/Temp/needle_small").gameObject.GetComponent<Renderer>().material,
                        GameObject.Find("dashboard meters(Clone)").transform.Find("Gauges/Speedometer/needle_large").gameObject.GetComponent<Renderer>().material,
                        GameObject.Find("rpm gauge(Clone)").transform.Find("Pivot/needle").gameObject.GetComponent<Renderer>().material,
                        GameObject.Find("clock gauge(Clone)").transform.Find("ClockCar/hour/needle_hour").gameObject.GetComponent<Renderer>().material,
                        GameObject.Find("clock gauge(Clone)").transform.Find("ClockCar/minute/needle_minute").gameObject.GetComponent<Renderer>().material
                    };

                    foreach (Material mat in dashboardMaterials)
                        mat.renderQueue = 100;

                    lightSelection = Resources.FindObjectsOfTypeAll<GameObject>()
                        .First(g => g.name == "dashboard meters(Clone)").transform.Find("Knobs/ButtonsDash/LightModes")
                        .gameObject.GetComponent<PlayMakerFSM>().FsmVariables.GetFsmInt("Selection");
                }
            }
            catch
            {
            }
        }

        private void SetupDoors()
        {
            try
            {
                // Заедание петель дверей.
                GameObject.Find("door left(Clone)").AddComponent<SatsumaHingeManager>();
                GameObject.Find("door right(Clone)").AddComponent<SatsumaHingeManager>();
                GameObject.Find("bootlid(Clone)").AddComponent<SatsumaHingeManager>();

                // Сброс цвета покраски дверей.
                GameObject.Find("door left(Clone)").GetPlayMaker("Paint").Fsm.RestartOnEnable = false;
                GameObject.Find("door right(Clone)").GetPlayMaker("Paint").Fsm.RestartOnEnable = false;
            }
            catch (System.Exception ex)
            {
                ExceptionManager.New(ex, true, "SATSUMA_DOOR_FIX_ERROR");
            }
        }

        private void SetupOnActionObjects()
        {
            try
            {
                satsumaOnActionObjects = new List<SatsumaOnActionObjects>
                {
                    new SatsumaOnActionObjects(transform.Find("CarSimulation/MechanicalWear").gameObject, SatsumaEnableOn.OnEngine),
                    new SatsumaOnActionObjects(transform.Find("CarSimulation/Fixes").gameObject, SatsumaEnableOn.OnEngine),
                    new SatsumaOnActionObjects(transform.Find("CarSimulation/DynoDistance").gameObject, SatsumaEnableOn.OnEngine),
                    new SatsumaOnActionObjects(transform.Find("CarSimulation/RandomBolt").gameObject, SatsumaEnableOn.OnEngine),
                    new SatsumaOnActionObjects(transform.Find("RainScript").gameObject, SatsumaEnableOn.OnPlayerClose),
                    new SatsumaOnActionObjects(transform.Find("DriverHeadPivot").gameObject, SatsumaEnableOn.OnPlayerClose),
                    new SatsumaOnActionObjects(transform.Find("AirIntake").gameObject, SatsumaEnableOn.OnPlayerClose),
                    new SatsumaOnActionObjects(gameObject.GetPlayMaker("ButtonShifter"), SatsumaEnableOn.OnPlayerClose),
                    new SatsumaOnActionObjects(transform.Find("Chassis").gameObject, SatsumaEnableOn.OnPlayerFar)
                };
            }
            catch (System.Exception ex)
            {
                ExceptionManager.New(ex, true, "SATSUMA_ON_ACTION_OBJECTS_ERROR");
            }
        }

        private void SetupAssembleAudio()
        {
            // Звук сборки блока играем своим скриптом (только если игрок рядом).
            try
            {
                PlayMakerFSM blockBoltCheck = GameObject.Find("block(Clone)").GetPlayMaker("BoltCheck");
                FsmState boltsONState = blockBoltCheck.GetState("Bolts ON");
                FsmStateAction[] boltsONActions = boltsONState.Actions;
                boltsONActions[1] = new MasterAudioAssembleCustom();
                boltsONState.Actions = boltsONActions;
                boltsONState.SaveActions();
            }
            catch (System.Exception ex)
            {
                ExceptionManager.New(ex, true, "SATSUMA_BOLTS_AUDIO_ERROR");
            }
        }

        private void SetupColliderSetup()
        {
            // Отключаем "Fix Collider", чтобы предметы не проваливались сквозь машину.
            try
            {
                gameObject.GetPlayMaker("Setup").Fsm.RestartOnEnable = false;

                MeshCollider bootFloor = transform.Find("Colliders/collider_floor3").gameObject.GetComponent<MeshCollider>();
                bootFloor.isTrigger = false;
                bootFloor.enabled = true;
            }
            catch (System.Exception ex)
            {
                ExceptionManager.New(ex, true, "SATSUMA_COLLIDER_SETUP_ERROR");
            }
        }

        private void SetupDriverHead()
        {
            // Водитель слишком легко «погибал» от мелких ударов.
            try
            {
                transform.Find("DriverHeadPivot").GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            }
            catch (System.Exception ex)
            {
                ExceptionManager.New(ex, true, "SATSUMA_DRIVER_HEAD_PIVOT_ERROR");
            }
        }

        private void SetupMaskedElements()
        {
            try
            {
                maskedElements = new Dictionary<GameObject, bool>();
                foreach (string obj in maskedObjectNames)
                {
                    GameObject gm = Resources.FindObjectsOfTypeAll<GameObject>().First(g => g.name == obj);
                    maskedElements.Add(gm, key.activeSelf);
                }
            }
            catch (System.Exception ex)
            {
                ExceptionManager.New(ex, true, "SATSUMA_MASKED_ELEMENTS_ERROR");
            }
        }

        private void SetupDrivingAI()
        {
            try
            {
                drivingAI = transform.Find("AI")?.GetPlayMaker("Driving");
            }
            catch (System.Exception ex)
            {
                ExceptionManager.New(ex, false, "SATSUMA_AI_ERROR");
            }
        }

        private void SetupRadiatorHose()
        {
            try
            {
                GameObject radiatorHosePart = Resources.FindObjectsOfTypeAll<GameObject>().First(g => g.name == "radiator hose3(xxxxx)");
                if (radiatorHosePart)
                    radiatorHosePart.AddComponent<SatsumaRadiatorHoseFix>();
            }
            catch
            {
                throw new System.Exception("Radiator hose 3 error");
            }
        }

        private void SetupWindscreen()
        {
            try
            {
                Transform windscreen = transform.Find("Body/Windshield");
                if (!windscreen)
                    throw new System.Exception("Couldn't find Windscreen.");

                SatsumaWindscreenFixer swf = windscreen.gameObject.AddComponent<SatsumaWindscreenFixer>();

                Transform windshieldJob = GameObject.Find("REPAIRSHOP").transform.Find("Jobs/Windshield");
                if (!windshieldJob)
                    throw new System.Exception("Couldn't find windshield job.");

                if (!windshieldJob.parent.gameObject.activeSelf)
                {
                    windshieldJob.parent.gameObject.SetActive(true);
                    windshieldJob.parent.gameObject.SetActive(false);
                }

                PlayMakerFSM windshieldJobFSM = windshieldJob.gameObject.GetComponent<PlayMakerFSM>();
                List<FsmStateAction> wait1Actions = windshieldJobFSM.GetState("Wait1").Actions.ToList();
                wait1Actions.Insert(0, new WindscreenRepairJob(swf));
                windshieldJobFSM.GetState("Wait1").Actions = wait1Actions.ToArray();
            }
            catch (System.Exception ex)
            {
                ExceptionManager.New(ex, true, "WINDSCREEN REPAIR FIX ERROR");
            }
        }

        private void SetupFireExtinguisherHolder()
        {
            try
            {
                GameObject extinguisherHolder = transform.Find("Interior/fire extinguisher holder(xxxxx)").gameObject;
                foreach (PlayMakerFSM fsm in extinguisherHolder.GetComponents<PlayMakerFSM>())
                    fsm.Fsm.RestartOnEnable = false;
            }
            catch
            {
                throw new System.Exception("Fire extinguisher holder error");
            }
        }

        private void SetupOdometer()
        {
            try
            {
                GameObject odometer = GameObject.Find("dashboard meters(Clone)/Gauges/Odometer");
                if (odometer.GetComponent<PlayMakerFSM>() == null)
                    ModConsole.Log("[MOPR] Can't reset Odometer FSM. Perhaps a mod has removed it?");
                else
                    odometer.GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;
            }
            catch (System.Exception ex)
            {
                ExceptionManager.New(ex, false, "SATS_ODOMETER_RESET_FIX_ERROR");
            }
        }

        private void SetupSubframeBolts()
        {
            try
            {
                GameObject subframeBolts = transform.Find("Chassis/sub frame(xxxxx)/Bolts")?.gameObject;
                GameObject triggerSubframe = transform.Find("Chassis/trigger_subframe")?.gameObject;
                if (triggerSubframe == null || subframeBolts == null)
                    throw new MissingReferenceException("Could not find trigger_subframe or subframe.");

                bool isSubframeTriggerActive = triggerSubframe.activeSelf;
                triggerSubframe.SetActive(true);

                PlayMakerFSM triggerSubframeFSM = triggerSubframe.GetPlayMaker("Assembly");
                if (triggerSubframeFSM == null)
                    throw new MissingReferenceException("Could not find Assembly FSM.");

                triggerSubframeFSM.GetState("Assemble").AddAction(new CustomToggleAllBoltsAction(subframeBolts, true));
                triggerSubframe.SetActive(isSubframeTriggerActive);
            }
            catch (System.Exception ex)
            {
                ExceptionManager.New(ex, false, "SUBFRAME_BOLTS_ERROR");
            }
        }

        private void SetupExhaustMuffler()
        {
            try
            {
                transform.Find("MiscParts/pivot_exhaust_muffler").gameObject.AddComponent<SatsumaExhaustMufflerFix>();
            }
            catch (System.Exception ex)
            {
                ExceptionManager.New(ex, false, "EXHAUST_MUFFLER_FIX_ERROR");
            }
        }

        private void SetupBlockHinge()
        {
            try
            {
                GameObject.Find("block(Clone)").AddComponent<SatsumaHingeManager>();
            }
            catch (System.Exception ex)
            {
                ExceptionManager.New(ex, false, "SATSUMA_BLOCK_FIND_FAILURE");
            }
        }

        private void SetupDriverHeadHinge()
        {
            try
            {
                transform.Find("DriverHeadPivot").gameObject.AddComponent<SatsumaHingeManager>();
            }
            catch (System.Exception ex)
            {
                ExceptionManager.New(ex, false, "SATSUMA_DRIVER_HEAD_PIVOT_FAILURE");
            }
        }

        /// <summary>Глубокая оптимизация Сатсумы за рулём (гасит триггеры/пивоты/косметику при езде).</summary>
        private void SetupDrivingOptimizer()
        {
            try
            {
                gameObject.AddComponent<SatsumaDrivingOptimizer>();
            }
            catch (System.Exception ex)
            {
                ExceptionManager.New(ex, false, "SATSUMA_DRIVING_OPTIMIZER_ERROR");
            }
        }

        /// <summary>Якорь ручника: не даёт машине сползать по уклону с поднятым ручником (домкрат не мешает).</summary>
        private void SetupParkingBrakeAnchor()
        {
            try
            {
                gameObject.AddComponent<SatsumaParkingBrakeAnchor>();
            }
            catch (System.Exception ex)
            {
                ExceptionManager.New(ex, false, "SATSUMA_PARKING_BRAKE_ANCHOR_ERROR");
            }
        }

        /// <summary>Навешивает SatsumaBoltsAntiReload на болты Сатсумы, чтобы их затяжка не сбрасывалась.</summary>
        private void BoltsFix()
        {
            satsumaBoltsAntiReloads = new List<SatsumaBoltsAntiReload>();

            GameObject[] bolts = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == "BoltPM").ToArray();
            foreach (GameObject bolt in bolts)
            {
                Transform parent = bolt.transform.parent;

                // У этих узлов родитель BoltCheck — дедушка (water_pump и Hooks — исключения).
                if (parent.name.EqualsAny("Bolts", "_Motor", "water_pump_pulley_mesh", "Hooks", "hooks"))
                {
                    if (parent.parent.gameObject.name.EqualsAny("water_pump_pulley_mesh", "Hooks", "hooks"))
                        parent = parent.parent.parent;
                    else
                        parent = parent.parent;
                }
                else if (parent.name.ContainsAny("Masked", "ValveAdjust"))
                {
                    parent = parent.parent.parent;
                }
                else if (parent.name.StartsWith("IK_wishbone_"))
                {
                    string suffix = parent.gameObject.name.Replace("IK_wishbone_", "");
                    parent = parent.Find("wishbone " + suffix + "(xxxxx)");
                }

                // Эти пропускаем.
                if (parent.name.ContainsAny("bolts_shock", "shock_bottom", "halfshaft_", "pivot_steering_arm_", "OFFSET", "pivot_shock_"))
                    continue;

                if (parent.name == "Pivot" && parent.parent.gameObject.name.StartsWith("alternator"))
                    continue;

                if (parent.gameObject.name == "hood(Clone)")
                    continue;

                if (parent.gameObject.GetComponent<SatsumaBoltsAntiReload>() == null)
                    parent.gameObject.AddComponent<SatsumaBoltsAntiReload>();
            }

            // Полуоси.
            GameObject[] halfshafts = Resources.FindObjectsOfTypeAll<GameObject>().Where(g => g.name == "halfshaft(xxxxx)").ToArray();
            foreach (GameObject halfshaft in halfshafts)
                if (halfshaft.GetComponent<SatsumaBoltsAntiReload>() == null)
                    halfshaft.AddComponent<SatsumaBoltsAntiReload>();

            // Блок двигателя.
            GameObject.Find("block(Clone)").AddComponent<SatsumaBoltsAntiReload>();

            // Рулевые тяги.
            transform.Find("Chassis/steering rod fr(xxxxx)").gameObject.AddComponent<SatsumaBoltsAntiReload>();
            transform.Find("Chassis/steering rod fl(xxxxx)").gameObject.AddComponent<SatsumaBoltsAntiReload>();

            // Топливная магистраль.
            transform.Find("MiscParts/fuel line(xxxxx)").gameObject.AddComponent<SatsumaBoltsAntiReload>();

            ModConsole.Log("[MOPR] Found " + satsumaBoltsAntiReloads.Count + " bolts.");
            if (satsumaBoltsAntiReloads.Count < MinimumBolts)
                ModConsole.Log("<color=yellow>[MOPR] Only " + satsumaBoltsAntiReloads.Count + " out of expected " + MinimumBolts + " have been reset!</color>");
        }
    }
}
