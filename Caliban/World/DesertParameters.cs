using System.IO;

namespace Caliban.Core.World
{
    public static class DesertParameters
    {
        public static DirectoryInfo DesertRoot = new DirectoryInfo(@"\\?\A:\\Desert");
        public static float WaterPercentage = 0.1f;
        public static int DesertWidth = 3;
        public static int DesertDepth = 3;
    }
}