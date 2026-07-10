// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Проверка целостности сейва: читает загруженный сейв через ES2, ищет известные паттерны порчи и
// (после бэкапа) предлагает их исправить. Каждая проверка самодостаточна — сравнивает значения
// только внутри одного файла, поэтому исправление не может испортить верное значение. Проверки и
// фиксы изолированы: сбой одной не прерывает остальные и никогда не бросает исключение в игру.

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using MSCLoader;
using MOPR.Common;
using MOPR.Localization;

namespace MOPR.Saves
{
    internal static class SaveVerifier
    {
        private const int MaxListed = 4;
        private const float FlatbedMaxDistance = 5.5f;

        private static List<SaveIssue> issues;

        /// <summary>Запускает верификацию (когда сейв уже безопасно читать).</summary>
        public static void Verify()
        {
            try
            {
                bool verify = MoprSettings.SaveVerifyOn;
                bool bolts = MoprSettings.RestoreBoltsOn;
                if (!verify && !bolts)
                    return;

                if (!SaveAccess.SaveExists)
                    return;

                // У сейва после перма-смерти нет тега PlayerIsDead — проверять нечего.
                if (!SaveAccess.Exists("PlayerIsDead"))
                {
                    ModConsole.Log("[MOPR] " + LocalizationCore.Get("log.verify_skipped"));
                    return;
                }

                ModConsole.Log("[MOPR] " + LocalizationCore.Get("log.verify_start"));
                issues = new List<SaveIssue>();

                if (verify)
                {
                    CheckBucketSeats();
                    CheckFlatbedTrailer();
                    CheckFuelLineTightness();
                    CheckItemCounters();
                }

                BoltIntegrity.CollectIssues(issues);

                if (issues.Count == 0)
                {
                    // Ничего не «уплыло»: принимаем текущие значения как новый снимок.
                    BoltIntegrity.Capture(false);
                    ModConsole.Log("[MOPR] " + LocalizationCore.Get("log.verify_clean"));
                    return;
                }

                ModConsole.LogWarning("[MOPR] " + LocalizationCore.Get("log.verify_found", issues.Count));
                Prompt();
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "SAVE_VERIFY_ERROR");
            }
        }

        private static void CheckBucketSeats()
        {
            try
            {
                if (SaveAccess.TryReadBool("bucket seat passenger(Clone)Purchased", out bool passenger) &&
                    SaveAccess.TryReadBool("bucket seat driver(Clone)Purchased", out bool driver) &&
                    passenger != driver)
                {
                    issues.Add(new SaveIssue(LocalizationCore.Get("issue.bucket_seats"), () =>
                    {
                        SaveAccess.WriteSave("bucket seat passenger(Clone)Purchased", true);
                        SaveAccess.WriteSave("bucket seat driver(Clone)Purchased", true);
                    }));
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "VERIFY_BUCKET_SEATS_ERROR");
            }
        }

        private static void CheckFlatbedTrailer()
        {
            try
            {
                if (!SaveAccess.TryReadBool("TractorTrailerAttached", out bool attached) || !attached)
                    return;

                if (SaveAccess.TryReadTransform("FlatbedTransform", out Transform flatbed) &&
                    SaveAccess.TryReadTransform("TractorTransform", out Transform tractor) &&
                    flatbed != null && tractor != null &&
                    Vector3.Distance(flatbed.position, tractor.position) > FlatbedMaxDistance)
                {
                    issues.Add(new SaveIssue(LocalizationCore.Get("issue.flatbed"), () =>
                        SaveAccess.WriteSave("TractorTrailerAttached", false)));
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "VERIFY_FLATBED_ERROR");
            }
        }

