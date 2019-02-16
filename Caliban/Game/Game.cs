using System;
using System.Diagnostics;
using System.Net.Sockets;
using Caliban.Transport;
using Caliban.Utility;

namespace Caliban.Game
{
    public class Game
    {
        private readonly ServerTerminal server;
        private WaterLevel waterLevel;
        private readonly bool DebugMode;

        public Game(bool debug)
        {
            DebugMode = debug;
            server = new ServerTerminal();
            server.MessageRecived += ServerOnMessageRecived;
            server.StartListen(5678);
            if (DebugMode)
                D.Init(server);
        }

        public void Start()
        {
            waterLevel = new WaterLevel(server);
        }

        public void Close()
        {
            waterLevel?.Dispose();
            server.BroadcastMessage(Messages.Build(MessageType.GAME_CLOSE, ""));
            server.Close();
        }

        private void ServerOnMessageRecived(Socket socket, byte[] message)
        {
            var msg = Messages.Parse(message);
            switch (msg.Type)
            {
                case MessageType.GAME_CLOSE:
                    break;
                case MessageType.WATERLEVEL_GET:
                    server.SendMessageToClient("WaterMeter",
                        Messages.Build(MessageType.WATERLEVEL_SET, 50.ToString()));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            D.Log("Received " + msg + " from " + socket.Handle);
        }
    }
}