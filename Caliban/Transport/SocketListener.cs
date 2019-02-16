using System;
using System.Diagnostics;
using System.Net.Sockets;

namespace Caliban.Transport
{
    public class SocketListener
    {
        public class CSocketPacket
        {
            public Socket ThisSocket;
            public byte[] DataBuffer;

            public CSocketPacket(int bufferLength)
            {
                DataBuffer = new byte[bufferLength];
            }
        }

        private const int BufferLength = 1000;
        private AsyncCallback _pfnWorkerCallBack;
        private Socket _mSocWorker;

        public event TcpTerminalMessageRecivedDel MessageReceived;
        public event TcpTerminalDisconnectDel Disconnected;

        public void StartReceiving(Socket socket)
        {
            _mSocWorker = socket;
            WaitForData(socket);
        }

        public void StopListening()
        {
            // Incase connection has been established with remote client - 
            // Raise the OnDisconnection event.
            if (_mSocWorker != null)
            {
                _mSocWorker.Shutdown(SocketShutdown.Both);
                _mSocWorker.Close();
                _mSocWorker = null;
            }
        }

        private void WaitForData(Socket soc)
        {
            try
            {
                if (_pfnWorkerCallBack == null)
                {
                    _pfnWorkerCallBack = new AsyncCallback(OnDataReceived);
                }

                CSocketPacket theSocPkt = new CSocketPacket(BufferLength);
                theSocPkt.ThisSocket = soc;
                // now start to listen for any data...
                soc.BeginReceive(
                    theSocPkt.DataBuffer,
                    0,
                    theSocPkt.DataBuffer.Length,
                    SocketFlags.None,
                    _pfnWorkerCallBack,
                    theSocPkt);
            }
            catch (SocketException sex)
            {
                Debug.Fail(sex.ToString(), "WaitForData: Socket failed");
            }
        }

        private void OnDataReceived(IAsyncResult asyn)
        {
            CSocketPacket theSockId = (CSocketPacket) asyn.AsyncState;
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
                    iRx = socket.EndReceive(asyn);
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

                WaitForData(_mSocWorker);
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.ToString(), "OnClientConnection: Socket failed");
            }
        }

        private void RaiseMessageReceived(byte[] bytes)
        {
            if (MessageReceived != null)
            {
                MessageReceived(_mSocWorker, bytes);
            }
        }

        private void OnDisconnection(Socket socket)
        {
            if (Disconnected != null)
            {
                Disconnected(socket);
            }
        }

        private void OnConnectionDropped(Socket socket)
        {
            _mSocWorker = null;
            OnDisconnection(socket);
        }
    }
}