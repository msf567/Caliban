using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Caliban.Core.Game;
using Caliban.Core.Utility;
using Newtonsoft.Json;
using Treasures.Resources;

namespace Caliban.Core.World
{
    public static class ChunkGenerator
    {        
        private static readonly List<string> folderIDs = new List<string>();
        private static readonly string[] DesertNames = new string[4]
        {
            "sand",
            "dune",
            "ridge",
            "dust"
        };
        
        static readonly Random r = new Random(Guid.NewGuid().GetHashCode());
        private static ChunkType CurrentChunkType;
        
        public static WorldNode GenerateChunk(WorldNode root, ChunkType type)
        {
            
            GenerateDesertNodeData(root, WorldParameters.DesertDepth);
            CurrentChunkType = type;
            DistributeWater(root);
            return root;
        }

        private static void DistributeWater(WorldNode _rootNode)
        {
          for(int x = 0; x <= WorldParameters.DesertDepth; x ++)
            AddWaterLevelAtDepth(_rootNode,x);  
        }

        private static void AddWaterLevelAtDepth(WorldNode _rootNode,int d)
        {
            var nodes = _rootNode.GetAllNodesAtDepth(d);
            if (nodes == null) return;

            if (!WorldParameters.WaterLevels.ContainsKey(d))
                return;
            
            int amt = (int) Math.Floor(nodes.Count * WorldParameters.WaterLevels[d]);
            //int amt = nodes.Count-1;
            var waterNodes = new List<int>();
            for (int i = 0; i < amt; i++)
            {
                int number;
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

        private static void PrintDebugInfo(WorldNode _rootNode)
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

        private static void GenerateDesertNodeData(WorldNode _parent, int _myMaxDepth)
        {
            if (_myMaxDepth == 0)
                return;

            int lowEnd = (_myMaxDepth - 1).Clamp(0, int.MaxValue);
            var newDepth = _myMaxDepth - 1;
            var numberOfChildren = Math.Abs( r.Next(WorldParameters.DesertWidth - 2, WorldParameters.DesertWidth));
            for (var i = 0; i < numberOfChildren; i++)
            {
                var newNode = new WorldNode(_parent, GetNewFolderName(), CurrentChunkType);
                _parent.AddChild(newNode);
                 GenerateDesertNodeData(newNode, newDepth);
            }
        }

        private static string GetNewFolderName()
        {
            var baseName = DesertNames[r.Next(DesertNames.Length)];
            var newFolderName = baseName + "_" +  UIDFactory.GetNewUID(8, folderIDs);

            return newFolderName;
        }
    }
}