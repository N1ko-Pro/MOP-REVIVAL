// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Запускает фоновую синхронизацию с сервером правил в главном меню, опрашивает флаг завершения и по
// готовности обновляет UI настроек, логирует итог и самоуничтожается.

using System.IO;
using System.Linq;
using UnityEngine;
using MSCLoader;
using MOPR.Localization;
using MOPR.Interface.Helpers;

namespace MOPR.Rules
{
    internal sealed class RuleSyncRunner : MonoBehaviour
    {
        private const float SafetyTimeoutSeconds = 30f;

        /// <summary>Каталог локальных файлов правил (&lt;config&gt;/Rules).</summary>
        public static string RulesFolder => Path.Combine(MOPR.ModConfigPath, "Rules");

        /// <summary>
        /// Создаёт объект-раннер и запускает синхронизацию. <paramref name="forceRefresh"/> сбрасывает
        /// прошлый результат (кнопка «Обновить»). Повторный запуск игнорируется, пока раннер жив.
        /// </summary>
        public static void Launch(bool forceRefresh)
        {
            if (forceRefresh)
                RemoteRuleSync.Reset();

            if (GameObject.Find("MOPR_RuleSync") != null)
                return;

            GameObject host = new GameObject("MOPR_RuleSync");
            RuleSyncRunner runner = host.AddComponent<RuleSyncRunner>();
            runner.RulesDirectory = RulesFolder;
            runner.ModIds = ModLoader.LoadedMods != null
                ? ModLoader.LoadedMods.Select(m => m.ID).ToArray()
                : new string[0];
        }

        public string RulesDirectory = string.Empty;
        public string[] ModIds = new string[0];

        /// <summary>Задержка перед началом проверки, чтобы строка инициализации залогировалась первой.</summary>
        public float StartDelay = 1f;

        private bool started;
        private bool done;
        private float elapsed;

        private void Update()
        {
            if (done)
                return;

            elapsed += Time.unscaledDeltaTime;

            if (!started)
            {
                if (elapsed < StartDelay)
                    return;

                started = true;
                elapsed = 0f;
                ModConsole.Log("[MOPR] " + LocalizationCore.Get("log.rules_sync_start"));
                RemoteRuleSync.BeginSync(RulesDirectory, ModIds);
                return;
            }

            if (RemoteRuleSync.Completed || elapsed >= SafetyTimeoutSeconds)
            {
                done = true;
                LocalizedUi.RefreshAll();
                LogOutcome();
                RulePrompt.OnSyncCompleted();
                Destroy(gameObject);
            }
        }

        private static void LogOutcome()
        {
            if (RemoteRuleSync.ManifestReached)
            {
                ModConsole.Log("[MOPR] " + LocalizationCore.Get("log.rules_sync_ok", RemoteRuleSync.ManifestCount, RemoteRuleSync.Matched, RemoteRuleSync.Downloaded));
                return;
            }

            ModConsole.LogWarning("[MOPR] " + LocalizationCore.Get("log.rules_sync_offline"));
            if (!string.IsNullOrEmpty(RemoteRuleSync.LastError))
                ModConsole.LogWarning("[MOPR] " + LocalizationCore.Get("log.rules_sync_error", RemoteRuleSync.LastError));
        }
    }
}
