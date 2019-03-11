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
            private int amount = 0;

            public WaterPuddle(int amount) : base("WaterPuddle", false)
            {
                this.amount = amount;
                int timeout = 10;
                while (!IsConnected && timeout > 0)
                {
                    timeout--;
                    Thread.Sleep(10);
                }

                SendMessageToHost(Messages.Build(MessageType.WATERLEVEL_ADD, amount.ToString()));

                KillSelf();
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