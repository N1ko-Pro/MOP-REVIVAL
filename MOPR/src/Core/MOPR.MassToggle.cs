// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Массовое переключение: разом включает или выключает все управляемые объекты (мир, предметы,
// транспорт, локации, Сатсума). Применяется при загрузке, сохранении (с заморозкой) и при аварийном
// восстановлении.

using System;
using System.Linq;
using UnityEngine;
using MOPR.Common;
using MOPR.Common.Enumerations;
using MOPR.Items;
using MOPR.Managers;
using MOPR.Vehicles.Cases;
using HutongGames.PlayMaker;

namespace MOPR
{
    internal partial class Core : MonoBehaviour
    {
        #region Аварийный выключатель оптимизации

        /// <summary>Приводит мод в состояние по чекбоксу «Отключить оптимизацию».</summary>
        public void ApplyOptimizationSetting()
        {
            if (MoprSettings.DisableOptimizationOn)
                StopOptimization();
            else
                StartOptimization();
        }

        /// <summary>Останавливает оптимизацию и возвращает игре все объекты (вкл. обратно).</summary>
        private void StopOptimization()
        {
            if (!MoprSettings.IsModActive)
                return;

            MoprSettings.IsModActive = false;
            if (currentLoop != null)
                StopCoroutine(currentLoop);
            if (currentControlCoroutine != null)
                StopCoroutine(currentControlCoroutine);

            ToggleAll(true);
            ModConsole.Log("[MOPR] Optimization disabled (emergency switch).");
        }

        /// <summary>Снова запускает оптимизацию с чистого состояния.</summary>
        private void StartOptimization()
        {
            if (MoprSettings.IsModActive)
                return;

            ToggleAll(false, ToggleAllMode.OnLoad);
            Startup();
            ModConsole.Log("[MOPR] Optimization enabled.");
        }

        #endregion

        #region Массовое переключение

        /// <summary>Включает или выключает все управляемые объекты сцены.</summary>
        public void ToggleAll(bool enabled, ToggleAllMode mode = ToggleAllMode.Default)
        {
            ModConsole.Log("[MOPR] Toggling all to " + enabled.ToString().ToUpper() + " in mode " + mode.ToString().ToUpper());

            ToggleAllWorldObjects(enabled);
            ModConsole.Log("[MOPR] Toggled WORLD OBJECTS");

            ToggleAllItems(enabled, mode);
            ModConsole.Log("[MOPR] Toggled ITEMS");

            ResetKiljuContainers(mode);
            ModConsole.Log("[MOPR] Toggled KILJU");

            ToggleAllVehicles(enabled, mode);
            ModConsole.Log("[MOPR] Toggled VEHICLES");

            ToggleAllPlaces(enabled);
            ModConsole.Log("[MOPR] Toggled PLACES");

            TeleportKiljuBottles(mode);
            ModConsole.Log("[MOPR] Toggled KILJU TELEPORT");

            ToggleSatsumaElements(enabled, mode);
            ModConsole.Log("[MOPR] Toggled SATSUMA ELEMENTS");

            ModConsole.Log("[MOPR] Toggle done!");
        }

        private void ToggleAllWorldObjects(bool enabled)
        {
            for (int i = 0; i < worldObjectManager.Count; i++)
            {
                try
                {
                    worldObjectManager[i]?.Toggle(enabled);
                }
                catch (Exception ex)
                {
                    ExceptionManager.New(ex, false, "TOGGLE_ALL_WORLD_OBJECTS_ERROR");
                }
            }
        }

        private void ToggleAllItems(bool enabled, ToggleAllMode mode)
        {
            for (int i = 0; i < ItemsManager.Instance.Count; i++)
            {
                try
                {
                    ItemBehaviour item = ItemsManager.Instance[i];
                    item.ApplyToggle(enabled);

                    // При сохранении замораживаем предмет, чтобы он не сместился.
                    if (mode == ToggleAllMode.OnSave)
                    {
                        item.gameObject.SetActive(true);
                        item.Freeze();
                        item.SaveGame();
                        item.ToggleLOD(false);
                    }
                    else if (enabled)
                    {
                        // Оптимизация выключается/сбрасывается (StopOptimization, Watchdog, mopr stop):
                        // цикл предметов больше не крутится и не снимет settle-кинематику, выставленную
                        // ApplyToggle→ReturnToRest. Возвращаем свободным предметам живую физику сразу —
                        // иначе кинематический груз в салоне «приклеивает» машину к земле.
                        item.RestoreLivePhysics();
                    }
                }
                catch (Exception ex)
                {
                    ExceptionManager.New(ex, false, "TOGGLE_ALL_ITEMS_ERROR");
                }
            }
        }

