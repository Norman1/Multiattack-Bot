using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarLight.Shared.AI.Dalek.Utils;

namespace WarLight.Shared.AI.Dalek.Evaluation
{
    public class MapEvaluator
    {
        public static int Evaluate(List<TerritoryStanding> territoryStandings)
        {
            int myValue = GetPlayerValue(GameState.MyPlayerId, territoryStandings);
            int opponentValue = GetPlayerValue(GameState.OpponentPlayerId, territoryStandings);
            return myValue - opponentValue;
        }


        private static int GetPlayerValue(PlayerIDType player, List<TerritoryStanding> territoryStandings)
        {
            List<TerritoryStanding> ownedTerritories = MapInformer.GetOwnedTerritories(territoryStandings, player);
            return ownedTerritories.Count();
        }

    }
}
