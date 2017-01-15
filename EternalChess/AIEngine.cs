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
            // ?chessBoard.ToString(),nq
            ChessBoard = new ChessBoard();
            var passedNodesWhite = new Dictionary<string, string>();
            var passedNodesBlack = new Dictionary<string, string>();
            var pastMoves = new List<string>();
            var colorToMove = "white";
            var moves = "";

//                        var moveTests = new string[]
//                        {
//                            "g1f3", "g8f6", "d2d4", "e7e6", "c1f4", "f8e7", "e2e3", "e8g8", "f1d3", "d7d5", "b1d2", "f6h5", "f4e5", "b8c6", "h2h3", "c6e5", "d4e5", "g7g6", "e1g1", "b7b6", "d3b5", "c8b7", "f3d4", "c7c5", "d4c6", "d8d7", "c6e7", "d7e7", "b5e2", "h5g7", "d2f3", "f7f6", "c2c3", "e7c7", "e5f6", "f8f6", "c3c4", "d5d4", "e3d4", "a8d8", "d4d5", "e6d5", "c4d5", "b7d5", "d1c2", "d5f3", "e2f3", "g7e6", "a1e1", "e6d4", "c2c4", "g8h8", "f3d1", "b6b5", "c4c3", "a7a5", "e1e4", "a5a4", "f1e1", "c7d6", "e4e5", "c5c4", "d1g4", "h8g8", "h3h4", "f6f4", "f2f3", "h7h5", "g4e6", "g8h7", "c3e3", "d4e6", "e5e6", "d6d4", "e6b6", "d8e8", "e3d4", "e8e1", "g1f2", "f4d4", "f2e1", "d4h4", "b6b5", "h4h1", "e1f2", "h1a1", "a2a3", "c4c3", "b2c3", "a1a3", "b5a5", "a3a2", "f2g3", "h7g7", "c3c4", "a4a3", "c4c5", "g7f6", "g3f4", "f6e6", "f4e4", "e6d7", "a5a6", "g6g5", "e4f5", "d7c7", "f5g6", "h5h4", "g6f5", "a2g2", "a6a3", "h4h3", "a3a6", "h3h2", "a6h6", "c7d7", "f5e5", "g2f2", "e5f5", "f2f3", "f5g5", "f3f2", "g5g4", "f2c2", "g4g3", "c2c3", "g3f4", "c3c4", "f4g3", "c4c2", "h6h2", "c2c5", "g3f3", "d7e6", "f3e4", "c5c8", "h2h6", "e6f7", "h6h1", "c8a8", "h1f1", "f7e6", "e4d4", "a8d8", "d4c3", "e6e5", "f1e1", "e5f4", "e1g1", "f4f3", "g1f1", "f3e2", "f1g1", "d8f8", "c3d4", "f8a8", "g1b1", "e2f3", "d4e5", "a8a2", "b1b4", "a2a3", "b4b6", "f3g4", "e5e4", "a3a4", "e4e3", "a4a3", "e3d2", "a3a5", "d2c3", "a5f5", "c3d4", "f5f7", "d4e3", "f7a7", "b6c6", "g4h5", "e3d4", "h5g4", "d4e3", "g4f5", "c6c8", "f5e6"
//                        };
//                        var moveTestNum = 0;

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

                var fenMoves = DatabaseContoller.GetMovesById(fen);
                var bestFenMove = "";

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
                
                if (bestFenMove == "") bestFenMove = askStockfish(moves, moveTime);
                
//                if (moveTestNum > moveTests.Length - 1)
//                {
//                    // success
//                }
//                bestFenMove = moveTests[moveTestNum];
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
                if ((moveCount >= 12 && isRepeat3Times(pastMoves.GetRange(moveCount - 12, 12))) || ChessBoard.remainingPieces == 2)
                {
                    EndGame("tie", passedNodesWhite, passedNodesBlack);
                    break;
                }
            }
        }

        private string askStockfish(string moves, int moveTime)
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
            write.WriteLine("setoption name Hash value 6144");
            write.WriteLine("setoption name SyzygyPath value D:\\syzygy\\wdl");
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

        private bool isRepeat3Times(List<string> nodes)
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

            foreach (var node in passedNodesWhite)
            {
                if (Utils.IsSixPieceOrLess(node.Key)) continue;

                var state = DatabaseContoller.GetMovesById(node.Key);
                if (state == null)
                {
                    state = new BoardState(){ Moves = new List<MoveStat>(){ new MoveStat() { l = whiteAddToLoss, w = whiteAddToWin, m = node.Value }}};
                    DatabaseContoller.WriteToDatabase(node.Key, state);
                    continue;
                }

                var found = false;
                foreach (var stateMove in state.Moves)
                {
                    if (stateMove.m != node.Value) continue;
                    stateMove.l += whiteAddToLoss;
                    stateMove.w += whiteAddToWin;
                    found = true;
                    break;
                }

                if (!found) state.Moves.Add(new MoveStat() {l = whiteAddToLoss, w = whiteAddToWin, m = node.Value});

                DatabaseContoller.WriteToDatabase(node.Key, state);
            }

            foreach (var node in passedNodesBlack)
            {
                if (Utils.IsSixPieceOrLess(node.Key)) continue;

                var state = DatabaseContoller.GetMovesById(node.Key);
                if (state == null)
                {
                    state = new BoardState() { Moves = new List<MoveStat>() { new MoveStat() { l = blackAddToLoss, w = blackAddToWin, m = node.Value } } };
                    DatabaseContoller.WriteToDatabase(node.Key, state);
                    continue;
                }

                var found = false;
                foreach (var stateMove in state.Moves)
                {
                    if (stateMove.m != node.Value) continue;
                    stateMove.l += blackAddToLoss;
                    stateMove.w += blackAddToWin;
                    found = true;
                    break;
                }

                if (!found) state.Moves.Add(new MoveStat() { l = whiteAddToLoss, w = whiteAddToWin, m = node.Value });

                DatabaseContoller.WriteToDatabase(node.Key, state);
            }
        }
    }
}
