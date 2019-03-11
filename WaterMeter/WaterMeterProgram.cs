using System;
using System.Diagnostics;
using Caliban.Core.OS;
using Caliban.Core.Utility;

namespace WaterMeter
{
    internal class WaterMeterProgram
    {
        public static void Main(string[] _args)
        {
            IntPtr hwnd = Process.GetCurrentProcess().MainWindowHandle;
            Windows.SetWindowPos(hwnd, IntPtr.Zero, 0, -10, 0, 0, Windows.Swp.NOSIZE);
            int width = 15;
            int height = 30;
            D.Write("Init");

            WaterMeter waterMeter = new WaterMeter(width, height);
        }
    }
}