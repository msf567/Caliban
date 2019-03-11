using System.Diagnostics;
using System.Threading;
using Caliban.Core.Transport;

namespace SimpleVictory
{
    internal class SimpleVictoryProgram
    {
        class SimpleVictory : ClientApp
        {
            
            
            public SimpleVictory() : base("SIMPLE_VICTORY", false)
            {
                int timeout = 10;
                while (!IsConnected && timeout > 0)
                {
                    timeout--;
                    Thread.Sleep(10);
                }

                SendMessageToHost(Messages.Build(MessageType.GAME_WIN, ""));

               // KillSelf();
                Deconstruct();
            }
        }

        public static void Main(string[] args)
        {
            Process[] pname = Process.GetProcessesByName("CALIBAN");
            if (pname.Length == 0)
                return;

            SimpleVictory wp = new SimpleVictory();
        }
    }
}