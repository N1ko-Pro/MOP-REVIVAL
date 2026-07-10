// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Помощник заднего бампера Сатсумы: гасит коллизии бампера с деталями машины (иначе бампер их
// толкает при загрузке) и разово перезагружает стадии болтов (иногда не грузятся корректно).
// Прикрепление/отсоединение прокидывается через OnAttach/OnDetach.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MOPR.FSM;
using MOPR.Vehicles.Cases;

namespace MOPR.Items.Helpers
{
    internal class RearBumperBehaviour : MonoBehaviour
    {
        private Collider coll;
        private List<Collider> ignored;
        private bool seekColliders;
        private PlayMakerFSM fsm;

        private void Start()
        {
            coll = GetComponent<Collider>();
            ignored = new List<Collider>();
            fsm = gameObject.GetPlayMaker("BoltCheck");

            // Активны только если бампер прикреплён к Сатсуме.
            seekColliders = transform.root == Satsuma.Instance.transform;
            if (seekColliders)
            {
                // Игнорируем коллизии со всеми деталями Сатсумы поблизости.
                Collider[] colls = Physics.OverlapSphere(transform.position, 5);
                foreach (Collider c in colls)
                {
                    if (c.transform.root == Satsuma.Instance.transform)
                    {
                        Physics.IgnoreCollision(coll, c);
                        ignored.Add(c);
                    }
                }

                StartCoroutine(ReloadBolts());
            }
        }

        // Небольшой хак: разово перезапускаем FSM, чтобы он перечитал стадии болтов.
        private IEnumerator ReloadBolts()
        {
            fsm.Fsm.RestartOnEnable = true;
            yield return null;
            fsm.Fsm.RestartOnEnable = false;
        }

        private void OnCollisionEnter(Collision other)
        {
            if (!seekColliders)
                return;

            // Новую деталь Сатсумы, с которой столкнулись, тоже добавляем в игнор-список.
            if (other.transform.root == Satsuma.Instance.transform && !ignored.Contains(other.collider))
            {
                Physics.IgnoreCollision(coll, other.collider);
                ignored.Add(other.collider);
            }
        }

        /// <summary>Бампер сняли: через секунду возвращаем коллизии со снятыми деталями.</summary>
        internal void OnDetach()
        {
            StartCoroutine(DetachAction());
        }

        private IEnumerator DetachAction()
        {
            yield return new WaitForSeconds(1);

            for (int i = 0; i < ignored.Count; i++)
            {
                if (ignored[i] != null)
                    Physics.IgnoreCollision(coll, ignored[i], false);
            }

            seekColliders = false;
        }

        /// <summary>Бампер прикрутили обратно: снова гасим коллизии с деталями машины.</summary>
        internal void OnAttach()
        {
            seekColliders = true;
        }
    }
}
