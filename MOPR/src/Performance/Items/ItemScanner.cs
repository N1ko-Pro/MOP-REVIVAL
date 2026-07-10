// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Аддитивный дозахват предметов: периодически (дёшево) проходит по контейнеру ITEMS и берёт под
// управление новичков без ItemBehaviour — страховка на случай, если предмет попал в ITEMS минуя
// игровые FSM-инъекторы. Полный проход по всей сцене намеренно не делаем — рантайм-спавны в других
// местах ловят штатные хуки.

using UnityEngine;
using MOPR.Common;

namespace MOPR.Items
{
    internal sealed class ItemScanner : MonoBehaviour
    {
        private const float ScanIntervalSeconds = 3f;

        private float timer;

        private void Update()
        {
            if (!MoprSettings.IsModActive)
                return;

            timer += Time.unscaledDeltaTime;
            if (timer < ScanIntervalSeconds)
                return;

            timer = 0f;
            ItemRegistrar.ScanItemsContainer();
        }
    }
}
