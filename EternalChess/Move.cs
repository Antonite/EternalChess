
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
            var output = color + " " + tempPiece + " " + columnToLetter(after.column) + (after.row + 1) + " from " + columnToLetter(before.column) + (before.row + 1);
            if (isProtion) output += ", Promoted to " + piece;
            return output;
        }

        private string columnToLetter(int column)
        {
            switch (column)
            {
                case 0: return "a";
                case 1: return "b";
                case 2: return "c";
                case 3: return "d";
                case 4: return "e";
                case 5: return "f";
                case 6: return "g";
                case 7: return "h";
                default: return "error";
            }
        }
    }
}
