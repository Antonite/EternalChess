using System;
using System.Collections.Generic;
using System.Linq;

namespace EternalChess
{
    class ChessBoard
    {
        public List<List<Square>> board;
        public Move previousMove;
        public Square whiteKing;
        public Square blackKing;
        public bool whiteKingMoved = false;
        public bool blackKingMoved = false;
        public bool whiteRightRookMoved = false;
        public bool whiteLeftRookMoved = false;
        public bool blackRightRookMoved = false;
        public bool blackLeftRookMoved = false;
        public int remainingPieces = 32;

        public ChessBoard()
        {
            initialize();
        }

        public Move GetMoveFromString(string move)
        {
            var columnBefore = letterToColumn(move.ElementAt(0));
            var rowBefore = (move.ElementAt(1) - '0') - 1;
            var columnAfter = letterToColumn(move.ElementAt(2));
            var rowAfter = (move.ElementAt(3) - '0') - 1;

            var squareBefore = board[rowBefore][columnBefore];

            var type = move.Length == 5 ? getTypeFromChar(move.ElementAt(4)) : squareBefore.occupiedBy.type;

            var output = new Move
                (
                    new Location(rowBefore, columnBefore),
                    new Location(rowAfter, columnAfter),
                    squareBefore.occupiedBy.color,
                    type
                );

            return output;
        }

        private string getTypeFromChar(char type)
        {
            switch (type)
            {
                case 'q': return "Queen";
                case 'r': return "Rook";
                case 'k': return "Knight";
                case 'b': return "Bishop";
                default: return "Error in pawn promition";
            }
        }

        private int letterToColumn(char letter)
        {
            switch (letter)
            {
                case 'a': return 0;
                case 'b': return 1;
                case 'c': return 2;
                case 'd': return 3;
                case 'e': return 4;
                case 'f': return 5;
                case 'g': return 6;
                case 'h': return 7;
                default: return 0;
            }
        }

        public List<Move> findAllMoves(string color)
        {
            string enemyColor = color == "white" ? "black" : "white";
            Square currentKing = color == "white" ? whiteKing : blackKing;
            List<Move> moves = new List<Move>();
            List<Location> locationsOfMove = new List<Location>();
            bool isChecked = !isSafe(currentKing.row, currentKing.column, enemyColor);
            if (isChecked)
            {
                moves = kingEscape(color, enemyColor);
                return moves;
            }
            else
            {
                // all possible moves
                foreach (List<Square> rows in board)
                {
                    foreach (Square square in rows)
                    {
                        if (square.occupiedBy == null) continue;
                        else if (square.occupiedBy.color == color)
                        {
                            Location currentLocation = new Location(square.row, square.column);
                            if(square.occupiedBy.type == "King") moves.AddRange(possibleKingMoves(currentLocation, color, enemyColor));
                            else
                            {
                                locationsOfMove = findPossibleMoves(currentLocation, color, enemyColor);
                                moves.AddRange(convertLocationsToMoves(currentLocation, locationsOfMove, square.occupiedBy.type, color));
                            }
                        }
                    }
                }
                // possible moves
                moves = verifyMoves(moves, enemyColor, currentKing);
            }

            return moves;
        }

        private List<Move> verifyMoves(List<Move> moves, string enemyColor, Square currentKing)
        {
            List<Move> verifiedMoves = new List<Move>();
            foreach(Move move in moves)
            {
                #region king
                // castling short
                if (move.piece == "King" && move.after.column - move.before.column == 2)
                {
                    board[move.after.row][6].occupiedBy = board[move.before.row][4].occupiedBy;
                    board[move.after.row][5].occupiedBy = board[move.before.row][7].occupiedBy;
                    board[move.after.row][4].occupiedBy = null;
                    board[move.after.row][7].occupiedBy = null;
                    if (isSafe(move.after.row, 6, enemyColor)) verifiedMoves.Add(move);
                    board[move.after.row][4].occupiedBy = board[move.after.row][6].occupiedBy;
                    board[move.after.row][7].occupiedBy = board[move.after.row][5].occupiedBy;
                    board[move.after.row][6].occupiedBy = null;
                    board[move.after.row][5].occupiedBy = null;
                }
                // castling long
                else if (move.piece == "King" && move.after.column - move.before.column == -2)
                {
                    board[move.after.row][2].occupiedBy = board[move.before.row][4].occupiedBy;
                    board[move.after.row][3].occupiedBy = board[move.before.row][0].occupiedBy;
                    board[move.after.row][4].occupiedBy = null;
                    board[move.after.row][0].occupiedBy = null;
                    if (isSafe(move.after.row, 2, enemyColor)) verifiedMoves.Add(move);
                    board[move.after.row][4].occupiedBy = board[move.after.row][2].occupiedBy;
                    board[move.after.row][0].occupiedBy = board[move.after.row][3].occupiedBy;
                    board[move.after.row][2].occupiedBy = null;
                    board[move.after.row][3].occupiedBy = null;
                }
                else if (move.piece == "King")
                {
                    verifiedMoves.Add(move);
                }
                #endregion
                else
                {
                    Piece takenPiece = board[move.after.row][move.after.column].occupiedBy;
                    board[move.after.row][move.after.column].occupiedBy = board[move.before.row][move.before.column].occupiedBy;
                    board[move.before.row][move.before.column].occupiedBy = null;
                    if (isSafe(currentKing.row, currentKing.column, enemyColor)) verifiedMoves.Add(move);
                    board[move.before.row][move.before.column].occupiedBy = board[move.after.row][move.after.column].occupiedBy;
                    board[move.after.row][move.after.column].occupiedBy = takenPiece;
                }
            }
            return verifiedMoves;
        }
        
