using System.IO;
using System.Reflection;

namespace Caliban.Core.Resources
{
    public class Resources
    {
        public static void WriteEmbeddedResource(string _assemblyName, string _resourceName, string _destinationFolder)
        {
            if (!Directory.Exists(_destinationFolder))
                Directory.CreateDirectory(_destinationFolder);
            var thisAssembly = Assembly.GetExecutingAssembly();
            using (var stream = thisAssembly.GetManifestResourceStream(_assemblyName + "." + _resourceName))
            {
                using (Stream file = File.Create(Path.Combine(_destinationFolder, _resourceName)))
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