// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Главный цикл оптимизации. Раз в ~0.7с проходит объекты мира, предметы, транспорт и локации,
// распределяя работу по кадрам. Порядок важен: предметы гасятся до транспорта и включаются после
// него, иначе они проваливаются сквозь ещё не активные машины.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MOPR.Common;
using MOPR.Common.Enumerations;
using MOPR.Helpers;
using MOPR.Items;
using MOPR.Managers;
using MOPR.Places;
using MOPR.Vehicles;
using MOPR.Vehicles.Cases;
using MOPR.WorldObjects;
using MOPR.FSM;

namespace MOPR
{
    internal partial class Core : MonoBehaviour
    {
        #region Главный цикл

        private const float LoopInterval = 0.7f;        // пауза между полными проходами, сек
        private const float VehicleLodDistance = 300f;  // дистанция LOD для выгруженного транспорта, м
        private const float YardRadius = 100f;          // радиус двора вокруг Yard, м

        private IEnumerator currentLoop;

        /// <summary>Крутится, пока мод активен, и последовательно обрабатывает все подсистемы.</summary>
        private IEnumerator LoopRoutine()
        {
            MoprSettings.IsModActive = true;

            int waitTimer = 0;

            while (MoprSettings.IsModActive)
            {
                // Тик для сторожа: если значение перестаёт расти — цикл считается зависшим.
                ++ticks;

                // Небольшая задержка перед завершением загрузки — даём предметам устояться.
                if (!IsItemInitializationDone())
                {
                    waitTimer++;
                    if (waitTimer >= WaitForPhysicsToSettleTime)
                        FinishLoading();
                }

                UpdatePlayerProximityState();

                // Отключённые модули разово возвращают свои объекты в активное состояние.
                HandleModuleToggles();

                yield return null;

                foreach (object step in ProcessWorldObjects())
                    yield return step;

                if (MoprSettings.OptimizeItemsOn)
                    foreach (object step in ProcessItems())
                        yield return step;

                if (MoprSettings.OptimizeVehiclesOn)
                    foreach (object step in ProcessVehicles())
                        yield return step;

                EnableQueuedItems();

                if (MoprSettings.OptimizePlacesOn)
                    foreach (object step in ProcessPlaces())
                        yield return step;

                RemoveDeletedItems();

                yield return new WaitForSeconds(LoopInterval);

                if (retries > 0 && !restartSucceedMessaged)
                {
                    restartSucceedMessaged = true;
                    ModConsole.Log("<color=green>[MOPR] Restart succeeded!</color>");
                }
            }
        }

        /// <summary>Определяет, находится ли игрок «во дворе» и в секторе.</summary>
        private void UpdatePlayerProximityState()
        {
            float yardThreshold = MoprSettings.ActiveDistanceValue <= 1
                ? YardRadius
                : YardRadius * MoprSettings.ActiveDistanceMultiplicationValue;

            isPlayerAtYard = Vector3.Distance(player.position, placeManager[0].transform.position) < yardThreshold;

            // В секторе игрок считается «во дворе».
            inSectorMode = SectorManager.Instance.IsPlayerInSector();
            if (inSectorMode)
                isPlayerAtYard = true;
        }

        // Отслеживание состояния модулей — чтобы восстановить объекты один раз при выключении.
        private bool itemsModuleEnabled = true;
        private bool vehiclesModuleEnabled = true;
        private bool placesModuleEnabled = true;

        /// <summary>При выключении модуля разово возвращает его объекты в активное состояние.</summary>
        private void HandleModuleToggles()
        {
            if (MoprSettings.OptimizeItemsOn)
            {
                itemsModuleEnabled = true;
            }
            else if (itemsModuleEnabled)
            {
                itemsModuleEnabled = false;
                ReenableAllItems();
            }

            if (MoprSettings.OptimizeVehiclesOn)
            {
                vehiclesModuleEnabled = true;
            }
            else if (vehiclesModuleEnabled)
            {
                vehiclesModuleEnabled = false;
                ReenableAllVehicles();
            }

            if (MoprSettings.OptimizePlacesOn)
            {
                placesModuleEnabled = true;
            }
            else if (placesModuleEnabled)
            {
                placesModuleEnabled = false;
                ReenableAllPlaces();
            }
        }

