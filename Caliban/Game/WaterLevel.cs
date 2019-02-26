using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using Caliban.Core.Transport;
using Caliban.Core.Utility;
using Caliban.Core.Windows;

namespace Caliban.Core.Game
{
    public class WaterLevel
    {
        private readonly ServerTerminal server;
        private float CurrentLevel { get; set; }

        public WaterLevel(ServerTerminal _s)
        {
            server = _s;
            server.MessageReceived += ServerOnMessageReceived;
            //D.Write("Subscribed...");
            CurrentLevel = 80;
            GlobalInput.OnGlobalMouseMove += OnGlobalMouseMove;
            GlobalInput.OnGlobalKeyPress += OnGlobalKeyPress;

            ModuleLoader.LoadModuleAndWait(@"WaterMeter.exe", "WaterMeter");
        }

        private void ServerOnMessageReceived(Socket __socket, byte[] _message)
        {
            var m = Messages.Parse(_message);
            switch (m.Type)
            {
                case MessageType.WATERLEVEL_ADD:
                    CurrentLevel += int.Parse(m.Value);
                    CurrentLevel = CurrentLevel.Clamp(0, 100);
                    break;
                case MessageType.WATERLEVEL_GET:
                    server.SendMessageToClient("WaterMeter",
                        Messages.Build(MessageType.WATERLEVEL_SET, CurrentLevel.ToString()));
                    break;
            }
        }

        public void Update()
        {
            CurrentLevel.Clamp(0, 100);
            if (CurrentLevel < 0 && Game.CurrentGame.state == GameState.IN_PROGRESS)
                server.SendMessageToSelf(Messages.Build(MessageType.GAME_LOSE, ""));

            server.SendMessageToClient("WaterMeter",
                Messages.Build(MessageType.WATERLEVEL_SET, CurrentLevel.ToString()));
            Thread.Sleep(70);
        }


        private void OnGlobalMouseMove(MouseArgs _e)
        {
            CurrentLevel -= 0.01f;
            if (_e.Message == MouseMessages.WM_LBUTTONDOWN)
                CurrentLevel--;
        }

        private void OnGlobalKeyPress(string _key)
        {
            //CurrentLevel -= 0.2f;
        }

        public void Dispose()
        {
        }
    }
}