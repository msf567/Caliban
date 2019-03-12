﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Caliban.Core.Game;
using Caliban.Core.OS;
using Caliban.Core.Utility;
using Caliban.Core.Treasures;
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

        [STAThread]
        public static void Main(string[] _args)
        {
            string folderLoc = AppDomain.CurrentDomain.BaseDirectory;
            Treasures.Spawn(folderLoc, "desert.jpg");
            Wallpaper.Set(new Uri(Path.Combine(folderLoc,"desert.jpg")), Wallpaper.Style.Stretched);
            Windows.ConfigureMenuWindow();
            Game.OnGameStateChange += OnGameStateChange;
            var userKey = ConsoleKey.M;
            D.debugMode = _args.Contains("debug");
            menuState = D.debugMode ? MenuState.MAIN : MenuState.INTRO;

            if (D.debugMode)
                Menu.Main(false);
            else
                Menu.Intro();

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
                        if (userKey == ConsoleKey.H)
                        {
                            Menu.Help(false);
                            menuState = MenuState.HELP;
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
                        if (userKey == ConsoleKey.Escape)
                        {
                            Menu.Main(menuState == MenuState.MAIN);
                            Game.CurrentGame?.Close();
                            menuState = MenuState.MAIN;
                        }
                        else
                        {
                            Menu.Help(true);
                        }
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
                //D.Write("Waiting for modules to load...");
                return;
            }

            Game.CurrentGame?.Close();

            ModuleLoader.Clear();
            Game.CurrentGame = new Game(_debug);
            Game.CurrentGame.Start();
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