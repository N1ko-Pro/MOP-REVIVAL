// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Модуль игровой корректности предмета: заморозка на сохранение, запись предмета в сейв, сброс
// контейнеров килью, состояние гриля, звук события, загрузка позиции из сейва и LOD-подставка.
// Это корректность игры, а не оптимизация.

using System.Collections;
using UnityEngine;
using HutongGames.PlayMaker;

using MOPR.Common;
using MOPR.Common.Enumerations;
using MOPR.FSM;
using MOPR.Managers;
using MOPR.Helpers;
using MOPR.Vehicles.Cases;
using MOPR.LOD;

namespace MOPR.Items
{
    internal partial class ItemBehaviour
    {
        #region Сохранение / заморозка / килью

        /// <summary>Замораживает предмет на время записи сохранения (чтобы не сместился).</summary>
        public void Freeze()
        {
            ResetKiljuContainer();
            gameObject.AddComponent<ItemFreezer>();
        }

        /// <summary>Опустошает/переносит контейнер килью или пустую канистру у триггеров Йокке/свалки.</summary>
        internal void ResetKiljuContainer()
        {
            if (!gameObject.name.ContainsAny("empty plastic can", "kilju", "emptyca"))
                return;

            gameObject.MakePickable();
            gameObject.tag = "ITEM";

            if (ItemsManager.Instance.GetCanTrigger())
            {
                if (Vector3.Distance(transform.position, ItemsManager.Instance.GetCanTrigger().transform.position) < 10)
                {
                    transform.position = ItemsManager.Instance.LostSpawner.position;
                    kiljuInitialReset = false;

                    PlayMakerFSM fsm = gameObject.GetPlayMaker("Use");
                    if (fsm)
                        fsm.FsmVariables.GetFsmBool("ContainsKilju").Value = false;

                    gameObject.name = "empty plastic can(itemx)";
                    return;
                }
            }

            if (ItemsManager.Instance.LandfillSpawn)
            {
                if (Vector3.Distance(transform.position, ItemsManager.Instance.LandfillSpawn.position) < 5)
                {
                    transform.position = ItemsManager.Instance.LandfillSpawn.position;

                    PlayMakerFSM fsm = gameObject.GetPlayMaker("Use");
                    if (fsm)
                        fsm.FsmVariables.GetFsmBool("ContainsKilju").Value = false;

                    gameObject.name = "empty plastic can(itemx)";
                }
            }
        }

        /// <summary>Пишет состояние предмета в сейв перед сохранением.</summary>
        internal void SaveGame()
        {
            if (!gameObject.GetPlayMaker("Use"))
                return;

            PlayMakerFSM useFSM = gameObject.GetPlayMaker("Use");
            string id = useFSM.FsmVariables.GetFsmString("ID").Value;

            if (gameObject.name.StartsWith("wheel"))
            {
                SaveManager.WriteSaveTag(id + "Transform", gameObject.transform);
                return;
            }

            SaveManager.WriteItemTag(id + "Transform", gameObject.transform);
            SaveManager.WriteItemTag(id + "Consumed", useFSM.FsmVariables.GetFsmBool("Consumed").Value);

            if (!id.Contains("juiceconcentrate"))
                return;

            if (gameObject.name.ContainsAny("emptyca", "empty plastic can"))
                useFSM.FsmVariables.GetFsmBool("ContainsKilju").Value = false;

            if (gameObject.name.ContainsAny("kilju"))
                useFSM.FsmVariables.GetFsmBool("ContainsKilju").Value = true;

            SaveManager.WriteItemTag(id + "ContainsJuice", useFSM.FsmVariables.GetFsmBool("ContainsJuice").Value);
            SaveManager.WriteItemTag(id + "ContainsKilju", useFSM.FsmVariables.GetFsmBool("ContainsKilju").Value);
            SaveManager.WriteItemTag(id + "KiljuAlc", useFSM.FsmVariables.GetFsmFloat("KiljuAlc").Value);
            SaveManager.WriteItemTag(id + "KiljuSweetness", useFSM.FsmVariables.GetFsmFloat("KiljuSweetness").Value);
            SaveManager.WriteItemTag(id + "KiljuVinegar", useFSM.FsmVariables.GetFsmFloat("KiljuVinegar").Value);
            SaveManager.WriteItemTag(id + "KiljuYeast", useFSM.FsmVariables.GetFsmFloat("KiljuYeast").Value);
            useFSM.enabled = false;
        }

        #endregion

        #region Гриль / звук события

        /// <summary>Горит ли гриль (тогда предмет-гриль нельзя выключать).</summary>
        private bool GrillIsOnFire()
        {
            bool onFire = (grillFlame && grillFlame.activeSelf) || (grillTrigger && grillTrigger.activeSelf) || grillKeepActive;
            if (onFire)
                grillKeepActive = true;

            if (grillTrigger && grillTrigger.activeSelf)
                grillKeepActive = false;

            return onFire;
        }

        /// <summary>Включает/выключает звук столкновений (ящик пива) с задержкой на включении.</summary>
        private IEnumerator ToggleEventSound(bool enabled)
        {
            if (enabled)
                yield return new WaitForSeconds(2f);

            if (eventSound == null)
                yield break;

            eventSound.useCollisionSound = enabled;
        }

        #endregion

        #region Загрузка позиции / LOD

        public bool WasTransformLoaded { get; private set; }

        /// <summary>Загружает позицию из сейва (пока только для r20 battery box).</summary>
        public void LoadTransform()
        {
            WasTransformLoaded = true;
            if (!gameObject.name.Contains("r20 battery box"))
                return;

            string transformID = gameObject.GetPlayMaker("Use")?.FsmVariables.GetFsmString("UniqueTagTransform").Value;
            Transform loadedTransform;

            if (SaveManager.IsItemTagPresent(transformID))
                loadedTransform = SaveManager.ReadItemTranform(transformID);
            else if (SaveManager.IsSaveTagPresent(transformID))
                loadedTransform = SaveManager.ReadTransform(transformID);
            else
                return;

            if (loadedTransform.position == Vector3.zero)
                return;

            transform.position = loadedTransform.position;
            transform.rotation = loadedTransform.rotation;
        }

        /// <summary>Показывает/прячет LOD-подставку (кроме предметов, прикреплённых к Сатсуме).</summary>
        public void ToggleLOD(bool enabled)
        {
            if (dummy == null)
                return;

            if (transform.root == Satsuma.Instance.transform)
                enabled = false;

            dummy.ToggleActive(enabled, transform);
        }

        /// <summary>Создаёт LOD-подставку для достаточно крупных ванильных предметов (не в Performance).</summary>
        private void LoadDummyObject()
        {
            if (MoprSettings.Mode == PerformanceMode.Performance || !ItemsManager.Instance.IsVanillaItem(this))
                return;

            Vector3 size;
            switch (gameObject.name)
            {
                case "motor hoist(itemx)":
                    size = transform.Find("motorhoist_frame").GetComponent<Renderer>().bounds.size;
                    break;
                default:
                    if (GetComponent<Renderer>() == null)
                        return;

                    size = GetComponent<Renderer>().bounds.size;
                    break;
            }

            float volume = size.x * size.y * size.z;
            if (volume > (MoprSettings.Mode == PerformanceMode.Quality ? 0.01f : 0.05f))
                dummy = new LodObject(gameObject);

            if (dummy != null && volume > 1 && MoprSettings.Mode >= PerformanceMode.Balanced)
                dummy.ToggleDistance *= 2;
        }

        public LodObject LodObject => dummy;

        #endregion
    }
}
