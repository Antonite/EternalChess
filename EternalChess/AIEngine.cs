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
        private int _openingTestDepth;

        public AIEngine()
        {
            Initialize();
            ExternalUtils.InitializeStockFish();
            ExternalUtils.InitializeFathom();
        }

        private void Initialize()
        {
            DatabaseContoller = new DatabaseContoller();
            _openingTestDepth = 1;
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
            // rnbqkbnr/pppppppp
            // ?ChessBoard.ToString(),nq
            ChessBoard = new ChessBoard();
            var passedNodesWhite = new Dictionary<string, string>();
            var passedNodesBlack = new Dictionary<string, string>();
            var experimentalMoves = new Dictionary<string, List<string>>();
            var pastMoves = new Dictionary<string, int>();
            var colorToMove = "white";
            var moves = "";
            var moveNum = 1;

//                                    var moveTests = new string[]
//                                    {
//                                        "e2e4", "e7e6", "d2d4", "d7d5", "b1c3", "g8f6", "e4d5", "e6d5", "g1f3", "f8d6", "c3b5", "e8g8", "b5d6", "d8d6", "f1e2", "b8c6", "e1g1", "h7h6", "e2d3", "c8g4", "c2c3", "a8e8", "h2h3", "g4h5", "c1e3", "f6e4", "g2g4", "h5g6", "f3h4", "e4f6", "h4g6", "e8e3", "f2e3", "d6g3", "g1h1", "g3h3", "h1g1", "h3g3", "g1h1", "g3h3", "h1g1", "h3g3", "g1h1", "g3h3", "h1g1", "h3g3", "g1h1", "f6g4"
//                                    };
//                                    var moveTestNum = 0;

            while (true)
            {
                var fen = ChessBoard.ToFen(colorToMove);
                var possibleMoves = ChessBoard.findAllMoves(colorToMove);
                var posMoveCount = possibleMoves.Count;

                // check for mate/stalemate
                if (posMoveCount == 0)
                {
                    colorToMove = colorToMove == "white" ? "black" : "white";
                    EndGame(colorToMove, passedNodesWhite, passedNodesBlack, experimentalMoves);
                    break;
                }

                // check if in endgame
                if (ChessBoard.remainingPieces <= 6)
                {
                    var result = ExternalUtils.AskFathom(fen);
                    if (!result.Equals("Error"))
                    {
                        EndGame(result, passedNodesWhite, passedNodesBlack, experimentalMoves);
                        break;
                    }
                }

                var moveTime = 1000;
                var bestFenMove = "";
                var fenMoves = DatabaseContoller.GetMovesById(fen);

                // check for current best move
                var topWinRatio = 0.0;
                var currentBestMove = "";
                if (fenMoves != null)
                {
                    foreach (var move in fenMoves.Moves)
                    {
                        var winRatio = move.w/(move.w + move.l);
                        if (winRatio > topWinRatio)
                        {
                            topWinRatio = winRatio;
                            currentBestMove = move.m;
                        }
                    }

                    if ((topWinRatio > 0.5 && colorToMove == "white") || (topWinRatio >= 0.5 && colorToMove == "black")) bestFenMove = currentBestMove;
                }

                // check untested moves in the opening
                if (bestFenMove == "" && moveNum <= _openingTestDepth)
                {
                    if (fenMoves != null)
                    {
                        if (posMoveCount > fenMoves.Moves.Count)
                        {
                            foreach (var possibleMove in possibleMoves)
                            {
                                // if move exists in database, skip
                                var found = false;
                                foreach (var m in fenMoves.Moves)
                                {
                                    // rare fen collisions, prevents a branch from losing once and never hitting again
                                    if (m.w < 0.01)
                                    {
                                        m.w += 0.49;
                                        m.l += 0.51;
                                    }
                                    if (!possibleMove.ToStringMove().Equals(m.m)) continue;
                                    found = true;
                                    break;
                                }
                                if (found) continue;

                                // else select move for testing
                                bestFenMove = possibleMove.ToStringMove();
                                if (experimentalMoves.ContainsKey(fen)) experimentalMoves[fen].Add(bestFenMove);
                                if (!experimentalMoves.ContainsKey(fen)) experimentalMoves.Add(fen, new List<string>() {bestFenMove});
                                break;
                            }
                        }
                        else
                        {
                            // all moves have been tested
                            bestFenMove = currentBestMove;
                        }
                    }
                    else
                    {
                        // select random move for testing
                        bestFenMove = possibleMoves[0].ToStringMove();
                        if (experimentalMoves.ContainsKey(fen)) experimentalMoves[fen].Add(bestFenMove);
                        if (!experimentalMoves.ContainsKey(fen)) experimentalMoves.Add(fen, new List<string>() { bestFenMove });
                    }

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
                moveNum++;
                colorToMove = colorToMove == "white" ? "black" : "white";

                if (pastMoves[fen] == 3 || ChessBoard.remainingPieces == 2)
                {
                    EndGame("tie", passedNodesWhite, passedNodesBlack, experimentalMoves);
                    break;
                }
            }
        }


        private void EndGame(string result, Dictionary<string, string> passedNodesWhite, Dictionary<string, string> passedNodesBlack, Dictionary<string, List<string>> experimentalMoves)
        {
            var whiteAddToWin = 0.5;
            var whiteAddToLoss = 0.5;
            var blackAddToWin = 0.5;
            var blackAddToLoss = 0.5;

            switch (result)
            {
                case "tie":
                    Utils.PrintResultOfGame("Game tied");
                    break;
                case "white":
                    Utils.PrintResultOfGame("White wins!");
                    whiteAddToWin = 1;
                    whiteAddToLoss = 0;
                    blackAddToWin = 0;
                    blackAddToLoss = 1;
                    break;
                case "black":
                    Utils.PrintResultOfGame("Black wins!");
                    whiteAddToWin = 0;
                    whiteAddToLoss = 1;
                    blackAddToWin = 1;
                    blackAddToLoss = 0;
                    break;
            }

            ProcessPassedNodes(passedNodesWhite, whiteAddToWin, whiteAddToLoss, experimentalMoves);
            ProcessPassedNodes(passedNodesBlack, blackAddToWin, blackAddToLoss, experimentalMoves);
        }


        private void ProcessPassedNodes(Dictionary<string, string> passedNodes, double addToWin, double addToLoss, Dictionary<string, List<string>> experimentalMoves)
        {
            foreach (var node in passedNodes)
            {
                if (Utils.IsSixPieceOrLess(node.Key)) continue;
                var state = DatabaseContoller.GetMovesById(node.Key);
                if (state == null)
                {
                    state = new BoardState() { Moves = new List<MoveStat>()
                    {
                        experimentalMoves.ContainsKey(node.Key)
                            ? new MoveStat() {l = addToLoss + 0.51, w = addToWin + 0.49, m = node.Value}
                            : new MoveStat() {l = addToLoss, w = addToWin, m = node.Value}
                    }};
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

                if (!found)
                {
                    state.Moves.Add(
                        experimentalMoves.ContainsKey(node.Key)
                            ? new MoveStat() {l = addToLoss + 0.51, w = addToWin + 0.49, m = node.Value}
                            : new MoveStat() {l = addToLoss, w = addToWin, m = node.Value});
                }

                DatabaseContoller.WriteToDatabase(node.Key, state);
            }
        }
    }
}
