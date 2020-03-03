using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarLight.Shared.AI.Dalek.Evaluation;
using WarLight.Shared.AI.Dalek.Utils;

namespace WarLight.Shared.AI.Dalek.Decision
{
    // Calcualtes for each single opponent bonus the necessary steps in order to break it.
    class BreakBonusMultiTask
    {
        private static readonly String REASON = "BreakBonusMultiTask";

        public List<MultiMoves> CalculateBreakBonusMultiTask(MultiMoves presentMoves)
        {
            List<MultiMoves> resultMoves = new List<MultiMoves>();
            var opponentBonuses = MapInformer.GetOwnedBonuses(presentMoves.GetTerritoryStandingsAfterAllMoves(), GameState.OpponentPlayerId);
            foreach (var opponentBonus in opponentBonuses)
            {
                MultiMoves breakBonusMoves = GetBreakBonusMoves(opponentBonus.Key, presentMoves);
                if (breakBonusMoves != null)
                {
                    resultMoves.Add(breakBonusMoves);
                }
            }
            return resultMoves;
        }

        private MultiMoves GetBreakBonusMoves(BonusIDType opponentBonus, MultiMoves presentMoves)
        {
            var bonusTerritories = GameState.Map.Bonuses[opponentBonus].Territories;
            var distances = MapInformer.GetDistancesFromTerritories(bonusTerritories);
            var ownedBorderTerritories = MapInformer.GetOwnedBorderTerritories(presentMoves.GetTerritoryStandingsAfterAllMoves(), GameState.MyPlayerId);
            int minDistance = ownedBorderTerritories.Min(o => distances[o.ID]);
            var bestStartTerritory = ownedBorderTerritories.Where(o => distances[o.ID] == minDistance).First();
            int currentDistance = minDistance;
            var currentTerritoryInAttackPath = bestStartTerritory.ID;
            List<TerritoryIDType> territoriesToTake = new List<TerritoryIDType>();
            while (currentDistance != 0)
            {
                var neighbors = MapInformer.GetNeighborTerritories(currentTerritoryInAttackPath);
                var possibleAttackNeighbors = neighbors.Where(n => distances[n] == currentDistance - 1).ToList();
                var bestNeighborToAttack = GetBestNeighborToAttack(possibleAttackNeighbors);
                territoriesToTake.Add(bestNeighborToAttack);
                currentTerritoryInAttackPath = bestNeighborToAttack;
                currentDistance--;
            }
            TakeTerritoriesTask takeTerritoriesTask = new TakeTerritoriesTask(REASON);
            MultiMoves resultMoves = takeTerritoriesTask.CalculateTakeTerritoriesMoves(territoriesToTake, presentMoves);
            return resultMoves;
        }

        private TerritoryIDType GetBestNeighborToAttack(List<TerritoryIDType> possibleTargets)
        {
            var bestTarget = GameState.CurrentTurn().LatestTurnStanding.Territories[possibleTargets.First()];
            foreach (TerritoryIDType territoryId in possibleTargets)
            {
                var testTarget = GameState.CurrentTurn().LatestTurnStanding.Territories[territoryId];
                if (bestTarget.IsNeutral && testTarget.OwnerPlayerID == GameState.OpponentPlayerId)
                {
                    testTarget = bestTarget;
                }
                else if (bestTarget.IsNeutral && testTarget.IsNeutral && testTarget.NumArmies.ArmiesOrZero < bestTarget.NumArmies.ArmiesOrZero)
                {
                    testTarget = bestTarget;
                }
                else if (bestTarget.OwnerPlayerID == GameState.OpponentPlayerId && testTarget.OwnerPlayerID == GameState.OpponentPlayerId)
                {
                    var testTargetBonus = MapInformer.GetBonus(testTarget.ID);
                    var bestTargetBonus = MapInformer.GetBonus(bestTarget.ID);
                    var opponentBonuses = MapInformer.GetOwnedBonuses(GameState.CurrentTurn().LatestTurnStanding.Territories, GameState.OpponentPlayerId);
                    if (opponentBonuses.Keys.Contains(testTargetBonus.ID) && !opponentBonuses.Keys.Contains(bestTargetBonus.ID))
                    {
                        testTarget = bestTarget;
                    }
                    else if (opponentBonuses.Keys.Contains(testTargetBonus.ID) && opponentBonuses.Keys.Contains(bestTargetBonus.ID)
                        && opponentBonuses[testTargetBonus.ID].Amount > opponentBonuses[bestTargetBonus.ID].Amount)
                    {
                        testTarget = bestTarget;
                    }
                }
            }
            return bestTarget.ID;
        }

    }
}