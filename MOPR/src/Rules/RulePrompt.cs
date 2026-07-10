// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Превращает завершённую синхронизацию правил в выбор игрока: при наличии новых/обновлённых правил для
// установленных модов предлагает скачать сейчас или позже и показывает итог загрузки.

using System;
using System.Text;
using UnityEngine;
using MSCLoader;
using MOPR.Common;
using MOPR.Localization;
using MOPR.Interface.Helpers;

namespace MOPR.Rules
{
    internal static class RulePrompt
    {
        // Снимок ID модов на загрузку, снятый до очистки очереди, чтобы итоговое сообщение перечислило те же моды.
        private static string[] downloadingIds = new string[0];

        /// <summary>Вызывается в главном потоке после завершения синхронизации (загрузка меню или «Обновить»).</summary>
        public static void OnSyncCompleted()
        {
            try
            {
                if (RemoteRuleSync.PendingCount <= 0)
                    return; // нет ничего нового

                ModUI.ShowCustomMessage(
                    LocalizationCore.Get("rules.prompt_msg", RemoteRuleSync.PendingCount, BuildModList(RemoteRuleSync.PendingIds())),
                    LocalizationCore.Get("rules.prompt_title"),
                    new MsgBoxBtn[]
                    {
                        ModUI.CreateMessageBoxBtn(LocalizationCore.Get("rules.download_btn"), StartDownload),
                        ModUI.CreateMessageBoxBtn(LocalizationCore.Get("rules.later_btn"), ShowLater)
                    },
                    new MsgBoxBtn[] { });
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "RULE_PROMPT_ERROR");
            }
        }

        private static void StartDownload()
        {
            downloadingIds = RemoteRuleSync.PendingIds();
            RemoteRuleSync.DownloadPending();

            GameObject host = new GameObject("MOPR_RuleDownload");
            host.AddComponent<RuleDownloadWaiter>();
        }

        private static void ShowLater()
        {
            ModUI.ShowMessage(LocalizationCore.Get("rules.later_msg"), LocalizationCore.Get("rules.prompt_title"));
        }

        /// <summary>Собирает маркированный список имён модов в стиле инфо-панели.</summary>
        private static string BuildModList(string[] ids)
        {
            StringBuilder builder = new StringBuilder();
            foreach (string id in ids)
            {
                if (!string.IsNullOrEmpty(id))
                    builder.Append("• <color=#F5A623>").Append(id).Append("</color>\n");
            }

            return builder.ToString();
        }

        /// <summary>Вызывается в главном потоке после завершения подтверждённой загрузки.</summary>
        public static void OnDownloadFinished()
        {
            try
            {
                LocalizedUi.RefreshAll();
                ModConsole.Log("[MOPR] " + LocalizationCore.Get("log.rules_downloaded", RemoteRuleSync.Downloaded));
                ModUI.ShowMessage(
                    LocalizationCore.Get("rules.downloaded_msg", RemoteRuleSync.Downloaded, BuildModList(downloadingIds)),
                    LocalizationCore.Get("rules.prompt_title"));
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "RULE_DOWNLOAD_RESULT_ERROR");
            }
        }
    }
}
