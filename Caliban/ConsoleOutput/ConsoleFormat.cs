using System;

namespace Caliban.Core.ConsoleOutput
{
    public static class ConsoleFormat
    {
        public static void CenterWrite(string _s)
        {
            var consoleWidth = Console.WindowWidth;
            var sLen = _s.Length;

            var gap = consoleWidth - sLen;
            Console.WriteLine(new string(' ', gap / 2) + _s);
        }
    }
}