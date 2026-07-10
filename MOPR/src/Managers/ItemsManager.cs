// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Менеджер предметов: владеет списком ItemBehaviour (регистрация/удаление/перебор для цикла) и
// точками входа сцены. Обнаружение — в ItemRegistrar, дозахват — в ItemScanner, игровые хуки —
// в Performance/Items/Hooks и Fixes/Items.

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using MOPR.Common;
using MOPR.Items;
using MOPR.Items.Hooks;
using MOPR.Items.Fixes;
using MOPR.Common.Interfaces;

namespace MOPR.Managers
{
    internal class ItemsManager : IManager<ItemBehaviour>
    {
        private static ItemsManager instance;
        public static ItemsManager Instance => instance ?? (instance = new ItemsManager());

        public ItemBehaviour this[int index] => itemHooks[index];

        private readonly List<ItemBehaviour> itemHooks = new List<ItemBehaviour>();

        private Transform lostSpawner, landfillSpawn;
        private GameObject canTrigger;
        private ItemBehaviour bucket;

        private ItemsManager() { }

        /// <summary>Разовая инициализация подсистемы предметов при загрузке сцены.</summary>
        internal void Initialize()
        {
            lostSpawner = GameObject.Find("LostSpawner").transform;
            landfillSpawn = GameObject.Find("LANDFILL").transform.Find("LandfillSpawn");

            // Внедряем хук в игровые спавнеры, чтобы новые предметы получали ItemBehaviour.
            Transform spawner = GameObject.Find("Spawner").transform;
            SpawnScriptInjector.Inject(spawner.Find("CreateItems").gameObject);
            SpawnScriptInjector.Inject(spawner.Find("CreateSpraycans").gameObject);
            SpawnScriptInjector.Inject(spawner.Find("CreateShoppingbag").gameObject);
            SpawnScriptInjector.Inject(spawner.Find("CreateMooseMeat").gameObject);
            SpawnScriptInjector.Inject(GameObject.Find("fish trap(itemx)").transform.Find("Spawn").gameObject);

            GetCanTrigger();

            // Хук выкупа заказа деталей в магазине.
            StoreOrderHook.Install();

            // Обнаружение и регистрация стартовых предметов.
            ItemRegistrar.RegisterAll();

            // Брошенные бутылки/стаканы/пачки.
            ThrowableBottleHook.Install();

            // Префабы дров и брёвен (без FSM).
            ItemPrefabHooks.HookFirewood();
            ItemPrefabHooks.HookLog();

            // Фикс спавна radiator hose3.
            RadiatorHose3Fix.Create();
        }

        public ItemBehaviour Add(ItemBehaviour newHook)
        {
            itemHooks.Add(newHook);
            return newHook;
        }

        public void Remove(ItemBehaviour objectHook)
        {
            if (itemHooks.Contains(objectHook))
                itemHooks.Remove(objectHook);
        }

        public void RemoveAt(int index)
        {
            if (itemHooks.Contains(itemHooks[index]))
                itemHooks.Remove(itemHooks[index]);
        }

        public int Count => itemHooks.Count;

        public List<ItemBehaviour> GetAll => itemHooks;

        /// <summary>Триггер канистры килью Йокке.</summary>
        public GameObject GetCanTrigger()
        {
            if (!canTrigger)
                canTrigger = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(g => g.name == "CanTrigger");
            return canTrigger;
        }

        public Transform LostSpawner => lostSpawner;
        public Transform LandfillSpawn => landfillSpawn;

        // radiator hose3 — состояние живёт в фиксе, здесь тонкие делегаты для внешних вызовов.
        internal void SetCurrentRadiatorHose(GameObject g) => RadiatorHose3Fix.SetRealHose(g);
        internal GameObject GetRadiatorHose3() => RadiatorHose3Fix.RealHose;
        internal void OnSave() => RadiatorHose3Fix.OnSave();

        public ItemBehaviour GetBucket()
        {
            if (bucket == null)
                bucket = itemHooks.First(g => g.name == "bucket(itemx)");
            return bucket;
        }

        public int EnabledCount
        {
            get
            {
                int enabled = 0;
                foreach (ItemBehaviour item in itemHooks)
                    if (item.ActiveSelf)
                        enabled++;
                return enabled;
            }
        }

        public bool IsVanillaItem(ItemBehaviour item)
        {
            if (item.gameObject.name.Contains("haybale"))
                return true;

            return ItemNameList.Names.Contains(item.gameObject.name.Replace("(itemx)", "").Replace("(Clone)", ""));
        }
    }
}
