using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using Caliban.Core.Transport;
using Caliban.Core.Utility;
using Caliban.Core.Windows;
using Caliban.Core.World;

namespace Caliban.Core.Game
{
    public class WaterLevel
    {
        private readonly ServerTerminal server;
        private float CurrentLevel { get; set; }

        private static Dictionary<string,string> WaterPuddles = new Dictionary<string, string>();
        
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
        
        public static void AddWaterPuddle(DesertNode _node)
        {
            _node.AddTreasure("WaterPuddle.exe","MyWater.exe");
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
        
//        public

        private void OnGlobalMouseMove(MouseArgs _e)
        {
            if (_e.Message == MouseMessages.WM_LBUTTONDOWN ||
                _e.Message == MouseMessages.WM_RBUTTONDOWN ||
                _e.Message == MouseMessages.WM_XBUTTONDOWN ||
                _e.Message == MouseMessages.WM_WHEELBUTTONDOWN)
                CurrentLevel--;
            else if (_e.Message != MouseMessages.WM_MOUSEMOVE)
                CurrentLevel -= 0.01f;
        }

        private void OnGlobalKeyPress(string _key)
        {
            CurrentLevel -= 20f;
        }

        public void Dispose()
        {
        }
    }
}