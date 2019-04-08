using System;
using System.Collections.Generic;
using System.IO;

namespace Caliban.Core.World
{
    public enum ChunkType
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
            {0, 0.0f},
            {1, 0.0f},
            {2, 0.1f},
            {3, 0.2f},
            {4, 0.3f},
            {5, 0.4f},
            {6, 0.6f}
        };

        public static readonly DirectoryInfo WorldRoot;
        public static int DesertWidth = 5;
        public static int DesertDepth = 5;

        static WorldParameters()
        {
            string path =@"\\?\" +  Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            WorldRoot = new DirectoryInfo(Path.Combine(path, "DESERT"));
        }
    }
}