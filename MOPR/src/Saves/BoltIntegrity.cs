// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Восстановление затяжки болтов по снимку. Ряд «поставил и забыл» креплений Сатсумы (бамперы,
// полуоси, минусовая клемма) страдают давним багом MSC: их затяжка самопроизвольно «уплывает»
// между сохранениями. При каждой загрузке снимаем известно-хороший снимок и на будущей загрузке
// предлагаем восстановить значения, которые перестали совпадать. Снимок привязан к версии мода,
// восстановление — только через подтверждение и бэкап, снимок обновляется лишь с разрешения игрока.

using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using MOPR.Common;
using MOPR.Localization;

namespace MOPR.Saves
{
    internal static class BoltIntegrity
    {
        private struct TrackedBolt
        {
            public readonly string InstalledTag;
            public readonly string TightnessTag;
            public readonly string BoltsTag;
            public readonly string NameKey;

            public TrackedBolt(string installed, string tightness, string bolts, string nameKey)
            {
                InstalledTag = installed;
                TightnessTag = tightness;
                BoltsTag = bolts;
                NameKey = nameKey;
            }
        }

        private static readonly TrackedBolt[] Tracked =
        {
            new TrackedBolt("bumper front(Clone)Installed", "Bumper_FrontTightness", "Bumper_FrontBolts", "bolt.bumper_front"),
            new TrackedBolt("bumper rear(Clone)Installed", "Bumper_RearTightness", "Bumper_RearBolts", "bolt.bumper_rear"),
            new TrackedBolt("halfshaft_flInstalled", "Halfshaft_FLTightness", "Halfshaft_FLBolts", "bolt.halfshaft_fl"),
            new TrackedBolt("halfshaft_frInstalled", "Halfshaft_FRTightness", "Halfshaft_FRBolts", "bolt.halfshaft_fr"),
            new TrackedBolt("battery_terminal_minus(xxxxx)Installed", "WiringBatteryMinusTightness", "WiringBatteryMinusBolts", "bolt.battery_minus"),
        };

        // Болты, помеченные «уплывшими» в текущем прогоне: их запись в снимке не перезаписывается
        // подозрительным текущим значением, пока игрок не разрешит вопрос.
        private static readonly List<string> flaggedThisRun = new List<string>();

        private static string SnapshotPath => Path.Combine(Application.persistentDataPath, "MOP_BoltSnapshot.json");

        /// <summary>
        /// Добавляет проблему-восстановление для каждого болта, чья текущая затяжка больше не совпадает
        /// со снимком, привязанным к версии. No-op, если фича выключена или снимка нет.
        /// </summary>
        public static void CollectIssues(List<SaveIssue> issues)
        {
            flaggedThisRun.Clear();

            if (!MoprSettings.RestoreBoltsOn)
                return;

            BoltSnapshot snapshot = ReadSnapshot();
            if (snapshot == null || snapshot.version != MOPR.ModVersion || snapshot.entries == null)
                return; // первый запуск или снимок другой версии

            foreach (TrackedBolt bolt in Tracked)
            {
                try
                {
                    BoltEntry entry = Find(snapshot, bolt.TightnessTag);
                    if (entry == null || entry.bolts == null || entry.tightness < 0f)
                        continue;

                    if (!SaveAccess.TryReadBool(bolt.InstalledTag, out bool installed) || !installed)
                        continue;

                    if (!SaveAccess.TryReadFloat(bolt.TightnessTag, out float liveTightness))
                        continue;

                    if (Mathf.Approximately(liveTightness, entry.tightness))
                        continue; // всё ещё совпадает

                    flaggedThisRun.Add(bolt.TightnessTag);

                    string name = LocalizationCore.Get(bolt.NameKey);
                    float restored = entry.tightness;
                    List<string> restoredBolts = entry.bolts;
                    string tightnessTag = bolt.TightnessTag;
                    string boltsTag = bolt.BoltsTag;

                    issues.Add(new SaveIssue(
                        LocalizationCore.Get("issue.bolt_tightness", name, liveTightness, restored),
                        () =>
                        {
                            SaveAccess.WriteSave(tightnessTag, restored);
                            SaveAccess.WriteSaveList(boltsTag, restoredBolts);
                        }));
                }
                catch (Exception ex)
                {
                    ExceptionManager.New(ex, false, "BOLT_INTEGRITY_CHECK_ERROR");
                }
            }
        }

