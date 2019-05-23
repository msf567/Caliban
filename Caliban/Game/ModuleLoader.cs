using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using Caliban.Core.Utility;
using Treasures.Resources;

namespace Caliban.Core.Game
{
    public static class ModuleLoader
    {
        private static readonly List<string> LoadingClients = new List<string>();
        private static List<string> SpawnedModules = new List<string>();

        private static void WaitForClientApp(string _clientAppName)
        {
            if (!LoadingClients.Contains(_clientAppName))
                LoadingClients.Add(_clientAppName);
        }

        public static void RunModuleFromMemory(string _processName)
        {
            Stream resourceStream = TreasureManager.GetStream(_processName);
            if (resourceStream == null)
            {
                Console.WriteLine("No exe");
                return;
            }

            //Read the raw bytes of the resource
            byte[] resourcesBuffer = new byte[resourceStream.Length];

            resourceStream.Read(resourcesBuffer, 0, resourcesBuffer.Length);
            resourceStream.Close();

            //Load the bytes as an assembly
            Assembly exeAssembly = Assembly.Load(resourcesBuffer);
            MethodInfo method = exeAssembly.EntryPoint;
            if (method == null) return;
            object[] parameters = method.GetParameters().Length == 0 ? null : new object[] {new string[0]};
            method.Invoke(null, parameters);
        }

        public static void LoadModuleAndWait(string _processName, string _clientAppName, string _args = "")
        {
            try
            {
                var filePath = Path.Combine(AppContext.BaseDirectory, _processName);

                if (File.Exists(filePath))
                    File.Delete(filePath);

                TreasureManager.Spawn(AppContext.BaseDirectory, new Treasure(_processName));
                SpawnedModules.Add(filePath);

                if (!File.Exists(_processName))
                    return;
                Process p = Process.Start(_processName, _args);
                if (p != null)
                {
                    WaitForClientApp(_clientAppName);
                }
            }
            catch (Exception e)
            {
                D.Write(e.Message);
            }
        }

        public static void Clear()
        {
            LoadingClients.Clear();
        }

        public static void Clean()
        {
            // KillModuleProcs();
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                DeleteModuleFiles();
            }).Start();
        }

        private static void DeleteModuleFiles()
        {
            foreach (string s in SpawnedModules)
            {
                while (File.Exists(s))
                    try
                    {
                        File.Delete(s);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
            }

            SpawnedModules.Clear();
        }

        public static void ReadyClient(string _clientAppName)
        {
            if (LoadingClients.Contains(_clientAppName))
            {
                //D.Write(_clientAppName + " is ready!");   
                LoadingClients.Remove(_clientAppName);
            }

            if (LoadingClients.Count == 0 && Game.CurrentGame.State != GameState.NOT_STARTED)
            {
                //D.Write("All clients loaded!");
            }
        }

        public static bool IsReady()
        {
            return LoadingClients.Count == 0;
        }
    }
}