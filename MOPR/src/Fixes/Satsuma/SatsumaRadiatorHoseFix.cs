// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Фикс radiator hose3 на самой Сатсуме: подставляет корректный префаб шланга в FSM снятия и
// вставляет наш CustomCreateHose, чтобы при отсоединении спавнился правильный экземпляр шланга.

using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using HutongGames.PlayMaker;

using MOPR.FSM;
using MOPR.FSM.Actions;
using MOPR.Managers;

namespace MOPR.Vehicles.Managers.SatsumaManagers
{
    internal class SatsumaRadiatorHoseFix : MonoBehaviour
    {
        private void Start()
        {
            PlayMakerFSM removalFSM = gameObject.GetPlayMaker("Removal");
            GameObject hosePrefab = ItemsManager.Instance.GetRadiatorHose3();
            removalFSM.FsmVariables.FindFsmGameObject("Part").Value = ItemsManager.Instance.GetRadiatorHose3();

            List<FsmStateAction> actions = removalFSM.GetState("Remove part").Actions.ToList();
            actions.Insert(0, new CustomCreateHose(gameObject, hosePrefab));
            removalFSM.GetState("Remove part").Actions = actions.ToArray();
        }
    }
}
