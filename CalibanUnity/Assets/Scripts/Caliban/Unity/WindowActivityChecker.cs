using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WindowActivityChecker : MonoBehaviour
{
    private float alpha = 0;
    public float speed = 1;
    public float decayTime = 5;
    private float timeSinceLastMoved;
    private Vector3 lastPos;
    private RawImage img;
    private bool fadingIn;
    private Material mat;
    void Start()
    {
        img = GetComponent<RawImage>();
        Material m = Instantiate(GetComponent<RawImage>().material);
        GetComponent<RawImage>().material = m;
        mat = GetComponent<RawImage>().material;
        img.color = new Color(Random.value, Random.value, Random.value);
    }

    void Update()
    {
        if (lastPos == transform.position)
        {
            timeSinceLastMoved += Time.deltaTime;
            if (timeSinceLastMoved > decayTime && !fadingIn)
            {
                StartCoroutine(FadeInColor());
            }
        }
        else
        {
            WakeUp();
        }

        img.color = new Color(img.color.r, img.color.g, img.color.b, alpha);
        lastPos = transform.position;        
        mat.SetColor("_Color",  img.color );
    }

    private IEnumerator FadeInColor()
    {
        fadingIn = true;
        alpha = 0;
        while (alpha < 1)
        {
            alpha += 0.001f;
            yield return new WaitForSeconds(0.01f);
        }
    }

    void WakeUp()
    {
        fadingIn = false;

        StopAllCoroutines();
        alpha = 0;
    }
}