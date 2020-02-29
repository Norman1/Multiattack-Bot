using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarLight.Shared.AI.Dalek.Debug;
using WarLight.Shared.AI.Dalek.Evaluation;
using WarLight.Shared.AI.Dalek.Utils;

namespace WarLight.Shared.AI.Dalek.Decision
{
    public class MovesChooser
    {
        public List<GameOrder> GetMoves()
        {
            MultiMoves multiMoves = new MultiMoves();
            multiMoves = GetAttackMoves(multiMoves);
            GetDeployMoves(multiMoves);
            return multiMoves.GetAllMoves();
        }

        private void GetDeployMoves(MultiMoves multiMoves)
        {
            TurnState currentTurn = GameState.CurrentTurn();
            int freeArmies = currentTurn.GetMyIncome() - multiMoves.GetCurrentDeployment();
            GameStanding gameStanding = currentTurn.LatestTurnStanding;
            List<TerritoryIDType> ownedTerritories = gameStanding.Territories.Values.Where(o => o.OwnerPlayerID == GameState.MyPlayerId).Select(o => o.ID).ToList();
            TerritoryIDType randomTerritory = ownedTerritories.First();
            while (freeArmies > 0)
            {
                GameOrderDeploy order = GameOrderDeploy.Create(GameState.MyPlayerId, 1, randomTerritory, true);
                multiMoves.AddDeployOrder(order);
                freeArmies--;
            }

        }



        private List<TerritoryIDType> GetNonOwnedBonusTerritoriesToTake(MultiMoves multiMoves)
        {
            // Choose an arbitrary territory with a non owned neighbor 
            TerritoryIDType nonOwnedTerritory = new TerritoryIDType();
            foreach (var territory in multiMoves.GetTerritoryStandingsAfterAllMoves().Values.Where(t => t.OwnerPlayerID == GameState.MyPlayerId))
            {
                var nonOwnedNeighbors = MapInformer.GetNonOwnedNeighborTerritories(territory.ID, multiMoves.GetTerritoryStandingsAfterAllMoves());
                if (nonOwnedNeighbors.Count > 0)
                {
                    nonOwnedTerritory = nonOwnedNeighbors.First().Key;
                    break;
                }
            }
            var bonus = MapInformer.GetBonus(nonOwnedTerritory);
            AILog.Log("Debug", "Attempting to take bonus: " + bonus.Name);
            var nonOwnedBonusTerritories = MapInformer.GetNonOwnedBonusTerritories(bonus, multiMoves.GetTerritoryStandingsAfterAllMoves());
            return nonOwnedBonusTerritories;
        }

        private MultiMoves GetAttackMoves(MultiMoves multiMoves)
        {
            bool foundSomething = true;
            int maxCounter = 10;
            while (foundSomething && maxCounter > 0)
            {
                foundSomething = false;
                var nonOwnedTerritories = GetNonOwnedBonusTerritoriesToTake(multiMoves);
                var newMoves = new TakeTerritoriesTask().CalculateTakeTerritoriesMoves(nonOwnedTerritories, multiMoves);
                if (newMoves != null)
                {
                    multiMoves = newMoves;
                    foundSomething = true;
                    maxCounter--;
                }
            }
            return multiMoves;

        }
    }
}
