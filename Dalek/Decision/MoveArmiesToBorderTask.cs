using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarLight.Shared.AI.Dalek.Evaluation;

namespace WarLight.Shared.AI.Dalek.Utils
{
    class MoveArmiesToBorderTask
    {
        private static readonly String REASON = "MoveArmiesToBorderTask";

        public MultiMoves CalculateMoveArmiesToBorderMoves(MultiMoves presentMoves)
        {


            MultiMoves result = presentMoves.Clone();
            var nonOwnedTerritories = MapInformer.GetNonOwnedTerritories(result.GetTerritoryStandingsAfterAllMoves().Values.ToList(), GameState.MyPlayerId);
            var keys = new List<TerritoryIDType>();
            nonOwnedTerritories.ForEach(k => keys.Add(k.ID));
            var distances = MapInformer.GetDistancesFromTerritories(keys);
            foreach (TerritoryIDType territoryId in distances.Keys)
            {
                int distance = distances[territoryId];
                if (distance <= 1)
                {
                    continue;
                }
                var neighbors = MapInformer.GetNeighborTerritories(territoryId);
                var lowestDistanceNeighborValue = neighbors.Min(c => distances[c]);
                var lowestDistanceNeighbor = neighbors.Where(n => distances[n] == lowestDistanceNeighborValue).First();
                TerritoryStanding theTerritory = result.GetTerritoryStandingsAfterAllMoves()[territoryId];
                int armiesAvailable = theTerritory.NumArmies.ArmiesOrZero - theTerritory.ArmiesMarkedAsUsed.ArmiesOrZero - 1;
                if (armiesAvailable == 0)
                {
                    continue;
                }
                GameOrderAttackTransfer attackMove =
                    GameOrderAttackTransfer.Create
                    (GameState.MyPlayerId, territoryId, lowestDistanceNeighbor, AttackTransferEnum.AttackTransfer, new Armies(armiesAvailable), REASON);
                result.AddAttackOrder(attackMove);
            }
            return result;
        }
    }
}