using System;
using System.Collections.Generic;

namespace Treasures.Resources
{
    [Serializable]
    public class Treasure
    {
        public TreasureType type;
        public string fileName;
        public Dictionary<string, string> Resources = new Dictionary<string, string>();

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

        public void AddResource(string resName, string val)
        {
            Resources.Add(resName, val);
        }
    }

}