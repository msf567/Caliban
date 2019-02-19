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
                Deconstruct();
            }
        }

        public static void Main(string[] args)
        {
            Environment.SetEnvironmentVariable("PATH",
                Environment.GetEnvironmentVariable("PATH") + ";" + @"D:\Caliban\Builds");
            Process[] pname = Process.GetProcessesByName("CALIBAN");
            if (pname.Length == 0)
                return;

            WaterPuddle wp = new WaterPuddle(15);
        }
    }
}