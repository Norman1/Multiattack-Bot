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
        static public void LogBeginMove()
        {
            AILog.Log("Debug", "----- Turn: " + GameState.CurrentTurn().NumberOfTurns + " -----");
            watch = System.Diagnostics.Stopwatch.StartNew();
        }

        static public void LogEndMove()
        {
            watch.Stop();
            AILog.Log("Debug", "Round execution time: " + watch.ElapsedMilliseconds);
        }

    }
}
