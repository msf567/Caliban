using System.Drawing;
using OpenTK.Mathematics;

namespace Caliban.Graphics.Utility;

public static class ColorUtility
{
    public static Vector3 ToVector3(Color c)
    {
        return new Vector3(c.R / 255.0f, c.G / 255.0f, c.B / 255.0f);
    }
}