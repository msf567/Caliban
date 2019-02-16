using System;
using System.Diagnostics;
using Caliban.Core.Transport;

namespace Caliban.Core.Utility
{
    public static class D
    {
        private static ServerTerminal s;
        private static Process p;

        public static void Init(ServerTerminal _server)
        {
            s = _server;
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
                s.SendMessageToClient("DEBUG", Messages.Build(MessageType.DEBUG_LOG, _m));
            else
            {
                Console.WriteLine(_m);
            }
        }
    }
}