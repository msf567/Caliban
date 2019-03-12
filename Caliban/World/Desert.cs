using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using Caliban.Core.Game;
using Caliban.Core.Transport;
using Caliban.Core.Windows;
using Caliban.Core.Utility;

namespace Caliban.Core.World
{
    public class Desert : IDisposable
    {
        private List<Tuple<string, int>> consumedTreasures = new List<Tuple<string, int>>();
        private DesertGenerator generator = new DesertGenerator();
        private ExplorerWatcher explorerWatcher;
        private FileSystemWatcher fileSystemWatcher;
        public DesertNode DesertRoot;
        private ClueManager clueManager; 
        public Desert(ServerTerminal _s)
        {
            _s.MessageReceived += ServerOnMessageReceived;
            DeleteFolders();
            DesertRoot = generator.GenerateDataNodes();
            string victoryPath = DesertRoot.GetAllNodes().Find(_e => _e.Treasures.Find(e => e.Item1 == "SimpleVictory.exe") != null)
                .FullName();
            
            clueManager = new ClueManager(_s);
            clueManager.AddClue(new SoundClue(victoryPath));
            clueManager.AddClue(new MapClue("corrupted_map",victoryPath,this));
                        
            InitWatchers();
        }

        private void InitWatchers()
        {
            explorerWatcher = new ExplorerWatcher();
            explorerWatcher.OnNewExplorerFolder += OnNewExplorerNav;

            fileSystemWatcher = new FileSystemWatcher(DesertParameters.DesertRoot.FullName.Replace(@"\\?\",""));
            fileSystemWatcher.IncludeSubdirectories = true;
            fileSystemWatcher.NotifyFilter = NotifyFilters.LastAccess
                                             | NotifyFilters.LastWrite
                                             | NotifyFilters.FileName
                                             | NotifyFilters.DirectoryName;
            fileSystemWatcher.EnableRaisingEvents = true;
            fileSystemWatcher.Changed += OnChanged;
            fileSystemWatcher.Created += OnChanged;
            fileSystemWatcher.Deleted += OnChanged;
            fileSystemWatcher.Renamed += OnRenamed;
        }

        private void OnRenamed(object _sender, RenamedEventArgs _e)
        {
            D.Write(_e.OldName + " was renamed!");
        }

        private void OnChanged(object _sender, FileSystemEventArgs _e)
        {
            if (_e.ChangeType == WatcherChangeTypes.Deleted)
            {
                // D.Write(_e.Name + " was deleted. Rebuilding...");
                var folder = Path.GetFileName(_e.Name.TrimEnd(Path.DirectorySeparatorChar));
                var deletedNode = DesertRoot.GetNode(folder);
                RenderNode(deletedNode?.ParentNode);
            }

            if (_e.ChangeType == WatcherChangeTypes.Changed)
            {
                var folder = Path.GetFileName(_e.Name.TrimEnd(Path.DirectorySeparatorChar));

                var changedNode = DesertRoot.GetNode(folder);
                RenderNode(changedNode?.ParentNode);
               //   D.Write(_e.Name + " was changed. Rebuilding...");
            }
        }

        private void OnNewExplorerNav(string _newfolder)
        {
            if (Game.Game.CurrentGame.state != GameState.IN_PROGRESS)
                return;

            var folder = Path.GetFileName(_newfolder.TrimEnd(Path.DirectorySeparatorChar));

            var currentNode = DesertRoot.GetNode(folder);
            if (currentNode == null) return;

            RenderNode(currentNode);
            foreach (var node in currentNode.ChildNodes)
            {
                RenderNode(node);
            }

           ClueManager.FolderNav(folder);
        }

        private void RenderNode(DesertNode _node)
        {
            if (Game.Game.CurrentGame.state != GameState.IN_PROGRESS || _node == null)
                return;
            var dir = new DirectoryInfo(_node.FullName());

            foreach (var f in dir.GetDirectories())
            {
                //dir.Delete(true);
            }

            foreach (var c in _node.ChildNodes)
            {
                var ci = new DirectoryInfo(c.FullName());
                if (!dir.GetDirectories().Contains(ci))
                {
                    Directory.CreateDirectory(ci.FullName);
                }
            }

            foreach (var t in _node.Treasures)
            {
                Treasures.Treasures.Spawn(_node.FullName(), t.Item1);
            }
        }

        public void Update()
        {
            foreach (var treasure in consumedTreasures)
            {
                ConsumeTreasure(treasure.Item1, treasure.Item2);
            }

            consumedTreasures.Clear();
        }

        private void ServerOnMessageReceived(Socket _socket, byte[] _message)
        {
            var m = Messages.Parse(_message);
            D.Write("Desert Manager received " + m);
            switch (m.Type)
            {
                case MessageType.CONSUME_TREASURE:
                    var p = m.Value.Split(' ');
                    consumedTreasures.Add(new Tuple<string, int>(p[0], int.Parse(p[1])));
                    break;
            }
        }

        private void DeleteFolders()
        {
            DesertRoot = null;

            if (Directory.Exists(DesertParameters.DesertRoot.FullName))
            {
                var count = DesertParameters.DesertRoot.GetDirectories().Length;
                var fullCount = count;
                var deletedCount = 0;
                foreach (var subdir in DesertParameters.DesertRoot.GetDirectories())
                {
                    // ThreadPool.QueueUserWorkItem(delegate
                    //{
                    try
                    {
                        if (Directory.Exists(subdir.FullName))
                            DeleteDirectory(subdir.FullName);
                        deletedCount++;
                        if (deletedCount == fullCount)
                        {
                            D.Write("Desert Cleared!");
                        }
                    }
                    catch (Exception e)
                    {
                        D.Write("DELETION ERROR: " + e);
                    }

                    // });
                }
            }
        }

        private static void DeleteDirectory(string target_dir)
        {
            string[] files = Directory.GetFiles(target_dir);
            string[] dirs = Directory.GetDirectories(target_dir);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(target_dir, false);
        }

        private void ConsumeTreasure(string _filePath, int _processId)
        {
            D.Write("Consuming " + _filePath);
            FileInfo fileInfo = new FileInfo(_filePath);
            string treasureName = fileInfo.Name;
            if (fileInfo.Directory != null)
            {
                var nodeName = Path.GetFileName(fileInfo.Directory.Name.TrimEnd(Path.DirectorySeparatorChar));
                DesertRoot.GetNode(nodeName)?.DeleteTreasure(treasureName);
            }

            try
            {
                var proc = Process.GetProcessById(_processId);
                proc.Kill();
            }
            catch (ArgumentException e)
            {
                D.Write(e.Message);
            }

            if (!File.Exists(_filePath)) return;

            try
            {
                File.Delete(_filePath);
            }
            catch (Exception e)
            {
                D.Write(e.Message);
            }
        }

        private void DropTreasure(string _path)
        {
            Treasures.Treasures.Spawn("SimpleVictory.exe", _path, "SimpleVictory.exe");
        }

        public void Dispose()
        {
            ClueManager.Dispose();
            fileSystemWatcher.EnableRaisingEvents = false;
            DeleteFolders();
            explorerWatcher.Dispose();
            fileSystemWatcher.Dispose();
        }
    }
}