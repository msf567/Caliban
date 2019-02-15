using System;
using System.Media;
using System.Threading;
using CalibanLib.ConsoleOutput;

namespace CalibanCore
{
    public class CalibanMenu
    {
        private static readonly SoundPlayer Music = new SoundPlayer();

        public char Show(bool introMode = false)
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
            foreach (var s in _titleGraphic)
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

        private readonly string[] _titleGraphic = new string[]
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