using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;

[assembly: InternalsVisibleTo("CALIBAN")]

namespace Caliban.Core.Debug
{
    public static class D
    {
        private static ConcurrentQueue<string> SendQueue = new ConcurrentQueue<string>();
        private static UdpClient udpClient;
        public static bool debugMode = false;
        static bool inited = false;
        private static volatile bool isRunning;
        private static Thread sendThread;

        public static void Init()
        {
            udpClient = new UdpClient();
            udpClient.EnableBroadcast = true;
            isRunning = true;
            sendThread = new Thread(SendThread) { IsBackground = true };
            sendThread.Start();
            inited = true;
        }

        static void SendThread()
        {
            while (isRunning)
            {
                if (SendQueue.TryDequeue(out string message))
                {
                    try
                    {
                        byte[] bytes = System.Text.Encoding.ASCII.GetBytes(message);
                        udpClient.Send(bytes, bytes.Length, new IPEndPoint(IPAddress.Loopback, 7778));
                    }
                    catch (Exception)
                    {
                    }
                }
                else
                {
                    Thread.Sleep(10); // Prevent CPU spinning
                }
            }
        }

        internal static void Write(string m)
        {
            if (!inited)
                Init();
            if (debugMode)
            {
                //Console.WriteLine(m);
            }

            SendQueue.Enqueue(m);
        }

        public static void Dispose()
        {
            isRunning = false;
            if (sendThread != null && sendThread.IsAlive)
            {
                sendThread.Join(1000);
            }

            if (udpClient != null)
            {
                udpClient.Close();
                udpClient = null;
            }

            inited = false;
        }
    }
}