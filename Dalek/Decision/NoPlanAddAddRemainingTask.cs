using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarLight.Shared.AI.Dalek.Evaluation;
using WarLight.Shared.AI.Dalek.Utils;

namespace WarLight.Shared.AI.Dalek.Decision
{
    // This class is supposed to get called after all calculated moves. If still armies are available it adds them
    class NoPlanAddAddRemainingTask
    {

        private static readonly String REASON = "NoPlanAddAddRemainingTask";

        public MultiMoves CalculateNoPlanMoves(MultiMoves presentMoves)
        {
            MultiMoves resultMoves = presentMoves.Clone();
            // we currently assume the biggest attack has to start from an owned territory
            GameOrderAttackTransfer biggestAttack = GetBiggestAttackMove(resultMoves);
            int stillAvailableDeployment = GameState.CurrentTurn().GetMyIncome() - resultMoves.GetCurrentDeployment();
            if (biggestAttack != null)
            {
                PumpBiggestAttack(resultMoves, biggestAttack, stillAvailableDeployment);
            }
            else
            {
                AddBorderTerritoryDeployment(resultMoves, stillAvailableDeployment);
            }
            return resultMoves;
        }



        private void AddBorderTerritoryDeployment(MultiMoves movesSoFar, int availableDeployment)
        {
            TerritoryStanding territoryToDeployTo = MapInformer.GetOwnedBorderTerritories(movesSoFar.GetTerritoryStandingsAfterAllMoves(), GameState.MyPlayerId).FirstOrDefault();
            if (territoryToDeployTo == null)
            {
                territoryToDeployTo = MapInformer.GetOwnedTerritories(GameState.CurrentTurn().LatestTurnStanding.Territories.Values.ToList(), GameState.MyPlayerId).First();
            }
            movesSoFar.AddDeployOrder(GameOrderDeploy.Create(GameState.MyPlayerId, availableDeployment, territoryToDeployTo.ID, REASON));
        }



        private void PumpBiggestAttack(MultiMoves movesSoFar, GameOrderAttackTransfer biggestAttack, int availableDeployment)
        {
            if (availableDeployment == 0)
            {
                return;
            }
            movesSoFar.AddDeployOrder(GameOrderDeploy.Create(GameState.MyPlayerId, availableDeployment, biggestAttack.From, REASON));
            var endAttack = biggestAttack;
            bool foundStep = true;
            while (foundStep)
            {
                foundStep = false;
                // probably endless loop possible as soon as we add transfer moves back to attacking territory
                var nextAttack = movesSoFar.AttackMoves.Where(a => a.From == endAttack.To).FirstOrDefault();
                if (nextAttack != null)
                {
                    endAttack = nextAttack;
                    foundStep = true;
                }

            }
            movesSoFar.PumpArmies(endAttack.To, availableDeployment, REASON);
        }



        private GameOrderAttackTransfer GetBiggestAttackMove(MultiMoves movesSoFar)
        {
            if (movesSoFar.AttackMoves.Count() == 0)
            {
                return null;
            }
            GameOrderAttackTransfer biggestAttack = movesSoFar.AttackMoves.First();
            foreach (GameOrderAttackTransfer attackMove in movesSoFar.AttackMoves)
            {
                if (attackMove.NumArmies.ArmiesOrZero > biggestAttack.NumArmies.ArmiesOrZero)
                {
                    biggestAttack = attackMove;
                }
            }
            return biggestAttack;
        }


    }
}
