using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using Caliban.Core.Game;
using Caliban.Core.Transport;
using Caliban.Core.Windows;
using Treasures;

namespace Caliban.Core.World
{
   
    public class Desert : IDisposable
    {
        private List<Tuple<string, int>> filesToDelete = new List<Tuple<string, int>>();
        private DesertGenerator generator = new DesertGenerator();
        private readonly ExplorerWatcher watcher;
        private readonly List<FileStream> heavyRocks = new List<FileStream>();

        public DesertNode DesertRoot;

        public Desert(ServerTerminal _s)
        {
            _s.MessageReceived += ServerOnMessageReceived;
            DesertRoot = generator.GenerateDataNodes();

            watcher = new ExplorerWatcher();
            watcher.OnNewExplorerFolder += OnNewExplorerFolder;
        }


        private void OnNewExplorerFolder(string _newfolder)
        {
            string folder = Path.GetFileName(_newfolder.TrimEnd(Path.DirectorySeparatorChar));
            Console.WriteLine("New Explorer Folder: " + folder);
        }

        public void Update()
        {
            foreach (var file in filesToDelete)
            {
                DeleteFile(file.Item1, file.Item2);
            }

            filesToDelete.Clear();
        }

        private void ServerOnMessageReceived(Socket _socket, byte[] _message)
        {
            Message m = Messages.Parse(_message);
            Console.WriteLine("Desert Manager received " + m);
            switch (m.Type)
            {
                case MessageType.KILL_ME:
                    var p = m.Value.Split(' ');
                    filesToDelete.Add(new Tuple<string, int>(p[0], int.Parse(p[1])));
                    break;
            }
        }

        public void Clear()
        {
            foreach (var rock in heavyRocks)
            {
                // rock.Close();
                // rock.Unlock(0, 3);
            }

            heavyRocks.Clear();

            if (Directory.Exists(DesertParameters.DesertRoot.FullName))
            {
                var count = DesertParameters.DesertRoot.GetDirectories().Length;
                var fullCount = count;
                var deletedCount = 0;
                foreach (var subdir in DesertParameters.DesertRoot.GetDirectories())
                {
                    ThreadPool.QueueUserWorkItem(delegate
                    {
                        try
                        {
                            Directory.Delete(subdir.FullName, true);
                            deletedCount++;
                            if (deletedCount == fullCount)
                            {
                                Console.WriteLine("Desert Cleared!");
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    });
                }
            }

        }
        
        private void DeleteFile(string _filePath, int _processId)
        {
            Console.WriteLine("Deleting " + _filePath);
            try
            {
                var proc = Process.GetProcessById(_processId);
                proc.Kill();
            }
            catch (ArgumentException e)
            {
                Console.WriteLine(e.Message);
            }

            if (!File.Exists(_filePath)) return;

            try
            {
                File.Delete(_filePath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void DropRock(string _path)
        {
            var f = new FileInfo(Path.Combine(_path, "heavy.rock"));
            var newRock = f.Create();
            newRock.Close();
            // newRock.Lock(0, 3);
            heavyRocks.Add(newRock);

            Random r = new Random(Guid.NewGuid().GetHashCode());
            if (r.NextDouble() < 0.05f)
                DropTreasure(_path);
            else
                TreasureManager.WriteEmbeddedResource("Treasures", "WaterPuddle.exe", _path, "WaterPuddle.exe");
        }
             
        private void DropTreasure(string _path)
        {
            // Console.WriteLine("Dropped Treasure!");
            TreasureManager.WriteEmbeddedResource("Treasures", "SimpleVictory.exe", _path, "SimpleVictory.exe");
        }
  
        public void Dispose()
        {
            Clear();
            watcher.Dispose();
        }
    }
}