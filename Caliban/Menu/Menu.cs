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
            Main();
        }

        public static void Main()
        {
            Console.Clear();
            int height = 22;

            Console.SetWindowSize(width, height);
            Console.SetBufferSize(width, height);
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            
            Version version = System.Reflection.Assembly.GetEntryAssembly().GetName().Version;
            string displayableVersion = $"Alpha Version: {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
            ConsoleFormat.WriteLine(displayableVersion, Color.DarkGray);

            ConsoleFormat.CenterWrite("C Presents", Color.Yellow);
            ConsoleFormat.CenterWrite("~~~~", Color.Yellow);
            ConsoleFormat.CenterWrite("A File System Survival Game", Color.Yellow);
            ConsoleFormat.CenterWrite("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~",
                Color.Yellow);
            ConsoleFormat.CenterWrite("");
            foreach (var s in titleGraphic)
            {
                ConsoleFormat.CenterWrite(s, Color.Gold);
            }

            ConsoleFormat.CenterWrite("");
            ConsoleFormat.CenterWrite("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~",
                Color.Yellow);
            ConsoleFormat.CenterWrite("");
            ConsoleFormat.CenterWrite("");
            ConsoleFormat.CenterWrite(@"(E)mbark | (H)elp | (A)bout | (Q)uit");
        }

        public static void About()
        {
            int height = 13;
            Console.SetWindowSize(width, height);
            Console.SetBufferSize(width, height);
            Console.Clear();

            ConsoleFormat.CenterWrite("");

            ConsoleFormat.CenterWrite("Will, I left this here for you.");
            ConsoleFormat.CenterWrite("");
            ConsoleFormat.CenterWrite("Made with the assistance of");
            ConsoleFormat.CenterWrite("");

            ConsoleFormat.CenterWrite("☼ Gentle_Virus ☼", Color.Gold);
            ConsoleFormat.CenterWrite("");

            ConsoleFormat.CenterWrite("♫ Wallhax ♫", Color.Coral);
            ConsoleFormat.CenterWrite("");

            ConsoleFormat.CenterWrite("");
            ConsoleFormat.CenterWrite("Press [Esc] to return to Main Menu.");
        }

        public static void Help()
        {
            int height = 12;
            Console.SetWindowSize(width, height);
            Console.SetBufferSize(width, height);
            Console.Clear();
            ConsoleFormat.CenterWrite("");
            ConsoleFormat.CenterWrite("");
            ConsoleFormat.CenterWrite("");
            ConsoleFormat.CenterWrite("Find SimpleVictory.exe. Be sure to drink water.");
            ConsoleFormat.CenterWrite("");
            ConsoleFormat.CenterWrite(
                "Mouse actions are taxing. Key presses are deadly. Don't even think about a CLI.");
            ConsoleFormat.CenterWrite("");
            ConsoleFormat.CenterWrite("There may be some clues for you along the way. Stay vigilant.");
            ConsoleFormat.CenterWrite("");
            ConsoleFormat.CenterWrite("Press [Esc] to return to Main Menu.");
        }

        public static void Standby()
        {
            int height = D.debugMode ? 10 : 6;
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
            int height = 13;
            Console.SetWindowSize(width, height);
            Console.SetBufferSize(width, height);
            Console.Clear();
            ConsoleFormat.CenterWrite("");
            ConsoleFormat.CenterWrite("");
            ConsoleFormat.CenterWrite("");
            foreach (var line in deathGraphic)
            {
                ConsoleFormat.CenterWrite(line, Color.Red);
            }

            ConsoleFormat.CenterWrite("");
            ConsoleFormat.CenterWrite("Press [Esc] to return to Main Menu.", Color.Red);
        }

        public static void Win()
        {
            const int height = 14;
            Console.SetWindowSize(width, height);
            Console.SetBufferSize(width, height);
            Console.Clear();
            ConsoleFormat.CenterWrite("");
            ConsoleFormat.CenterWrite("");
            ConsoleFormat.CenterWrite("");
            foreach (var line in victoryGraphic)
            {
                ConsoleFormat.CenterWrite(line, Color.Green);
            }

            ConsoleFormat.CenterWrite("");
            ConsoleFormat.CenterWrite("Press [Esc] to return to Main Menu.", Color.Green);
        }

        public static void Cheat()
        {
            Console.SetWindowSize(width, 17);
            Console.SetBufferSize(width, 17);
            Console.Clear();
            ConsoleFormat.CenterWrite("");
            ConsoleFormat.CenterWrite("");
            ConsoleFormat.CenterWrite("");
            foreach (var line in cheaterGraphic)
            {
                ConsoleFormat.CenterWrite(line, Color.Red);
            }

            ConsoleFormat.CenterWrite("");
            ConsoleFormat.CenterWrite("LOL Will, Did you think I wouldn't notice?", Color.Red);
            ConsoleFormat.CenterWrite("");
            ConsoleFormat.CenterWrite("Press [Esc] to return to Main Menu.", Color.Red);
        }

        private static void ConfigureWindow()
        {
            window = new RenderingWindow("CALIBAN", width, 20);
            buffer = new RenderingBuffer(width, 20);
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