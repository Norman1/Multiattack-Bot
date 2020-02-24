using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.Shared.AI.Dalek.Utils
{
    public class AttackOutcome
    {
        public Boolean IsTerritoryTaken;
        public int AttackerLostArmies;
        public int AttackerKilledArmies;
        public int RemainingArmiesAttackingTerritory;
        public int RemainingArmiesDefendingTerritoryDefender;
        public int NewArmiesDefendingTerritoryAttacker;


        public AttackOutcome(int armiesInAttackingTerritory, int attackingArmies, int defendingArmies)
        {
            double offenceKillRate = GameState.GameSettings.OffenseKillRate;
            double defenceKillRate = GameState.GameSettings.DefenseKillRate;
            int attackerArmiesKill = Convert.ToInt32(Math.Round(attackingArmies * offenceKillRate));
            int defenderArmiesKill = Convert.ToInt32(Math.Round(defendingArmies * defenceKillRate));


            IsTerritoryTaken = attackerArmiesKill >= defendingArmies && !(defenderArmiesKill >= attackingArmies);

            AttackerLostArmies = Math.Min(defenderArmiesKill, attackingArmies);

            int notUsedArmies = armiesInAttackingTerritory - attackingArmies;
            RemainingArmiesAttackingTerritory = notUsedArmies;
            if (!IsTerritoryTaken)
            {
                RemainingArmiesAttackingTerritory += attackingArmies - AttackerLostArmies;
            }

            AttackerKilledArmies = Math.Min(attackerArmiesKill, defendingArmies);
            // edge case since for example in case of a 1v1 attack the defender keeps his 1 army
            if (IsTerritoryTaken && AttackerKilledArmies >= defendingArmies)
            {
                AttackerKilledArmies--;
            }

            RemainingArmiesDefendingTerritoryDefender = defendingArmies - this.AttackerKilledArmies;

            if (IsTerritoryTaken)
            {
                NewArmiesDefendingTerritoryAttacker = attackingArmies - this.AttackerLostArmies;
            }
            else
            {
                NewArmiesDefendingTerritoryAttacker = 0;
            }
        }

    }
}
