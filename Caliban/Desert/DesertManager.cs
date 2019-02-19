using System;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Net.Sockets;
using Caliban.Core.Transport;

namespace Caliban.Core.Desert
{
    public class DesertManager
    {
        private ServerTerminal server;

        public DesertManager(ServerTerminal s)
        {
            server = s;
            server.MessageReceived += ServerOnMessageReceived;
        }

        private void ServerOnMessageReceived(Socket __socket, byte[] _message)
        {
            Message m = Messages.Parse(_message);
            Console.WriteLine("Desert Manager received " + m);
            switch (m.Type)
            {
                case MessageType.KILL_ME:
                    var p = m.Value.Split(' ');
                    DeleteFile(p[0], int.Parse(p[1]));
                    break;
            }
        }

        private void DeleteFile(string filePath, int ProcessID)
        {
            try
            {
                Process proc = Process.GetProcessById(ProcessID);
                proc?.Kill();
            }
            catch (ArgumentException e)
            {
                Console.WriteLine(e.Message);
            }
            
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Console.WriteLine("Deleted file...?");
            }
        }
        
        
    }
}