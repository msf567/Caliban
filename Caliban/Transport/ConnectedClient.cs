using System;
using System.Net.Sockets;

namespace Caliban.Core.Transport
{
    public class ConnectedClient
    {
        private Socket mClientSocket;
        SocketListener mListener;

        public event TcpTerminalMessageRecivedDel MessageRecived
        {
            add { mListener.MessageReceived += value; }
            remove { mListener.MessageReceived -= value; }
        }

        public event TcpTerminalDisconnectDel Disconnected
        {
            add { mListener.Disconnected += value; }
            remove { mListener.Disconnected -= value; }
        }

        public ConnectedClient(Socket _clientSocket)
        {
            mClientSocket = _clientSocket;
            mListener = new SocketListener();
        }

        public void StartListen()
        {
            mListener.StartReceiving(mClientSocket);
        }

        public void Send(byte[] _buffer)
        {
            if (mClientSocket == null)
            {
                return;
            }

            // Frame the message with a 4-byte length prefix so payloads > 255 bytes are supported.
            byte[] lengthPrefix = BitConverter.GetBytes(_buffer.Length);
            byte[] sendData = new byte[lengthPrefix.Length + _buffer.Length];
            Buffer.BlockCopy(lengthPrefix, 0, sendData, 0, lengthPrefix.Length);
            Buffer.BlockCopy(_buffer, 0, sendData, lengthPrefix.Length, _buffer.Length);
            mClientSocket.Send(sendData);
        }

        public void Stop()
        {
            mListener.StopListening();
            mClientSocket = null;
        }
    }
}