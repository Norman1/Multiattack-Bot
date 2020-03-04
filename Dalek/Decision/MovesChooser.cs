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
        public MultiMoves GetMoves()
        {
            List<MultiMoves> allChoices = GetAllMoves();
            MultiMoves bestChoice = GetBestMoves(allChoices);
            return bestChoice;
        }

        private List<MultiMoves> GetAllMoves()
        {
            MultiMoves bestMove = new MultiMoves();
            List<MultiMoves> allMoves = new List<MultiMoves>();
            int currentCount = 0;
            for (int i = 0; i < 4; i++)
            {
                allMoves.AddRange(GetFollowupMoves(bestMove, 0));
                if (allMoves.Count == currentCount)
                {
                    break;
                }
                currentCount = allMoves.Count;
                bestMove = GetBestMoves(allMoves);
            }

            if (allMoves.Count == 0)
            {
                allMoves.Add(new MultiMoves());
            }

            var completedMoves = new List<MultiMoves>();
            foreach (MultiMoves multiMove in allMoves)
            {
                var completedMove = new NoPlanAddAddRemainingTask().CalculateNoPlanMoves(multiMove);
                completedMove = new MoveArmiesToBorderTask().CalculateMoveArmiesToBorderMoves(completedMove);
                completedMoves.Add(completedMove);
            }
            return completedMoves;
        }

        // TODO endless recursion for some  reason, so max depth. Probably when multiple choices available
        private List<MultiMoves> GetFollowupMoves(MultiMoves currentMoves, int currentDepth)
        {
            int maxDepth = 1;
            BreakBonusMultiTask breakBonusMultiTask = new BreakBonusMultiTask();
            List<MultiMoves> breakFollowupupMoves = breakBonusMultiTask.CalculateBreakBonusMultiTask(currentMoves);
            TakeBonusMultiTask takeBonusMultiTask = new TakeBonusMultiTask();
            List<MultiMoves> takeBonusFollowupMoves = takeBonusMultiTask.CalculateTakeBonusMultiTask(currentMoves);
            List<MultiMoves> followupMoves = new List<MultiMoves>();
            followupMoves.AddRange(breakFollowupupMoves);
            followupMoves.AddRange(takeBonusFollowupMoves);
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
    }
}
