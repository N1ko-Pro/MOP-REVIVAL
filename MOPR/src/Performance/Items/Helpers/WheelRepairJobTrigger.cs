// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Помощник шиномонтажа у Флеетари: триггер-зона у ремонтной кассы. Пока колесо (wheel_) в зоне,
// запрещает его выключать (DontDisable), иначе job-скрипт ремонта колёс не срабатывает. Первые ~2с
// после загрузки реагирует и на OnTriggerStay (триггер иногда не даёт Enter на старте).

using System.Collections;
using UnityEngine;

namespace MOPR.Items.Helpers
{
    internal class WheelRepairJobTrigger : MonoBehaviour
    {
        private bool isTriggerLoaded;

        private void Start()
        {
            BoxCollider col = gameObject.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(6, 5, 8);

            StartCoroutine(InitializeRoutine());
        }

        private IEnumerator InitializeRoutine()
        {
            // После паузы считаем триггер «прогретым» и перестаём реагировать на Stay.
            yield return new WaitForSeconds(2);
            isTriggerLoaded = true;
        }

        private void SetBehaviour(GameObject g, bool keepEnabled)
        {
            ItemBehaviour hook = g.GetComponent<ItemBehaviour>();
            if (hook != null)
                hook.DontDisable = keepEnabled;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.name.StartsWith("wheel_"))
                SetBehaviour(other.gameObject, true);
        }

        private void OnTriggerStay(Collider other)
        {
            // Подстраховка только на старте: пока триггер не «прогрелся».
            if (isTriggerLoaded)
                return;

            if (other.gameObject.name.StartsWith("wheel_"))
                SetBehaviour(other.gameObject, true);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.name.StartsWith("wheel_"))
                SetBehaviour(other.gameObject, false);
        }
    }
}
