// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Обновляет локальные файлы правил с сервера MOP Revival в фоновом потоке через TlsHttpClient,
// тихо откатываясь на локальные файлы; качает только совпавшие с модами правила с изменённым sha256.

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

namespace MOPR.Rules
{
    internal static class RemoteRuleSync
    {
        /// <summary>Публичный адрес сайта / сервера правил (также открывается кнопкой в настройках).</summary>
        public const string SiteUrl = "https://mop-revival.vercel.app";

        private const string BaseUrl = SiteUrl;
        private const string ManifestUrl = BaseUrl + "/manifest.json";
        private const int TimeoutMs = 15000;

        private static bool started;

        // Читаются главным потоком для статуса/лога после завершения фонового потока.
        public static volatile bool Completed;
        public static volatile bool ManifestReached;
        public static volatile int ManifestCount;
        public static volatile int Matched;
        public static volatile int Downloaded;

        // Новые/обновлённые правила, совпадающие с установленными модами, но которых ещё нет на диске.
        // Обнаружены синхронизацией, но НЕ качаются автоматически — сначала спрашиваем игрока.
        public static volatile int PendingCount;

        // Флаги фазы загрузки (после согласия игрока скачать ожидающие правила).
        public static volatile bool DownloadStarted;
        public static volatile bool DownloadCompleted;

        private static readonly List<RuleEntry> pending = new List<RuleEntry>();
        private static string pendingRulesDir = string.Empty;

        /// <summary>Последняя причина сбоя (для диагностики); пусто при успехе.</summary>
        public static volatile string LastError = string.Empty;

        /// <summary>
        /// Ключ локализации статуса сервера правил для панели настроек: «проверяем» пока идёт
        /// синхронизация, затем «доступен»/«недоступен» по результату.
        /// </summary>
        public static string StatusKey
        {
            get
            {
                if (!Completed)
                    return "settings.server.checking";

                return ManifestReached ? "settings.server.available" : "settings.server.offline";
            }
        }

        public static void BeginSync(string rulesDirectory, string[] installedModIds)
        {
            if (started)
                return;

            started = true;

            Thread worker = new Thread(() => Run(rulesDirectory, installedModIds));
            worker.IsBackground = true;
            worker.Start();
        }

        /// <summary>Сбрасывает состояние результата, чтобы запустить свежую синхронизацию (кнопка «Обновить»).</summary>
        public static void Reset()
        {
            started = false;
            Completed = false;
            ManifestReached = false;
            ManifestCount = 0;
            Matched = 0;
            Downloaded = 0;
            PendingCount = 0;
            DownloadStarted = false;
            DownloadCompleted = false;
            lock (pending)
            {
                pending.Clear();
            }
            LastError = string.Empty;
        }

        /// <summary>
        /// Снимок ID модов, чьи правила новые/обновлённые и ждут подтверждённой загрузки.
        /// Безопасно читать в главном потоке после завершения синхронизации.
        /// </summary>
        public static string[] PendingIds()
        {
            lock (pending)
            {
                string[] ids = new string[pending.Count];
                for (int i = 0; i < pending.Count; i++)
                    ids[i] = pending[i] != null ? pending[i].Id : string.Empty;

                return ids;
            }
        }