        private List<Square> calculateCheckers(string color)
        {
            Square currentKing = color == "white" ? whiteKing : blackKing;
            string enemyColor = previousMove.color;
            List<Square> attackers = new List<Square>();

            #region analyze last piece
            //is last piece moved checking king
            Square lastMove = new Square(previousMove.after.row, previousMove.after.column, board[previousMove.after.row][previousMove.after.column].occupiedBy);
            bool isChecking = true;
            switch (previousMove.piece)
            {
                case "Pawn": 
                    if(enemyColor == "black") if(currentKing.row + 1 == previousMove.after.row && 
                            (currentKing.column + 1 == previousMove.after.column) || (currentKing.column - 1 == previousMove.after.column)) attackers.Add(lastMove);
                    else if (currentKing.row - 1 == previousMove.after.row &&
                            (currentKing.column + 1 == previousMove.after.column) || (currentKing.column - 1 == previousMove.after.column)) attackers.Add(lastMove);
                    break;
                case "Knight":
                    if(Math.Abs(previousMove.after.row - currentKing.row) + Math.Abs(previousMove.after.column - currentKing.column) == 3) attackers.Add(lastMove);
                    break;
                case "Bishop":
                    if (previousMove.after.row == currentKing.row || previousMove.after.column == currentKing.column) break;
                    if (Math.Abs(previousMove.after.row - currentKing.row) != Math.Abs(previousMove.after.column - currentKing.column)) break;
                    // above
                    if (previousMove.after.row > currentKing.row)
                    {
                        // left
                        if(previousMove.after.column < currentKing.column)
                        {
                            for (int r = currentKing.row + 1, c = currentKing.column - 1; r < previousMove.after.row && c > previousMove.after.column; r++, c--)
                            {
                                if (board[r][c].occupiedBy != null) isChecking = false;
                            }
                        }
                        // right
                        else
                        {
                            for (int r = currentKing.row + 1, c = currentKing.column + 1; r < previousMove.after.row && c < previousMove.after.column; r++, c++)
                            {
                                if (board[r][c].occupiedBy != null) isChecking = false;
                            }
                        }
                    }
                    // below
                    else
                    {
                        // left
                        if (previousMove.after.column < currentKing.column)
                        {
                            for (int r = currentKing.row - 1, c = currentKing.column - 1; r > previousMove.after.row && c > previousMove.after.column; r--, c--)
                            {
                                if (board[r][c].occupiedBy != null) isChecking = false;
                            }
                        }
                        // right
                        else
                        {
                            for (int r = currentKing.row - 1, c = currentKing.column + 1; r > previousMove.after.row && c < previousMove.after.column; r--, c++)
                            {
                                if (board[r][c].occupiedBy != null) isChecking = false;
                            }
                        }
                    }
                    if (isChecking) attackers.Add(lastMove);
                    break;
                case "Rook":
                    if (previousMove.after.row != currentKing.row && previousMove.after.column != currentKing.column) break;
                    // right
                    if (previousMove.after.column > currentKing.column)
                    {
                        for (int c = currentKing.column + 1; c < previousMove.after.column; c++)
                        {
                            if (board[currentKing.row][c].occupiedBy != null) isChecking = false;
                        }
                    }
                    // left
                    else if (previousMove.after.column < currentKing.column)
                    {
                        for (int c = currentKing.column - 1; c > previousMove.after.column; c--)
                        {
                            if (board[currentKing.row][c].occupiedBy != null) isChecking = false;
                        }
                    }
                    // below
                    else if (previousMove.after.row < currentKing.row)
                    {
                        for (int r = currentKing.row - 1; r > previousMove.after.row; r--)
                        {
                            if (board[r][currentKing.column].occupiedBy != null) isChecking = false;
                        }
                    }
                    // above
                    else if (previousMove.after.row > currentKing.row)
                    {
                        for (int r = currentKing.row + 1; r < previousMove.after.row; r++)
                        {
                            if (board[r][currentKing.column].occupiedBy != null) isChecking = false;
                        }
                    }
                    if (isChecking) attackers.Add(lastMove);
                    break;
                case "Queen":
                    if (previousMove.after.row != currentKing.row && previousMove.after.column != currentKing.column &&
                        Math.Abs(previousMove.after.row - currentKing.row) != Math.Abs(previousMove.after.column - currentKing.column)) break;
                    // horizontal or vertical
                    if(previousMove.after.row == currentKing.row || previousMove.after.column == currentKing.column)
                    {
                        if (previousMove.after.column > currentKing.column)
                        {
                            for (int c = currentKing.column + 1; c < previousMove.after.column; c++)
                            {
                                if (board[currentKing.row][c].occupiedBy != null) isChecking = false;
                            }
                        }
                        // left
                        else if (previousMove.after.column < currentKing.column)
                        {
                            for (int c = currentKing.column - 1; c > previousMove.after.column; c--)
                            {
                                if (board[currentKing.row][c].occupiedBy != null) isChecking = false;
                            }
                        }
                        // below
                        else if (previousMove.after.row < currentKing.row)
                        {
                            for (int r = currentKing.row - 1; r > previousMove.after.row; r--)
                            {
                                if (board[r][currentKing.column].occupiedBy != null) isChecking = false;
                            }
                        }
                        // above
                        else if (previousMove.after.row > currentKing.row)
                        {
                            for (int r = currentKing.row + 1; r < previousMove.after.row; r++)
                            {
                                if (board[r][currentKing.column].occupiedBy != null) isChecking = false;
                            }
                        }
                    }
                    // diagonal
                    else
                    {
                        // above
                        if (previousMove.after.row > currentKing.row)
                        {
                            // left
                            if (previousMove.after.column < currentKing.column)
                            {
                                for (int r = currentKing.row + 1, c = currentKing.column - 1; r < previousMove.after.row && c > previousMove.after.column; r++, c--)
                                {
                                    if (board[r][c].occupiedBy != null) isChecking = false;
                                }
                            }
                            // right
                            else
                            {
                                for (int r = currentKing.row + 1, c = currentKing.column + 1; r < previousMove.after.row && c < previousMove.after.column; r++, c++)
                                {
                                    if (board[r][c].occupiedBy != null) isChecking = false;
                                }
                            }
                        }
                        // below
                        else
                        {
                            // left
                            if (previousMove.after.column < currentKing.column)
                            {
                                for (int r = currentKing.row - 1, c = currentKing.column - 1; r > previousMove.after.row && c > previousMove.after.column; r--, c--)
                                {
                                    if (board[r][c].occupiedBy != null) isChecking = false;
                                }
                            }
                            // right
                            else
                            {
                                for (int r = currentKing.row - 1, c = currentKing.column + 1; r > previousMove.after.row && c < previousMove.after.column; r--, c++)
                                {
                                    if (board[r][c].occupiedBy != null) isChecking = false;
                                }
                            }
                        }
                    }
                    if (isChecking) attackers.Add(lastMove);
                    break;
                default : break;
            }
            #endregion

            #region hidden check piece
            // find hidden checks
            // on same row
            if (previousMove.before.row == currentKing.row)
            {
                // to left of king
                if(previousMove.before.column < currentKing.column)
                {
                    for(int i = currentKing.column - 1; i >= 0; i--)
                    {
                        if (board[previousMove.before.row][i].occupiedBy == null) continue;
                        if (isEnemyQueenRock(previousMove.before.row, i, enemyColor))
                        {
                            attackers.Add(new Square(previousMove.before.row, i, board[previousMove.before.row][i].occupiedBy));
                            return attackers;
                        }
                        else break;
                    }
                }
                // to right of king
                else if (previousMove.before.column > currentKing.column)
                {
                    for (int i = currentKing.column + 1; i <= 7; i++)
                    {
                        if (board[previousMove.before.row][i].occupiedBy == null) continue;
                        if (isEnemyQueenRock(previousMove.before.row, i, enemyColor))
                        {
                            attackers.Add(new Square(previousMove.before.row, i, board[previousMove.before.row][i].occupiedBy));
                            return attackers;
                        }
                        else break;
                    }
                }
            }
            // on same column
            if (previousMove.before.column == currentKing.column)
            {
                // below king
                if (previousMove.before.row < currentKing.row)
                {
                    for (int i = currentKing.row - 1; i >= 0; i--)
                    {
                        if (board[i][previousMove.before.column].occupiedBy == null) continue;
                        if (isEnemyQueenRock(i, previousMove.before.column, enemyColor))
                        {
                            attackers.Add(new Square(i, previousMove.before.column, board[previousMove.before.row][i].occupiedBy));
                            return attackers;
                        }
                        else break;
                    }
                }
                // above king
                if (previousMove.before.row > currentKing.row)
                {
                    for (int i = currentKing.row + 1; i <= 7; i++)
                    {
                        if (board[i][previousMove.before.column].occupiedBy == null) continue;
                        if (isEnemyQueenRock(i, previousMove.before.column, enemyColor))
                        {
                            attackers.Add(new Square(i, previousMove.before.column, board[previousMove.before.row][i].occupiedBy));
                            return attackers;
                        }
                        else break;
                    }
                }
            }
            //same diagonal
            if (Math.Abs(previousMove.before.row - currentKing.row) == Math.Abs(previousMove.before.column - currentKing.column))
            {
                // above
                if (previousMove.before.row > currentKing.row)
                {
                    // left
                    if (previousMove.before.column < currentKing.column)
                    {
                        for (int r = currentKing.row + 1, c = currentKing.column - 1; r < previousMove.before.row && c > previousMove.before.column; r++, c--)
                        {
                            if (board[r][c].occupiedBy == null) continue;
                            if (isEnemyQueenBishop(r, c, enemyColor))
                            {
                                attackers.Add(new Square(r, c, board[r][c].occupiedBy));
                                return attackers;
                            }
                            else break;
                        }
                    }
                    // right
                    else
                    {
                        for (int r = currentKing.row + 1, c = currentKing.column + 1; r < previousMove.before.row && c < previousMove.before.column; r++, c++)
                        {
                            if (board[r][c].occupiedBy == null) continue;
                            if (isEnemyQueenBishop(r, c, enemyColor))
                            {
                                attackers.Add(new Square(r, c, board[r][c].occupiedBy));
                                return attackers;
                            }
                            else break;
                        }
                    }
                }
                // below
                else
                {
                    // left
                    if (previousMove.before.column < currentKing.column)
                    {
                        for (int r = currentKing.row - 1, c = currentKing.column - 1; r > previousMove.before.row && c > previousMove.before.column; r--, c--)
                        {
                            if (board[r][c].occupiedBy == null) continue;
                            if (isEnemyQueenBishop(r, c, enemyColor))
                            {
                                attackers.Add(new Square(r, c, board[r][c].occupiedBy));
                                return attackers;
                            }
                            else break;
                        }
                    }
                    // right
                    else
                    {
                        for (int r = currentKing.row - 1, c = currentKing.column + 1; r > previousMove.before.row && c < previousMove.before.column; r--, c++)
                        {
                            if (board[r][c].occupiedBy == null) continue;
                            if (isEnemyQueenBishop(r, c, enemyColor))
                            {
                                attackers.Add(new Square(r, c, board[r][c].occupiedBy));
                                return attackers;
                            }
                            else break;
                        }
                    }
                }
            }
            #endregion

            return attackers;
        }

