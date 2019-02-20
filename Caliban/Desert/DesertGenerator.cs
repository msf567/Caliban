using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using Caliban.Core.Utility;
using Treasures;

namespace Caliban.Core.Desert
{
    public static class DesertGenerator
    {
        private static bool DesertGenerated { get; set; }
        private static readonly List<FileStream> HeavyRocks = new List<FileStream>();
        private static DesertNameGenerator nameGenerator = new DesertNameGenerator();

        public static readonly DirectoryInfo DesertRoot = new DirectoryInfo(@"\\?\A:\\Desert");

        public static void ClearDesert()
        {
            //Console.WriteLine("Clearing Desert!");
            foreach (var rock in HeavyRocks)
            {
                //   rock.Close();
                // rock.Unlock(0, 3);
            }

            HeavyRocks.Clear();

            if (Directory.Exists(DesertRoot.FullName))
            {
                int count = DesertRoot.GetDirectories().Length;
                int fullCount = count;
                int DeletedCount = 0;
                //Console.WriteLine(count + " directories");
                foreach (var subdir in DesertRoot.GetDirectories())
                {
                    ThreadPool.QueueUserWorkItem(delegate
                    {
                        try
                        {
                            Directory.Delete(subdir.FullName, true);
                            DeletedCount++;
                            //Console.WriteLine(DeletedCount);
                            if (DeletedCount == fullCount)
                            {
                                //Console.WriteLine("Desert Cleared!");
                            }
                        }
                        catch (Exception)
                        {
                            //Console.WriteLine("failed " + e.Message);
                        }
                    });
                }
            }

            DesertGenerated = false;
        }

        public static void GenerateDesert()
        {
            if (!Directory.Exists(DesertRoot.FullName))
                Directory.CreateDirectory(DesertRoot.FullName);
            Process.Start(@"A:\\Desert");
            ThreadPool.QueueUserWorkItem(delegate { GenerateDesertNode(DesertRoot, DesertParameters.DesertDepth); });
            DesertGenerated = true;
            foreach (var path in HeavyRocks)
            {
            }
        }

        // TODO: figure out how to actually detect when desert is finished generating
        //private static int RockCount = 0;
        private static void GenerateDesertNode(DirectoryInfo _parent, int _myMaxDepth)
        {
            if (_myMaxDepth == 0) // bottom of the stack
            {
                DropRock(_parent.FullName);
            }
            else
            {
                var r = new Random(Guid.NewGuid().GetHashCode());
                var numberOfChildren = r.Next(1, DesertParameters.DesertWidth);
                int lowEnd = (_myMaxDepth - 3).Clamp(0, int.MaxValue);
                var newDepth = r.Next(lowEnd, _myMaxDepth - 1);

                for (var i = 0; i < numberOfChildren; i++)
                {
                    string newfolderName = nameGenerator.GetNewFolderName(_parent);
                    var newDir = new DirectoryInfo(_parent.FullName).CreateSubdirectory(newfolderName);
                    ThreadPool.QueueUserWorkItem(delegate { GenerateDesertNode(newDir, newDepth); });
                }
            }
        }

        private static void DropTreasure(string _path)
        {
            Console.WriteLine("Dropped Treasure!");
            TreasureManager.WriteEmbeddedResource("Treasures", "SimpleVictory.exe", _path, "SimpleVictory.exe");
        }

        // place a heavy rock at the end of each folder path to prevent modifying the folders mid-game
        private static void DropRock(string _path)
        {
            var f = new FileInfo(Path.Combine(_path, "heavy.rock"));
            var newRock = f.Create();
            newRock.Close();
            // newRock.Lock(0, 3);
            HeavyRocks.Add(newRock);
            Random r = new Random(Guid.NewGuid().GetHashCode());
            if (r.NextDouble() < 0.05f)
                DropTreasure(_path);
            else
                TreasureManager.WriteEmbeddedResource("Treasures", "WaterPuddle.exe", _path, "WaterPuddle.exe");
        }
    }
}