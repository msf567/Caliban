using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using Caliban.Core.Debug;

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
        private const int LengthPrefixSize = 4;
        private AsyncCallback pfnWorkerCallBack;
        private Socket mSocWorker;

        // Accumulates partial/coalesced data so complete frames can be extracted reliably
        // even when TCP splits one frame across segments or coalesces several into one.
        private readonly List<byte> receiveBuffer = new List<byte>();

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
            catch (SocketException)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Environment.Exit(-1);
            }
        }

        private void OnDataReceived(IAsyncResult _asyn)
        {
            CSocketPacket theSockId = (CSocketPacket)_asyn.AsyncState;
            Socket socket = theSockId.ThisSocket;

            if (!socket.Connected)
            {
                return;
            }

            int iRx;
            try
            {
                iRx = socket.EndReceive(_asyn);
            }
            catch (SocketException)
            {
                //Debug.D.Write("Apperently client has been closed and connot answer.");

                OnConnectionDropped(socket);
                return;
            }
            catch (ObjectDisposedException)
            {
                OnConnectionDropped(socket);
                return;
            }

            if (iRx == 0)
            {
                //Debug.D.Write("Apperently client socket has been closed.");
                // If client socket has been closed (but client still answers)- 
                // EndReceive will return 0.
                OnConnectionDropped(socket);
                return;
            }

            try
            {
                // Only consume the bytes actually received (honor iRx) rather than the whole buffer.
                ProcessReceivedBytes(theSockId.DataBuffer, iRx);
            }
            catch (Exception ex)
            {
                // Never let a single bad frame kill the receive loop: log and keep going.
                D.Write("SocketListener receive error: " + ex);
            }
            finally
            {
                // Always re-arm the receive loop so we can't silently go deaf on this connection.
                if (mSocWorker != null)
                {
                    WaitForData(mSocWorker);
                }
            }
        }

        private void ProcessReceivedBytes(byte[] _buffer, int _count)
        {
            for (int i = 0; i < _count; i++)
            {
                receiveBuffer.Add(_buffer[i]);
            }

            // Extract every complete frame currently buffered.
            while (receiveBuffer.Count >= LengthPrefixSize)
            {
                int messageLength = BitConverter.ToInt32(receiveBuffer.GetRange(0, LengthPrefixSize).ToArray(), 0);

                if (messageLength < 0)
                {
                    // Corrupt length prefix; drop everything to resynchronise.
                    receiveBuffer.Clear();
                    break;
                }

                if (receiveBuffer.Count < LengthPrefixSize + messageLength)
                {
                    // The full frame hasn't arrived yet; wait for more data.
                    break;
                }

                byte[] message = receiveBuffer.GetRange(LengthPrefixSize, messageLength).ToArray();
                receiveBuffer.RemoveRange(0, LengthPrefixSize + messageLength);

                RaiseMessageReceived(message);
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