using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Caliban.Core.Audio;
using Caliban.Core.Debug;

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

        static Menu()
        {
            AudioManager.LoadFile("town_dusk_short.wav", "IntroMusic");
            //AudioManager.LoadFile(Treasures.Treasures.GetStream("town_dusk_short.wav"), "IntroMusic");
        }

        public static void ZipUp()
        {
            int height = Console.WindowHeight;
            while (height > 1)
            {
                try
                {
                    Console.SetWindowSize(width, --height);
                    Console.SetBufferSize(width, height);
                }
                catch (Exception)
                {
                    break;
                }
            }
        }

        public static void Intro()
        {
            HideMenu();
        }

        public static void Intro_Legacy()
        {
            HideMenu();
            AudioManager.PlaySound("IntroMusic", true);
            Thread.Sleep(14_754);
            TriggerIntoNote();
            Thread.Sleep(14_754);
            ShowMenu();
            Main();
        }

        public static void TriggerIntoNote()
        {
            Process.Start("Note.exe", "Intro.txt");
        }

        public static void HideMenu()
        {
            if (MenuApp.IsInitialized)
            {
                MenuApp.Form.Hide();
            }
        }

        public static void ShowMenu()
        {
            if (MenuApp.IsInitialized)
            {
                MenuApp.Form.Show();
                // We shouldn't need DockToTop anymore as UpdateScreen handles it, but let's just make sure it's visible
            }
        }

        public static void Main()
        {
            const int height = 22;

            Version version = System.Reflection.Assembly.GetEntryAssembly().GetName().Version;
            string displayableVersion =
                $"Alpha Version: {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";

            var screen = new MenuScreen();
            screen.WriteLine(displayableVersion, Color.DarkGray);

            screen.CenterWrite("C Presents", Color.Yellow);
            screen.CenterWrite("~~~~", Color.Yellow);
            screen.CenterWrite("A File System Survival Game", Color.Yellow);
            screen.CenterWrite("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~",
                Color.Yellow);
            screen.CenterWrite("");
            foreach (var s in titleGraphic)
            {
                screen.CenterWrite(s, Color.Gold);
            }

            screen.CenterWrite("");
            screen.CenterWrite("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~",
                Color.Yellow);
            screen.CenterWrite("");
            screen.CenterWrite("");
            screen.CenterWrite(@"(E)mbark | (H)elp | (A)bout | (Q)uit");

            MenuApp.Show(screen, height);
        }

        public static void About()
        {
            const int height = 13;

            var screen = new MenuScreen();
            screen.CenterWrite("");

            screen.CenterWrite("Will, I left this here for you.");
            screen.CenterWrite("");
            screen.CenterWrite("Made with the assistance of");
            screen.CenterWrite("");

            screen.CenterWrite("☼ Gentle_Virus ☼", Color.Gold);
            screen.CenterWrite("");

            screen.CenterWrite("♫ Wallhax ♫", Color.Coral);
            screen.CenterWrite("");

            screen.CenterWrite("");
            screen.CenterWrite("Press [Esc] to return to Main Menu.");

            MenuApp.Show(screen, height);
        }

        public static void Help()
        {
            const int height = 12;

            var screen = new MenuScreen();
            screen.CenterWrite("");
            screen.CenterWrite("");
            screen.CenterWrite("");
            screen.CenterWrite("Find SimpleVictory.exe. Be sure to drink water.");
            screen.CenterWrite("");
            screen.CenterWrite(
                "Mouse actions are taxing. Key presses are deadly. Don't even think about a CLI.");
            screen.CenterWrite("");
            screen.CenterWrite("There may be some clues for you along the way. Stay vigilant.");
            screen.CenterWrite("");
            screen.CenterWrite("Press [Esc] to return to Main Menu.");

            MenuApp.Show(screen, height);
        }

        public static void Standby()
        {
            int height = 6;

            var screen = new MenuScreen();
            screen.CenterWrite("");
            screen.CenterWrite("Game in progress.");
            screen.CenterWrite("");
            screen.CenterWrite("Press [Esc] to quit and return to Main Menu.");

            MenuApp.Show(screen, height);
        }

        public static void Lose()
        {
            const int height = 13;

            var screen = new MenuScreen();
            screen.CenterWrite("");
            screen.CenterWrite("");
            screen.CenterWrite("");
            foreach (var line in deathGraphic)
            {
                screen.CenterWrite(line, Color.Red);
            }

            screen.CenterWrite("");
            screen.CenterWrite("Press [Esc] to return to Main Menu.", Color.Red);

            MenuApp.Show(screen, height);
        }

        public static void Win()
        {
            const int height = 14;

            var screen = new MenuScreen();
            screen.CenterWrite("");
            screen.CenterWrite("");
            screen.CenterWrite("");
            foreach (var line in victoryGraphic)
            {
                screen.CenterWrite(line, Color.Green);
            }

            screen.CenterWrite("");
            screen.CenterWrite("Press [Esc] to return to Main Menu.", Color.Green);

            MenuApp.Show(screen, height);
        }

        public static void Cheat(string cheatReason)
        {
            const int height = 17;

            var screen = new MenuScreen();
            screen.CenterWrite("");
            screen.CenterWrite("");
            screen.CenterWrite("");
            foreach (var line in cheaterGraphic)
            {
                screen.CenterWrite(line, Color.Red);
            }

            screen.CenterWrite("");
            screen.CenterWrite($"LOL Will, did you really think I wouldn't notice that {cheatReason}?", Color.Red);
            screen.CenterWrite("");
            screen.CenterWrite("Press [Esc] to return to Main Menu.", Color.Red);

            MenuApp.Show(screen, height);
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