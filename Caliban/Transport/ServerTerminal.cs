using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Caliban.Core.Game;
using Caliban.Core.Utility;

namespace Caliban.Core.Transport
{
    public class ServerTerminal
    {
        public ServerTerminal()
        {
            MClients = new Dictionary<long, ConnectedClient>();
        }

        public event TcpTerminalMessageRecivedDel MessageReceived;
        public event TcpTerminalConnectDel ClientConnect;
        public event TcpTerminalDisconnectDel ClientDisconnect;

        private Socket socket;
        private bool closed;

        private Dictionary<long, ConnectedClient> MClients { get; set; }

        private readonly Dictionary<string, List<ConnectedClient>>
            namedClients = new Dictionary<string, List<ConnectedClient>>();

        public void StartListen(int _port)
        {
            IPEndPoint ipLocal = new IPEndPoint(IPAddress.Loopback, _port);
            socket = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);
            try
            {
                socket.Bind(ipLocal);
            }
            catch (Exception)
            {
                //D.Write(ex.ToString(), string.Format("Can't connect to port {0}!", _port));
                return;
            }

            socket.Listen(4);
            //D.Write("Started Server...");
            socket.BeginAccept(OnClientConnection, null);
        }

        private void OnClientConnection(IAsyncResult _asyn)
        {
            if (closed)
            {
                return;
            }

            try
            {
                Socket clientSocket = socket.EndAccept(_asyn);

                RaiseClientConnected(clientSocket);

                ConnectedClient connectedClient = new ConnectedClient(clientSocket);

                connectedClient.MessageRecived += OnMessageReceived;
                connectedClient.Disconnected += OnClientDisconnection;

                connectedClient.StartListen();

                long key = clientSocket.Handle.ToInt64();
                if (MClients.ContainsKey(key))
                {
                    //D.Write("Client with handle key '{0}' already exist!", key);
                }

                MClients[key] = connectedClient;

                socket.BeginAccept(OnClientConnection, null);
            }
            catch (ObjectDisposedException)
            {
                //D.Write(odex.ToString(), "OnClientConnection: Socket has been closed");
            }
            catch (Exception)
            {
                //D.Write(sex.ToString(),   "OnClientConnection: Socket failed");
            }
        }

        private void OnClientDisconnection(Socket _socket)
        {
            long key = _socket.Handle.ToInt64();
            if (MClients.ContainsKey(key))
            {
                ConnectedClient c = MClients[key];
                foreach (var cList in namedClients.Values)
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
                D.Write("Unknown client " + key + " has been disconncted!");
            }

            RaiseClientDisconnected(_socket);
        }

        public void Clean()
        {
            for (int x = namedClients.Keys.Count - 1; x >= 0; x--)
            {
                string keyToTest = namedClients.Keys.ToList()[x];
                if (namedClients[keyToTest].Count == 0)
                {
                    namedClients.Remove(keyToTest);
                }
            }
        }

        private void RegisterClient(Socket _socket, string _name)
        {
            ConnectedClient c = MClients[_socket.Handle.ToInt64()];
            if (namedClients.ContainsKey(_name))
            {
                D.Write("Registering another " + _name);
                namedClients[_name].Add(c);
            }
            else
            {
                D.Write("Registering " + _name);
                namedClients.Add(_name, new List<ConnectedClient>());
                namedClients[_name].Add(c);
            }

            ModuleLoader.ReadyClient(_name);
        }

        public void SendMessageToSelf(byte[] _message)
        {
            byte[] sendData = new byte[_message.Length + 1];
            sendData[0] = Convert.ToByte(_message.Length);
            _message.CopyTo(sendData, 1);
            OnMessageReceived(socket, sendData);
        }

        public void SendMessageToClient(string _clientName, byte[] _message)
        {
            try
            {
                if (namedClients.ContainsKey(_clientName))
                {
                    foreach (ConnectedClient client in namedClients[_clientName])
                    {
                        client.Send(_message);
                    }
                }
                else
                {
                    ////D.Write(clientName +" not in client dictionary!");
                }
            }
            catch (SocketException)
            {
                //D.Write(se.ToString(), "Buffer could not be sent");
            }
        }

        public void BroadcastMessage(byte[] _message)
        {
            try
            {
                foreach (ConnectedClient connectedClient in MClients.Values)
                {
                    connectedClient.Send(_message);
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
                if (socket != null)
                {
                    closed = true;

                    // Close the clients
                    foreach (ConnectedClient connectedClient in MClients.Values)
                    {
                        connectedClient.Stop();
                    }

                    socket.Close();

                    socket = null;
                }
            }
            catch (ObjectDisposedException)
            {
                D.Write("Stop failed");
            }
        }

        private void OnMessageReceived(Socket _socket, byte[] _message)
        {
            if (MessageReceived != null)
            {
                int msgLen = Convert.ToInt16(_message[0]);
                byte[] trimmedMessage = new byte[msgLen];
                Array.Copy(_message, 1, trimmedMessage, 0, msgLen);

                Message m = Messages.Parse(trimmedMessage);
                if (m.Type == MessageType.REGISTER)
                    RegisterClient(_socket, m.Value);
                else
                {
                    MessageReceived(_socket, trimmedMessage);
                }
            }
        }

        private void RaiseClientConnected(Socket _socket)
        {
            if (ClientConnect != null)
            {
                ClientConnect(_socket);
            }
        }

        private void RaiseClientDisconnected(Socket _socket)
        {
            if (ClientDisconnect != null)
            {
                ClientDisconnect(_socket);
            }
        }
    }
}