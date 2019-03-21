using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.Audio;
using Cursor = System.Windows.Forms.Cursor;

public class Standstorm : MonoBehaviour
{
    public AudioMixer mixer;
    private AudioMixerSnapshot stormSnapshot;
    private float Strength = 0;
    private float TargetStrength = 0;
    private float buildUp = 0;

    private void Awake()
    {
        
    }

    void Start()
    {
    }

    public void StartSandstorm()
    {
        StartCoroutine(SandstormAudioFadeIn());
    }

    private IEnumerator SandstormAudioFadeIn()
    {
        TargetStrength = 6;
        mixer.FindSnapshot("Sandstorm").TransitionTo(15);
        yield return new WaitForSeconds(15);
        mixer.FindSnapshot("SandstormFull").TransitionTo(30);
    }

    public void StopSandstorm()
    {
    }

    void Update()
    {
        if (Input.GetKeyDown("q"))
            StartSandstorm();

        buildUp += Strength;
        while (buildUp > 1)
        {
            buildUp -= 1;
            Cursor.Position = new Point(Cursor.Position.X + 1, Cursor.Position.Y);
        }

        float dif = (TargetStrength - Strength) * 0.0005f;
        Strength += dif;
    }
}