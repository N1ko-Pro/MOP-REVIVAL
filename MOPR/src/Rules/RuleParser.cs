// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Парсер файлов правил .mopconfig (формат оригинального MOP): одна директива на строку, «#» — комментарий,
// %20 — пробел; ведущий min_ver гейтит весь файл по версии мода, разобранное добавляется в RulesManager.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;
using MSCLoader;
using MOPR.Common;
using MOPR.Common.Enumerations;
using MOPR.Rules.Types;

namespace MOPR.Rules
{
    internal static class RuleParser
    {
        public static void ParseFile(string path, RulesManager manager)
        {
            try
            {
                string sourceMod = Path.GetFileNameWithoutExtension(path);
                string[] lines = File.ReadAllLines(path);

                foreach (string rawLine in lines)
                {
                    try
                    {
                        ParseLine(rawLine, sourceMod, manager);
                    }
                    catch (Exception ex)
                    {
                        ExceptionManager.New(ex, false, $"RULE_PARSE_LINE_{sourceMod}");
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "RULE_PARSE_FILE_ERROR");
            }
        }

        private static void ParseLine(string raw, string sourceMod, RulesManager manager)
        {
            string line = raw == null ? string.Empty : raw.Trim();
            if (line.Length == 0 || line.StartsWith("#"))
                return;

            string directive;
            string value;
            int colonIndex = line.IndexOf(':');
            if (colonIndex >= 0)
            {
                directive = line.Substring(0, colonIndex).Trim().ToLowerInvariant();
                value = line.Substring(colonIndex + 1).Trim();
            }
            else
            {
                directive = line.ToLowerInvariant();
                value = string.Empty;
            }

            switch (directive)
            {
                case "min_ver":
                    // Гейт всего файла: если версия мода ниже требуемой — правила файла игнорируются.
                    if (!IsVersionAtLeast(MOPR.ModVersionShort, value))
                        throw new Exception($"Rule file '{sourceMod}' requires MOP {value}; skipped.");
                    break;

                case "ignore":
                    ParseIgnore(value, sourceMod, manager);
                    break;

                case "toggle":
                    ParseToggle(value, sourceMod, manager);
                    break;

                case "change_parent":
                    ParseChangeParent(value, sourceMod, manager);
                    break;

                case "sector":
                    ParseSector(value, sourceMod, manager);
                    break;

                case "no_lod":
                    if (value.Length > 0)
                        manager.AddRule(new NoLod { SourceMod = sourceMod, ObjectName = Unescape(value) });
                    break;

                // Особые флаги на весь сейв.
                case "satsuma_ignore_renderer":
                    manager.SpecialRules.SatsumaIgnoreRenderers = true;
                    break;
                case "satsuma_ignore":
                    // Полный отказ от оптимизации Сатсумы — для модов, целиком переделывающих её
                    // (напр. Satsuma LX), где наше вмешательство валит игру при сохранении.
                    manager.SpecialRules.SatsumaIgnore = true;
                    break;
                case "dont_destroy_empty_bottles":
                    manager.SpecialRules.DontDestroyEmptyBeerBottles = true;
                    break;
                case "skip_fury_collider_fix":
                    manager.SpecialRules.SkipFuryColliderFix = true;
                    break;
                case "ignore_mod_vehicles":
                    manager.SpecialRules.IgnoreModVehicles = true;
                    break;
                case "toggle_all_vehicles_physics_only":
                    manager.SpecialRules.ToggleAllVehiclesPhysicsOnly = true;
                    break;
                case "no_lods":
                    manager.SpecialRules.NoLods = true;
                    break;

                default:
                    ModConsole.Log($"[MOPR] Rule '{sourceMod}': unknown directive \"{directive}\" (ignored).");
                    break;
            }
        }

        private static void ParseIgnore(string value, string sourceMod, RulesManager manager)
        {
            string[] tokens = Tokenize(value);
            if (tokens.Length == 0)
                return;

            if (tokens.Length == 1)
            {
                manager.AddRule(new IgnoreRule { SourceMod = sourceMod, ObjectName = tokens[0], TotalIgnore = false });
                return;
            }

            // Второй токен "fullignore" → полное игнорирование объекта, иначе первый токен — локация.
            if (tokens[tokens.Length - 1].Equals("fullignore", StringComparison.OrdinalIgnoreCase))
            {
                if (tokens.Length == 2)
                    manager.AddRule(new IgnoreRule { SourceMod = sourceMod, ObjectName = tokens[0], TotalIgnore = true });
                else
                    manager.AddRule(new IgnoreRuleAtPlace { SourceMod = sourceMod, Place = tokens[0], ObjectName = tokens[1] });
                return;
            }

            manager.AddRule(new IgnoreRuleAtPlace { SourceMod = sourceMod, Place = tokens[0], ObjectName = tokens[1] });
        }

        private static void ParseToggle(string value, string sourceMod, RulesManager manager)
        {
            string[] tokens = Tokenize(value);
            if (tokens.Length == 0)
                return;

            ToggleModes mode = ToggleModes.Simple;
            if (tokens.Length > 1)
            {
                switch (tokens[1].ToLowerInvariant())
                {
                    case "renderer": mode = ToggleModes.Renderer; break;
                    case "multitoggle": mode = ToggleModes.MultipleRenderers; break;
                    case "item": mode = ToggleModes.Item; break;
                    case "vehicle": mode = ToggleModes.Vehicle; break;
                    case "vehicle_physics": mode = ToggleModes.VehiclePhysics; break;
                    default: mode = ToggleModes.Simple; break;
                }
            }

            manager.AddRule(new ToggleRule { SourceMod = sourceMod, ObjectName = tokens[0], ToggleMode = mode });
        }

        private static void ParseChangeParent(string value, string sourceMod, RulesManager manager)
        {
            string[] tokens = Tokenize(value);
            if (tokens.Length < 2)
                return;

            manager.AddRule(new ChangeParentRule { SourceMod = sourceMod, ObjectName = tokens[0], NewParentName = tokens[1] });
        }

        private static void ParseSector(string value, string sourceMod, RulesManager manager)
        {
            string[] tokens = Tokenize(value);
            if (tokens.Length < 3)
                return;

            Vector3 position = ParseVector(tokens[0]);
            Vector3 scale = ParseVector(tokens[1]);
            Vector3 rotation = ParseVector(tokens[2]);

            List<string> whitelist = new List<string>();
            for (int i = 3; i < tokens.Length; i++)
                whitelist.Add(tokens[i]);

            manager.AddRule(new NewSector
            {
                SourceMod = sourceMod,
                Position = position,
                Scale = scale,
                Rotation = rotation,
                Whitelist = whitelist.ToArray(),
            });
        }

        /// <summary>Разбивает по пробелам и заменяет %20 на пробел в каждом токене.</summary>
        private static string[] Tokenize(string value)
        {
            if (string.IsNullOrEmpty(value))
                return new string[0];

            string[] parts = value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < parts.Length; i++)
                parts[i] = Unescape(parts[i]);

            return parts;
        }

