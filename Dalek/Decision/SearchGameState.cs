using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarLight.Shared.AI.Dalek.Evaluation;

namespace WarLight.Shared.AI.Dalek.Decision
{
    public class SearchGameState
    {
        public int SearchDepth;
        public SearchGameState Successor;
        public SearchGameState Predecessor;
        public int StillAvailableIncome;
        public List<TerritoryStanding> TerritoryStandings;


        public int getBoardValue()
        {
            return MapEvaluator.Evaluate(TerritoryStandings);
        }

    }
}
