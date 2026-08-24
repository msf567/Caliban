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
using Terminal.Gui;
using TerminalApp = Terminal.Gui.Application;

namespace CALIBAN
{
    internal static class CalibanProgram
    {
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

            Caliban.Core.Menu.MenuApp.EnsureInit();
            TerminalApp.RootKeyEvent = RootKeyHandler;

            menuState = D.debugMode ? MenuState.MAIN : MenuState.INTRO;
            Menu.Main();
            if (!D.debugMode)
            {
                Menu.HideMenu();
                Cinematic introCinematic = new Cinematic(server, "Intro");
                CinematicPlayer.PlayCinematic(introCinematic);
            }

            RunGraphics();
            TerminalApp.Run();
            TerminalApp.Shutdown();
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
                            TerminalApp.MainLoop.Invoke(() =>
                            {
                                Menu.ShowMenu();
                                menuState = MenuState.MAIN;
                                Menu.Main();
                            });
                            break;
                        case "INTRO_NOTE":
                            TerminalApp.MainLoop.Invoke(() => Menu.TriggerIntoNote());
                            break;
                    }

                    break;
                case MessageType.DEBUG_LOG:
                    D.Write(m.Value);
                    break;
                case MessageType.APP_CLOSE:
                    CloseApp();
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

        // Top-level Terminal.Gui key handler replacing the old Console.ReadKey state
        // machine. Returns true when the key was consumed by the menu.
        private static bool RootKeyHandler(KeyEvent _kb)
        {
            uint masks = (uint)(Key.ShiftMask | Key.AltMask | Key.CtrlMask);
            int stripped = (int)((uint)_kb.Key & ~masks);

            if (stripped == (int)Key.Esc)
                return HandleEscape();

            char c = char.ToLowerInvariant((char)(stripped & 0xFF));

            if (menuState == MenuState.MAIN)
            {
                switch (c)
                {
                    case 'a':
                        D.Write("Menu key: About");
                        Menu.About();
                        menuState = MenuState.ABOUT;
                        return true;
                    case 'h':
                        D.Write("Menu key: Help");
                        Menu.Help();
                        menuState = MenuState.HELP;
                        return true;
                    case 'e':
                        D.Write("Menu key: Embark");
                        NewGame();
                        return true;
                    case 'q':
                        D.Write("Menu key: Quit");
                        CloseApp();
                        return true;
                }
            }

            return false;
        }

        private static bool HandleEscape()
        {
            switch (menuState)
            {
                case MenuState.ABOUT:
                case MenuState.HELP:
                case MenuState.STANDBY:
                    D.Write("Menu key: Escape -> Main");
                    Menu.Main();
                    CloseCurrentGame();
                    menuState = MenuState.MAIN;
                    return true;
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
                    TerminalApp.MainLoop.Invoke(() =>
                    {
                        Menu.Win();
                        CloseCurrentGame();
                    });
                    break;
                case GameState.LOST:
                    TerminalApp.MainLoop.Invoke(() =>
                    {
                        Menu.Lose();
                        CloseCurrentGame();
                    });
                    break;
                case GameState.CHEATED:
                    TerminalApp.MainLoop.Invoke(() =>
                    {
                        Menu.Cheat(cheatReason);
                        CloseCurrentGame();
                    });
                    break;
                case GameState.IN_PROGRESS:
                    TerminalApp.MainLoop.Invoke(() =>
                    {
                        Menu.Standby();
                        menuState = MenuState.STANDBY;
                    });
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
            CinematicPlayer.StopActive();
            CloseCurrentGame();
            ClearGraphics();
            //Menu.ZipUp();
            server.Close();
            D.Dispose();
            TerminalApp.RequestStop();
        }
    }
}