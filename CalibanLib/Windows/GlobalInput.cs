using System;
using EventHook;

namespace CalibanLib.Windows
{
    public class GlobalInput
    {
        private static readonly EventHookFactory EventHookFactory = new EventHookFactory();
        private static KeyboardWatcher kbWatcher;
        private static MouseWatcher mouseWatcher;
        
        public static void RegisiterOnGlobalKeyPress(EventHandler<KeyInputEventArgs> callback)
        {
            if (kbWatcher == null)
            {
                kbWatcher = EventHookFactory.GetKeyboardWatcher();
                kbWatcher.Start();
            }

            kbWatcher.OnKeyInput += callback;
        }

        public static void RegisterOnGlobalMouse(EventHandler<MouseEventArgs> callback)
        {
            if (mouseWatcher == null)
            {
                mouseWatcher = EventHookFactory.GetMouseWatcher();
                mouseWatcher.Start();
            }

            mouseWatcher.OnMouseInput += callback;
        }
    }
}