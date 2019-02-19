using System;
using System.IO;
using System.Net;
using System.Threading;
using Caliban.Core.ConsoleOutput;

namespace Note
{
    internal class NoteProgram
    {
        static bool closeFlag;

        public static void Main(string[] args)
        {
            if (args.Length < 1)
                return;
            string notePath = args[0];
            var lines = File.ReadLines("intro.txt");
            foreach (var line in lines)
            {
                ConsoleFormat.CenterWrite(line);
                Thread.Sleep(1000);
            }

            while (!closeFlag)
            {
                Thread.Sleep(50);
            }
        }
    }
}