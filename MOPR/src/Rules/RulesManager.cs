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
                string customPath = Path.Combine(MOPR.ModConfigPath, "Custom.txt");
                if (File.Exists(customPath))
                {
                    RuleParser.ParseFile(customPath, this);
                    fileCount++;
                }
            }
            catch (System.Exception ex)
            {
                ExceptionManager.New(ex, false, "RULES_LOAD_ERROR");
            }

            ModConsole.Log($"[MOPR] Rules loaded: {Rules.Count} rule(s) from {fileCount} file(s).");
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
