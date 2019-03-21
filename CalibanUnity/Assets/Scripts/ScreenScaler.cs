using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Windows.Forms;
using System.Drawing;
public class ScreenScaler : MonoBehaviour
{
    public Vector2 OverrideRes;
    public bool Override = false;
    void Start()
    {
        Rectangle resolution = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
        int w = resolution.Width;
        int h = resolution.Height;
        if (!Override)
            UnityEngine.Screen.SetResolution(w - 1, h - 1, true);
        else
            UnityEngine.Screen.SetResolution((int)OverrideRes.x, (int)OverrideRes.y, false);
    }

    void Update()
    {

    }
}

