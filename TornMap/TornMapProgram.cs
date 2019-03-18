using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Caliban.Core.Transport;
using CLIGL;

namespace TornMap
{
    internal static class TornMapProgram
    {
        public static bool _debug;

        public static void Main(string[] _args)
        {
            if (_args.Contains("debug"))
                _debug = true;


            Map m = new Map(AppDomain.CurrentDomain.BaseDirectory);
        }
    }
}