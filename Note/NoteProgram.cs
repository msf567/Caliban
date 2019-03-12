using System.Diagnostics;
using System.Threading;
using Caliban.Core.Utility;
using System.Windows.Forms;
using Caliban.Core.Transport;

namespace Note
{
    internal class NoteProgram
    {
        private class NoteClient : ClientApp
        {
            private NoteForm noteForm;

            public NoteClient(string _clientName) : base(_clientName)
            {
                int timeout = 10;
                while (!IsConnected && timeout > 0)
                {
                    timeout--;
                    Thread.Sleep(10);
                }

                Application.EnableVisualStyles();
                noteForm = new NoteForm(_clientName + ".txt");
               
                Application.Run(noteForm);
            }

            protected override void ClientOnMessageReceived(byte[] _message)
            {
                base.ClientOnMessageReceived(_message);
                if (Messages.Parse(_message).Type == MessageType.GAME_CLOSE)
                {
                    noteForm.Close();   
                }
            }
        }


        public static void Main(string[] args)
        {
            Process[] pname = Process.GetProcessesByName("CALIBAN");
            if (pname.Length == 0)
                return;

            foreach (string s in args)
                D.Write("arg:" + s);
            
            var nc = new NoteClient("intro");
        }
    }
}