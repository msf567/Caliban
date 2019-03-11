using System;
using System.IO;

namespace Scratch
{
    internal class Program
    {
        public const int WINDOW_WIDTH = 50;
        public const int WINDOW_HEIGHT = 40;
        public const string WINDOW_TITLE = "WaterMeter";
        private static Random r;
        private static bool bubbling = false;
        private static int bubbleDrift = 0;
        private static int bubbleHeight = 20;
        private static int bubblePos = 0;
        private static int waterWidth = 7;
        private static int waterHeight = 20;
        private static int ContainerWidth = 9;
        private static int ContainerHeight = 30;
        private static int popCount = 2;
        private static int popPos = 0;

        public static void Main(string[] args)
        {
            Explode();
        }

        static void Explode()
        {
            DirectoryInfo root = new DirectoryInfo(@"//?/A:/Explode");
            if (!Directory.Exists(root.FullName))
                Directory.CreateDirectory(root.FullName);
            for (int x = 0; x < 3_000; x++)
            {
                string newFolder = x.ToString();
                root = Directory.CreateDirectory(Path.Combine(root.FullName, newFolder));
            }
        }
    }
}