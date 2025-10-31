using System;
using System.Net.Sockets;
using System.Threading;
using Caliban.Core.Transport;
using Caliban.Core.Utility;

namespace DebugLog
{
    internal class DebugLogProgram
    {
        class DebugLog
        {
            private bool closeFlag;
            private UdpClient udpClient;

            public DebugLog()
            {
                udpClient = new UdpClient(7778);
                Thread t = new Thread(ListenThread);
                t.Start();
            }

            private void WriteLine(string s)
            {
                Console.WriteLine(s);
            }

            private void ListenThread()
            {
                while (!closeFlag)
                {
                    try
                    {
                        var endPoint = new System.Net.IPEndPoint(System.Net.IPAddress.Any, 7778);
                        byte[] receivedData = udpClient.Receive(ref endPoint);
                        string message = System.Text.Encoding.ASCII.GetString(receivedData);
                        WriteLine(message);
                    }
                    catch (Exception ex)
                    {
                        WriteLine($"Error: {ex.Message}");
                    }
                }
            }

            public void Stop()
            {
                closeFlag = true;
                udpClient.Close();
            }
        }

        public static void Main(string[] args)
        {
            Console.WriteLine("Debug Log Listener started on port 7778. Press any key to exit.");
            DebugLog d = new DebugLog();
            Console.ReadKey();
            d.Stop();
        }
    }
}