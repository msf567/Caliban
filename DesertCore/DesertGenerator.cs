using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ZetaLongPaths;

namespace DesertCore
{
    public static class DesertGenerator
    {
        public static int MaxWidth = 6;

        public static  int MaxDepth = 8;
        //private readonly int _amountOfTreasures = 1;
        public static bool DesertGenerated { get; private set; }
        private static readonly List<FileStream> HeavyRocks = new List<FileStream>();

        private static readonly string[] DesertNames = new string[2]
        {
            "sand",
            "dune"
        };

        private static readonly ZlpDirectoryInfo DesertRoot = new ZlpDirectoryInfo(@"D:\\Desert");

        public static void ClearDesert()
        {
            foreach (var rock in HeavyRocks)
            {
                rock.Close();
            }

            if(Directory.Exists(DesertRoot.FullName))
                DesertRoot.DeleteContents(true);

            DesertGenerated = false;
        }

        public static void GenerateDesert()
        {
            ClearDesert();
            GenerateDesertNode(DesertRoot, MaxDepth);
            DesertGenerated = true;
        }

        private static void GenerateDesertNode(ZlpDirectoryInfo parent, int myMaxDepth)
        {
            if (myMaxDepth == 0) // bottom of the stack
            {
                DropRock(parent.FullName);
                return;
            }
            else
            {
                var r = new Random(Guid.NewGuid().GetHashCode());
                var numberOfChildren = r.Next(1, MaxWidth);
                var newDepth = r.Next(0, myMaxDepth - 1);
                for (var i = 0; i < numberOfChildren; i++)
                {
                    var newDir = new ZlpDirectoryInfo(parent.FullName).CreateSubdirectory(GetNewFolderName(parent));

                    GenerateDesertNode(newDir, newDepth);
                }
            }
        }

        private static string GetNewFolderName(ZlpDirectoryInfo parent)
        {
            var r = new Random(Guid.NewGuid().GetHashCode());
            var count = 0;
            var baseName = DesertNames[r.Next(DesertNames.Length)];
            var newFolderName = baseName;
            while (Directory.Exists(Path.Combine(parent.FullName, newFolderName)))
                newFolderName = baseName + count++.ToString();

            return newFolderName;
        }

        private static void DropRock(string path)
        {
            ZlpFileInfo f = new ZlpFileInfo(Path.Combine(path, "heavy.rock"));
            var newRock = f.OpenCreate();
            newRock.Lock(0, 0);
            HeavyRocks.Add(newRock);
        }
    }
}