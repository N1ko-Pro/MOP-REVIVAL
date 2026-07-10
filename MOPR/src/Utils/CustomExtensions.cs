// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Набор методов-расширений: удобный поиск по строкам, обход иерархии Transform, эмуляция
// Enum.HasFlag (в .NET 3.5 его нет) и построение полного пути объекта в сцене.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MOPR.Common
{
    internal static class CustomExtensions
    {
        /// <summary>Содержит ли строка хотя бы одну из переданных подстрок.</summary>
        public static bool ContainsAny(this string source, params string[] values)
        {
            for (int i = 0; i < values.Length; i++)
                if (source.Contains(values[i]))
                    return true;
            return false;
        }

        /// <summary>Содержит ли строка хотя бы одну из подстрок списка.</summary>
        public static bool ContainsAny(this string source, List<string> values)
        {
            for (int i = 0; i < values.Count; i++)
                if (source.Contains(values[i]))
                    return true;
            return false;
        }

        /// <summary>Содержит ли массив строк хотя бы один из переданных элементов.</summary>
        public static bool ContainsAny(this string[] source, params string[] values)
        {
            for (int i = 0; i < values.Length; i++)
                if (source.Contains(values[i]))
                    return true;
            return false;
        }

        /// <summary>Равна ли строка хотя бы одному из переданных значений.</summary>
        public static bool EqualsAny(this string source, params string[] values)
        {
            for (int i = 0; i < values.Length; i++)
                if (source == values[i])
                    return true;
            return false;
        }

        /// <summary>Ищет компонент объекта по имени его типа.</summary>
        public static Component GetComponentByName(this GameObject gameObject, string typeName)
        {
            Component[] components = gameObject.GetComponents(typeof(Component));
            for (int i = 0; i < components.Length; i++)
                if (components[i].GetType().Name == typeName)
                    return components[i];
            return null;
        }

        /// <summary>Рекурсивно ищет дочерний Transform по имени.</summary>
        public static Transform FindRecursive(this Transform root, string name)
        {
            foreach (Transform child in root.GetComponentsInChildren<Transform>())
                if (child.name == name)
                    return child;
            return null;
        }

        /// <summary>Замена Enum.HasFlag, отсутствующего в .NET 3.5.</summary>
        public static bool HasFlag(this Enum variable, Enum value)
        {
            if (variable.GetType() != value.GetType())
                throw new ArgumentException("The checked flag is not from the same type as the checked variable.");

            ulong flag = Convert.ToUInt64(value);
            ulong self = Convert.ToUInt64(variable);
            return (self & flag) == flag;
        }

        /// <summary>Полный путь объекта в иерархии сцены (Root/Child/.../Name).</summary>
        public static string Path(this Transform transform)
        {
            if (transform == null)
                return "null";

            string path = transform.name;
            while (transform.parent != null)
            {
                transform = transform.parent;
                path = transform.name + "/" + path;
            }
            return path;
        }

        /// <summary>Полный путь объекта в иерархии сцены.</summary>
        public static string Path(this GameObject gameObject)
        {
            return gameObject == null ? "null" : Path(gameObject.transform);
        }
    }
}
