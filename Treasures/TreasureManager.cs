using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Treasures
{
    public static class TreasureManager
    {
        public static void WriteEmbeddedResource(string _assemblyName, string _resourceName, string _destFolder, string _destName = "")
        {
            if (!Directory.Exists(_destFolder))
                Directory.CreateDirectory(_destFolder);
            var thisAssembly = Assembly.GetExecutingAssembly();
            if (_destName == "")
                _destName = _resourceName;
            using (var stream = thisAssembly.GetManifestResourceStream(_assemblyName + "." + _resourceName))
            {
                using (Stream file = File.Create(Path.Combine(_destFolder, _destName)))
                {
                    CopyStream(stream, file);
                }
            }
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
