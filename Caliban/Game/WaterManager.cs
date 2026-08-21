using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using Caliban.Core.Debug;
using Caliban.Core.Transport;
using Caliban.Core.Utility;
using Caliban.Core.Windows;
using Caliban.Core.World;
using Treasures.Resources;

namespace Caliban.Core.Game
{
    public class WaterManager
    {
        private readonly ServerTerminal server;
        private float CurrentLevel { get; set; }
        private static List<string> waterIDs = new List<string>();

        private static Dictionary<string, string> WaterPuddles = new Dictionary<string, string>();

        public WaterManager(ServerTerminal _s)
        {
            server = _s;
            server.MessageReceived += ServerOnMessageReceived;
            //D.Write("Subscribed...");
            CurrentLevel = 80;
            if (!D.debugMode)
            {
                GlobalInput.OnGlobalMouseAction += OnGlobalMouseAction;
                GlobalInput.OnGlobalKeyPress += OnGlobalKeyPress;
            }
            else
            {
                D.Write("Debug: Disabling water consumption");
            }

            ModuleLoader.LoadModuleAndWait(@"Caliban.WaterMeter.exe", "Caliban.WaterMeter");
        }

        private void ServerOnMessageReceived(Socket __socket, byte[] _message)
        {
            var m = Messages.Parse(_message);
            switch (m.Type)
            {
                case MessageType.WATERLEVEL_ADD:
                    string amount = m.Value.Split(' ')[0];
                    string id = m.Value.Split(' ')[1];
                    if (!IsLegalWater(id))
                    {
                        Game.CurrentGame.CheatFlag("you moved the water puddle");
                        break;
                    }

                    CurrentLevel += int.Parse(amount);
                    CurrentLevel = CurrentLevel.Clamp(0, 100);

                    break;
                case MessageType.WATERLEVEL_GET:
                    server.SendMessageToClient("Caliban.WaterMeter",
                        Messages.Build(MessageType.WATERLEVEL_SET, CurrentLevel.ToString()));
                    break;
            }
        }

        private bool IsLegalWater(string _id)
        {
            bool legal = false;
            if (waterIDs.Contains(_id))
            {
                legal = true;
                waterIDs.Remove(_id);
            }

            return legal;
        }

        public static void AddWaterPuddle(WorldNode _node)
        {
            string newID = UIDFactory.GetNewUID(8, waterIDs);
            string waterName = "WaterPuddle_" + newID + ".exe";
            Treasure water = new Treasure(TreasureType.WATER_PUDDLE, waterName);
            water.removeIfMoved = true;
            water.spawnLocation = _node.FullName;
            _node.AddTreasure(water);
        }

        public void Update()
        {
            CurrentLevel.Clamp(0, 100);
            if (CurrentLevel < 0 && Game.CurrentGame.State == GameState.IN_PROGRESS)
                server.SendMessageToSelf(Messages.Build(MessageType.GAME_LOSE, ""));

            server.SendMessageToClient("Caliban.WaterMeter",
                Messages.Build(MessageType.WATERLEVEL_SET, CurrentLevel.ToString()));
            Thread.Sleep(70);
        }

        private void OnGlobalMouseAction(MouseArgs _e)
        {
            if (_e.Message == MouseMessages.WM_LBUTTONDOWN ||
                _e.Message == MouseMessages.WM_RBUTTONDOWN ||
                _e.Message == MouseMessages.WM_XBUTTONDOWN ||
                _e.Message == MouseMessages.WM_WHEELBUTTONDOWN)
                CurrentLevel--;
            else if (_e.Message == MouseMessages.WM_MOUSEMOVE)
                CurrentLevel -= 0.01f;
        }

        private void OnGlobalKeyPress(string _key)
        {
            CurrentLevel -= 20f;
        }

        public void Dispose()
        {
            waterIDs.Clear();
        }
    }
}