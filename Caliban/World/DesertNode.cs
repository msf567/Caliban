using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Caliban.Core.Treasures;
using Caliban.Core.Utility;

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
            return new List<DesertNode>(_self.GetAllNodes().Where(_n => _n.Depth == _depth));
        }
    }

    [Serializable]
    public class NodeTreasure
    {
        public TreasureType type;
        public string fileName;

        public NodeTreasure(TreasureType _type, string _fileName)
        {
            type = _type;
            fileName = _fileName;
        }
    }

    [Serializable]
    public class DesertNode
    {
        public string Name;
        public string FullName = "";

        public DesertNode ParentNode;

        public int Depth;
        public List<DesertNode> ChildNodes = new List<DesertNode>();
        public List<NodeTreasure> Treasures = new List<NodeTreasure>();

        public DesertNode(DesertNode _parentNode, string _name)
        {
            Name = _name;
            ParentNode = _parentNode;
            Depth = GetDepth();
            FullName = GetFullName();
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

        public List<DesertNode> GetSiblings()
        {
            if (ParentNode == null)
                return new List<DesertNode>();

            return ParentNode.ChildNodes.Where(_i => _i.FullName != FullName).ToList();
        }

        public void AddChild(DesertNode _n)
        {
            if (!ChildNodes.Contains(_n))
                ChildNodes.Add(_n);
        }

        public void AddTreasure(TreasureType _type, string _fileName)
        {
            Treasures.Add(new NodeTreasure(_type, _fileName));
        }

        public void DeleteTreasure(string _treasureFileName)
        {
            var foundItem = Treasures.Find(e => e.fileName == _treasureFileName);
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


        private string GetFullName()
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