using System;
using System.IO;
using System.Media;
using System.Reflection;
using System.Threading;
using DesertGenerator;

namespace DesertCore
{
    internal static class DesertCoreProgram
    {
        private static SoundPlayer _music = new SoundPlayer();

        public static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[8 * 1024];
            int len;
            while ((len = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, len);
            }
        }

        public static void Main(string[] args)
        {
            if (!Directory.Exists("WaterMeter"))
                Directory.CreateDirectory("WaterMeter");
            var thisAssembly = Assembly.GetExecutingAssembly();

            using (var stream = thisAssembly.GetManifestResourceStream("DesertCore.WaterMeter.exe"))
            {
                using (Stream file = File.Create("WaterMeter\\WaterMeter.exe"))
                {
                    CopyStream(stream, file);
                }
            }

            var userKey = DrawMenu(true);

            while (userKey != 'q')
            {
                switch (userKey)
                {
                    case 'g':
                        DesertGenerator.GenerateDesert();
                        break;
                    case 'c':
                        DesertGenerator.ClearDesert();
                        break;
                    default:
                        break;
                }

                userKey = DrawMenu();
            }
        }


        private static char DrawMenu(bool introMode = false)
        {
            Console.Clear();
            Console.SetWindowSize(122, 24);
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            if (introMode)
            {
                _music.SoundLocation = "desert.wav";
                _music.Load();
                Thread.Sleep(2000);
                _music.Play();
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            CenterWrite("~~~");
            CenterWrite("Gentle_Virus Presents");
            CenterWrite("~~~");
            if (introMode) Thread.Sleep(2398);
            CenterWrite("A File System Survival Game");
            CenterWrite("~~~");
            if (introMode) Thread.Sleep(2412);
            Console.ForegroundColor = DesertGenerator.DesertGenerated ? ConsoleColor.Yellow : ConsoleColor.Yellow;

            foreach (var s in TitleGraphic)
            {
                CenterWrite(s);
                if (introMode) Thread.Sleep(594);
            }

            CenterWrite("");
            CenterWrite("");
            CenterWrite("");

            Console.ForegroundColor = ConsoleColor.White;
            CenterWrite(@"(E)mbark | (H)elp | (A)bout | (Q)uit");
            return Console.ReadKey().KeyChar;
        }

        private static void CenterWrite(string s)
        {
            var consoleWidth = Console.WindowWidth;
            var sLen = s.Length;

            var gap = consoleWidth - sLen;
            Console.WriteLine(new string(' ', gap / 2) + s);
        }

        private static string[] TitleGraphic = new string[]
        {
            " ▄████████    ▄████████  ▄█        ▄█  ▀█████████▄     ▄████████ ███▄▄▄▄       ",
            "███    ███   ███    ███ ███       ███    ███    ███   ███    ███ ███▀▀▀██▄     ",
            "███    █▀    ███    ███ ███       ███▌   ███    ███   ███    ███ ███   ███     ",
            "███          ███    ███ ███       ███▌  ▄███▄▄▄██▀    ███    ███ ███   ███     ",
            "███        ▀███████████ ███       ███▌ ▀▀███▀▀▀██▄  ▀███████████ ███   ███     ",
            "███    █▄    ███    ███ ███       ███    ███    ██▄   ███    ███ ███   ███     ",
            "███    ███   ███    ███ ███▌    ▄ ███    ███    ███   ███    ███ ███   ███     ",
            "████████▀    ███    █▀  █████▄▄██ █▀   ▄█████████▀    ███    █▀   ▀█   █▀      "
        };
    }
}