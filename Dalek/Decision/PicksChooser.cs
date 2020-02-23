using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.Shared.AI.Dalek.Decision
{
    public class PicksChooser
    {

        public List<TerritoryIDType> GetPicks()
        {
            int maxPicks = GameState.GameSettings.LimitDistributionTerritories;
            List<TerritoryIDType> availableTerritories = GameState.DistributionStanding.Territories.Values.Where(o => o.OwnerPlayerID == TerritoryStanding.AvailableForDistribution).Select(o => o.ID).ToList();
            List<TerritoryIDType> picks = new List<TerritoryIDType>();
            for (int i = 0; i < maxPicks; i++)
            {
                picks.Add(availableTerritories[i]);
            }
            return picks;
        }
    }
}
