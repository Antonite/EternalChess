using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EternalChess
{
    class AIEngine
    {
        public ChessBoard chessBoard;
//        public EternalTree eternalTree;

        public AIEngine()
        {
            initialize();
        }

        private void initialize()
        {
//            eternalTree = new EternalTree();
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
            // ?chessBoard.ToString(),nq
            //            EternalTree currentTree = eternalTree;
            chessBoard = new ChessBoard();
            List<string> passedNodes = new List<string>();
            string colorToMove = "white";
            var moves = "";
//
//                        var moveTests = new string[]
//                        {
//                            "e2e4", "e7e5", "g1f3", "b8c6", "f1b5", "a7a6", "b5c6", "d7c6", "e1g1", "f8d6", "d2d4", "e5d4", "d1d4", "f7f6", "f1e1", "g8e7", "e4e5", "f6e5", "f3e5", "e8g8", "b1d2", "c8e6", "d2f3", "d8e8", "c1d2", "e7g6", "e5c4", "a8d8", "c4d6", "d8d6", "d4e3", "f8f3", "g2f3", "e8f7", "e3e4", "e6d7", "a1d1", "d7f5", "e4e2", "f5c2", "d1c1", "g6h4", "e2e8"
//                        };
//                        var moveTestNum = 0;

            while (true)
            {
//                if (currentTree.responses == null) currentTree.populateResponses(chessBoard.findAllMoves(colorToMove));
                if (chessBoard.findAllMoves(colorToMove).Count == 0)
                {
                    colorToMove = colorToMove == "white" ? "black" : "white";
                    endGame(colorToMove, passedNodes);
                    break;
                }

                var bestStockfishMove = askStockfish(moves);
//                if (moveTestNum > moveTests.Length - 1)
//                {
//                    // success
//                }
//                var bestStockfishMove = moveTests[moveTestNum];
//                moveTestNum++;
                moves += " " + bestStockfishMove;
                var bestMove = chessBoard.GetMoveFromString(bestStockfishMove);

//                currentTree.FindMove(bestMove);

                string currentMove = bestMove.stringMove;
                Console.WriteLine(currentMove);

                //perform move
                chessBoard.performMove(bestMove);
                passedNodes.Add(bestStockfishMove);
                colorToMove = colorToMove == "white" ? "black" : "white";
//                currentTree = currentTree.FindMove(bestMove);

                int moveCount = passedNodes.Count;
                if ((moveCount >= 12 && isRepeat3Times(passedNodes.GetRange(moveCount - 12, 12))) || chessBoard.remainingPieces == 2)
                {
                    endGame("tie", passedNodes);
                    break;
                }
            }
        }

        private string askStockfish(string moves)
        {
            var proc = new System.Diagnostics.Process
            {
                StartInfo =
                {
                    FileName = "cmd.exe",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true
                }
            };

            proc.Start();
            var write = proc.StandardInput;
            write.WriteLine("stockfish_8_x64.exe");
            write.WriteLine("setoption name Threads value 8");
            write.WriteLine("setoption name SyzygyPath value D:\\syzygy\\dtz");
            write.WriteLine("position startpos moves " + moves);
            write.WriteLine("go movetime 100");

            while (true)
            {
                var line = proc.StandardOutput.ReadLine();
                if (line.Contains("bestmove"))
                {
                    return line.Split()[1];
                }
            }
        }

        private int FindResponse(Move move, ref List<EternalTree> responses)
        {
            for (int i = 0; i < responses.Count; i++)
            {
                if (responses[i].move.stringMove.Equals(move.stringMove))
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
//                    tieGame(ref eternalTree, passedNodes);
                    Console.WriteLine("Game tied");
                    break;
                case "white":
//                    winGame(ref eternalTree, passedNodes, "white");
                    Console.WriteLine("White wins!");
                    break;
                case "black":
//                    winGame(ref eternalTree, passedNodes, "black");
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
