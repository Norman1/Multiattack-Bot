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
        public static Dictionary<PlayerIDType, GamePlayer> Players;
        public static GameStanding DistributionStanding;
        public static GameSettings GameSettings;
        public static List<TurnState> TurnStates = new List<TurnState>();

        public static void PushMoveState(PlayerIDType myPlayerId, Dictionary<PlayerIDType, GamePlayer> players,
            MapDetails map, GameStanding distributionStanding, GameSettings gameSettings, int numberOfTurns,
            Dictionary<PlayerIDType, PlayerIncome> incomes, GameOrder[] prevTurn, GameStanding latestTurnStanding,
            GameStanding previousTurnStanding)
        {
            MyPlayerId = myPlayerId;
            Players = players;
            DistributionStanding = distributionStanding;
            GameSettings = gameSettings;
            TurnStates.Add(new TurnState(map, numberOfTurns, incomes, prevTurn, latestTurnStanding, previousTurnStanding));
        }

        public static TurnState CurrentTurn()
        {
            return TurnStates[TurnStates.Count -1];
        }


    }
}
