using Caliban.Game;
using Caliban.Utility;
using Caliban.Windows;
using Menu = Caliban.Menu.Menu;

namespace CalibanMenu
{
    internal static class CalibanCoreProject
    {
        private static bool _closeFlag;
        private static Game currentGame = new Game(true);
        private static Menu menu;

        public static void Main(string[] args)
        {
            Windows.ConfigureMenuWindow();

            menu = new Menu();
            var userKey = menu.Main();

            while (!_closeFlag)
            {
                switch (userKey)
                {
                    case 'e':
                        NewGame();
                        break;
                    case 'a':
                        menu.About();
                        break;
                    case 'c':
                        CloseGame();
                        break;
                    case 'q':
                        CloseApp();
                        continue;
                }

                userKey = menu.Main();
            }
        }


        private static void NewGame()
        {
            currentGame?.Close();
            currentGame = new Game(true);
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
            _closeFlag = true;
        }
    }
}