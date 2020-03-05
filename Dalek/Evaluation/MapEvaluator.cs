using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarLight.Shared.AI.Dalek.Utils;

namespace WarLight.Shared.AI.Dalek.Evaluation
{
    public class MapEvaluation
    {
        private static readonly int BONUS_MULTIPLICATOR = 100;
        private static readonly int ARMIES_MULTIPLICATOR = 10;
        private static readonly int TERRITORIES_MULTIPLICATOR = 1;
        private static readonly int MAX_BONUS_SIZE_ADJUSTMENT = 10;
        private static readonly int MAX_BONUS_DISTANCE_ADJUSTMENT = 20;

        private MultiMoves Moves;
        private Dictionary<TerritoryIDType, TerritoryStanding> FinalStandings;
        Dictionary<TerritoryIDType, int> distancesCache = null;




        public MapEvaluation(MultiMoves moves)
        {
            Moves = moves;
            FinalStandings = moves.GetTerritoryStandingsAfterAllMoves();
        }

        public int GetValue()
        {
            int bonusesValue = GetOwnedBonusesValue(GameState.MyPlayerId) - GetOwnedBonusesValue(GameState.OpponentPlayerId);
            int ownedArmiesValue = GetOwnedArmiesValue(GameState.MyPlayerId) - GetOwnedArmiesValue(GameState.OpponentPlayerId);
            int ownedTerritoriesValue = GetOwnedTerritoriesValue(GameState.MyPlayerId) - GetOwnedTerritoriesValue(GameState.OpponentPlayerId);
            return bonusesValue + ownedArmiesValue + ownedTerritoriesValue;
        }


        private int GetOwnedBonusesValue(PlayerIDType playerId)
        {
            Dictionary<BonusIDType, BonusDetails> ownedBonuses = MapInformer.GetOwnedBonuses(FinalStandings, playerId);

            int value = 0;
            ownedBonuses.ForEach(b => value += GetOwnedBonusValue(b.Value, playerId));

            return value;
        }

        private int GetMedianBonusSize()
        {
            var bonuses = GameState.Map.Bonuses.Values.ToList();
            bonuses = bonuses.OrderBy(b => b.Territories.Count()).ToList();
            var medianBonus = bonuses[bonuses.Count / 2];
            return medianBonus.Territories.Count();
        }

        private int GetBonusSizeMultiplicator(BonusDetails bonus)
        {
            int medianBonusSize = GetMedianBonusSize();
            int bonusSize = bonus.Territories.Count;
            int differenceFromMedian = bonusSize - medianBonusSize;
            int multiplicator;

            if (differenceFromMedian >= 0)
            {
                multiplicator = Math.Min(differenceFromMedian, MAX_BONUS_SIZE_ADJUSTMENT);
            }
            else
            {
                multiplicator = Math.Max(differenceFromMedian, -MAX_BONUS_SIZE_ADJUSTMENT);
            }
            return multiplicator + 100;
        }
        /*
        private int GetAverageTerritoryDistance()
        {
            var opponentTerritories = MapInformer.GetOwnedTerritories(FinalStandings.Values.ToList(), GameState.OpponentPlayerId);
            var opponentTerritoryIds = new List<TerritoryIDType>();
            opponentTerritories.ForEach(t => opponentTerritoryIds.Add(t.ID));
            var distances = MapInformer.GetDistancesFromTerritories(opponentTerritoryIds);
            int territoryCount = 0;
            int allDistances = 0;
            foreach (int distance in distances.Values)
            {
                if (distance > 0)
                {
                    territoryCount++;
                    allDistances += distance;
                }
            }
            return allDistances / territoryCount;
        }
*/


        private int GetBonusDistanceMultiplicator(BonusDetails bonus)
        {
            var opponentTerritories = MapInformer.GetOwnedTerritories(FinalStandings.Values.ToList(), GameState.OpponentPlayerId);
            var opponentTerritoryIds = new List<TerritoryIDType>();
            opponentTerritories.ForEach(t => opponentTerritoryIds.Add(t.ID));
            if (distancesCache == null)
            {
                distancesCache = MapInformer.GetDistancesFromTerritories(opponentTerritoryIds);
            }
            int territoryCount = 0;
            int allDistances = 0;
            int bonusTerritoryCount = 0;
            int bonusTerritoryDistances = 0;
            foreach (var territory in distancesCache.Keys)
            {
                var distance = distancesCache[territory];
                if (distance > 0)
                {
                    territoryCount++;
                    allDistances += distance;
                }

                if (bonus.Territories.Contains(territory))
                {
                    bonusTerritoryCount++;
                    bonusTerritoryDistances += distancesCache[territory];
                }

            }
            int averageDistance;
            if (territoryCount != 0)
            {
                averageDistance = allDistances / territoryCount;
            }
            else
            {
                averageDistance = 0;
            }
            int bonusAverageDistance = bonusTerritoryDistances / bonusTerritoryCount;
            int difference = bonusAverageDistance - averageDistance;

            int multiplicator;
            if (bonusAverageDistance >= 0)
            {
                multiplicator = Math.Min(difference, MAX_BONUS_DISTANCE_ADJUSTMENT);
            }
            else
            {
                multiplicator = Math.Max(difference, -MAX_BONUS_DISTANCE_ADJUSTMENT);
            }
            return multiplicator + 100;
        }

        private int GetFudgeFactor(BonusDetails bonus, PlayerIDType playerId)
        {
            if (playerId == GameState.OpponentPlayerId)
            {
                return 100;
            }
            return GetBonusDistanceMultiplicator(bonus);
        }

        private int GetOwnedBonusValue(BonusDetails bonus, PlayerIDType playerId)
        {
            // we like high income
            int value = bonus.Amount * BONUS_MULTIPLICATOR;
            value = value * GetFudgeFactor(bonus, playerId) / 100;



            return value;
        }

        private int GetOwnedArmiesValue(PlayerIDType playerId)
        {
            var ownedTerritories = MapInformer.GetOwnedTerritories(FinalStandings.Values.ToList(), playerId);
            int amountArmiesOwned = 0;
            ownedTerritories.ForEach(o => amountArmiesOwned += o.NumArmies.ArmiesOrZero);
            int value = amountArmiesOwned * ARMIES_MULTIPLICATOR;
            return value;
        }

        private int GetOwnedTerritoriesValue(PlayerIDType playerId)
        {
            var ownedTerritories = MapInformer.GetOwnedTerritories(FinalStandings.Values.ToList(), playerId);
            int amountOwned = ownedTerritories.Count();
            int value = amountOwned * TERRITORIES_MULTIPLICATOR;
            return value;
        }

    }
}
