using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using Caliban.Core.Transport;
using CLIGL;

namespace TornMap
{
    internal static class TornMapProgram
    {
        class Map : ClientApp
        {
            private bool closeFlag;
            private Random r = new Random(Guid.NewGuid().GetHashCode());
            private string baseClueLoc = "";

            private RenderingBuffer buffer;
            private RenderingWindow window;
            public const int WINDOW_WIDTH = 25;
            public const int WINDOW_HEIGHT = 30;
            
            public const string TITLE = "░or█upt░d █ap";

            public Map(string _clientName) : base(_clientName)
            {
                if (_debug)
                {
                    Console.WriteLine("Debug Mode!");
                    baseClueLoc = @"A:\Caliban\Desert\sand_234f43\dune_234d\ridge_3248jf\dune_234wfe3\sand_32849ur";
                }
                
                while (baseClueLoc == "")
                    Thread.Sleep(50);
                Console.Title = "░or█upt░d █ap";
                Console.SetWindowSize(20, 20);
                Console.SetBufferSize(30, 30);
                Console.WriteLine(GetString(baseClueLoc));
                while (!closeFlag)
                {
                    Thread.Sleep(50);
                }
            }

            protected override void ClientOnConnected(Socket _socket)
            {
                base.ClientOnConnected(_socket);
                Thread.Sleep(100);
                SendMessageToHost(Messages.Build(MessageType.MAP_LOCAITON, AppDomain.CurrentDomain.BaseDirectory));
            }

            protected override void ClientOnMessageReceived(byte[] _message)
            {
                base.ClientOnMessageReceived(_message);
                var m = Messages.Parse(_message);
                if (m.Type == MessageType.GAME_CLOSE)
                    closeFlag = true;
                if (m.Type == MessageType.MAP_LOCAITON)
                    baseClueLoc = m.Value;
            }

            private string GetString(string _baseString)
            {
                string corruptedString = "";
                foreach (char c in _baseString)
                {
                    if (r.NextDouble() < 0.1)
                        corruptedString += Environment.NewLine;

                    if (r.NextDouble() < 0.5f)
                    {
                        corruptedString += "░";
                    }
                    else
                        corruptedString += c;
                }

                return corruptedString;
            }
        }

        private static bool _debug;
        public static void Main(string[] _args)
        {
            if (_args.Contains("debug"))
                _debug = true;
            
            Map m = new Map(AppDomain.CurrentDomain.BaseDirectory);
        }
    }
}