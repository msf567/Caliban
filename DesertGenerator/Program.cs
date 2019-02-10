using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;


namespace DesertGenerator
{
    internal class Program
    {
        private readonly int _desertWidth = 4;
        private readonly int _desertDepth = 2;
        private readonly int _amountOfTreasures = 1;

        private static readonly List<FileStream> HeavyRocks = new List<FileStream>();

        private string[] desertNames = new string[3]
        {
            "sand",
            "hill",
            "dune"
        };

        private static DirectoryInfo _desertRoot = new DirectoryInfo("D:\\Desert");
        private static MusicPlayer _music = new MusicPlayer();

        public static void Main(string[] args)
        {

            DrawMenu();
        }

        private static void DrawMenu()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Thread.Sleep(500);
            Console.WriteLine(
                "███████╗ ██████╗ ██████╗ ███████╗ █████╗ ██╗  ██╗███████╗███╗   ██╗    ██████╗ ██╗   ██╗███╗   ██╗███████╗███████╗    ");
            Thread.Sleep(500);
            Console.WriteLine(
                "██╔════╝██╔═══██╗██╔══██╗██╔════╝██╔══██╗██║ ██╔╝██╔════╝████╗  ██║    ██╔══██╗██║   ██║████╗  ██║██╔════╝██╔════╝    ");
            Thread.Sleep(500);
            Console.WriteLine(
                "█████╗  ██║   ██║██████╔╝███████╗███████║█████╔╝ █████╗  ██╔██╗ ██║    ██║  ██║██║   ██║██╔██╗ ██║█████╗  ███████╗    ");
            Thread.Sleep(500);
            Console.WriteLine(
                "██╔══╝  ██║   ██║██╔══██╗╚════██║██╔══██║██╔═██╗ ██╔══╝  ██║╚██╗██║    ██║  ██║██║   ██║██║╚██╗██║██╔══╝  ╚════██║    ");
            Thread.Sleep(500);
            Console.WriteLine(
                "██║     ╚██████╔╝██║  ██║███████║██║  ██║██║  ██╗███████╗██║ ╚████║    ██████╔╝╚██████╔╝██║ ╚████║███████╗███████║    ");
            Thread.Sleep(500);
            Console.WriteLine(
                "╚═╝      ╚═════╝ ╚═╝  ╚═╝╚══════╝╚═╝  ╚═╝╚═╝  ╚═╝╚══════╝╚═╝  ╚═══╝    ╚═════╝  ╚═════╝ ╚═╝  ╚═══╝╚══════╝╚══════╝    ");
            Thread.Sleep(500);
            Console.WriteLine(
                "                                                                                                                      ");
            Thread.Sleep(500);
            Console.ForegroundColor = ConsoleColor.White;
        }

        private static void ClearDesert()
        {
            foreach (var rock in HeavyRocks)
            {
                rock.Unlock(0, 0);
            }

            _desertRoot.Delete(true);
        }

        private void GenerateDesert()
        {
            ClearDesert();

            Directory.CreateDirectory(_desertRoot.FullName);
        }
    }
}