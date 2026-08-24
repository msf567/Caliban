using System;
using System.Collections.Generic;
using System.Diagnostics;
using Terminal.Gui;
using Drawing = System.Drawing;
using TColor = Terminal.Gui.Color;
using Screen = System.Windows.Forms.Screen;

namespace Caliban.Core.Menu
{
    /// <summary>
    /// A single screen definition built from a sequence of lines that mirror the
    /// old <see cref="ConsoleOutput.ConsoleFormat"/> calls (CenterWrite / WriteLine).
    /// </summary>
    internal sealed class MenuScreen
    {
        internal readonly List<Line> Lines = new List<Line>();

        internal struct Line
        {
            public string Text;
            public Drawing.Color Color;
            public bool Centered;
        }

        public void CenterWrite(string _text, Drawing.Color _color = default(Drawing.Color))
        {
            Lines.Add(new Line { Text = _text ?? "", Color = _color, Centered = true });
        }

        public void WriteLine(string _text, Drawing.Color _color = default(Drawing.Color))
        {
            Lines.Add(new Line { Text = _text ?? "", Color = _color, Centered = false });
        }
    }

    /// <summary>
    /// Terminal.Gui host for the menu. Owns the one-time Application.Init, holds the
    /// current screen content view, maps the legacy true-color intent to the 16-color
    /// palette, and reproduces the old per-screen console sizing / top-center docking.
    /// </summary>
    internal static class MenuApp
    {
        private const int MenuWidth = 96;

        private static bool initialized;
        private static View currentContent;

        public static bool IsInitialized => initialized;

        public static void EnsureInit()
        {
            if (initialized)
                return;

            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Application.Init();
            Application.Top.ColorScheme = MakeScheme(TColor.White, TColor.Black);
            initialized = true;
        }

        /// <summary>
        /// Builds the screen, applies the per-screen console size, swaps it in as the
        /// current content and (optionally) re-docks the window to the top-center.
        /// Must be invoked on the Terminal.Gui main loop thread (callers on background
        /// threads marshal through <see cref="MainLoop.Invoke"/>).
        /// </summary>
        public static void Show(MenuScreen _screen, int _height, bool _dock = true)
        {
            EnsureInit();

            ResizeConsole(_height);

            var content = Build(_screen);
            SwapContent(content);

            if (_dock)
                DockToTop();

            if (Application.Driver != null)
                Application.Refresh();
        }

        /// <summary>
        /// Maps the legacy <see cref="System.Drawing.Color"/> intent used by the old
        /// menu to the nearest Terminal.Gui 16-color value.
        /// </summary>
        public static TColor MapColor(Drawing.Color _c)
        {
            if (_c == default(Drawing.Color))
                return TColor.White; // ConsoleFormat default was Azure -> white

            if (_c.ToArgb() == Drawing.Color.Gold.ToArgb()) return TColor.BrightYellow;
            if (_c.ToArgb() == Drawing.Color.Yellow.ToArgb()) return TColor.BrightYellow;
            if (_c.ToArgb() == Drawing.Color.Coral.ToArgb()) return TColor.BrightRed;
            if (_c.ToArgb() == Drawing.Color.Azure.ToArgb()) return TColor.White;
            if (_c.ToArgb() == Drawing.Color.DarkGray.ToArgb()) return TColor.DarkGray;
            if (_c.ToArgb() == Drawing.Color.Red.ToArgb()) return TColor.BrightRed;
            if (_c.ToArgb() == Drawing.Color.Green.ToArgb()) return TColor.BrightGreen;

            return TColor.White;
        }

        private static ColorScheme MakeScheme(TColor _fg, TColor _bg)
        {
            var attr = Application.Driver != null
                ? Application.Driver.MakeAttribute(_fg, _bg)
                : new Terminal.Gui.Attribute(_fg, _bg);

            return new ColorScheme
            {
                Normal = attr,
                Focus = attr,
                HotNormal = attr,
                HotFocus = attr,
                Disabled = attr
            };
        }

        private static View Build(MenuScreen _screen)
        {
            var container = new View
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                ColorScheme = MakeScheme(TColor.White, TColor.Black)
            };

            int y = 0;
            foreach (var line in _screen.Lines)
            {
                var label = new Label(line.Text)
                {
                    X = line.Centered ? Pos.Center() : (Pos)0,
                    Y = y,
                    AutoSize = true,
                    ColorScheme = MakeScheme(MapColor(line.Color), TColor.Black)
                };
                container.Add(label);
                y++;
            }

            return container;
        }

        private static void SwapContent(View _content)
        {
            if (currentContent != null)
                Application.Top.Remove(currentContent);

            currentContent = _content;
            Application.Top.Add(_content);
        }

        private static void ResizeConsole(int _height)
        {
            try
            {
                Console.SetWindowSize(MenuWidth, _height);
                Console.SetBufferSize(MenuWidth, _height);
            }
            catch (Exception)
            {
                // ignored - some hosts don't allow resizing
            }
        }

        private static void DockToTop()
        {
            var hwnd = OS.Windows.GetConsoleWindow();
            if (hwnd == IntPtr.Zero)
                hwnd = Process.GetCurrentProcess().MainWindowHandle;

            var sWidth = Screen.PrimaryScreen.Bounds.Width;
            OS.Windows.GetWindowRect(hwnd, out OS.Windows.RECT r);

            OS.Windows.SetWindowPos(hwnd, IntPtr.Zero, (sWidth / 2) - (r.Width / 2), 0, 0, 0,
                OS.Windows.Swp.NOSIZE);
        }
    }
}