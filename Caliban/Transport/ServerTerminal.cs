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
            //Console.WriteLine("Server Constructor");
            MClients = new Dictionary<long, ConnectedClient>();
        }

        public event TcpTerminalMessageRecivedDel MessageReceived;
        public event TcpTerminalConnectDel ClientConnect;
        public event TcpTerminalDisconnectDel ClientDisconnect;

        private Socket socket;
        private bool closed;

        private Dictionary<long, ConnectedClient> MClients { get; set; }

        private Dictionary<string, List<ConnectedClient>>
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
            catch (Exception ex)
            {
                //Console.WriteLine(ex.ToString(), string.Format("Can't connect to port {0}!", _port));
                return;
            }

            socket.Listen(4);
            //Console.WriteLine("Started Server...");
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
                    //Console.WriteLine("Client with handle key '{0}' already exist!", key);
                }

                MClients[key] = connectedClient;

                socket.BeginAccept(OnClientConnection, null);
            }
            catch (ObjectDisposedException odex)
            {
                //Console.WriteLine(odex.ToString(), "OnClientConnection: Socket has been closed");
            }
            catch (Exception sex)
            {
                //Console.WriteLine(sex.ToString(),   "OnClientConnection: Socket failed");
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
                D.Log("Unknown client " + key + " has been disconncted!");
            }

            RaiseClientDisconnected(_socket);
        }

        private void RegisterClient(Socket _socket, string _name)
        {
            D.Log("Registering " + _name);
            ConnectedClient c = MClients[_socket.Handle.ToInt64()];
            if (namedClients.ContainsKey(_name))
            {
                namedClients[_name].Add(c);
            }
            else
            {
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
                    ////Console.WriteLine(clientName +" not in client dictionary!");
                }
            }
            catch (SocketException se)
            {
                //Console.WriteLine(se.ToString(), "Buffer could not be sent");
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
                D.Log("Stop failed");
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
                if (m.Type == MessageType.REGIESTER)
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