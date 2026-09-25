using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

[assembly: InternalsVisibleTo("CALIBAN")]

namespace Caliban.Core.Debug
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class LogDomainAttribute : Attribute
    {
        public string DomainName { get; }
        public LogDomainAttribute(string domainName) => DomainName = domainName;
    }

    public static class D
    {
        private static readonly ConcurrentDictionary<Assembly, string> DomainCache = new ConcurrentDictionary<Assembly, string>();
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

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Write(string m)
        {
            if (!inited)
                Init();

            Assembly callingAssembly = Assembly.GetCallingAssembly();
            string domain = DomainCache.GetOrAdd(callingAssembly, ResolveDomainName);

            string formattedMessage = $"[{domain}] {m}";

            if (debugMode)
            {
                //Console.WriteLine(formattedMessage);
            }

            SendQueue.Enqueue(formattedMessage);
        }

        public static void Write(object o) => Write(o?.ToString());

        private static string ResolveDomainName(Assembly assembly)
        {
            var attribute = assembly.GetCustomAttribute<LogDomainAttribute>();
            return attribute?.DomainName ?? assembly.GetName().Name ?? "UnknownDomain";
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