        private List<Move> kingEscape(string color, string enemyColor)
        {
            Square currentKing = color == "white" ? whiteKing : blackKing;
            var acceptableMoves = new List<Move>();
            List<Square> attackers = calculateCheckers(color);
            List<Location> possibleEscapeMoves = moveKing(currentKing.row, currentKing.column, color);
            foreach (var possibleEscapeMove in possibleEscapeMoves)
            {
                acceptableMoves.Add(new Move(new Location(currentKing.row, currentKing.column), possibleEscapeMove, color, "King"));
            }

            if (attackers.Count == 1)
            {
                #region Find possible cover squares
                List<Location> possibleCoverSquares = new List<Location>();
                switch (attackers[0].occupiedBy.type)
                {
                    case "Bishop":
                        // above
                        if (attackers[0].row > currentKing.row)
                        {
                            // left
                            if (attackers[0].column < currentKing.column)
                            {
                                for (int r = currentKing.row + 1, c = currentKing.column - 1; r < attackers[0].row && c > attackers[0].column; r++, c--)
                                {
                                        possibleCoverSquares.Add(new Location(r, c));
                                }
                            }
                            // right
                            else
                            {
                                for (int r = currentKing.row + 1, c = currentKing.column + 1; r < attackers[0].row && c < attackers[0].column; r++, c++)
                                {
                                        possibleCoverSquares.Add(new Location(r, c));
                                }
                            }
                        }
                        // below
                        else
                        {
                            // left
                            if (attackers[0].column < currentKing.column)
                            {
                                for (int r = currentKing.row - 1, c = currentKing.column - 1; r > attackers[0].row && c > attackers[0].column; r--, c--)
                                {
                                        possibleCoverSquares.Add(new Location(r, c));
                                }
                            }
                            // right
                            else
                            {
                                for (int r = currentKing.row - 1, c = currentKing.column + 1; r > attackers[0].row && c < attackers[0].column; r--, c++)
                                {
                                        possibleCoverSquares.Add(new Location(r, c));
                                }
                            }
                        }
                        break;
                    case "Rook":
                        // right
                        if (attackers[0].column > currentKing.column)
                        {
                            for (int c = currentKing.column + 1; c < attackers[0].column; c++)
                            {
                                possibleCoverSquares.Add(new Location(currentKing.row, c));
                            }
                        }
                        // left
                        else if (attackers[0].column < currentKing.column)
                        {
                            for (int c = currentKing.column - 1; c > attackers[0].column; c--)
                            {
                                possibleCoverSquares.Add(new Location(currentKing.row, c));
                            }
                        }
                        // below
                        else if (attackers[0].row < currentKing.row)
                        {
                            for (int r = currentKing.row - 1; r > attackers[0].row; r--)
                            {
                                possibleCoverSquares.Add(new Location(r, currentKing.column));
                            }
                        }
                        // above
                        else if (attackers[0].row > currentKing.row)
                        {
                            for (int r = currentKing.row + 1; r < attackers[0].row; r++)
                            {
                                possibleCoverSquares.Add(new Location(r, currentKing.column));
                            }
                        }
                        break;
                    case "Queen":
                        // horizontal or vertical
                        if (attackers[0].row == currentKing.row || attackers[0].column == currentKing.column)
                        {
                            if (attackers[0].column > currentKing.column)
                            {
                                for (int c = currentKing.column + 1; c < attackers[0].column; c++)
                                {
                                    possibleCoverSquares.Add(new Location(currentKing.row, c));
                                }
                            }
                            // left
                            else if (attackers[0].column < currentKing.column)
                            {
                                for (int c = currentKing.column - 1; c > attackers[0].column; c--)
                                {
                                    possibleCoverSquares.Add(new Location(currentKing.row, c));
                                }
                            }
                            // below
                            else if (attackers[0].row < currentKing.row)
                            {
                                for (int r = currentKing.row - 1; r > attackers[0].row; r--)
                                {
                                    possibleCoverSquares.Add(new Location(r, currentKing.column));
                                }
                            }
                            // above
                            else if (attackers[0].row > currentKing.row)
                            {
                                for (int r = currentKing.row + 1; r < attackers[0].row; r++)
                                {
                                    possibleCoverSquares.Add(new Location(r, currentKing.column));
                                }
                            }
                        }
                        // diagonal
                        else
                        {
                            // above
                            if (attackers[0].row > currentKing.row)
                            {
                                // left
                                if (attackers[0].column < currentKing.column)
                                {
                                    for (int r = currentKing.row + 1, c = currentKing.column - 1; r < attackers[0].row && c > attackers[0].column; r++, c--)
                                    {
                                        possibleCoverSquares.Add(new Location(r, c));
                                    }
                                }
                                // right
                                else
                                {
                                    for (int r = currentKing.row + 1, c = currentKing.column + 1; r < attackers[0].row && c < attackers[0].column; r++, c++)
                                    {
                                        possibleCoverSquares.Add(new Location(r, c));
                                    }
                                }
                            }
                            // below
                            else
                            {
                                // left
                                if (attackers[0].column < currentKing.column)
                                {
                                    for (int r = currentKing.row - 1, c = currentKing.column - 1; r > attackers[0].row && c > attackers[0].column; r--, c--)
                                    {
                                        possibleCoverSquares.Add(new Location(r, c));
                                    }
                                }
                                // right
                                else
                                {
                                    for (int r = currentKing.row - 1, c = currentKing.column + 1; r > attackers[0].row && c < attackers[0].column; r--, c++)
                                    {
                                        possibleCoverSquares.Add(new Location(r, c));
                                    }
                                }
                            }
                        }
                        break;
                    default: break;
                }

                possibleCoverSquares.Add(new Location(attackers[0].row, attackers[0].column));

                #endregion
                #region Try cover the king
                foreach (List<Square> rows in board)
                {
                    foreach (Square square in rows)
                    {
                        if(square.occupiedBy != null && square.occupiedBy.color == color)
                        {
                            List<Location> possibleMoves = findPossibleMoves(new Location(square.row, square.column), color, enemyColor);
                            foreach(Location possibleCoverMove in possibleCoverSquares)
                            {
                                var coverFound = false;
                                foreach (var possibleMove in possibleMoves)
                                {
                                    if (possibleMove.column == possibleCoverMove.column &&
                                        possibleMove.row == possibleCoverMove.row)
                                    {
                                        coverFound = true;
                                        break;
                                    }
                                }

                                // cover found
                                if (coverFound)
                                {
                                    Piece takenPiece = board[possibleCoverMove.row][possibleCoverMove.column].occupiedBy;
                                    board[possibleCoverMove.row][possibleCoverMove.column].occupiedBy = square.occupiedBy;
                                    square.occupiedBy = null;
                                    bool isMoveSafe = isSafe(currentKing.row, currentKing.column, enemyColor);
                                    square.occupiedBy = board[possibleCoverMove.row][possibleCoverMove.column].occupiedBy;
                                    board[possibleCoverMove.row][possibleCoverMove.column].occupiedBy = takenPiece;
                                    if (isMoveSafe) acceptableMoves.Add(new Move(new Location(square.row, square.column), possibleCoverMove, square.occupiedBy.color, square.occupiedBy.type));
                                }
                            }
                        }
                    }
                }
                #endregion
            }
            return acceptableMoves;
        }

