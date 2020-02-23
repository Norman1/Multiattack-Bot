using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.Shared.AI.Dalek.Evaluation
{
    public class MultiMoves
    {
        public List<SingleMove> Moves = new List<SingleMove>();

        public void AddMove(SingleMove move)
        {
            Moves.Add(move);
        }

        public List<GameOrder> GetAllGameOrders()
        {
            return Moves.Select(o => o.Move).ToList();
        }

    }

}
