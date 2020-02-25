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
                HandleMoveOrder(newStandings, (GameOrderAttackTransfer)gameOrder);
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

        private void HandleMoveOrder(Dictionary<TerritoryIDType, TerritoryStanding> territoryStandings, GameOrderAttackTransfer attackOrder)
        {
            var from = territoryStandings[attackOrder.From];
            var to = territoryStandings[attackOrder.To];
            bool isTransfer = from.OwnerPlayerID == to.OwnerPlayerID;
            from.TerritoriesMarkedAsUsed.Add(to.ID);
            if (isTransfer)
            {
                HandleTransferOrder(territoryStandings, attackOrder);
            }
            else
            {
                HandleAttackOrder(territoryStandings, attackOrder);
            }
        }

        private void HandleTransferOrder(Dictionary<TerritoryIDType, TerritoryStanding> territoryStandings, GameOrderAttackTransfer attackOrder)
        {
            var from = territoryStandings[attackOrder.From];
            var to = territoryStandings[attackOrder.To];
            from.NumArmies = new Armies(from.NumArmies.ArmiesOrZero - attackOrder.NumArmies.ArmiesOrZero);
            to.NumArmies = new Armies(to.NumArmies.ArmiesOrZero + attackOrder.NumArmies.ArmiesOrZero);
            to.ArmiesMarkedAsUsed = new Armies(to.ArmiesMarkedAsUsed.ArmiesOrZero + attackOrder.NumArmies.ArmiesOrZero);
        }

        private void HandleAttackOrder(Dictionary<TerritoryIDType, TerritoryStanding> territoryStandings, GameOrderAttackTransfer attackOrder)
        {
            var from = territoryStandings[attackOrder.From];
            var to = territoryStandings[attackOrder.To];
            int armiesInAttackingTerritory = from.NumArmies.ArmiesOrZero;
            int defendingArmies = to.NumArmies.ArmiesOrZero;
            int armies = attackOrder.NumArmies.ArmiesOrZero;
            AttackOutcome attackOutcome = new AttackOutcome(armiesInAttackingTerritory, armies, defendingArmies);
            from.NumArmies = new Armies(attackOutcome.RemainingArmiesAttackingTerritory);
            if (attackOutcome.IsTerritoryTaken)
            {
                to.OwnerPlayerID = GameState.MyPlayerId;
                to.NumArmies = new Armies(attackOutcome.NewArmiesDefendingTerritoryAttacker);
            }
            else
            {
                to.NumArmies = new Armies(attackOutcome.RemainingArmiesDefendingTerritoryDefender);
            }
        }



    }
}
