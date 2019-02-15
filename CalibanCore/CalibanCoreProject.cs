using System;
using System.Threading;
using CalibanLib.Desert;
using CalibanLib.Transport;
using System.Net;
using System.Net.Sockets;
using System.Text;
using CalibanLib.Game;
using CalibanLib.Utility;
using CalibanLib.Windows;

namespace CalibanCore
{
    internal static class CalibanCoreProject
    {
        private static ServerTerminal _server;

        private static readonly CalibanMenu Menu = new CalibanMenu();
        private static readonly WaterLevel WaterLevel = new WaterLevel();
        private static bool _closeFlag;
        public static void Main(string[] args)
        {
            Windows.DeleteMenu(Windows.GetSystemMenu(Windows.GetConsoleWindow(), false), 
                Windows.SC_CLOSE,
                Windows.MF_BYCOMMAND);
            
            InitServer();
            WaterLevel.Init(_server);
            var userKey = Menu.Show();

            while (!_closeFlag)
            {
                switch (userKey)
                {
                    case 'e':
                        DesertGenerator.GenerateDesert();
                        break;
                    case 'c':
                        DesertGenerator.ClearDesert();
                        break;
                    case 'q':
                        CloseGame();
                        continue;
                }

                userKey = Menu.Show();
            }
        }

        private static void CloseGame()
        {
            WaterLevel.Dispose();
            _server.BroadcastMessage(Messages.Build(MessageType.GAME_CLOSE,""));
            _closeFlag = true;
        }

        private static void InitServer()
        {
            _server = new ServerTerminal();
            _server.ClientConnect += ServerOnClientConnect;
            _server.MessageRecived += ServerOnMessageRecived;
            _server.ClientDisconnect += ServerOnClientDisconnect;
            _server.StartListen(5678);
        }

        private static void ServerOnClientDisconnect(Socket socket)
        {
            Console.WriteLine(socket.Handle.ToInt64() + " disconnected!");
        }

        private static void ServerOnMessageRecived(Socket socket, byte[] message)
        {
            var msg = Messages.Parse(message);
            switch (msg.Type)
            {
                case MessageType.GAME_CLOSE:
                    break;
                case MessageType.WATERLEVEL_GET:
                    _server.SendMessageToClient("WaterMeter",
                        Messages.Build(MessageType.WATERLEVEL_SET, WaterLevel.CurrentLevel.ToString()));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Console.WriteLine("Received " + msg + " from " + socket.Handle);
        }

        private static void ServerOnClientConnect(Socket socket)
        {
            Console.WriteLine("Client Connected!");
        }
    }
}