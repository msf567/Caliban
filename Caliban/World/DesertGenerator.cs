using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using Caliban.Core.Utility;
using Newtonsoft.Json;

namespace Caliban.Core.World
{
    public class DesertGenerator
    {
        private readonly DesertNameGenerator nameGenerator = new DesertNameGenerator();
        readonly Random r = new Random(Guid.NewGuid().GetHashCode());

        public DesertNode GenerateDataNodes()
        {
            if (!Directory.Exists(DesertParameters.DesertRoot.FullName))
                Directory.CreateDirectory(DesertParameters.DesertRoot.FullName);

            Process.Start(@"A:\\Desert");

            DesertNode rootNode = new DesertNode(null, @"\\?\" + DesertParameters.DesertRoot.FullName);
            GenerateDesertNodeData(rootNode, DesertParameters.DesertDepth);

            DistributeWater(rootNode);
            
            PrintDebugInfo(rootNode);

            return rootNode;
        }

        private void DistributeWater(DesertNode _rootNode)
        {
          for(int x = 0; x <= DesertParameters.DesertDepth; x ++)
            AddWaterLevelAtDepth(_rootNode,x);
         
        }

        private void AddWaterLevelAtDepth(DesertNode _rootNode,int d)
        {
            var nodes = _rootNode.GetAllNodesAtDepth(d);
            if (nodes == null) return;
         
            D.Write("There are " + nodes?.Count + " nodes at depth " + d);

            if (!DesertParameters.WaterLevels.ContainsKey(d))
                return;
            
            int amt = (int) Math.Floor(nodes.Count * DesertParameters.WaterLevels[d]);
            D.Write("Adding " + amt + " water to depth " + d);
            List<int> waterNodes = new List<int>();
            int number;
            for (int i = 0; i < amt; i++)
            {
                do {
                    number = r.Next(1, nodes.Count);
                } while (waterNodes.Contains(number));
                waterNodes.Add(number);
            }

            for (int x = 0; x < waterNodes.Count; x++)
            {
                nodes[waterNodes[x]].AddTreasure("WaterPuddle.exe");
                D.Write("Adding water to " +  nodes[waterNodes[x]].FullName());   
            }
        }

        private static void PrintDebugInfo(DesertNode _rootNode)
        {
            var json = JsonConvert.SerializeObject(_rootNode, Formatting.Indented,
                new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });
            File.WriteAllText("DesertJSON.json", json);

            D.Write("Desert has " + _rootNode.ChildNodes.Count + " direct children");
            D.Write("Desert has " + _rootNode.GetAllNodes().Count + " total nodes.");
            using (Stream s = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(s, _rootNode);
                D.Write("Desert is " + s.Length + " bytes big");
            }
        }

        private void GenerateDesertNodeData(DesertNode _parent, int _myMaxDepth)
        {
            if (_myMaxDepth == 0)
                return;

            int lowEnd = (_myMaxDepth - 1).Clamp(0, int.MaxValue);
            var newDepth = _myMaxDepth - 1;
            var numberOfChildren = r.Next(1, DesertParameters.DesertWidth);
            for (var i = 0; i < numberOfChildren; i++)
            {
                var newDir = new DesertNode(_parent, nameGenerator.GetNewFolderName());
                _parent.AddChild(newDir);
                 GenerateDesertNodeData(newDir, newDepth);
            }
        }
    }
}