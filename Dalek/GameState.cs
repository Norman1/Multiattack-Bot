using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.Shared.AI.Dalek
{
    public static class GameState
    {
        public static PlayerIDType MyPlayerId;
        public static PlayerIDType OpponentPlayerId;
        public static Dictionary<PlayerIDType, GamePlayer> Players;
        public static GameStanding DistributionStanding;
        public static GameSettings GameSettings;
        public static MapDetails Map;
        public static List<TurnState> TurnStates = new List<TurnState>();

        public static void PushMoveState(PlayerIDType myPlayerId, Dictionary<PlayerIDType, GamePlayer> players,
            MapDetails map, GameStanding distributionStanding, GameSettings gameSettings, int numberOfTurns,
            Dictionary<PlayerIDType, PlayerIncome> incomes, GameOrder[] prevTurn, GameStanding latestTurnStanding,
            GameStanding previousTurnStanding)
        {
            MyPlayerId = myPlayerId;
            OpponentPlayerId = players.Keys.Where(o => o != MyPlayerId).First();
            Players = players;
            DistributionStanding = distributionStanding;
            GameSettings = gameSettings;
            Map = map;
            TurnStates.Add(new TurnState(numberOfTurns, incomes, prevTurn, latestTurnStanding, previousTurnStanding));
        }

        public static TurnState CurrentTurn()
        {
            return TurnStates[TurnStates.Count - 1];
        }


    }
}
