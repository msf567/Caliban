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
                throw new Exception("Can't send data. ConnectedClient is Closed!");
            }

            byte[] sendData = new byte[_buffer.Length + 1];
            sendData[0] = Convert.ToByte(_buffer.Length);
            _buffer.CopyTo(sendData, 1);
            mClientSocket.Send(sendData);
        }

        public void Stop()
        {
            mListener.StopListening();
            mClientSocket = null;
        }
    }
}