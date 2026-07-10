// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Отладочная выгрузка списков объектов под управлением MOPR (мир/транспорт/предметы/локации/
// сектора/Сатсума) в текстовые файлы папки MOPR_Lists. Включается настройкой generate-list.

using System.Collections.Generic;
using System.IO;
using UnityEngine;

using MOPR.Items;
using MOPR.Places;
using MOPR.Vehicles;
using MOPR.WorldObjects;

namespace MOPR.Common
{
    internal static class ToggledItemsListGenerator
    {
        private const string ListFolder = "MOPR_Lists";

        private static void Write(string filename, IEnumerable<string> names)
        {
            Directory.CreateDirectory(ListFolder);
            string path = Path.Combine(ListFolder, filename);
            File.WriteAllText(path, string.Join(", ", Distinct(names)));
        }

        private static string[] Distinct(IEnumerable<string> names)
        {
            HashSet<string> seen = new HashSet<string>();
            List<string> result = new List<string>();
            foreach (string name in names)
                if (seen.Add(name))
                    result.Add(name);
            return result.ToArray();
        }

        public static void CreateWorldList(List<GenericObject> list)
        {
            List<string> names = new List<string>();
            foreach (GenericObject obj in list)
                names.Add(obj.GetName());
            Write("world.txt", names);
        }

        public static void CreateVehicleList(List<Vehicle> list)
        {
            List<string> names = new List<string>();
            foreach (Vehicle obj in list)
                names.Add(obj.gameObject.name);
            Write("vehicle.txt", names);
        }

        public static void CreateItemsList(List<ItemBehaviour> list)
        {
            List<string> names = new List<string>();
            foreach (ItemBehaviour obj in list)
                names.Add(obj.gameObject.name);
            Write("items.txt", names);
        }

        public static void CreatePlacesList(List<Place> list)
        {
            List<string> names = new List<string>();
            foreach (Place obj in list)
                names.Add(obj.GetName());
            Write("places.txt", names);
        }

        public static void CreateSectorList(List<GameObject> list)
        {
            List<string> names = new List<string>();
            foreach (GameObject obj in list)
                names.Add(obj.name);
            Write("sector.txt", names);
        }

        public static void CreateSatsumaList(Transform[] list)
        {
            List<string> names = new List<string>();
            foreach (Transform obj in list)
                names.Add(obj.gameObject.name);
            Write("satsuma.txt", names);
        }

        public static void OpenFolder()
        {
            System.Diagnostics.Process.Start(ListFolder);
        }
    }
}
