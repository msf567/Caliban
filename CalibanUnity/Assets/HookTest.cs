using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using UnityEngine;

public class HookTest : MonoBehaviour
{
    public Renderer ren;

    void Start()
    {
    }

    private void OnMouseClick()
    {
        ren.material.color = new Color(Random.value, Random.value, Random.value);
    }
    

    private void OnApplicationQuit()
    {
    }
}