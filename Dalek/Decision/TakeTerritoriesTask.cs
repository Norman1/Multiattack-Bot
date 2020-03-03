using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarLight.Shared.AI.Dalek.Evaluation;
using WarLight.Shared.AI.Dalek.Utils;

namespace WarLight.Shared.AI.Dalek.Decision
{
    // This class is responsible for calculating the moves in order to take given territories
    public class TakeTerritoriesTask
    {
        private readonly String REASON;

        public TakeTerritoriesTask(String reason)
        {
            REASON = reason;
        }

        public MultiMoves CalculateTakeTerritoriesMoves(List<TerritoryIDType> territoriesToTake, MultiMoves precondition)
        {
            MultiMoves resultMoves = precondition.Clone();
            List<TerritoryIDType> territoriesToTakeCopy = new List<TerritoryIDType>(territoriesToTake);


            while (territoriesToTakeCopy.Count > 0)
            {
                var reachableTerritories = FilterNonBorderingTerritories(territoriesToTakeCopy, resultMoves);
                if (reachableTerritories.Count == 0)
                {
                    return null;
                }
                var bestStartTerritory = GetBestAttackingTerritory(reachableTerritories, resultMoves);
                var bestAttackTerritory = GetBestAttackTerritory(reachableTerritories, territoriesToTakeCopy, bestStartTerritory, resultMoves);
                int neededAttackArmies = AttackInformer.GetNeededBreakArmies
                    (resultMoves.GetTerritoryStandingsAfterAllMoves()[bestAttackTerritory].NumArmies.ArmiesOrZero);
                GameOrderAttackTransfer attackOrder = GameOrderAttackTransfer.Create
                (GameState.MyPlayerId, bestStartTerritory, bestAttackTerritory, AttackTransferEnum.AttackTransfer, new Armies(neededAttackArmies), REASON);

                var standing = resultMoves.GetTerritoryStandingsAfterAllMoves().Where(o => o.Key == attackOrder.From).First().Value;
                int leftoverArmies = standing.NumArmies.ArmiesOrZero - standing.ArmiesMarkedAsUsed.ArmiesOrZero - 1;
                int armiesToPump = Math.Max(0, attackOrder.NumArmies.ArmiesOrZero - leftoverArmies);
                bool successfull = resultMoves.PumpArmies(attackOrder.From, armiesToPump, REASON);
                if (!successfull)
                {
                    return null;
                }
                resultMoves.AddAttackOrder(attackOrder);
                territoriesToTakeCopy.Remove(bestAttackTerritory);
            }

            return resultMoves;
        }

        private TerritoryIDType GetBestAttackTerritory(List<TerritoryIDType> immediateTakeTargets, List<TerritoryIDType> allTakeTargets, TerritoryIDType attackingTerritory, MultiMoves presentMoves)
        {
            List<TerritoryIDType> borderingTargets = immediateTakeTargets.Where(t => MapInformer.GetNeighborTerritories(t).Contains(attackingTerritory)).ToList();

            // We prefer territories with a as little neighboring territories in the takeTargets as possible
            var bestAttackTerritory = borderingTargets.First();
            foreach (var testAttackTerritory in borderingTargets)
            {
                var bestAttackTerritoryNeighborCount = MapInformer.GetNeighborTerritories(bestAttackTerritory).Where(t => allTakeTargets.Contains(t)).Count();
                var testAttackTerritoryNeighborCount = MapInformer.GetNeighborTerritories(testAttackTerritory).Where(t => allTakeTargets.Contains(t)).Count();
                if (testAttackTerritoryNeighborCount < bestAttackTerritoryNeighborCount)
                {
                    bestAttackTerritory = testAttackTerritory;
                }
                else if (testAttackTerritoryNeighborCount == bestAttackTerritoryNeighborCount)
                {
                    // If both have the same amount we prefer territories with as little unowned neighbors as possible
                    var bestUnownedNeighborCount = MapInformer.GetNonOwnedNeighborTerritories(bestAttackTerritory, presentMoves.GetTerritoryStandingsAfterAllMoves()).Count();
                    var testUnownedNeighborCount = MapInformer.GetNonOwnedNeighborTerritories(testAttackTerritory, presentMoves.GetTerritoryStandingsAfterAllMoves()).Count();
                    if (testUnownedNeighborCount < bestUnownedNeighborCount)
                    {
                        bestAttackTerritory = testAttackTerritory;
                    }
                }
            }
            return bestAttackTerritory;
        }

