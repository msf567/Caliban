using System;
using System.Collections.Generic;
using System.IO;

namespace Caliban.Core.World
{
    [Serializable]
    public class DesertNode
    {
        public string Name;
        public DesertNode ParentNode;
        public List<DesertNode> ChildNodes = new List<DesertNode>();
        public List<string> Treasures = new List<string>();

        public DesertNode(DesertNode _parentNode, string _name)
        {
            Name = _name;
            ParentNode = _parentNode;
        }

        public void AddChild(DesertNode n)
        {
            if (!ChildNodes.Contains(n))
                ChildNodes.Add(n);
        }

        public void AddTreasure(string _treasureName)
        {
            Treasures.Add(_treasureName);
        }

        public string FullName()
        {
            DesertNode currentNode = this;
            string path = currentNode.Name;
            while (currentNode.ParentNode != null)
            {
                path = Path.Combine(currentNode.ParentNode.Name, path);
                currentNode = currentNode.ParentNode;
            }

            return path;
        }
    }
}