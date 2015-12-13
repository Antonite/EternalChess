using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EternalChess
{
    class Piece
    {
        public string type;
        public string color;

        public Piece(string type, string color)
        {
            this.type = type;
            this.color = color;
        }
    }
}
