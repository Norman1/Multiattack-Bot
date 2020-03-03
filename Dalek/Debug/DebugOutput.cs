using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarLight.Shared.AI.Dalek.Evaluation;

namespace WarLight.Shared.AI.Dalek.Debug
{
    public class DebugOutput
    {
        private static System.Diagnostics.Stopwatch watch = null;

        public static void LogBeginMove()
        {
            AILog.Log("Debug", "----- Turn: " + GameState.CurrentTurn().NumberOfTurns + " -----");
            watch = System.Diagnostics.Stopwatch.StartNew();
        }

        public static void LogEndMove(MultiMoves moves)
        {
            watch.Stop();
            AILog.Log("Debug", "Round execution time: " + watch.ElapsedMilliseconds);
            LogAllMoves(moves);
        }

        public static void LogBeginGame()
        {
            var territories = GameState.Map.Territories.Values.ToList();
            foreach(var territory in territories)
            {
                AILog.Log("Debug", territory.ID+" --> "+territory.Name);
            }
        }

        public static void LogAllMoves(MultiMoves multiMoves)
        {
            var deployMoves = multiMoves.DeployMoves;
            var attackMoves = multiMoves.AttackMoves;
            MapDetails map = GameState.Map;
            foreach (GameOrderDeploy deployMove in deployMoves)
            {
                var territoryName = map.Territories[deployMove.DeployOn].Name;
                int armies = deployMove.NumArmies;
                String message = "Deployment on " + territoryName + " - Armies: " + armies + " - Reason: " + deployMove.Reason;
                AILog.Log("Debug", message);
            }

            foreach (GameOrderAttackTransfer attackMove in attackMoves)
            {
                var fromName = map.Territories[attackMove.From].Name;
                var toName = map.Territories[attackMove.To].Name;
                int armies = attackMove.NumArmies.ArmiesOrZero;
                String reason = attackMove.Reason;
                String message = "Attacking: " + fromName + " -[" + armies + "]-> " + toName + " - Reason: " + attackMove.Reason;
                AILog.Log("Debug", message);
            }
        }



    }
}
