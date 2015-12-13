using System;
using System.Collections.Generic;

namespace EternalChess
{
    class Board
    {
        List<List<Square>> board;
        Move previousMove;
        Square whiteKing;
        Square blackKing;

        public Board()
        {
            initialize();
        }

        public List<int> move(string color)
        {
            if (previousMove.causedCheck)
            {
                List<Square> attackers = calculateCheckers(color);
            }

            return null;
        }

        private List<Square> calculateCheckers(string color)
        {
            Square currentKing = color == "white" ? whiteKing : blackKing;
            string enemyColor = previousMove.color;
            List<Square> attackers = new List<Square>();

            #region analyze last piece
            //is last piece moved checking king
            Square lastMove = new Square(previousMove.after.row, previousMove.after.row, board[previousMove.after.row][previousMove.after.column].occupiedBy);
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
                            for (int r = currentKing.row, c = currentKing.column; r < previousMove.after.row && c > previousMove.after.column; r++, c--)
                            {
                                if (board[r][c].occupiedBy != null) isChecking = false;
                            }
                        }
                        // right
                        else
                        {
                            for (int r = currentKing.row, c = currentKing.column; r < previousMove.after.row && c < previousMove.after.column; r++, c++)
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
                            for (int r = currentKing.row, c = currentKing.column; r > previousMove.after.row && c > previousMove.after.column; r--, c--)
                            {
                                if (board[r][c].occupiedBy != null) isChecking = false;
                            }
                        }
                        // right
                        else
                        {
                            for (int r = currentKing.row, c = currentKing.column; r > previousMove.after.row && c < previousMove.after.column; r--, c++)
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
                        for (int c = currentKing.column; c < previousMove.after.column; c++)
                        {
                            if (board[currentKing.row][c].occupiedBy != null) isChecking = false;
                        }
                    }
                    // left
                    else if (previousMove.after.column < currentKing.column)
                    {
                        for (int c = currentKing.column; c > previousMove.after.column; c--)
                        {
                            if (board[currentKing.row][c].occupiedBy != null) isChecking = false;
                        }
                    }
                    // below
                    else if (previousMove.after.row < currentKing.row)
                    {
                        for (int r = currentKing.row; r > previousMove.after.row; r--)
                        {
                            if (board[r][currentKing.column].occupiedBy != null) isChecking = false;
                        }
                    }
                    // above
                    else if (previousMove.after.row > currentKing.row)
                    {
                        for (int r = currentKing.row; r < previousMove.after.row; r++)
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
                            for (int c = currentKing.column; c < previousMove.after.column; c++)
                            {
                                if (board[currentKing.row][c].occupiedBy != null) isChecking = false;
                            }
                        }
                        // left
                        else if (previousMove.after.column < currentKing.column)
                        {
                            for (int c = currentKing.column; c > previousMove.after.column; c--)
                            {
                                if (board[currentKing.row][c].occupiedBy != null) isChecking = false;
                            }
                        }
                        // below
                        else if (previousMove.after.row < currentKing.row)
                        {
                            for (int r = currentKing.row; r > previousMove.after.row; r--)
                            {
                                if (board[r][currentKing.column].occupiedBy != null) isChecking = false;
                            }
                        }
                        // above
                        else if (previousMove.after.row > currentKing.row)
                        {
                            for (int r = currentKing.row; r < previousMove.after.row; r++)
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
                                for (int r = currentKing.row, c = currentKing.column; r < previousMove.after.row && c > previousMove.after.column; r++, c--)
                                {
                                    if (board[r][c].occupiedBy != null) isChecking = false;
                                }
                            }
                            // right
                            else
                            {
                                for (int r = currentKing.row, c = currentKing.column; r < previousMove.after.row && c < previousMove.after.column; r++, c++)
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
                                for (int r = currentKing.row, c = currentKing.column; r > previousMove.after.row && c > previousMove.after.column; r--, c--)
                                {
                                    if (board[r][c].occupiedBy != null) isChecking = false;
                                }
                            }
                            // right
                            else
                            {
                                for (int r = currentKing.row, c = currentKing.column; r > previousMove.after.row && c < previousMove.after.column; r--, c++)
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
                    for(int i = currentKing.column; i >= 0; i--)
                    {
                        if (board[previousMove.before.row][i].occupiedBy == null) continue;
                        if (isEnemyQueenRock(previousMove.before.row, i, enemyColor)) attackers.Add(new Square(previousMove.before.row, i, board[previousMove.before.row][i].occupiedBy));
                        else break;
                    }
                }
                // to right of king
                else if (previousMove.before.column > currentKing.column)
                {
                    for (int i = currentKing.column; i <= 7; i++)
                    {
                        if (board[previousMove.before.row][i].occupiedBy == null) continue;
                        if (isEnemyQueenRock(previousMove.before.row, i, enemyColor)) attackers.Add(new Square(previousMove.before.row, i, board[previousMove.before.row][i].occupiedBy));
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
                    for (int i = currentKing.row; i >= 0; i--)
                    {
                        if (board[i][previousMove.before.column].occupiedBy == null) continue;
                        if (isEnemyQueenRock(i, previousMove.before.column, enemyColor)) attackers.Add(new Square(i, previousMove.before.column, board[previousMove.before.row][i].occupiedBy));
                        else break;
                    }
                }
                // above king
                if (previousMove.before.row > currentKing.row)
                {
                    for (int i = currentKing.row; i <= 7; i++)
                    {
                        if (board[i][previousMove.before.column].occupiedBy == null) continue;
                        if (isEnemyQueenRock(i, previousMove.before.column, enemyColor)) attackers.Add(new Square(i, previousMove.before.column, board[previousMove.before.row][i].occupiedBy));
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
                        for (int r = currentKing.row, c = currentKing.column; r < previousMove.before.row && c > previousMove.before.column; r++, c--)
                        {
                            if (board[r][c].occupiedBy == null) continue;
                            if (isEnemyQueenBishop(r, c, enemyColor)) attackers.Add(new Square(r, c, board[r][c].occupiedBy));
                            else break;
                        }
                    }
                    // right
                    else
                    {
                        for (int r = currentKing.row, c = currentKing.column; r < previousMove.before.row && c < previousMove.before.column; r++, c++)
                        {
                            if (board[r][c].occupiedBy == null) continue;
                            if (isEnemyQueenBishop(r, c, enemyColor)) attackers.Add(new Square(r, c, board[r][c].occupiedBy));
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
                        for (int r = currentKing.row, c = currentKing.column; r > previousMove.before.row && c > previousMove.before.column; r--, c--)
                        {
                            if (board[r][c].occupiedBy == null) continue;
                            if (isEnemyQueenBishop(r, c, enemyColor)) attackers.Add(new Square(r, c, board[r][c].occupiedBy));
                            else break;
                        }
                    }
                    // right
                    else
                    {
                        for (int r = currentKing.row, c = currentKing.column; r > previousMove.before.row && c < previousMove.before.column; r--, c++)
                        {
                            if (board[r][c].occupiedBy == null) continue;
                            if (isEnemyQueenBishop(r, c, enemyColor)) attackers.Add(new Square(r, c, board[r][c].occupiedBy));
                            else break;
                        }
                    }
                }
            }
            #endregion

            return attackers;
        }

