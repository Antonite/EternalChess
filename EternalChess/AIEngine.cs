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
        public ChessBoard ChessBoard;
        public DatabaseContoller DatabaseContoller;

        public AIEngine()
        {
            Initialize();
        }

        private void Initialize()
        {
            DatabaseContoller = new DatabaseContoller();
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
            // ?ChessBoard.ToString(),nq
            ChessBoard = new ChessBoard();
            var passedNodesWhite = new Dictionary<string, string>();
            var passedNodesBlack = new Dictionary<string, string>();
            var pastMoves = new List<string>();
            var colorToMove = "white";
            var moves = "";

//                                    var moveTests = new string[]
//                                    {
//                                        "e2e4", "e7e6", "d2d4", "d7d5", "b1c3", "g8f6", "e4d5", "e6d5", "g1f3", "f8d6", "c3b5", "e8g8", "b5d6", "d8d6", "f1e2", "b8c6", "e1g1", "h7h6", "e2d3", "c8g4", "c2c3", "a8e8", "h2h3", "g4h5", "c1e3", "f6e4", "g2g4", "h5g6", "f3h4", "e4f6", "h4g6", "e8e3", "f2e3", "d6g3", "g1h1", "g3h3", "h1g1", "h3g3", "g1h1", "g3h3", "h1g1", "h3g3", "g1h1", "g3h3", "h1g1", "h3g3", "g1h1", "f6g4"
//                                    };
//                                    var moveTestNum = 0;

            while (true)
            {
                var fen = ChessBoard.ToFen(colorToMove);
                var moveTime = 1000;

                var possibleMoves = ChessBoard.findAllMoves(colorToMove);
                if (possibleMoves.Count == 0)
                {
                    colorToMove = colorToMove == "white" ? "black" : "white";
                    EndGame(colorToMove, passedNodesWhite, passedNodesBlack);
                    break;
                }

                var bestFenMove = "";
                if (ChessBoard.remainingPieces <= 6)
                {
                    var result = AskFathom(fen);
                    if (!result.Equals("Error"))
                    {
                        EndGame(result, passedNodesWhite, passedNodesBlack);
                        break;
                    }
                }

                var fenMoves = DatabaseContoller.GetMovesById(fen);
                if (fenMoves != null)
                {
                    var topWinRatio = 0.0;
                    foreach (var move in fenMoves.Moves)
                    {
                        var winRatio = move.w/(move.w + move.l);
                        if (winRatio > 0.5 && winRatio > topWinRatio)
                        {
                            topWinRatio = winRatio;
                            bestFenMove = move.m;
                        }
                    }

                    if (bestFenMove == "") moveTime = 3000;
                }
                
                if (bestFenMove == "") bestFenMove = AskStockfish(moves, moveTime);

//                if (moveTestNum > moveTests.Length - 1)
//                {
//                    bestFenMove = AskStockfish(moves, moveTime);
//                }
//                else
//                {
//                    bestFenMove = moveTests[moveTestNum];
//                }
//                moveTestNum++;

                moves += " " + bestFenMove;
                var bestMove = ChessBoard.GetMoveFromString(bestFenMove);

                var currentMove = bestMove.stringMove;
                Console.WriteLine(currentMove);

                if (colorToMove.Equals("white"))
                {
                    if (!passedNodesWhite.ContainsKey(fen)) passedNodesWhite.Add(fen, bestFenMove);
                }
                else
                {
                    if (!passedNodesBlack.ContainsKey(fen)) passedNodesBlack.Add(fen, bestFenMove);
                }
                pastMoves.Add(fen);

                //perform move
                ChessBoard.performMove(bestMove);
                colorToMove = colorToMove == "white" ? "black" : "white";

                int moveCount = pastMoves.Count;
                if ((moveCount >= 12 && IsRepeat3Times(pastMoves.GetRange(moveCount - 12, 12))) || ChessBoard.remainingPieces == 2)
                {
                    EndGame("tie", passedNodesWhite, passedNodesBlack);
                    break;
                }
            }
        }

        private string AskStockfish(string moves, int moveTime)
        {
            var proc = new System.Diagnostics.Process
            {
                StartInfo =
                {
                    FileName = "cmd.exe",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    CreateNoWindow = true
                }
            };

            proc.Start();
            var write = proc.StandardInput;
            write.WriteLine("stockfish_8_x64.exe");
            write.WriteLine("setoption name Threads value 8");
            write.WriteLine("setoption name Hash value 6144");
            write.WriteLine("setoption name Ponder value False");
            write.WriteLine("setoption name SyzygyPath value D:\\syzygy\\wdl;D:\\syzygy\\dtz");
            write.WriteLine("position startpos moves " + moves);
            write.WriteLine("go movetime " + moveTime);

            while (true)
            {
                var line = proc.StandardOutput.ReadLine();
                if (line.Contains("bestmove"))
                {
                    return line.Split()[1];
                }
            }
        }

        private string AskFathom(string fenMove)
        {
            var proc = new System.Diagnostics.Process
            {
                StartInfo =
                {
                    FileName = "cmd.exe",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    CreateNoWindow = true
                }
            };

            proc.Start();
            var write = proc.StandardInput;
            write.WriteLine("fathom.exe --path=D:\\syzygy\\wdl;D:\\syzygy\\dtz \"" + fenMove + "\" --test");

            var lineCount = 0;
            var line = "";
            while (lineCount <= 4)
            {
                lineCount++;
                line = proc.StandardOutput.ReadLine();
            }

            switch (line)
            {
                case "1 - 0": return "white";
                case "0 - 1": return "black";
                case "1/2 - 1/2": return "tie";
                default: return "Error";
            }
        }

        

        private bool IsRepeat3Times(List<string> nodes)
        {
            return nodes[0].Equals(nodes[4]) && nodes[0].Equals(nodes[8]) &&
                    nodes[1].Equals(nodes[5]) && nodes[1].Equals(nodes[9]) &&
                    nodes[2].Equals(nodes[6]) && nodes[2].Equals(nodes[10]) &&
                    nodes[3].Equals(nodes[7]) && nodes[3].Equals(nodes[11]);
        }

        private void EndGame(string result, Dictionary<string, string> passedNodesWhite, Dictionary<string, string> passedNodesBlack)
        {
            var whiteAddToWin = 0.5;
            var whiteAddToLoss = 0.5;
            var blackAddToWin = 0.5;
            var blackAddToLoss = 0.5;

            switch (result)
            {
                case "tie":
                    Console.WriteLine("Game tied");
                    break;
                case "white":
                    Console.WriteLine("White wins!");
                    whiteAddToWin = 1;
                    whiteAddToLoss = 0;
                    blackAddToWin = 0;
                    blackAddToLoss = 1;
                    break;
                case "black":
                    Console.WriteLine("Black wins!");
                    whiteAddToWin = 0;
                    whiteAddToLoss = 1;
                    blackAddToWin = 1;
                    blackAddToLoss = 0;
                    break;
            }

            ProcessPassedNodes(passedNodesWhite, whiteAddToWin, whiteAddToLoss);
            ProcessPassedNodes(passedNodesBlack, blackAddToWin, blackAddToLoss);
        }


        private void ProcessPassedNodes(Dictionary<string, string> passedNodes, double addToWin, double addToLoss)
        {
            foreach (var node in passedNodes)
            {
                var state = DatabaseContoller.GetMovesById(node.Key);
                if (state == null)
                {
                    state = new BoardState() { Moves = new List<MoveStat>() { new MoveStat() { l = addToLoss, w = addToWin, m = node.Value } } };
                    DatabaseContoller.WriteToDatabase(node.Key, state);
                    continue;
                }

                var found = false;
                foreach (var stateMove in state.Moves)
                {
                    if (stateMove.m != node.Value) continue;
                    stateMove.l += addToLoss;
                    stateMove.w += addToWin;
                    found = true;
                    break;
                }

                if (!found) state.Moves.Add(new MoveStat() { l = addToLoss, w = addToWin, m = node.Value });

                DatabaseContoller.WriteToDatabase(node.Key, state);
            }
        }
    }
}
