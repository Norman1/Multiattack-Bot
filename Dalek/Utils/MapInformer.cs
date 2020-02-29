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

        public static BonusDetails GetBonus(TerritoryIDType territoryId)
        {
            return GameState.Map.Bonuses.Where(o => o.Value.Territories.Contains(territoryId)).FirstOrDefault().Value;
        }

        public static List<TerritoryIDType> GetNonOwnedBonusTerritories(BonusDetails bonus, Dictionary<TerritoryIDType, TerritoryStanding> territoryStandings)
        {
            return bonus.Territories.Where(o => territoryStandings[o].OwnerPlayerID != GameState.MyPlayerId).ToList();
        }

        public static List<TerritoryIDType> GetNeighborTerritories(TerritoryIDType territory)
        {
            MapDetails map = GameState.Map;
            List<TerritoryDetails> territoryDetails = map.Territories.Select(o => o.Value).ToList();
            TerritoryDetails territoryInQuestion = territoryDetails.Where(o => o.ID == territory).First();
            List<TerritoryIDType> connectedTerritoryIds = territoryInQuestion.ConnectedTo.Keys.ToList();
            return connectedTerritoryIds;
        }

        public static List<TerritoryIDType> RemoveMarkedAsUsedTerritories(TerritoryStanding territory, List<TerritoryIDType> testTerritories)
        {
            List<TerritoryIDType> territoriesMarkedAsUsed = territory.TerritoriesMarkedAsUsed;
            List<TerritoryIDType> result = new List<TerritoryIDType>();
            testTerritories.Where(o => !territoriesMarkedAsUsed.Contains(o)).ForEach(x => result.Add(x));
            return result;
        }

        public static Dictionary<TerritoryIDType, TerritoryStanding> GetOwnedNeighborTerritories(TerritoryIDType territory, Dictionary<TerritoryIDType, TerritoryStanding> territoryStandings)
        {
            List<TerritoryIDType> neighborTerritories = GetNeighborTerritories(territory);
            Dictionary<TerritoryIDType, TerritoryStanding> outDict = new Dictionary<TerritoryIDType, TerritoryStanding>();
            foreach (TerritoryIDType neighbor in neighborTerritories)
            {
                if (territoryStandings[neighbor].OwnerPlayerID == GameState.MyPlayerId)
                {
                    outDict.Add(neighbor, territoryStandings[neighbor]);
                }
            }

            return outDict;
        }


        public static Dictionary<TerritoryIDType, TerritoryStanding> GetNonOwnedNeighborTerritories(TerritoryIDType territory, Dictionary<TerritoryIDType, TerritoryStanding> territoryStandings)
        {
            List<TerritoryIDType> neighborTerritories = GetNeighborTerritories(territory);
            Dictionary<TerritoryIDType, TerritoryStanding> outDict = new Dictionary<TerritoryIDType, TerritoryStanding>();
            foreach (TerritoryIDType neighbor in neighborTerritories)
            {
                if (territoryStandings[neighbor].OwnerPlayerID != GameState.MyPlayerId)
                {
                    outDict.Add(neighbor, territoryStandings[neighbor]);
                }
            }

            return outDict;
        }

        public static TerritoryStanding GetTerritory(List<TerritoryStanding> territoryStandings, TerritoryIDType territoryId)
        {
            return territoryStandings.Where(o => o.ID.GetValue() == territoryId.GetValue()).First();
        }


    }
}
