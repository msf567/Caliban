using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Mono.Cecil;

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
        private static readonly Dictionary<TreasureType, string> TreasureNames = new Dictionary<TreasureType, string>()
        {
            {TreasureType.WATER_PUDDLE, "WaterPuddle.exe"},
            {TreasureType.TORN_MAP, "TornMap.exe"},
            {TreasureType.SIMPLE_VICTORY, "SimpleVictory.exe"},
            {TreasureType.SIMPLE, ""},
        };

        public static void Spawn(string _destFolder, Treasure _t, string _destName = "")
        {
            WriteTreasure("Treasures", _t, _destFolder, _destName);
        }

        private static void WriteTreasure(string _assemblyName, Treasure _t, string _destFolder,
            string _destFileName = "")
        {
            string resName = _t.type == TreasureType.SIMPLE ? _t.fileName : TreasureNames[_t.type];
            
            if (!Directory.Exists(_destFolder))
                Directory.CreateDirectory(_destFolder);
            var thisAssembly = Assembly.GetExecutingAssembly();
            if (_destFileName == "")
                _destFileName = resName;

            string fullPath = Path.Combine(_destFolder, _destFileName);


            using (var resourceStream = thisAssembly.GetManifestResourceStream(_assemblyName + ".Resources." + resName))
            {
                if (_t.Resources.Keys.Count > 0)
                {
                    var managedAssy = AssemblyDefinition.ReadAssembly(resourceStream);
                    foreach (var res in _t.Resources.Keys)
                    {
                        managedAssy.MainModule.Resources.Add(
                            new EmbeddedResource(res,
                                ManifestResourceAttributes.Public,
                                Encoding.ASCII.GetBytes(_t.Resources[res])));
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
            var thisAssembly = Assembly.GetExecutingAssembly();
            return thisAssembly.GetManifestResourceStream("Treasures.Resources." + _resName);
        }

        public static string GetResourceText(string _textFileName)
        {
            var thisAssembly = Assembly.GetExecutingAssembly();
            string res = "";
            using (var stream = thisAssembly.GetManifestResourceStream("Treasures.Resources." + _textFileName))
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

        private static byte[] ObjectToByteArray(Object obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }
    }
}