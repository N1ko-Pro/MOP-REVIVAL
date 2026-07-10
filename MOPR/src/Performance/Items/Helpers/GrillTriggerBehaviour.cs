// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Помощник гриля: висит на триггере сосисок гриля. Пока в триггере лежит предмет, сообщает ему
// (через IsObjectOnGrill), что он «на гриле» — такой предмет нельзя выключать, иначе прервётся
// готовка.

using UnityEngine;

namespace MOPR.Items.Helpers
{
    internal class GrillTriggerBehaviour : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            SetOnGrill(other, true);
        }

        private void OnTriggerExit(Collider other)
        {
            SetOnGrill(other, false);
        }

        private void SetOnGrill(Collider other, bool onGrill)
        {
            ItemBehaviour item = other.gameObject.GetComponent<ItemBehaviour>();
            if (item != null)
                item.IsObjectOnGrill(onGrill);
        }
    }
}
