using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Caliban.Core.Utility;
using SHDocVw;
using Caliban.Core.Debug;

namespace Caliban.Core.Windows
{
    public class ExplorerWatcher : IDisposable
    {
        public delegate void NewExplorerFolder(string _newFolder);

        public NewExplorerFolder OnNewExplorerFolder;

        public delegate void FileAdded(string directory, string filePath);

        public FileAdded OnFileAddedToFolder;

        private bool closeFlag;

        readonly ShellWindows shellWindows = new ShellWindows();
        private readonly List<InternetExplorer> explorerProcs = new List<InternetExplorer>();

        private readonly Dictionary<string, FileSystemWatcher> dirWatchers =
            new Dictionary<string, FileSystemWatcher>(StringComparer.OrdinalIgnoreCase);

        public ExplorerWatcher()
        {
            var updateThread = new Thread(UpdateLoop);
            updateThread.Start();
        }

        private void UpdateLoop()
        {
            while (!closeFlag)
            {
                var currentlyOpenExplorers = new List<InternetExplorer>();

                //register any new explorer windows
                foreach (InternetExplorer ie in shellWindows)
                {
                    currentlyOpenExplorers.Add(ie);
                    var filename = Path.GetFileNameWithoutExtension(ie.FullName)?.ToLower();

                    if (filename == null || !filename.Equals("explorer")) continue;
                    if (explorerProcs.Contains(ie)) continue;

                    explorerProcs.Add(ie);
                    OnNewExplorerFolder?.Invoke(ie.LocationURL);
                    ie.NavigateComplete2 += OnExplorerNavigateComplete;
                }

                var closedExplorers = explorerProcs.Except(currentlyOpenExplorers);
                foreach (var i in closedExplorers as InternetExplorer[] ?? closedExplorers.ToArray())
                {
                    i.NavigateComplete2 -= OnExplorerNavigateComplete;
                    explorerProcs.Remove(i);
                }

                try
                {
                    var currentDirs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    foreach (var ie in currentlyOpenExplorers)
                    {
                        var filename = Path.GetFileNameWithoutExtension(ie.FullName)?.ToLower();
                        if (filename == null || !filename.Equals("explorer")) continue;

                        var localPath = ToLocalPath(ie.LocationURL);
                        if (!string.IsNullOrEmpty(localPath) && Directory.Exists(localPath))
                        {
                            currentDirs.Add(Path.GetFullPath(localPath));
                        }
                    }

                    // Add watchers for newly opened folders
                    foreach (var dir in currentDirs)
                    {
                        if (!dirWatchers.ContainsKey(dir))
                        {
                            dirWatchers[dir] = CreateWatcher(dir);
                        }
                    }

                    // Remove watchers for folders no longer open
                    var toRemove = dirWatchers.Keys.Where(d => !currentDirs.Contains(d)).ToList();
                    foreach (var dir in toRemove)
                    {
                        try
                        {
                            dirWatchers[dir].EnableRaisingEvents = false;
                            dirWatchers[dir].Dispose();
                        }
                        catch
                        {
                            /* swallow */
                        }

                        dirWatchers.Remove(dir);
                    }
                }
                catch
                {
                    // Swallow to keep loop resilient; consider logging in real use
                }

                Thread.Sleep(200);
            }
        }

        private void OnExplorerNavigateComplete(object _prevFolder, ref object _newFolder)
        {
            OnNewExplorerFolder?.Invoke((string)_newFolder);
        }

        private FileSystemWatcher CreateWatcher(string directoryPath)
        {
            var watcher = new FileSystemWatcher(directoryPath)
            {
                IncludeSubdirectories = false, // only the open folder
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime
            };

            FileSystemEventHandler fileCreatedHandler = (s, e) =>
            {
                D.Write("File added: " + e.FullPath);
                // e.FullPath is the created file path
                OnFileAddedToFolder?.Invoke(directoryPath, e.FullPath);
            };
            watcher.Created += fileCreatedHandler;

            RenamedEventHandler renamedHandler = (s, e) =>
            {
                if (string.Equals(Path.GetDirectoryName(e.FullPath),
                        directoryPath,
                        StringComparison.OrdinalIgnoreCase))
                {
                    D.Write("File added: " + e.FullPath);
                    OnFileAddedToFolder?.Invoke(directoryPath, e.FullPath);
                }
            };
            watcher.Renamed += renamedHandler;

            watcher.EnableRaisingEvents = true;
            return watcher;
        }

        private static string ToLocalPath(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return null;
            try
            {
                if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
                {
                    if (uri.IsFile)
                    {
                        return uri.LocalPath;
                    }
                    // For shell: etc. we can't watch; return null
                }
            }
            catch
            {
                /* ignore */
            }

            return null;
        }

        public void Dispose()
        {
            closeFlag = true;
            // ... existing code ...
            foreach (var kv in dirWatchers.ToList())
            {
                try
                {
                    kv.Value.EnableRaisingEvents = false;
                    kv.Value.Dispose();
                }
                catch
                {
                    /* swallow */
                }

                dirWatchers.Remove(kv.Key);
            }
        }
    }
}