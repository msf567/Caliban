using System.IO;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Caliban.Core.Treasures
{
    public static class Treasures
    {
        public static void Spawn(string _destFolder, string _resName, string _destName = "")
        {
            WriteEmbeddedResource("Treasures", _resName, _destFolder, _destName);
        }

        private static void WriteEmbeddedResource(string _assemblyName, string _resourceName, string _destFolder,
            string _destName = "")
        {
            if (!Directory.Exists(_destFolder))
                Directory.CreateDirectory(_destFolder);
            var thisAssembly = Assembly.GetExecutingAssembly();
            if (_destName == "")
                _destName = _resourceName;
            using (var stream = thisAssembly.GetManifestResourceStream(_assemblyName + ".Resources." + _resourceName))
            {
                if (!File.Exists(Path.Combine(_destFolder, _destName)))
                    using (Stream file = File.Create(Path.Combine(_destFolder, _destName)))
                    {
                        CopyStream(stream, file);
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
    }
}