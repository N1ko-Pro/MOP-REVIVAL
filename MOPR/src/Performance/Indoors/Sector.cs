// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Сектор — триггер-объём помещения. Когда игрок входит внутрь, сектор регистрируется активным
// (это включает режим «в помещении» для DDD, предметов и куллера декора). Само отсечение декора
// выполняет IndoorCuller — здесь только детекция и белый список объектов, которые гасить нельзя.

using UnityEngine;
using MOPR.Managers;

namespace MOPR
{
    internal class Sector : MonoBehaviour
    {
        public string[] IgnoreList { get; private set; }
        public int DrawDistance { get; private set; }

        public void Initialize(Vector3 size, int drawDistance, params string[] ignoreList)
        {
            // Слой Ignore Raycast — чтобы триггер не мешал «руке» игрока.
            gameObject.layer = 2;
            DrawDistance = drawDistance;

            BoxCollider collider = gameObject.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size = size;

            IgnoreList = ignoreList ?? new string[0];

            transform.parent = Core.Instance.gameObject.transform;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject == SectorManager.Instance.PlayerCheck)
                SectorManager.Instance.AddActiveSector(this);
            else
                Physics.IgnoreCollision(GetComponent<Collider>(), other);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject == SectorManager.Instance.PlayerCheck)
                SectorManager.Instance.RemoveActiveSector(this);
        }
    }
}
