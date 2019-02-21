using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using Caliban.Core.Utility;
using Treasures;
using Newtonsoft.Json;

namespace Caliban.Core.World
{
    public class DesertGenerator
    {
        private DesertNameGenerator nameGenerator = new DesertNameGenerator();

        public DesertNode GenerateDataNodes()
        {
            if (!Directory.Exists(DesertParameters.DesertRoot.FullName))
                Directory.CreateDirectory(DesertParameters.DesertRoot.FullName);
            
            Process.Start(@"A:\\Desert");
            
            DesertNode rootNode = new DesertNode(null, DesertParameters.DesertRoot.FullName);
            GenerateDesertNodeData(rootNode, DesertParameters.DesertDepth);

            GetDebugInfo(rootNode);

            return rootNode;
        }

        private static void GetDebugInfo(DesertNode _rootNode)
        {
            var json = JsonConvert.SerializeObject(_rootNode, Formatting.Indented,
                new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });
            File.WriteAllText("DesertJSON.json", json);

            Console.WriteLine("Desert has " + _rootNode.ChildNodes.Count + " direct children");
            
            using (Stream s = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(s, _rootNode);
                Console.WriteLine("Desert is " + s.Length + " bytes big");
            }
        }

        private void GenerateDesertNodeData(DesertNode _parent, int _myMaxDepth)
        {
            if (_myMaxDepth == 0)
            {
                //add rock or treasure (spawned at runtime)
                Console.WriteLine("Dropping Rocke");
                _parent.AddTreasure("HeavyRock");
            }
            else
            {
                var r = new Random(Guid.NewGuid().GetHashCode());
                var numberOfChildren = r.Next(1, DesertParameters.DesertWidth);
                int lowEnd = (_myMaxDepth - 1).Clamp(0, int.MaxValue);
                var newDepth = _myMaxDepth - 1;
                for (var i = 0; i < numberOfChildren; i++)
                {
                    string newfolderName = nameGenerator.GetNewFolderName();
                    var newDir = new DesertNode(_parent, newfolderName);
                    _parent.AddChild(newDir);
                    ThreadPool.QueueUserWorkItem(delegate { GenerateDesertNodeData(newDir, newDepth); });
                    //GenerateDesertNodeData(newDir, newDepth);
                }
            }
        }

        //legacy generation
        /*
public void GenerateDirect()
{
    if (!Directory.Exists(DesertParameters.DesertRoot.FullName))
        Directory.CreateDirectory(DesertParameters.DesertRoot.FullName);
    Process.Start(@"A:\\Desert");
    ThreadPool.QueueUserWorkItem(delegate
    {
        GenerateDesertNodeDirect(DesertParameters.DesertRoot, DesertParameters.DesertDepth);
    });

    DesertGenerated = true;
}
private void GenerateDesertNodeDirect(DirectoryInfo _parent, int _myMaxDepth)
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
            string newfolderName = nameGenerator.GetNewFolderName();
            var newDir = new DirectoryInfo(_parent.FullName).CreateSubdirectory(newfolderName);
            ThreadPool.QueueUserWorkItem(delegate { GenerateDesertNodeDirect(newDir, newDepth); });
        }
    }
}
*/

    }
}