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
            TerritoryIDType randomTerritory = ownedTerritories.Last();
            GameOrder order = GameOrderDeploy.Create(GameState.MyPlayerId, freeArmies, randomTerritory, true);

            MultiMoves multiMoves = new MultiMoves();
            SingleMove singleMove = new SingleMove(MapInformer.GetTerritoryStandings(gameStanding), order);
            multiMoves.AddMove(singleMove);
            DebugOutput.LogMultiMoves(multiMoves);
            return multiMoves.getAllGameOrders();
        }



    }
}
