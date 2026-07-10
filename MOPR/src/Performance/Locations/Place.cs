// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// База оптимизации локации (магазин, ремонт, ферма, двор и т.п.): по дистанции включает/выключает
// «безопасные» дочерние объекты, соответствующие FSM и тени источников света, не трогая объекты из
// чёрного списка (routine-скрипты, коллайдеры и пр.). Compress убирает вложенные дубли из списка.

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using MOPR.FSM;
using MOPR.Common;
using MOPR.Rules;
using MOPR.Rules.Types;

namespace MOPR.Places
{
    internal class Place
    {
        private readonly GameObject gameObject;
        internal GameObject ThisGameObject => gameObject;
        protected internal Transform transform => gameObject.transform;

        private readonly float toggleDistance;

        // Объекты из этого списка не выключаются.
        internal List<string> GameObjectBlackList;

        internal List<Transform> DisableableChilds;
        internal List<PlayMakerFSM> PlayMakers;
        internal List<Light> LightSources;

        // Последнее применённое состояние — чтобы не гонять цикл впустую.
        protected bool isActive = true;

        public Place(string placeName, float distance = 200)
        {
            gameObject = GameObject.Find(placeName);
            if (gameObject == null)
            {
                MSCLoader.ModConsole.Error("[MOPR] Place " + placeName + " not found!");
                return;
            }

            toggleDistance = distance;
            GameObjectBlackList = new List<string>();
            PlayMakers = new List<PlayMakerFSM>();
            LightSources = new List<Light>();

            IgnoreRuleAtPlace[] ignoreRulesAtThisPlace = RulesManager.Instance.GetList<IgnoreRuleAtPlace>().Where(r => r.Place == placeName).ToArray();
            foreach (IgnoreRuleAtPlace rule in ignoreRulesAtThisPlace)
                GameObjectBlackList.Add(rule.ObjectName);
        }

        /// <summary>Включает/выключает локацию.</summary>
        public virtual void ToggleActive(bool enabled)
        {
            if (isActive == enabled)
                return;
            isActive = enabled;

            for (int i = 0; i < DisableableChilds.Count; i++)
            {
                if (DisableableChilds[i] == null)
                    continue;
                DisableableChilds[i].gameObject.SetActive(enabled);
            }

            for (int i = 0; i < PlayMakers.Count; i++)
            {
                if (PlayMakers[i] == null)
                    continue;
                PlayMakers[i].enabled = enabled;
            }

            if (LightSources.Count > 0 && FsmManager.ShadowsHouse)
            {
                for (int i = 0; i < LightSources.Count; i++)
                    LightSources[i].shadows = enabled ? LightShadows.Hard : LightShadows.None;
            }
        }

        /// <summary>Все дочерние объекты, не попавшие в чёрный список.</summary>
        internal List<Transform> GetDisableableChilds()
        {
            return gameObject.GetComponentsInChildren<Transform>(true)
                .Where(trans => !trans.gameObject.name.ContainsAny(GameObjectBlackList)).ToList();
        }

        public string GetName() => gameObject.name;

        public float GetToggleDistance() => toggleDistance;

        /// <summary>Убирает из списка потомков тех, чей предок уже в списке (чтобы не дублировать SetActive).</summary>
        internal void Compress()
        {
            List<Transform> toRemove = new List<Transform>();
            foreach (Transform child in DisableableChilds)
            {
                Transform currentChild = child;
                try
                {
                    while (currentChild.parent != null)
                    {
                        if (DisableableChilds.Contains(currentChild.parent))
                            toRemove.Add(child);

                        currentChild = currentChild.parent;
                    }
                }
                catch
                {
                    ModConsole.LogError(currentChild.Path());
                }
            }

            foreach (Transform child in toRemove)
                DisableableChilds.Remove(child);
        }

        internal List<Light> GetLightSources()
        {
            return Resources.FindObjectsOfTypeAll<GameObject>()
                .Where(g => g.transform.root == gameObject.transform && g.name.StartsWith("Light") && g.GetComponent<Light>())
                .Select(g => g.GetComponent<Light>()).ToList();
        }

        public bool IsActive => isActive;
    }
}
