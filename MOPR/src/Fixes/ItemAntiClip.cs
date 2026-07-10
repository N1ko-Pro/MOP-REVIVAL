// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Триггер-ловушка под коттеджем: ловит предметы, проваливающиеся сквозь пол, и приподнимает их
// на Y+1, чтобы они не терялись под миром.

using UnityEngine;

namespace MOPR.Helpers
{
    internal class ItemAntiClip : MonoBehaviour
    {
        private void Start()
        {
            BoxCollider trigger = gameObject.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(5.35f, 5f, 5f);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag == "PART")
                other.gameObject.transform.position = other.gameObject.transform.position + Vector3.up;
        }
    }
}
