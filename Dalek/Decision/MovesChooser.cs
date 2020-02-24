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
            return multiMoves.GetAllGameOrders();
        }

        private void GetAttackMoves(MultiMoves multiMoves)
        {
            var afterStandings = multiMoves.Moves.Last().AfterStandings;
            foreach (var territoryStanding in afterStandings.Values)
            {
                if (territoryStanding.OwnerPlayerID != GameState.MyPlayerId)
                {
                    continue;
                }
                int armiesAvailable = territoryStanding.NumArmies.ArmiesOrZero - 1;
                if (armiesAvailable == 0)
                {
                    continue;
                }
                List<TerritoryIDType> neighbors = MapInformer.GetNeighborTerritories(territoryStanding.ID);
                TerritoryIDType randomNeighbor = neighbors.Random();
                Armies attackArmies = new Armies(armiesAvailable);
                GameOrder order = GameOrderAttackTransfer.Create(GameState.MyPlayerId, territoryStanding.ID, randomNeighbor, AttackTransferEnum.AttackTransfer, false, attackArmies, false);
                SingleMove singleAttackMove = new SingleMove(afterStandings, order);
                multiMoves.AddMove(singleAttackMove);
            }
        }

        private void GetDeployMoves(MultiMoves multiMoves)
        {
            TurnState currentTurn = GameState.CurrentTurn();
            int freeArmies = currentTurn.GetMyIncome();
            GameStanding gameStanding = currentTurn.LatestTurnStanding;
            List<TerritoryIDType> ownedTerritories = gameStanding.Territories.Values.Where(o => o.OwnerPlayerID == GameState.MyPlayerId).Select(o => o.ID).ToList();
            TerritoryIDType randomTerritory = ownedTerritories.Random();
            GameOrder order = GameOrderDeploy.Create(GameState.MyPlayerId, freeArmies, randomTerritory, true);
            SingleMove singleDeployMove = new SingleMove(gameStanding.Territories, order);
            multiMoves.AddMove(singleDeployMove);
        }




    }
}
