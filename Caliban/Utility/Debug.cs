using System;
using System.Diagnostics;
using Caliban.Core.Transport;

namespace Caliban.Core.Utility
{
    public static class D
    {
        private static ServerTerminal server;
        private static Process p;
        public static bool debugMode = false;
        public static void Init(ServerTerminal _server)
        {
            server = _server;
            p = Process.Start("DebugLog.exe");
        }

        public static void Close()
        {
            if (p != null && !p.HasExited)
                p?.Kill();
        }

        public static void Log(string _m)
        {
            if (p != null)
                server.SendMessageToClient("DEBUG", Messages.Build(MessageType.DEBUG_LOG, _m));
            else
            {
                //D.Write(_m);
            }
        }

        public static void Write(string m)
        {
            if(debugMode)
              Console.WriteLine(m);
        }
    }
}