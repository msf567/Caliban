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
        //i need to keep a dictionary here with a reference to map spawn locations and their clue locations
        //a map will spawn and will send a "get clue location" message and its own exe location to the server which will respond with the clue location from this dictionary.


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
                string trimmedBaseLoc = DesertParameters.DesertRoot.FullName.Replace(@"\\?\", "");
                string trimmedLoc = m.Value.Replace(trimmedBaseLoc, "").TrimEnd(Path.DirectorySeparatorChar);
                if (MapLocations.ContainsKey(trimmedLoc))
                    server.SendMessageToClient(m.Value, Messages.Build(MessageType.MAP_LOCAITON,MapLocations[trimmedLoc]));
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