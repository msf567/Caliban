namespace Caliban.ConsoleOutput
{
    public static class ConsoleFormat
    {
        public static void CenterWrite(string s)
        {
            var consoleWidth = System.Console.WindowWidth;
            var sLen = s.Length;

            var gap = consoleWidth - sLen;
            System.Console.WriteLine(new string(' ', gap / 2) + s);
        }
    }
}