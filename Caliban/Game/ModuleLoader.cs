using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Caliban.Core.Game
{
    public static class ModuleLoader
    {
        private static readonly List<string> LoadingClients = new List<string>();

        public static void WaitForClientApp(string _clientAppName)
        {
            if (!LoadingClients.Contains(_clientAppName))
                LoadingClients.Add(_clientAppName);
        }

        public static void LoadModuleAndWait(string processName, string _clientAppName)
        {
            Process p = Process.Start(processName);
            if (p != null)
                WaitForClientApp(_clientAppName);
        }

        public static void Clear()
        {
            LoadingClients.Clear();
        }

        public static void ReadyClient(string _clientAppName)
        {
            if (LoadingClients.Contains(_clientAppName))
            {
                //Console.WriteLine(_clientAppName + " is ready!");   
                LoadingClients.Remove(_clientAppName);
            }

            if (LoadingClients.Count == 0 && Game.CurrentGame.state != GameState.NOT_STARTED)
            {
                //Console.WriteLine("All clients loaded!");
            }
                
        }

        public static bool IsReady()
        {
            return LoadingClients.Count == 0;
        }
    }
}