using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using Caliban.Core.Transport;

namespace WaterPuddle
{
    internal class WaterPuddleProgram
    {
        class WaterPuddle : ClientApp
        {
            public WaterPuddle(int amount) : base("WaterPuddle", false)
            {
                int timeout = 10;
                while (!IsConnected && timeout > 0)
                {
                    timeout--;
                    Thread.Sleep(10);
                }

                //string spawnLocation = StreamToString(Assembly.GetExecutingAssembly().GetManifestResourceStream("spawnLocation"));
                //string executedLocation = Assembly.GetExecutingAssembly().Location;
                string myID = AppDomain.CurrentDomain.FriendlyName.Replace(".exe", "").Split('_')[1];

                SendMessageToHost(Messages.Build(MessageType.WATERLEVEL_ADD, amount + " " + myID));
                //SendMessageToHost(Messages.Build(MessageType.DEBUG_LOG,$"Hello!"));
                KillSelf("WaterPuddle.exe");
                Deconstruct();
            }
        }

        public static void Main(string[] args)
        {
            Process[] pname = Process.GetProcessesByName("CALIBAN");
            if (pname.Length == 0)
                return;

            Random r = new Random(Guid.NewGuid().GetHashCode());
            WaterPuddle wp = new WaterPuddle(r.Next(15, 30));
        }

        private static string StreamToString(Stream stream)
        {
            stream.Position = 0;
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }
    }
}