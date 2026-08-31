using System;
using SharpHook;
using SharpHook.Data;

namespace Caliban.Core.Windows
{
    public class Point
    {
        public readonly int X;
        public readonly int Y;

        public Point(int x, int y)
        {
            this.X = x;
            this.Y = y;
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
        public MouseArgs(MouseMessages message, Point point, uint mouseData = 0)
        {
            Message = message;
            Point = point;
            MouseData = mouseData;
        }

        public MouseMessages Message { get; set; }
        public Point Point { get; set; }
        public uint MouseData { get; set; }
    }

    public static class GlobalInput
    {
        private static readonly SimpleGlobalHook Hook;

        public delegate void GlobalKeyPressEvent(string key);

        public delegate void GlobalMouseMoveEvent(MouseArgs e);

        public static event GlobalKeyPressEvent? OnGlobalKeyPress;
        public static event GlobalMouseMoveEvent? OnGlobalMouseAction;

        static GlobalInput()
        {
            Hook = new SimpleGlobalHook();

            // Subscribe to Keyboard events
            Hook.KeyPressed += OnKeyPressed;

            // Subscribe to Mouse events
            Hook.MouseMoved += OnMouseMoved;
            Hook.MousePressed += OnMousePressed;
            Hook.MouseReleased += OnMouseReleased;
            Hook.MouseWheel += OnMouseWheel;

            // Start hook background task (does not block main thread)
            Hook.RunAsync();
        }

        private static void OnKeyPressed(object? sender, KeyboardHookEventArgs e)
        {
            OnGlobalKeyPress?.Invoke(e.Data.KeyCode.ToString());
        }

        private static void OnMouseMoved(object? sender, MouseHookEventArgs e)
        {
            var args = new MouseArgs(
                MouseMessages.WM_MOUSEMOVE,
                new Point(e.Data.X, e.Data.Y)
            );
            OnGlobalMouseAction?.Invoke(args);
        }

        private static void OnMousePressed(object? sender, MouseHookEventArgs e)
        {
            var msg = e.Data.Button switch
            {
                MouseButton.Button1 => MouseMessages.WM_LBUTTONDOWN,
                MouseButton.Button2 => MouseMessages.WM_RBUTTONDOWN,
                MouseButton.Button3 => MouseMessages.WM_WHEELBUTTONDOWN,
                _ => MouseMessages.WM_XBUTTONDOWN
            };

            var args = new MouseArgs(msg, new Point(e.Data.X, e.Data.Y));
            OnGlobalMouseAction?.Invoke(args);
        }

        private static void OnMouseReleased(object? sender, MouseHookEventArgs e)
        {
            var msg = e.Data.Button switch
            {
                MouseButton.Button1 => MouseMessages.WM_LBUTTONUP,
                MouseButton.Button2 => MouseMessages.WM_RBUTTONUP,
                MouseButton.Button3 => MouseMessages.WM_WHEELBUTTONUP,
                _ => MouseMessages.WM_XBUTTONUP
            };

            var args = new MouseArgs(msg, new Point(e.Data.X, e.Data.Y));
            OnGlobalMouseAction?.Invoke(args);
        }

        private static void OnMouseWheel(object? sender, MouseWheelHookEventArgs e)
        {
            var args = new MouseArgs(
                MouseMessages.WM_MOUSEWHEEL,
                new Point(e.Data.X, e.Data.Y),
                (uint)e.Data.Rotation
            );
            OnGlobalMouseAction?.Invoke(args);
        }

        /// <summary>
        /// Call when shutting down the application to unhook Windows hooks cleanly.
        /// </summary>
        public static void Stop()
        {
            Hook.Dispose();
        }
    }
}