        private Move convertLocationToMove(Location locationBefore, Location locationAfter, string type, string color)
        {
            return new Move(locationBefore, locationAfter, color, type);
        }

        private List<Move> convertLocationsToMoves(Location locationBefore, List<Location> locationsAfter, string type, string color)
        {
            List<Move> moves = new List<Move>();
            foreach (Location loc in locationsAfter)
            {
                moves.Add(convertLocationToMove(locationBefore, loc, type, color));
            }
            return moves;
        }

        private List<Location> findPossibleMoves(Location location, string color, string enemyColor)
        {
            Square square = board[location.row][location.column];
            List<Location> possibleMoves = new List<Location>();
            switch (square.occupiedBy.type)
            {
                #region bishop
                case "Bishop":
                    // top left
                    for (int r = location.row + 1, c = location.column - 1; r <= 7 && c >= 0; r++, c--)
                    {
                        if (board[r][c].occupiedBy == null) possibleMoves.Add(new Location(r, c));
                        else if (board[r][c].occupiedBy.color == enemyColor)
                        {
                            possibleMoves.Add(new Location(r, c));
                            break;
                        }
                        else break;
                    }
                    // top right
                    for (int r = location.row + 1, c = location.column + 1; r <= 7 && c <= 7; r++, c++)
                    {
                        if (board[r][c].occupiedBy == null) possibleMoves.Add(new Location(r, c));
                        else if (board[r][c].occupiedBy.color == enemyColor)
                        {
                            possibleMoves.Add(new Location(r, c));
                            break;
                        }
                        else break;
                    }
                    // bottom left
                    for (int r = location.row - 1, c = location.column - 1; r >= 0 && c >= 0; r--, c--)
                    {
                        if (board[r][c].occupiedBy == null) possibleMoves.Add(new Location(r, c));
                        else if (board[r][c].occupiedBy.color == enemyColor)
                        {
                            possibleMoves.Add(new Location(r, c));
                            break;
                        }
                        else break;
                    }
                    // bottom right
                    for (int r = location.row - 1, c = location.column + 1; r >= 0 && c <= 7; r--, c++)
                    {
                        if (board[r][c].occupiedBy == null) possibleMoves.Add(new Location(r, c));
                        else if (board[r][c].occupiedBy.color == enemyColor)
                        {
                            possibleMoves.Add(new Location(r, c));
                            break;
                        }
                        else break;
                    }
                    break;
                #endregion
                #region rook
                case "Rook":
                    // right
                    for (int c = location.column + 1; c <= 7 ; c++)
                    {
                        if (board[location.row][c].occupiedBy == null) possibleMoves.Add(new Location(location.row, c));
                        else if (board[location.row][c].occupiedBy.color == enemyColor)
                        {
                            possibleMoves.Add(new Location(location.row, c));
                            break;
                        }
                        else break;
                    }
                    // left
                    for (int c = location.column - 1; c >= 0; c--)
                    {
                        if (board[location.row][c].occupiedBy == null) possibleMoves.Add(new Location(location.row, c));
                        else if (board[location.row][c].occupiedBy.color == enemyColor)
                        {
                            possibleMoves.Add(new Location(location.row, c));
                            break;
                        }
                        else break;
                    }
                    // below
                    for (int r = location.row - 1; r >= 0; r--)
                    {
                        if (board[r][location.column].occupiedBy == null) possibleMoves.Add(new Location(r, location.column));
                        else if (board[r][location.column].occupiedBy.color == enemyColor)
                        {
                            possibleMoves.Add(new Location(r, location.column));
                            break;
                        }
                        else break;
                    }
                    // above
                    for (int r = location.row + 1; r <= 7; r++)
                    {
                        if (board[r][location.column].occupiedBy == null) possibleMoves.Add(new Location(r, location.column));
                        else if (board[r][location.column].occupiedBy.color == enemyColor)
                        {
                            possibleMoves.Add(new Location(r, location.column));
                            break;
                        }
                        else break;
                    }
                    break;
                #endregion
                #region queen
                case "Queen":
                    // right
                    for (int c = location.column + 1; c <= 7; c++)
                    {
                        if (board[location.row][c].occupiedBy == null) possibleMoves.Add(new Location(location.row, c));
                        else if (board[location.row][c].occupiedBy.color == enemyColor)
                        {
                            possibleMoves.Add(new Location(location.row, c));
                            break;
                        }
                        else break;
                    }
                    // left
                    for (int c = location.column - 1; c >= 0; c--)
                    {
                        if (board[location.row][c].occupiedBy == null) possibleMoves.Add(new Location(location.row, c));
                        else if (board[location.row][c].occupiedBy.color == enemyColor)
                        {
                            possibleMoves.Add(new Location(location.row, c));
                            break;
                        }
                        else break;
                    }
                    // below
                    for (int r = location.row - 1; r >= 0; r--)
                    {
                        if (board[r][location.column].occupiedBy == null) possibleMoves.Add(new Location(r, location.column));
                        else if (board[r][location.column].occupiedBy.color == enemyColor)
                        {
                            possibleMoves.Add(new Location(r, location.column));
                            break;
                        }
                        else break;
                    }
                    // above
                    for (int r = location.row + 1; r <= 7; r++)
                    {
                        if (board[r][location.column].occupiedBy == null) possibleMoves.Add(new Location(r, location.column));
                        else if (board[r][location.column].occupiedBy.color == enemyColor)
                        {
                            possibleMoves.Add(new Location(r, location.column));
                            break;
                        }
                        else break;
                    }
                    // top left
                    for (int r = location.row + 1, c = location.column - 1; r <= 7 && c >= 0; r++, c--)
                    {
                        if (board[r][c].occupiedBy == null) possibleMoves.Add(new Location(r, c));
                        else if (board[r][c].occupiedBy.color == enemyColor)
                        {
                            possibleMoves.Add(new Location(r, c));
                            break;
                        }
                        else break;
                    }
                    // top right
                    for (int r = location.row + 1, c = location.column + 1; r <= 7 && c <= 7; r++, c++)
                    {
                        if (board[r][c].occupiedBy == null) possibleMoves.Add(new Location(r, c));
                        else if (board[r][c].occupiedBy.color == enemyColor)
                        {
                            possibleMoves.Add(new Location(r, c));
                            break;
                        }
                        else break;
                    }
                    // bottom left
                    for (int r = location.row - 1, c = location.column - 1; r >= 0 && c >= 0; r--, c--)
                    {
                        if (board[r][c].occupiedBy == null) possibleMoves.Add(new Location(r, c));
                        else if (board[r][c].occupiedBy.color == enemyColor)
                        {
                            possibleMoves.Add(new Location(r, c));
                            break;
                        }
                        else break;
                    }
                    // bottom right
                    for (int r = location.row - 1, c = location.column + 1; r >= 0 && c <= 7; r--, c++)
                    {
                        if (board[r][c].occupiedBy == null) possibleMoves.Add(new Location(r, c));
                        else if (board[r][c].occupiedBy.color == enemyColor)
                        {
                            possibleMoves.Add(new Location(r, c));
                            break;
                        }
                        else break;
                    }
                    break;
                #endregion
                #region pawn
                case "Pawn":
                    // can move 2 spaces
                    if (color == "white")
                    {
                        // white
                        // move 1
                        if (isValid(location.row + 1, location.column) && board[location.row + 1][location.column].occupiedBy == null)
                        {
                            possibleMoves.Add(new Location(location.row + 1, location.column));
                            // move 2
                            if (location.row == 1 && isValid(location.row + 2, location.column) && board[location.row + 2][location.column].occupiedBy == null)
                                possibleMoves.Add(new Location(location.row + 2, location.column));
                        }
                        // take left
                        if (isValid(location.row + 1, location.column - 1) 
                            && board[location.row + 1][location.column - 1].occupiedBy != null
                            && board[location.row + 1][location.column - 1].occupiedBy.color == "black")
                            possibleMoves.Add(new Location(location.row + 1, location.column - 1));
                        // take right
                        if (isValid(location.row + 1, location.column + 1)
                            && board[location.row + 1][location.column + 1].occupiedBy != null
                            && board[location.row + 1][location.column + 1].occupiedBy.color == "black")
                            possibleMoves.Add(new Location(location.row + 1, location.column + 1));
                        // En passant
                        if (location.row == 4 && previousMove.piece == "Pawn" && previousMove.before.row == 6 && Math.Abs(previousMove.after.column - location.column) == 1)
                        {
                            possibleMoves.Add(new Location(location.row + 1, previousMove.after.column));
                        }
                        break;
                    }
                    // black
                    // move 1
                    if (isValid(location.row - 1, location.column) && board[location.row - 1][location.column].occupiedBy == null)
                    {
                        possibleMoves.Add(new Location(location.row - 1, location.column));
                        // move 2
                        if (location.row == 6 && isValid(location.row - 2, location.column) && board[location.row - 2][location.column].occupiedBy == null)
                            possibleMoves.Add(new Location(location.row - 2, location.column));
                    }
                    // take left
                    if (isValid(location.row - 1, location.column - 1)
                        && board[location.row - 1][location.column - 1].occupiedBy != null
                        && board[location.row - 1][location.column - 1].occupiedBy.color == "white")
                        possibleMoves.Add(new Location(location.row - 1, location.column - 1));
                    // take right
                    if (isValid(location.row - 1, location.column + 1)
                        && board[location.row - 1][location.column + 1].occupiedBy != null
                        && board[location.row - 1][location.column + 1].occupiedBy.color == "white")
                        possibleMoves.Add(new Location(location.row - 1, location.column + 1));
                    // En passant
                    if (location.row == 3 && previousMove.piece == "Pawn" && previousMove.before.row == 1 && Math.Abs(previousMove.after.column - location.column) == 1)
                    {
                        possibleMoves.Add(new Location(location.row - 1, previousMove.after.column));
                    }
                    break;
                #endregion
                #region knight
                case "Knight":
                    if (isValid(location.row + 1, location.column - 2) &&
                        (board[location.row + 1][location.column - 2].occupiedBy == null ||
                        board[location.row + 1][location.column - 2].occupiedBy.color == enemyColor))
                        possibleMoves.Add(new Location(location.row + 1, location.column - 2));
                    if (isValid(location.row + 2, location.column - 1) &&
                        (board[location.row + 2][location.column - 1].occupiedBy == null ||
                        board[location.row + 2][location.column - 1].occupiedBy.color == enemyColor))
                        possibleMoves.Add(new Location(location.row + 2, location.column - 1));
                    if (isValid(location.row + 2, location.column + 1) &&
                        (board[location.row + 2][location.column + 1].occupiedBy == null ||
                        board[location.row + 2][location.column + 1].occupiedBy.color == enemyColor))
                        possibleMoves.Add(new Location(location.row + 2, location.column + 1));
                    if (isValid(location.row + 1, location.column + 2) &&
                        (board[location.row + 1][location.column + 2].occupiedBy == null ||
                        board[location.row + 1][location.column + 2].occupiedBy.color == enemyColor))
                        possibleMoves.Add(new Location(location.row + 1, location.column + 2));
                    if (isValid(location.row - 1, location.column - 2) &&
                        (board[location.row - 1][location.column - 2].occupiedBy == null ||
                        board[location.row - 1][location.column - 2].occupiedBy.color == enemyColor))
                        possibleMoves.Add(new Location(location.row - 1, location.column - 2));
                    if (isValid(location.row - 2, location.column - 1) &&
                        (board[location.row - 2][location.column - 1].occupiedBy == null ||
                        board[location.row - 2][location.column - 1].occupiedBy.color == enemyColor))
                        possibleMoves.Add(new Location(location.row - 2, location.column - 1));
                    if (isValid(location.row - 2, location.column + 1) &&
                        (board[location.row - 2][location.column + 1].occupiedBy == null ||
                        board[location.row - 2][location.column + 1].occupiedBy.color == enemyColor))
                        possibleMoves.Add(new Location(location.row - 2, location.column + 1));
                    if (isValid(location.row - 1, location.column + 2) &&
                        (board[location.row - 1][location.column + 2].occupiedBy == null ||
                        board[location.row - 1][location.column + 2].occupiedBy.color == enemyColor))
                        possibleMoves.Add(new Location(location.row - 1, location.column + 2));
                    break;
                #endregion
                default: break;
            }
            return possibleMoves;
        }

