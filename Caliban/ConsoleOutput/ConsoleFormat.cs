using System;
using System.Drawing;
using Console = Colorful.Console;

namespace Caliban.Core.ConsoleOutput
{
    public static class ConsoleFormat
    {
        public static void CenterWrite(string _s, Color _color = default(Color))
        {
            if (_color == default(Color))
                _color = Color.Azure;
            var consoleWidth = Console.WindowWidth;
            var sLen = _s.Length;

            var gap = consoleWidth - sLen;
            Console.WriteLine(new string(' ', gap / 2) + _s,_color);
        }
        
        public static void WriteLine(string _s, Color _color = default(Color))
        {
            if (_color == default(Color))
                _color = Color.Azure;       
            Console.WriteLine(_s,_color);
        }
        
        public static void CenterWriteWithGradient(string _s, Color _colorBegin, Color _colorEnd)
        {
          
            var consoleWidth = Console.WindowWidth;
            var sLen = _s.Length;

            var gap = consoleWidth - sLen;
            Console.WriteWithGradient(new string(' ', gap / 2) + _s + Environment.NewLine, _colorBegin, _colorEnd);      
        }
    }
}