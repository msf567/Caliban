using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Caliban.Core.World
{
    public static class TreeNodeEx
    {
        public static List<DesertNode> GetAllNodes(this DesertNode _self)
        {
            List<DesertNode> result = new List<DesertNode>();
            result.Add(_self);
            foreach (DesertNode child in _self.ChildNodes)
            {
                result.AddRange(child.GetAllNodes());
            }
            return result;
        }
        
        public static List<DesertNode> GetAllNodesAtDepth(this DesertNode _self, int _depth)
        {
            return new List<DesertNode>( _self.GetAllNodes().Where(_n => _n.Depth == _depth));
        }
        
    }
    
    [Serializable]
    public class DesertNode
    {
        public string Name;
        public DesertNode ParentNode;
        public int Depth;
        public List<DesertNode> ChildNodes = new List<DesertNode>();
        public List<Tuple<string, string>> Treasures = new List<Tuple<string,string>>();        
        public DesertNode(DesertNode _parentNode, string _name)
        {
            Name = _name;
            ParentNode = _parentNode;
            Depth = GetDepth();
        }
        
        private int GetDepth()
        {
            int depth = 0;
            DesertNode testingNode = this;
            while (testingNode.ParentNode != null)
            {
                testingNode = testingNode.ParentNode;
                depth++;
            }

            return depth;
        }

        public void AddChild(DesertNode _n)
        {
            if (!ChildNodes.Contains(_n)) 
                ChildNodes.Add(_n);

        }

        public void AddTreasure(string _treasureName, string _treasureArgs = "")
        {
            Treasures.Add(new Tuple<string, string>(_treasureName, _treasureArgs));
        }

        public void DeleteTreasure(string _treasureName)
        {
            var foundItem = Treasures.Find(e => e.Item1 == _treasureName);
            if (foundItem != null)
                Treasures.Remove(foundItem);
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