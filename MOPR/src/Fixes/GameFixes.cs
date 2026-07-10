// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Общие игровые фиксы, применяемые один раз при загрузке: развязка объектов PERAJARVI/Buildings
// для раздельной выгрузки, анти-сброс множества FSM (RestartOnEnable=false), фиксы предметов коттеджа,
// такси-скрипты, анти-клип, z-fighting часов, GrandmaHiker и т.д. Плюс отложенный фикс капота/
// бампера/батареи Сатсумы (HoodFix) и принудительная отцепка прицепа. Каждый фикс изолирован
// try/catch — сбой одного не срывает остальные.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HutongGames.PlayMaker;

using MOPR.Managers;
using MOPR.Common;
using MOPR.Common.Enumerations;
using MOPR.FSM;
using MOPR.FSM.Actions;
using MOPR.Vehicles.Cases;
using MOPR.Vehicles.Managers;
using MOPR.Vehicles.Managers.SatsumaManagers;
using MOPR.Helpers;

namespace MOPR
{
    internal class GameFixes : MonoBehaviour
    {
        private static GameFixes instance;
        public static GameFixes Instance => instance;

        public bool HoodFixDone { get; private set; }

        public GameFixes()
        {
            instance = this;
        }

        public void MainFixes()
        {
            // Сброс GT-решётки при активации.
            try
            {
                foreach (PlayMakerFSM fsm in GameObject.Find("grille gt(Clone)").GetComponents<PlayMakerFSM>())
                    fsm.Fsm.RestartOnEnable = false;
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "GT_GRILLE_ERROR");
            }

            // Отвязываем дом Тэймо от PERAJARVI, чтобы грузить/выгружать раздельно.
            Transform buildings = null;
            Transform perajarvi = null;
            try
            {
                buildings = GameObject.Find("Buildings").transform;
                perajarvi = GameObject.Find("PERAJARVI").transform;
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, true, "PERAJARVI_FIXES_BUILDINGS_FIND_ERROR");
            }

            if (buildings != null && perajarvi != null)
            {
                SetParent(perajarvi, buildings, "HouseRintama3");
                SetParent(perajarvi, buildings, "HouseSmall3");
                SetParent(null, "CHURCHWALL");
                SetParent(null, "DINGONBIISI");

                // Объекты PERAJARVI с одинаковыми именами переносим под Buildings.
                Transform[] perajarviChilds = perajarvi.GetComponentsInChildren<Transform>();
                for (int i = 0; i < perajarviChilds.Length; i++)
                {
                    if (perajarviChilds[i] == null)
                        continue;

                    string objName = perajarviChilds[i].gameObject.name;
                    if (objName.Contains("silo") || objName == "MailBox" || objName == "Greenhouse")
                        SetParent(buildings, perajarviChilds[i]);
                }

                // Дискеты у нового дома Йокке.
                while (perajarvi.transform.Find("TerraceHouse/diskette(itemx)") != null)
                {
                    Transform diskette = perajarvi.transform.Find("TerraceHouse/diskette(itemx)");
                    if (diskette != null && diskette.parent != null)
                        SetParent(null, diskette);
                }

                // Мебель дома Йокке проваливается сквозь пол.
                SetParent(perajarvi, null, "TerraceHouse/Colliders");
            }

