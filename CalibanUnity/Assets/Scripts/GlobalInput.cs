using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using UnityEngine;
#pragma warning disable 649

public class GlobalInput : MonoBehaviour
{
    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_KEYUP = 0x0101;
    private const int WH_MOUSE_LL = 14;
    private const int WM_LBUTTONDOWN = 0x201;

    private static LowLevelProc _MSProc = MSHookCallback;


    private static LowLevelProc _KBProc = KBHookCallback;

    private static IntPtr _KBHookID = IntPtr.Zero;
    private static IntPtr _MSHookID = IntPtr.Zero;

    private delegate IntPtr LowLevelProc(int nCode, IntPtr wParam, IntPtr lParam);

    public delegate void GlobalKeyEvent(Keys key);

    public delegate void GlobalMouseEvent();

    public static GlobalMouseEvent OnMouseClick;
    public static GlobalKeyEvent OnKeyPress;
    public static GlobalKeyEvent OnKeyRelease;

    [StructLayout(LayoutKind.Sequential)]
    private class POINT
    {
        public int x;
        public int y;
    }
    
    [StructLayout(LayoutKind.Sequential)]
    private class MouseLLHookStruct
    {
        public POINT pt;
        public int mouseData;
        public int flags;
        public int time;
        public int dwExtraInfo;
    }

    void Start()
    {
        _KBHookID = SetKBHook(_KBProc);
        _MSHookID = SetMSHook(_MSProc);
    }

    void OnApplicationQuit()
    {
        UnhookWindowsHookEx(_KBHookID);
        UnhookWindowsHookEx(_MSHookID);
    }

    private static IntPtr SetKBHook(LowLevelProc proc)
    {
        using (System.Diagnostics.Process curProcess = System.Diagnostics.Process.GetCurrentProcess())
        using (System.Diagnostics.ProcessModule curModule = curProcess.MainModule)
        {
            return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                GetModuleHandle(curModule.ModuleName), 0);
        }
    }

    private static IntPtr SetMSHook(LowLevelProc proc)
    {
        using (System.Diagnostics.Process curProcess = System.Diagnostics.Process.GetCurrentProcess())
        using (System.Diagnostics.ProcessModule curModule = curProcess.MainModule)
        {
            return SetWindowsHookEx(WH_MOUSE_LL, proc,
                GetModuleHandle(curModule.ModuleName), 0);
        }
    }

    private static IntPtr KBHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && wParam == (IntPtr) WM_KEYDOWN)
        {
            int vkCode = Marshal.ReadInt32(lParam);
            Keys key = (Keys) vkCode;
            if (OnKeyPress != null)
                OnKeyPress(key);
        }
        else if (nCode >= 0 && wParam == (IntPtr) WM_KEYUP)
        {
            int vkCode = Marshal.ReadInt32(lParam);
            Keys key = (Keys) vkCode;
            if (OnKeyRelease != null)
                OnKeyRelease(key);
        }

        return CallNextHookEx(_KBHookID, nCode, wParam, lParam);
    }

    private static IntPtr MSHookCallback(int nCode, IntPtr wparam, IntPtr lparam)
    {
        if (nCode >= 0)
        {
            if (wparam == (IntPtr)WM_LBUTTONDOWN)
            {
                if (OnMouseClick != null)
                    OnMouseClick();
            }    
        }

        return CallNextHookEx(_MSHookID, nCode, wparam, lparam);
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);
}