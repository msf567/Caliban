using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows;
using System.Drawing;
using Caliban.Unity;
using UnityEngine.Serialization;
using Screen = UnityEngine.Screen;

public class WindowQuad : MonoBehaviour
{
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    internal static extern IntPtr GetForegroundWindow();


    [DllImport("user32.dll")]
    static extern bool ClientToScreen(IntPtr hWnd, ref System.Drawing.Point lpPoint);


    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left; // x position of upper-left corner
        public int Top; // y position of upper-left corner
        public int Right; // x position of lower-right corner
        public int Bottom; // y position of lower-right corner
    }

    public Renderer FrameMat;
    public IntPtr Window = IntPtr.Zero;

    void Start()
    {
//        FrameMat = GameObject.Find("FRAME1").GetComponent<Renderer>();

        GlobalInput.OnKeyPress += OnGlobalKeyDown;
        GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.height / 2.0f,Screen.height / 2.0f);
    }

    Texture2D GetTextureFromScreen(Rect r)
    {
        int x = Mathf.FloorToInt(r.x);
        int y = Mathf.FloorToInt(r.y);
        int width = Mathf.FloorToInt(r.width);
        int height = Mathf.FloorToInt(r.height);

        Texture mainTexture = FrameMat.material.mainTexture;
        Texture2D srcTex = new Texture2D(mainTexture.width, mainTexture.height, TextureFormat.RGBA32, false);

        RenderTexture currentRT = RenderTexture.active;

        RenderTexture renderTexture = new RenderTexture(mainTexture.width, mainTexture.height, 32);
        UnityEngine.Graphics.Blit(mainTexture, renderTexture);

        RenderTexture.active = renderTexture;
        srcTex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        srcTex.Apply();
        RenderTexture.active = currentRT;

        UnityEngine.Color[] pix = srcTex.GetPixels(x, y, width, height);
        Texture2D destTex = new Texture2D(width, height);
        destTex.SetPixels(pix);
        destTex.Apply();

        Destroy(srcTex);
        Destroy(renderTexture);
        return destTex;
    }

    private void OnGlobalKeyDown(Keys k)
    {
        if (k == Keys.R)
        {
            Window = GetForegroundWindow();   
        }
    }

    void Update()
    {
        if (Window != IntPtr.Zero)
        {
            StickToWindow(Window);
        }
    }

    public Rect BoundsToScreenRect(Bounds bounds)
    {
        // Get mesh origin and farthest extent (this works best with simple convex meshes)
        Vector3 origin = Camera.main.WorldToScreenPoint(new Vector3(bounds.min.x, bounds.max.y, 0f));
        Vector3 extent = Camera.main.WorldToScreenPoint(new Vector3(bounds.max.x, bounds.min.y, 0f));

        // Create rect in screen space and return - does not account for camera perspective
        return new Rect(origin.x, UnityEngine.Screen.height - origin.y, extent.x - origin.x, origin.y - extent.y);
    }


    public void StickToWindow(IntPtr hwnd)
    {
        RECT windowRect = new RECT();

        if (hwnd == null)
            return;
        if (!GetClientRect(hwnd, out windowRect))
        {
            Debug.LogError("Could ont grab Window Rect!");
            return;
        }

        System.Drawing.Point LEFTTOP = new System.Drawing.Point(windowRect.Left, windowRect.Top);
        System.Drawing.Point RIGHTBTTM = new System.Drawing.Point(windowRect.Right, windowRect.Bottom);
        
        
        ClientToScreen(hwnd, ref LEFTTOP); // convert top-left
        ClientToScreen(hwnd, ref RIGHTBTTM); // convert bottom-right

        
        Vector3 WorldTopLeft = Camera.main.ScreenToWorldPoint(new Vector3(LEFTTOP.X, LEFTTOP.Y, 1));
        Vector3 WorldTopRight = Camera.main.ScreenToWorldPoint(new Vector3(RIGHTBTTM.X, LEFTTOP.Y, 1));
        Vector3 WorldBottomLeft = Camera.main.ScreenToWorldPoint(new Vector3(LEFTTOP.X, RIGHTBTTM.Y, 1));
        Vector3 WorldBottomRight = Camera.main.ScreenToWorldPoint(new Vector3(RIGHTBTTM.X, RIGHTBTTM.Y, 1));
        
        float newWidth = Vector3.Distance(WorldTopLeft, WorldTopRight);
        float newHeight = Vector3.Distance(WorldTopLeft, WorldBottomLeft);
        transform.localScale = new Vector3(newWidth, newHeight, 1) ;

        Vector3 newPos =
            Camera.main.ScreenToWorldPoint(new Vector3(LEFTTOP.X, UnityEngine.Screen.height - LEFTTOP.Y, 1));
        //newPos += new Vector3(transform.localScale.x, -transform.localScale.y);
        transform.position = new Vector3( newPos.x,newPos.y,transform.position.z);
    }
}