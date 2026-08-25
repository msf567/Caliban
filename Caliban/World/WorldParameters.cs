using System;
using System.Collections.Generic;
using System.IO;

namespace Caliban.Core.World
{
    public enum Biome
    {
        DESERT,
        CITY,
        CAVE,
        OCEAN,
        OASIS
    }

    public static class WorldParameters
    {
        public static readonly IDictionary<int, float> WaterLevels = new Dictionary<int, float>
        {
            { 0, 0.0f },
            { 1, 0.0f },
            { 2, 0.1f },
            { 3, 0.2f },
            { 4, 0.3f },
            { 5, 0.4f },
            { 6, 0.6f }
        };

        public static readonly DirectoryInfo WorldRoot;

        public class BiomeSize
        {
            public int Width;
            public int Depth;
        }

        public static Dictionary<Biome, BiomeSize> BiomeSizes = new Dictionary<Biome, BiomeSize>
        {
            { Biome.DESERT, new BiomeSize { Width = 5, Depth = 5 } },
            { Biome.CITY, new BiomeSize { Width = 5, Depth = 5 } },
            { Biome.CAVE, new BiomeSize { Width = 5, Depth = 5 } },
            { Biome.OCEAN, new BiomeSize { Width = 5, Depth = 5 } },
            { Biome.OASIS, new BiomeSize { Width = 5, Depth = 5 } }
        };

        public static readonly string[] DesertNames = new string[4]
        {
            "sand",
            "dune",
            "ridge",
            "dust"
        };

        static WorldParameters()
        {
            string path = @"\\?\" + Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            WorldRoot = new DirectoryInfo(Path.Combine(path, "DESERT"));
        }
    }
}