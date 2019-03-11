using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using SHDocVw;

namespace Caliban.Core.Windows
{
    public class ExplorerWatcher : IDisposable
    {
        public delegate void NewExplorerFolder(string _newFolder);

        public NewExplorerFolder OnNewExplorerFolder;
        private bool closeFlag;

        readonly ShellWindows shellWindows = new ShellWindows();
        private readonly List<InternetExplorer> winProcs = new List<InternetExplorer>();

        public ExplorerWatcher()
        {
            var updateThread = new Thread(UpdateLoop);
            updateThread.Start();
        }

        private void UpdateLoop()
        {
            while (!closeFlag)
            {
                var explorers = new List<InternetExplorer>();
                foreach (InternetExplorer ie in shellWindows)
                {
                    explorers.Add(ie);
                    var filename = Path.GetFileNameWithoutExtension(ie.FullName)?.ToLower();

                    if (filename == null || !filename.Equals("explorer")) continue;
                    if (winProcs.Contains(ie)) continue;
                    
                    winProcs.Add(ie);
                    OnNewExplorerFolder?.Invoke(ie.LocationURL);
                    ie.NavigateComplete2 += IeOnNavigateComplete2;
                }

                var listToRemove = winProcs.Except(explorers);
                foreach (var i in listToRemove as InternetExplorer[] ?? listToRemove.ToArray())
                {
                    i.NavigateComplete2 -= IeOnNavigateComplete2;   
                    winProcs.Remove(i);
                }
                
                Thread.Sleep(50);
            }
        }

        private void IeOnNavigateComplete2(object _pdisp, ref object _url)
        {
            OnNewExplorerFolder?.Invoke((string) _url);
        }

        public void Dispose()
        {
            closeFlag = true;
        }
    }
}