        private void ReenableAllItems()
        {
            for (int i = 0; i < ItemsManager.Instance.Count; i++)
            {
                try
                {
                    ItemBehaviour item = ItemsManager.Instance[i];
                    if (item == null)
                        continue;

                    item.ApplyToggle(true);
                    item.ToggleLOD(false);
                }
                catch (Exception ex)
                {
                    ExceptionManager.New(ex, false, "MODULE_REENABLE_ITEM_ERROR");
                }
            }
        }

        private void ReenableAllVehicles()
        {
            for (int i = 0; i < VehicleManager.Instance.Count; i++)
            {
                try
                {
                    Vehicle vehicle = VehicleManager.Instance[i];
                    if (vehicle == null)
                        continue;

                    vehicle.Toggle(true);
                    vehicle.ForceToggleUnityCar(true);
                    vehicle.ToggleLOD(false);
                }
                catch (Exception ex)
                {
                    ExceptionManager.New(ex, false, "MODULE_REENABLE_VEHICLE_ERROR");
                }
            }
        }

        private void ReenableAllPlaces()
        {
            for (int i = 0; i < placeManager.Count; i++)
            {
                try
                {
                    placeManager[i].ToggleActive(true);
                }
                catch (Exception ex)
                {
                    ExceptionManager.New(ex, false, "MODULE_REENABLE_PLACE_ERROR");
                }
            }
        }

        #endregion

        #region Шаги цикла

        // Каждый шаг делит свой список пополам и делает yield на середине — так работа за одну
        // итерацию распределяется на два кадра. Исключение любого элемента изолируется.

        private IEnumerable ProcessWorldObjects()
        {
            long half = worldObjectManager.Count >> 1;
            for (int i = 0; i < worldObjectManager.Count; ++i)
            {
                if (i == half)
                    yield return null;

                try
                {
                    ToggleWorldObject(worldObjectManager[i]);
                }
                catch (Exception ex)
                {
                    ExceptionManager.New(ex, false, "WORLD_OBJECT_TOGGLE_ERROR");
                }
            }
        }

        private IEnumerable ProcessItems()
        {
            long half = ItemsManager.Instance.Count >> 1;
            for (int i = 0; i < ItemsManager.Instance.Count; ++i)
            {
                if (i == half)
                    yield return null;

                try
                {
                    ItemBehaviour item = ItemsManager.Instance[i];
                    if (item == null || item.gameObject == null)
                    {
                        itemsToRemove.Enqueue(item);
                        continue;
                    }

                    GatherItem(item);
                }
                catch (Exception ex)
                {
                    ExceptionManager.New(ex, false, "ITEM_TOGGLE_GATHER_ERROR");
                }
            }
        }

        private IEnumerable ProcessVehicles()
        {
            long half = VehicleManager.Instance.Count >> 1;
            for (int i = 0; i < VehicleManager.Instance.Count; ++i)
            {
                if (i == half)
                    yield return null;

                try
                {
                    Vehicle vehicle = VehicleManager.Instance[i];
                    if (vehicle == null)
                    {
                        VehicleManager.Instance.RemoveAt(i);
                        continue;
                    }

                    ToggleVehicle(vehicle);
                }
                catch (Exception ex)
                {
                    ExceptionManager.New(ex, false, "VEHICLE_TOGGLE_ERROR_" + i);
                }
            }
        }

