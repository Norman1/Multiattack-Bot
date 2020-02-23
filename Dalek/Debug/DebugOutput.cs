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
        public static void LogMultiMoves(MultiMoves multiMoves)
        {
            foreach (SingleMove singleMove in multiMoves.Moves)
            {
                AILog.Log("Debug", "After standings count= " + singleMove.AfterStandings.Count());
            }

        }
    }
}
