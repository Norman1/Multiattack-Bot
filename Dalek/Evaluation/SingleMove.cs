using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarLight.Shared.AI.Dalek.Utils;

namespace WarLight.Shared.AI.Dalek.Evaluation
{
    public class SingleMove
    {
        public GameOrder Move;
        public List<TerritoryStanding> BeforeStandings;
        public List<TerritoryStanding> AfterStandings;

        public SingleMove(List<TerritoryStanding> beforeStandings, GameOrder move)
        {
            BeforeStandings = beforeStandings;
            Move = move;
            AfterStandings = GetMoveResult(beforeStandings, move);
        }

        private List<TerritoryStanding> GetMoveResult(List<TerritoryStanding> beginStandings, GameOrder gameOrder)
        {
            List<TerritoryStanding> newStandings = new List<TerritoryStanding>();
            beginStandings.ForEach(o => newStandings.Add(o.Clone()));
            if (gameOrder is GameOrderDeploy)
            {
                HandleDeployOrder(newStandings, (GameOrderDeploy)gameOrder);
            }
            else if (gameOrder is GameOrderAttackTransfer)
            {
                HandleAttackOrder(newStandings, (GameOrderAttackTransfer)gameOrder);
            }
            return newStandings;
        }

        private void HandleDeployOrder(List<TerritoryStanding> territoryStandings, GameOrderDeploy deployOrder)
        {
            TerritoryStanding deployTerritory = MapInformer.GetTerritory(territoryStandings, deployOrder.DeployOn);
            int amountNewArmies = deployTerritory.NumArmies.ArmiesOrZero + deployOrder.NumArmies;
            Armies newArmies = new Armies(amountNewArmies);
            deployTerritory.NumArmies = newArmies;
        }

        private void HandleAttackOrder(List<TerritoryStanding> territoryStandings, GameOrderAttackTransfer attackOrder)
        {
            // TODO
        }



    }
}
