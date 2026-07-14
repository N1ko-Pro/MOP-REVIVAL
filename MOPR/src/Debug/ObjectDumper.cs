// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Отладочная выгрузка полной иерархии конкретного объекта сцены в текстовый файл. Для каждого узла
// пишет имя и ключевые компоненты (Rigidbody/Joint/Collider/Renderer/PlayMakerFSM), а также пометки
// MOPR: забрал ли предмет под ItemBehaviour, взял бы ли его сканер как предмет, и есть ли ignore-
// правило. Нужна для написания точных правил совместимости: показывает реальные рантайм-имена
// объектов мода и что именно MOPR у них трогает. Вызывается командой «mopr dump <ObjectName>».

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using HutongGames.PlayMaker;
using MSCLoader;

using MOPR.Items;
using MOPR.Rules;

namespace MOPR.Common
{
    internal static class ObjectDumper
    {
        private const string DumpFolder = "MOPR_Lists";
        private const int MaxNodes = 20000;
        private const int MaxDepth = 60;

        /// <summary>Находит объект по имени (в т.ч. неактивный) и выгружает его поддерево в файл.</summary>
        public static void Dump(string name)
        {
            GameObject root = FindObject(name);
            if (root == null)
            {
                ModConsole.LogAlways("[MOPR] dump: object \"" + name + "\" not found in the scene.");
                return;
            }

            StringBuilder sb = new StringBuilder(8192);
            List<string> managedItems = new List<string>();
            List<string> vehicleRoots = new List<string>();
            int[] counters = { 0 }; // [0] — сколько узлов записано.

            sb.Append("[MOPR] Object dump: ").AppendLine(root.name);
            sb.Append("Full path: ").AppendLine(GetPath(root.transform));
            sb.Append("Generated: ").AppendLine(System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            sb.AppendLine("Tags: INACTIVE, RB / RB(kinematic), Joint:<type>, Collider, Renderer, FSM:<name>,");
            sb.AppendLine("      <MOP-ItemBehaviour> = MOPR manages it as an item RIGHT NOW,");
            sb.AppendLine("      <MOP-would-grab> = matches MOPR item whitelist, <ignored-by-rule> = an ignore rule hit,");
            sb.AppendLine("      CarDynamics/Axles/Drivetrain = UnityCar parts; a node with all three is a valid");
            sb.AppendLine("      target for `toggle: <name> vehicle` / `vehicle_physics`.");
            sb.AppendLine(new string('-', 70));

            AppendNode(sb, root.transform, 0, counters, managedItems, vehicleRoots);

            sb.AppendLine();
            sb.AppendLine(new string('=', 70));
            sb.Append("Nodes MOPR touches / would touch as items (").Append(managedItems.Count).AppendLine("):");
            if (managedItems.Count == 0)
                sb.AppendLine("  (none)");
            else
                foreach (string line in managedItems)
                    sb.Append("  ").AppendLine(line);

            sb.AppendLine();
            sb.Append("Vehicle candidates (UnityCar root -> valid `toggle: <name> vehicle[_physics]`) (")
              .Append(vehicleRoots.Count).AppendLine("):");
            if (vehicleRoots.Count == 0)
                sb.AppendLine("  (none)");
            else
                foreach (string line in vehicleRoots)
                    sb.Append("  ").AppendLine(line);

            sb.AppendLine();
            sb.Append("Total transforms written: ").Append(counters[0]);
            if (counters[0] >= MaxNodes)
                sb.Append(" (truncated at ").Append(MaxNodes).Append(")");
            sb.AppendLine();

            string path = WriteFile(root.name, sb.ToString());
            if (path == null)
                return;

            ModConsole.LogAlways("[MOPR] Dumped \"" + root.name + "\" (" + counters[0] + " transforms, " +
                managedItems.Count + " item-managed) to:\n" + path);
            TryOpenFolder();
        }

        /// <summary>Активный поиск, затем перебор всех объектов (ловит неактивные экземпляры сцены).</summary>
        private static GameObject FindObject(string name)
        {
            GameObject go = GameObject.Find(name);
            if (go != null)
                return go;

            // Фолбэк ловит неактивные экземпляры (как в Vehicle). Ищем по точному имени;
            // имя вида "SVOBODA(855kg)" не совпадёт с префабом-ассетом, так что берём объект сцены.
            return Resources.FindObjectsOfTypeAll<GameObject>()
                .FirstOrDefault(g => g.name == name);
        }

        private static void AppendNode(StringBuilder sb, Transform t, int depth, int[] counters, List<string> managedItems, List<string> vehicleRoots)
        {
            if (counters[0] >= MaxNodes || depth > MaxDepth)
                return;

            counters[0]++;
            sb.Append(new string(' ', depth * 2)).Append(t.gameObject.name).Append(Tags(t, managedItems, vehicleRoots)).AppendLine();

            foreach (Transform child in t)
                AppendNode(sb, child, depth + 1, counters, managedItems, vehicleRoots);
        }

        /// <summary>Собирает компонентные пометки узла и попутно копит списки «предметных» узлов и вехикл-кандидатов.</summary>
        private static string Tags(Transform t, List<string> managedItems, List<string> vehicleRoots)
        {
            List<string> parts = new List<string>();
            GameObject go = t.gameObject;

            if (!go.activeSelf)
                parts.Add("INACTIVE");

            Rigidbody rb = t.GetComponent<Rigidbody>();
            if (rb != null)
                parts.Add(rb.isKinematic ? "RB(kinematic)" : "RB");

            Joint joint = t.GetComponent<Joint>();
            if (joint != null)
                parts.Add("Joint:" + joint.GetType().Name);

            if (t.GetComponent<Collider>() != null)
                parts.Add("Collider");

            if (t.GetComponent<Renderer>() != null)
                parts.Add("Renderer");

            // UnityCar-стек: узел, где есть все три, годится под `toggle: <name> vehicle[_physics]`.
            bool hasCarDynamics = t.GetComponent<CarDynamics>() != null;
            bool hasAxles = t.GetComponent<Axles>() != null;
            bool hasDrivetrain = t.GetComponent<Drivetrain>() != null;
            if (hasCarDynamics) parts.Add("CarDynamics");
            if (hasAxles) parts.Add("Axles");
            if (hasDrivetrain) parts.Add("Drivetrain");
            if (hasCarDynamics && hasAxles && hasDrivetrain)
                vehicleRoots.Add(go.name + (t.GetComponent<Rigidbody>() != null ? "" : "  (WARNING: no Rigidbody)"));

            foreach (PlayMakerFSM fsm in t.GetComponents<PlayMakerFSM>())
                parts.Add("FSM:" + fsm.FsmName);

            bool hasItemBehaviour = t.GetComponent<ItemBehaviour>() != null;
            if (hasItemBehaviour)
                parts.Add("<MOP-ItemBehaviour>");

            bool wouldGrab = ItemNameList.IsItemName(go.name);
            if (wouldGrab)
                parts.Add("<MOP-would-grab>");

            bool ignored = RulesManager.Instance != null && RulesManager.Instance.IsObjectInIgnoreList(go);
            if (ignored)
                parts.Add("<ignored-by-rule>");

            if (hasItemBehaviour || wouldGrab)
                managedItems.Add(go.name + (hasItemBehaviour ? "  (has ItemBehaviour)" : "  (matches whitelist)") + (ignored ? "  (ignored)" : ""));

            return parts.Count > 0 ? "   [" + string.Join(", ", parts.ToArray()) + "]" : "";
        }

        private static string GetPath(Transform t)
        {
            string path = t.name;
            Transform current = t.parent;
            while (current != null)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }

            return path;
        }

        private static string WriteFile(string objectName, string contents)
        {
            try
            {
                Directory.CreateDirectory(DumpFolder);
                string safe = Sanitize(objectName);
                string path = Path.GetFullPath(Path.Combine(DumpFolder, "dump_" + safe + ".txt"));
                File.WriteAllText(path, contents);
                return path;
            }
            catch (System.Exception ex)
            {
                ExceptionManager.New(ex, false, "OBJECT_DUMP_WRITE_ERROR");
                return null;
            }
        }

        private static string Sanitize(string name)
        {
            StringBuilder sb = new StringBuilder(name.Length);
            char[] invalid = Path.GetInvalidFileNameChars();
            foreach (char c in name)
                sb.Append(System.Array.IndexOf(invalid, c) >= 0 ? '_' : c);
            return sb.ToString();
        }

        private static void TryOpenFolder()
        {
            try
            {
                System.Diagnostics.Process.Start(DumpFolder);
            }
            catch (System.Exception ex)
            {
                ExceptionManager.New(ex, false, "OBJECT_DUMP_OPEN_ERROR");
            }
        }
    }
}
