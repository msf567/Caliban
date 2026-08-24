using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using Caliban.Core.Transport;
using Caliban.Core.Debug;

namespace Note
{
    public class NoteProgram
    {
        public class NoteClient : ClientApp
        {
            private NoteForm noteForm;

            public NoteClient(string _clientName) : base(_clientName)
            {
                int timeout = 10;
                while (!IsConnected && timeout > 0)
                {
                    timeout--;
                    Thread.Sleep(20);
                }

                Application.EnableVisualStyles();
                noteForm = new NoteForm(_clientName + ".txt", this);

                Application.Run(noteForm);
            }

            public new void SendMessageToHost(byte[] message) => base.SendMessageToHost(message);


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

            var nc = new NoteClient("intro");
        }
    }
}