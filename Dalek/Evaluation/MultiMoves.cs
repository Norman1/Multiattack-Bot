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

        public MultiMoves Clone()
        {
            MultiMoves clone = new MultiMoves();
            DeployMoves.ForEach(d => clone.DeployMoves.Add(d.Clone()));
            AttackMoves.ForEach(a => clone.AttackMoves.Add(a.Clone()));
            return clone;
        }

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

        public int GetCurrentDeployment()
        {
            int result = 0;
            DeployMoves.ForEach(d => result += d.NumArmies);
            return result;
        }

        public List<GameOrder> GetAllMoves()
        {
            List<GameOrder> outMoves = new List<GameOrder>();
            outMoves.AddRange(DeployMoves);
            outMoves.AddRange(AttackMoves);
            return outMoves;
        }

        //https://www.warzone.com/MultiPlayer?GameID=20960576 todo fehler, bot deployt 1 zu wenig im letzten zug
        public bool PumpArmies(TerritoryIDType pumpTarget, int amountArmies)
        {
            List<GameOrderAttackTransfer> pumpPath = GetPumpPath(pumpTarget);
            var endStandings = GetTerritoryStandingsAfterAllMoves();
            int stillAvailableDeployment = GameState.CurrentTurn().GetMyIncome() - GetCurrentDeployment();
            // check if attempt to pump to owned territory
            if (pumpPath.Count == 0)
            {
                var availableArmies = endStandings[pumpTarget].NumArmies.ArmiesOrZero - endStandings[pumpTarget].ArmiesMarkedAsUsed.ArmiesOrZero - 1;
                int missingArmies = amountArmies - availableArmies;
                if (missingArmies > 0 && stillAvailableDeployment >= missingArmies)
                {
                    GameOrderDeploy deployOrder = GameOrderDeploy.Create(GameState.MyPlayerId, missingArmies, pumpTarget, true);
                    AddDeployOrder(deployOrder);
                    availableArmies += deployOrder.NumArmies;
                }
                return availableArmies >= amountArmies;
            }
            var armiesAvailableForPump = endStandings[pumpPath[0].From].NumArmies.ArmiesOrZero - endStandings[pumpPath[0].From].ArmiesMarkedAsUsed.ArmiesOrZero - 1;
            // deploy if we have to
            if (armiesAvailableForPump < amountArmies)
            {
                int missingArmies = amountArmies - armiesAvailableForPump;
                if (missingArmies > 0 && stillAvailableDeployment >= missingArmies)
                {
                    GameOrderDeploy deployOrder = GameOrderDeploy.Create(GameState.MyPlayerId, missingArmies, pumpPath[0].From, true);
                    AddDeployOrder(deployOrder);
                    armiesAvailableForPump += deployOrder.NumArmies;
                }
            }
            int pumpArmies = Math.Max(0, Math.Min(amountArmies, armiesAvailableForPump));
            foreach (GameOrderAttackTransfer attackOrder in pumpPath)
            {
                attackOrder.NumArmies = new Armies(attackOrder.NumArmies.ArmiesOrZero + pumpArmies);
            }
            return pumpArmies == amountArmies;
        }

        // Returns an already calculated attack path from an owned territory to a territory in the path to which we want to pump extra deployment.
        private List<GameOrderAttackTransfer> GetPumpPath(TerritoryIDType territoryToPumpTo)
        {
            // we can't pump from transfer moves
            GameStanding gameStanding = GameState.CurrentTurn().LatestTurnStanding;
            List<TerritoryIDType> ownedTerritories = gameStanding.Territories.Values.Where(o => o.OwnerPlayerID == GameState.MyPlayerId).Select(o => o.ID).ToList();
            var attackMoves = AttackMoves.Where(o => !ownedTerritories.Contains(o.To)).ToList();

            List<GameOrderAttackTransfer> pumpPath = new List<GameOrderAttackTransfer>();
            TerritoryIDType currentPumpToTerritory = territoryToPumpTo;
            while (true)
            {
                if (ownedTerritories.Contains(currentPumpToTerritory))
                {
                    break;
                }
                GameOrderAttackTransfer pumpOrder = attackMoves.Find(o => o.To == currentPumpToTerritory);
                pumpPath.Insert(0, pumpOrder);
                currentPumpToTerritory = pumpOrder.From;
            }
            return pumpPath;
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
