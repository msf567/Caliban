using System;
using System.Net;
using System.Net.Sockets;

namespace Caliban.Core.Transport
{
    public class ClientTerminal
    {
        Socket mSocClient;
        private SocketListener mListener;

        public event TcpTerminalMessageRecivedDel MessageRecived;
        public event TcpTerminalConnectDel Connected;
        public event TcpTerminalDisconnectDel Disconncted;

        public void Connect(int _alPort)
        {
            //create a new client socket ...
            mSocClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var remoteEndPoint = new IPEndPoint(IPAddress.Loopback, _alPort);

            // Connect
            mSocClient.Connect(remoteEndPoint);

            OnServerConnection();
        }

        public void SendMessage(string _message)
        {
            if (mSocClient == null)
            {
                return;
            }

            var messageData = System.Text.Encoding.ASCII.GetBytes(_message);
            var sendData = new byte[messageData.Length + 1];
            sendData[0] = Convert.ToByte(messageData.Length);
            messageData.CopyTo(sendData, 1);
            mSocClient.Send(sendData);
        }

        public void SendMessage(byte[] _messageData)
        {
            var sendData = new byte[_messageData.Length + 1];
            sendData[0] = Convert.ToByte(_messageData.Length);
            _messageData.CopyTo(sendData, 1);
            mSocClient.Send(sendData);
        }

        public void StartListen()
        {
            if (mSocClient == null)
            {
                return;
            }

            if (mListener != null)
            {
                return;
            }

            mListener = new SocketListener();
            mListener.Disconnected += OnServerConnectionDroped;
            mListener.MessageReceived += OnMessageReceived;

            mListener.StartReceiving(mSocClient);
        }

        public string ReadData()
        {
            if (mSocClient == null)
            {
                return string.Empty;
            }

            var buffer = new byte[1024];
            var iRx = mSocClient.Receive(buffer);
            var chars = new char[iRx];

            var d = System.Text.Encoding.UTF8.GetDecoder();
            d.GetChars(buffer, 0, iRx, chars, 0);
            var szData = new String(chars);

            return szData;
        }

        public void Close()
        {
            if (mSocClient == null)
                return;

            mListener?.StopListening();

            mSocClient.Close();
            mListener = null;
            mSocClient = null;
        }

        private void OnServerConnection()
        {
            Connected?.Invoke(mSocClient);
        }

        private void OnMessageReceived(Socket _socket, byte[] _message)
        {
            if (MessageRecived == null) return;
            int msgLen = Convert.ToInt16(_message[0]);
            var trimmedMessage = new byte[msgLen];
            Array.Copy(_message, 1, trimmedMessage, 0, msgLen);
            MessageRecived(_socket, trimmedMessage);
        }

        private void OnServerConnectionDroped(Socket _socket)
        {
            Close();
            RaiseServerDisconnected(_socket);
        }

        private void RaiseServerDisconnected(Socket _socket)
        {
            Disconncted?.Invoke(_socket);
        }
    }
}