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
            {3, 0.05f},
            {4, 0.1f},
            {5, 0.15f},
            {6, 0.2f}
        };

        public static DirectoryInfo DesertRoot = new DirectoryInfo(@"A:\\Desert");
        public static int DesertWidth = 5;
        public static int DesertDepth = 5;
    }
}