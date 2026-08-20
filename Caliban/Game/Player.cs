using System;
using Caliban.Core.Transport;

namespace Caliban.Core.Game
{
    public class Player : IDisposable
    {
        public Backpack Backpack { get; set; }
        private WaterManager waterManager;
        private readonly ServerTerminal server;

        public Player(ServerTerminal _server)
        {
            server = _server;
            waterManager = new WaterManager(_server);
        }

        public void Update()
        {
            waterManager.Update();
        }

        public void Dispose()
        {
            waterManager.Dispose();
        }
    }
}