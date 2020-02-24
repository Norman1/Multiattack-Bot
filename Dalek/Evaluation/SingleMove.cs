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
        public Dictionary<TerritoryIDType, TerritoryStanding> BeforeStandings;
        public Dictionary<TerritoryIDType, TerritoryStanding> AfterStandings;

        public SingleMove(Dictionary<TerritoryIDType, TerritoryStanding> beforeStandings, GameOrder move)
        {
            BeforeStandings = beforeStandings;
            Move = move;
            AfterStandings = GetMoveResult(beforeStandings, move);
        }

        private Dictionary<TerritoryIDType, TerritoryStanding> GetMoveResult(Dictionary<TerritoryIDType, TerritoryStanding> beginStandings, GameOrder gameOrder)
        {
            Dictionary<TerritoryIDType, TerritoryStanding> newStandings = new Dictionary<TerritoryIDType, TerritoryStanding>();
            foreach (var x in beginStandings)
            {
                newStandings.Add(x.Key, x.Value.Clone());
            }

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

        private void HandleDeployOrder(Dictionary<TerritoryIDType, TerritoryStanding> territoryStandings, GameOrderDeploy deployOrder)
        {
            TerritoryStanding deployTerritory = territoryStandings[deployOrder.DeployOn];
            int amountNewArmies = deployTerritory.NumArmies.ArmiesOrZero + deployOrder.NumArmies;
            Armies newArmies = new Armies(amountNewArmies);
            deployTerritory.NumArmies = newArmies;
        }

        private void HandleAttackOrder(Dictionary<TerritoryIDType, TerritoryStanding> territoryStandings, GameOrderAttackTransfer attackOrder)
        {
            // TODO
        }



    }
}
