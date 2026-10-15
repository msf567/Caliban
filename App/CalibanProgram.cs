using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Versioning;
using Caliban.Core.Cinematics;
using Caliban.Core.Game;
using Caliban.Core.Transport;
using Caliban.Core.Utility;
using Treasures.Resources;
using Menu = Caliban.Core.Menu.Menu;
using Caliban.Core.Debug;

[assembly: SupportedOSPlatform("windows")]

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

        private static bool isRunning = true;
        private static readonly System.Collections.Concurrent.ConcurrentQueue<Action> mainThreadActions = new System.Collections.Concurrent.ConcurrentQueue<Action>();

        public static void InvokeOnMainThread(Action action)
        {
            mainThreadActions.Enqueue(action);
        }

        [STAThread]
        public static void Main(string[] _args)
        {
            D.debugMode = _args.Contains("debug");
            if (D.debugMode)
            {
                if (!Process.GetProcessesByName("DebugLog").Any())
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "DebugLog.exe",
                        UseShellExecute = true,
                        CreateNoWindow = false
                    });
                }
            }

            D.Init();

            // Load Treasures.dll at runtime and hold the reference. The treasure logic lives in
            // Caliban., Core, but the actual resources (embedded subprogram exes, etc.) are pulled
            // from this separately built DLL, so it can be rebuilt without rebuilding the solution.
            TreasureManager.LoadTreasures();

            server.StartListen(5678);
            server.MessageReceived += ServerOnMessageReceived;
            ModuleLoader.ModuleLoaded += ModuleLoaderOnModuleLoaded;

            Game.OnGameStateChange += OnGameStateChange;

            Caliban.Core.Menu.MenuApp.EnsureInit();
            Caliban.Core.Menu.MenuApp.OnKeyPressed = (key) => RootKeyHandler(key);

            menuState = D.debugMode ? MenuState.MAIN : MenuState.INTRO;
            Menu.Main();
            if (!D.debugMode)
            {
                Menu.HideMenu();
                Cinematic introCinematic = new Cinematic(server, "Intro");
                CinematicPlayer.PlayCinematic(introCinematic);
            }

            RunGraphics();

            while (isRunning)
            {
                while (mainThreadActions.TryDequeue(out var action))
                {
                    action();
                }

                System.Windows.Forms.Application.DoEvents();
                System.Threading.Thread.Sleep(10);
            }

            // Run the heavy teardown here, off the UI event path, so a slow or failing
            // shutdown step can never block/crash the loop and leave the menu frozen.
            ShutdownServices();
        }

        private static void ModuleLoaderOnModuleLoaded(string processName)
        {
            D.Write("Module Loaded: " + processName);
        }

        private static void ServerOnMessageReceived(Socket socket, byte[] message)
        {
            Message m = Messages.Parse(message);
            switch (m.Type)
            {
                case MessageType.CHOREO:
                    D.Write(m.ToString());
                    switch (m.Value)
                    {
                        case "DESKTOP_BG":
                            string folderLoc = AppDomain.CurrentDomain.BaseDirectory;
                            TreasureManager.Spawn(folderLoc, new Treasure("desert.jpg"));
                            _ = Wallpaper.Set(new Uri(Path.Combine(folderLoc, "desert.jpg")), Wallpaper.Style.Stretched);
                            break;
                        case "SHOW_MENU":
                            InvokeOnMainThread(() =>
                            {
                                Menu.ShowMenu();
                                menuState = MenuState.MAIN;
                                Menu.Main();
                            });
                            break;
                        case "INTRO_NOTE":
                            InvokeOnMainThread(Menu.TriggerIntoNote);
                            break;
                    }

                    break;
                case MessageType.DEBUG_LOG:
                    D.Write(m.Value);
                    break;
                case MessageType.HEARTBEAT:
                    server.SendMessageToClient(m.SenderID, Messages.Build("CALIBAN", MessageType.HEARTBEAT, ""));
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
            // GetProcessesByName expects the name WITHOUT the .exe extension.
            foreach (var process in Process.GetProcessesByName("Graphics"))
            {
                try
                {
                    process.Kill();
                }
                catch (Exception)
                {
                    // ignored - process may already have exited
                }
            }

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

        // Top-level WinForms key handler replacing the old Console.ReadKey state
        // machine. Returns true when the menu consumes the key.
        private static bool RootKeyHandler(System.Windows.Forms.Keys key)
        {
            if (key == System.Windows.Forms.Keys.Escape)
                return HandleEscape();

            if (menuState == MenuState.MAIN)
            {
                switch (key)
                {
                    case System.Windows.Forms.Keys.A:
                        D.Write("Menu key: About");
                        Menu.About();
                        menuState = MenuState.ABOUT;
                        return true;
                    case System.Windows.Forms.Keys.H:
                        D.Write("Menu key: Help");
                        Menu.Help();
                        menuState = MenuState.HELP;
                        return true;
                    case System.Windows.Forms.Keys.E:
                        D.Write("Menu key: Embark");
                        NewGame();
                        return true;
                    case System.Windows.Forms.Keys.Q:
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
                    InvokeOnMainThread(() =>
                    {
                        Menu.Win();
                        CloseCurrentGame();
                    });
                    break;
                case GameState.LOST:
                    InvokeOnMainThread(() =>
                    {
                        Menu.Lose();
                        CloseCurrentGame();
                    });
                    break;
                case GameState.CHEATED:
                    InvokeOnMainThread(() =>
                    {
                        Menu.Cheat(cheatReason);
                        CloseCurrentGame();
                    });
                    break;
                case GameState.IN_PROGRESS:
                    InvokeOnMainThread(() =>
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

        // Requests the app to close. This only asks the loop to stop; the actual
        // teardown runs after the loop returns (see Main). RequestStop is marshaled
        // onto the main loop, so this is safe to call from background socket threads (APP_CLOSE)
        // as well as from the UI key handler ('q'), and it can never leave the menu frozen.
        private static void CloseApp()
        {
            InvokeOnMainThread(() => isRunning = false);
        }

        // Tears down all services. Runs on the main thread after the Terminal.Gui loop exits.
        private static void ShutdownServices()
        {
            try
            {
                server.BroadcastMessage(MessageType.APP_CLOSE, "");
            }
            catch (Exception)
            {
                // ignored
            }

            CinematicPlayer.StopActive();
            CloseCurrentGame();
            ClearGraphics();
            //Menu.ZipUp();
            server.Close();
            D.Dispose();
        }
    }
}