// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Переключение объекта, у которого несколько дочерних Renderer'ов: скрывает всю отрисовку,
// оставляя сам объект и его логику активными. Первый рендерер служит признаком текущего состояния.

using System;
using UnityEngine;
using MOPR.Common.Enumerations;

namespace MOPR.WorldObjects
{
    internal class MultipleRenderersToggle : GenericObject
    {
        private readonly Renderer[] renderers;

        public MultipleRenderersToggle(GameObject gameObject, DisableOn disableOn = DisableOn.Distance, int distance = 200)
            : base(gameObject, distance, disableOn)
        {
            renderers = gameObject.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
                throw new Exception("[MOPR] (RenderersToggle) Couldn't find the renderers of " + gameObject.name);
        }

        public override void Toggle(bool enabled)
        {
            // Состояние уже нужное — выходим (первый рендерер как индикатор).
            if (renderers[0].enabled == enabled)
                return;

            for (int i = 0; i < renderers.Length; i++)
            {
                if (!renderers[i].gameObject.activeSelf)
                    continue;

                renderers[i].enabled = enabled;
            }
        }
    }
}
