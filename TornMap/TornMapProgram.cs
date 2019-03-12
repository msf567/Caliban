using System;
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
            
            private RenderingWindow window;
            private RenderingBuffer buffer;
            public readonly int Width;
            public readonly int Height;
            public const string TITLE = "░or█upt░d █ap";

            public Map(string _clientName) : base(_clientName)
            {
                while (baseClueLoc == "")
                    Thread.Sleep(50);
                Console.Title = "░or█upt░d █ap";
                Console.SetWindowSize(20,20);
                Console.SetBufferSize(30,30);
                Console.WriteLine(GetCorruptedMapString(baseClueLoc));
                while (!closeFlag)
                {
                    Thread.Sleep(50);
                }
            }
            
            protected override void ClientOnConnected(Socket _socket)
            {
                base.ClientOnConnected(_socket);
                Thread.Sleep(100);
                SendMessageToHost(Messages.Build(MessageType.MAP_LOCAITON,AppDomain.CurrentDomain.BaseDirectory));
            }
            
            private string GetCorruptedMapString(string _clueLocation)
            {
                string corruptedString = "";
                foreach (char c in _clueLocation)
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
            
            protected override void ClientOnMessageReceived(byte[] _message)
            {
                base.ClientOnMessageReceived(_message);
                var m = Messages.Parse(_message); 
                if (m.Type == MessageType.GAME_CLOSE)
                    closeFlag = true;
                if (m.Type == MessageType.MAP_LOCAITON)
                    baseClueLoc = m.Value;
            }
        }
        
        public static void Main(string[] _args)
        {
            Map m = new Map(AppDomain.CurrentDomain.BaseDirectory);
        }
    }
}