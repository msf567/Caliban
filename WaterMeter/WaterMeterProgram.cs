using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using EventHook;

namespace WaterMeter
{
    internal static class WaterMeterProgram
    {
        private static float WaterLevel = 0.7f;
        private const int WaterMeterRenderHeight = 40;
        private static readonly EventHookFactory EventHookFactory = new EventHookFactory();

        public static void Main(string[] args)
        {
           SetKeyboardHook();
           Console.SetWindowSize(10,WaterMeterRenderHeight);
           
            RenderWaterLevel();
            while (Console.ReadKey().Key != ConsoleKey.Q)
            {
                Thread.Sleep(20);
            }
            
        }

        private static void RenderWaterLevel()
        {
                Console.WriteLine("_____");
                Console.WriteLine("|   |");
                Console.WriteLine("|XXX|");
            
        }

        private static void SetKeyboardHook()
        {
            var kbWatcher = EventHookFactory.GetKeyboardWatcher();
            kbWatcher.Start();
            kbWatcher.OnKeyInput += OnGlobalKeyPress;
        }

        private static void OnGlobalKeyPress(object s, KeyInputEventArgs e)
        {
            if(e.KeyData.Keyname == "Q")
                Console.WriteLine(GetActiveWindowTitle());
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
    }
}