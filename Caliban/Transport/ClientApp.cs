using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Reflection;

namespace Caliban.Core.Transport
{
    public class ClientApp //TODO add heartbeat system to auto-close when caliban closes
    {
        protected void Deconstruct()
        {
            client.Close();
        }

        private ClientTerminal client;
        private readonly string clientName;

        private readonly bool ShouldRegister;

        protected bool IsConnected;
        protected bool IsReady;
        private bool Registered;

        protected void SetClientReady()
        {
            //D.Write("Readying client");
            IsReady = true;
            if (!ShouldRegister || !IsConnected) return;
            if (Registered) return;
            SendMessageToHost(MessageType.REGISTER, clientName);
            Registered = true;
        }

        protected ClientApp(string _clientName, bool _shouldRegister = true)
        {
            clientName = _clientName;
            ShouldRegister = _shouldRegister;
            InitClient();
        }

        private void InitClient()
        {
            client = new ClientTerminal();
            client.Connected += ClientOnConnected;
            client.Disconncted += ClientOnDisconnected;
            client.MessageRecived += (_, _e) => ClientOnMessageReceived(_e);

            try
            {
                client.Connect(5678);
                client.StartListen();
            }
            catch (SocketException)
            {
                Console.WriteLine("Could not connect to server!");
            }
        }

        protected void KillSelf(string _treasureName)
        {
            var pid = Process.GetCurrentProcess().Id;
            var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var exeName = AppDomain.CurrentDomain.FriendlyName;
            if (assemblyPath == null) return;
            var fullPath = Path.Combine(assemblyPath, exeName);
            SendMessageToHost(MessageType.CONSUME_TREASURE, _treasureName + " " + fullPath + " " + pid);
        }

        protected virtual void ClientOnMessageReceived(byte[] _message)
        {
            ////D.Write("Received Message " + Messages.Parse(message));
        }

        protected virtual void ClientOnDisconnected(Socket _socket)
        {
            IsConnected = false;
        }

        public void SendMessageToHost(MessageType _type, string _message)
        {
            client.SendMessage(Messages.Build(clientName, _type, _message));
        }

        protected virtual void ClientOnConnected(Socket _socket)
        {
            IsConnected = true;
            if (ShouldRegister && !Registered)
            {
                SendMessageToHost(MessageType.REGISTER, clientName);
                Registered = true;
            }
        }

        protected void Log(string s)
        {
            if (!IsConnected)
                return;

            SendMessageToHost(MessageType.DEBUG_LOG, $"[{clientName}] {s}");
        }
    }
}