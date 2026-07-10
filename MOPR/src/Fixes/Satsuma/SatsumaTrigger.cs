// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Триггер сборки детали Сатсумы: запоминает pivot-родителя и при своём выключении просит
// SatsumaTriggerFixer проверить, не осталась ли точка сборки пустой (тогда триггер надо вернуть).

using System;
using UnityEngine;

using MOPR.Common;

namespace MOPR.Vehicles.Managers.SatsumaManagers
{
    internal class SatsumaTrigger : MonoBehaviour
    {
        public Transform Pivot { get; private set; }

        private void Awake()
        {
            try
            {
                PlayMakerFSM fsm = GetComponent<PlayMakerFSM>();
                if (fsm == null)
                {
                    Destroy(this);
                    return;
                }

                GameObject gameObjectPivot = fsm.FsmVariables.GetFsmGameObject("Parent").Value;
                if (gameObjectPivot == null)
                {
                    Destroy(this);
                    return;
                }

                Pivot = gameObjectPivot.transform;
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, gameObject.Path());
            }
        }

        private void OnDisable()
        {
            if (Pivot != null)
                SatsumaTriggerFixer.Instance.Check(this);
        }
    }
}
