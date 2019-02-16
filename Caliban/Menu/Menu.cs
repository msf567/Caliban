using System;
using System.Diagnostics;
using System.Media;
using System.Threading;
using System.Windows.Forms;
using Caliban.Core.ConsoleOutput;
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

    public class Menu
    {
        private readonly SoundPlayer music = new SoundPlayer();
        private int width = 90;
        private int height = 5;
        private RenderingWindow window;
        private RenderingBuffer buffer;

        public Menu()
        {
            ConfigureWindow();
        }

        public void Close()
        {
            int height = Console.WindowHeight;
            while (height > 1)
            {
                Console.SetWindowSize(width, --height);
                Console.SetBufferSize(width, height);
            }
        }

        public void Main(bool _introMode = false)
        {
            Console.Clear();
            int height = 1;
            Console.SetWindowSize(width, height);
            Console.SetBufferSize(width, height);
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            if (_introMode)
            {
                music.SoundLocation = "desert.wav";
                music.Load();
                Thread.Sleep(2000);
                music.Play();
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            ConsoleFormat.CenterWrite("~~~");
            IncreaseWindow(ref height);
            IncreaseWindow(ref height);
            ConsoleFormat.CenterWrite("");
            ConsoleFormat.CenterWrite("");
            ConsoleFormat.CenterWrite("Gentle_Virus Presents");
            IncreaseWindow(ref height);
            ConsoleFormat.CenterWrite("~~~");
            IncreaseWindow(ref height);
            if (_introMode) Thread.Sleep(2398);
            ConsoleFormat.CenterWrite("A File System Survival Game");
            IncreaseWindow(ref height);
            IncreaseWindow(ref height);
            IncreaseWindow(ref height);
            if (_introMode) Thread.Sleep(2412);
            ConsoleFormat.CenterWrite("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            ConsoleFormat.CenterWrite("");
            foreach (var s in titleGraphic)
            {
                ConsoleFormat.CenterWrite(s);
                IncreaseWindow(ref height);
                if (_introMode) Thread.Sleep(594);
            }

            ConsoleFormat.CenterWrite("");
            IncreaseWindow(ref height);
            ConsoleFormat.CenterWrite("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            IncreaseWindow(ref height);
            ConsoleFormat.CenterWrite("");
            IncreaseWindow(ref height);
            ConsoleFormat.CenterWrite("");
            IncreaseWindow(ref height);

            Console.ForegroundColor = ConsoleColor.White;
            ConsoleFormat.CenterWrite(@"(E)mbark | (H)elp | (A)bout | (Q)uit");
            IncreaseWindow(ref height);
           // return Console.ReadKey().Key;
        }

        private void ConfigureWindow()
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

        private void IncreaseWindow(ref int _height)
        {
            Console.SetWindowSize(width, ++_height);
            Console.SetBufferSize(width, _height);
        }

        private readonly string[] titleGraphic =
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

        public void About()
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
    }
}