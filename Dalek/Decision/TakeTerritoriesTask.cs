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
        public MultiMoves CalculateTakeTerritoriesMoves(List<TerritoryIDType> territoriesToTake, MultiMoves presentMoves)
        {
            MultiMoves resultMoves = presentMoves.Clone();
            Dictionary<TerritoryIDType, TerritoryStanding> precondition = resultMoves.GetTerritoryStandingsAfterAllMoves();
            resultMoves = CalculateMovePath(territoriesToTake, resultMoves);
            return resultMoves;
        }

        private MultiMoves CalculateMovePath(List<TerritoryIDType> territoriesToTake, MultiMoves precondition)
        {
            List<TerritoryIDType> territoriesToTakeCopy = new List<TerritoryIDType>(territoriesToTake);
            SortTerritoriesToTake(territoriesToTakeCopy, precondition);
            Boolean foundSomething = true;
            while (foundSomething)
            {
                List<TerritoryIDType> addedTerritories = new List<TerritoryIDType>();
                foundSomething = false;
                foreach (TerritoryIDType territoryIdToTake in territoriesToTakeCopy)
                {
                    var territoryStandingsAfterMoves = precondition.GetTerritoryStandingsAfterAllMoves();
                    TerritoryStanding territoryToTake = territoryStandingsAfterMoves.Where(o => o.Key == territoryIdToTake).First().Value;
                    var ownedNeighbors = MapInformer.GetOwnedNeighborTerritories(territoryIdToTake, territoryStandingsAfterMoves);
                    if (ownedNeighbors.Count == 0)
                    {
                        continue;
                    }
                    TerritoryIDType ownedNeighbor = GetBestOwnTerritoryToMakeAttack(ownedNeighbors, territoryIdToTake, precondition);
                    int neededAttackArmies = AttackInformer.GetNeededBreakArmies(territoryToTake.NumArmies.ArmiesOrZero);
                    GameOrderAttackTransfer attackOrder = GameOrderAttackTransfer.Create(GameState.MyPlayerId, ownedNeighbor, territoryIdToTake, AttackTransferEnum.AttackTransfer, false, new Armies(neededAttackArmies), true);

                    // pump to the start territory of the attack order
                    // the territory can already have some armies like leftovers from a previous attack
                    var standing = territoryStandingsAfterMoves.Where(o => o.Key == attackOrder.From).First().Value;
                    int leftoverArmies = standing.NumArmies.ArmiesOrZero - standing.ArmiesMarkedAsUsed.ArmiesOrZero - 1;
                    int armiesToPump = Math.Max(0, attackOrder.NumArmies.ArmiesOrZero - leftoverArmies);

                    bool successfull = precondition.PumpArmies(attackOrder.From, armiesToPump);
                    precondition.AddAttackOrder(attackOrder);
                    foundSomething = true;
                    addedTerritories.Add(territoryIdToTake);
                    if (!successfull)
                    {
                        return null;
                    }
                }
                territoriesToTakeCopy = territoriesToTakeCopy.Except(addedTerritories).ToList();
                SortTerritoriesToTake(territoriesToTakeCopy, precondition);
            }

            return precondition;
        }

        private int GetNonOwnedNeighborTerritoryCount
            (TerritoryIDType territory, List<TerritoryIDType> territoryRange, Dictionary<TerritoryIDType, TerritoryStanding> territoryStandings)
        {
            var nonOwnedNeighborTerritories = MapInformer.GetNonOwnedNeighborTerritories(territory, territoryStandings).Keys.ToList();
            nonOwnedNeighborTerritories.RemoveWhere(t => !territoryRange.Contains(t));
            return nonOwnedNeighborTerritories.Count();
        }

        private void SortTerritoriesToTake(List<TerritoryIDType> territoriesToTake, MultiMoves calculatedMoves)
        {
            // Sort by unowned neighbors in territories to take
            var currentTerritoryStandings = calculatedMoves.GetTerritoryStandingsAfterAllMoves();
            territoriesToTake.Sort((x, y) =>
            {
                int xCount = GetNonOwnedNeighborTerritoryCount(x, territoriesToTake, currentTerritoryStandings);
                int yCount = GetNonOwnedNeighborTerritoryCount(y, territoriesToTake, currentTerritoryStandings);
                return xCount.CompareTo(yCount);
            });

            // Prefer territories when we are already attacking a neighbor
            var territoriesWithNeighborAttacks = new List<TerritoryIDType>();
            foreach (TerritoryIDType territoryId in territoriesToTake)
            {
                var neighbors = MapInformer.GetNeighborTerritories(territoryId);
                if (calculatedMoves.AttackMoves.Where(a => neighbors.Contains(a.To)).Count() > 0)
                {
                    territoriesWithNeighborAttacks.Add(territoryId);
                }
            }
            var territoriesWithNonNeighborAttacks = territoriesToTake.Where(t => !territoriesWithNeighborAttacks.Contains(t)).ToList();
            territoriesToTake.Clear();
            territoriesToTake.AddRange(territoriesWithNeighborAttacks);
            territoriesToTake.AddRange(territoriesWithNonNeighborAttacks);

            // Especially prefer territories when our last attack order attacks a neighbor
            if (calculatedMoves.AttackMoves.Count() > 0)
            {
                TerritoryIDType lastAttackTerritory = calculatedMoves.AttackMoves.Last().To;
                var territoriesWithLastNeighborAttacks = new List<TerritoryIDType>();
                foreach (TerritoryIDType territoryId in territoriesToTake)
                {
                    var neighbors = MapInformer.GetNeighborTerritories(territoryId);
                    if (neighbors.Contains(lastAttackTerritory))
                    {
                        territoriesWithLastNeighborAttacks.Add(territoryId);
                    }
                }
                var territoriesWithNonLastNeighborAttacks = territoriesToTake.Where(t => !territoriesWithLastNeighborAttacks.Contains(t)).ToList();
                territoriesToTake.Clear();
                territoriesToTake.AddRange(territoriesWithLastNeighborAttacks);
                territoriesToTake.AddRange(territoriesWithNonLastNeighborAttacks);
            }


            // Prefer territories when we already own a neighbor (else we can't move with current code)
            territoriesToTake.Sort((x, y) =>
            {
                int xCount = Math.Max(1, MapInformer.GetOwnedNeighborTerritories(x, currentTerritoryStandings).Count);
                int yCount = Math.Max(1, MapInformer.GetOwnedNeighborTerritories(y, currentTerritoryStandings).Count);
                return yCount.CompareTo(xCount);
            });



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
