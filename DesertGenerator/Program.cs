using System;
using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Threading;
using System.Xml.Schema;


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
        private static SoundPlayer _music = new SoundPlayer();

        public static void Main(string[] args)
        {
           
            DrawMenu();
            
            while(true){}
        }

        private static void DrawMenu()
        {
            Console.SetWindowSize(122,24);
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            _music.SoundLocation = "desert.wav";
            _music.Load();
            
            Thread.Sleep(2000);
            
            _music.Play();
            Console.ForegroundColor = ConsoleColor.Yellow;
            
            Console.WriteLine(Center("~~~"));
            Console.WriteLine(Center("Mark Fingerhut Presents"));
            Console.WriteLine(Center("~~~"));
            Thread.Sleep(2398);
            Console.WriteLine(Center("A FileSystem Survival Game"));
            Console.WriteLine(Center("~~~"));
            Thread.Sleep(2412);
            Console.WriteLine(Center(" ▄▀▀▀█▄    ▄▀▀▀▀▄   ▄▀▀▄▀▀▀▄  ▄▀▀▀▀▄  ▄▀▀█▄   ▄▀▀▄ █  ▄▀▀█▄▄▄▄  ▄▀▀▄ ▀▄      ▄▀▀▀▀▄  ▄▀▀█▄   ▄▀▀▄ ▀▄  ▄▀▀█▄▄   ▄▀▀▀▀▄ "));
            Thread.Sleep(594);
            Console.WriteLine(Center("█  ▄▀  ▀▄ █      █ █   █   █ █ █   ▐ ▐ ▄▀ ▀▄ █  █ ▄▀ ▐  ▄▀   ▐ █  █ █ █     █ █   ▐ ▐ ▄▀ ▀▄ █  █ █ █ █ ▄▀   █ █ █   ▐ "));
            Thread.Sleep(594);
            Console.WriteLine(Center("▐ █▄▄▄▄   █      █ ▐  █▀▀█▀     ▀▄     █▄▄▄█ ▐  █▀▄    █▄▄▄▄▄  ▐  █  ▀█        ▀▄     █▄▄▄█ ▐  █  ▀█ ▐ █    █    ▀▄   "));
            Thread.Sleep(594);
            Console.WriteLine(Center(" █    ▐   ▀▄    ▄▀  ▄▀    █  ▀▄   █   ▄▀   █   █   █   █    ▌    █   █      ▀▄   █   ▄▀   █   █   █    █    █ ▀▄   █  "));
            Thread.Sleep(594);
            Console.WriteLine(Center(" █          ▀▀▀▀   █     █    █▀▀▀   █   ▄▀  ▄▀   █   ▄▀▄▄▄▄   ▄▀   █        █▀▀▀   █   ▄▀  ▄▀   █    ▄▀▄▄▄▄▀  █▀▀▀   "));
            Thread.Sleep(594);
            Console.WriteLine(Center("█                  ▐     ▐    ▐      ▐   ▐   █    ▐   █    ▐   █    ▐        ▐      ▐   ▐   █    ▐   █     ▐   ▐      "));
            Thread.Sleep(594);
            Console.WriteLine(Center("▐                                            ▐        ▐        ▐                            ▐        ▐                "));
            Thread.Sleep(594);
            Console.ForegroundColor = ConsoleColor.White;
        }

        private static string Center(string s)
        {
            int ConsoleWidth = Console.WindowWidth;
            int sLen = s.Length;
            int gap = ConsoleWidth - sLen;
            return new string(' ',gap/2) + s;
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