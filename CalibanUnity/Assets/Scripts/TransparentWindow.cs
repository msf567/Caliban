using System;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Windows.Forms;
public class TransparentWindow : MonoSingleton<TransparentWindow>
{
    [SerializeField]
    private Material m_Material;
    private struct MARGINS
    {
        public int cxLeftWidth;
        public int cxRightWidth;
        public int cyTopHeight;
        public int cyBottomHeight;
    }

    public bool Clickthrough = false;

    int fWidth, fHeight;
    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

    [DllImport("Dwmapi.dll")]
    private static extern uint DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS margins);

    [DllImport("user32.dll")]
    static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll", EntryPoint = "SetLayeredWindowAttributes")]
    static extern int SetLayeredWindowAttributes(IntPtr hwnd, int crKey, byte bAlpha, int dwFlags);

    [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
    private static extern int SetWindowPos(IntPtr hwnd, int hwndInsertAfter, int x, int y, int cx, int cy, int uFlags);

    [DllImport("user32.dll", EntryPoint = "FindWindow")]
    public static extern IntPtr FindWindow(System.String className, System.String windowName);


    const int GWL_STYLE = -16;
    const uint WS_POPUP = 0x80000000;
    const uint WS_VISIBLE = 0x10000000;
    const int HWND_TOPMOST = -1;
    const int HWND_BOTTOM = 1;
    const int GWL_EXSTYLE = -20;
    const uint TOOL_WINDOW = 0x00000080;
    const int WS_EX_LAYERED = 524288;
    public bool topmost = true;
    MARGINS margins;
    IntPtr hwnd;
    void Awake()
    {
        margins = new MARGINS() { cxLeftWidth = -1 };
        hwnd = GetMyWindow();
        if (!UnityEngine.Application.isEditor)
        {
            SetWindowLong(hwnd, GWL_STYLE, WS_POPUP | WS_VISIBLE);
            if (Clickthrough)
                EnableClickThrough();
            if (topmost)
                SetTopMost();
        }

        if (!UnityEngine.Application.isEditor)
        {
            DwmExtendFrameIntoClientArea(hwnd, ref margins);
        }
    }

    public void SetTopMost()
    {
#if !UNITY_EDITOR
        SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, fWidth, fHeight, 32 | 64);
#endif
    }

    public override void Init()
    {
        if (!UnityEngine.Application.isEditor)
        {
            DwmExtendFrameIntoClientArea(hwnd, ref margins);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown("q"))
        {
            if (!UnityEngine.Application.isEditor)
            {
                DwmExtendFrameIntoClientArea(hwnd, ref margins);
            }
        }
    }

    public void EnableClickThrough()
    {
        var hwnd = GetMyWindow();
        SetWindowLong(hwnd, GWL_EXSTYLE, WS_EX_LAYERED | 32 | WS_POPUP | TOOL_WINDOW);
    }

        IntPtr GetMyWindow()
    {
        string windowName = UnityEngine.Application.productName;
        IntPtr hwnd = FindWindow(null, windowName);

        return hwnd;
    }
}
