using System;
using System.Net.Sockets;
using System.Text;

namespace CalibanLib.Transport
{
    public class ClientApp
    {
        public void Deconstruct()
        {
           _client.Close();
        }

        private ClientTerminal _client;
        private readonly string _name;
        private readonly byte _registerToken;
        public ClientApp(string name)
        {
            _name = name;
            _registerToken = Convert.ToByte('!');
            InitClient();
        }

        private void InitClient()
        {
            _client = new ClientTerminal();
            _client.Connected += ClientOnConnected;
            _client.Disconncted += ClientOnDisconncted;
            _client.MessageRecived += (s,e) =>ClientOnMessageReceived(e);

            _client.Connect(5678);
            _client.StartListen();
        }

        protected virtual void ClientOnMessageReceived(byte[] message)
        {
            //Console.WriteLine("Received Message " + Messages.Parse(message));
        }

        protected virtual void ClientOnDisconncted(Socket socket)
        {
        }

        protected void SendMessageToHost(string message)
        {
            _client.SendMessage(message);
        }
        
        protected void SendMessageToHost(byte[] message)
        {
            _client.SendMessage(message);
        }

        protected virtual void ClientOnConnected(Socket socket)
        {
            var nameBytes = Encoding.ASCII.GetBytes(_name);
            var registerMessage = new byte[_name.Length+1];
            registerMessage[0] = _registerToken;
            nameBytes.CopyTo(registerMessage,1);
            SendMessageToHost(registerMessage);
        }
    }
}