using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarLight.Shared.AI.Dalek.Utils;

namespace WarLight.Shared.AI.Dalek.Evaluation
{
    public class MultiMoves
    {
        public List<GameOrderDeploy> DeployMoves = new List<GameOrderDeploy>();
        public List<GameOrderAttackTransfer> AttackMoves = new List<GameOrderAttackTransfer>();

        public void AddDeployOrder(GameOrderDeploy deployOrder)
        {
            GameOrderDeploy alreadyPresentDeployOrder = DeployMoves.Where(o => o.DeployOn == deployOrder.DeployOn).FirstOrDefault();
            if (alreadyPresentDeployOrder == null)
            {
                DeployMoves.Add(deployOrder);
            }
            else
            {
                alreadyPresentDeployOrder.NumArmies += deployOrder.NumArmies;
            }
        }

        public void AddAttackOrder(GameOrderAttackTransfer attackOrder)
        {
            AttackMoves.Add(attackOrder);
        }

        public List<GameOrder> GetAllMoves()
        {
            List<GameOrder> outMoves = new List<GameOrder>();
            outMoves.AddRange(DeployMoves);
            outMoves.AddRange(AttackMoves);
            return outMoves;

        }


        public Dictionary<TerritoryIDType, TerritoryStanding> GetTerritoryStandingsAfterAllMoves()
        {
            Dictionary<TerritoryIDType, TerritoryStanding> beginStandings = GameState.CurrentTurn().LatestTurnStanding.Territories;
            Dictionary<TerritoryIDType, TerritoryStanding> endStandings = new Dictionary<TerritoryIDType, TerritoryStanding>();
            // init
            foreach (TerritoryIDType territoryId in beginStandings.Keys)
            {
                endStandings.Add(territoryId, beginStandings[territoryId].Clone());
            }
            // handle deployments
            foreach (GameOrderDeploy deployMove in DeployMoves)
            {
                endStandings[deployMove.DeployOn].NumArmies = new Armies(endStandings[deployMove.DeployOn].NumArmies.ArmiesOrZero + deployMove.NumArmies);
            }
            // handle attack and transfer moves
            foreach (GameOrderAttackTransfer attackTransferMove in AttackMoves)
            {
                int attackingArmies = attackTransferMove.NumArmies.ArmiesOrZero;
                if (endStandings[attackTransferMove.To].OwnerPlayerID == GameState.MyPlayerId)
                {
                    // if transfer remove armies from start and add armies to too
                    endStandings[attackTransferMove.From].NumArmies = new Armies(endStandings[attackTransferMove.From].NumArmies.ArmiesOrZero - attackingArmies);
                    endStandings[attackTransferMove.To].NumArmies = new Armies(endStandings[attackTransferMove.To].NumArmies.ArmiesOrZero + attackingArmies);
                    endStandings[attackTransferMove.To].ArmiesMarkedAsUsed = new Armies(endStandings[attackTransferMove.To].ArmiesMarkedAsUsed.ArmiesOrZero + attackingArmies);
                }
                else
                {
                    // if attack the result needs to get calculated
                    TerritoryStanding from = endStandings[attackTransferMove.From];
                    TerritoryStanding to = endStandings[attackTransferMove.To];
                    AttackOutcome attackOutcome = new AttackOutcome(from.NumArmies.ArmiesOrZero, attackingArmies, to.NumArmies.ArmiesOrZero);
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
            return endStandings;
        }
    }
}
