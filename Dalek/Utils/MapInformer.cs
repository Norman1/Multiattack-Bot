using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.Shared.AI.Dalek.Utils
{
    public class MapInformer
    {
        public static List<TerritoryStanding> GetOwnedTerritories(List<TerritoryStanding> territoryStandings, PlayerIDType player)
        {
            return territoryStandings.Where(o => o.OwnerPlayerID == player).ToList();

        }
    }
}
