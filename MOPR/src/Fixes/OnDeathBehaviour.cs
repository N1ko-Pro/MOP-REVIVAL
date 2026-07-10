// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Вешается на объект смерти игрока: при его активации вызывает переданное действие (обычно
// Core.PreSaveGame) — чтобы MOPR подготовил сцену перед сохранением, которое игра делает при смерти.

using System;
using UnityEngine;

namespace MOPR.Helpers
{
    internal class OnDeathBehaviour : MonoBehaviour
    {
        private Action action;

        public void Initialize(Action action) => this.action = action;

        private void OnEnable() => action();
    }
}
