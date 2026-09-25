using System;
using System.IO;
using Caliban.Core.Utility;
using Treasures.Resources;
using Caliban.Core.Debug;

namespace Caliban.Core.World
{
    public static class WorldGenerator
    {
        static readonly Random r = new Random(Guid.NewGuid().GetHashCode());

        public static WorldNode GenerateWorld()
        {
            if (!Directory.Exists(WorldParameters.WorldRoot.FullName))
                Directory.CreateDirectory(WorldParameters.WorldRoot.FullName);

            WorldNode worldRoot = ChunkGenerator.GenerateChunk(new WorldNode(null, WorldParameters.WorldRoot.FullName, Biome.DESERT),
                Biome.DESERT);

            var deepestNodes = worldRoot.GetAllNodesAtDepth(WorldParameters.BiomeSizes[Biome.DESERT].Depth);
            int random = r.Next(0, deepestNodes.Count);
            D.Write(deepestNodes[random].FullName);
            ChunkGenerator.GenerateChunk(deepestNodes[random], Biome.DESERT);
            SpawnVictory(worldRoot);

            //WorldNodeTreeRenderer.SavePng(worldRoot, "generatedMap.png", verticalSpacing: 360, padding: 100, horizontalSpacing: 120);
            return worldRoot;
        }

        private static void SpawnVictory(WorldNode _rootNode)
        {
            var deepestNodes = _rootNode.GetAllNodesAtDepth(WorldParameters.BiomeSizes[Biome.DESERT].Depth);
            int random = r.Next(0, deepestNodes.Count);
            deepestNodes[random].AddTreasure(TreasureType.SIMPLE_VICTORY, "SimpleVictory.exe");
            D.Write("Adding victory to " + deepestNodes[random].FullName);
        }
    }
}