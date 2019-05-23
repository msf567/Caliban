using System;
using System.IO;
using Caliban.Core.Utility;
using Treasures.Resources;

namespace Caliban.Core.World
{
    public static class WorldGenerator
    {
        static readonly Random r = new Random(Guid.NewGuid().GetHashCode());
        
        public static WorldNode GenerateWorld()
        {
            if (!Directory.Exists(WorldParameters.WorldRoot.FullName))
                Directory.CreateDirectory(WorldParameters.WorldRoot.FullName);

            WorldNode worldRoot = ChunkGenerator.GenerateChunk(new WorldNode(null, WorldParameters.WorldRoot.FullName, ChunkType.DESERT),
                ChunkType.DESERT);

            var deepestNodes = worldRoot.GetAllNodesAtDepth(WorldParameters.DesertDepth);
            int random = r.Next(0, deepestNodes.Count);
            D.Write(deepestNodes[random].FullName);
            ChunkGenerator.GenerateChunk(deepestNodes[random], ChunkType.DESERT);
            
            SpawnVictory(worldRoot);
            
            return worldRoot;
        }
        
        private static void SpawnVictory(WorldNode _rootNode)
        {
            var deepestNodes = _rootNode.GetAllNodesAtDepth(WorldParameters.DesertDepth);
            int random = r.Next(0, deepestNodes.Count);
            deepestNodes[random].AddTreasure(TreasureType.SIMPLE_VICTORY,"SimpleVictory.exe");
            D.Write("Adding victory to " + deepestNodes[random].FullName);
        }
    }
}