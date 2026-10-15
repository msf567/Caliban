using System;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello from single file!");
        long len = new FileInfo(Environment.ProcessPath).Length;
        Console.WriteLine($"My size: {len}");
    }
}
