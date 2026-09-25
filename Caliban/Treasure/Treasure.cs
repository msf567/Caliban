using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Treasures.Resources
{
    public class Treasure
    {
        public TreasureType type;
        public string fileName;
        public string spawnLocation;
        public bool removeIfMoved; //TODO do cheat detection in water to detect if player has moved it by registering spawn location
        public Dictionary<string, string> InternalResources = new Dictionary<string, string>();

        public Treasure(TreasureType type, string fileName)
        {
            this.type = type;
            this.fileName = fileName;
        }

        public Treasure(string fileName)
        {
            this.type = TreasureType.SIMPLE;
            this.fileName = fileName;
        }

        public void AddInternalResource(string resName, string val)
        {
            InternalResources.Add(resName, val);
        }
    }
}