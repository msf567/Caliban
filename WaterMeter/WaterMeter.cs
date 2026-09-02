using System;
using System.Net.Sockets;
using System.Runtime.Versioning;
using System.Threading;
using Caliban.Core.Transport;
using Caliban.Core.Utility;
using SadConsole;
using SadRogue.Primitives;
using Console = SadConsole.Console;
using Message = Caliban.Core.Transport.Message;

[assembly: SupportedOSPlatform("windows")]

namespace WaterMeter
{
    public class WaterMeter : Console
    {
        public const string TITLE = "Water Meter";
        private const string CLIENT_NAME = "WaterMeter";
        private const int PORT = 5678;

        private readonly ClientTerminal client;
        private bool registered;

        private float waterLevel;
        private bool initialized;
        private bool closeFlag;

        public WaterMeter(int _w, int _h) : base(_w, _h)
        {
            client = new ClientTerminal();
            client.Connected += OnConnected;
            client.MessageRecived += (_s, _e) => OnMessageReceived(_e);

            try
            {
                client.Connect(PORT);
                client.StartListen();
            }
            catch (SocketException)
            {
                System.Console.WriteLine("Could not connect to server!");
            }
        }

        public override void Update(TimeSpan delta)
        {
            if (closeFlag)
            {
                Game.Instance.MonoGameInstance.Exit();
                return;
            }

            RenderWaterLevel();
            base.Update(delta);
        }

        private void RenderWaterLevel()
        {
            if (!initialized)
            {
                this.Fill(Color.Black, Color.Black, ' ');
                return;
            }

            int waterHeight = (int)Math.Floor((waterLevel / 100.0f) * Height);
            string waterLevelString = Math.Ceiling(waterLevel).ToString();

            this.Clear();
            this.Fill(Color.Black, Color.Black, ' ');
            this.Fill(new Rectangle(0, Height - waterHeight, Width, waterHeight),
                Color.Blue, Color.DarkBlue, '.');
            this.Print(0, 0, waterLevelString, Color.White, Color.Black);
        }

        #region networking

        private void OnConnected(Socket _socket)
        {
            if (!registered)
            {
                client.SendMessage(Messages.Build("WaterMeter", MessageType.REGISTER, CLIENT_NAME));
                registered = true;
            }

            Thread.Sleep(500);
            client.SendMessage(Messages.Build("WaterMeter", MessageType.WATERLEVEL_GET, ""));
        }

        private void OnMessageReceived(byte[] _message)
        {
            Message m = Messages.Parse(_message);
            switch (m.Type)
            {
                case MessageType.GAME_CLOSE:
                    closeFlag = true;
                    break;
                case MessageType.WATERLEVEL_SET:
                    initialized = true;
                    waterLevel = (float)Math.Floor(float.Parse(m.Value)).Clamp(0, 100);
                    break;
            }
        }

        #endregion
    }
}