using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using Caliban.Core.ConsoleOutput;
using Caliban.Core.Audio;
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
        private static int width = 90;
        private static int height = 5;
        private static RenderingWindow window;
        private static RenderingBuffer buffer;

        static Menu()
        {
            ConfigureWindow();
            AudioPlayer.LoadFile("town_dusk_1.wav", "MainMenu");
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

        public static void Main(bool fastDraw, bool _introMode = false)
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
            if (_introMode)
            {
                Thread.Sleep(2000);
                AudioPlayer.PlaySound("MainMenu");
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            ConsoleFormat.CenterWrite("~~~");
            if (!fastDraw) IncreaseWindow(ref height);
            if (!fastDraw) IncreaseWindow(ref height);
            ConsoleFormat.CenterWrite("");
            ConsoleFormat.CenterWrite("");
            ConsoleFormat.CenterWrite("Gentle_Virus Presents");
            if (!fastDraw) IncreaseWindow(ref height);
            ConsoleFormat.CenterWrite("~~~");
            if (!fastDraw) IncreaseWindow(ref height);
            if (_introMode) Thread.Sleep(2398);
            ConsoleFormat.CenterWrite("A File System Survival Game");
            if (!fastDraw) IncreaseWindow(ref height);
            if (!fastDraw) IncreaseWindow(ref height);
            if (!fastDraw) IncreaseWindow(ref height);
            if (_introMode) Thread.Sleep(2412);
            ConsoleFormat.CenterWrite("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            ConsoleFormat.CenterWrite("");
            foreach (var s in titleGraphic)
            {
                ConsoleFormat.CenterWrite(s);
                if (!fastDraw) IncreaseWindow(ref height);
                if (_introMode) Thread.Sleep(594);
            }

            ConsoleFormat.CenterWrite("");
            if (!fastDraw) IncreaseWindow(ref height);
            ConsoleFormat.CenterWrite("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            if (!fastDraw) IncreaseWindow(ref height);
            ConsoleFormat.CenterWrite("");
            if (!fastDraw) IncreaseWindow(ref height);
            ConsoleFormat.CenterWrite("");
            if (!fastDraw) IncreaseWindow(ref height);

            Console.ForegroundColor = ConsoleColor.White;
            ConsoleFormat.CenterWrite(@"(E)mbark | (H)elp | (A)bout | (Q)uit");
            if (!fastDraw) IncreaseWindow(ref height);
            // return Console.ReadKey().Key;
        }

        private static void ConfigureWindow()
        {
            window = new RenderingWindow("CALIBAN", width, height);
            buffer = new RenderingBuffer(width, height);

            Console.Title = "CALIBAN";
            var hwnd = Process.GetCurrentProcess().MainWindowHandle;
            var style = Windows.Windows.GetWindowLong(hwnd, Windows.Windows.GWL_STYLE);
            Windows.Windows.SetWindowLong(hwnd, Windows.Windows.GWL_STYLE, (style & ~ Windows.Windows.WS_CAPTION));
            var sWidth = Screen.PrimaryScreen.Bounds.Width;
            Windows.Windows.RECT r;
            Windows.Windows.GetWindowRect(hwnd, out r);

            Windows.Windows.SetWindowPos(hwnd, IntPtr.Zero, (sWidth / 2) - (r.Width / 2), -10, 0, 0,
                Windows.Windows.Swp.NOSIZE);
        }

        private static void IncreaseWindow(ref int _height)
        {
            Console.SetWindowSize(width, ++_height);
            Console.SetBufferSize(width, _height);
        }

        public static void About(bool fastDraw)
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
            ConsoleFormat.CenterWrite("This is the about page. Any questions?");
            IncreaseWindow(ref height);
            ConsoleFormat.CenterWrite("");
            IncreaseWindow(ref height);
            ConsoleFormat.CenterWrite("Press [Esc] to return to Main Menu.");
            IncreaseWindow(ref height);
        }

        public static void Standby()
        {
            int height = 5;
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
            Console.ForegroundColor = ConsoleColor.Red;
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
                ConsoleFormat.CenterWrite(line);
                IncreaseWindow(ref height);
            }

            ConsoleFormat.CenterWrite("");
            IncreaseWindow(ref height);
            ConsoleFormat.CenterWrite("Press [Esc] to return to Main Menu.");
            IncreaseWindow(ref height);
            Console.ForegroundColor = ConsoleColor.White;
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
    }
}