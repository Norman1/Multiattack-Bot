using System.Collections.Generic;
using System.Diagnostics;
using WarLight.Shared.AI.Dalek.Debug;
using WarLight.Shared.AI.Dalek.Decision;
using WarLight.Shared.AI.Dalek.Evaluation;

namespace WarLight.Shared.AI.Dalek.Bot
{
    public class BotMain : IWarLightAI
    {
        public BotMain()
        {
        }

        public string Name()
        {
            return "Dalek";
        }

        public string Description()
        {
            return "A pure multiattack bot.";
        }
        // don't care
        public bool SupportsSettings(GameSettings settings, out string whyNot)
        {
            whyNot = null;
            return true;
        }
        // don't care
        public bool RecommendsSettings(GameSettings settings, out string whyNot)
        {
            whyNot = null;
            return true;
        }



        public void Init(GameIDType gameID, PlayerIDType myPlayerID, Dictionary<PlayerIDType, GamePlayer> players, MapDetails map, GameStanding distributionStanding, GameSettings gameSettings, int numberOfTurns, Dictionary<PlayerIDType, PlayerIncome> incomes, GameOrder[] prevTurn, GameStanding latestTurnStanding, GameStanding previousTurnStanding, Dictionary<PlayerIDType, TeammateOrders> teammatesOrders, List<CardInstance> cards, int cardsMustPlay, Stopwatch timer, List<string> directives)
        {
            GameState.PushMoveState(myPlayerID, players, map, distributionStanding, gameSettings, numberOfTurns, incomes, prevTurn, latestTurnStanding, previousTurnStanding);
        }



        public List<GameOrder> GetOrders()
        {
            if (GameState.CurrentTurn().NumberOfTurns == 0)
            {
                DebugOutput.LogBeginGame();
            }
            DebugOutput.LogBeginMove();
            MovesChooser movesChooser = new MovesChooser();
            var calculatedMoves = movesChooser.GetMoves();
            DebugOutput.LogEndMove(calculatedMoves);
            return calculatedMoves.GetAllMoves();
        }

        public List<TerritoryIDType> GetPicks()
        {
            PicksChooser picksChooser = new PicksChooser();
            return picksChooser.GetPicks();
        }






    }
}
