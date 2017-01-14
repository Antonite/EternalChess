
namespace EternalChess
{
    class Move
    {
        public Location before;
        public Location after;
        public string color;
        public string piece;
        public string stringMove;

        public Move(Location before, Location after, string color, string piece)
        {
            this.before = before;
            this.after = after;
            this.color = color;
            this.piece = piece;
            stringMove = toString();
        }

        public string toString()
        {
            var isProtion = piece.Equals("Pawn") && (after.row == 7 || after.row == 0);
            var tempPiece = isProtion ? "Pawn" : piece;
            var output = color + " " + tempPiece + " " + Utils.ColumnToLetter(after.column) + (after.row + 1) + " from " + Utils.ColumnToLetter(before.column) + (before.row + 1);
            if (isProtion) output += ", Promoted to " + piece;
            return output;
        }


    }
}
