using System;
using System.Collections.Generic;

namespace Treasures.Resources
{
    [Serializable]
    public class Treasure
    {
        public TreasureType type;
        public string fileName;
        public string spawnLocation;
        public bool removeIfMoved; //TODO do cheat detection in water to detect if player has moved it by registering spawn location
        public Dictionary<string, string> InternalResources = new Dictionary<string, string>();

        //TODO make treasure factory to simplify treasure generation
        public Treasure(TreasureType _type, string _fileName)
        {
            type = _type;
            fileName = _fileName;
        }

        public Treasure(string _fileName)
        {
            type = TreasureType.SIMPLE;
            fileName = _fileName;
        }

        public void AddInternalResource(string resName, string val)
        {
            InternalResources.Add(resName, val);
        }
    }
}