        private List<Location> moveKing(int row, int column, string color)
        {
            string enemyColor = color == "white" ? "black" : "white";
            Piece king = board[row][column].occupiedBy;
            board[row][column].occupiedBy = null;
            List<Location> moves = new List<Location>();

            if (isValidEmptyOrEnemy(row, column - 1, enemyColor)) if (isSafe(row, column - 1, enemyColor)) moves.Add(new Location(row, column - 1));
            if (isValidEmptyOrEnemy(row, column + 1, enemyColor)) if (isSafe(row, column + 1, enemyColor)) moves.Add(new Location(row, column + 1));
            if (isValidEmptyOrEnemy(row + 1, column - 1, enemyColor)) if (isSafe(row + 1, column - 1, enemyColor)) moves.Add(new Location(row + 1, column - 1));
            if (isValidEmptyOrEnemy(row + 1, column + 1, enemyColor)) if (isSafe(row + 1, column + 1, enemyColor)) moves.Add(new Location(row + 1, column + 1));
            if (isValidEmptyOrEnemy(row + 1, column, enemyColor)) if (isSafe(row + 1, column, enemyColor)) moves.Add(new Location(row + 1, column));
            if (isValidEmptyOrEnemy(row - 1, column - 1, enemyColor)) if (isSafe(row - 1, column - 1, enemyColor)) moves.Add(new Location(row - 1, column - 1));
            if (isValidEmptyOrEnemy(row - 1, column + 1, enemyColor)) if (isSafe(row - 1, column + 1, enemyColor)) moves.Add(new Location(row - 1, column + 1));
            if (isValidEmptyOrEnemy(row - 1, column, enemyColor)) if (isSafe(row - 1, column, enemyColor)) moves.Add(new Location(row - 1, column));

            board[row][column].occupiedBy = king;
            return moves;
        }

