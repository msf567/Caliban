using System;
using System.Runtime.InteropServices;
using System.Text;

namespace CalibanLib.Windows
{
    public static class Windows
    {
        private static void GetHighlightedFile()
        {
            var myHwnd = FindWindow(null, "Builds");
            var t = Type.GetTypeFromProgID("Shell.Application");
            dynamic o = Activator.CreateInstance(t);
            try
            {
                var ws = o.Windows();
                for (var i = 0; i < ws.Count; i++)
                {
                    var ie = ws.Item(i);
                    if (ie == null || ie.hwnd != (long) myHwnd) continue;
                    var path = System.IO.Path.GetFileName((string) ie.FullName);
                    if (path == null || path.ToLower() != "explorer.exe") continue;
                    var explorepath = ie.document.focuseditem.path;
                    System.Console.WriteLine(explorepath);
                }
            }
            finally
            {
                Marshal.FinalReleaseComObject(o);
            }
        } //export

        private static string GetActiveWindowTitle()
        {
            const int nChars = 256;
            var buff = new StringBuilder(nChars);
            var handle = GetForegroundWindow();
            return GetWindowText(handle, buff, nChars) > 0 ? buff.ToString() : null;
        } //export

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
    }
}