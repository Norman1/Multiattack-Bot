using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarLight.Shared.AI.Dalek.Evaluation;
using WarLight.Shared.AI.Dalek.Utils;

namespace WarLight.Shared.AI.Dalek.Decision
{
    // This class is responsible for scheduling already calculated moves
    class MovesScheduler
    {

        public MultiMoves CalculateScheduledMoves(MultiMoves precondition)
        {
            MultiMoves resultMoves = precondition.Clone();
            ScheduleDeployMoves(resultMoves);
            ScheduleAttackMoves(resultMoves);
            return resultMoves;
        }


        private void ScheduleDeployMoves(MultiMoves multiMoves)
        {
            var deployMoves = multiMoves.DeployMoves;

            deployMoves = deployMoves.OrderBy(d => d.DeployOn.GetValue()).ToList();
            var enemyTerritories = MapInformer.GetOwnedTerritories(GameState.CurrentTurn().LatestTurnStanding.Territories.Values.ToList(), GameState.OpponentPlayerId);
            var enemyTerritoryIds = new List<TerritoryIDType>();
            enemyTerritories.ForEach(t => enemyTerritoryIds.Add(t.ID));
            var distances = MapInformer.GetDistancesFromTerritories(enemyTerritoryIds);

            var opponentNeighboringDeployMoves = deployMoves.Where(d => distances[d.DeployOn] == 1);
            var nonOpponentNeighboringDeployMoves = deployMoves.Where(d => distances[d.DeployOn] > 1);
            multiMoves.DeployMoves.Clear();
            multiMoves.DeployMoves.AddRange(opponentNeighboringDeployMoves);
            multiMoves.DeployMoves.AddRange(nonOpponentNeighboringDeployMoves);
        }

        private void ScheduleAttackMoves(MultiMoves multiMoves)
        {
            var attackMoves = multiMoves.AttackMoves;
            /*
             * Algorithm
             * Step 1: Calculate the moves which belong together. The order of moves belonging together can't get changed.
             * Step 3: Schedule the moves
             */
            List<List<GameOrderAttackTransfer>> movesBelongingTogether = GetMovesBelongingTogether(attackMoves);
            List<GameOrderAttackTransfer> resultMoves = new List<GameOrderAttackTransfer>();
            while (movesBelongingTogether.Count() != 0)
            {
                var bestNextMove = movesBelongingTogether[0][0];
                int bestNextMoveListIndex = 0;
                for (int i = 0; i < movesBelongingTogether.Count(); i++)
                {
                    var testMove = movesBelongingTogether[i][0];
                    if (GetClassification(testMove) < GetClassification(bestNextMove))
                    {
                        bestNextMove = testMove;
                        bestNextMoveListIndex = i;
                    }
                    else if (GetClassification(testMove) == GetClassification(bestNextMove) && testMove.NumArmies.ArmiesOrZero > bestNextMove.NumArmies.ArmiesOrZero)
                    {
                        bestNextMove = testMove;
                        bestNextMoveListIndex = i;
                    }
                }
                resultMoves.Add(bestNextMove);
                movesBelongingTogether[bestNextMoveListIndex].Remove(bestNextMove);
                // Remove empty lists
                movesBelongingTogether.RemoveWhere(list => list.Count() == 0);
            }

            multiMoves.AttackMoves = resultMoves;
        }

        private int GetClassification(GameOrderAttackTransfer attackMove)
        {
            var initialState = GameState.CurrentTurn().LatestTurnStanding.Territories;
            bool opponentAttack = initialState[attackMove.To].OwnerPlayerID == GameState.OpponentPlayerId;
            var attackFromNeighbors = MapInformer.GetNeighborTerritories(initialState[attackMove.From].ID);
            var opponentTerritories = MapInformer.GetOwnedTerritories(initialState.Values.ToList(), GameState.OpponentPlayerId);
            var opponentNeighboringAttack = false;

            foreach (var opponentTerritory in opponentTerritories)
            {
                if (attackFromNeighbors.Contains(opponentTerritory.ID))
                {
                    opponentNeighboringAttack = true;
                }
            }
            bool clearOpponentBreak = false;
            int opponentIncome = GameState.CurrentTurn().Incomes[GameState.OpponentPlayerId].FreeArmies;
            int maxOpponentArmies = 0;
            if (opponentAttack)
            {
                maxOpponentArmies = initialState[attackMove.To].NumArmies.ArmiesOrZero + opponentIncome;
                int maxNeededArmies = AttackInformer.GetNeededBreakArmies(maxOpponentArmies);
                if (attackMove.NumArmies.ArmiesOrZero >= maxNeededArmies)
                {
                    clearOpponentBreak = true;
                }
            }
            bool isTransfer = initialState[attackMove.To].OwnerPlayerID == GameState.MyPlayerId;
            bool isExpansion = initialState[attackMove.To].IsNeutral;

            if (clearOpponentBreak)
            {
                return 1;
            }
            if (isTransfer)
            {
                return 2;
            }
            if (isExpansion && !opponentNeighboringAttack)
            {
                return 3;
            }
            if (isExpansion && opponentNeighboringAttack)
            {
                return 4;
            }
            // non clear opponent attacks
            return 5;

        }
        private List<List<GameOrderAttackTransfer>> GetMovesBelongingTogether(List<GameOrderAttackTransfer> attackMoves)
        {
            var attackMovesCopy = new List<GameOrderAttackTransfer>(attackMoves);
            List<List<GameOrderAttackTransfer>> resultMoves = new List<List<GameOrderAttackTransfer>>();
            while (attackMovesCopy.Count != 0)
            {
                var resultPath = new List<GameOrderAttackTransfer>();
                resultPath.Add(attackMovesCopy.First());
                bool foundSomething = true;
                while (foundSomething)
                {
                    foundSomething = false;
                    foreach (GameOrderAttackTransfer testMove in attackMovesCopy)
                    {
                        bool isToAdd = resultPath.Where(o => o.To.Equals(testMove.From)).Count() > 0;
                        if (isToAdd)
                        {
                            resultPath.Add(testMove);
                            foundSomething = true;
                        }
                    }
                    attackMovesCopy = attackMovesCopy.Except(resultPath).ToList();
                }

                resultMoves.Add(resultPath);
            }
            return resultMoves;

        }

    }
}

