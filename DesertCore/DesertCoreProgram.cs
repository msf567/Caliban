using System;
using System.Media;
using System.Threading;
using DesertGenerator;

namespace DesertCore
{
    internal static class DesertCoreProgram
    {
        private static SoundPlayer _music = new SoundPlayer();

        public static void Main(string[] args)
        {
            var userKey = DrawMenu(true);

            while (userKey != 'q')
            {
                switch (userKey)
                {
                    case 'g':
                        DesertGenerator.GenerateDesert();
                        break;
                    case 'c':
                        DesertGenerator.ClearDesert();
                        break;
                    default:
                        break;
                }

                userKey = DrawMenu();
            }
        }


        private static char DrawMenu(bool introMode = false)
        {
            Console.Clear();
            Console.SetWindowSize(122, 24);
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            if (introMode)
            {
                _music.SoundLocation = "desert.wav";
                _music.Load();
                Thread.Sleep(2000);
                _music.Play();
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(Center("~~~"));
            Console.WriteLine(Center("Mark Fingerhut Presents"));
            Console.WriteLine(Center("~~~"));
            if (introMode) Thread.Sleep(2398);
            Console.WriteLine(Center("A FileSystem Survival Game"));
            Console.WriteLine(Center("~~~"));
            if (introMode) Thread.Sleep(2412);
            Console.ForegroundColor = DesertGenerator.DesertGenerated ?  ConsoleColor.Yellow : ConsoleColor.DarkYellow;

            Console.WriteLine(Center(
                    " ▄▀▀▀█▄    ▄▀▀▀▀▄   ▄▀▀▄▀▀▀▄  ▄▀▀▀▀▄  ▄▀▀█▄   ▄▀▀▄ █  ▄▀▀█▄▄▄▄  ▄▀▀▄ ▀▄      ▄▀▀▀▀▄  ▄▀▀█▄   ▄▀▀▄ ▀▄  ▄▀▀█▄▄   ▄▀▀▀▀▄ "))
                ;
            if (introMode) Thread.Sleep(594);
            Console.WriteLine(Center(
                    "█  ▄▀  ▀▄ █      █ █   █   █ █ █   ▐ ▐ ▄▀ ▀▄ █  █ ▄▀ ▐  ▄▀   ▐ █  █ █ █     █ █   ▐ ▐ ▄▀ ▀▄ █  █ █ █ █ ▄▀   █ █ █   ▐ "))
                ;
            if (introMode) Thread.Sleep(594);
            Console.WriteLine(Center(
                    "▐ █▄▄▄▄   █      █ ▐  █▀▀█▀     ▀▄     █▄▄▄█ ▐  █▀▄    █▄▄▄▄▄  ▐  █  ▀█        ▀▄     █▄▄▄█ ▐  █  ▀█ ▐ █    █    ▀▄   "))
                ;
            if (introMode) Thread.Sleep(594);
            Console.WriteLine(Center(
                    " █    ▐   ▀▄    ▄▀  ▄▀    █  ▀▄   █   ▄▀   █   █   █   █    ▌    █   █      ▀▄   █   ▄▀   █   █   █    █    █ ▀▄   █  "))
                ;
            if (introMode) Thread.Sleep(594);
            Console.WriteLine(Center(
                    " █          ▀▀▀▀   █     █    █▀▀▀   █   ▄▀  ▄▀   █   ▄▀▄▄▄▄   ▄▀   █        █▀▀▀   █   ▄▀  ▄▀   █    ▄▀▄▄▄▄▀  █▀▀▀   "))
                ;
            if (introMode) Thread.Sleep(594);
            Console.WriteLine(Center(
                    "█                  ▐     ▐    ▐      ▐   ▐   █    ▐   █    ▐   █    ▐        ▐      ▐   ▐   █    ▐   █     ▐   ▐      "))
                ;
            if (introMode) Thread.Sleep(594);
            Console.WriteLine(Center(
                    "▐                                            ▐        ▐        ▐                            ▐        ▐                "))
                ;
            if (introMode) Thread.Sleep(594);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(Center(@"(G)enerate Desert | (C)lear Desert | (Q)uit"));
            return Console.ReadKey().KeyChar;
        }

        private static string Center(string s)
        {
            int consoleWidth = Console.WindowWidth;
            int sLen = s.Length;

            int gap = consoleWidth - sLen;
            return new string(' ', gap / 2) + s;
        }
    }
}