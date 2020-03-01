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
            List<MultiMoves> allChoices = GetAllMoves();
            MultiMoves bestChoice = GetBestMoves(allChoices);
            if (bestChoice == null)
            {
                bestChoice = new MultiMoves();
            }
            bestChoice = new NoPlanAddAddRemainingTask().CalculateNoPlanMoves(bestChoice);
            bestChoice = new MoveArmiesToBorderTask().CalculateMoveArmiesToBorderMoves(bestChoice);
            return bestChoice.GetAllMoves();
        }

        private List<MultiMoves> GetAllMoves()
        {
            MultiMoves initialMoves = new MultiMoves();
            List<MultiMoves> allMoves = new List<MultiMoves>();
            allMoves.Add(initialMoves);
            allMoves.AddRange(GetFollowupMoves(initialMoves, 0));
            MultiMoves bestMove = GetBestMoves(allMoves);
            allMoves.AddRange(GetFollowupMoves(bestMove, 0));
            bestMove = GetBestMoves(allMoves);
            allMoves.AddRange(GetFollowupMoves(bestMove, 0));
            bestMove = GetBestMoves(allMoves);
            allMoves.AddRange(GetFollowupMoves(bestMove, 0));
            return allMoves;
        }

        // TODO endless recursion for some  reason, so max depth. Probably when multiple choices available
        private List<MultiMoves> GetFollowupMoves(MultiMoves currentMoves, int currentDepth)
        {
            int maxDepth = 1;
            TakeBonusMultiTask task = new TakeBonusMultiTask();
            List<MultiMoves> followupMoves = task.CalculateTakeBonusMultiTask(currentMoves);
            if (currentDepth == maxDepth)
            {
                return followupMoves;
            }
            List<MultiMoves> deeperMoves = new List<MultiMoves>();
            foreach (MultiMoves followupMove in followupMoves)
            {
                deeperMoves.AddRange(GetFollowupMoves(followupMove, currentDepth + 1));
            }
            followupMoves.AddRange(deeperMoves);

            return followupMoves;
        }

        private MultiMoves GetBestMoves(List<MultiMoves> choices)
        {
            MultiMoves bestMoves = choices.FirstOrDefault();
            foreach (MultiMoves multimoves in choices)
            {
                MapEvaluation testEvaluation = new MapEvaluation(multimoves);
                MapEvaluation bestMovesEvaluation = new MapEvaluation(bestMoves);
                int testEvaluationValue = testEvaluation.GetValue();
                int bestMovesEvaluationValue = bestMovesEvaluation.GetValue();
                if (testEvaluationValue > bestMovesEvaluationValue)
                {
                    bestMoves = multimoves;
                }
            }
            return bestMoves;
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