        private static void Run(string rulesDirectory, string[] installedModIds)
        {
            try
            {
                if (!Directory.Exists(rulesDirectory))
                    Directory.CreateDirectory(rulesDirectory);

                string manifestJson = TlsHttpClient.Get(ManifestUrl, TimeoutMs, out string manifestError);
                if (manifestJson == null)
                {
                    LastError = "manifest: " + manifestError;
                    return;
                }

                Manifest manifest;
                try
                {
                    manifest = JsonConvert.DeserializeObject<Manifest>(manifestJson);
                }
                catch (Exception ex)
                {
                    LastError = "parse: " + ex.Message;
                    return;
                }

                if (manifest == null || manifest.Rules == null)
                {
                    LastError = "parse: manifest had no rules";
                    return;
                }

                // Сервер действительно доступен и понят на этом этапе.
                ManifestReached = true;
                ManifestCount = manifest.Rules.Length;

                HashSet<string> installed = new HashSet<string>(installedModIds, StringComparer.Ordinal);
                int matchedCount = 0;

                lock (pending)
                {
                    pending.Clear();
                }

                pendingRulesDir = rulesDirectory;

                foreach (RuleEntry rule in manifest.Rules)
                {
                    if (rule == null || string.IsNullOrEmpty(rule.Id) || string.IsNullOrEmpty(rule.Path))
                        continue;

                    if (!installed.Contains(rule.Id))
                        continue;

                    matchedCount++; // правило для одного из установленных модов игрока

                    string localPath = Path.Combine(rulesDirectory, rule.Id + ".mopconfig");
                    if (File.Exists(localPath) &&
                        string.Equals(Sha256File(localPath), rule.Sha256, StringComparison.OrdinalIgnoreCase))
                    {
                        continue; // уже актуально
                    }

                    // Новое или обновлённое: в очередь на подтверждённую загрузку, не качаем сейчас.
                    lock (pending)
                    {
                        pending.Add(rule);
                    }
                }

                Matched = matchedCount;
                lock (pending)
                {
                    PendingCount = pending.Count;
                }
            }
            catch (Exception ex)
            {
                LastError = ex.GetType().Name + ": " + ex.Message;
            }
            finally
            {
                Completed = true;
            }
        }

        /// <summary>
        /// Качает правила, поставленные в очередь (<see cref="PendingCount"/>) последней синхронизацией,
        /// в фоновом потоке (TLS нельзя в главном потоке игры). Вызывать один раз после согласия игрока;
        /// результат опрашивать через <see cref="DownloadCompleted"/>.
        /// </summary>
        public static void DownloadPending()
        {
            if (DownloadStarted)
                return;

            DownloadStarted = true;
            DownloadCompleted = false;

            Thread worker = new Thread(RunDownload);
            worker.IsBackground = true;
            worker.Start();
        }

        private static void RunDownload()
        {
            int downloadedCount = 0;
            try
            {
                RuleEntry[] toGet;
                lock (pending)
                {
                    toGet = pending.ToArray();
                }

                foreach (RuleEntry rule in toGet)
                {
                    if (rule == null || string.IsNullOrEmpty(rule.Id) || string.IsNullOrEmpty(rule.Path))
                        continue;

                    string content = TlsHttpClient.Get(BaseUrl + "/" + rule.Path, TimeoutMs, out string _);
                    if (content == null)
                        continue; // сохраняем существующую локальную копию

                    try
                    {
                        File.WriteAllText(Path.Combine(pendingRulesDir, rule.Id + ".mopconfig"), content);
                        downloadedCount++;
                    }
                    catch
                    {
                    }
                }

                lock (pending)
                {
                    pending.Clear();
                }
            }
            catch (Exception ex)
            {
                LastError = ex.GetType().Name + ": " + ex.Message;
            }
            finally
            {
                Downloaded = downloadedCount;
                PendingCount = 0;
                DownloadCompleted = true;
            }
        }

        private static string Sha256File(string path)
        {
            using (SHA256 sha = SHA256.Create())
            using (FileStream stream = File.OpenRead(path))
            {
                byte[] hash = sha.ComputeHash(stream);
                StringBuilder builder = new StringBuilder(hash.Length * 2);
                for (int i = 0; i < hash.Length; i++)
                    builder.Append(hash[i].ToString("x2"));

                return builder.ToString();
            }
        }

        private sealed class Manifest
        {
            [JsonProperty("rules")]
            public RuleEntry[] Rules { get; set; }
        }

        private sealed class RuleEntry
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("path")]
            public string Path { get; set; }

            [JsonProperty("sha256")]
            public string Sha256 { get; set; }
        }
    }
}
