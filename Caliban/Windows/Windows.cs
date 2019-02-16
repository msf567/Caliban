using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Caliban.Core.Windows
{
    public static class Windows
    {
        public static int GWL_STYLE = -16;
        public static int WS_BORDER = 0x00800000; //window with border
        public static int WS_DLGFRAME = 0x00400000; //window with double border but no title
        public static int WS_CAPTION = WS_BORDER | WS_DLGFRAME; //window with a title bar

        public static void ConfigureMenuWindow()
        {
        }

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
        }

        private static string GetActiveWindowTitle()
        {
            const int nChars = 256;
            var buff = new StringBuilder(nChars);
            var handle = GetForegroundWindow();
            return GetWindowText(handle, buff, nChars) > 0 ? buff.ToString() : null;
        }

        static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        static readonly IntPtr HWND_TOP = new IntPtr(0);
        static readonly IntPtr HWND_BOTTOM = new IntPtr(1);

        public static class Hwnd
        {
            public static IntPtr
                NoTopMost = new IntPtr(-2),
                TopMost = new IntPtr(-1),
                Top = new IntPtr(0),
                Bottom = new IntPtr(1);
        }

        public static class Swp
        {
            public static readonly int
                NOSIZE = 0x0001,
                NOMOVE = 0x0002,
                NOZORDER = 0x0004,
                NOREDRAW = 0x0008,
                NOACTIVATE = 0x0010,
                DRAWFRAME = 0x0020,
                FRAMECHANGED = 0x0020,
                SHOWWINDOW = 0x0040,
                HIDEWINDOW = 0x0080,
                NOCOPYBITS = 0x0100,
                NOOWNERZORDER = 0x0200,
                NOREPOSITION = 0x0200,
                NOSENDCHANGING = 0x0400,
                DEFERERASE = 0x2000,
                ASYNCWINDOWPOS = 0x4000;
        }

        public const int MF_BYCOMMAND = 0x00000000;
        public const int SC_CLOSE = 0xF060;

        [DllImport("user32.dll")]
        public static extern int DeleteMenu(IntPtr _hMenu, int _nPosition, int _wFlags);

        [DllImport("user32.dll")]
        public static extern IntPtr GetSystemMenu(IntPtr _hWnd, bool _bRevert);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        public static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr _hWnd, StringBuilder _text, int _count);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string _lpClassName, string _lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetWindowPos(IntPtr _hWnd, IntPtr _hWndInsertAfter, int _x, int _y, int _cx, int _cy,
            int _uFlags);

        [DllImport("user32.dll")]
        public static extern IntPtr GetActiveWindow();

        [DllImport("USER32.DLL")]
        public static extern int SetWindowLong(IntPtr _hWnd, int _nIndex, int _dwNewLong);

        [DllImport("USER32.DLL")]
        public static extern int GetWindowLong(IntPtr _hWnd, int _nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetWindowRect(IntPtr _hwnd, out RECT _lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left, Top, Right, Bottom;

            public RECT(int _left, int _top, int _right, int _bottom)
            {
                Left = _left;
                Top = _top;
                Right = _right;
                Bottom = _bottom;
            }

            public RECT(System.Drawing.Rectangle _r) : this(_r.Left, _r.Top, _r.Right, _r.Bottom)
            {
            }

            public int X
            {
                get { return Left; }
                set
                {
                    Right -= (Left - value);
                    Left = value;
                }
            }

            public int Y
            {
                get { return Top; }
                set
                {
                    Bottom -= (Top - value);
                    Top = value;
                }
            }

            public int Height
            {
                get { return Bottom - Top; }
                set { Bottom = value + Top; }
            }

            public int Width
            {
                get { return Right - Left; }
                set { Right = value + Left; }
            }

            public System.Drawing.Point Location
            {
                get { return new System.Drawing.Point(Left, Top); }
                set
                {
                    X = value.X;
                    Y = value.Y;
                }
            }

            public System.Drawing.Size Size
            {
                get { return new System.Drawing.Size(Width, Height); }
                set
                {
                    Width = value.Width;
                    Height = value.Height;
                }
            }

            public static implicit operator System.Drawing.Rectangle(RECT _r)
            {
                return new System.Drawing.Rectangle(_r.Left, _r.Top, _r.Width, _r.Height);
            }

            public static implicit operator RECT(System.Drawing.Rectangle _r)
            {
                return new RECT(_r);
            }

            public static bool operator ==(RECT _r1, RECT _r2)
            {
                return _r1.Equals(_r2);
            }

            public static bool operator !=(RECT _r1, RECT _r2)
            {
                return !_r1.Equals(_r2);
            }

            public bool Equals(RECT _r)
            {
                return _r.Left == Left && _r.Top == Top && _r.Right == Right && _r.Bottom == Bottom;
            }

            public override bool Equals(object _obj)
            {
                if (_obj is RECT)
                    return Equals((RECT) _obj);
                else if (_obj is System.Drawing.Rectangle)
                    return Equals(new RECT((System.Drawing.Rectangle) _obj));
                return false;
            }

            public override int GetHashCode()
            {
                return ((System.Drawing.Rectangle) this).GetHashCode();
            }

            public override string ToString()
            {
                return string.Format(System.Globalization.CultureInfo.CurrentCulture,
                    "{{Left={0},Top={1},Right={2},Bottom={3}}}", Left, Top, Right, Bottom);
            }
        }
    }
}