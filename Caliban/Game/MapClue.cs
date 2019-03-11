using System;
using System.IO;
using Caliban.Core.Utility;
using Caliban.Core.World;

namespace Caliban.Core.Game
{
    public class MapClue : Clue
    {
        private readonly string fileName;
        Random r = new Random(Guid.NewGuid().GetHashCode());
        //i need to keep a dictionary here with a reference to map spawn locations and their clue locations
        //a map will spawn and will send a "get clue location" message and its own exe location to the server which will respond with the clue location from this dictionary.
        public MapClue(string _mapName, string _clueLocation, Desert d, int SpawnDepth = 2) : base(_clueLocation)
        {
            var spawnNode = d.DesertRoot.GetAllNodesAtDepth(SpawnDepth);
            int random = r.Next(0, spawnNode.Count);

            //var mapLocation = d.DesertRoot.FullName();
            var mapLocation = spawnNode[random].FullName();
            spawnNode[random].AddTreasure("TornMap.exe");
            D.Write("Spawning map in " + mapLocation);
            return;
            
            fileName = Path.Combine(mapLocation, _mapName + ".txt");
            string mapContent = GetCorruptedMapString(_clueLocation);
            
            File.WriteAllText(fileName, mapContent);
        }

        private string GetCorruptedMapString(string _clueLocation)
        {
            string corruptedString = "";
            foreach (char c in _clueLocation)
            {
                if (r.NextDouble() < 0.1)
                    corruptedString += Environment.NewLine;

                if (r.NextDouble() < 0.5f)
                {
                    corruptedString += "█";     
                }
                else
                    corruptedString += c;
            }

            return corruptedString;
        }

        public override void Dispose()
        {
            if (File.Exists(fileName))
                File.Delete(fileName);
        }
    }
}