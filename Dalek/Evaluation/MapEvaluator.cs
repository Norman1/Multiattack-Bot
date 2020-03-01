using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarLight.Shared.AI.Dalek.Utils;

namespace WarLight.Shared.AI.Dalek.Evaluation
{
    public class MapEvaluation
    {
        private static readonly int BONUS_MULTIPLICATOR = 100;
        private static readonly int ARMIES_MULTIPLICATOR = 10;
        private static readonly int TERRITORIES_MULTIPLICATOR = 1;

        private MultiMoves Moves;
        private Dictionary<TerritoryIDType, TerritoryStanding> FinalStandings;



        public MapEvaluation(MultiMoves moves)
        {
            Moves = moves;
            FinalStandings = moves.GetTerritoryStandingsAfterAllMoves();
        }

        public int GetValue()
        {
            int bonusesValue = GetOwnedBonusesValue(GameState.MyPlayerId) - GetOwnedBonusesValue(GameState.OpponentPlayerId);
            int ownedArmiesValue = GetOwnedArmiesValue(GameState.MyPlayerId) - GetOwnedArmiesValue(GameState.OpponentPlayerId);
            int ownedTerritoriesValue = GetOwnedTerritoriesValue(GameState.MyPlayerId) - GetOwnedTerritoriesValue(GameState.OpponentPlayerId);
            return bonusesValue + ownedArmiesValue + ownedTerritoriesValue;
        }


        private int GetOwnedBonusesValue(PlayerIDType playerId)
        {
            Dictionary<BonusIDType, BonusDetails> ownedBonuses = MapInformer.GetOwnedBonuses(FinalStandings, playerId);
            int bonusIncome = 0;
            ownedBonuses.ForEach(b => bonusIncome += b.Value.Amount);
            int value = bonusIncome * BONUS_MULTIPLICATOR;
            return value;
        }

        private int GetOwnedArmiesValue(PlayerIDType playerId)
        {
            var ownedTerritories = MapInformer.GetOwnedTerritories(FinalStandings.Values.ToList(), playerId);
            int amountArmiesOwned = 0;
            ownedTerritories.ForEach(o => amountArmiesOwned += o.NumArmies.ArmiesOrZero);
            int value = amountArmiesOwned * ARMIES_MULTIPLICATOR;
            return value;
        }

        private int GetOwnedTerritoriesValue(PlayerIDType playerId)
        {
            var ownedTerritories = MapInformer.GetOwnedTerritories(FinalStandings.Values.ToList(), playerId);
            int amountOwned = ownedTerritories.Count();
            int value = amountOwned * TERRITORIES_MULTIPLICATOR;
            return value;
        }

    }
}
