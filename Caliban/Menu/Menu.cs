using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Caliban.Core.ConsoleOutput;
using Caliban.Core.Audio;
using Caliban.Core.Utility;
using CLIGL;

namespace Caliban.Core.Menu
{
    enum MenuState
    {
        MAIN,
        STANDBY,
        ABOUT,
        HELP
    }

    public static class Menu
    {
        private static int width = 96;
        private static int height = 5;
        private static RenderingWindow window;
        private static RenderingBuffer buffer;

        static Menu()
        {
            ConfigureWindow();
            AudioManager.LoadFile("town_dusk_short.wav", "IntroMusic");
            //AudioManager.LoadFile(Treasures.Treasures.GetStream("town_dusk_short.wav"), "IntroMusic");
        }

        public static void Close()
        {
            int height = Console.WindowHeight;
            while (height > 1)
            {
                Console.SetWindowSize(width, --height);
                Console.SetBufferSize(width, height);
            }
        }

        public static void Intro()
        {
            var handle = OS.Windows.GetConsoleWindow();
            OS.Windows.ShowWindow(handle, OS.Windows.SW_HIDE);
            Console.Clear();
            AudioManager.PlaySound("IntroMusic", true);
            Thread.Sleep(14_754);
            //Thread.Sleep(22_100);
            Process.Start("Note.exe", "Intro.txt");
            Thread.Sleep(14_754);
            //Thread.Sleep(29_500);
            OS.Windows.ShowWindow(handle, OS.Windows.SW_SHOW);
            Main(false);
        }

        public static void Main(bool fastDraw)
        {
            Console.Clear();
            int height = 1;
            if (fastDraw)
            {
                height = 12 + titleGraphic.Length - 1;
            }

            Console.SetWindowSize(width, height);
            Console.SetBufferSize(width, height);
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            ConsoleFormat.CenterWrite("~~~", Color.Yellow);
            if (!fastDraw) IncreaseWindow(ref height);
            if (!fastDraw) IncreaseWindow(ref height);
            ConsoleFormat.CenterWrite("");
            ConsoleFormat.CenterWrite("");
            ConsoleFormat.CenterWrite("C Presents", Color.Yellow);
            if (!fastDraw) IncreaseWindow(ref height);
            ConsoleFormat.CenterWrite("~~~", Color.Yellow);
            if (!fastDraw) IncreaseWindow(ref height);
            ConsoleFormat.CenterWrite("A File System Survival Game", Color.Yellow);
            if (!fastDraw) IncreaseWindow(ref height);
            if (!fastDraw) IncreaseWindow(ref height);
            if (!fastDraw) IncreaseWindow(ref height);
            ConsoleFormat.CenterWrite("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~", Color.Yellow);
            ConsoleFormat.CenterWrite("");

            foreach (var s in titleGraphic)
            {
                ConsoleFormat.CenterWrite(s, Color.Gold);
                if (!fastDraw) IncreaseWindow(ref height);
            }

            ConsoleFormat.CenterWrite("");
            if (!fastDraw) IncreaseWindow(ref height);
            ConsoleFormat.CenterWrite("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~", Color.Yellow);
            if (!fastDraw) IncreaseWindow(ref height);
            ConsoleFormat.CenterWrite("");
            if (!fastDraw) IncreaseWindow(ref height);
            ConsoleFormat.CenterWrite("");
            if (!fastDraw) IncreaseWindow(ref height);

            ConsoleFormat.CenterWrite(@"(E)mbark | (H)elp | (A)bout | (Q)uit");
            if (!fastDraw) IncreaseWindow(ref height);
            // return Console.ReadKey().Key;
        }

        public static void About()
        {
            int height = 1;
            Console.SetWindowSize(width, height);
            Console.SetBufferSize(width, height);
            Console.Clear();
            ConsoleFormat.CenterWrite("");
            IncreaseWindow(ref height);
            ConsoleFormat.CenterWrite("");
            IncreaseWindow(ref height);
            ConsoleFormat.CenterWrite("");
            IncreaseWindow(ref height);
            ConsoleFormat.CenterWrite("");
            ConsoleFormat.CenterWrite("");
            ConsoleFormat.CenterWrite("");
            ConsoleFormat.CenterWrite("");
            ConsoleFormat.CenterWrite("");
            IncreaseWindow(ref height);
            
            ConsoleFormat.CenterWrite("☼ Gentle_Virus ☼", Color.Gold);
            IncreaseWindow(ref height);
            ConsoleFormat.CenterWrite("");  
            
            ConsoleFormat.CenterWrite("♫ Wallhax ♫", Color.Coral);
            IncreaseWindow(ref height);
            ConsoleFormat.CenterWrite("");
            
            IncreaseWindow(ref height);
            ConsoleFormat.CenterWrite("");
            IncreaseWindow(ref height);
            ConsoleFormat.CenterWrite("Press [Esc] to return to Main Menu.");
            IncreaseWindow(ref height);
        }