        /// <summary>
        /// Обновляет снимок из свежезагруженного сейва. Болты, помеченные «уплывшими» в этом прогоне,
        /// сохраняют прежнее (известно-хорошее) значение, ЕСЛИ <paramref name="acceptFlaggedLive"/> не
        /// true (игрок отказался от восстановления и принял текущее значение).
        /// </summary>
        public static void Capture(bool acceptFlaggedLive)
        {
            if (!MoprSettings.RestoreBoltsOn)
                return;

            try
            {
                BoltSnapshot previous = ReadSnapshot();
                BoltSnapshot snapshot = new BoltSnapshot
                {
                    version = MOPR.ModVersion,
                    entries = new List<BoltEntry>(),
                };

                foreach (TrackedBolt bolt in Tracked)
                {
                    try
                    {
                        bool preserve = !acceptFlaggedLive && flaggedThisRun.Contains(bolt.TightnessTag);
                        if (preserve && previous != null && previous.version == MOPR.ModVersion)
                        {
                            BoltEntry old = Find(previous, bolt.TightnessTag);
                            if (old != null)
                            {
                                snapshot.entries.Add(old);
                                continue;
                            }
                        }

                        if (!SaveAccess.Exists(bolt.TightnessTag) || !SaveAccess.Exists(bolt.BoltsTag))
                            continue;

                        if (!SaveAccess.TryReadFloat(bolt.TightnessTag, out float tightness))
                            continue;

                        if (!SaveAccess.TryReadStringList(bolt.BoltsTag, out List<string> bolts) || bolts == null)
                            continue;

                        snapshot.entries.Add(new BoltEntry
                        {
                            tightnessTag = bolt.TightnessTag,
                            tightness = tightness,
                            bolts = bolts,
                        });
                    }
                    catch (Exception ex)
                    {
                        ExceptionManager.New(ex, false, "BOLT_SNAPSHOT_CAPTURE_PART_ERROR");
                    }
                }

                WriteSnapshot(snapshot);
                ModConsole.Log("[MOPR] " + LocalizationCore.Get("log.bolt_snapshot_saved", snapshot.entries.Count));
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "BOLT_SNAPSHOT_CAPTURE_ERROR");
            }
            finally
            {
                flaggedThisRun.Clear();
            }
        }

        private static BoltEntry Find(BoltSnapshot snapshot, string tightnessTag)
        {
            if (snapshot == null || snapshot.entries == null)
                return null;

            foreach (BoltEntry e in snapshot.entries)
                if (e != null && e.tightnessTag == tightnessTag)
                    return e;

            return null;
        }

        private static BoltSnapshot ReadSnapshot()
        {
            try
            {
                if (!File.Exists(SnapshotPath))
                    return null;

                return JsonConvert.DeserializeObject<BoltSnapshot>(File.ReadAllText(SnapshotPath));
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "BOLT_SNAPSHOT_READ_ERROR");
                return null;
            }
        }

        private static void WriteSnapshot(BoltSnapshot snapshot)
        {
            try
            {
                File.WriteAllText(SnapshotPath, JsonConvert.SerializeObject(snapshot));
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "BOLT_SNAPSHOT_WRITE_ERROR");
            }
        }
    }

    /// <summary>Сериализуемый снимок затяжки болтов (MOP_BoltSnapshot.json).</summary>
    internal sealed class BoltSnapshot
    {
        public string version;
        public List<BoltEntry> entries;
    }

    /// <summary>Известно-хорошая затяжка одного отслеживаемого болта и список стадий.</summary>
    internal sealed class BoltEntry
    {
        public string tightnessTag;
        public float tightness;
        public List<string> bolts;
    }
}
