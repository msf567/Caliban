using System;
using System.Threading;
using Caliban.Transport;

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
                Console.WriteLine(s);
            }

            private void UpdateThread()
            {
                while (!closeFlag)
                {
                    Thread.Sleep(100);
                }
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
                        WriteLine(Messages.Parse(message).Param);
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