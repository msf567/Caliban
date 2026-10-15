using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Drawing = System.Drawing;
using Screen = System.Windows.Forms.Screen;

namespace Caliban.Core.Menu
{
    /// <summary>
    /// A single screen definition built from a sequence of lines that mirror the
    /// old ConsoleOutput.ConsoleFormat calls (CenterWrite / WriteLine).
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

    internal class MenuForm : Form
    {
        private MenuScreen currentScreen;

        public MenuForm()
        {
            this.BackColor = Color.Black;
            this.ForeColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.Manual;
            this.DoubleBuffered = true;
            this.Text = "Caliban";

            // Console font feel
            this.Font = new Font("Consolas", 14, FontStyle.Regular, GraphicsUnit.Point);
        }

        public void UpdateScreen(MenuScreen screen, int heightChars, int widthChars, bool dock)
        {
            this.currentScreen = screen;

            using (var g = this.CreateGraphics())
            {
                var h = this.Font.Height;
                // Average width of monospaced character "W"
                var w = TextRenderer.MeasureText(g, "W", this.Font, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding).Width;
                
                this.ClientSize = new Size(widthChars * w, heightChars * h);
            }

            if (dock)
            {
                var screenBounds = Screen.PrimaryScreen.Bounds;
                this.Location = new Point((screenBounds.Width - this.Width) / 2, 0);
            }

            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (currentScreen == null) return;

            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            float y = 0;
            float lineHeight = this.Font.GetHeight(e.Graphics);

            foreach (var line in currentScreen.Lines)
            {
                using (var brush = new SolidBrush(line.Color == default(Color) ? Color.White : line.Color))
                {
                    if (line.Centered)
                    {
                        var size = e.Graphics.MeasureString(line.Text, this.Font);
                        e.Graphics.DrawString(line.Text, this.Font, brush, (this.ClientSize.Width - size.Width) / 2, y);
                    }
                    else
                    {
                        e.Graphics.DrawString(line.Text, this.Font, brush, 0, y);
                    }
                }
                y += lineHeight;
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            MenuApp.OnKeyPressed?.Invoke(e.KeyCode);
        }
    }

    /// <summary>
    /// Host for the menu using Windows Forms to create a dedicated window.
    /// </summary>
    internal static class MenuApp
    {
        private const int MenuWidth = 96;

        private static bool initialized;
        public static MenuForm Form { get; private set; }
        public static Action<Keys> OnKeyPressed;

        public static bool IsInitialized => initialized;

        public static void EnsureInit()
        {
            if (initialized)
                return;

            Form = new MenuForm();
            Form.Show();
            initialized = true;
        }

        /// <summary>
        /// Builds the screen, applies the per-screen sizing, swaps it in as the
        /// current content and (optionally) re-docks the window to the top-center.
        /// </summary>
        public static void Show(MenuScreen _screen, int _height, bool _dock = true)
        {
            EnsureInit();
            Form.UpdateScreen(_screen, _height, MenuWidth, _dock);
        }
    }
}