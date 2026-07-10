// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Хук магазина: при выкупе заказа деталей (почтовое отделение) нужно навесить ItemBehaviour на
// приехавшие пакеты. Внедряем CashRegisterBehaviour в кассу, временно активируя LOD/магазин/
// PostOrderBuy и возвращая их прежнее состояние.

using UnityEngine;

using MOPR.FSM;
using MOPR.Items.Helpers;

namespace MOPR.Items.Hooks
{
    internal static class StoreOrderHook
    {
        public static void Install()
        {
            GameObject storeLOD = GameObject.Find("STORE").transform.Find("LOD").gameObject;
            GameObject activateStore = storeLOD.transform.Find("ActivateStore").gameObject;

            // Запоминаем текущее состояние LOD/магазина, чтобы восстановить его в конце.
            bool lodLastState = storeLOD.activeSelf;
            bool activeStore = activateStore.activeSelf;

            storeLOD.SetActive(true);
            activateStore.SetActive(true);

            GameObject postOrder = GameObject.Find("STORE").transform.Find("LOD/ActivateStore/PostOffice/PostOrderBuy").gameObject;
            bool isPostOrderActive = postOrder.activeSelf;
            postOrder.SetActive(true);

            // Внедряем помощник кассы в состояние выкупа заказа.
            postOrder.FsmInject("State 3",
                GameObject.Find("STORE/StoreCashRegister/Register").AddComponent<CashRegisterBehaviour>().Packages);

            // Возвращаем прежние состояния объектов.
            postOrder.SetActive(isPostOrderActive);
            storeLOD.SetActive(lodLastState);
            activateStore.SetActive(activeStore);
        }
    }
}
