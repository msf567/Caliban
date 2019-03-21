using System;
using System.Net.Sockets;
using Caliban.Core.Transport;
using UnityEngine;

namespace Caliban.Unity
{
    public class CalibanClient : ClientApp
    {
        public bool Ready = false;
        public CalibanClient() : base("Unity")
        {
        }

        protected override void ClientOnConnected(Socket _socket)
        {
            base.ClientOnConnected(_socket);
            Ready = true;
        }

        protected override void ClientOnMessageReceived(byte[] _message)
        {
            Message m = Messages.Parse(_message);
            switch (m.Type)
            {
                case MessageType.GAME_CLOSE:
                    Application.Quit();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}