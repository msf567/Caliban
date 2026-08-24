using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Forms;
using Caliban.Core.Cinematics;
using Caliban.Core.Game;
using Caliban.Core.OS;
using Caliban.Core.Transport;
using Caliban.Core.Utility;
using Treasures.Resources;
using Menu = Caliban.Core.Menu.Menu;
using Caliban.Core.Debug;

namespace CALIBAN
{
    internal static class CalibanProgram
    {
        private static bool closeFlag;

        private enum MenuState
        {
            MAIN,
            ABOUT,
            HELP,
            INTRO,
            STANDBY,
        }

        private static MenuState menuState = MenuState.MAIN;
        private static readonly ServerTerminal server = new ServerTerminal();
#pragma warning disable CS0169
        private static string CheatReason;
#pragma warning restore CS0169

        [STAThread]
        public static void Main(string[] _args)
        {
            D.debugMode = _args.Contains("debug");
            if (D.debugMode)
            {
                if (!Process.GetProcessesByName("DebugLog").Any())
                {
                    Process.Start("DebugLog.exe");
                }
            }

            D.Init();
            server.StartListen(5678);
            server.MessageReceived += ServerOnMessageReceived;
            ModuleLoader.ModuleLoaded += ModuleLoaderOnModuleLoaded;

            Game.OnGameStateChange += OnGameStateChange;

            Windows.ConfigureMenuWindow();
            menuState = D.debugMode ? MenuState.MAIN : MenuState.INTRO;
            if (D.debugMode)
            {
                Menu.Main();
                menuState = MenuState.MAIN;
            }
            else
            {
                Menu.HideMenu();
                RunGraphics();
                menuState = MenuState.INTRO;
                Cinematic introCinematic = new Cinematic(server, "Intro");
                CinematicPlayer.PlayCinematic(introCinematic);
            }


            var userKey = ConsoleKey.M;
            while (!closeFlag)
            {
                if (MenuLoop(userKey)) continue;

                if (!closeFlag)
                    userKey = Console.ReadKey().Key;
            }
        }

        private static void ModuleLoaderOnModuleLoaded(string processName)
        {
            D.Write("Module Loaded: " + processName);
        }

        private static void ServerOnMessageReceived(Socket socket, byte[] message)
        {
            Caliban.Core.Transport.Message m = Messages.Parse(message);
            switch (m.Type)
            {
                case MessageType.CHOREO:
                    D.Write(m.ToString());
                    switch (m.Value)
                    {
                        case "DESKTOP_BG":
                            string folderLoc = AppDomain.CurrentDomain.BaseDirectory;
                            TreasureManager.Spawn(folderLoc, new Treasure("desert.jpg"));
                            Wallpaper.Set(new Uri(Path.Combine(folderLoc, "desert.jpg")), Wallpaper.Style.Stretched);
                            break;
                        case "SHOW_MENU":
                            Menu.ShowMenu();
                            menuState = MenuState.MAIN;
                            Menu.Main();
                            break;
                        case "INTRO_NOTE":
                            Menu.TriggerIntoNote();
                            break;
                    }

                    break;
                case MessageType.DEBUG_LOG:
                    D.Write(m.Value);
                    break;
            }
        }

        private static void RunGraphics()
        {
            var filePath = Path.Combine(AppContext.BaseDirectory, "Graphics.exe");
            Process.Start(filePath, D.debugMode ? "debug" : "");
        }

        private static void ClearGraphics()
        {
            foreach (var process in Process.GetProcessesByName("Graphics.exe"))
                process.Kill();

            if (File.Exists("Graphics.exe"))
                try
                {
                    File.Delete("Graphics.exe");
                }
                catch (Exception)
                {
                    // ignored
                }
        }

        private static bool MenuLoop(ConsoleKey userKey)
        {
            switch (menuState)
            {
                case MenuState.MAIN:
                    if (userKey == ConsoleKey.A)
                    {
                        Menu.About();
                        menuState = MenuState.ABOUT;
                    }
                    else if (userKey == ConsoleKey.H)
                    {
                        Menu.Help();
                        menuState = MenuState.HELP;
                    }
                    else if (userKey == ConsoleKey.E)
                    {
                        NewGame();
                    }

                    else if (userKey == ConsoleKey.Q)
                    {
                        CloseApp();
                        return true;
                    }
                    else
                    {
                        Menu.Main();
                    }

                    break;
                case MenuState.ABOUT:
                    if (userKey == ConsoleKey.Escape)
                    {
                        Menu.Main();
                        CloseCurrentGame();

                        menuState = MenuState.MAIN;
                    }
                    else
                    {
                        Menu.About();
                    }

                    break;
                case MenuState.HELP:
                    if (userKey == ConsoleKey.Escape)
                    {
                        Menu.Main();
                        CloseCurrentGame();
                        menuState = MenuState.MAIN;
                    }
                    else
                    {
                        Menu.Help();
                    }

                    break;
                case MenuState.STANDBY:
                    if (userKey == ConsoleKey.Escape)
                    {
                        Menu.Main();
                        CloseCurrentGame();
                        menuState = MenuState.MAIN;
                    }
                    else
                        Menu.Standby();

                    break;
                case MenuState.INTRO:
                    break;
            }

            return false;
        }

        private static void CloseCurrentGame(bool _closeExplorers = true)
        {
            Game.CurrentGame?.Close(_closeExplorers);
            Game.CurrentGame = null;
            server.Clean();
        }

        private static void OnGameStateChange(GameState _state, string cheatReason)
        {
            switch (_state)
            {
                case GameState.WON:
                    Menu.Win();
                    CloseCurrentGame();
                    break;
                case GameState.LOST:
                    Menu.Lose();
                    CloseCurrentGame();
                    break;
                case GameState.CHEATED:
                    Menu.Cheat(cheatReason);
                    CloseCurrentGame();
                    break;
                case GameState.IN_PROGRESS:
                    Menu.Standby();
                    menuState = MenuState.STANDBY;
                    break;
                case GameState.NOT_STARTED:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_state), _state, null);
            }
        }

        private static void NewGame()
        {
            if (!ModuleLoader.IsReady())
            {
            }

            CloseCurrentGame(false);

            ModuleLoader.Clear();
            D.Write("Modules Clear");
            Game.CurrentGame = new Game(server);
            D.Write("Game Created");
            Game.CurrentGame.Start();
        }

        private static void CloseApp()
        {
            D.Write("Broadcasting App Close");
            server.BroadcastMessage(Messages.Build(MessageType.APP_CLOSE, ""));
            D.Write("Stopping Cinematic");
            CinematicPlayer.StopActive();
            D.Write("Closing Current Game");
            CloseCurrentGame();
            D.Write("Closing Graphics Module");
            ClearGraphics();
            D.Write("Closing Menu");
            //Menu.Close();
            D.Write("Closing Server");
            server.Close();
            D.Write("Setting close flag");
            closeFlag = true;
            D.Write("Disposing Debug");
            D.Dispose();
        }
    }
}