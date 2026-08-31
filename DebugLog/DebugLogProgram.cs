using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Caliban.Core.Debug
{
    internal static class DebugLogProgram
    {
        private class DebugLogListener
        {
            private readonly UdpClient _udpClient;
            private readonly Thread _listenThread;
            private volatile bool _isStopping;

            // Known domains with fixed colors
            private static readonly ConcurrentDictionary<string, ConsoleColor> DomainColors = new(StringComparer.OrdinalIgnoreCase)
            {
                ["CALIBAN"] = ConsoleColor.Cyan,
                ["Graphics"] = ConsoleColor.Magenta,
                ["System"] = ConsoleColor.Yellow,
                ["Network"] = ConsoleColor.Green
            };

            // Fallback palette for dynamically discovered domains
            private static readonly ConsoleColor[] DynamicPalette = new[]
            {
                ConsoleColor.Yellow,
                ConsoleColor.Green,
                ConsoleColor.Blue,
                ConsoleColor.Red,
                ConsoleColor.DarkCyan,
                ConsoleColor.DarkMagenta,
                ConsoleColor.DarkYellow
            };

            private static int _nextColorIndex = 0;

            // Regex pattern to extract [Domain] from formatted logs
            private static readonly Regex DomainRegex = new(@"^\s*\[(?<domain>[^\]]+)\]\s*(?<message>.*)$", RegexOptions.Compiled);

            public DebugLogListener(int port = 7778)
            {
                _udpClient = new UdpClient(port);
                _listenThread = new Thread(ListenLoop)
                {
                    IsBackground = true,
                    Name = "DebugLogListenerThread"
                };
                _listenThread.Start();
            }

            private void ListenLoop()
            {
                var endPoint = new IPEndPoint(IPAddress.Any, 0);

                while (!_isStopping)
                {
                    try
                    {
                        byte[] receivedData = _udpClient.Receive(ref endPoint);
                        string rawMessage = Encoding.ASCII.GetString(receivedData);

                        PrintFormattedLog(rawMessage, endPoint);
                    }
                    catch (SocketException) when (_isStopping)
                    {
                        // Expected exception during UdpClient.Close() on shutdown
                        break;
                    }
                    catch (Exception ex)
                    {
                        if (!_isStopping)
                        {
                            WriteError($"Listener Error: {ex.Message}");
                        }
                    }
                }
            }

            private static void PrintFormattedLog(string rawMessage, IPEndPoint sender)
            {
                string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                Match match = DomainRegex.Match(rawMessage);

                // Lock console printing so multi-line output doesn't interleave across threads
                lock (Console.Out)
                {
                    // Print timestamp prefix
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write($"[{timestamp}] ");

                    if (match.Success)
                    {
                        string domain = match.Groups["domain"].Value;
                        string message = match.Groups["message"].Value;

                        // Get or dynamically allocate a color for new domains
                        ConsoleColor domainColor = DomainColors.GetOrAdd(domain, _ =>
                        {
                            int index = Interlocked.Increment(ref _nextColorIndex) % DynamicPalette.Length;
                            return DynamicPalette[index];
                        });

                        // Print domain header
                        Console.ForegroundColor = domainColor;
                        Console.Write($"[{domain.PadRight(10)}] ");

                        // Print body message
                        Console.ResetColor();
                        Console.WriteLine(message);
                    }
                    else
                    {
                        // Fallback for unformatted log strings
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.WriteLine(rawMessage);
                    }

                    Console.ResetColor();
                }
            }

            private static void WriteError(string errorMessage)
            {
                lock (Console.Out)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] [ERROR] {errorMessage}");
                    Console.ResetColor();
                }
            }

            public void Stop()
            {
                _isStopping = true;
                _udpClient?.Close();

                if (_listenThread != null && _listenThread.IsAlive)
                {
                    _listenThread.Join(1000);
                }
            }
        }

        public static void Main(string[] args)
        {
            Console.Title = "Caliban Debug Log Receiver";
            Console.OutputEncoding = Encoding.UTF8;

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("==================================================");
            Console.WriteLine("  Debug Log Listener Started [UDP Port 7778]     ");
            Console.WriteLine("  Press [ESC] or [CTRL+C] to exit.                ");
            Console.WriteLine("==================================================");
            Console.ResetColor();
            Console.WriteLine();

            var listener = new DebugLogListener(7778);

            // Wait for ESC key or exit trigger
            while (true)
            {
                var key = Console.ReadKey(intercept: true);
                if (key.Key == ConsoleKey.Escape) break;
            }

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("\nShutting down log listener...");
            listener.Stop();
            Console.ResetColor();
        }
    }
}