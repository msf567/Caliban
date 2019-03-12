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
            Process[] pname = Process.GetProcessesByName("CALIBAN");
            if (pname.Length == 0)
                return;
            
            IntPtr hwnd = Process.GetCurrentProcess().MainWindowHandle;
            Windows.SetWindowPos(hwnd, IntPtr.Zero, 0, -10, 0, 0, Windows.Swp.NOSIZE);
            int width = 20;
            int height = 40;
            D.Write("Init");

            WaterMeter waterMeter = new WaterMeter(width, height);
        }
    }
}