        public static void Help()
        {
            int height = 1;
            Console.SetWindowSize(width, height);
            Console.SetBufferSize(width, height);
            Console.Clear();
            ConsoleFormat.CenterWrite("");
            IncreaseWindow(ref height);
            ConsoleFormat.CenterWrite("");
            IncreaseWindow(ref height);
            ConsoleFormat.CenterWrite("");
            IncreaseWindow(ref height);
            ConsoleFormat.CenterWrite("Find SimpleVictory.exe. Be sure to drink water.");
            ConsoleFormat.CenterWrite("");
            IncreaseWindow(ref height);
            IncreaseWindow(ref height);
            ConsoleFormat.CenterWrite("Mouse actions are taxing. Key presses are deadly. Don't even think about a CLI.");
            IncreaseWindow(ref height);
            ConsoleFormat.CenterWrite("");
            IncreaseWindow(ref height);
            ConsoleFormat.CenterWrite("There may be some clues for you along the way. Stay vigilant.");
            IncreaseWindow(ref height);
            IncreaseWindow(ref height);
            ConsoleFormat.CenterWrite("");
            IncreaseWindow(ref height);
            ConsoleFormat.CenterWrite("Press [Esc] to return to Main Menu.");
            IncreaseWindow(ref height);
        }
        
        public static void Standby()
        {
            int height = D.debugMode ? 80 : 6;
            Console.SetWindowSize(width, height);
            Console.SetBufferSize(width, height);
            Console.Clear();
            ConsoleFormat.CenterWrite("");
            ConsoleFormat.CenterWrite("Game in progress.");
            ConsoleFormat.CenterWrite("");
            ConsoleFormat.CenterWrite("Press [Esc] to quit and return to Main Menu.");
        }

        public static void Lose()
        {
            int height = 1;
            Console.SetWindowSize(width, 40);
            Console.SetBufferSize(width, 40);
            Console.Clear();
            ConsoleFormat.CenterWrite("");
            IncreaseWindow(ref height);
            ConsoleFormat.CenterWrite("");
            IncreaseWindow(ref height);
            ConsoleFormat.CenterWrite("");
            IncreaseWindow(ref height);
            foreach (var line in deathGraphic)
            {
                ConsoleFormat.CenterWrite(line,Color.Red);
                IncreaseWindow(ref height);
            }

            ConsoleFormat.CenterWrite("");
            IncreaseWindow(ref height);
            ConsoleFormat.CenterWrite("Press [Esc] to return to Main Menu.", Color.Red);
            IncreaseWindow(ref height);
        }

        public static void Win()
        {
            int cHeight = 1;
            Console.SetWindowSize(width, 40);
            Console.SetBufferSize(width, 40);
            Console.Clear();
            ConsoleFormat.CenterWrite("");
            IncreaseWindow(ref cHeight);
            ConsoleFormat.CenterWrite("");
            IncreaseWindow(ref cHeight);
            ConsoleFormat.CenterWrite("");
            IncreaseWindow(ref cHeight);
            foreach (var line in victoryGraphic)
            {
                ConsoleFormat.CenterWrite(line,Color.Green);
                IncreaseWindow(ref cHeight);
            }

            ConsoleFormat.CenterWrite("");
            IncreaseWindow(ref cHeight);
            ConsoleFormat.CenterWrite("Press [Esc] to return to Main Menu.",Color.Green);
            IncreaseWindow(ref cHeight);
        }

        public static void Cheat()
        {
            int height = 1;
            Console.SetWindowSize(width, 40);
            Console.SetBufferSize(width, 40);
            Console.Clear();
            ConsoleFormat.CenterWrite("");
            IncreaseWindow(ref height);
            ConsoleFormat.CenterWrite("");
            IncreaseWindow(ref height);
            ConsoleFormat.CenterWrite("");
            IncreaseWindow(ref height);
            foreach (var line in cheaterGraphic)
            {
                ConsoleFormat.CenterWrite(line, Color.Red);
                IncreaseWindow(ref height);
            }
            ConsoleFormat.CenterWrite("");
            IncreaseWindow(ref height);
            ConsoleFormat.CenterWrite("LOL Will, Did you think I wouldn't notice?", Color.Red);
            IncreaseWindow(ref height);
            ConsoleFormat.CenterWrite("");
            IncreaseWindow(ref height);
            ConsoleFormat.CenterWrite("Press [Esc] to return to Main Menu.", Color.Red);
            IncreaseWindow(ref height);
        }

        private static void ConfigureWindow()
        {
            window = new RenderingWindow("CALIBAN", width, height);
            buffer = new RenderingBuffer(width, height);
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Console.Title = "CALIBAN";
            var hwnd = Process.GetCurrentProcess().MainWindowHandle;
            var style = OS.Windows.GetWindowLong(hwnd, OS.Windows.GWL_STYLE);
            OS.Windows.SetWindowLong(hwnd, OS.Windows.GWL_STYLE, (style & ~ OS.Windows.WS_CAPTION));
            var sWidth = Screen.PrimaryScreen.Bounds.Width;
            OS.Windows.RECT r;
            OS.Windows.GetWindowRect(hwnd, out r);

            OS.Windows.SetWindowPos(hwnd, IntPtr.Zero, (sWidth / 2) - (r.Width / 2), -10, 0, 0,
                OS.Windows.Swp.NOSIZE);
        }

