// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Базовый класс управляемого статического объекта мира (здания, природа и т.п.). Хранит целевой
// GameObject, дистанцию и условия выгрузки; конкретный способ переключения задают наследники.

using UnityEngine;
using MOPR.Common.Enumerations;

namespace MOPR.WorldObjects
{
    internal abstract class GenericObject
    {
        protected readonly GameObject gameObject;

        public GameObject GameObject => gameObject;
        public Transform transform => gameObject.transform;

        /// <summary>Базовая дистанция переключения (м). -1 означает «никогда не включать».</summary>
        public int Distance { get; }

        /// <summary>Условия/модификаторы выгрузки объекта.</summary>
        public DisableOn DisableOn { get; }

        /// <summary>Персональный минимальный порог переключения вне сектора (0 — не задан).</summary>
        public int MinimumToggleDistance { get; set; }

        protected GenericObject(GameObject gameObject, int distance = 200, DisableOn disableOn = DisableOn.Distance)
        {
            this.gameObject = gameObject;
            Distance = distance;
            DisableOn = disableOn;
        }

        /// <summary>Включает или выключает объект выбранным для наследника способом.</summary>
        public abstract void Toggle(bool enabled);

        public string GetName() => gameObject.name;
    }
}
