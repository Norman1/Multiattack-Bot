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
            GetDeployMoves(multiMoves);
            multiMoves = GetAttackMoves(multiMoves);
            return multiMoves.GetAllMoves();
        }

        private void GetDeployMoves(MultiMoves multiMoves)
        {
            TurnState currentTurn = GameState.CurrentTurn();
            int freeArmies = currentTurn.GetMyIncome();
            GameStanding gameStanding = currentTurn.LatestTurnStanding;
            List<TerritoryIDType> ownedTerritories = gameStanding.Territories.Values.Where(o => o.OwnerPlayerID == GameState.MyPlayerId).Select(o => o.ID).ToList();
            TerritoryIDType randomTerritory = ownedTerritories.Random();
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
            foreach (var territory in GameState.CurrentTurn().LatestTurnStanding.Territories.Where(t => t.Value.OwnerPlayerID == GameState.MyPlayerId))
            {
                var nonOwnedNeighbors = MapInformer.GetNonOwnedNeighborTerritories(territory.Key, GameState.CurrentTurn().LatestTurnStanding.Territories);
                if (nonOwnedNeighbors.Count > 0)
                {
                    nonOwnedTerritory = nonOwnedNeighbors.Random().Key;
                    break;
                }
            }
            var bonus = MapInformer.GetBonus(nonOwnedTerritory);
            AILog.Log("Debug", "Attempting to take bonus: " + bonus.Name);
            var nonOwnedBonusTerritories = MapInformer.GetNonOwnedBonusTerritories(bonus, GameState.CurrentTurn().LatestTurnStanding.Territories);
            return nonOwnedBonusTerritories;
        }

        private MultiMoves GetAttackMoves(MultiMoves multiMoves)
        {
            var nonOwnedTerritories = GetNonOwnedBonusTerritoriesToTake(multiMoves);
            var newMoves = new TakeTerritoriesTask().CalculateTaketerritoriesMoves(nonOwnedTerritories, multiMoves);
            if (newMoves != null)
            {
                multiMoves = newMoves;
            }
            return multiMoves;

            /*
            List<TerritoryIDType> allTerritories = GameState.Map.Territories.Keys.ToList();
            for (int i = 0; i <= 5; i++)
            {
                foreach (TerritoryIDType territoryId in allTerritories)
                {
                    var afterStandings = multiMoves.GetTerritoryStandingsAfterAllMoves();
                    TerritoryStanding fromTerritory = afterStandings[territoryId];
                    if (fromTerritory.OwnerPlayerID != GameState.MyPlayerId)
                    {
                        continue;
                    }
                    int armiesAvailable = fromTerritory.NumArmies.ArmiesOrZero - 1 - fromTerritory.ArmiesMarkedAsUsed.NumArmies;
                    if (armiesAvailable < 7)
                    {
                        continue;
                    }
                    var neighbors = MapInformer.GetNonOwnedNeighborTerritories(territoryId, afterStandings).Keys.ToList();
                    var nonUsedNeighbors = MapInformer.RemoveMarkedAsUsedTerritories(fromTerritory, neighbors);
                    if (nonUsedNeighbors.Count == 0)
                    {
                        continue;
                    }
                    var randomNeighbor = nonUsedNeighbors.Random();
                    Armies attackArmies = new Armies(7);
                    GameOrderAttackTransfer order = GameOrderAttackTransfer.Create(GameState.MyPlayerId, territoryId, randomNeighbor, AttackTransferEnum.AttackTransfer, false, attackArmies, false);
                    multiMoves.AddAttackOrder(order);
                }
            }

            // test pump functionality
            var beforeStandings = GameState.CurrentTurn().LatestTurnStanding.Territories;
            var afterStandingsX = multiMoves.GetTerritoryStandingsAfterAllMoves();
            foreach (TerritoryStanding territory in afterStandingsX.Values)
            {
                if (territory.OwnerPlayerID == GameState.MyPlayerId && beforeStandings[territory.ID].OwnerPlayerID != GameState.MyPlayerId)
                {
                    multiMoves.PumpArmies(territory.ID, 100);
                }
            }
*/
        }
    }
}
