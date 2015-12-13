using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EternalChess
{
    class Move
    {
        public Location before;
        public Location after;
        public string color;
        public string piece;

        public Move(Location before, Location after, string color, string piece)
        {
            this.before = before;
            this.after = after;
            this.color = color;
            this.piece = piece;
            this.causedCheck = causedCheck;
        }
    }
}
