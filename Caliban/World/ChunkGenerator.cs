using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Caliban.Core.Game;
using Caliban.Core.Utility;
using Newtonsoft.Json;
using Treasures.Resources;
using Caliban.Core.Debug;

namespace Caliban.Core.World
{
    public static class ChunkGenerator
    {
        private static readonly List<string> folderIDs = new List<string>();

        static readonly Random r = new Random(Guid.NewGuid().GetHashCode());

        public static WorldNode GenerateChunk(WorldNode root, Biome type)
        {
            GenerateNodeData(root, WorldParameters.BiomeSizes[type].Depth);
            DistributeWater(root);
            return root;
        }

        private static void DistributeWater(WorldNode _rootNode)
        {
            for (int x = 0; x <= WorldParameters.BiomeSizes[_rootNode.Biome].Depth; x++)
                AddWaterLevelAtDepth(_rootNode, x);
        }

        private static void AddWaterLevelAtDepth(WorldNode _rootNode, int d)
        {
            var nodes = _rootNode.GetAllNodesAtDepth(d);
            if (nodes == null) return;

            if (!WorldParameters.WaterLevels.ContainsKey(d))
                return;

            int amt = (int)Math.Floor(nodes.Count * WorldParameters.WaterLevels[d]);
            //int amt = nodes.Count-1;
            var waterNodes = new List<int>();
            for (int i = 0; i < amt; i++)
            {
                int number;
                do
                {
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

        private static void DistributeFeatures(WorldNode _rootNode)
        {
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
            using (var s = new MemoryStream())
            {
                System.Text.Json.JsonSerializer.Serialize(s, _rootNode);
                D.Write("Desert is " + s.Length + " bytes big");
            }
        }

        private static void GenerateNodeData(WorldNode _parent, int _myMaxDepth)
        {
            if (_myMaxDepth == 0)
                return;

            int lowEnd = (_myMaxDepth - 1).Clamp(0, int.MaxValue);
            var newDepth = _myMaxDepth - 1;
            var numberOfChildren = Math.Abs(r.Next(WorldParameters.BiomeSizes[_parent.Biome].Width - 2, WorldParameters.BiomeSizes[_parent.Biome].Width));
            for (var i = 0; i < numberOfChildren; i++)
            {
                var newNode = new WorldNode(_parent, GetNewFolderName(), _parent.Biome);
                _parent.AddChild(newNode);
                GenerateNodeData(newNode, newDepth);
            }
        }

        private static string GetNewFolderName()
        {
            var baseName = WorldParameters.DesertNames[r.Next(WorldParameters.DesertNames.Length)];
            var newFolderName = baseName + "_" + UIDFactory.GetNewUID(8, folderIDs);

            return newFolderName;
        }
    }
}