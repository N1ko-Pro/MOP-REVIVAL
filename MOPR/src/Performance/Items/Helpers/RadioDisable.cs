// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Помощник радио: пока радио играет (объект Channel активен), запрещает оптимизациям выключать
// радио, выставляя DontDisable на его ItemBehaviour. Возвращает флаг, когда музыка выключена.

using UnityEngine;

namespace MOPR.Items.Helpers
{
    internal class RadioDisable : MonoBehaviour
    {
        private ItemBehaviour item;
        private bool dontAct;

        private void Awake()
        {
            item = transform.parent.gameObject.GetComponent<ItemBehaviour>();

            // Запоминаем исходное значение: если предмет и так нельзя выключать — сами ничего не трогаем.
            if (item)
                dontAct = item.DontDisable;
        }

        private void OnEnable()
        {
            if (item && !dontAct)
                item.DontDisable = true;
        }

        private void OnDisable()
        {
            if (item && !dontAct)
                item.DontDisable = false;
        }
    }
}
