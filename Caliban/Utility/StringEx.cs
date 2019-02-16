using System.Text;

namespace Caliban.Core.Utility
{
    public static class StringEx
    {
        public static string String(this byte[] _bytes)
        {
            var chars = new char[_bytes.Length];
            var d = Encoding.UTF8.GetDecoder();
            d.GetChars(_bytes, 0, _bytes.Length, chars, 0);
            var szData = new string(chars);

            return szData;
        }
    }
}