        private List<Move> possibleKingMoves(Location location, string color, string enemyColor)
        {
            bool hasKingMoved = color == "white" ? whiteKingMoved : blackKingMoved;
            bool haveLeftRooksMoved = color == "white" ? whiteLeftRookMoved : blackLeftRookMoved;
            bool haveRightRooksMoved = color == "white" ? whiteRightRookMoved : blackRightRookMoved;
            Square currentKing = color == "white" ? whiteKing : blackKing;
            List<Move> possibleMoves = new List<Move>();
            if (!hasKingMoved)
            {
                // castle short
                if(!haveRightRooksMoved && board[currentKing.row][5].occupiedBy == null && 
                    board[currentKing.row][6].occupiedBy == null &&
                    board[currentKing.row][7].occupiedBy != null &&
                    board[currentKing.row][7].occupiedBy.type == "Rook" &&
                    board[currentKing.row][7].occupiedBy.color == color)
                {
                    if (isSafe(currentKing.row, 5, enemyColor) && isSafe(currentKing.row, 6, enemyColor))
                        possibleMoves.Add(new Move(new Location(currentKing.row, 4),
                                                    new Location(currentKing.row, 6), color, "King"));
                }
                if (!haveLeftRooksMoved && board[currentKing.row][3].occupiedBy == null &&
                    board[currentKing.row][2].occupiedBy == null &&
                    board[currentKing.row][1].occupiedBy == null &&
                    board[currentKing.row][0].occupiedBy != null &&
                    board[currentKing.row][0].occupiedBy.type == "Rook" &&
                    board[currentKing.row][0].occupiedBy.color == color)
                {
                    if (isSafe(currentKing.row, 3, enemyColor) && isSafe(currentKing.row, 2, enemyColor))
                        possibleMoves.Add(new Move(new Location(currentKing.row, 4),
                                                new Location(currentKing.row, 2), color, "King"));
                }
            }
            possibleMoves.AddRange(convertLocationsToMoves(location, moveKing(location.row, location.column, color), "King", color));
            return possibleMoves;
        }

