using System;
using System.Collections.Generic;
using System.IO;

namespace Caliban.Core.World
{
    public static class DesertParameters
    {
        public static readonly IDictionary<int, float> WaterLevels = new Dictionary<int, float>
        {
            {0, 0.0f},
            {1, 0.0f},
            {2, 0.0f},
            {3, 0.1f},
            {4, 0.2f},
            {5, 0.3f},
            {6, 0.5f}
        };

        public static readonly DirectoryInfo DesertRoot;
        public static int DesertWidth = 5;
        public static int DesertDepth = 5;

        static DesertParameters()
        {
            string path =@"\\?\" +  Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            DesertRoot = new DirectoryInfo(Path.Combine(path, "Desert"));
        }
    }
}