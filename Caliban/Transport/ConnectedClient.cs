using System;
using System.Net.Sockets;

namespace Caliban.Transport
{
    public class ConnectedClient
    {
        private Socket _mClientSocket;
        SocketListener _mListener;

        public event TcpTerminalMessageRecivedDel MessageRecived
        {
            add { _mListener.MessageReceived += value; }
            remove { _mListener.MessageReceived -= value; }
        }

        public event TcpTerminalDisconnectDel Disconnected
        {
            add { _mListener.Disconnected += value; }
            remove { _mListener.Disconnected -= value; }
        }

        public ConnectedClient(Socket clientSocket)
        {
            _mClientSocket = clientSocket;
            _mListener = new SocketListener();
        }

        public void StartListen()
        {
            _mListener.StartReceiving(_mClientSocket);
        }

        public void Send(byte[] buffer)
        {
            if (_mClientSocket == null)
            {
                throw new Exception("Can't send data. ConnectedClient is Closed!");
            }

            byte[] sendData = new byte[buffer.Length + 1];
            sendData[0] = Convert.ToByte(buffer.Length);
            buffer.CopyTo(sendData, 1);
            _mClientSocket.Send(sendData);
        }

        public void Stop()
        {
            _mListener.StopListening();
            _mClientSocket = null;
        }
    }
}