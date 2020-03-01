using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.Shared.AI.Dalek.Utils
{
    public class MapInformer
    {


        public static Dictionary<TerritoryIDType, int> GetDistancesFromTerritories
            (List<TerritoryIDType> distanceTerritories)
        {
            // init 
            var result = new Dictionary<TerritoryIDType, int>();
            foreach (var territoryId in GameState.CurrentTurn().LatestTurnStanding.Territories.Keys)
            {
                bool present = distanceTerritories.Contains(territoryId);
                if (present)
                {
                    result.Add(territoryId, 0);
                }
                else
                {
                    result.Add(territoryId, -1);
                }
            }

            // calculate
            bool foundSomething = true;
            while (foundSomething)
            {
                foundSomething = false;
                foreach (TerritoryIDType territoryId in GameState.CurrentTurn().LatestTurnStanding.Territories.Keys)
                {
                    if (result[territoryId] != -1)
                    {
                        continue;
                    }
                    var neighbors = GetNeighborTerritories(territoryId);
                    var calculatedNeighbors = neighbors.Where(n => result[n] != -1).ToList();
                    if (calculatedNeighbors.Count > 0)
                    {
                        int minNeighborValue = calculatedNeighbors.Min(c => result[c]);
                        result[territoryId] = minNeighborValue + 1; ;
                        foundSomething = true;
                    }

                }
            }

            return result;
        }

        public static List<TerritoryIDType> GetBonusTerritoriesAndNeighbors(BonusDetails bonus)
        {
            HashSet<TerritoryIDType> result = new HashSet<TerritoryIDType>(bonus.Territories);
            // System.InvalidOperationException: "Die Sammlung wurde geändert. Der Enumerationsvorgang kann möglicherweise nicht ausgeführt werden."
            foreach (TerritoryIDType testTerritory in bonus.Territories)
            {
                List<TerritoryIDType> neighbors = GetNeighborTerritories(testTerritory);
                result.UnionWith(neighbors);
            }
            return result.ToList();
        }

        public static List<TerritoryStanding> GetOwnedBorderTerritories(Dictionary<TerritoryIDType, TerritoryStanding> territoryStandings, PlayerIDType player)
        {
            var ownedTerritories = territoryStandings.Values.Where(o => o.OwnerPlayerID == player);
            return ownedTerritories.Where(o => GetNonOwnedNeighborTerritories(o.ID, territoryStandings).Count() > 0).ToList();
        }

        public static List<TerritoryStanding> GetOwnedTerritories(List<TerritoryStanding> territoryStandings, PlayerIDType player)
        {
            return territoryStandings.Where(o => o.OwnerPlayerID == player).ToList();
        }

        public static List<TerritoryStanding> GetNonOwnedTerritories(List<TerritoryStanding> territoryStandings, PlayerIDType player)
        {
            return territoryStandings.Where(o => o.OwnerPlayerID != player).ToList();
        }

        public static Dictionary<BonusIDType, BonusDetails> GetOwnedBonuses(Dictionary<TerritoryIDType, TerritoryStanding> territoryStandings, PlayerIDType player)
        {
            Dictionary<BonusIDType, BonusDetails> allBonuses = GameState.Map.Bonuses;
            Dictionary<BonusIDType, BonusDetails> ownedBonuses = new Dictionary<BonusIDType, BonusDetails>();
            List<TerritoryIDType> ownedTerritories = territoryStandings.Keys.Where(t => territoryStandings[t].OwnerPlayerID == player).ToList();

            foreach (BonusIDType bonusId in allBonuses.Keys)
            {
                BonusDetails bonusDetails = allBonuses[bonusId];
                List<TerritoryIDType> ownedBonusTerritories = bonusDetails.Territories.Where(t => ownedTerritories.Contains(t)).ToList();
                if (ownedBonusTerritories.Count() == bonusDetails.Territories.Count())
                {
                    ownedBonuses.Add(bonusId, bonusDetails);
                }
            }

            return ownedBonuses;
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
