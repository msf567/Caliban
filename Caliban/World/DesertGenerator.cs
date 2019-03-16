using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Caliban.Core.Game;
using Caliban.Core.Treasures;
using Caliban.Core.Utility;
using Newtonsoft.Json;

namespace Caliban.Core.World
{
    public class DesertGenerator
    {        
        private List<string> folderIDs = new List<string>();
        private static readonly string[] DesertNames = new string[4]
        {
            "sand",
            "dune",
            "ridge",
            "dust"
        };
        
        readonly Random r = new Random(Guid.NewGuid().GetHashCode());

        public DesertNode GenerateDataNodes()
        {
            if (!Directory.Exists(DesertParameters.DesertRoot.FullName))
                Directory.CreateDirectory(DesertParameters.DesertRoot.FullName);

            DesertNode rootNode = new DesertNode(null,DesertParameters.DesertRoot.FullName);
            GenerateDesertNodeData(rootNode, DesertParameters.DesertDepth);

            DistributeWater(rootNode);
            SpawnVictory(rootNode);
            
         //   PrintDebugInfo(rootNode);

            return rootNode;
        }

        private void SpawnVictory(DesertNode _rootNode)
        {
            var deepestNodes = _rootNode.GetAllNodesAtDepth(DesertParameters.DesertDepth);
            int random = r.Next(0, deepestNodes.Count);
            deepestNodes[random].AddTreasure(TreasureType.SIMPLE_VICTORY,"SimpleVictory.exe");
           D.Write("Adding victory to " + deepestNodes[random].FullName());
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
         

            if (!DesertParameters.WaterLevels.ContainsKey(d))
                return;
            
            int amt = (int) Math.Floor(nodes.Count * DesertParameters.WaterLevels[d]);
            //int amt = nodes.Count-1;
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
                WaterManager.AddWaterPuddle(nodes[waterNodes[x]]);
               // D.Write("Adding water to " +  nodes[waterNodes[x]].FullName());   
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
            var numberOfChildren = Math.Abs( r.Next(DesertParameters.DesertWidth - 2, DesertParameters.DesertWidth));
            for (var i = 0; i < numberOfChildren; i++)
            {
                var newDir = new DesertNode(_parent, GetNewFolderName());
                _parent.AddChild(newDir);
                 GenerateDesertNodeData(newDir, newDepth);
            }
        }

        private string GetNewFolderName()
        {
            var r = new Random(Guid.NewGuid().GetHashCode());
            var baseName = DesertNames[r.Next(DesertNames.Length)];
            var newFolderName = baseName + "_" +  UIDFactory.GetNewUID(8, folderIDs);

            return newFolderName;
        }
    }
}