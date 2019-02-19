using System;
using System.Diagnostics;
using System.Threading;
using Caliban.Core.Desert;
using Caliban.Core.Game;
using Caliban.Core.Utility;
using Caliban.Core.Windows;
using Menu = Caliban.Core.Menu.Menu;

namespace CalibanMenu
{
    internal static class MenuProgram
    {
        private static bool closeFlag;

        private enum MenuState
        {
            MAIN,
            ABOUT,
            HELP,
            STANDBY
        }

        private static MenuState menuState = MenuState.MAIN;

        public static void Main(string[] _args)
        {
            Windows.ConfigureMenuWindow();
            Game.OnGameStateChange += OnGameStateChange;
            var userKey = ConsoleKey.Escape;
            Menu.Main(false,false);
            while (!closeFlag)
            {
                switch (userKey)
                {
                    case ConsoleKey.Escape:
                        Menu.Main(menuState == MenuState.MAIN);
                        Game.CurrentGame?.Close();
                        menuState = MenuState.MAIN;
                        break;
                    case ConsoleKey.E:
                        if (menuState == MenuState.MAIN)
                            NewGame(false);
                        break;
                    case ConsoleKey.A:
                        Menu.About(menuState != MenuState.ABOUT);
                        menuState = MenuState.ABOUT;
                        break;
                    case ConsoleKey.Q:
                        if (menuState == MenuState.MAIN)
                            CloseApp();
                        continue;
                    default:
                        Menu.Main(menuState == MenuState.MAIN);
                        menuState = MenuState.MAIN;
                        break;
                }

                userKey = Console.ReadKey().Key;
            }
        }

        private static void OnGameStateChange(GameState _state)
        {
            switch (_state)
            {
                case GameState.WON:
                    break;
                case GameState.LOST:
                    Menu.Lose();
                    Game.CurrentGame?.Close();
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


        private static void NewGame(bool _debug)
        {
            if (!ModuleLoader.IsReady())
            {
                //Console.WriteLine("Waiting for modules to load...");
                return;
            }

            Game.CurrentGame?.Close();

            ModuleLoader.Clear();
            Game.CurrentGame = new Game(_debug);
            Game.CurrentGame.Start();
        }

        private static void CloseGame()
        {
            Game.CurrentGame?.Close();
            Game.CurrentGame = null;
        }

        private static void CloseApp()
        {
            D.Close();
            Menu.Close();
            Game.CurrentGame?.Close();
            closeFlag = true;
        }
    }
}