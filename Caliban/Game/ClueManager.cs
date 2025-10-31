using System.Collections.Generic;
using System.Net.Sockets;
using Caliban.Core.Transport;

namespace Caliban.Core.Game
{
    public class ClueManager
    {
        private static ServerTerminal server;
        private static List<Clue> currentClues = new List<Clue>();

        public ClueManager(ServerTerminal _server)
        {
            server = _server;
            server.MessageReceived += ServerOnMessageReceived;
        }

        private static void ServerOnMessageReceived(Socket _socket, byte[] _message)
        {
        }
        
        public void AddClue(Clue c)
        {
            if (currentClues.Contains(c))
                return;
            currentClues.Add(c);
        }
        
        public static void FolderNav(string _folder)
        {
            foreach (Clue c in currentClues)
                if (c is SoundClue)
                {
                    SoundClue s = (SoundClue) c;
                    s.FolderNav(_folder);
                }
        }

        public static void Dispose()
        {
            currentClues.Clear();
        }
    }
}