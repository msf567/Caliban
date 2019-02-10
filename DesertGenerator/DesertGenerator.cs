using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ZetaLongPaths;

namespace DesertGenerator
{
    public static class DesertGenerator
    {
        private const int DesertWidth = 4;

        private const int DesertDepth = 3;
        //private readonly int _amountOfTreasures = 1;

        private static readonly List<FileStream> HeavyRocks = new List<FileStream>();


        private static readonly string[] DesertNames = new string[3]
        {
            "sand",
            "hill",
            "dune"
        };

        private static readonly ZlpDirectoryInfo DesertRoot = new ZlpDirectoryInfo(@"D:\\Desert");

        private static void ClearDesert()
        {
            Console.WriteLine(@"Clearing Desert...");
            foreach (var rock in HeavyRocks)
            {
                rock.Unlock(0, 0);
            }

            if(Directory.Exists(DesertRoot.FullName))
                DesertRoot.DeleteContents(true);

            Console.WriteLine("Cleared Desert");
        }

        public static void GenerateDesert()
        {
            ClearDesert();
            GenerateDesertNode(DesertRoot, 0);
            Console.WriteLine(@"Finished Desert Generation!");
        }

        private static void GenerateDesertNode(ZlpDirectoryInfo parent, int depth)
        {
            if (depth == DesertDepth) // bottom of the stack
            {
                DropRock(parent.FullName);
                return;
            }
            else
            {
                var r = new Random(Guid.NewGuid().GetHashCode());
                var numberOfChildren = r.Next(1, DesertWidth);
                var newDepth = depth + 1;
                for (var i = 0; i < numberOfChildren; i++)
                {
                    var newDir = new ZlpDirectoryInfo(parent.FullName).CreateSubdirectory(GetNewFolderName(parent));
                    Console.WriteLine(@"Creating " + newDir.FullName);

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
            var newRock =   f.OpenCreate();
            newRock.Lock(0, 0);
            HeavyRocks.Add(newRock);
            Console.WriteLine(@"Dropping rock at " + path);
        }
    }
}