        private IEnumerable ProcessPlaces()
        {
            int full = placeManager.Count;
            long half = full >> 1;
            for (int i = 0; i < full; ++i)
            {
                if (i == half)
                    yield return null;

                try
                {
                    TogglePlace(placeManager[i]);
                }
                catch (Exception ex)
                {
                    ExceptionManager.New(ex, false, "PLACE_TOGGLE_ERROR_" + i);
                }
            }
        }

        /// <summary>Включает предметы, отложенные в очередь на шаге <see cref="GatherItem"/>.</summary>
        private void EnableQueuedItems()
        {
            while (itemsToEnable.Count > 0)
            {
                try
                {
                    ItemBehaviour behaviour = itemsToEnable.Dequeue();
                    behaviour.ApplyToggle(true);
                    behaviour.ToggleLOD(false);
                }
                catch (Exception ex)
                {
                    ExceptionManager.New(ex, false, "ITEM_TOGGLE_ENABLE_ERROR");
                }
            }
        }

        /// <summary>Убирает из менеджера предметы, которых уже не существует.</summary>
        private void RemoveDeletedItems()
        {
            while (itemsToRemove.Count > 0)
                ItemsManager.Instance.Remove(itemsToRemove.Dequeue());
        }

        #endregion

        #region Переключение одного объекта

        /// <summary>Переключает один объект мира по флагам <see cref="DisableOn"/>.</summary>
        private void ToggleWorldObject(GenericObject worldObject)
        {
            GameObject gm = worldObject.GameObject;

            // Объект уничтожен (обычно AI-пешеходы) — убираем из списка.
            if (gm == null)
            {
                worldObjectManager.Remove(worldObject);
                return;
            }

            string name = gm.name;

            if (SectorManager.Instance.IsPlayerInSector() && SectorManager.Instance.SectorRulesContains(name))
            {
                gm.SetActive(true);
                return;
            }

            if (worldObject.DisableOn.HasFlag(DisableOn.PlayerAwayFromHome) || worldObject.DisableOn.HasFlag(DisableOn.PlayerInHome))
            {
                if (name == "NPC_CARS" && inSectorMode)
                    return;

                if (name == "COMPUTER" && computerSystem != null && computerSystem.activeSelf)
                    return;

                bool enableObject = worldObject.DisableOn.HasFlag(DisableOn.PlayerAwayFromHome) ? isPlayerAtYard : !isPlayerAtYard;

                // «Не включать при уходе из дома» — оставить выключенным вне двора.
                if (worldObject.DisableOn.HasFlag(DisableOn.DoNotEnableWhenLeavingHome) && enableObject && !isPlayerAtYard)
                    return;

                worldObject.Toggle(enableObject);
            }
            else if (worldObject.DisableOn.HasFlag(DisableOn.Distance))
            {
                worldObject.Toggle(IsGenericObjectEnabled(worldObject));
            }
        }

        /// <summary>Решает судьбу одного предмета: гасит сейчас или ставит в очередь на включение.</summary>
        private void GatherItem(ItemBehaviour item)
        {
            float distance = Vector3.Distance(player.position, item.transform.position);

            // В Performance порог ужимается: в машине вне двора — 20 м, иначе 50 м.
            bool toEnable = MoprSettings.Mode == PerformanceMode.Performance
                ? IsEnabled(distance, FsmManager.IsPlayerInCar() && !isPlayerAtYard ? 20 : 50)
                : IsEnabled(distance);

            if (toEnable)
            {
                item.ToggleChangeFix();

                // Позицию грузим только после реального старта и если предмет включается.
                if (ticks > 1 && !item.WasTransformLoaded)
                    item.LoadTransform();

                // Предмет уже активен и остаётся включённым (близко к игроку) — сопровождаем физику,
                // но НЕ усыпляем его тело в кинематику. Иначе близкие интерактивные детали (двери/
                // капот/багажник Сатсумы) замерзают. Совпадает с оригиналом (там был continue).
                if (item.ActiveSelf)
                {
                    item.MaintainPhysics(freezeSleepers: false);
                    return;
                }

                itemsToEnable.Enqueue(item);
            }
            else
            {
                if (item.LodObject != null)
                    item.ToggleLOD(distance < item.LodObject.ToggleDistance);

                if (item.ActiveSelf)
                    item.ApplyToggle(false);
            }

            // Settle-окно, восстановление провалившихся и усыпление физики — покадрово в самом предмете.
            item.MaintainPhysics();
        }