        private static void CheckFuelLineTightness()
        {
            try
            {
                if (!SaveAccess.TryReadStringList("FuelLineBolts", out List<string> bolts) || bolts == null || bolts.Count == 0)
                    return;

                if (!SaveAccess.TryReadFloat("FuelLineTightness", out float tightness))
                    return;

                // Болты хранятся как "int(N)"; затяжка магистрали должна совпадать со значением болта.
                string raw = bolts[0].Replace("int(", string.Empty).Replace(")", string.Empty).Trim();
                if (!int.TryParse(raw, out int boltValue))
                    return;

                if (boltValue != tightness)
                {
                    issues.Add(new SaveIssue(LocalizationCore.Get("issue.fuel_line", tightness, boltValue), () =>
                        SaveAccess.WriteSave("FuelLineTightness", (float)boltValue)));
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "VERIFY_FUEL_LINE_ERROR");
            }
        }

        private static void CheckItemCounters()
        {
            foreach (KeyValuePair<string, string> entry in ItemCounterMap.Entries)
            {
                try
                {
                    string key = entry.Key;
                    string transformBase = entry.Value;

                    // Часть предметов сохраняется в варианте "namexID" вместо "nameID".
                    if (!SaveAccess.ItemExists(key))
                    {
                        string xKey = key.Split(new[] { "ID" }, StringSplitOptions.None)[0] + "xID";
                        if (!SaveAccess.ItemExists(xKey))
                            continue;

                        key = xKey;
                        transformBase += "x";
                    }

                    if (!SaveAccess.TryReadItemInt(key, out int savedCount))
                        continue;

                    int present = 1;
                    while (SaveAccess.ItemExists(transformBase + present + "Transform"))
                        present++;

                    if (present > 0)
                        present--;

                    if (savedCount < present)
                    {
                        string fixKey = key;
                        int fixValue = present;
                        issues.Add(new SaveIssue(LocalizationCore.Get("issue.item_count", key, savedCount, present), () =>
                            SaveAccess.WriteItem(fixKey, fixValue)));
                    }
                }
                catch (Exception ex)
                {
                    ExceptionManager.New(ex, false, "VERIFY_ITEM_COUNTER_ERROR");
                }
            }
        }

        private static void Prompt()
        {
            StringBuilder list = new StringBuilder();
            int shown = Math.Min(issues.Count, MaxListed);
            for (int i = 0; i < shown; i++)
                list.Append("• ").AppendLine(issues[i].Name);

            if (issues.Count > MaxListed)
                list.AppendLine(LocalizationCore.Get("save.verify_more", issues.Count - MaxListed));

            string message = LocalizationCore.Get("save.verify_found_msg", issues.Count, list.ToString());

            try
            {
                ModUI.ShowCustomMessage(
                    message,
                    LocalizationCore.Get("save.verify_title"),
                    new[]
                    {
                        ModUI.CreateMessageBoxBtn(LocalizationCore.Get("common.yes"), FixAll),
                        ModUI.CreateMessageBoxBtn(LocalizationCore.Get("common.no"), Decline)
                    },
                    new MsgBoxBtn[] { });
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "VERIFY_PROMPT_ERROR");
            }
        }

        /// <summary>
        /// Игрок отказался от исправлений: принимаем текущие значения как новый снимок болтов, чтобы
        /// MOP не предлагал снова откатывать значение, которое игрок решил оставить.
        /// </summary>
        private static void Decline()
        {
            BoltIntegrity.Capture(true);
        }

        private static void FixAll()
        {
            SaveProtection.Backup();

            int fixedCount = 0;
            int failedCount = 0;
            foreach (SaveIssue issue in issues)
            {
                try
                {
                    issue.Fix();
                    fixedCount++;
                }
                catch (Exception ex)
                {
                    failedCount++;
                    ExceptionManager.New(ex, false, "SAVE_FIX_ERROR");
                }
            }

            ModConsole.Log("[MOPR] " + LocalizationCore.Get("log.verify_done", fixedCount, failedCount));

            // Фиксы записали известно-хорошие значения; снимок уже держит их — фиксируем.
            BoltIntegrity.Capture(false);

            try
            {
                ModUI.ShowMessage(LocalizationCore.Get("save.verify_result", fixedCount, failedCount), LocalizationCore.Get("save.verify_title"));
            }
            catch
            {
            }
        }
    }
}
