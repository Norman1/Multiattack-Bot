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

        // Fehler: verschiedene moves beeinflussen sich nicht (innere schleife)???
        private void GetAttackMoves(MultiMoves multiMoves)
        {
            for (int i = 0; i <= 5; i++)
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
                    var nonOwnedNeighbors = MapInformer.GetNonOwnedNeighborTerritories(territoryStanding.ID, afterStandings);
                    if (nonOwnedNeighbors.Count == 0)
                    {
                        continue;
                    }
                    var randomNeighbor = nonOwnedNeighbors.Random();
                    Armies attackArmies = new Armies(armiesAvailable);
                    GameOrder order = GameOrderAttackTransfer.Create(GameState.MyPlayerId, territoryStanding.ID, randomNeighbor.Key, AttackTransferEnum.AttackTransfer, false, attackArmies, false);
                    // TODO falsch, afterStandings. before = last single attack executed
                    // SingleMove singleAttackMove = new SingleMove(afterStandings, order);
                    SingleMove singleAttackMove = new SingleMove(multiMoves.Moves.Last().AfterStandings, order);
                    multiMoves.AddMove(singleAttackMove);
                }

            }

        }




    }
}
