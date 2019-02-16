using System;
using System.Net;
using System.Net.Sockets;

namespace Caliban.Transport
{
    public class ClientTerminal
    {
        Socket _mSocClient;
        private SocketListener _mListener;

        public event TcpTerminalMessageRecivedDel MessageRecived;
        public event TcpTerminalConnectDel Connected;
        public event TcpTerminalDisconnectDel Disconncted;

        public void Connect(int alPort)
        {
            //create a new client socket ...
            _mSocClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var remoteEndPoint = new IPEndPoint(IPAddress.Loopback, alPort);

            // Connect
            _mSocClient.Connect(remoteEndPoint);

            OnServerConnection();
        }

        public void SendMessage(string message)
        {
            if (_mSocClient == null)
            {
                return;
            }

            var messageData = System.Text.Encoding.ASCII.GetBytes(message);
            var sendData = new byte[messageData.Length + 1];
            sendData[0] = Convert.ToByte(messageData.Length);
            messageData.CopyTo(sendData, 1);
            _mSocClient.Send(sendData);
        }

        public void SendMessage(byte[] messageData)
        {
            var sendData = new byte[messageData.Length + 1];
            sendData[0] = Convert.ToByte(messageData.Length);
            messageData.CopyTo(sendData, 1);
            _mSocClient.Send(sendData);
        }

        public void StartListen()
        {
            if (_mSocClient == null)
            {
                return;
            }

            if (_mListener != null)
            {
                return;
            }

            _mListener = new SocketListener();
            _mListener.Disconnected += OnServerConnectionDroped;
            _mListener.MessageReceived += OnMessageReceived;

            _mListener.StartReceiving(_mSocClient);
        }

        public string ReadData()
        {
            if (_mSocClient == null)
            {
                return string.Empty;
            }

            var buffer = new byte[1024];
            var iRx = _mSocClient.Receive(buffer);
            var chars = new char[iRx];

            var d = System.Text.Encoding.UTF8.GetDecoder();
            d.GetChars(buffer, 0, iRx, chars, 0);
            var szData = new String(chars);

            return szData;
        }

        public void Close()
        {
            if (_mSocClient == null)
                return;

            _mListener?.StopListening();

            _mSocClient.Close();
            _mListener = null;
            _mSocClient = null;
        }

        private void OnServerConnection()
        {
            Connected?.Invoke(_mSocClient);
        }

        private void OnMessageReceived(Socket socket, byte[] message)
        {
            if (MessageRecived == null) return;
            int msgLen = Convert.ToInt16(message[0]);
            var trimmedMessage = new byte[msgLen];
            Array.Copy(message, 1, trimmedMessage, 0, msgLen);
            MessageRecived(socket, trimmedMessage);
        }

        private void OnServerConnectionDroped(Socket socket)
        {
            Close();
            RaiseServerDisconnected(socket);
        }

        private void RaiseServerDisconnected(Socket socket)
        {
            Disconncted?.Invoke(socket);
        }
    }
}