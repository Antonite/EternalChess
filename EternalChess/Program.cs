using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EternalChess
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting EternalTree");

            Board board = new Board();
            EternalTree eternalTree = new EternalTree();


            //eternalTree.populateResponses(board.findAllMoves("black"));


            Console.Read();
        }
    }
}
