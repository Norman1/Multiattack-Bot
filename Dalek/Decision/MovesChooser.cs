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
            TurnState currentTurn = GameState.CurrentTurn();
            int freeArmies = currentTurn.GetMyIncome();
            GameStanding gameStanding = currentTurn.LatestTurnStanding;
            List<TerritoryIDType> ownedTerritories = gameStanding.Territories.Values.Where(o => o.OwnerPlayerID == GameState.MyPlayerId).Select(o => o.ID).ToList();
            // Add a single deploy order and move the armies from that territory to a neighboring one
            TerritoryIDType randomTerritory = ownedTerritories.Last();
            GameOrder order = GameOrderDeploy.Create(GameState.MyPlayerId, freeArmies, randomTerritory, true);

            List<TerritoryIDType> neighbors = MapInformer.GetNeighborTerritories(randomTerritory);
            TerritoryIDType firstNeighbor = neighbors[0];
            int amountArmies = freeArmies;
            TerritoryStanding theRandomTerritory = MapInformer.GetTerritory(MapInformer.GetTerritoryStandings(gameStanding), randomTerritory);
            Armies attackArmies = new Armies(amountArmies + theRandomTerritory.NumArmies.ArmiesOrZero - 1);

            GameOrder order2 = GameOrderAttackTransfer.Create(GameState.MyPlayerId, randomTerritory, firstNeighbor, AttackTransferEnum.AttackTransfer, false, attackArmies, false);


            MultiMoves multiMoves = new MultiMoves();
            SingleMove singleDeployMove = new SingleMove(MapInformer.GetTerritoryStandings(gameStanding), order);
            multiMoves.AddMove(singleDeployMove);

            SingleMove singleAttackMove = new SingleMove(MapInformer.GetTerritoryStandings(gameStanding), order2);
            multiMoves.AddMove(singleAttackMove);

            return multiMoves.GetAllGameOrders();
        }



    }
}
