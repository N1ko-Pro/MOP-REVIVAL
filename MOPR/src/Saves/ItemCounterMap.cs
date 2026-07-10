// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Сопоставляет тег счётчика расходуемого предмета (из items.txt) с базовым именем тегов
// трансформов его экземпляров. Нужно для выявления ситуации, когда сохранённый счётчик меньше
// числа реально присутствующих экземпляров (классический баг «предметы не спавнятся после покупки»).

using System.Collections.Generic;

namespace MOPR.Saves
{
    internal static class ItemCounterMap
    {
        public static readonly Dictionary<string, string> Entries = new Dictionary<string, string>
        {
            { "alternatorbeltID", "alternator belt" },
            { "batteryID", "battery" },
            { "BeerCaseID", "beercase" },
            { "BoozeID", "booze" },
            { "brakefluidID", "brakefluid" },
            { "cigarettesID", "cigarettes" },
            { "coolantID", "coolant" },
            { "fireextinguisherID", "fireextinguisher" },
            { "grillcharcoalID", "grillcharcoal" },
            { "groundcoffeeID", "groundcoffee" },
            { "juiceconcentrateID", "juiceconcentrate" },
            { "lightbulbID", "light bulb" },
            { "lightbulbboxID", "light bulb box" },
            { "macaronboxID", "macaronbox" },
            { "milkID", "milk" },
            { "mosquitosprayID", "mosquitospray" },
            { "motoroilID", "motoroil" },
            { "oilfilterID", "oil filter" },
            { "pikeID", "pike" },
            { "pizzaID", "pizza" },
            { "SausagesID", "sausages" },
            { "shoppingbagID", "shoppingbag" },
            { "sparkplugID", "spark plug" },
            { "sparkplugboxID", "spark plug box" },
            { "Spraycan01ID", "spraycan01" },
            { "Spraycan02ID", "spraycan02" },
            { "Spraycan03ID", "spraycan03" },
            { "Spraycan04ID", "spraycan04" },
            { "Spraycan05ID", "spraycan05" },
            { "Spraycan06ID", "spraycan06" },
            { "Spraycan07ID", "spraycan07" },
            { "Spraycan08ID", "spraycan08" },
            { "Spraycan09ID", "spraycan09" },
            { "Spraycan10ID", "spraycan10" },
            { "Spraycan11ID", "spraycan11" },
            { "Spraycan12ID", "spraycan12" },
            { "Spraycan13ID", "spraycan13" },
            { "sugarID", "sugar" },
            { "twostrokeID", "twostroke" },
            { "yeastID", "yeast" },
        };
    }
}
