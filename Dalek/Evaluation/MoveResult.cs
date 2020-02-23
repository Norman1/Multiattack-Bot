using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarLight.Shared.AI.Dalek.Utils;

namespace WarLight.Shared.AI.Dalek.Evaluation
{
    public class MoveResult
    {
        public static List<TerritoryStanding> GetMoveResult(List<TerritoryStanding> beginStandings, List<GameOrder> gameOrders)
        {
            List<TerritoryStanding> newStandings = new List<TerritoryStanding>();
            beginStandings.ForEach(o => newStandings.Add(o.Clone()));

            foreach (GameOrder gameOrder in gameOrders)
            {
                if (gameOrder is GameOrderDeploy)
                {
                    HandleDeployOrder(newStandings, (GameOrderDeploy)gameOrder);
                }
                else if (gameOrder is GameOrderAttackTransfer)
                {
                    HandleAttackOrder(newStandings, (GameOrderAttackTransfer)gameOrder);
                }
            }
            return newStandings;
        }

        private static void HandleDeployOrder(List<TerritoryStanding> territoryStandings, GameOrderDeploy deployOrder)
        {
            TerritoryStanding deployTerritory = MapInformer.GetTerritory(territoryStandings, deployOrder.DeployOn);
            int amountNewArmies = deployTerritory.NumArmies.ArmiesOrZero + deployOrder.NumArmies;
            Armies newArmies = new Armies(amountNewArmies);
            deployTerritory.NumArmies = newArmies;
        }

        private static void HandleAttackOrder(List<TerritoryStanding> territoryStandings, GameOrderAttackTransfer attackOrder)
        {
            // TODO
        }


    }
}
