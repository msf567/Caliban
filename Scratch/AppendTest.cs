using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        string dummyExe = "dummy.exe";
        File.WriteAllBytes(dummyExe, new byte[100]); // Fake exe

        // Write
        using (var fs = new FileStream(dummyExe, FileMode.Append, FileAccess.Write))
        using (var bw = new BinaryWriter(fs, Encoding.UTF8, true))
        {
            long startPos = fs.Position;
            bw.Write(Encoding.UTF8.GetBytes("CALIBAN_TREASURE"));
            bw.Write(2); // count
            bw.Write("location"); bw.Write("A:\\Caliban\\Desert");
            bw.Write("clue"); bw.Write("hello world");
            long payloadLen = fs.Position - startPos;
            bw.Write((int)payloadLen);
            bw.Write(Encoding.UTF8.GetBytes("CALIBAN_END"));
        }

        // Read
        using (var fs = new FileStream(dummyExe, FileMode.Open, FileAccess.Read))
        using (var br = new BinaryReader(fs, Encoding.UTF8, true))
        {
            fs.Position = fs.Length - 11;
            byte[] endMarker = br.ReadBytes(11);
            if (Encoding.UTF8.GetString(endMarker) == "CALIBAN_END")
            {
                fs.Position = fs.Length - 11 - 4;
                int payloadLen = br.ReadInt32();
                fs.Position = fs.Length - 11 - 4 - payloadLen;
                
                byte[] startMarker = br.ReadBytes(16);
                if (Encoding.UTF8.GetString(startMarker) == "CALIBAN_TREASURE")
                {
                    int count = br.ReadInt32();
                    for (int i = 0; i < count; i++)
                    {
                        string k = br.ReadString();
                        string v = br.ReadString();
                        Console.WriteLine($"{k} = {v}");
                    }
                }
            }
        }
    }
}
