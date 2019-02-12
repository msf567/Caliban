using System;
using System.Media;
using System.Threading;
using CalibanLib.Desert;
using CalibanLib.ConsoleOutput;
using System.Net;
using System.Net.Sockets;
using System.Text;
namespace CalibanCore
{
    internal static class CalibanCoreProject
    {
        private static readonly SoundPlayer Music = new SoundPlayer();

        public static void Main(string[] args)
        {
            var userKey = DrawMenu(false);
    
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
                Music.SoundLocation = "desert.wav";
                Music.Load();
                Thread.Sleep(2000);
                Music.Play();
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            ConsoleFormat.CenterWrite("~~~");
            ConsoleFormat.CenterWrite("Gentle_Virus Presents");
            ConsoleFormat.CenterWrite("~~~");
            if (introMode) Thread.Sleep(2398);
            ConsoleFormat.CenterWrite("A File System Survival Game");
            ConsoleFormat.CenterWrite("~~~");
            if (introMode) Thread.Sleep(2412);
            foreach (var s in TitleGraphic)
            {
                ConsoleFormat.CenterWrite(s);
                if (introMode) Thread.Sleep(594);
            }

            ConsoleFormat.CenterWrite("");
            ConsoleFormat.CenterWrite("");
            ConsoleFormat.CenterWrite("");

            Console.ForegroundColor = ConsoleColor.White;
            ConsoleFormat.CenterWrite(@"(E)mbark | (H)elp | (A)bout | (Q)uit");
            return Console.ReadKey().KeyChar;
        }

        private static readonly string[] TitleGraphic = new string[]
        {
            " ▄████████    ▄████████  ▄█        ▄█  ▀█████████▄     ▄████████ ███▄▄▄▄       ",
            "███    ███   ███    ███ ███       ███    ███    ███   ███    ███ ███▀▀▀██▄     ",
            "███    █▀    ███    ███ ███       ███▌   ███    ███   ███    ███ ███   ███     ",
            "███          ███    ███ ███       ███▌  ▄███▄▄▄██▀    ███    ███ ███   ███     ",
            "███        ▀███████████ ███       ███▌ ▀▀███▀▀▀██▄  ▀███████████ ███   ███     ",
            "███    █▄    ███    ███ ███       ███    ███    ██▄   ███    ███ ███   ███     ",
            "███    ███   ███    ███ ███▌    ▄ ███    ███    ███   ███    ███ ███   ███     ",
            "████████▀    ███    █▀  █████▄▄██ █▀   ▄█████████▀    ███    █▀   ▀█   █▀      "
        };
    }
}