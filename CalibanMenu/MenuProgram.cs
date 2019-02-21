using System;
using System.Diagnostics;
using System.Threading;
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
            INTRO,
            STANDBY
        }

        private static MenuState menuState = MenuState.MAIN;

        public static void Main(string[] _args)
        {
            Windows.ConfigureMenuWindow();
            Game.OnGameStateChange += OnGameStateChange;
            var userKey = ConsoleKey.M;
          //  bool playIntro = false;
         //   menuState = playIntro ? MenuState.INTRO : MenuState.MAIN;
            Menu.Main(false, false);
            menuState = MenuState.MAIN;
            while (!closeFlag)
            {
                switch (menuState)
                {
                    case MenuState.MAIN:
                        if (userKey == ConsoleKey.A)
                        {
                            Menu.About(false);
                            menuState = MenuState.ABOUT;
                        }

                        else if (userKey == ConsoleKey.E)
                        {
                            NewGame(false);
                        }

                        else if (userKey == ConsoleKey.Q)
                        {
                            CloseApp();
                            continue;
                        }
                        else
                        {
                            Menu.Main(true);
                        }

                        break;
                    case MenuState.ABOUT:
                        if (userKey == ConsoleKey.Escape)
                        {
                            Menu.Main(menuState == MenuState.MAIN);
                            Game.CurrentGame?.Close();
                            menuState = MenuState.MAIN;
                        }
                        else
                        {
                            Menu.About(true);
                        }

                        break;
                    case MenuState.HELP:
                        break;
                    case MenuState.STANDBY:
                        if (userKey == ConsoleKey.Escape)
                        {
                            Menu.Main(menuState == MenuState.MAIN);
                            Game.CurrentGame?.Close();
                            menuState = MenuState.MAIN;
                        }
                        else
                            Menu.Standby();

                        break;
                    case MenuState.INTRO:
                        break;
                }

                if (!closeFlag)
                    userKey = Console.ReadKey().Key;
            }
        }

        private static void OnGameStateChange(GameState _state)
        {
            switch (_state)
            {
                case GameState.WON:
                    Menu.Win();
                    Game.CurrentGame?.Close();
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