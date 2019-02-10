using System;
using System.Media;
using System.Threading;

namespace DesertCore
{
    internal static class DesertCoreProgram
    {
        private static SoundPlayer _music = new SoundPlayer();

        public static void Main(string[] args)
        {
            DrawMenu();
            while (true)
            {
            }
        }


        public static void DrawMenu()
        {
            Console.SetWindowSize(122, 24);
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
            Console.WriteLine(Center(
                    " ▄▀▀▀█▄    ▄▀▀▀▀▄   ▄▀▀▄▀▀▀▄  ▄▀▀▀▀▄  ▄▀▀█▄   ▄▀▀▄ █  ▄▀▀█▄▄▄▄  ▄▀▀▄ ▀▄      ▄▀▀▀▀▄  ▄▀▀█▄   ▄▀▀▄ ▀▄  ▄▀▀█▄▄   ▄▀▀▀▀▄ "))
                ;
            Thread.Sleep(594);
            Console.WriteLine(Center(
                    "█  ▄▀  ▀▄ █      █ █   █   █ █ █   ▐ ▐ ▄▀ ▀▄ █  █ ▄▀ ▐  ▄▀   ▐ █  █ █ █     █ █   ▐ ▐ ▄▀ ▀▄ █  █ █ █ █ ▄▀   █ █ █   ▐ "))
                ;
            Thread.Sleep(594);
            Console.WriteLine(Center(
                    "▐ █▄▄▄▄   █      █ ▐  █▀▀█▀     ▀▄     █▄▄▄█ ▐  █▀▄    █▄▄▄▄▄  ▐  █  ▀█        ▀▄     █▄▄▄█ ▐  █  ▀█ ▐ █    █    ▀▄   "))
                ;
            Thread.Sleep(594);
            Console.WriteLine(Center(
                    " █    ▐   ▀▄    ▄▀  ▄▀    █  ▀▄   █   ▄▀   █   █   █   █    ▌    █   █      ▀▄   █   ▄▀   █   █   █    █    █ ▀▄   █  "))
                ;
            Thread.Sleep(594);
            Console.WriteLine(Center(
                    " █          ▀▀▀▀   █     █    █▀▀▀   █   ▄▀  ▄▀   █   ▄▀▄▄▄▄   ▄▀   █        █▀▀▀   █   ▄▀  ▄▀   █    ▄▀▄▄▄▄▀  █▀▀▀   "))
                ;
            Thread.Sleep(594);
            Console.WriteLine(Center(
                    "█                  ▐     ▐    ▐      ▐   ▐   █    ▐   █    ▐   █    ▐        ▐      ▐   ▐   █    ▐   █     ▐   ▐      "))
                ;
            Thread.Sleep(594);
            Console.WriteLine(Center(
                    "▐                                            ▐        ▐        ▐                            ▐        ▐                "))
                ;
            Thread.Sleep(594);
            Console.ForegroundColor = ConsoleColor.White;
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