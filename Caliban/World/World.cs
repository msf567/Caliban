using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using Caliban.Core.Game;
using Caliban.Core.Transport;
using Caliban.Core.Treasures;
using Caliban.Core.Windows;
using Caliban.Core.Utility;

namespace Caliban.Core.World
{
    public class World : IDisposable
    {
        private List<Tuple<string, string, int>> consumedTreasures = new List<Tuple<string, string, int>>();
        private ExplorerWatcher explorerWatcher;
        private FileSystemWatcher fileSystemWatcher;
        public WorldNode WorldRoot;
        private readonly ClueManager clueManager;

        public World(ServerTerminal _s)
        {
            _s.MessageReceived += ServerOnMessageReceived;
            ClearNode(WorldParameters.WorldRoot.FullName);
            if (Directory.Exists(WorldParameters.WorldRoot.FullName))
                DeleteDirectory(WorldParameters.WorldRoot.FullName);

            WorldRoot = ChunkGenerator.GenerateChunk(ChunkType.DESERT);
            var victoryPath = WorldRoot.GetAllNodes()
                .Find(_e => _e.Treasures.Find(_nodeTreasure => _nodeTreasure.type == TreasureType.SIMPLE_VICTORY) !=
                            null)
                .FullName;

            clueManager = new ClueManager(_s);
            // clueManager.AddClue(new SoundClue(victoryPath));

            clueManager.AddClue(new MapClue(victoryPath, this));

            InitWatchers();
        }

        private void InitWatchers()
        {
            explorerWatcher = new ExplorerWatcher();
            explorerWatcher.OnNewExplorerFolder += OnNewExplorerNav;

            fileSystemWatcher = new FileSystemWatcher(WorldParameters.WorldRoot.FullName.Replace(@"\\?\", ""));
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
            if (_e.ChangeType == WatcherChangeTypes.Deleted || _e.ChangeType == WatcherChangeTypes.Changed)
            {
                var folder = Path.GetFileName(_e.Name.TrimEnd(Path.DirectorySeparatorChar));
                var node = WorldRoot.GetNode(folder);
                RenderNode(node?.ParentNode);
            }
        }

        private void OnNewExplorerNav(string _newfolder)
        {
            if (Game.Game.CurrentGame.State != GameState.IN_PROGRESS)
                return;

            var folder = Path.GetFileName(_newfolder.TrimEnd(Path.DirectorySeparatorChar));

            var currentNode = WorldRoot.GetNode(folder);
            if (currentNode == null)
            {
                return;
            }

            foreach (var sibling in currentNode.GetSiblings())
            {
                //  D.Write("Clearing Sibling " + sibling.FullName);
                //   DeleteDirectory(sibling.FullName);
            }

            RenderNode(currentNode);
            foreach (var node in currentNode.ChildNodes)
            {
                RenderNode(node);
            }

            ClueManager.FolderNav(folder);
        }

        private void RenderNode(WorldNode _node)
        {
            if (Game.Game.CurrentGame.State != GameState.IN_PROGRESS || _node == null)
                return;
            var dir = new DirectoryInfo(_node.FullName);

            foreach (var c in _node.ChildNodes)
            {
                var ci = new DirectoryInfo(c.FullName);
                if (!dir.GetDirectories().Contains(ci))
                {
                    Directory.CreateDirectory(ci.FullName);
                }
            }

            foreach (var t in _node.Treasures)
            {
                TreasureManager.Spawn(_node.FullName, t, t.fileName);
            }
        }

        public void Update()
        {
            foreach (var treasure in consumedTreasures)
            {
                ConsumeTreasure(treasure.Item2, treasure.Item3);
            }

            consumedTreasures.Clear();
        }

        private void ServerOnMessageReceived(Socket _socket, byte[] _message)
        {
            var m = Messages.Parse(_message);
            switch (m.Type)
            {
                case MessageType.CONSUME_TREASURE:
                    var p = m.Value.Split(' ');
                    consumedTreasures.Add(new Tuple<string, string, int>(p[0], p[1], int.Parse(p[2])));
                    break;
            }
        }

        private void ClearNode(string _fullPath)
        {
            if (!Directory.Exists(_fullPath))
                return;

            var dInfo = new DirectoryInfo(_fullPath);
            var folderCount = dInfo.GetDirectories().Length;
            while (folderCount > 0)
            {
                DeleteNode(_fullPath);
                folderCount = dInfo.GetDirectories().Length;
            }
        }

        private void DeleteNode(string _fullPath)
        {
            if (Directory.Exists(_fullPath))
            {
                var dInfo = new DirectoryInfo(_fullPath);
                var count = dInfo.GetDirectories().Length;
                var fullCount = count;
                var deletedCount = 0;

                foreach (var file in dInfo.GetFiles())
                {
                    File.SetAttributes(file.FullName, FileAttributes.Normal);
                    File.Delete(file.FullName);
                }

                foreach (var subdir in dInfo.GetDirectories())
                {
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
                }
            }
        }

        private static void DeleteDirectory(string _targetDir)
        {
            string[] files = Directory.GetFiles(_targetDir);
            string[] dirs = Directory.GetDirectories(_targetDir);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);

                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            try
            {
                Directory.Delete(_targetDir, false);
            }
            catch (IOException)
            {
                // Directory.Delete(target_dir, false);
            }
            catch (UnauthorizedAccessException)
            {
                // Directory.Delete(target_dir, false);
            }
        }

        private void ConsumeTreasure(string _filePath, int _processId)
        {
            FileInfo fileInfo = new FileInfo(_filePath);
            if (fileInfo.Directory != null)
            {
                var nodeName = Path.GetFileName(fileInfo.Directory.Name.TrimEnd(Path.DirectorySeparatorChar));
                string fileName = Path.GetFileName(_filePath);
                WorldRoot.GetNode(nodeName)?.DeleteTreasure(fileName);
            }

            try
            {
                var proc = Process.GetProcessById(_processId);
                proc.Kill();
            }
            catch (ArgumentException)
            {
                //D.Write(e.Message);
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

        public void Dispose()
        {
            fileSystemWatcher.EnableRaisingEvents = false;
            ClueManager.Dispose();
            explorerWatcher.Dispose();
            fileSystemWatcher.Dispose();
            ClearNode(WorldParameters.WorldRoot.FullName);
            WorldRoot = null;
        }
    }
}