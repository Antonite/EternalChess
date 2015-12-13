using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EternalChess
{
    class EternalTree
    {
        long wins;
        long losses;
        long winChance;
        string move;
        List<EternalTree> respones;

        public EternalTree(string move, List<EternalTree> respones)
        {
            this.move = move;
            this.respones = respones;
            wins = 1;
            losses = 0;
            winChance = 100;
        }

        public void updateWinChance()
        {
            winChance = wins / losses;
        }

        public void killNode()
        {
            wins = 0;
            winChance = 0;
            losses = 1;
        }
    }
}
