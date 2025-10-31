using System;
using System.Net;
using System.Net.Sockets;
using Caliban.Core.Transport;

namespace Caliban.Core.Utility
{
    public static class D
    {
        private static UdpClient udpClient;
        public static bool debugMode = false;
        static bool inited = false;

        public static void Init()
        {
            udpClient = new UdpClient();
            udpClient.EnableBroadcast = true;
            inited = true;
        }

        public static void Write(string m)
        {
            if (!inited)
                Init();
            if (debugMode)
                Console.WriteLine(m);

            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(m);
            udpClient.Send(bytes, bytes.Length, new IPEndPoint(IPAddress.Loopback, 7778));
        }
    }
}