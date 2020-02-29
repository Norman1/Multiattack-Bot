using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.Shared.AI.Dalek.Utils
{
    public class AttackInformer
    {

        public static int GetNeededBreakArmies(int defendingArmies)
        {
            double offenceKillRate = GameState.GameSettings.OffenseKillRate;
            double defenceKillRate = GameState.GameSettings.DefenseKillRate;
            int neededAttackArmies = (int)Math.Round(defendingArmies / offenceKillRate);
            int defenderKills = (int)Math.Round(defendingArmies * defenceKillRate);
            if (neededAttackArmies == 0)
            {
                neededAttackArmies = 1;
            }

            else if (defenderKills >= neededAttackArmies)
            {
                neededAttackArmies++;
            }
            return neededAttackArmies;
        }

    }
}
