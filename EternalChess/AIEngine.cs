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
            ExternalUtils.InitializeStockFish();
            ExternalUtils.InitializeFathom();
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
            var pastMoves = new Dictionary<string, int>();
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
                    var result = ExternalUtils.AskFathom(fen);
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
                
                if (bestFenMove == "") bestFenMove = ExternalUtils.AskStockfish(moves, moveTime);

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

                if (pastMoves.ContainsKey(fen))
                {
                    pastMoves[fen]++;
                }
                else
                {
                    pastMoves.Add(fen, 1);
                }

                //perform move
                ChessBoard.performMove(bestMove);
                colorToMove = colorToMove == "white" ? "black" : "white";

                if (pastMoves[fen] == 3 || ChessBoard.remainingPieces == 2)
                {
                    EndGame("tie", passedNodesWhite, passedNodesBlack);
                    break;
                }
            }
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
                if (Utils.IsSixPieceOrLess(node.Key)) continue;
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
