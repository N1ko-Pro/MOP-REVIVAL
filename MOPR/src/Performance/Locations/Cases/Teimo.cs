// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Магазин Тэймо (STORE): выгружает безопасные части магазина, не ломая ресток и маршрут Тэймо на
// велосипеде. Не выключает сам магазин — только его дочерние объекты. Плюс набор точечных фиксов
// (видеопокер, слот-машина, разбиваемые окна, микроволновка, реклама и т.д.).

using System;
using System.Collections.Generic;
using HutongGames.PlayMaker.Actions;
using UnityEngine;

using MOPR.FSM;
using MOPR.Places.Cases.Misc;
using MOPR.Common;

namespace MOPR.Places
{
    internal class Teimo : Place
    {
        private readonly string[] blackList =
        {
            "STORE", "SpawnToStore", "BikeStore", "BikeHome", "Inventory", "Collider", "TeimoInShop", "Bicycle",
            "bicycle_pedals", "Pedal", "Teimo", "bodymesh", "skeleton", "pelvs", "spine", "collar", "shoulder",
            "hand", "ItemPivot", "finger", "collar", "arm", "fingers", "HeadPivot", "head", "eye_glasses_regular",
            "teimo_hat", "thig", "knee", "ankle", "OriginalPos", "TeimoInBike", "Pivot", "pelvis",
            "bicycle", "Collider", "collider", "StoreCashRegister", "cash_register", "Register", "store_", "MESH",
            "tire", "rim", "MailBox", "GeneralItems", "(Clone)", "(itemx)", "Boxes", "n2obottle", "RagDoll",
            "thigh", "shopping bag", "Advert", "advert", "ActivateBar", "FoodSpawnPoint", "BagCreator",
            "ShoppingBagSpawn", "Post", "PayMoneyAdvert", "Name", "LOTTO", "Lottery", "lottery", "Products",
            "PRODUCST", "Microwave", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "WoodSheetStore", "WoodSheetPub",
            "Bolt", "Pin", "pin", "Handle", "Explosion", "Fire", "Smoke", "Trail", "Fireball",
            "Shower", "Dust", "Shockwave", "Force", "Sound", "Point light", "smoke", "Flame", "Dynamics", "bottle",
            "needle", "Parts", "_gfx", "LookTarget", "Speak", "Functions", "Bottle", "GrillBox", "Food",
            "BeerBottle", "VodkaShot", "CoffeeCup", "Cigarettes", "Fighter2", "TargetPoint", "RayPivot",
            "TargetSelf", "AudioClips", "HitPosition", "HumanCol", "Ray", "bodymesh_fighter", "Char", "ThrowBody",
            "PlayerRigid", "GrillboxMicro", "PhysHead", "Shades", "hat", "glasses", "FighterFist", "GameLogic",
            "Buttons", "Bet", "Double", "Hold", "InsertCoin", "Deal", "TakeWin", "Pokeri", "CashSound", "videopoker_on",
            "Hatch", "HookSlot", "Disabled", "slot_machine_off", "Money", "Lock", "Cash", "Accessories", "VideoPoker", "videopoker",
            "Monitor", "screen", "button_", "BreakableWindows", "BreakableWindows", "teimo advert pile(itemx)"
        };

        public Teimo() : base("STORE")
        {
            RunInitialActions(
                BoxesFix,
                InjectVideoPoker,
                AdvertFix,
                () => GameObjectBlackList.AddRange(blackList),
                () => DisableableChilds = GetDisableableChilds(),
                FixSlotAndPoker,
                TeimoShitFix,
                FixWindowBreaking,
                CheckMicrowaveDistanceFix,
                UncleRagdollFix,
                GrillBoxFix,
                Compress);
        }

        private void RunInitialActions(params Action[] actions)
        {
            foreach (Action action in actions)
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    ExceptionManager.New(ex, false, "ACTION_FAIL_" + action.Method.Name);
                }
            }
        }

        // Фикс предметов, купленных через конверт.
        private void BoxesFix()
        {
            Transform boxes = transform.Find("Boxes");
            if (boxes != null)
                boxes.parent = null;
        }

        // Фикс исчезающей стопки рекламы при взятии.
        private void AdvertFix()
        {
            transform.Find("AdvertSpawn").transform.parent = null;
        }

        // Отвязываем видеопокер (проще, чем хукать его FSM).
        private void InjectVideoPoker()
        {
            Transform videoPoker = transform.Find("LOD/VideoPoker/HookSlot");
            if (videoPoker != null)
                PlayMakerExtensions.FsmInject(videoPoker.gameObject, "Activate cable", RemoveVideoPokerParent);
        }

        private void FixSlotAndPoker()
        {
            DisableableChilds.Remove(transform.Find("LOD/VideoPoker/Hatch/Pivot/mesh"));

            // Z-fighting стекла слот-машины.
            transform.Find("LOD/GFX_Store/SlotMachine/slot_machine 1/slot_machine_glass")
                .gameObject.GetComponent<Renderer>().material.renderQueue = 3001;
        }

        private void TeimoShitFix()
        {
            PlayMakers.AddRange(transform.Find("TeimoInShop").GetComponents<PlayMakerFSM>());
            PlayMakers.AddRange(transform.Find("TeimoInShop").GetComponents<PlayMakerFSM>());

            List<Transform> teimoShit = new List<Transform>
            {
                transform.Find("TeimoInShop/Pivot/Speak"),
                transform.Find("TeimoInShop/Pivot/FacePissTrigger"),
                transform.Find("TeimoInShop/Pivot/TeimoCollider"),
                transform.Find("GasolineFire")
            };
            DisableableChilds.AddRange(teimoShit);
        }

        private static void FixWindowBreaking()
        {
            Transform storeBreakableWindow = GameObject.Find("STORE").transform.Find("LOD/GFX_Store/BreakableWindows/BreakableWindow");
            if (storeBreakableWindow != null)
            {
                foreach (PlayMakerFSM fsm in storeBreakableWindow.gameObject.GetComponents<PlayMakerFSM>())
                    fsm.Fsm.RestartOnEnable = false;
            }

            Transform storeBreakableWindowPub = GameObject.Find("STORE").transform.Find("LOD/GFX_Pub/BreakableWindowsPub/BreakableWindowPub");
            if (storeBreakableWindowPub != null)
            {
                foreach (PlayMakerFSM fsm in storeBreakableWindowPub.gameObject.GetComponents<PlayMakerFSM>())
                    fsm.Fsm.RestartOnEnable = false;
            }
        }

        private void CheckMicrowaveDistanceFix()
        {
            // Убираем дурацкое ограничение дистанции, ниже которого рестока не происходит.
            FloatCompare checkDistance = transform.Find("Inventory").gameObject.GetComponent<PlayMakerFSM>().GetState("Check distance").Actions[1] as FloatCompare;
            checkDistance.float2 = 0;
        }

        private void UncleRagdollFix()
        {
            transform.Find("RagDoll")?.SetParent(transform.Find("LOD"));
        }

        private void GrillBoxFix()
        {
            transform.Find("LOD/GFX_Pub/Microwave/GrillboxMicro").gameObject.AddComponent<SausageInMicrowaveBehaviour>();
        }

        /// <summary>Отвязывает видеопокер, чтобы он не исчезал при оттаскивании далеко от магазина.</summary>
        private void RemoveVideoPokerParent()
        {
            Transform poker = GameObject.Find("VideoPoker").transform;
            DisableableChilds.Remove(poker);
            poker.transform.parent = null;
        }
    }
}
