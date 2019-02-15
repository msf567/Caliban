using System;
using System.Diagnostics;
using System.Windows.Forms;
using CalibanLib.Windows;
using System.Drawing;

namespace WaterMeter
{
    internal class WaterMeterProgram
    {
        public static void Main(string[] args)
        {
            int WIDTH = 15;
            int HEIGHT = 30;
            int SWidth = Screen.PrimaryScreen.Bounds.Width;
            Windows.SetWindowPos(Process.GetCurrentProcess().MainWindowHandle,
                IntPtr.Zero, SWidth - WIDTH * 40, 0, 0, 0, Windows.SWP.NOSIZE);
            Windows.DeleteMenu(Windows.GetSystemMenu(Windows.GetConsoleWindow(), false), 
                Windows.SC_CLOSE,
                Windows.MF_BYCOMMAND);

            WaterMeter waterMeter = new WaterMeter(WIDTH, HEIGHT);
        }
    }
}