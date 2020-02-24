using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.Shared.AI.Dalek.Utils
{
    public class RandomUtils
    {
        public static List<TerritoryIDType> ChooseRandom(List<TerritoryIDType> from, int amount)
        {
            List<TerritoryIDType> copy = new List<TerritoryIDType>(from);
            List<TerritoryIDType> result = new List<TerritoryIDType>();

            for (int i = 0; i < amount; i++)
            {
                TerritoryIDType randomElement = copy.Random();
                copy.Remove(randomElement);
                result.Add(randomElement);
                amount--;
            }
            return result;
        }

    }
}
