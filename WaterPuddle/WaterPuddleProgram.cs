using System;
using System.Diagnostics;
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

                string myID = AppDomain.CurrentDomain.FriendlyName.Replace(".exe","").Split('_')[1];

                SendMessageToHost(Messages.Build(MessageType.WATERLEVEL_ADD, amount + " " + myID));

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
            WaterPuddle wp = new WaterPuddle(r.Next(15,30));
        }
    }
}