        private void ToggleAllVehicles(bool enabled, ToggleAllMode mode)
        {
            for (int i = 0; i < VehicleManager.Instance.Count; i++)
            {
                try
                {
                    VehicleManager.Instance[i].Toggle(enabled);

                    if (mode == ToggleAllMode.OnLoad)
                    {
                        VehicleManager.Instance[i].ForceToggleUnityCar(false);
                    }
                    else if (mode == ToggleAllMode.OnSave)
                    {
                        VehicleManager.Instance[i].ToggleUnityCar(enabled);
                        VehicleManager.Instance[i].Freeze();
                    }
                }
                catch (Exception ex)
                {
                    ExceptionManager.New(ex, false, "TOGGLE_ALL_VEHICLE_ERROR_" + i);
                }
            }

            Guard("TOGGLE_SATSUMA_GLUE_ALL_ERROR", () =>
            {
                if (mode == ToggleAllMode.OnSave)
                    Satsuma.Instance.OnSaveGlueAll();
            });
        }

        private void ToggleAllPlaces(bool enabled)
        {
            for (int i = 0; i < placeManager.Count; i++)
            {
                try
                {
                    placeManager[i].ToggleActive(enabled);
                }
                catch (Exception ex)
                {
                    ExceptionManager.New(ex, false, "TOGGLE_ALL_PLACES_" + i);
                }
            }
        }

        /// <summary>Опустошает контейнеры килью/канистры (и сохраняет их при записи игры).</summary>
        private void ResetKiljuContainers(ToggleAllMode mode)
        {
            Guard("KILJU_RESET_FORCE_ERROR", () =>
            {
                foreach (GameObject bottle in Resources.FindObjectsOfTypeAll<GameObject>().Where(g => g.name.ContainsAny("kilju", "emptyca", "empty plastic can")))
                {
                    bottle.GetComponent<ItemBehaviour>()?.ResetKiljuContainer();
                    if (mode == ToggleAllMode.OnSave)
                    {
                        bottle.SetActive(true);
                        bottle.GetComponent<ItemBehaviour>()?.SaveGame();
                    }
                }
            });
        }

        /// <summary>Принудительно телепортирует бутылки килью на место при сохранении.</summary>
        private void TeleportKiljuBottles(ToggleAllMode mode)
        {
            Guard("TOGGLE_ALL_JOBS_DRUNK", () =>
            {
                if (mode != ToggleAllMode.OnSave)
                    return;

                GameObject canTrigger = ItemsManager.Instance.GetCanTrigger();
                if (canTrigger)
                {
                    if (!canTrigger.transform.parent.gameObject.activeSelf)
                        canTrigger.transform.parent.gameObject.SetActive(true);
                    canTrigger.GetComponent<PlayMakerFSM>().SendEvent("STOP");
                }
            });
        }

        /// <summary>Приводит уровень детализации Сатсумы к нужному состоянию.</summary>
        private void ToggleSatsumaElements(bool enabled, ToggleAllMode mode)
        {
            Guard("TOGGLE_ALL_SATSUMA_TOGGLE_ELEMENTS", () =>
            {
                if (mode == ToggleAllMode.OnSave)
                    Satsuma.Instance.ToggleElements(0);
                else
                    Satsuma.Instance.ToggleElements(enabled ? 0 : 10000);

                // Массовое переключение обязано примениться синхронно (сохранение/загрузка сериализует
                // состояние немедленно) — сбрасываем отложенную очередь сглаживания.
                Satsuma.Instance.FlushPacedToggles();
            });
        }

        #endregion
    }
}
