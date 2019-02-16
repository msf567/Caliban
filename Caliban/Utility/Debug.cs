using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using Caliban.Transport;

namespace Caliban.Utility
{
    public static class D
    {
        private static ServerTerminal s;
        private static Process p;

        public static void Init(ServerTerminal server)
        {
            s = server;
            p = Process.Start("DebugLog.exe");
        }

        public static void Close()
        {
            p?.Kill();
        }

        public static void Log(string m)
        {
            s.SendMessageToClient("DEBUG", Messages.Build(MessageType.DEBUG_LOG, m));
        }
    }
}