using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarLight.Shared.AI.Dalek.Utils;

namespace WarLight.Shared.AI.Dalek.Evaluation
{
    public class MultiMoves
    {
        public List<SingleMove> Moves = new List<SingleMove>();

        public Dictionary<TerritoryIDType, TerritoryStanding> GetGameStateAfterAllMoves()
        {
            if (Moves.Count == 0)
            {
                return GameState.CurrentTurn().LatestTurnStanding.Territories;
            }
            return Moves.Last().AfterStandings;
        }


        public void AddMove(SingleMove move)
        {
            if (move.Move is GameOrderDeploy)
            {
                AddDeployMove(move);
            }
            else
            {
                AddAttackMove(move);
            }
        }

        private void AddDeployMove(SingleMove move)
        {
            // Step 1: Add the move or merge it if already present
            GameOrderDeploy deployMove = (GameOrderDeploy)move.Move;
            List<SingleMove> deployMoves = GetDeployMoves();
            SingleMove alreadyPresentDeploymentMove = deployMoves.Where(o => ((GameOrderDeploy)o.Move).DeployOn == deployMove.DeployOn).FirstOrDefault();
            if (alreadyPresentDeploymentMove != null)
            {
                GameOrderDeploy alreadyPresentDeployment = (GameOrderDeploy)alreadyPresentDeploymentMove.Move;
                alreadyPresentDeployment.NumArmies += deployMove.NumArmies;
            }
            else
            {
                Moves.Add(move);
            }
            // Step 2: Add the armies to all other moves preconditions and postconditions
            int deployArmies = deployMove.NumArmies;
            Armies armies = new Armies(deployArmies);
            foreach (SingleMove singleMove in Moves)
            {
                if (singleMove == move)
                {
                    continue;
                }
                singleMove.AfterStandings.Where(o => o.Key == deployMove.DeployOn).ForEach(o => o.Value.NumArmies = o.Value.NumArmies.Add(armies));
            }
        }

        public List<SingleMove> GetDeployMoves()
        {
            return Moves.Where(o => o.Move is GameOrderDeploy).ToList();
        }

        public List<SingleMove> GetAttackMoves()
        {
            return Moves.Where(o => o.Move is GameOrderAttackTransfer).ToList();
        }


        private void AddAttackMove(SingleMove move)
        {
            Moves.Add(move);
        }

        public List<GameOrder> GetAllGameOrders()
        {
            return Moves.Select(o => o.Move).ToList();
        }

        public List<GameOrderDeploy> GetDeployOrders()
        {
            List<GameOrderDeploy> deployOrders = GetAllGameOrders().Where(o => o is GameOrderDeploy).ToList().Cast<GameOrderDeploy>().ToList();
            return deployOrders;
        }

        public List<GameOrderAttackTransfer> GetMoveOrders()
        {
            List<GameOrderAttackTransfer> moveOrders = GetAllGameOrders().Where(o => o is GameOrderAttackTransfer).ToList().Cast<GameOrderAttackTransfer>().ToList();
            return moveOrders;
        }

        public int GetStillAvailableIncome()
        {
            TurnState currentTurn = GameState.CurrentTurn();
            int income = currentTurn.GetMyIncome();
            List<GameOrderDeploy> deployOrders = GetDeployOrders();
            deployOrders.ForEach(o => income = income - o.NumArmies);
            return income;
        }

        public int GetAvailableArmiesOnTerritory(TerritoryIDType territoryId)
        {
            TurnState currentTurn = GameState.CurrentTurn();
            TerritoryStanding territoryStanding = currentTurn.LatestTurnStanding.Territories.Where(o => o.Key == territoryId).First().Value;
            int amountArmies = territoryStanding.NumArmies.ArmiesOrZero - 1;

            List<GameOrderDeploy> deployOrders = GetDeployOrders();
            deployOrders.Where(o => o.DeployOn == territoryId).ForEach(o => amountArmies += o.NumArmies);
            List<GameOrderAttackTransfer> moveOrders = GetMoveOrders();
            moveOrders.Where(o => o.From == territoryId).ForEach(o => amountArmies -= o.NumArmies.ArmiesOrZero);
            return amountArmies;
        }

        // TODO
        // Pumps the armies. Returns true if the armies could get pumped, false otherwise 
        public Boolean PumpArmies(int amountArmies, TerritoryIDType territoryToPumpTo)
        {
            List<GameOrderAttackTransfer> pumpPath = GetPumpPath(territoryToPumpTo);
            int availableIncome = GetStillAvailableIncome();
            TerritoryIDType pumpFromTerritory = pumpPath[0].From;
            int availableArmiesOnTerritory = GetAvailableArmiesOnTerritory(pumpFromTerritory);
            int neededExtraArmies = amountArmies - availableArmiesOnTerritory;
            if (neededExtraArmies > availableIncome)
            {
                return false;
            }
            if (neededExtraArmies > 0)
            {
                GameOrderDeploy deployOrder = GameOrderDeploy.Create(GameState.MyPlayerId, neededExtraArmies, pumpFromTerritory, true);
                //     AddMove(new SingleMove();
            }// TODO merge single move into the already calculated single moves

            return true;
        }

        // Returns an already calculated attack path from an owned territory to a territory in the path to which we want to pump extra deployment.
        private List<GameOrderAttackTransfer> GetPumpPath(TerritoryIDType territoryToPumpTo)
        {
            List<GameOrderAttackTransfer> moveOrders = GetAllGameOrders().Where(o => o is GameOrderAttackTransfer).ToList().Cast<GameOrderAttackTransfer>().ToList();
            // we can't pump from transfer moves
            GameStanding gameStanding = GameState.CurrentTurn().LatestTurnStanding;
            List<TerritoryIDType> ownedTerritories = gameStanding.Territories.Values.Where(o => o.OwnerPlayerID == GameState.MyPlayerId).Select(o => o.ID).ToList();

            moveOrders = moveOrders.Where(o => !ownedTerritories.Contains(o.To)).ToList();
            List<GameOrderAttackTransfer> pumpPath = null;
            TerritoryIDType currentPumpToTerritory = territoryToPumpTo;
            while (true)
            {
                if (ownedTerritories.Contains(currentPumpToTerritory))
                {
                    break;
                }
                GameOrderAttackTransfer pumpOrder = moveOrders.Find(o => o.To == currentPumpToTerritory);
                pumpPath.Add(pumpOrder);
                currentPumpToTerritory = pumpOrder.From;
            }
            return pumpPath;
        }


    }
}
