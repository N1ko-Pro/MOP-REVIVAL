// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Необязательный косметический фикс ванильного бага MSC: ряска на мелководье озера
// (LAKE_VEGETATION — плоский меш на y ≈ -3.3) издалека/под углом сортируется ПОВЕРХ прозрачной
// воды и выглядит «висящей» над поверхностью. Когда включён фикс «убрать водную ряску», компонент
// просто перестаёт рендерить этот меш, и озеро выглядит чисто.
//
// Только рендер и полностью обратимо: трогается лишь Renderer.enabled одного объекта — никаких
// коллайдеров, логики, физики или состояния сейва. Реагирует на настройку вживую. Живёт лёгким
// компонентом на контроллере MOPR.

using UnityEngine;
using MOPR.Common;

namespace MOPR.Fixes
{
    internal sealed class LakeVegetationFix : MonoBehaviour
    {
        private const float RetrySeconds = 2f;

        private Renderer vegetation;
        private float retryTimer;

        private void Update()
        {
            if (vegetation == null)
            {
                // Ищем объект (дёшево) пока он не появится, затем прекращаем поиск.
                retryTimer -= Time.unscaledDeltaTime;
                if (retryTimer > 0f)
                    return;

                retryTimer = RetrySeconds;

                GameObject found = GameObject.Find("LAKE_VEGETATION");
                if (found == null)
                    return;

                vegetation = found.GetComponent<Renderer>();
                if (vegetation == null)
                    return;
            }

            bool shouldRender = !MoprSettings.HideLakeVegetationOn;
            if (vegetation.enabled != shouldRender)
                vegetation.enabled = shouldRender;
        }
    }
}