        private TerritoryIDType GetBestAttackingTerritory(List<TerritoryIDType> takeTargets, MultiMoves presentMoves)
        {
            var currentGameStandings = presentMoves.GetTerritoryStandingsAfterAllMoves();
            var ownedTerritories = MapInformer.GetOwnedTerritories(currentGameStandings.Values.ToList(), GameState.MyPlayerId);
            var possibleStartTerritories = new List<TerritoryStanding>();
            foreach (var ownedTerritory in ownedTerritories)
            {
                List<TerritoryIDType> neighbors = MapInformer.GetNeighborTerritories(ownedTerritory.ID);
                var matchingNeighbors = neighbors.Where(n => takeTargets.Contains(n)).ToList();
                if (matchingNeighbors.Count > 0)
                {
                    possibleStartTerritories.Add(ownedTerritory);
                }
            }

            var bestStartTerritory = possibleStartTerritories.First();
            var beginTurnStandings = GameState.CurrentTurn().LatestTurnStanding.Territories;

            foreach (var testTerritory in possibleStartTerritories)
            {
                bool bestTerritoryOwnedByMyself = beginTurnStandings[bestStartTerritory.ID].OwnerPlayerID == GameState.MyPlayerId;
                bool testTerritoryOwnedByMyself = beginTurnStandings[testTerritory.ID].OwnerPlayerID == GameState.MyPlayerId;

                // if one of them has no neighbors to take prefer the other one
                bool bestTerritoryCanContinue = MapInformer.GetNeighborTerritories(bestStartTerritory.ID).Where(n => takeTargets.Contains(n)).Count() > 0;
                bool testTerritoryCanContinue = MapInformer.GetNeighborTerritories(testTerritory.ID).Where(n => takeTargets.Contains(n)).Count() > 0;
                if (testTerritoryCanContinue && !bestTerritoryCanContinue)
                {
                    bestStartTerritory = testTerritory;
                }
                // if both are already owned by myself at the beginning of the round prefer the one with more armies on it
                else if (bestTerritoryOwnedByMyself && testTerritoryOwnedByMyself)
                {
                    if (testTerritory.NumArmies.ArmiesOrZero > bestStartTerritory.NumArmies.ArmiesOrZero)
                    {
                        bestStartTerritory = testTerritory;
                    }
                }
                // if only one of them is owned by myself at the beginning of the round prefer the one not owned
                else if (bestTerritoryOwnedByMyself && !testTerritoryOwnedByMyself)
                {
                    bestStartTerritory = testTerritory;
                }

                // if both aren't owned by myself at the beginning of the round prefer the one with the longer attack path
                else if (!bestTerritoryOwnedByMyself && !testTerritoryOwnedByMyself)
                {
                    var bestPathCount = presentMoves.GetMovePath(bestStartTerritory.ID).Count();
                    var testPathCount = presentMoves.GetMovePath(testTerritory.ID).Count();
                    if (testPathCount > bestPathCount)
                    {
                        bestStartTerritory = testTerritory;
                    }
                }
            }
            return bestStartTerritory.ID;
        }

        private List<TerritoryIDType> FilterNonBorderingTerritories(List<TerritoryIDType> territoriesToTake, MultiMoves presentMoves)
        {
            var standings = presentMoves.GetTerritoryStandingsAfterAllMoves();
            return territoriesToTake.Where(t => MapInformer.GetOwnedNeighborTerritories(t, standings).Count > 0).ToList();
        }

        private int GetNonOwnedNeighborTerritoryCount
            (TerritoryIDType territory, List<TerritoryIDType> territoryRange, Dictionary<TerritoryIDType, TerritoryStanding> territoryStandings)
        {
            var nonOwnedNeighborTerritories = MapInformer.GetNonOwnedNeighborTerritories(territory, territoryStandings).Keys.ToList();
            nonOwnedNeighborTerritories.RemoveWhere(t => !territoryRange.Contains(t));
            return nonOwnedNeighborTerritories.Count();
        }


        private TerritoryIDType GetBestOwnTerritoryToMakeAttack(Dictionary<TerritoryIDType, TerritoryStanding> ownedNeighbors, TerritoryIDType territoryToTake,
            MultiMoves currentMoves)
        {
            TerritoryIDType bestNeighbor = ownedNeighbors.First().Key;
            foreach (TerritoryIDType territoryId in ownedNeighbors.Keys)
            {
                var attackMoves = currentMoves.AttackMoves.Where(o => o.From == bestNeighbor && o.To == territoryId).ToList();
                if (attackMoves.Count > 0)
                {
                    bestNeighbor = attackMoves.First().To;
                }

            }
            // if we have all neighbors already choose the one with the max armies
            var ownedTerritories = MapInformer.GetOwnedTerritories(GameState.CurrentTurn().LatestTurnStanding.Territories.Values.ToList(), GameState.MyPlayerId);
            bool allNeighborsOwnedAtStart = true;
            foreach (TerritoryIDType ownedNeighbor in ownedNeighbors.Keys)
            {
                if (ownedTerritories.Where(o => o.ID == ownedNeighbor).Count() == 0)
                {
                    allNeighborsOwnedAtStart = false;
                    break;
                }
            }
            if (allNeighborsOwnedAtStart)
            {
                var territoryStandings = currentMoves.GetTerritoryStandingsAfterAllMoves();
                foreach (TerritoryIDType ownedNeighbor in ownedNeighbors.Keys)
                {
                    if (territoryStandings[ownedNeighbor].NumArmies.ArmiesOrZero > territoryStandings[bestNeighbor].NumArmies.ArmiesOrZero)
                    {
                        bestNeighbor = ownedNeighbor;
                    }
                }
            }

            return bestNeighbor;
        }

    }

}
