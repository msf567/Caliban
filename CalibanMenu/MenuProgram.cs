using System;
using System.Threading;
using Caliban.Core.Game;
using Caliban.Core.Utility;
using Caliban.Core.Windows;
using Menu = Caliban.Core.Menu.Menu;

namespace CalibanMenu
{
    internal static class CalibanCoreProject
    {
        private static bool closeFlag;
        private static Game currentGame = new Game(false);
        private static Menu menu;

        public static void Main(string[] _args)
        {
            Windows.ConfigureMenuWindow();

            menu = new Menu();
            var userKey = ConsoleKey.Escape;
            menu.Main();
            while (!closeFlag)
            {
                switch (userKey)
                {
                    case ConsoleKey.Escape:
                        menu.Main();
                        break;
                    case ConsoleKey.E:
                        NewGame(false);
                        break;
                    case ConsoleKey.A:
                        menu.About();
                        break;
                    case ConsoleKey.C:
                        CloseGame();
                        break;
                    case ConsoleKey.Q:
                        CloseApp();
                        continue;
                }

                userKey = Console.ReadKey().Key;
            }
        }


        private static void NewGame(bool _debug)
        {
            if (!ModuleLoader.IsReady())
            {
                Console.WriteLine("Waiting for modules to load...");
                return;
            }
            currentGame?.Close();
            
            ModuleLoader.Clear();
            currentGame = new Game(_debug);
            currentGame.Start();
        }

        private static void CloseGame()
        {
            currentGame?.Close();
            currentGame = null;
        }

        private static void CloseApp()
        {
            D.Close();
            menu.Close();
            currentGame?.Close();
            closeFlag = true;
        }
    }
}