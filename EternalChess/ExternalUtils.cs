using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EternalChess
{
    public class ExternalUtils
    {
        public static Process FathomProcess = new System.Diagnostics.Process
        {
            StartInfo =
                {
                    FileName = "cmd.exe",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    CreateNoWindow = true
                }
        };

        public static Process StockfishProcess = new System.Diagnostics.Process
        {
            StartInfo =
                {
                    FileName = "cmd.exe",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    CreateNoWindow = true
                }
        };

        private static StreamWriter _fathomWrite;
        private static StreamWriter _stockFishWrite;

        public static string AskFathom(string fenMove)
        {
            _fathomWrite.WriteLine("fathom.exe --path=D:\\syzygy\\wdl;D:\\syzygy\\dtz \"" + fenMove + "\" --test");

            var lineCount = 0;
            var line = "";
            while (lineCount <= 2)
            {
                lineCount++;
                line = FathomProcess.StandardOutput.ReadLine();
            }

            switch (line)
            {
                case "1-0": return "white";
                case "0-1": return "black";
                case "1/2-1/2": return "tie";
                default: return "Error";
            }
        }

        public static string AskStockfish(string moves, int moveTime)
        {
            _stockFishWrite.WriteLine("position startpos moves " + moves);
            _stockFishWrite.WriteLine("go movetime " + moveTime);

            while (true)
            {
                var line = StockfishProcess.StandardOutput.ReadLine();
                if (line.Contains("bestmove"))
                {
                    return line.Split()[1];
                }
            }

        }

        public static void InitializeStockFish()
        {
            StockfishProcess.Start();
            _stockFishWrite = StockfishProcess.StandardInput;
            _stockFishWrite.WriteLine("stockfish_8_x64.exe");
            _stockFishWrite.WriteLine("setoption name Threads value 8");
            _stockFishWrite.WriteLine("setoption name Hash value 8192");
            _stockFishWrite.WriteLine("setoption name Ponder value False");
            _stockFishWrite.WriteLine("setoption name SyzygyPath value D:\\syzygy\\wdl;D:\\syzygy\\dtz");
        }

        public static void InitializeFathom()
        {
            FathomProcess.Start();
            _fathomWrite = FathomProcess.StandardInput;
            _fathomWrite.WriteLine("fathom.exe --path=D:\\syzygy\\wdl;D:\\syzygy\\dtz \"8/p7/5k2/8/5p1P/8/P7/5K2 w - - 0 0\" --test");
            FathomProcess.StandardOutput.ReadLine();
            FathomProcess.StandardOutput.ReadLine();
        }
    }
}
