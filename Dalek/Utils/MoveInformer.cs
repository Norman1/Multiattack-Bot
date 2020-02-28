using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.Shared.AI.Dalek.Utils
{
    public class MoveInformer
    {
        public static int GetNeededBreakArmies(int territoryArmies)
        {
            double offenceKillRate = GameState.GameSettings.OffenseKillRate;
            double defenceKillRage = GameState.GameSettings.DefenseKillRate;
            int neededArmies = (int)Math.Round(territoryArmies / offenceKillRate);
            if (neededArmies == 0)
            {
                neededArmies = 1;
            }
            else if (neededArmies == 1 && defenceKillRage >= 0.5)
            {
                neededArmies = 2;
            }

            return neededArmies;
        }

    }
}