        private bool isSafe(int r, int c, string enemyColor)
        {
            // check knights
            if (isValid(r + 1, c - 2)) if (isEnemyKnight(r + 1, c - 2, enemyColor)) return false;
            if (isValid(r + 2, c - 1)) if (isEnemyKnight(r + 2, c - 1, enemyColor)) return false;
            if (isValid(r + 2, c + 1)) if (isEnemyKnight(r + 2, c + 1, enemyColor)) return false;
            if (isValid(r + 1, c + 2)) if (isEnemyKnight(r + 1, c + 2, enemyColor)) return false;
            if (isValid(r - 1, c - 2)) if (isEnemyKnight(r - 1, c - 2, enemyColor)) return false;
            if (isValid(r - 2, c - 1)) if (isEnemyKnight(r - 2, c - 1, enemyColor)) return false;
            if (isValid(r - 2, c + 1)) if (isEnemyKnight(r - 2, c + 1, enemyColor)) return false;
            if (isValid(r - 1, c + 2)) if (isEnemyKnight(r - 1, c + 2, enemyColor)) return false;

            //check enemy king
            if (isValid(r, c - 1)) if (isEnemyKing(r, c - 1, enemyColor)) return false;
            if (isValid(r, c + 1)) if (isEnemyKing(r, c + 1, enemyColor)) return false;
            if (isValid(r + 1, c - 1)) if (isEnemyKing(r + 1, c - 1, enemyColor)) return false;
            if (isValid(r + 1, c + 1)) if (isEnemyKing(r + 1, c + 1, enemyColor)) return false;
            if (isValid(r + 1, c)) if (isEnemyKing(r + 1, c, enemyColor)) return false;
            if (isValid(r - 1, c - 1)) if (isEnemyKing(r - 1, c - 1, enemyColor)) return false;
            if (isValid(r - 1, c + 1)) if (isEnemyKing(r - 1, c + 1, enemyColor)) return false;
            if (isValid(r - 1, c)) if (isEnemyKing(r - 1, c, enemyColor)) return false;

            //check pawns
            if(enemyColor == "black")
            {
                if (isValid(r + 1, c - 1)) if (isEnemyPawn(r + 1, c - 1, enemyColor)) return false;
                if (isValid(r + 1, c + 1)) if (isEnemyPawn(r + 1, c + 1, enemyColor)) return false;
            }
            if (enemyColor == "white")
            {
                if (isValid(r - 1, c - 1)) if (isEnemyPawn(r - 1, c - 1, enemyColor)) return false;
                if (isValid(r - 1, c + 1)) if (isEnemyPawn(r - 1, c + 1, enemyColor)) return false;
            }

            //check horizontally left
            for(int i = c - 1; i >= 0; i--)
            {
                if (board[r][i].occupiedBy == null) continue;
                if (board[r][i].occupiedBy.color != enemyColor) break;
                if (isEnemyQueenRock(r,i,enemyColor)) return false;
                else break;
            }

            //check horizontally right
            for (int i = c + 1; i <= 7; i++)
            {
                if (board[r][i].occupiedBy == null) continue;
                if (board[r][i].occupiedBy.color != enemyColor) break;
                if (isEnemyQueenRock(r, i, enemyColor)) return false;
                else break;
            }

            //check vertically up
            for (int i = r + 1; i <= 7; i++)
            {
                if (board[i][c].occupiedBy == null) continue;
                if (board[i][c].occupiedBy.color != enemyColor) break;
                if (isEnemyQueenRock(i, c, enemyColor)) return false;
                else break;
            }

            //check vertically down
            for (int i = r - 1; i >= 0; i--)
            {
                if (board[i][c].occupiedBy == null) continue;
                if (board[i][c].occupiedBy.color != enemyColor) break;
                if (isEnemyQueenRock(i, c, enemyColor)) return false;
                else break;
            }

            //check diagonally left up
            for (int i = r + 1, n = c - 1; i <= 7 && n >= 0; i++, n--)
            {
                if (board[i][n].occupiedBy == null) continue;
                if (board[i][n].occupiedBy.color != enemyColor) break;
                if (isEnemyQueenBishop(i, n, enemyColor)) return false;
                else break;
            }

            //check diagonally left down
            for (int i = r - 1, n = c - 1; i >= 0 && n >= 0; i--, n--)
            {
                if (board[i][n].occupiedBy == null) continue;
                if (board[i][n].occupiedBy.color != enemyColor) break;
                if (isEnemyQueenBishop(i, n, enemyColor)) return false;
                else break;
            }

            //check diagonally right up
            for (int i = r + 1, n = c + 1; i <= 7 && n <= 7; i++, n++)
            {
                if (board[i][n].occupiedBy == null) continue;
                if (board[i][n].occupiedBy.color != enemyColor) break;
                if (isEnemyQueenBishop(i, n, enemyColor)) return false;
                else break;
            }

            //check diagonally right down
            for (int i = r - 1, n = c + 1; i >= 0 && n <= 7; i--, n++)
            {
                if (board[i][n].occupiedBy == null) continue;
                if (board[i][n].occupiedBy.color != enemyColor) break;
                if (isEnemyQueenBishop(i, n, enemyColor)) return false;
                else break;
            }

            return true;
        }

        private bool isValidEmptyOrEnemy(int r, int c, string enemyColor)
        {
            return isValid(r, c) && (board[r][c].occupiedBy == null || board[r][c].occupiedBy.color == enemyColor);
        }

        private bool isValid(int r, int c)
        {
            return r >= 0 && r <= 7 && c >= 0 && c <= 7;
        }

        #region isEnemyPiece

        private bool isEnemyKing(int r, int c, string enemyColor)
        {
            if (board[r][c].occupiedBy == null) return false;
            return board[r][c].occupiedBy.type == "King" && board[r][c].occupiedBy.color == enemyColor;
        }

        private bool isEnemyPawn(int r, int c, string enemyColor)
        {
            if (board[r][c].occupiedBy == null) return false;
            return board[r][c].occupiedBy.type == "Pawn" && board[r][c].occupiedBy.color == enemyColor;
        }

        private bool isEnemyKnight(int r, int c, string enemyColor)
        {
            if (board[r][c].occupiedBy == null) return false;
            return board[r][c].occupiedBy.type == "Knight" && board[r][c].occupiedBy.color == enemyColor;
        }

        private bool isEnemyQueenRock(int r, int c, string enemyColor)
        {
            if (board[r][c].occupiedBy == null) return false;
            return (board[r][c].occupiedBy.type == "Queen" || board[r][c].occupiedBy.type == "Rook") && board[r][c].occupiedBy.color == enemyColor;
        }

