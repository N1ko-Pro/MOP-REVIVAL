// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Рантайм-компонент одного подбираемого предмета (ядро). Класс разбит на partial-модули:
//   * ItemBehaviour.cs           — поля, конструктор, жизненный цикл Unity, биндеры.
//   * ItemBehaviour.Toggling.cs  — режим/гварды/settle/восстановление (ApplyToggle, MaintainPhysics).
//   * ItemBehaviour.GameFixes.cs — сейв/заморозка/kilju/гриль/LOD/загрузка позиции.
// Компонент самрегистрируется в ItemsManager. FSM-фиксы — в Fixes/Items/ItemFsmFixes, обнаружение —
// в ItemRegistrar, дозахват — в ItemScanner, помощники — в Performance/Items/Helpers.

using UnityEngine;
using HutongGames.PlayMaker;

using MOPR.Common;
using MOPR.Common.Interfaces;
using MOPR.FSM;
using MOPR.Managers;
using MOPR.Places;
using MOPR.LOD;
using MOPR.Items.Fixes;

namespace MOPR.Items
{
    internal partial class ItemBehaviour : MonoBehaviour, ILod
    {
        private const float MovingThresholdSqr = 0.01f; // (0.1 м/с)^2 — предметы оседают медленно.
        private const float SettleSeconds = 3f;          // окно «заморозки» после включения.
        private const float SpoilTimeFactor = 0.33f;     // особый счёт времени порчи в MSC.
        private const float FallThroughY = -50f;          // ниже — предмет провалился сквозь мир.

        private ItemToggleMode mode;

        internal Rigidbody rb;
        private Renderer renderer;
        private Vector3 savedPosition;
        private bool active = true;

        // Settle-окно.
        private bool settling;
        private float settleUntil;

        // Порча (еда) — компенсируется только в режиме Full (в PhysicsOnly FSM тикает сам).
        private FsmFloat spoilRate;
        private FsmFloat spoilRateFridge;
        private FsmFloat condition;
        private float timeDisabled;

        // Частные игровые переменные/гварды.
        private FsmBool batteryOnCharged;
        private FsmFloat floorJackTriggerY;
        private GameObject grillFlame, grillTrigger;
        private bool grillKeepActive;
        private bool isObjectOnGrill;
        private bool kiljuInitialReset;
        private bool fsmFixesOnActive;
        private EventSounds eventSound;

        private LodObject dummy;

        #region Флаги для помощников

        private bool dontDisable;
        /// <summary>Запрет выключения (радио играет, колесо у кассы ремонта и т.п.). Ставят помощники.</summary>
        internal bool DontDisable
        {
            get => dontDisable;
            set => dontDisable = value;
        }

        private bool ignoreRenderer;
        /// <summary>Не трогать рендер предмета (для ignore-правил).</summary>
        public bool IgnoreRenderer
        {
            get => ignoreRenderer;
            set
            {
                if (value && !ignoreRenderer && renderer)
                    renderer.enabled = true;

                ignoreRenderer = value;
            }
        }

        /// <summary>Помечает, что предмет сейчас на гриле (его нельзя выключать). Ставит GrillTriggerBehaviour.</summary>
        public void IsObjectOnGrill(bool value) => isObjectOnGrill = value;

        #endregion

        #region Конструктор и жизненный цикл Unity

