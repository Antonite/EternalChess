using System;

namespace EternalChess
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting EternalTree");

            AIEngine engine = new AIEngine();
            engine.run();

        }
    }
}
