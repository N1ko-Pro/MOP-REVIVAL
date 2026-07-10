// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Фикс «набора массы» деталей Сатсумы: игра при снятии/установке детали неверно учитывает массу
// (CarMass), из-за чего при частых респавнах машина «толстеет». Здесь мы убираем штатные экшены
// прибавления/снятия массы и сами корректно вычитаем/прибавляем массу детали при её вкл/выкл.

using System;
using HutongGames.PlayMaker;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using MOPR.FSM;
using MOPR.Common;
using MOPR.Vehicles.Cases;

namespace MOPR.Vehicles.Managers.SatsumaManagers
{
    internal class SatsumaPartMassManager : MonoBehaviour
    {
        private float objectMass;
        private FsmFloat carMass;
        private bool isClean;

        private void Awake()
        {
            if (transform.root != transform && !transform.root.gameObject.name.EqualsAny("SATSUMA(557kg, 248)", "CARPARTS"))
                Destroy(this);
        }

        private void OnEnable()
        {
            if (!isClean)
            {
                isClean = true;
                try
                {
                    PlayMakerFSM removalFSM = gameObject.GetPlayMaker("Removal");
                    if (!removalFSM)
                        throw new Exception("SatsumaPartMassManager: " + gameObject.name + " - Removal PlayMakerFSM is missing!\n\nPath: " + gameObject.Path());

                    objectMass = removalFSM.FsmVariables.GetFsmFloat("Mass").Value;
                    carMass = PlayMakerGlobals.Instance.Variables.FindFsmFloat("CarMass");

                    if (gameObject.name.ContainsAny("strut", "antenna", "marker light"))
                    {
                        RemoveActionAt(removalFSM, "Disable trigger", 0);
                        RemoveActionAt(removalFSM, "Remove part", 9);
                    }
                    else
                    {
                        RemoveActionAt(removalFSM, "Add mass", 0);
                        RemoveActionAt(removalFSM, "Remove part", 7);
                    }

                    if (gameObject.activeSelf)
                        carMass.Value -= objectMass;
                }
                catch (Exception ex)
                {
                    ExceptionManager.New(ex, true, "There was an error with SatsumaPartMassManager: " + gameObject.Path());
                }
            }

            if (gameObject.transform.root.gameObject == Satsuma.Instance.gameObject)
                carMass.Value += objectMass;
        }

        private void OnDisable()
        {
            if (gameObject.transform.root.gameObject == Satsuma.Instance.gameObject)
                carMass.Value -= objectMass;
        }

        private static void RemoveActionAt(PlayMakerFSM fsm, string stateName, int index)
        {
            FsmState state = fsm.GetState(stateName);
            if (state == null)
                throw new Exception("SatsumaPartMassManager: state '" + stateName + "' is null or empty.");

            List<FsmStateAction> actions = state.Actions.ToList();
            if (actions == null)
                throw new Exception("SatsumaPartMassManager: action list for '" + stateName + "' is null or empty.");

            actions.RemoveAt(index);
            state.Actions = actions.ToArray();
        }
    }
}
