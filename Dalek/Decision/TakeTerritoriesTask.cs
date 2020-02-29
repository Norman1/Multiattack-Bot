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
        //TODO as for now we ignore the deployment and just return the presentMoves object
        public MultiMoves CalculateTaketerritoriesMoves(List<TerritoryIDType> territoriesToTake, MultiMoves presentMoves)
        {
            MultiMoves resultMoves = presentMoves.Clone();
            Dictionary<TerritoryIDType, TerritoryStanding> precondition = resultMoves.GetTerritoryStandingsAfterAllMoves();
            resultMoves = GalculateMovePath(territoriesToTake, resultMoves);
            return resultMoves;
        }


        private MultiMoves GalculateMovePath(List<TerritoryIDType> territoriesToTake, MultiMoves precondition)
        {
            List<TerritoryIDType> territoriesToTakeCopy = new List<TerritoryIDType>(territoriesToTake);
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
                    TerritoryIDType ownedNeighbor = ownedNeighbors.Random().Key;
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
            }

            return precondition;
        }

    }

}
