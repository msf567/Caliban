using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using EventHook;

namespace WaterMeter
{
    internal static class WaterMeterProgram
    {
        private static float TargetWaterLevel = 0.7f;
        private static float WaterLevel = 0.7f;
        private static float prevWaterLevel;
        private const int WaterMeterRenderHeight = 40;
        private static readonly EventHookFactory EventHookFactory = new EventHookFactory();

        public static void Main(string[] args)
        {
            SetMouseHook();
             SetKeyboardHook();
            Console.SetWindowSize(20, WaterMeterRenderHeight + 4);
            Console.SetBufferSize(20, WaterMeterRenderHeight + 4);
            TargetWaterLevel = WaterLevel;
            Thread t = new Thread(UpdateThread);
            t.Start();
            while (true)
            {
                Thread.Sleep(20);
            }
        }

        private static void UpdateThread()
        {
            while (true)
            {
                WaterLevel = TargetWaterLevel.Clamp(0, 1);
                if (Math.Abs(prevWaterLevel - WaterLevel) > 0.001f)
                {
                    //RenderWaterLevel();
                }
                Thread.Sleep(100);
                prevWaterLevel = WaterLevel;
            }
        }

        public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) return min;
            else if (val.CompareTo(max) > 0) return max;
            else return val;
        }

        private static void RenderWaterLevel()
        {
            if (Math.Abs(WaterLevel) < 0.0001f)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.Clear();
            }
            else
                Console.SetCursorPosition(0, 0);

            CenterWrite("Water Level: " + Math.Ceiling((WaterLevel * 100)) + "/100");
            CenterWrite("________________");


            int numEmpty = (int) Math.Floor(((1 - WaterLevel) * WaterMeterRenderHeight));
            int numFull = (int) Math.Ceiling((WaterLevel) * WaterMeterRenderHeight);


            for (int x = 0; x < numEmpty; x++)
                CenterWrite("|              |");
            for (int x = 0; x < numFull; x++)
            {
                CenterWrite("|XXXXXXXXXXXXXX|");
            }

            CenterWrite("----------------");
        }

        private static void CenterWrite(string s)
        {
            var consoleWidth = Console.WindowWidth;
            var sLen = s.Length;

            var gap = consoleWidth - sLen;
            Console.WriteLine(new string(' ', gap / 2) + s);
        }

        private static void SetKeyboardHook()
        {
            var kbWatcher = EventHookFactory.GetKeyboardWatcher();
            kbWatcher.Start();
            kbWatcher.OnKeyInput += OnGlobalKeyPress;
        }

        private static void SetMouseHook()
        {
            var mouseWatcher = EventHookFactory.GetMouseWatcher();
            mouseWatcher.Start();
            mouseWatcher.OnMouseInput += OnGlobalMouseMove;
        }

        private static void OnGlobalMouseMove(object s, MouseEventArgs e)
        {
            TargetWaterLevel -= 0.0005f;
        }


        private static void OnGlobalKeyPress(object s, KeyInputEventArgs e)
        {
            if (e.KeyData.Keyname == "Q" && e.KeyData.EventType == KeyEvent.down)
            {
                Console.WriteLine("hey");
                IntPtr MyHwnd = FindWindow(null, "Builds");
                var t = Type.GetTypeFromProgID("Shell.Application");
                dynamic o = Activator.CreateInstance(t);
                try
                {
                    var ws = o.Windows();
                    for (int i = 0; i < ws.Count; i++)
                    {
                        var ie = ws.Item(i);
                        if (ie == null || ie.hwnd != (long)MyHwnd) continue;
                        var path = System.IO.Path.GetFileName((string)ie.FullName);
                        if (path.ToLower() == "explorer.exe")
                        {
                            var explorepath = ie.document.focuseditem.path;
                            Console.WriteLine(explorepath);
                        }
                        else
                        {
                            Console.WriteLine("nope " + path);
                        }
                    }
                }
                finally
                {
                    Console.WriteLine("finally");
                    Marshal.FinalReleaseComObject(o);
                } 
            }
        }

        private static string GetActiveWindowTitle()
        {
            const int nChars = 256;
            var buff = new StringBuilder(nChars);
            var handle = GetForegroundWindow();

            return GetWindowText(handle, buff, nChars) > 0 ? buff.ToString() : null;
        }

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);
        
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
    }
}