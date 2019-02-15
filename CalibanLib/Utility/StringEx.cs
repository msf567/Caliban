using System.Text;

namespace CalibanLib.Utility
{
    public static class StringEx
    {
        public static string String(this byte[] bytes)
        {
            var chars = new char[bytes.Length];
            var d = Encoding.UTF8.GetDecoder();
            d.GetChars(bytes, 0, bytes.Length, chars, 0);
            var szData = new string(chars);

            return szData;
        }

    }
}