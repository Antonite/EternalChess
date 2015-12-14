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

            GameEngine engine = new GameEngine();
            engine.run();
        }
    }
}
