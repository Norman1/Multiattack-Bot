using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarLight.Shared.AI.Dalek.Evaluation;
using WarLight.Shared.AI.Dalek.Utils;

namespace WarLight.Shared.AI.Dalek.Decision
{

    class TakeBonusMultiTask
    {
        private static readonly String REASON = "TakeBonusMultiTask";

        // Calculates for each bonus on the map the steps necessary to take it
        // Currently only neighboring bonuses are seen as applicable
        public List<MultiMoves> CalculateTakeBonusMultiTask(MultiMoves presentMoves)
        {
            List<MultiMoves> resultMoves = new List<MultiMoves>();
            Dictionary<BonusIDType, BonusDetails> applicableBonuses = GetApplicableBonuses(presentMoves);
            foreach (var bonus in applicableBonuses.Values)
            {
                var territoriesToTake = GetMissingBonusTerritories(bonus, presentMoves);
                TakeTerritoriesTask takeTerritoriesTask = new TakeTerritoriesTask(REASON);
                MultiMoves calculatedMoves = takeTerritoriesTask.CalculateTakeTerritoriesMoves(territoriesToTake, presentMoves);
                if (calculatedMoves != null)
                {
                    resultMoves.Add(calculatedMoves);
                }
            }

            return resultMoves;
        }

        private List<TerritoryIDType> GetMissingBonusTerritories(BonusDetails bonus, MultiMoves presentMoves)
        {
            var bonusTerritories = bonus.Territories;
            var currentStatus = presentMoves.GetTerritoryStandingsAfterAllMoves();
            List<TerritoryIDType> territories = new List<TerritoryIDType>();
            foreach (var bonusTerritory in bonusTerritories)
            {
                if (currentStatus[bonusTerritory].OwnerPlayerID != GameState.MyPlayerId)
                {
                    territories.Add(bonusTerritory);
                }
            }
            return territories;
        }
        // TODO returns no results
        private Dictionary<BonusIDType, BonusDetails> GetApplicableBonuses(MultiMoves presentMoves)
        {
            var territoryStandings = presentMoves.GetTerritoryStandingsAfterAllMoves();
            Dictionary<BonusIDType, BonusDetails> allBonuses = GameState.Map.Bonuses;
            Dictionary<BonusIDType, BonusDetails> applicableBonuses = new Dictionary<BonusIDType, BonusDetails>();
            Dictionary<BonusIDType, BonusDetails> ownedBonuses = MapInformer.GetOwnedBonuses(territoryStandings, GameState.MyPlayerId);
            foreach (BonusDetails bonus in allBonuses.Values)
            {
                if (ownedBonuses.Keys.Contains(bonus.ID))
                {
                    continue;
                }
                List<TerritoryIDType> bonusTerritoriesAndNeighbors = MapInformer.GetBonusTerritoriesAndNeighbors(bonus);
                foreach (var territoryId in bonusTerritoriesAndNeighbors)
                {
                    if (territoryStandings[territoryId].OwnerPlayerID == GameState.MyPlayerId)
                    {
                        applicableBonuses.Add(bonus.ID, bonus);
                        break;
                    }
                }

            }
            return applicableBonuses;
        }

    }
}
