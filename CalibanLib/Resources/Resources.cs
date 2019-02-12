using System.IO;
using System.Reflection;

namespace CalibanLib.Resources
{
    public class Resources
    {
        public static void WriteEmbeddedResource(string assemblyName, string resourceName, string destinationFolder)
        {
            if (!Directory.Exists(destinationFolder))
                Directory.CreateDirectory(destinationFolder);
            var thisAssembly = Assembly.GetExecutingAssembly();
            using (var stream = thisAssembly.GetManifestResourceStream(assemblyName + "." + resourceName))
            {
                using (Stream file = File.Create(Path.Combine(destinationFolder, resourceName)))
                {
                    CopyStream(stream, file);
                }
            }
        }

        private static void CopyStream(Stream input, Stream output)
        {
            var buffer = new byte[8 * 1024];
            int len;
            while ((len = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, len);
            }
        }
    }
}