        private List<int> kingEscape(string color)
        {
            var row = 0;
            var column = 0;
            foreach (List<Square> rows in board){
                foreach (Square square in rows)
                {
                    if(square.occupiedBy.type == "King" && square.occupiedBy.color == color)
                    {
                        row = square.row;
                        column = square.column;
                        return moveKing(row, column, color);
                    }
                }
            }

            return null;
        }

        private List<int> moveKing(int row, int column, string color)
        {
            string enemyColor = color == "white" ? "black" : "white";

            if (isValid(row, column - 1)) if (isSafe(row, column - 1, enemyColor)) return new List<int>(2) { row, column - 1 };
            if (isValid(row, column + 1)) if (isSafe(row, column + 1, enemyColor)) return new List<int>(2) { row, column + 1 };
            if (isValid(row + 1, column - 1)) if (isSafe(row + 1, column - 1, enemyColor)) return new List<int>(2) { row + 1, column - 1 };
            if (isValid(row + 1, column + 1)) if (isSafe(row + 1, column + 1, enemyColor)) return new List<int>(2) { row + 1, column + 1 };
            if (isValid(row + 1, column)) if (isSafe(row + 1, column, enemyColor)) return new List<int>(2) { row + 1, column };
            if (isValid(row - 1, column - 1)) if (isSafe(row - 1, column - 1, enemyColor)) return new List<int>(2) { row - 1, column - 1 };
            if (isValid(row - 1, column + 1)) if (isSafe(row - 1, column + 1, enemyColor)) return new List<int>(2) { row - 1, column + 1 };
            if (isValid(row - 1, column)) if (isSafe(row - 1, column, enemyColor)) return new List<int>(2) { row - 1, column };

            return null;
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
            for(int i = c; i >= 0; i--)
            {
                if (board[r][i].occupiedBy == null) continue;
                if (board[r][i].occupiedBy.color != enemyColor) break;
                if (isEnemyQueenRock(r,i,enemyColor)) return false;
                else break;
            }

            //check horizontally right
            for (int i = c; i <= 7; i++)
            {
                if (board[r][i].occupiedBy == null) continue;
                if (board[r][i].occupiedBy.color != enemyColor) break;
                if (isEnemyQueenRock(r, i, enemyColor)) return false;
                else break;
            }

            //check vertically up
            for (int i = r; i <= 7; i++)
            {
                if (board[r][i].occupiedBy == null) continue;
                if (board[r][i].occupiedBy.color != enemyColor) break;
                if (isEnemyQueenRock(r, i, enemyColor)) return false;
                else break;
            }

            //check vertically down
            for (int i = r; i >= 0; i--)
            {
                if (board[r][i].occupiedBy == null) continue;
                if (board[r][i].occupiedBy.color != enemyColor) break;
                if (isEnemyQueenRock(r, i, enemyColor)) return false;
                else break;
            }

            //check diagonally left up
            for (int i = r, n = c; i <= 7 && n >= 0; i++, n--)
            {
                if (board[r][i].occupiedBy == null) continue;
                if (board[r][i].occupiedBy.color != enemyColor) break;
                if (isEnemyQueenBishop(r, i, enemyColor)) return false;
                else break;
            }

            //check diagonally left down
            for (int i = r, n = c; i >= 0 && n >= 0; i--, n--)
            {
                if (board[r][i].occupiedBy == null) continue;
                if (board[r][i].occupiedBy.color != enemyColor) break;
                if (isEnemyQueenBishop(r, i, enemyColor)) return false;
                else break;
            }

            //check diagonally right up
            for (int i = r, n = c; i <= 7 && n <= 7; i++, n++)
            {
                if (board[r][i].occupiedBy == null) continue;
                if (board[r][i].occupiedBy.color != enemyColor) break;
                if (isEnemyQueenBishop(r, i, enemyColor)) return false;
                else break;
            }

            //check diagonally right down
            for (int i = r, n = c; i >= 0 && n <= 7; i--, n++)
            {
                if (board[r][i].occupiedBy == null) continue;
                if (board[r][i].occupiedBy.color != enemyColor) break;
                if (isEnemyQueenBishop(r, i, enemyColor)) return false;
                else break;
            }

            return true;
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
    }
}
