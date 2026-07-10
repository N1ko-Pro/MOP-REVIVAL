// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Хуки сохранения: находит объекты SAVEGAME и внедряет в их FSM вызов PreSaveGame, чтобы перед
// записью всё было включено и заморожено (сейв фиксирует корректные позиции). Также вешает
// сохранение при смерти игрока и при звонке большому Суски.

using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using MOPR.Common;
using MOPR.Common.Enumerations;
using MOPR.FSM;
using MOPR.FSM.Actions;
using MOPR.Helpers;
using MOPR.Items;
using MOPR.Managers;
using HutongGames.PlayMaker;

namespace MOPR
{
    internal partial class Core : MonoBehaviour
    {
        #region Действия сохранения

        /// <summary>Ищет объекты SAVEGAME и внедряет в них хуки предсохранения.</summary>
        private void HookPreSaveGame()
        {
            try
            {
                // Перебор Resources дорогой — материализуем один раз.
                GameObject[] saveGames = Resources.FindObjectsOfTypeAll<GameObject>()
                    .Where(obj => obj.name.Contains("SAVEGAME"))
                    .ToArray();

                int i = 0;
                for (; i < saveGames.Length; i++)
                {
                    bool useInnactiveFix = false;
                    bool isJail = false;

                    GameObject savegame = saveGames[i];

                    if (!savegame.activeSelf)
                    {
                        useInnactiveFix = true;
                        savegame.SetActive(true);
                    }

                    if (savegame.transform.parent != null && savegame.transform.parent.name == "JAIL" && !savegame.transform.parent.gameObject.activeSelf)
                    {
                        useInnactiveFix = true;
                        isJail = true;
                        savegame.transform.parent.gameObject.SetActive(true);
                    }

                    savegame.FsmInject("Mute audio", PreSaveGame);
                    savegame.FsmInject("Wait for click", SaveManager.RemoveReadOnlyAttribute);

                    if (useInnactiveFix)
                    {
                        if (isJail)
                        {
                            savegame.transform.parent.gameObject.SetActive(false);
                            continue;
                        }

                        savegame.SetActive(false);
                    }
                }

                // Сохранение при смерти.
                GameObject onDeathSaveObject = new GameObject("MOPR_OnDeathSave");
                onDeathSaveObject.transform.parent = GameObject.Find("Systems").transform.Find("Death/GameOverScreen");
                OnDeathBehaviour behaviour = onDeathSaveObject.AddComponent<OnDeathBehaviour>();
                behaviour.Initialize(PreSaveGame);
                i++;

                // Сохранение при звонке большому Суски.
                GameObject telephone = GameObject.Find("Telephone");
                if (telephone != null)
                {
                    telephone.transform.Find("Logic/UseHandle").GetComponent<PlayMakerFSM>().GetState("Pick phone").InsertAction(0, new CustomSuskiLargeFlip());
                    i++;
                }

                ModConsole.Log("[MOPR] Hooked " + i + " save points!");
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, true, "SAVE_HOOK_ERROR");
            }
        }

        /// <summary>Готовит игру к сохранению: останавливает цикл, включает и замораживает всё.</summary>
        public void PreSaveGame()
        {
            ModConsole.Log("[MOPR] Initializing Pre-Save Actions...");
            SaveManager.ReleaseSave();
            MoprSettings.IsModActive = false;
            StopCoroutine(currentLoop);
            StopCoroutine(currentControlCoroutine);

            SaveManager.RemoveReadOnlyAttribute();
            ItemsManager.Instance.OnSave();

            ToggleAll(true, ToggleAllMode.OnSave);
            ModConsole.Log("[MOPR] Pre-Save Actions Completed!");
        }

        /// <summary>Запускает отложенное предсохранение (используется звонком Суски).</summary>
        public void DelayedPreSave()
        {
            if (currentDelayedSaveRoutine != null)
                StopCoroutine(currentDelayedSaveRoutine);

            currentDelayedSaveRoutine = DelayedSaveRoutine();
            StartCoroutine(currentDelayedSaveRoutine);
        }

        private IEnumerator currentDelayedSaveRoutine;

        private IEnumerator DelayedSaveRoutine()
        {
            yield return new WaitForSeconds(1);
            if (FsmManager.IsSuskiLargeCall())
                PreSaveGame();
        }

        #endregion
    }
}
