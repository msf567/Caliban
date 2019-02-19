using System;
using System.Net.Sockets;
using System.Text;

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

        protected bool IsConnected = false;
        protected bool IsReady = false;

        protected void SetClientReady()
        {
            //Console.WriteLine("Readying client");
            IsReady = true;
            if (ShouldRegister && IsConnected)
            {
                SendMessageToHost(Messages.Build(MessageType.REGIESTER, clientName));
            }
        }

        public ClientApp(string _clientName, bool _shouldRegister = true)
        {
            this.clientName = _clientName;
            this.ShouldRegister = _shouldRegister;
           InitClient();
        }

        private void InitClient()
        {
            client = new ClientTerminal();
            client.Connected += ClientOnConnected;
            client.Disconncted += ClientOnDisconncted;
            client.MessageRecived += (_s, _e) => ClientOnMessageReceived(_e);

            client.Connect(5678);
            client.StartListen();
        }

        protected virtual void ClientOnMessageReceived(byte[] _message)
        {
            ////Console.WriteLine("Received Message " + Messages.Parse(message));
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
            if (ShouldRegister)
                SendMessageToHost(Messages.Build(MessageType.REGIESTER, clientName));
        }
    }
}