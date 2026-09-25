using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Mono.Cecil;
using ManifestResourceAttributes = Mono.Cecil.ManifestResourceAttributes;

// ReSharper disable once CheckNamespace
namespace Treasures.Resources
{
    public enum TreasureType
    {
        WATER_PUDDLE,
        TORN_MAP,
        SIMPLE_VICTORY,
        SIMPLE
    }

    public static class TreasureManager
    {
        private const string TreasuresAssemblyName = "Treasures";
        private const string TreasuresFileName = "Treasures.dll";
        private const string ResourcePrefix = "Treasures.Resources.";

        // The Treasures.dll is loaded at runtime and its reference held here, so the treasure
        // logic (this class) can live in Caliban.Core without a compile-time dependency on the
        // Treasures assembly. This breaks the build cycle where Treasures.dll embeds the
        // sub-program executables that themselves depend on Caliban.Core.
        private static Assembly treasuresAssembly;

        private static readonly Dictionary<TreasureType, string> TreasureNames = new Dictionary<TreasureType, string>()
        {
            { TreasureType.WATER_PUDDLE, "WaterPuddle.exe" },
            { TreasureType.TORN_MAP, "TornMap.exe" },
            { TreasureType.SIMPLE_VICTORY, "SimpleVictory.exe" },
            { TreasureType.SIMPLE, "" },
        };

        /// <summary>
        /// Loads Treasures.dll from the application base directory and holds the reference.
        /// Call this once at startup; subsequent treasure lookups reuse the held assembly.
        /// </summary>
        public static Assembly LoadTreasures()
        {
            treasuresAssembly = ResolveTreasuresAssembly();
            return treasuresAssembly;
        }

        private static Assembly TreasuresAssembly
        {
            get
            {
                if (treasuresAssembly == null)
                    treasuresAssembly = ResolveTreasuresAssembly();
                return treasuresAssembly;
            }
        }

        private static Assembly ResolveTreasuresAssembly()
        {
            // Reuse the assembly if it is already loaded into the current AppDomain.
            foreach (var loaded in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (string.Equals(loaded.GetName().Name, TreasuresAssemblyName, StringComparison.OrdinalIgnoreCase))
                    return loaded;
            }

            string path = Path.Combine(AppContext.BaseDirectory, TreasuresFileName);
            if (!File.Exists(path))
                throw new FileNotFoundException("Could not locate " + TreasuresFileName + " next to the application.", path);

            return Assembly.LoadFrom(path);
        }

        public static void Spawn(string _destFolder, Treasure _t, string _destName = "")
        {
            WriteTreasure(_t, _destFolder, _destName);
        }

        private static void WriteTreasure(Treasure _t, string _destFolder, string _destFileName = "")
        {
            string resName = _t.type == TreasureType.SIMPLE ? _t.fileName : TreasureNames[_t.type];

            if (!Directory.Exists(_destFolder))
                Directory.CreateDirectory(_destFolder);
            if (_destFileName == "")
                _destFileName = resName;

            string fullPath = Path.Combine(_destFolder, _destFileName);

            //D.Write("Looking for " + ResourcePrefix + resName);
            using (var resourceStream = TreasuresAssembly.GetManifestResourceStream(ResourcePrefix + resName))
            {
                if (resourceStream == null)
                    return;
                if (_t.InternalResources.Keys.Count > 0)
                {
                    var managedAssy = AssemblyDefinition.ReadAssembly(resourceStream);
                    foreach (var res in _t.InternalResources.Keys)
                    {
                        managedAssy.MainModule.Resources.Add(
                            new EmbeddedResource(res,
                                ManifestResourceAttributes.Public,
                                Encoding.ASCII.GetBytes(_t.InternalResources[res])));
                    }

                    if (!File.Exists(fullPath))
                        managedAssy.Write(fullPath);
                }
                else // not a c# exe
                {
                    if (!File.Exists(fullPath))
                        using (Stream file = File.Create(fullPath))
                            CopyStream(resourceStream, file);
                }
            }
        }

        public static Stream GetStream(string _resName)
        {
            return TreasuresAssembly.GetManifestResourceStream(ResourcePrefix + _resName);
        }

        public static string GetResourceText(string _textFileName)
        {
            string res = "";
            using (var stream = TreasuresAssembly.GetManifestResourceStream(ResourcePrefix + _textFileName))
            {
                if (stream != null)
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        res = reader.ReadToEnd();
                    }
            }

            return res;
        }

        private static void CopyStream(Stream _input, Stream _output)
        {
            var buffer = new byte[8 * 1024];
            int len;
            while ((len = _input.Read(buffer, 0, buffer.Length)) > 0)
            {
                _output.Write(buffer, 0, len);
            }
        }
    }
}