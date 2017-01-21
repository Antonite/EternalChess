using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EternalChess
{
    public static class Utils
    {
        public static string ColumnToLetter(int column)
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

        public static int LetterToColumn(char letter)
        {
            switch (letter)
            {
                case 'a': return 0;
                case 'b': return 1;
                case 'c': return 2;
                case 'd': return 3;
                case 'e': return 4;
                case 'f': return 5;
                case 'g': return 6;
                case 'h': return 7;
                default: return 0;
            }
        }

        public static bool IsSixPieceOrLess(string fer)
        {
            var board = fer.Split()[0];
            var count = 0;
            for (var i = 0; i < board.Length; i++)
            {
                var charAt = board.ElementAt(i).ToString().ToLower();
                if (charAt == "r" || charAt == "n" || charAt == "b" || charAt == "q" || charAt == "k" || charAt == "p")
                    count++;
            }

            return count <= 6;
        }

        public static void PrintResultOfGame(string result)
        {
            Console.WriteLine(result);
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("");
        }
    }
}
