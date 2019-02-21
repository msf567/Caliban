using System;
using EventHook;

namespace Caliban.Core.Windows
{
    public class Point
    {
        public readonly int X;
        public readonly int Y;

        public Point(EventHook.Hooks.Point _p)
        {
            this.X = _p.x;
            this.Y = _p.y;
        }
    }

    public enum MouseMessages
    {
        WM_MOUSEMOVE = 512, // 0x00000200
        WM_LBUTTONDOWN = 513, // 0x00000201
        WM_LBUTTONUP = 514, // 0x00000202
        WM_RBUTTONDOWN = 516, // 0x00000204
        WM_RBUTTONUP = 517, // 0x00000205
        WM_WHEELBUTTONDOWN = 519, // 0x00000207
        WM_WHEELBUTTONUP = 520, // 0x00000208
        WM_MOUSEWHEEL = 522, // 0x0000020A
        WM_XBUTTONDOWN = 523, // 0x0000020B
        WM_XBUTTONUP = 524, // 0x0000020C
    }

    public class MouseArgs : EventArgs
    {
        public MouseArgs(MouseEventArgs _e)
        {
            Message = (MouseMessages) _e.Message;
            Point = new Point(_e.Point);
            MouseData = _e.MouseData;
        }

        public MouseMessages Message { get; set; }

        public Point Point { get; set; }

        public uint MouseData { get; set; }
    }

    public static class GlobalInput
    {
        private static readonly EventHookFactory EventHookFactory = new EventHookFactory();
        private static readonly KeyboardWatcher KbWatcher;
        private static readonly MouseWatcher MouseWatcher;

        public delegate void GlobalKeyPressEvent(string _key);

        public delegate void GlobalMouseMoveEvent(MouseArgs _key);

        public static GlobalKeyPressEvent OnGlobalKeyPress;
        public static GlobalMouseMoveEvent OnGlobalMouseMove;

        static GlobalInput()
        {
            if (KbWatcher == null)
            {
                KbWatcher = EventHookFactory.GetKeyboardWatcher();
                KbWatcher.Start();
            }

            KbWatcher.OnKeyInput += GlobalKeyPress;

            if (MouseWatcher == null)
            {
                MouseWatcher = EventHookFactory.GetMouseWatcher();
                MouseWatcher.Start();
            }

            MouseWatcher.OnMouseInput += GlobalMouse;
        }

        private static void GlobalKeyPress(object _s, KeyInputEventArgs _e)
        {
            if (_e.KeyData.EventType == KeyEvent.down)
                OnGlobalKeyPress?.Invoke(_e.KeyData.Keyname);
        }

        private static void GlobalMouse(object _s, MouseEventArgs _e)
        {
            OnGlobalMouseMove?.Invoke(new MouseArgs(_e));
        }
    }
}