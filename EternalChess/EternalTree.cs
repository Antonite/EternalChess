using System.Collections.Generic;

namespace EternalChess
{
    class EternalTree
    {
        public double wins;
        public double losses;
        public Move move;
        public List<EternalTree> responses;

        public EternalTree(Move move, List<EternalTree> responses)
        {
            this.move = move;
            this.responses = responses;
            wins = 1;
            losses = 0;
        }

        public EternalTree()
        {
            move = null;
            this.responses = null;
            wins = 1;
            losses = 0;
            initialize();
        }

        public void populateResponses(List<Move> moves)
        {
            responses = new List<EternalTree>();
            foreach (Move move in moves)
            {
                responses.Add(new EternalTree(move, null));
            }
        }

        public int bestResponse()
        {
            double bestWinChance = responses[0].getWinChance();
            int currentWinner = 0;
            for(int i = 1; i<responses.Count; i++)
            {
                double winChance = responses[i].getWinChance();
                if (winChance > bestWinChance)
                {
                    bestWinChance = winChance;
                    currentWinner = i;
                }
            }
            return currentWinner;
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
            responses.Add(new EternalTree(new Move(new Location(0, 1), new Location(0, 2), "white", "Knight"), null));
            responses.Add(new EternalTree(new Move(new Location(0, 6), new Location(2, 7), "white", "Knight"), null));

            this.responses = responses;
        }

        public double getWinChance()
        {
            return wins / (wins + losses);
        }

        private void killNode()
        {
            wins = 0;
            losses = 1;
        }

        public EternalTree FindMove(Move move)
        {
            foreach (var eternalTree in responses)
            {
                if (eternalTree.move.stringMove.Equals(move.stringMove)) return eternalTree;
            }

            return null;
        }

    }
}
