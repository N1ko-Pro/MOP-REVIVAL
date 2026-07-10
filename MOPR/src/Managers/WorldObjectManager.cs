// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Менеджер статических объектов мира (здания и т.п.): регистрация по имени/ссылке с выбором способа
// переключения, учёт правил ignore, перебор и подсчёт активных. Само переключение — в GenericObject.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using MOPR.Rules;
using MOPR.Rules.Types;
using MOPR.Common.Enumerations;
using MOPR.WorldObjects;
using MOPR.Common.Interfaces;

namespace MOPR.Managers
{
    internal class WorldObjectManager : IManager<GenericObject>
    {
        private static WorldObjectManager instance;
        public static WorldObjectManager Instance => instance;

        public GenericObject this[int index] => worldObjects[index];

        private readonly List<GenericObject> worldObjects;

        public int Count => worldObjects.Count;

        public WorldObjectManager()
        {
            instance = this;
            worldObjects = new List<GenericObject>();
        }

        /// <summary>Находит объект по имени и добавляет под управление. silent — не бросать при отсутствии.</summary>
        public GenericObject Add(string gameObjectName, DisableOn disableOn, int distance = 200, ToggleModes toggleMode = ToggleModes.Simple, bool silent = false)
        {
            IgnoreRule rule = RulesManager.Instance.GetList<IgnoreRule>().Find(f => f.ObjectName == gameObjectName);
            if (rule != null)
            {
                if (rule.TotalIgnore)
                    return null;
                toggleMode = ToggleModes.Renderer;
            }

            GameObject gm = GameObject.Find(gameObjectName);
            if (gm == null)
                gm = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(g => g.name == gameObjectName);

            if (!gm)
            {
                if (!silent)
                    throw new Exception("WorldObjectManager: Couldn't find gameObjectName: \"" + gameObjectName + "\".");
                return null;
            }

            return Add(gm, disableOn, distance, toggleMode);
        }

        /// <summary>Добавляет объект под управление выбранным способом переключения.</summary>
        public GenericObject Add(GameObject gameObject, DisableOn disableOn, int distance = 200, ToggleModes toggleMode = ToggleModes.Simple)
        {
            if (!gameObject)
                throw new NullReferenceException("WorldObjectManager: gameObject is null.");

            IgnoreRule rule = RulesManager.Instance.GetList<IgnoreRule>().Find(f => f.ObjectName == gameObject.name);
            if (rule != null)
            {
                if (rule.TotalIgnore)
                    return null;
                toggleMode = ToggleModes.Renderer;
            }

            GenericObject obj;
            switch (toggleMode)
            {
                default:
                    throw new NotImplementedException("Toggle mode: " + toggleMode + " is not supported by WorldObjectManager!");
                case ToggleModes.Simple:
                    obj = new SimpleObjectToggle(gameObject, disableOn, distance);
                    break;
                case ToggleModes.Renderer:
                    obj = new RendererToggle(gameObject, disableOn, distance);
                    break;
                case ToggleModes.MultipleRenderers:
                    obj = new MultipleRenderersToggle(gameObject, disableOn, distance);
                    break;
            }

            return Add(obj);
        }

        public GenericObject Add(GenericObject generic)
        {
            worldObjects.Add(generic);
            return generic;
        }

        public List<GenericObject> GetAll => worldObjects;

        public void Remove(GenericObject worldObject) => worldObjects.Remove(worldObject);

        public GameObject Get(string name)
        {
            return worldObjects.FirstOrDefault(g => g.GetName() == name)?.GameObject;
        }

        public void RemoveAt(int index) => worldObjects.RemoveAt(index);

        public int EnabledCount
        {
            get
            {
                int enabled = 0;
                foreach (GenericObject obj in GetAll)
                    if (obj.GameObject.activeSelf)
                        enabled++;
                return enabled;
            }
        }
    }
}