        private static string Unescape(string token)
        {
            return token.Replace("%20", " ");
        }

        private static Vector3 ParseVector(string token)
        {
            string[] components = token.Split(',');
            float x = components.Length > 0 ? ParseFloat(components[0]) : 0f;
            float y = components.Length > 1 ? ParseFloat(components[1]) : 0f;
            float z = components.Length > 2 ? ParseFloat(components[2]) : 0f;
            return new Vector3(x, y, z);
        }

        private static float ParseFloat(string s)
        {
            float.TryParse(s.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float value);
            return value;
        }

        /// <summary>Сравнивает версии покомпонентно: true, если current >= required.</summary>
        private static bool IsVersionAtLeast(string current, string required)
        {
            try
            {
                int[] cur = ParseVersion(current);
                int[] req = ParseVersion(required);
                int len = Math.Max(cur.Length, req.Length);
                for (int i = 0; i < len; i++)
                {
                    int a = i < cur.Length ? cur[i] : 0;
                    int b = i < req.Length ? req[i] : 0;
                    if (a != b)
                        return a > b;
                }

                return true; // равны
            }
            catch
            {
                return true; // не смогли распарсить — не гейтим
            }
        }

        private static int[] ParseVersion(string version)
        {
            string[] parts = version.Trim().Split('.');
            List<int> numbers = new List<int>();
            foreach (string part in parts)
            {
                string digits = new string(part.Where(char.IsDigit).ToArray());
                int.TryParse(digits, out int n);
                numbers.Add(n);
            }

            return numbers.ToArray();
        }
    }
}
