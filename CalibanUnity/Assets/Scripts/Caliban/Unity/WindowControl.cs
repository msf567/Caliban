using System.Collections;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using Application = UnityEngine.Application;
using Debug = UnityEngine.Debug;

public class WindowControl : MonoSingleton<WindowControl>
{
    protected delegate bool EnumWindowsProc(IntPtr _hWnd, IntPtr _lParam);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowText(IntPtr _hWnd, StringBuilder _strText, int _maxCount);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowTextLength(IntPtr _hWnd);

    [DllImport("user32.dll")]
    protected static extern bool EnumWindows(EnumWindowsProc _enumProc, IntPtr _lParam);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr _hWnd);

    private  readonly List<IntPtr> OpenWindows = new List<IntPtr>();
    private  readonly List<IntPtr> NewWindows = new List<IntPtr>();
    private  List<WindowQuad> windowQuads = new List<WindowQuad>();

    public GameObject WindowQuadPrefab;
    public static WindowControl Instance;
    
    private static string GetWindowTitle(IntPtr hWnd)
    {
        int size = GetWindowTextLength(hWnd) + 1;
        StringBuilder sb = new StringBuilder(size);
        GetWindowText(hWnd, sb, size);
        return sb.ToString();
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        NewWindows.Clear();
        EnumWindows(new EnumWindowsProc(EnumTheWindows), IntPtr.Zero);

        var closedWindows = OpenWindows.Where(_p => NewWindows.All(_p2 => _p2 != _p));

        var intPtrs = closedWindows.ToList();
        for (var x = intPtrs.Count() - 1; x >= 0; x--)
        {
            IntPtr w = intPtrs[x];
            DestroyWindowQuad(w);
        }
        
        OrderWindows();
    }

    void OrderWindows()
    {
        int z = 0;
        foreach (var window in Instance.OpenWindows)
        {
            WindowQuad q = Instance.windowQuads.Find(e => e.Window == window);
            q.gameObject.GetComponent<RectTransform>().localPosition = new Vector3(q.gameObject.transform.position.x,q.gameObject.transform.position.y,z);
            z-=10;
        }
    }

    private static bool IsValidWindow(IntPtr w)
    {
        string windowTitle = GetWindowTitle(w);
        return !((string.IsNullOrEmpty(windowTitle) ||
                  w == Process.GetCurrentProcess().MainWindowHandle ||
                  windowTitle == "Settings" ||
                  windowTitle == Application.productName ||
                  windowTitle == "Program Manager"
                  || windowTitle == "Microsoft Edge" ||
                  windowTitle.Length < 2));
    }
    
    private static void SpawnWindowQuad(IntPtr w)
    {
        if(!IsValidWindow(w))  return;
        
        string windowTitle = GetWindowTitle(w);
        Instance.OpenWindows.Add(w);

        GameObject newQuad = Instantiate(Instance.WindowQuadPrefab) as GameObject;
        newQuad.transform.SetParent(Instance.transform);
        newQuad.GetComponent<WindowQuad>().Window = w;
        newQuad.name = windowTitle;
        Instance.windowQuads.Add(newQuad.GetComponent<WindowQuad>());
        Debug.Log("Opening " + windowTitle + "(" + w.ToString() + ")");
    }

    private void DestroyWindowQuad(IntPtr w)
    {
        var q = windowQuads.Find(_e => _e.Window == w);
        if (!q) return;
        
        windowQuads.Remove(q);
        OpenWindows.Remove(w);
        Debug.Log("Closing " + GetWindowTitle(w) + "(" + w.ToString() + ")");

        Destroy(q.gameObject);
    }

    private static bool EnumTheWindows(IntPtr _hWnd, IntPtr _lParam)
    {
        var size = GetWindowTextLength(_hWnd);
        if (size++ <= 0 || !IsWindowVisible(_hWnd)) return true;

        Instance.NewWindows.Add(_hWnd);
        if (Instance.OpenWindows.Contains(_hWnd))
        {
            int currentIndex = Instance.OpenWindows.FindIndex(e => e == _hWnd);
            IntPtr temp = Instance.OpenWindows[currentIndex];
            Instance.OpenWindows.RemoveAt(currentIndex);
            Instance.OpenWindows.Insert(0, temp);
        }
        else
        {
            SpawnWindowQuad(_hWnd);
        }
        return true;
    }
}