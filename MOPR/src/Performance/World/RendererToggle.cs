// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Переключение объекта через единственный Renderer: сам GameObject остаётся активным (его FSM/
// коллайдеры продолжают работать), скрывается только отрисовка. Нужно там, где полное отключение
// объекта ломало бы логику.

using System;
using UnityEngine;
using MOPR.Common.Enumerations;

namespace MOPR.WorldObjects
{
    internal class RendererToggle : GenericObject
    {
        private readonly Renderer renderer;

        public RendererToggle(GameObject gameObject, DisableOn disableOn, int distance = 200)
            : base(gameObject, distance, disableOn)
        {
            renderer = gameObject.GetComponent<Renderer>();
            if (renderer == null)
                throw new Exception("[MOPR] Couldn't find the renderer of " + gameObject.name);
        }

        public override void Toggle(bool enabled)
        {
            // Ничего не делаем, если объект неактивен или состояние уже нужное.
            if (!renderer.gameObject.activeSelf || renderer.enabled == enabled)
                return;

            renderer.enabled = enabled;
        }
    }
}
