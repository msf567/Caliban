using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Treasures.Resources;

namespace Caliban.Core.World
{
    public static class TreeNodeEx
    {
        public static List<WorldNode> GetAllNodes(this WorldNode _self)
        {
            List<WorldNode> result = new List<WorldNode>();
            result.Add(_self);
            foreach (WorldNode child in _self.ChildNodes)
            {
                result.AddRange(child.GetAllNodes());
            }

            return result;
        }

        public static List<WorldNode> GetAllNodesAtDepth(this WorldNode _self, int _depth)
        {
            return new List<WorldNode>(_self.GetAllNodes().Where(_n => _n.Depth == _depth));
        }
    }


    [Serializable]
    public class WorldNode
    {
        public string Name;
        public string FullName = "";
        public ChunkType Zone;

        public WorldNode ParentNode;

        public int Depth;
        public List<WorldNode> ChildNodes = new List<WorldNode>();
        public List<Treasure> Treasures = new List<Treasure>();
        public List<Feature> Features = new List<Feature>();

        public WorldNode(WorldNode _parentNode, string _name, ChunkType _zone)
        {
            Name = _name;
            ParentNode = _parentNode;
            Zone = _zone;
            Depth = GetDepth();
            FullName = GetFullName();
        }

        private int GetDepth()
        {
            int depth = 0;
            WorldNode testingNode = this;
            while (testingNode.ParentNode != null)
            {
                testingNode = testingNode.ParentNode;
                depth++;
            }

            return depth;
        }

        public List<WorldNode> GetSiblings()
        {
            if (ParentNode == null)
                return new List<WorldNode>();

            return ParentNode.ChildNodes.Where(_i => _i.FullName != FullName).ToList();
        }

        public void AddChild(WorldNode _n)
        {
            if (!ChildNodes.Contains(_n))
                ChildNodes.Add(_n);
        }

        public void AddTreasure(Treasure _t)
        {
            Treasures.Add(_t);
        }

        public void AddTreasure(TreasureType _type, string _fileName, Dictionary<string, string> InternalResources = null)
        {
            Treasure treasure = new Treasure(_type, _fileName);
            if (InternalResources != null)
                foreach (string s in InternalResources.Keys)
                    treasure.AddInternalResource(s, InternalResources[s]);

            Treasures.Add(treasure);
        }

        public Treasure FindFirstTreasureByType(TreasureType _t)
        {
            return Treasures.Find(e => e.type == _t);
        }

        public void DeleteTreasure(string _treasureFileName)
        {
            var foundItem = Treasures.Find(e => e.fileName == _treasureFileName);
            if (foundItem != null)
                Treasures.Remove(foundItem);
        }

        public WorldNode GetNode(string _name)
        {
            WorldNode returnNode = null;
            WorldNode currentNode = this;

            if (currentNode.Name.Contains(_name))
            {
                return currentNode;
            }

            if (currentNode.ChildNodes.Count == 0)
                return null;

            foreach (WorldNode d in currentNode.ChildNodes)
            {
                WorldNode testingNode = d.GetNode(_name);
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
            WorldNode currentNode = this;
            string path = currentNode.Name;
            while (currentNode.ParentNode != null)
            {
                path = Path.Combine(currentNode.ParentNode.Name, path);
                currentNode = currentNode.ParentNode;
            }

            return path;
        }

        public string Print()
        {
            // Use StringBuilder for efficient string building.
            // We fully qualify it here so you don't need to add 'using System.Text;'
            var sb = new System.Text.StringBuilder();

            // Call the recursive helper, starting with indentation level 0
            PrintRecursive(sb, 0);

            return sb.ToString();
        }

        // Private helper function to handle recursion and indentation
        private void PrintRecursive(System.Text.StringBuilder sb, int indentLevel)
        {
            // 1. Create indentation strings
            string indent = new string(' ', indentLevel * 4);
            string propertyIndent = new string(' ', (indentLevel + 1) * 4);

            // 2. Print this node's opening brace
            sb.AppendLine(indent + "{");

            // 3. Print this node's properties
            sb.AppendLine(propertyIndent + $"\"Name\": \"{Name}\",");
            sb.AppendLine(propertyIndent + $"\"FullName\": \"{FullName}\",");
            sb.AppendLine(propertyIndent + $"\"Zone\": \"{Zone}\","); // Relies on Enum.ToString()
            sb.AppendLine(propertyIndent + $"\"Depth\": {Depth},");

            // 4. Print Treasures array
            sb.AppendLine(propertyIndent + "\"Treasures\": [");
            if (Treasures.Any())
            {
                string treasureIndent = new string(' ', (indentLevel + 2) * 4);
                for (int i = 0; i < Treasures.Count; i++)
                {
                    Treasure t = Treasures[i];
                    // Uses the .type and .fileName fields from your other methods
                    sb.Append(treasureIndent + $"{{ \"Type\": \"{t.type}\", \"FileName\": \"{t.fileName}\" }}");

                    // Add a comma if it's not the last one
                    if (i < Treasures.Count - 1)
                    {
                        sb.Append(",");
                    }

                    sb.AppendLine();
                }
            }

            sb.AppendLine(propertyIndent + "],"); // Close Treasures array

            // 5. Print ChildNodes array
            sb.AppendLine(propertyIndent + "\"ChildNodes\": [");
            if (ChildNodes.Any())
            {
                for (int i = 0; i < ChildNodes.Count; i++)
                {
                    WorldNode child = ChildNodes[i];

                    // --- RECURSIVE CALL ---
                    // Tell the child to print itself, at one level deeper
                    child.PrintRecursive(sb, indentLevel + 1);

                    // Add a comma if it's not the last one
                    if (i < ChildNodes.Count - 1)
                    {
                        sb.Append(",");
                    }

                    sb.AppendLine();
                }
            }

            sb.AppendLine(propertyIndent + "]"); // Close ChildNodes array

            // 6. Print this node's closing brace
            sb.Append(indent + "}");
        }
    }
}