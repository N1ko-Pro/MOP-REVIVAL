// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Помощник для правок UI настроек, которым нужен уже созданный элемент (MSCLoader строит их при
// открытии страницы). Правки применяются периодически и остаются «живыми»: страница настроек
// пересобирается при открытии и при сбросе настроек, поэтому одноразовое применение недостаточно —
// иначе, например, кнопки после «Сбросить настройки» снова встают друг под другом.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MSCLoader;

namespace MOPR.Interface.Helpers
{
    internal sealed class SettingsUiPatcher : MonoBehaviour
    {
        private static SettingsUiPatcher instance;
        private readonly List<Func<bool>> patches = new List<Func<bool>>();

        private static SettingsUiPatcher Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject host = new GameObject("MOPR_SettingsUiPatcher");
                    UnityEngine.Object.DontDestroyOnLoad(host);
                    instance = host.AddComponent<SettingsUiPatcher>();
                    instance.StartCoroutine(instance.Loop());
                }

                return instance;
            }
        }

        /// <summary>Сбрасывает набор правок (вызывать в начале построения окна настроек).</summary>
        public static void Clear()
        {
            if (instance != null)
                instance.patches.Clear();
        }

        /// <summary>Прячет пустую подпись контрола (убирает лишний отступ, напр., у дропдауна).</summary>
        public static void HideLabelWhenReady(ModSetting setting)
        {
            Instance.patches.Add(() => SettingsReflection.HideLabel(setting));
        }

        /// <summary>Размещает две кнопки на одной строке.</summary>
        public static void PairButtonsWhenReady(ModSetting a, ModSetting b)
        {
            Instance.patches.Add(() => SettingsReflection.PairButtons(a, b));
        }

        // Правки идемпотентны (когда уже применены — быстрый no-op), поэтому просто переприменяем их
        // раз в ~0.4 с. Так после пересборки/сброса страницы всё восстанавливается автоматически.
        private IEnumerator Loop()
        {
            WaitForSeconds wait = new WaitForSeconds(0.4f);
            while (true)
            {
                for (int i = 0; i < patches.Count; i++)
                {
                    try
                    {
                        patches[i]();
                    }
                    catch
                    {
                    }
                }

                yield return wait;
            }
        }
    }
}
