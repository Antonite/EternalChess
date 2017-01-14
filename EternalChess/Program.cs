using System;

namespace EternalChess
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting EternalTree");

//            GameEngine engine = new GameEngine();
            AIEngine engine = new AIEngine();
            engine.run();
        }
    }
}
