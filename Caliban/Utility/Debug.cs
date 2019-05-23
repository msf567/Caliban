using System;
using Caliban.Core.Transport;

namespace Caliban.Core.Utility
{
    public static class D
    {
        private static ServerTerminal server;
        public static bool debugMode = false;
        public static void Init(ServerTerminal _server)
        {
            server = _server;
        }
        
        public static void Write(string m)
        {
            if(debugMode)
              Console.WriteLine(m);
        }
    }
}