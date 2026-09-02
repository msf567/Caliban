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

        public void SendMessage(byte[] _messageData)
        {
            // Frame the message with a 4-byte length prefix so payloads > 255 bytes are supported.
            byte[] lengthPrefix = BitConverter.GetBytes(_messageData.Length);
            var sendData = new byte[lengthPrefix.Length + _messageData.Length];
            Buffer.BlockCopy(lengthPrefix, 0, sendData, 0, lengthPrefix.Length);
            Buffer.BlockCopy(_messageData, 0, sendData, lengthPrefix.Length, _messageData.Length);
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
            // SocketListener already de-framed the message, so _message is a complete frame body.
            MessageRecived?.Invoke(_socket, _message);
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