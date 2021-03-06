﻿using System;
using System.Net.Sockets;
using System.Threading;
using Caliban.Core.Transport;

namespace DebugLog
{
    internal class DebugLogProgram
    {
        class DebugLog : ClientApp
        {
            private bool closeFlag;

            public DebugLog() : base("DEBUG")
            {
                Thread t = new Thread(UpdateThread);
                t.Start();
            }

            private void WriteLine(string s)
            {
                //D.Write(s);
            }

            private void UpdateThread()
            {
                while (!closeFlag)
                {
                    Thread.Sleep(100);
                }
            }

            protected override void ClientOnDisconncted(Socket _socket)
            {
                base.ClientOnDisconncted(_socket);
                Environment.Exit(-1);
            }

            protected override void ClientOnMessageReceived(byte[] message)
            {
                Message m = Messages.Parse(message);
                switch (m.Type)
                {
                    case MessageType.GAME_CLOSE:
                        closeFlag = true;
                        break;
                    case MessageType.DEBUG_LOG:
                        WriteLine(Messages.Parse(message).Value);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public static void Main(string[] args)
        {
            DebugLog d = new DebugLog();
        }
    }
}