        public ItemBehaviour()
        {
            // На клоне-подставке LOD (стоит в спец-позиции) ItemBehaviour не нужен.
            if (transform.position == LodObject.LodStoragePosition)
                return;

            // Защита от двойного навешивания.
            if (gameObject.GetComponents<ItemBehaviour>().Length > 1)
            {
                Destroy(this);
                return;
            }

            // Выбор режима (+ правила ignore/fullignore).
            ItemModeDecision decision = ItemModeDecider.Decide(gameObject);
            mode = decision.Mode;

            rb = GetComponent<Rigidbody>();
            renderer = decision.KeepRenderer ? GetComponent<Renderer>() : null;

            if (decision.Destroy)
            {
                Destroy(this);
                return;
            }

            savedPosition = transform.position;

            // Шлем: минимальная инициализация (иначе баг с невозможностью снова надеть).
            if (gameObject.name == "helmet(itemx)")
                return;

            // Домкраты: получаем FsmFloat Y триггера (в работе — не выключаем).
            if (gameObject.name.EqualsAny("floor jack(itemx)", "car jack(itemx)"))
                floorJackTriggerY = transform.Find("Trigger").gameObject.GetComponent<PlayMakerFSM>().FsmVariables.GetFsmFloat("Y");

            // Пакет amis-auto: FSM-фиксы откладываем до активации (State 1/Load сбрасывают переменные).
            if (transform.root.gameObject.name == "amis-auto ky package(xxxxx)")
                fsmFixesOnActive = true;
            else
                ItemFsmFixes.Apply(this);

            // HACK: колесо у кассы ремонтной — не выключаем (триггер иногда не срабатывает на загрузке).
            if (gameObject.name.StartsWith("wheel_"))
            {
                RepairShop repairShop = PlaceManager.Instance[2] as RepairShop;
                if (repairShop != null && Vector3.Distance(transform.position, repairShop.GetCashRegister().position) < 7)
                    dontDisable = true;
            }

            timeDisabled = Time.timeSinceLevelLoad;

            // Усыпляем покоящееся тело (кроме брошенной тары, которая ещё летит).
            if (!gameObject.name.EqualsAny("empty bottle(Clone)", "empty pack(Clone)", "empty cup(Clone)", "coffee cup(Clone)", "empty glass(Clone)"))
            {
                if (rb != null && rb.velocity.magnitude <= 0.1f)
                    rb.Sleep();
            }

            if (gameObject.name.Equals("spoiled pike(itemx)") && !gameObject.tag.Equals("PART"))
                gameObject.MakePickable();

            if (gameObject.name == "beer case(itemx)")
                eventSound = GetComponent<EventSounds>();

            LoadDummyObject();
        }

        private void Awake()
        {
            ItemsManager.Instance.Add(this);

            // Предмет заспавнился «под миром» — вернём к спавнеру потеряшек.
            if (transform.position.y < -100 && transform.position.x != 0 && transform.position.z != 0)
                transform.position = ItemsManager.Instance.LostSpawner.position;
        }

        private void OnEnable()
        {
            if (!kiljuInitialReset || gameObject.name == "emptyca")
            {
                kiljuInitialReset = true;
                ResetKiljuContainer();
            }

            if (fsmFixesOnActive)
            {
                fsmFixesOnActive = false;
                ItemFsmFixes.Apply(this);
            }

            // Компенсация порчи за время выключения (только Full: в PhysicsOnly FSM порчи тикает сам).
            if (mode == ItemToggleMode.Full && spoilRate != null && condition != null)
            {
                float currentSpoilRate = Yard.Instance.IsItemInFridge(gameObject) ? spoilRateFridge.Value : spoilRate.Value;
                condition.Value -= (Time.timeSinceLevelLoad - timeDisabled) * currentSpoilRate * SpoilTimeFactor;
            }
        }

        private void OnDisable()
        {
            if (gameObject.name == "emptyca")
                ResetKiljuContainer();

            if (spoilRate != null)
                timeDisabled = Time.timeSinceLevelLoad;
        }

        private void OnDestroy() => RemoveSelf();

        /// <summary>Снимает предмет с учёта менеджера (из FSM уничтожения и OnDestroy).</summary>
        public void RemoveSelf() => ItemsManager.Instance.Remove(this);

        #endregion

        #region Биндеры для ItemFsmFixes

        internal void BindSpoilage(FsmFloat rate, FsmFloat cond, FsmFloat fridge)
        {
            spoilRate = rate;
            condition = cond;
            spoilRateFridge = fridge;
        }

        internal void BindBattery(FsmBool onCharged) => batteryOnCharged = onCharged;

        internal void BindGrill(GameObject flame, GameObject trigger)
        {
            grillFlame = flame;
            grillTrigger = trigger;
        }

        #endregion
    }
}
