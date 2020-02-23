using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.Shared.AI.Dalek.Decision
{
    public class SearchTree
    {


        public SearchTree(int maxDepth, int maxIncome, List<TerritoryStanding> territoryStandings)
        {
            SpanSearchTree(maxDepth, 0, maxIncome, territoryStandings);
        }

        private void SpanSearchTree(int maxDepth, int currentDepth, int stillAvailableIncome, List<TerritoryStanding> territoryStandings)
        {

        }


    }
}
