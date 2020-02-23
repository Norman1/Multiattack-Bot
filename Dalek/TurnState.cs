using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.Shared.AI.Dalek
{
    public class TurnState
    {
        public MapDetails Map;
        public int NumberOfTurns;
        public Dictionary<PlayerIDType, PlayerIncome> Incomes;
        public GameOrder[] PrevTurn;
        public GameStanding LatestTurnStanding;
        public GameStanding PreviousTurnStanding;

        public TurnState(MapDetails map, int numberOfTurns,
            Dictionary<PlayerIDType, PlayerIncome> incomes,
            GameOrder[] prevTurn,
            GameStanding latestTurnStanding,
            GameStanding previousTurnStanding
            )
        {
            Map = map;
            NumberOfTurns = numberOfTurns;
            Incomes = incomes;
            PrevTurn = prevTurn;
            LatestTurnStanding = latestTurnStanding;
            PreviousTurnStanding = previousTurnStanding;
        }

        public int GetMyIncome()
        {
            PlayerIDType myId = GameState.MyPlayerId ;
            PlayerIncome income = Incomes[myId];
            return income.FreeArmies;
        }

    }
}
