using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
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
                int PID = Process.GetCurrentProcess().Id;
                string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string exeName = AppDomain.CurrentDomain.FriendlyName;
                string fullPath = Path.Combine(assemblyPath, exeName);
                SendMessageToHost(Messages.Build(MessageType.KILL_ME, fullPath + " " + PID));
                Deconstruct();
            }
        }

        public static void Main(string[] args)
        {
            Process[] pname = Process.GetProcessesByName("CALIBAN");
            if (pname.Length == 0)
                return;

            WaterPuddle wp = new WaterPuddle(50);
            ProcessStartInfo Info = new ProcessStartInfo();
            Info.Arguments = "Del " + Assembly.GetExecutingAssembly().Location;
            Info.WindowStyle = ProcessWindowStyle.Hidden;
            Info.CreateNoWindow = true;
            Info.FileName = "cmd.exe";
            Process.Start(Info);
        }
    }
}