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

        public static TerritoryStanding GetTerritory(List<TerritoryStanding> territoryStandings, TerritoryIDType territoryId)
        {
            return territoryStandings.Where(o => o.ID.GetValue() == territoryId.GetValue()).First();
        }

        public static List<TerritoryStanding> GetTerritoryStandings(GameStanding gameStanding)
        {
            return gameStanding.Territories.Select(o => o.Value).ToList();
        }

    }
}
