using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using CalibanLib.Utility;

namespace CalibanLib.Transport
{
    public class ServerTerminal
    {
        public ServerTerminal()
        {
            MClients = new Dictionary<long, ConnectedClient>();
        }

        public event TcpTerminalMessageRecivedDel MessageRecived;
        public event TcpTerminalConnectDel ClientConnect;
        public event TcpTerminalDisconnectDel ClientDisconnect;

        private Socket _socket;
        private bool _closed;

        private Dictionary<long, ConnectedClient> MClients { get; set; }

        private Dictionary<string, List<ConnectedClient>>
            _namedClients = new Dictionary<string, List<ConnectedClient>>();

        public void StartListen(int port)
        {
            IPEndPoint ipLocal = new IPEndPoint(IPAddress.Loopback, port);
            _socket = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);
            try
            {
                _socket.Bind(ipLocal);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString(), string.Format("Can't connect to port {0}!", port));
                return;
            }
            _socket.Listen(4);
            _socket.BeginAccept(OnClientConnection, null);
        }

        private void OnClientConnection(IAsyncResult asyn)
        {
            if (_closed)
            {
                return;
            }

            try
            {
                Socket clientSocket = _socket.EndAccept(asyn);

                RaiseClientConnected(clientSocket);

                ConnectedClient connectedClient = new ConnectedClient(clientSocket);

                connectedClient.MessageRecived += OnMessageReceived;
                connectedClient.Disconnected += OnClientDisconnection;

                connectedClient.StartListen();

                long key = clientSocket.Handle.ToInt64();
                if (MClients.ContainsKey(key))
                {
                   Console.WriteLine("Client with handle key '{0}' already exist!", key);
                }

                MClients[key] = connectedClient;

                _socket.BeginAccept(OnClientConnection, null);
            }
            catch (ObjectDisposedException odex)
            {
                Console.WriteLine(odex.ToString(),
                    "OnClientConnection: Socket has been closed");
            }
            catch (Exception sex)
            {
               Console.WriteLine(sex.ToString(),
                    "OnClientConnection: Socket failed");
            }
        }

        private void OnClientDisconnection(Socket socket)
        {
            long key = socket.Handle.ToInt64();
            if (MClients.ContainsKey(key))
            {
                ConnectedClient c = MClients[key];
                foreach (var cList in _namedClients.Values)
                {
                    if (cList.Contains(c))
                    {
                        cList.Remove(c);
                    }
                }
                
                MClients.Remove(key);
            }
            else
            {
                Console.WriteLine("Unknown client '{0}' has been disconncted!", key);
            }
            RaiseClientDisconnected(socket);
        }

        private void RegisterClient(Socket socket, string name)
        {
            Console.WriteLine("Registering " + name);
            ConnectedClient c = MClients[socket.Handle.ToInt64()];
            if (_namedClients.ContainsKey(name))
            {
                _namedClients[name].Add(c);
            }
            else
            {
                _namedClients.Add(name, new List<ConnectedClient>());
                _namedClients[name].Add(c);
            }
        }

        public void SendMessageToClient(string clientName, byte[] message)
        {
            try
            {
                if (_namedClients.ContainsKey(clientName))
                {
                    foreach (ConnectedClient client in _namedClients[clientName])
                    {
                        client.Send(message);
                    }
                }
                else
                {
                    //Console.WriteLine(clientName +" not in client dictionary!");
                }
            }
            catch (SocketException se)
            {
               Console.WriteLine(se.ToString(), "Buffer could not be sent");
            }
        }

        public void BroadcastMessage(byte[] message)
        {
            try
            {
                foreach (ConnectedClient connectedClient in MClients.Values)
                {
                    connectedClient.Send(message);
                }
            }
            catch (SocketException se)
            {
                Debug.Fail(se.ToString(), "Buffer could not be sent");
            }
        }

        public void Close()
        {
            try
            {
                if (_socket != null)
                {
                    _closed = true;

                    // Close the clients
                    foreach (ConnectedClient connectedClient in MClients.Values)
                    {
                        connectedClient.Stop();
                    }

                    _socket.Close();

                    _socket = null;
                }
            }
            catch (ObjectDisposedException odex)
            {
                Console.WriteLine(odex.ToString(), "Stop failed");
            }
        }

        private void OnMessageReceived(Socket socket, byte[] message)
        {
            if (MessageRecived != null)
            {
                int msgLen = Convert.ToInt16(message[0]);
                byte[] trimmedMessage = new byte[msgLen];
                Array.Copy(message, 1, trimmedMessage, 0, msgLen);
                
                if (trimmedMessage[0] == Convert.ToByte('!'))
                    RegisterClient(socket, trimmedMessage.Skip(1).ToArray().String());
                else
                    MessageRecived(socket, trimmedMessage);
            }
        }
        private void RaiseClientConnected(Socket socket)
        {
            if (ClientConnect != null)
            {
                ClientConnect(socket);
            }
        }

        private void RaiseClientDisconnected(Socket socket)
        {
            if (ClientDisconnect != null)
            {
                ClientDisconnect(socket);
            }
        }
    }
}