// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Компонент-хук для поддержки слотов сохранений: при деактивации (переключении слота) запускает
// повторную проверку целостности сейва.

using MOPR.Helpers;
using UnityEngine;

namespace MOPR.Common
{
    internal class SaveSlotsSupport : MonoBehaviour
    {
        private void OnDisable()
        {
            SaveManager.VerifySave();
        }
    }
}
