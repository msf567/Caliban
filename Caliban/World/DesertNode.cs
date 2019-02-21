using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

        public void AddChild(DesertNode _n)
        {
            if (!ChildNodes.Contains(_n))// TODO: put back exact match - init fucntion
                ChildNodes.Add(_n);
        }

        public void AddTreasure(string _treasureName)
        {
            Treasures.Add(_treasureName);
        }

        public DesertNode GetNode(string _name)
        {
            DesertNode returnNode = null;
            DesertNode currentNode = this;

            if (currentNode.Name.Contains(_name))
            {
                return currentNode;
            }

            if (currentNode.ChildNodes.Count == 0)
                return null;

            foreach (DesertNode d in currentNode.ChildNodes)
            {
                DesertNode testingNode = d.GetNode(_name);
                if (testingNode != null)
                {
                    returnNode = testingNode;
                    break;
                }
            }

            return returnNode;
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