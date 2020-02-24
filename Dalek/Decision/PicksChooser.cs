using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarLight.Shared.AI.Dalek.Utils;

namespace WarLight.Shared.AI.Dalek.Decision
{
    public class PicksChooser
    {

        public List<TerritoryIDType> GetPicks()
        {
            int maxPicks = GameState.GameSettings.LimitDistributionTerritories;
            int neededPicks = maxPicks * 2;
            List<TerritoryIDType> availableTerritories = GameState.DistributionStanding.Territories.Values.Where(o => o.OwnerPlayerID == TerritoryStanding.AvailableForDistribution).Select(o => o.ID).ToList();
            List<TerritoryIDType> picks = RandomUtils.ChooseRandom(availableTerritories, neededPicks);
            return picks;
        }
    }
}
