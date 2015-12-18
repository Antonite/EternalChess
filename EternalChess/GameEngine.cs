using System;
using System.Collections.Generic;
using System.Linq;

namespace EternalChess
{
    class GameEngine
    {
        public ChessBoard chessBoard;
        public EternalTree eternalTree;
        public List<Move> setupMoves;

        public GameEngine()
        {
            initialize();
        }

        private void initialize()
        {
            eternalTree = new EternalTree();
            setupMoves = new List<Move>();
            Console.WriteLine("Please input setup moves ('done' to continue)>  <int>[from row], <int>[from column]," +
                    "<int>[to row], <int>[to column], <string>[piece color], <string>[piece type]");
            char[] delimiterChars = { ',', ' ' };
            while (true)
            {
                string moveString = Console.ReadLine();
                if (moveString == "done") break;
                string[] moveParams = moveString.Split(delimiterChars);
                setupMoves.Add(new Move(new Location(int.Parse(moveParams[0]), int.Parse(moveParams[1])),
                                        new Location(int.Parse(moveParams[2]), int.Parse(moveParams[3])),
                                        moveParams[4].ToLower(), moveParams[5]));
            }
        }

        public void run()
        {
            while (true)
            {
                runSingleGame();
            }
        }

        private void runSingleGame()
        {
            EternalTree currentTree = eternalTree;
            chessBoard = new ChessBoard();
            List<string> passedNodes = new List<string>();
            string colorToMove = "white";

            //perform setup moves
            foreach(Move move in setupMoves)
            {
                if (currentTree.responses == null) currentTree.populateResponses(chessBoard.findAllMoves(colorToMove));
                

                string currentMove = move.stringMove;
                Console.WriteLine(currentMove);

                //perform move
                chessBoard.performMove(move);
                passedNodes.Add(currentMove);
                colorToMove = colorToMove == "white" ? "black" : "white";
                currentTree = currentTree.responses[FindResponse(move, ref currentTree.responses)];
            }

            while (true)
            {
                if (currentTree.responses == null) currentTree.populateResponses(chessBoard.findAllMoves(colorToMove));
                if (currentTree.responses.Count == 0)
                {
                    colorToMove = colorToMove == "white" ? "black" : "white";
                    endGame(colorToMove, passedNodes);
                    break;
                }

                int bestResponse = currentTree.bestResponse();
                Move bestMove = currentTree.responses[bestResponse].move;
                string currentMove = bestMove.stringMove;
                Console.WriteLine(currentMove);

                //perform move
                chessBoard.performMove(bestMove);
                passedNodes.Add(currentMove);
                colorToMove = colorToMove == "white" ? "black" : "white";
                currentTree = currentTree.responses[bestResponse];

                int moveCount = passedNodes.Count;
                if (moveCount >= 12 && isRepeat3Times(passedNodes.GetRange(moveCount - 12, 12)))
                {
                    endGame("tie", passedNodes);
                    break;
                }
            }
        }

        private int FindResponse(Move move, ref List<EternalTree> responses)
        {
            for(int i = 0; i<responses.Count; i++)
            {
                if(responses[i].move.stringMove.Equals(move.stringMove))
                {
                    return i;
                }
            }
            Console.Write("Error identifying setup respose.");
            return 0;
        }

        private bool isRepeat3Times(List<string> nodes)
        {
            return nodes[0].Equals(nodes[4]) && nodes[0].Equals(nodes[8]) &&
                    nodes[1].Equals(nodes[5]) && nodes[1].Equals(nodes[9]) &&
                    nodes[2].Equals(nodes[6]) && nodes[2].Equals(nodes[10]) &&
                    nodes[3].Equals(nodes[7]) && nodes[3].Equals(nodes[11]);
        }

        private void endGame(string result, List<string> passedNodes)
        {
            List<int> pathing = new List<int>();

            switch (result)
            {
                case "tie":
                    tieGame(ref eternalTree, passedNodes);
                    Console.WriteLine("Game tied");
                    break;
                case "white":
                    winGame(ref eternalTree, passedNodes, "white");
                    Console.WriteLine("White wins!");
                    break;
                case "black":
                    winGame(ref eternalTree, passedNodes, "black");
                    Console.WriteLine("Black wins!");
                    break;
                default: break;
            }
        }

        private void tieGame(ref EternalTree tree, List<string> passedNodes)
        {
            if (passedNodes.Count == 0) return;
            else
            {
                for (int i = 0; i < tree.responses.Count; i++)
                {
                    if (tree.responses[i].move.stringMove == passedNodes.First())
                    {
                        tree.responses[i].wins += 0.5;
                        tree.responses[i].losses += 0.5;
                        passedNodes.RemoveAt(0);
                        EternalTree updatedTree = tree.responses[i];
                        tieGame(ref updatedTree, passedNodes);
                        tree.responses[i] = updatedTree;
                        break;
                    }
                }
            }
        }

        private void winGame(ref EternalTree tree, List<string> passedNodes, string winnerColor)
        {
            if (passedNodes.Count == 1) return;
            else
            {
                for (int i = 0; i < tree.responses.Count; i++)
                {
                    if (tree.responses[i].move.stringMove == passedNodes.First())
                    {
                        tree.responses[i].wins += tree.responses[i].move.color == winnerColor ? 1 : 0;
                        tree.responses[i].losses += tree.responses[i].move.color == winnerColor ? 0 : 1;
                        passedNodes.RemoveAt(0);
                        EternalTree updatedTree = tree.responses[i];
                        winGame(ref updatedTree, passedNodes, winnerColor);
                        tree.responses[i] = updatedTree;
                        break;
                    }
                }
            }
        }




}
}