        private bool isEnemyQueenBishop(int r, int c, string enemyColor)
        {
            if (board[r][c].occupiedBy == null) return false;
            return (board[r][c].occupiedBy.type == "Queen" || board[r][c].occupiedBy.type == "Bishop") && board[r][c].occupiedBy.color == enemyColor;
        }

        #endregion

        public void initialize()
        {
            Console.WriteLine("Initializing board...");

            board = new List<List<Square>>(8);

            previousMove = new Move(new Location(0, 0), new Location(0, 0), "default", "default");

            for (var i = 0; i < 8; i++)
            {
                List<Square> row = new List<Square>(8);
                for (var j = 0; j < 8; j++)
                {
                    row.Add(null);
                }
                board.Add(row);
            }

            Piece whiteRook = new Piece("Rook","white");
            Piece blackRook = new Piece("Rook", "black");
            Piece whiteKnight = new Piece("Knight", "white");
            Piece blackKnight = new Piece("Knight", "black");
            Piece whiteBishop = new Piece("Bishop", "white");
            Piece blackBishop = new Piece("Bishop", "black");
            Piece whiteQueen = new Piece("Queen", "white");
            Piece blackQueen = new Piece("Queen", "black");
            Piece whiteKing = new Piece("King", "white");
            Piece blackKing = new Piece("King", "black");
            Piece whitePawn = new Piece("Pawn", "white");
            Piece blackPawn = new Piece("Pawn", "black");

            this.whiteKing = new Square(0, 4, whiteKing);
            this.blackKing = new Square(7, 4, blackKing);

            board[0][0] = new Square(0, 0, whiteRook);
            board[0][1] = new Square(0, 1, whiteKnight);
            board[0][2] = new Square(0, 2, whiteBishop);
            board[0][3] = new Square(0, 3, whiteQueen);
            board[0][4] = new Square(0, 4, whiteKing);
            board[0][5] = new Square(0, 5, whiteBishop);
            board[0][6] = new Square(0, 6, whiteKnight);
            board[0][7] = new Square(0, 7, whiteRook);

            board[1][0] = new Square(1, 0, whitePawn);
            board[1][1] = new Square(1, 1, whitePawn);
            board[1][2] = new Square(1, 2, whitePawn);
            board[1][3] = new Square(1, 3, whitePawn);
            board[1][4] = new Square(1, 4, whitePawn);
            board[1][5] = new Square(1, 5, whitePawn);
            board[1][6] = new Square(1, 6, whitePawn);
            board[1][7] = new Square(1, 7, whitePawn);

            board[7][0] = new Square(7, 0, blackRook);
            board[7][1] = new Square(7, 1, blackKnight);
            board[7][2] = new Square(7, 2, blackBishop);
            board[7][3] = new Square(7, 3, blackQueen);
            board[7][4] = new Square(7, 4, blackKing);
            board[7][5] = new Square(7, 5, blackBishop);
            board[7][6] = new Square(7, 6, blackKnight);
            board[7][7] = new Square(7, 7, blackRook);

            board[6][0] = new Square(6, 0, blackPawn);
            board[6][1] = new Square(6, 1, blackPawn);
            board[6][2] = new Square(6, 2, blackPawn);
            board[6][3] = new Square(6, 3, blackPawn);
            board[6][4] = new Square(6, 4, blackPawn);
            board[6][5] = new Square(6, 5, blackPawn);
            board[6][6] = new Square(6, 6, blackPawn);
            board[6][7] = new Square(6, 7, blackPawn);

            for (var i = 0; i<8; i++)
            {
                board[2][i] = new Square(2, i, null);
                board[3][i] = new Square(3, i, null);
                board[4][i] = new Square(4, i, null);
                board[5][i] = new Square(5, i, null);
            }
        }

        private void killUnit(int row, int column)
        {
            if (board[row][column].occupiedBy != null) remainingPieces--;
        }

        public void performMove(Move move)
        {
            var canMove = true;
            #region Promotion
            if (move.piece != "Pawn" && board[move.before.row][move.before.column].occupiedBy.type.Equals("Pawn"))
            {
                killUnit(move.after.row, move.after.column);
                board[move.after.row][move.after.column].occupiedBy = new Piece(move.piece, move.color);
                board[move.before.row][move.before.column].occupiedBy = null;
                canMove = false;
            }
            #endregion
            #region king
            // castling short
            if (move.piece == "King" && move.after.column - move.before.column == 2)
            {
                board[move.after.row][6].occupiedBy = board[move.before.row][4].occupiedBy;
                board[move.after.row][5].occupiedBy = board[move.before.row][7].occupiedBy;
                board[move.after.row][4].occupiedBy = null;
                board[move.after.row][7].occupiedBy = null;
                canMove = false;
            }
            // castling long
            else if (move.piece == "King" && move.after.column - move.before.column == -2)
            {
                board[move.after.row][2].occupiedBy = board[move.before.row][4].occupiedBy;
                board[move.after.row][3].occupiedBy = board[move.before.row][0].occupiedBy;
                board[move.after.row][4].occupiedBy = null;
                board[move.after.row][0].occupiedBy = null;
                canMove = false;
            }
            #endregion
            #region En passant
            else if (move.piece == "Pawn" && move.after.column != move.before.column && board[move.after.row][move.after.column].occupiedBy == null)
            {
                remainingPieces--;
                board[move.after.row][move.after.column].occupiedBy = board[move.before.row][move.before.column].occupiedBy;
                board[move.before.row][move.before.column].occupiedBy = null;
                board[move.before.row][move.after.column].occupiedBy = null;
                canMove = false;
            }
            #endregion
            if (canMove)
            {
                killUnit(move.after.row, move.after.column);
                board[move.after.row][move.after.column].occupiedBy = board[move.before.row][move.before.column].occupiedBy;
                board[move.before.row][move.before.column].occupiedBy = null;
            }

            if (move.piece == "King" && move.color == "white")
            {
                whiteKing = board[move.after.row][move.after.column];
                whiteKingMoved = true;
            }
            if (move.piece == "Rook" && move.color == "white")
            {
                if (move.before.column == 0) whiteLeftRookMoved = true;
                if (move.before.column == 7) whiteRightRookMoved = true;
            }
            if (move.piece == "King" && move.color == "black")
            {
                blackKing = board[move.after.row][move.after.column];
                blackKingMoved = true;
            }
            if (move.piece == "Rook" && move.color == "black")
            {
                if (move.before.column == 0) blackLeftRookMoved = true;
                if (move.before.column == 7) blackRightRookMoved = true;
            }
            previousMove = move;
        }

        public override string ToString()
        {
            var output = "";
            for (var i = 7; i >= 0; i--)
            {
                foreach (var square in board[i])
                {
                    output += square + " ";
                }
                output += "\n";
            }
            return output;
        }
    }
}
