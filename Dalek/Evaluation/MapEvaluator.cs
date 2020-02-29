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
        private static readonly int ARMIES_MULTIPLICATOR = 1;
        private static readonly int BONUS_MULTIPLICATOR = 10;

        private MultiMoves Moves;
        private Dictionary<TerritoryIDType, TerritoryStanding> FinalStandings;



        public MapEvaluation(MultiMoves moves)
        {
            Moves = moves;
            FinalStandings = moves.GetTerritoryStandingsAfterAllMoves();
        }

        public int GetValue()
        {
            int ownedArmiesValue = GetOwnedArmiesValue();
            int bonusesValue = GetOwnedBonusesValue();
            return ownedArmiesValue + bonusesValue;
        }

        private int GetOwnedArmiesValue()
        {
            var ownedTerritories = MapInformer.GetOwnedTerritories(FinalStandings.Values.ToList(), GameState.MyPlayerId);
            int amountArmiesOwned = 0;
            ownedTerritories.ForEach(o => amountArmiesOwned += o.NumArmies.ArmiesOrZero);
            int value = amountArmiesOwned * ARMIES_MULTIPLICATOR;
            return value;
        }

        private int GetOwnedBonusesValue()
        {
            Dictionary<BonusIDType, BonusDetails> ownedBonuses = MapInformer.GetOwnedBonuses(FinalStandings, GameState.MyPlayerId);
            int bonusIncome = 0;
            ownedBonuses.ForEach(b => bonusIncome += b.Value.Amount);
            int value = bonusIncome * BONUS_MULTIPLICATOR;
            return value;
        }

    }
}
