// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Активный набор правил совместимости: парсит локальные .mopconfig из <config>/Rules/ и Custom.txt,
// хранит разобранные правила с особыми флагами и даёт типизированный доступ потребителям.

using System.Collections.Generic;
using System.IO;
using UnityEngine;
using MSCLoader;
using MOPR.Common;
using MOPR.Rules.Types;

namespace MOPR.Rules
{
    internal sealed class RulesManager
    {
        public static RulesManager Instance { get; private set; }

        public List<Rule> Rules { get; private set; }
        public SpecialRules SpecialRules;

        private RulesManager()
        {
            Rules = new List<Rule>();
            SpecialRules = new SpecialRules();
        }

        /// <summary>Пересоздаёт активный набор и парсит текущие локальные файлы правил.</summary>
        public static void LoadActive()
        {
            Instance = new RulesManager();
            Instance.LoadAll();
        }

        private void LoadAll()
        {
            int fileCount = 0;
            bool customFound = false;
            string customPath = Path.Combine(MOPR.ModConfigPath, "Custom.txt");

            try
            {
                string rulesDir = RuleSyncRunner.RulesFolder;
                if (Directory.Exists(rulesDir))
                {
                    foreach (string filePath in Directory.GetFiles(rulesDir, "*.mopconfig"))
                    {
                        RuleParser.ParseFile(filePath, this);
                        fileCount++;
                    }
                }

                // Пользовательские правила игрока (не с сервера).
                if (File.Exists(customPath))
                {
                    RuleParser.ParseFile(customPath, this);
                    fileCount++;
                    customFound = true;
                }
            }
            catch (System.Exception ex)
            {
                ExceptionManager.New(ex, false, "RULES_LOAD_ERROR");
            }

            // Всегда выводим итог загрузки правил (LogAlways), чтобы диагностика совместимости не
            // зависела от настройки «показывать лог». Явно сообщаем судьбу Custom.txt и активные флаги.
            ModConsole.LogAlways($"[MOPR] Rules loaded: {Rules.Count} rule(s) from {fileCount} file(s).");
            ModConsole.LogAlways($"[MOPR] Custom.txt: {(customFound ? "loaded" : "not found")} ({customPath})");
            ModConsole.LogAlways($"[MOPR] Active rule flags: {DescribeActiveFlags()}");
        }

        /// <summary>Список активных особых флагов правил (для диагностики: лог загрузки и «mopr status»).</summary>
        public string DescribeActiveFlags()
        {
            List<string> flags = new List<string>();
            if (SpecialRules.SatsumaIgnoreRenderers) flags.Add("satsuma_ignore_renderer");
            if (SpecialRules.SatsumaIgnore) flags.Add("satsuma_ignore");
            if (SpecialRules.DontDestroyEmptyBeerBottles) flags.Add("dont_destroy_empty_bottles");
            if (SpecialRules.SkipFuryColliderFix) flags.Add("skip_fury_collider_fix");
            if (SpecialRules.IgnoreModVehicles) flags.Add("ignore_mod_vehicles");
            if (SpecialRules.ToggleAllVehiclesPhysicsOnly) flags.Add("toggle_all_vehicles_physics_only");
            if (SpecialRules.NoLods) flags.Add("no_lods");
            return flags.Count > 0 ? string.Join(", ", flags.ToArray()) : "(none)";
        }

        public void AddRule(Rule rule)
        {
            Rules.Add(rule);
        }

        public List<Rule> GetList()
        {
            return Rules;
        }

        public List<T> GetList<T>() where T : class
        {
            List<T> result = new List<T>();
            foreach (Rule rule in Rules)
            {
                if (rule is T typed)
                    result.Add(typed);
            }

            return result;
        }

        public bool IsObjectInIgnoreList(GameObject gm)
        {
            if (gm == null)
                return false;

            foreach (Rule rule in Rules)
            {
                if (rule is IgnoreRule ignore && ignore.ObjectName == gm.name)
                    return true;
            }

            return false;
        }
    }
}
