using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using Caliban.Core.Transport;
using Caliban.Core.Utility;
using Caliban.Core.World;

namespace Caliban.Core.Game
{
    public class ClueManager
    {
        private static ServerTerminal server;
        private static List<Clue> currentClues = new List<Clue>();
        private static readonly Dictionary<string, string> MapLocations = new Dictionary<string, string>();

        public ClueManager(ServerTerminal _server)
        {
            server = _server;
            server.MessageReceived += ServerOnMessageReceived;
        }

        private static void ServerOnMessageReceived(Socket _socket, byte[] _message)
        {
            var m = Messages.Parse(_message);
            if (m.Type == MessageType.MAP_LOCAITON)
            {
                string trimmedBaseLoc = WorldParameters.WorldRoot.FullName.Replace(@"\\?\", "");
                string trimmedLoc = m.Value.Replace(trimmedBaseLoc, "").TrimEnd(Path.DirectorySeparatorChar);
                if (MapLocations.ContainsKey(trimmedLoc))
                    server.SendMessageToClient(m.Value, Messages.Build(MessageType.MAP_LOCAITON,MapLocations[trimmedLoc]));
            }
            
            if (m.Type == MessageType.MAP_REVEAL)
            {
               D.Write("Recieved a Map Revel!");
            }
        }
        

        public void AddClue(Clue c)
        {
            if (currentClues.Contains(c))
                return;
            currentClues.Add(c);
        }

        public static void AddMapLocation(string _mapLocation, string _clueLocation)
        {
            MapLocations.Add(_mapLocation, _clueLocation);
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
            MapLocations.Clear();
        }
    }
}