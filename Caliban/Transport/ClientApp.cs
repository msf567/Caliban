using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Reflection;

namespace Caliban.Core.Transport
{
    public class ClientApp
    {
        public void Deconstruct()
        {
            client.Close();
        }

        private ClientTerminal client;
        protected readonly string clientName;

        protected bool ShouldRegister = true;

        public bool IsConnected = false;
        protected bool IsReady = false;
        public bool Registered = false;

        protected void SetClientReady()
        {
            //D.Write("Readying client");
            IsReady = true;
            if (ShouldRegister && IsConnected)
            {
                if (!Registered)
                {
                    SendMessageToHost(Messages.Build(MessageType.REGISTER, clientName));
                    Registered = true;
                }
            }
        }

        public ClientApp(string _clientName, bool _shouldRegister = true)
        {
            clientName = _clientName;
            ShouldRegister = _shouldRegister;
            InitClient();
        }

        private void InitClient()
        {
            client = new ClientTerminal();
            client.Connected += ClientOnConnected;
            client.Disconncted += ClientOnDisconncted;
            client.MessageRecived += (_s, _e) => ClientOnMessageReceived(_e);

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
            SendMessageToHost(Messages.Build(MessageType.CONSUME_TREASURE, _treasureName + " " + fullPath + " " + pid));
        }

        protected virtual void ClientOnMessageReceived(byte[] _message)
        {
            ////D.Write("Received Message " + Messages.Parse(message));
        }

        protected virtual void ClientOnDisconncted(Socket _socket)
        {
            IsConnected = false;
        }

        protected void SendMessageToHost(string _message)
        {
            client.SendMessage(_message);
        }

        protected void SendMessageToHost(byte[] _message)
        {
            client.SendMessage(_message);
        }

        protected virtual void ClientOnConnected(Socket _socket)
        {
            IsConnected = true;
            if (ShouldRegister && !Registered)
            {
                SendMessageToHost(Messages.Build(MessageType.REGISTER, clientName));
                Registered = true;
            }
        }
    }
}