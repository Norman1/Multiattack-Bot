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
            GetAttackMoves(multiMoves);
            return multiMoves.GetAllMoves();
        }

        private void GetDeployMoves(MultiMoves multiMoves)
        {
            TurnState currentTurn = GameState.CurrentTurn();
            int freeArmies = currentTurn.GetMyIncome();
            GameStanding gameStanding = currentTurn.LatestTurnStanding;
            List<TerritoryIDType> ownedTerritories = gameStanding.Territories.Values.Where(o => o.OwnerPlayerID == GameState.MyPlayerId).Select(o => o.ID).ToList();

            while (freeArmies > 0)
            {
                TerritoryIDType randomTerritory = ownedTerritories.Random();
                GameOrderDeploy order = GameOrderDeploy.Create(GameState.MyPlayerId, 1, randomTerritory, true);
                multiMoves.AddDeployOrder(order);
                freeArmies--;
            }

        }

        private void GetAttackMoves(MultiMoves multiMoves)
        {
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
                    if (armiesAvailable == 0)
                    {
                        continue;
                    }
                    var neighbors = MapInformer.GetNeighborTerritories(territoryId);
                    var nonUsedNeighbors = MapInformer.RemoveMarkedAsUsedTerritories(fromTerritory, neighbors);
                    if (nonUsedNeighbors.Count == 0)
                    {
                        continue;
                    }
                    var randomNeighbor = nonUsedNeighbors.Random();
                    Armies attackArmies = new Armies(armiesAvailable);
                    GameOrderAttackTransfer order = GameOrderAttackTransfer.Create(GameState.MyPlayerId, territoryId, randomNeighbor, AttackTransferEnum.AttackTransfer, false, attackArmies, false);
                    multiMoves.AddAttackOrder(order);
                }
            }
        }
    }
}
