// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Ждёт в главном потоке завершения подтверждённой фоновой загрузки правил (со страховочным таймаутом),
// сообщает результат через RulePrompt.OnDownloadFinished и самоуничтожается.

using UnityEngine;

namespace MOPR.Rules
{
    internal sealed class RuleDownloadWaiter : MonoBehaviour
    {
        private const float SafetyTimeoutSeconds = 30f;

        private bool done;
        private float elapsed;

        private void Update()
        {
            if (done)
                return;

            elapsed += Time.unscaledDeltaTime;
            if (RemoteRuleSync.DownloadCompleted || elapsed >= SafetyTimeoutSeconds)
            {
                done = true;
                RulePrompt.OnDownloadFinished();
                Destroy(gameObject);
            }
        }
    }
}
