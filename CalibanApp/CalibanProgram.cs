using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Forms;
using Caliban.Core.Game;
using Caliban.Core.OS;
using Caliban.Core.Transport;
using Caliban.Core.Utility;
using Treasures.Resources;
using Menu = Caliban.Core.Menu.Menu;

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

        [STAThread]
        public static void Main(string[] _args)
        {
            D.debugMode = _args.Contains("debug");
            D.Init();
            server.StartListen(5678);
            server.MessageReceived += ServerOnMessageReceived;
            ModuleLoader.ModuleLoaded += ModuleLoaderOnModuleLoaded;

            string folderLoc = AppDomain.CurrentDomain.BaseDirectory;
            TreasureManager.Spawn(folderLoc, new Treasure("desert.jpg"));
            Wallpaper.Set(new Uri(Path.Combine(folderLoc, "desert.jpg")), Wallpaper.Style.Stretched);

            //RunPackagedUnity();
            Game.OnGameStateChange += OnGameStateChange;

            Windows.ConfigureMenuWindow();
            menuState = D.debugMode ? MenuState.MAIN : MenuState.INTRO;

            RunUnity();
            if (D.debugMode)
            {
                Menu.Main();
                menuState = MenuState.MAIN;
            }
            else
            {
                menuState = MenuState.INTRO;
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
            switch (processName)
            {
                case "Unity":
                    if (!D.debugMode)
                    {
                        Menu.Intro();
                        server.SendMessageToClient("Unity", Messages.Build(MessageType.CHOREO, "StartIntro"));
                    }

                    break;
            }
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
                            D.Write("Switching Desktop");
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
            }
        }

        private static void RunUnity()
        {
            var filePath = Path.Combine(AppContext.BaseDirectory, "CalibanUnity.exe");
            Process.Start(filePath, D.debugMode ? "debug" : "");
        }

        private static void RunPackagedUnity()
        {
            var filePath = Path.Combine(AppContext.BaseDirectory, "CU.exe");
            if (File.Exists(filePath))
                File.Delete(filePath);
            TreasureManager.Spawn(AppContext.BaseDirectory, new Treasure("CU.exe"));
            if (!File.Exists("CU.exe"))
                return;

            Process.Start("CU.exe", D.debugMode ? "debug" : "");
        }

        private static void ClearUnity()
        {
            foreach (var process in Process.GetProcessesByName("CU.exe"))
                process.Kill();

            if (File.Exists("CU.exe"))
                try
                {
                    File.Delete("CU.exe");
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

        private static void OnGameStateChange(GameState _state)
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
                    Menu.Cheat();
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
            server.BroadcastMessage(Messages.Build(MessageType.APP_CLOSE, ""));
            CloseCurrentGame();
            ClearUnity();
            Menu.Close();
            server.Close();
            closeFlag = true;
        }
    }
}