        /// <summary>Переключает один транспорт: LOD, физику и полное состояние.</summary>
        private void ToggleVehicle(Vehicle vehicle)
        {
            float distance = Vector3.Distance(player.transform.position, vehicle.transform.position);
            float physicsToggleDistance = MoprSettings.ActiveDistanceValue <= 1
                ? MoprSettings.UnityCarActiveDistance
                : MoprSettings.UnityCarActiveDistance * MoprSettings.ActiveDistanceMultiplicationValue;

            switch (vehicle.VehicleType)
            {
                case VehiclesTypes.Satsuma:
                    Satsuma.Instance.ToggleElements(distance);
                    vehicle.ToggleEventSounds(distance < 3);
                    break;
                case VehiclesTypes.Jonnez:
                    vehicle.ToggleEventSounds(distance < 2);
                    break;
            }

            bool isVehicleEnabled = IsVehicleEnabled(distance);
            vehicle.ToggleUnityCar(IsVehiclePhysicsEnabled(distance, physicsToggleDistance));
            vehicle.Toggle(isVehicleEnabled);

            // LOD только когда сам транспорт выгружен и он в пределах видимости.
            vehicle.ToggleLOD(!isVehicleEnabled && distance < VehicleLodDistance);
        }

        /// <summary>Переключает одну локацию по дистанции (кроме объектов активного сектора).</summary>
        private void TogglePlace(Place place)
        {
            if (SectorManager.Instance.IsPlayerInSector() && SectorManager.Instance.SectorRulesContains(place.GetName()))
                return;

            place.ToggleActive(IsPlaceEnabled(place.transform, place.GetToggleDistance()));
        }

        #endregion

        #region Покадровое обновление

        private void Update()
        {
#if DEBUG
            if (Input.GetKeyDown(KeyCode.F5))
            {
                PreSaveGame();
                Application.LoadLevel(1);
            }

            if (Input.GetKeyDown(KeyCode.F6))
                PreSaveGame();

            if (Input.GetKeyDown(KeyCode.F7))
                SaveManager.RemoveReadOnlyAttribute();

            if (Input.GetKeyDown(KeyCode.PageUp))
                FinishLoading();

            if (Input.GetKeyDown(KeyCode.F8))
            {
                List<ItemBehaviour> b = ItemsManager.Instance.GetAll;
                foreach (ItemBehaviour i in b)
                    if (i.name.Contains("r20 battery box"))
                        i.LoadTransform();
            }
#endif
            if (!MoprSettings.IsModActive)
                return;

            Satsuma.Instance?.ForceRotation();
            Satsuma.Instance?.TickPacedWork();

            if (MoprSettings.Mode <= PerformanceMode.Performance)
                return;

            // Проверка столкновений трафика имеет смысл, только если трафик включён.
            if (!traffic.activeSelf)
                return;

            if (!trafficHighway.activeSelf && !trafficDirt.activeSelf)
                return;

            for (int i = 0; i < VehicleManager.Instance.Count; ++i)
            {
                Vehicle vehicle = VehicleManager.Instance[i];
                if (vehicle == null)
                    continue;
                if (!vehicle.IsActive)
                    continue;

                // Транспорт не на грунтовке и не на шоссе — вряд ли будет задет.
                if (Vector3.Distance(Vector3.zero, vehicle.transform.position) < 1000)
                    continue;

                if (vehicle.IsTrafficCarInArea())
                    vehicle.ToggleUnityCar(true);
            }
        }

        #endregion
    }
}
