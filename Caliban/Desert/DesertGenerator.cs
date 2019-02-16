using System;
using System.Collections.Generic;
using System.IO;
using ZetaLongPaths;

namespace Caliban.Core.Desert
{
    public static class DesertGenerator
    {
        private const int MaxWidth = 4;

        private const int MaxDepth = 3;

        //private readonly int _amountOfTreasures = 1;
        private static bool DesertGenerated { get; set; }
        private static readonly List<FileStream> HeavyRocks = new List<FileStream>();
        private static DesertNameGenerator nameGenerator = new DesertNameGenerator();

        private static readonly ZlpDirectoryInfo DesertRoot = new ZlpDirectoryInfo(@"D:\\Desert");

        public static void ClearDesert()
        {
            foreach (var rock in HeavyRocks)
            {
                rock.Close();
            }

            if (Directory.Exists(DesertRoot.FullName))
                DesertRoot.DeleteContents(true);

            DesertGenerated = false;
        }

        public static void GenerateDesert()
        {
            ClearDesert();
            GenerateDesertNode(DesertRoot, MaxDepth);
            DesertGenerated = true;
        }

        private static void GenerateDesertNode(ZlpDirectoryInfo _parent, int _myMaxDepth)
        {
            if (_myMaxDepth == 0) // bottom of the stack
            {
                DropRock(_parent.FullName);
                return;
            }
            else
            {
                var r = new Random(Guid.NewGuid().GetHashCode());
                var numberOfChildren = r.Next(1, MaxWidth);
                var newDepth = r.Next(0, _myMaxDepth - 1);
                for (var i = 0; i < numberOfChildren; i++)
                {
                    string newfolderName = nameGenerator.GetNewFolderName(_parent);
                    var newDir = new ZlpDirectoryInfo(_parent.FullName).CreateSubdirectory(newfolderName);

                    GenerateDesertNode(newDir, newDepth);
                }
            }
        }

        private static void DropRock(string _path)
        {
            var f = new ZlpFileInfo(Path.Combine(_path, "heavy.rock"));
            var newRock = f.OpenCreate();
            newRock.Lock(0, 0);
            HeavyRocks.Add(newRock);
        }
    }
}