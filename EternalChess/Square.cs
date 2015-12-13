using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
