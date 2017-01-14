
using System.Linq;

namespace EternalChess
{
    class Square
    {
        public int row;
        public int column;
        public Piece occupiedBy;

        public Square(int row, int column, Piece occupiedBy)
        {
            this.row = row;
            this.column = column;
            this.occupiedBy = occupiedBy;
        }

        public override string ToString()
        {
            if (occupiedBy == null) return " - ";
            var output = "";
            output += occupiedBy.color.ElementAt(0) + " ";
            output += occupiedBy.type.Equals("Knight") ? "N" : occupiedBy.type.ElementAt(0) + "";

            return output;
        }
    }
}
