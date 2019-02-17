using System;
using System.Net.Sockets;
using Caliban.Core.Desert;
using Caliban.Core.Transport;
using Caliban.Core.Utility;

namespace Caliban.Core.Game
{
    public class Game
    {
        private readonly ServerTerminal server;
        private WaterLevel waterLevel;

        public Game(bool _debug)
        {
            server = new ServerTerminal();
            server.MessageReceived += ServerOnMessageReceived;
            server.StartListen(5678);
            if (_debug)
                D.Init(server);
        }

        public void Start()
        {
            waterLevel = new WaterLevel(server);
          //  DesertGenerator.GenerateDesert();
        }

        public void Close()
        {
            waterLevel?.Dispose();
            server.BroadcastMessage(Messages.Build(MessageType.GAME_CLOSE, ""));
            server.Close();
            DesertGenerator.ClearDesert();
        }

        private void ServerOnMessageReceived(Socket _socket, byte[] _message)
        {
            var msg = Messages.Parse(_message);
            switch (msg.Type)
            {
                case MessageType.GAME_CLOSE:
                    break;
                case MessageType.WATERLEVEL_GET:
                    server.SendMessageToClient("WaterMeter",
                        Messages.Build(MessageType.WATERLEVEL_SET, 50.ToString()));
                    break;
                case MessageType.WATERLEVEL_ADD:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            D.Log("Received " + msg + " from " + _socket.Handle);
        }
    }
}