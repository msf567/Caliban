using System;
using System.Diagnostics;
using System.Net.Sockets;

namespace Caliban.Core.Transport
{
    public class SocketListener
    {
        public class CSocketPacket
        {
            public Socket ThisSocket;
            public byte[] DataBuffer;

            public CSocketPacket(int _bufferLength)
            {
                DataBuffer = new byte[_bufferLength];
            }
        }

        private const int BufferLength = 1000;
        private AsyncCallback pfnWorkerCallBack;
        private Socket mSocWorker;

        public event TcpTerminalMessageRecivedDel MessageReceived;
        public event TcpTerminalDisconnectDel Disconnected;

        public void StartReceiving(Socket _socket)
        {
            mSocWorker = _socket;
            WaitForData(_socket);
        }

        public void StopListening()
        {
            // Incase connection has been established with remote client - 
            // Raise the OnDisconnection event.
            if (mSocWorker != null)
            {
                mSocWorker?.Shutdown(SocketShutdown.Both);
                mSocWorker?.Close();
                mSocWorker = null;
            }
        }

        private void WaitForData(Socket _soc)
        {
            try
            {
                if (pfnWorkerCallBack == null)
                {
                    pfnWorkerCallBack = new AsyncCallback(OnDataReceived);
                }

                CSocketPacket theSocPkt = new CSocketPacket(BufferLength);
                theSocPkt.ThisSocket = _soc;
                // now start to listen for any data...
                _soc.BeginReceive(
                    theSocPkt.DataBuffer,
                    0,
                    theSocPkt.DataBuffer.Length,
                    SocketFlags.None,
                    pfnWorkerCallBack,
                    theSocPkt);
            }
            catch (SocketException sex)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Environment.Exit(-1);
                Debug.Fail(sex.ToString(), "WaitForData: Socket failed");
            }
        }

        private void OnDataReceived(IAsyncResult _asyn)
        {
            CSocketPacket theSockId = (CSocketPacket) _asyn.AsyncState;
            Socket socket = theSockId.ThisSocket;

            if (!socket.Connected)
            {
                return;
            }

            try
            {
                int iRx;
                try
                {
                    iRx = socket.EndReceive(_asyn);
                }
                catch (SocketException)
                {
                    Debug.Write("Apperently client has been closed and connot answer.");

                    OnConnectionDropped(socket);
                    return;
                }

                if (iRx == 0)
                {
                    Debug.Write("Apperently client socket has been closed.");
                    // If client socket has been closed (but client still answers)- 
                    // EndReceive will return 0.
                    OnConnectionDropped(socket);
                    return;
                }

                byte[] bytes = theSockId.DataBuffer;

                RaiseMessageReceived(bytes);

                WaitForData(mSocWorker);
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.ToString(), "OnClientConnection: Socket failed");
            }
        }

        private void RaiseMessageReceived(byte[] _bytes)
        {
            if (MessageReceived != null)
            {
                MessageReceived(mSocWorker, _bytes);
            }
        }

        private void OnDisconnection(Socket _socket)
        {
            if (Disconnected != null)
            {
                Disconnected(_socket);
            }
        }

        private void OnConnectionDropped(Socket _socket)
        {
            mSocWorker = null;
            OnDisconnection(_socket);
        }
    }
}