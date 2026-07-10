// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Менеджер локаций: при создании инициализирует все управляемые места (двор, магазин, мастерская,
// техосмотр, ферма) и даёт перебор для цикла. Индекс 0 — Yard, 2 — RepairShop (на это опираются
// ядро и ItemBehaviour).

using System;
using System.Collections.Generic;

using MOPR.Places;
using MOPR.Common;
using MOPR.Common.Interfaces;

namespace MOPR.Managers
{
    internal class PlaceManager : IManager<Place>
    {
        private static PlaceManager instance;
        public static PlaceManager Instance => instance;

        public Place this[int index] => places[index];

        private readonly List<Place> places;

        public PlaceManager()
        {
            instance = this;

            try
            {
                places = new List<Place>
                {
                    new Yard(),
                    new Teimo(),
                    new RepairShop(),
                    new Inspection(),
                    new Farm()
                };

                ModConsole.Log("[MOPR] Places initialized");
            }
            catch (Exception ex)
            {
                ExceptionManager.New(ex, false, "PLACES_INITIALIZATION_FAILURE");
            }
        }

        public int Count => places.Count;

        public List<Place> GetAll => places;

        public Place Add(Place obj)
        {
            places.Add(obj);
            return obj;
        }

        public void Remove(Place obj)
        {
            if (places.Contains(obj))
                places.Remove(obj);
        }

        public void RemoveAt(int index) => places.RemoveAt(index);

        public int EnabledCount
        {
            get
            {
                int enabled = 0;
                foreach (Place place in places)
                    if (place.IsActive)
                        enabled++;
                return enabled;
            }
        }
    }
}
