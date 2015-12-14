using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EternalChess
{
    class EternalTree
    {
        public long wins;
        public long losses;
        public long winChance;
        public Move move;
        public List<EternalTree> responses;

        public EternalTree(Move move, List<EternalTree> responses)
        {
            this.move = move;
            this.responses = responses;
            wins = 1;
            losses = 0;
            winChance = 100;
        }

        public EternalTree()
        {
            move = null;
            this.responses = null;
            wins = 1;
            losses = 0;
            winChance = 100;
            initialize();
        }

        private void initialize()
        {
            List<EternalTree> responses = new List<EternalTree>();

            responses.Add(new EternalTree(new Move(new Location(1, 0), new Location(3, 0), "white", "Pawn"), null));
            responses.Add(new EternalTree(new Move(new Location(1, 1), new Location(3, 1), "white", "Pawn"), null));
            responses.Add(new EternalTree(new Move(new Location(1, 2), new Location(3, 2), "white", "Pawn"), null));
            responses.Add(new EternalTree(new Move(new Location(1, 3), new Location(3, 3), "white", "Pawn"), null));
            responses.Add(new EternalTree(new Move(new Location(1, 4), new Location(3, 4), "white", "Pawn"), null));
            responses.Add(new EternalTree(new Move(new Location(1, 5), new Location(3, 5), "white", "Pawn"), null));
            responses.Add(new EternalTree(new Move(new Location(1, 6), new Location(3, 6), "white", "Pawn"), null));
            responses.Add(new EternalTree(new Move(new Location(1, 7), new Location(3, 7), "white", "Pawn"), null));
            responses.Add(new EternalTree(new Move(new Location(1, 0), new Location(2, 0), "white", "Pawn"), null));
            responses.Add(new EternalTree(new Move(new Location(1, 1), new Location(2, 1), "white", "Pawn"), null));
            responses.Add(new EternalTree(new Move(new Location(1, 2), new Location(2, 2), "white", "Pawn"), null));
            responses.Add(new EternalTree(new Move(new Location(1, 3), new Location(2, 3), "white", "Pawn"), null));
            responses.Add(new EternalTree(new Move(new Location(1, 4), new Location(2, 4), "white", "Pawn"), null));
            responses.Add(new EternalTree(new Move(new Location(1, 5), new Location(2, 5), "white", "Pawn"), null));
            responses.Add(new EternalTree(new Move(new Location(1, 6), new Location(2, 6), "white", "Pawn"), null));
            responses.Add(new EternalTree(new Move(new Location(1, 7), new Location(2, 7), "white", "Pawn"), null));
            responses.Add(new EternalTree(new Move(new Location(0, 1), new Location(2, 2), "white", "Knight"), null));
            responses.Add(new EternalTree(new Move(new Location(0, 6), new Location(2, 5), "white", "Knight"), null));
            responses.Add(new EternalTree(new Move(new Location(7, 1), new Location(5, 2), "black", "Knight"), null));
            responses.Add(new EternalTree(new Move(new Location(7, 6), new Location(5, 5), "black", "Knight"), null));

            this.responses = responses;
        }

        private void updateWinChance()
        {
            winChance = wins / losses;
        }

        private void killNode()
        {
            wins = 0;
            winChance = 0;
            losses = 1;
        }

        public void populateResponses(List<Move> moves)
        {
            responses = new List<EternalTree>();
            foreach (Move move in moves)
            {
                responses.Add(new EternalTree(move, null));
            }
        }
    }
}
