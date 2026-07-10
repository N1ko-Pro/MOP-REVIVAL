// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Триггер над «бревном», удерживающим прицеп на земле: если он касается карты/двора, сбрасывает
// крен прицепа по X в 0 — чтобы прицеп не проваливался/не заваливался под пол.

using UnityEngine;
using MOPR.Common;

namespace MOPR.Vehicles.Managers
{
    internal class TrailerLogUnderFloor : MonoBehaviour
    {
        private readonly GameObject log;
        private readonly Transform flatbed;

        public TrailerLogUnderFloor()
        {
            flatbed = transform.parent;
            log = flatbed.Find("Log").gameObject;

            transform.localPosition = new Vector3(0, 0.5f, 3.3f);
            BoxCollider trigger = gameObject.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(0.5f, 0.7f, 0.5f);
        }

        private void OnTriggerStay(Collider other)
        {
            if (!log.activeSelf)
                return;

            if (other.gameObject.transform.root.gameObject.name.EqualsAny("MAP", "YARD"))
            {
                Vector3 angles = flatbed.localEulerAngles;
                angles.x = 0;
                flatbed.localEulerAngles = angles;
            }
        }
    }
}