            // Предметы коттеджа исчезают при перемещении — отвязываем от родителя.
            try
            {
                GameObject.Find("coffee pan(itemx)").transform.parent = null;
                GameObject.Find("lantern(itemx)").transform.parent = null;
                GameObject.Find("coffee cup(itemx)").transform.parent = null;
                GameObject.Find("camera(itemx)").transform.parent = null;
                GameObject.Find("fireworks bag(itemx)").transform.parent = null;
                GameObject.Find("FishAreaAVERAGE").transform.parent = null;
                GameObject.Find("FishAreaBAD").transform.parent = null;
                GameObject.Find("FishAreaGOOD").transform.parent = null;
                GameObject.Find("FishAreaGOOD2").transform.parent = null;
                GameObject.Find("COTTAGE").transform.Find("MESH/Cottage_chimney").parent = null;
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "ITEMS_FIXES_ERROR");
            }

            // Такси (FITTAN/KUSKI): при посадке игрока отвязываем машину от родителя.
            try
            {
                GameObject fittan = GameObject.Find("TRAFFIC").transform.Find("VehiclesDirtRoad/Rally/FITTAN").gameObject;
                GameObject kuski = GameObject.Find("NPC_CARS").transform.Find("KUSKI").gameObject;
                fittan.transform.Find("PlayerTrigger/DriveTrigger").gameObject.FsmInject("Player in car", () => fittan.transform.parent = null);
                kuski.transform.Find("PlayerTrigger/DriveTrigger").gameObject.FsmInject("Player in car", () => kuski.transform.parent = null);
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, true, "TAXI_MANAGERS_ERROR");
            }

            Transform cabin = GameObject.Find("CABIN").transform;

            // Ставка в «Вентти» сбрасывалась при загрузке кабины.
            try
            {
                cabin.Find("Cabin/Ventti/Table/GameManager").GetPlayMaker("Use").Fsm.RestartOnEnable = false;
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "VENTII_FIX_ERROR");
            }

            // Сброс двери кабины.
            try
            {
                cabin.Find("Cabin/Door/Pivot/Handle").gameObject.GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "CABIN_DOOR_RESET_FIX_ERROR");
            }

            // Ржавые машины: гасим Load-сброс и берём под управление по дистанции.
            int junkCarCounter = 1;
            try
            {
                for (; GameObject.Find("JunkCar" + junkCarCounter) != null; junkCarCounter++)
                {
                    GameObject junk = GameObject.Find("JunkCar" + junkCarCounter);
                    junk.GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;
                    WorldObjectManager.Instance.Add(junk, DisableOn.Distance);
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "JUNK_CARS_" + junkCarCounter + "_ERROR");
            }

            // Пешеходы (кроме Farmer/Fighter2/FighterPub) — под управление по дистанции.
            try
            {
                GameObject humans = GameObject.Find("HUMANS");
                foreach (Transform t in humans.GetComponentsInChildren<Transform>().Where(t => t.parent == humans.transform))
                {
                    if (t.gameObject.name.EqualsAny("HUMANS", "Fighter2", "Farmer", "FighterPub"))
                        continue;

                    GenericObjectHuman(t);
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "HUMANS_ERROR");
            }

            // Осиные гнёзда сбрасывались на значения загрузки.
            try
            {
                foreach (GameObject wasphive in Resources.FindObjectsOfTypeAll<GameObject>().Where(g => g.name == "WaspHive"))
                    wasphive.GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "WASPHIVES_ERROR");
            }

            // Отключаем скрипт, сбрасывающий kinematic Сатсумы в false при отпускании детали.
            try
            {
                GameObject hand = GameObject.Find("PLAYER/Pivot/AnimPivot/Camera/FPSCamera/1Hand_Assemble/Hand");
                PlayMakerFSM pickUp = hand.GetPlayMaker("PickUp");

                pickUp.GetState("Drop part").RemoveAction(0);
                pickUp.GetState("Drop part 2").RemoveAction(0);
                pickUp.GetState("Tool picked").RemoveAction(2);
                pickUp.GetState("Drop tool").RemoveAction(0);
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, true, "SATSUMA_HAND_BS_FIX");
            }

            // Матрас в особняке не должен выгружаться.
            try
            {
                Transform mattres = GameObject.Find("DINGONBIISI").transform.Find("mattres");
                if (mattres != null)
                    mattres.parent = null;
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "MANSION_MATTRES_ERROR");
            }

            // Анти-клип предметов у коттеджа.
            try
            {
                GameObject area = new GameObject("MOPR_ItemAntiClip");
                area.transform.position = new Vector3(-848.3f, -5.4f, 505.5f);
                area.transform.eulerAngles = new Vector3(0, 343.0013f, 0);
                area.AddComponent<ItemAntiClip>();
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, true, "ITEM_ANTICLIP_ERROR");
            }

            // Z-fighting стрелок наручных часов.
            try
            {
                GameObject player = GameObject.Find("PLAYER");
                if (player)
                {
                    Transform hour = player.transform.Find("Pivot/AnimPivot/Camera/FPSCamera/FPSCamera/Watch/Animate/BreathAnim/WristwatchHand/Clock/Pivot/Hour/hour");
                    if (hour)
                        hour.GetComponent<Renderer>().material.renderQueue = 3001;

                    Transform minute = player.transform.Find("Pivot/AnimPivot/Camera/FPSCamera/FPSCamera/Watch/Animate/BreathAnim/WristwatchHand/Clock/Pivot/Minute/minute");
                    if (minute)
                        minute.GetComponent<Renderer>().material.renderQueue = 3002;
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "HANDWATCH_RENDERER_QUEUE_ERROR");
            }

            // Фикс кувырка автобуса.
            try
            {
                GameObject bus = GameObject.Find("BUS");
                if (bus)
                    bus.AddComponent<BusRollFix>();
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "BUS_ROLL_FIX_ERROR");
            }

            // Обёртка окна спальни сбрасывалась к значению по умолчанию.
            try
            {
                Transform triggerWindowWrap = GameObject.Find("YARD").transform.Find("Building/BEDROOM1/trigger_window_wrap");
                if (triggerWindowWrap != null)
                    triggerWindowWrap.GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "TRIGGER_WINDOW_WRAP_ERROR");
            }

            // Извлечение дискеты не работало.
            try
            {
                GameObject triggerDiskette = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(g => g.name == "TriggerDiskette");
                if (triggerDiskette != null)
                    triggerDiskette.GetPlayMaker("Assembly").Fsm.RestartOnEnable = false;
                else
                    ModConsole.Log("[MOPR] Trigger diskette was null");
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "TRIGGER_DISKETTE_ERROR");
            }

            // Память компьютера сбрасывалась.
            try
            {
                GameObject triggerPlayMode = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(g => g.name == "TriggerPlayMode");
                if (triggerPlayMode != null)
                    triggerPlayMode.GetPlayMaker("PlayerTrigger").Fsm.RestartOnEnable = false;
                else
                    ModConsole.Log("[MOPR] Trigger play mode was null");
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "TRIGGER_PLAY_MODE_ERROR");
            }

            // Навык сбора ягод сбрасывался.
            try
            {
                GameObject.Find("JOBS").transform.Find("StrawberryField").gameObject.GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "STRAWBERRY_FIELD_FSM");
            }

            // Бабка-автостопщица: включаем/выключаем её аниматор при посадке/высадке.
            try
            {
                GameObject grandmaHiker = GameObject.Find("GrannyHiker");
                if (grandmaHiker)
                {
                    GameObject skeleton = grandmaHiker.transform.Find("Char/skeleton").gameObject;
                    PlayMakerFSM logicFSM = grandmaHiker.GetPlayMaker("Logic");

                    logicFSM.GetState("Open door").AddAction(new GrandmaHiker(skeleton, false));
                    logicFSM.GetState("Set mass 2").AddAction(new GrandmaHiker(skeleton, true));
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "GRANDMA_HIKER_FIXES");
            }

            // Стройплощадка.
            try
            {
                Transform construction = GameObject.Find("PERAJARVI").transform.Find("ConstructionSite");
                if (construction)
                    construction.parent = null;
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "PERAJARVI_CONSTRUCTION_ERROR");
            }

            // Фикс триггеров деталей Сатсумы.
            try
            {
                gameObject.AddComponent<SatsumaTriggerFixer>();
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "SATSUMA_TRIGGER_FIXER_ERROR");
            }

            // Почтовые ящики.
            try
            {
                foreach (GameObject g in Resources.FindObjectsOfTypeAll<GameObject>().Where(g => g.name == "MailBox"))
                {
                    Transform hatch = g.transform.Find("BoxHatch");
                    if (hatch)
                        hatch.GetComponent<PlayMakerFSM>().Fsm.RestartOnEnable = false;
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "MAILBOX_ERROR");
            }

            ModConsole.Log("[MOPR] Finished applying fixes");
        }

        private static void GenericObjectHuman(Transform t)
        {
            WorldObjects.GenericObject human = WorldObjectManager.Instance.Add(t.gameObject, DisableOn.Distance);
            human.MinimumToggleDistance = 150;
        }

        /// <summary>Фикс: капот «выскакивает» из машины при загрузке.</summary>
        public void HoodFix(Transform hoodPivot, Transform batteryPivot, Transform batteryTrigger)
        {
            StartCoroutine(HoodFixCoroutine(hoodPivot, batteryPivot, batteryTrigger));
        }

        private IEnumerator HoodFixCoroutine(Transform hoodPivot, Transform batteryPivot, Transform batteryTrigger)
        {
            yield return new WaitForSeconds(2);

            Transform hood = Resources.FindObjectsOfTypeAll<GameObject>().First(g => g.name == "hood(Clone)").transform;
            CustomPlayMakerFixedUpdate hoodFixedUpdate = hood.gameObject.AddComponent<CustomPlayMakerFixedUpdate>();

            GameObject fiberHood = Resources.FindObjectsOfTypeAll<GameObject>()
                .First(obj => obj.name == "fiberglass hood(Clone)"
                              && obj.GetComponent<PlayMakerFSM>() != null
                              && obj.GetComponent<MeshCollider>() != null);

            int retries = 0;
            if (FsmManager.IsStockHoodBolted() && hood.parent != hoodPivot)
            {
                hood.gameObject.SetActive(true);

                while (hood.parent != hoodPivot)
                {
                    // Сатсума выгрузилась во время фикса — попробуем позже.
                    if (!hoodPivot.gameObject.activeSelf)
                        yield break;

                    FsmManager.ForceHoodAssemble();
                    yield return null;

                    if (++retries == 10)
                        break;
                }
            }

            hoodFixedUpdate.StartFixedUpdate();

            if (fiberHood != null && FsmManager.IsFiberHoodBolted() && fiberHood.transform.parent != hoodPivot)
            {
                retries = 0;
                while (fiberHood.transform.parent != hoodPivot)
                {
                    if (!hoodPivot.gameObject.activeSelf)
                    {
                        Satsuma.Instance.AfterFirstEnable = false;
                        yield break;
                    }

                    FsmManager.ForceHoodAssemble();
                    yield return null;

                    if (++retries == 60)
                        break;
                }
            }

            hood.gameObject.AddComponent<SatsumaBoltsAntiReload>();
            fiberHood.gameObject.AddComponent<SatsumaBoltsAntiReload>();

            // Отложенная инициализация петли капота.
            if (hood.gameObject.GetComponent<DelayedHingeManager>() == null)
                hood.gameObject.AddComponent<DelayedHingeManager>();

            // Фикс невозможности закрыть капот.
            if (hood.gameObject.GetComponent<SatsumaCustomHoodUse>() == null)
                hood.gameObject.AddComponent<SatsumaCustomHoodUse>();

            // Фикс «выскакивающей» батареи.
            if (FsmManager.IsBatteryInstalled() && batteryPivot.parent == null)
            {
                batteryTrigger.gameObject.SetActive(true);
                batteryTrigger.gameObject.GetComponent<PlayMakerFSM>().SendEvent("ASSEMBLE");
            }

            HoodFixDone = true;
        }

        internal void ForceDetachTrailer()
        {
            StartCoroutine(ForceDetachTrailerRoutine());
        }

        private IEnumerator ForceDetachTrailerRoutine()
        {
            while (!MoprSettings.IsModActive)
                yield return null;

            for (int i = 0; i < 10; i++)
            {
                PlayMakerFSM.BroadcastEvent("TRAILERDETACH");
                yield return null;
            }
        }

        private void SetParent(Transform root, Transform newParent, string objectName)
        {
            try
            {
                root.Find(objectName).parent = newParent;
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "GAMEFIXES_PARENTCHANGING\n" + root.gameObject.name + " => " + objectName + " => " + (newParent == null ? "null" : newParent.gameObject.name));
            }
        }

        private void SetParent(Transform newParent, string objectName)
        {
            try
            {
                GameObject.Find(objectName).transform.parent = newParent;
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "GAMEFIXES_PARENTCHANGING_" + objectName);
            }
        }

        private void SetParent(Transform newParent, Transform obj)
        {
            try
            {
                obj.parent = newParent;
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "GAMEFIXES_PARENTCHANGING_UNKNOWN_TRANSFORM");
            }
        }
    }
}