        private static void IncreaseWindow(ref int _height)
        {
            Console.SetWindowSize(width, ++_height);
            Console.SetBufferSize(width, _height);
        }

        private static readonly string[] titleGraphic =
        {
            " ▄████████    ▄████████  ▄█        ▄█  ▀█████████▄     ▄████████ ███▄▄▄▄  ",
            "███    ███   ███    ███ ███       ███    ███    ███   ███    ███ ███▀▀▀██▄",
            "███    █▀    ███    ███ ███       ███▌   ███    ███   ███    ███ ███   ███",
            "███          ███    ███ ███       ███▌  ▄███▄▄▄██▀    ███    ███ ███   ███",
            "███        ▀███████████ ███       ███▌ ▀▀███▀▀▀██▄  ▀███████████ ███   ███",
            "███    █▄    ███    ███ ███       ███    ███    ██▄   ███    ███ ███   ███",
            "███    ███   ███    ███ ███▌    ▄ ███    ███    ███   ███    ███ ███   ███",
            "████████▀    ███    █▀  █████▄▄██ █▀   ▄█████████▀    ███    █▀   ▀█   █▀ "
        };

        private static readonly string[] deathGraphic =
        {
            "▄██   ▄    ▄██████▄  ███    █▄       ████████▄   ▄█     ▄████████ ████████▄  ",
            "███   ██▄ ███    ███ ███    ███      ███   ▀███ ███    ███    ███ ███   ▀███ ",
            "███▄▄▄███ ███    ███ ███    ███      ███    ███ ███▌   ███    █▀  ███    ███ ",
            "▀▀▀▀▀▀███ ███    ███ ███    ███      ███    ███ ███▌  ▄███▄▄▄     ███    ███ ",
            "▄██   ███ ███    ███ ███    ███      ███    ███ ███▌ ▀▀███▀▀▀     ███    ███ ",
            "███   ███ ███    ███ ███    ███      ███    ███ ███    ███    █▄  ███    ███ ",
            "███   ███ ███    ███ ███    ███      ███   ▄███ ███    ███    ███ ███   ▄███ ",
            " ▀█████▀   ▀██████▀  ████████▀       ████████▀  █▀     ██████████ ████████▀  "
        };

        private static readonly string[] victoryGraphic =
        {
            " ▄█    █▄   ▄█   ▄████████     ███      ▄██████▄     ▄████████ ▄██   ▄   ",
            "███    ███ ███  ███    ███ ▀█████████▄ ███    ███   ███    ███ ███   ██▄ ",
            "███    ███ ███▌ ███    █▀     ▀███▀▀██ ███    ███   ███    ███ ███▄▄▄███ ",
            "███    ███ ███▌ ███            ███   ▀ ███    ███  ▄███▄▄▄▄██▀ ▀▀▀▀▀▀███ ",
            "███    ███ ███▌ ███            ███     ███    ███ ▀▀███▀▀▀▀▀   ▄██   ███ ",
            "███    ███ ███  ███    █▄      ███     ███    ███ ▀███████████ ███   ███ ",
            "███    ███ ███  ███    ███     ███     ███    ███   ███    ███ ███   ███ ",
            " ▀██████▀  █▀   ████████▀     ▄████▀    ▀██████▀    ███    ███  ▀█████▀   ",
            "                                                    ███    ███            "
        };

        private static readonly string[] cheaterGraphic =
        {
         "▄████████    ▄█    █▄       ▄████████    ▄████████     ███        ▄████████    ▄████████ ",
         "███    ███   ███    ███     ███    ███   ███    ███ ▀█████████▄   ███    ███   ███    ███",
         "███    █▀    ███    ███     ███    █▀    ███    ███    ▀███▀▀██   ███    █▀    ███    ███",
         "███         ▄███▄▄▄▄███▄▄  ▄███▄▄▄       ███    ███     ███   ▀  ▄███▄▄▄      ▄███▄▄▄▄██▀",
         "███        ▀▀███▀▀▀▀███▀  ▀▀███▀▀▀     ▀███████████     ███     ▀▀███▀▀▀     ▀▀███▀▀▀▀▀  ",
         "███    █▄    ███    ███     ███    █▄    ███    ███     ███       ███    █▄  ▀███████████",
         "███    ███   ███    ███     ███    ███   ███    ███     ███       ███    ███   ███    ███",
         "████████▀    ███    █▀      ██████████   ███    █▀     ▄████▀     ██████████   ███    ███",
         "                                                                               ███    ███"
        };
    }
}