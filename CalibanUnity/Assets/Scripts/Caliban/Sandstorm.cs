﻿using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using GlobalHook;
using UnityEngine;
using UnityEngine.Audio;
using Application = UnityEngine.Application;
using Cursor = System.Windows.Forms.Cursor;

public class Sandstorm : MonoSingleton<Sandstorm>
{
    public AudioMixer mixer;
    public float MaxParticles = 500;
    public Vector2 Direction;

    public float MaxStrength = 1;

    public Transform windTrans;
    public ParticleSystem particles;
    private ParticleSystem.EmissionModule emission;
    [SerializeField]
    private float Strength;
    private float TargetStrength;
    private float buildUp;
    private float HoldOn;

    public override void Init()
    {
        HookManager.MouseDown += HookManagerOnMouseDown;
        emission = particles.emission;
    }

    private void OnDestroy()
    {
        HookManager.MouseDown -= HookManagerOnMouseDown;
    }

    private void HookManagerOnMouseDown(object _sender, MouseEventArgs _e)
    {
        HoldOn = 0.25f;
    }

    public void StartSandstorm()
    {
        if (TargetStrength <= 0)
            StartCoroutine(SandstormAudioFadeIn());
    }

    private IEnumerator SandstormAudioFadeIn()
    {
        while (TargetStrength < 1)
        {
            yield return new WaitForSeconds(3);
            TargetStrength += 0.2f;
        }

        mixer.FindSnapshot("Sandstorm").TransitionTo(15);
        yield return new WaitForSeconds(15);
        mixer.FindSnapshot("SandstormFull").TransitionTo(30);
    }

    public void StopSandstorm()
    {
    }

    void Update()
    {
        if (Input.GetKeyDown("q") && Application.isEditor)
            StartSandstorm();

        if (HoldOn > 0)
        {
            HoldOn -= Time.deltaTime;
            return;
        }

        float TargetDirection = Mathf.Sign(Mathf.PerlinNoise(Time.time / 20.0f, 6) - 0.5f);
        Direction.x += (TargetDirection - Direction.x) * 0.001f;
        float windRot = Remap(Mathf.Sign(Direction.x), -1, 1, 90, 270);
        Mathf.Clamp(windRot, 90, 270);
        
        windTrans.eulerAngles = new Vector3(0,windRot,0);
        Strength *= Mathf.Abs(TargetDirection);

        buildUp += (Strength * MaxStrength);
        while (buildUp > 1)
        {
            buildUp -= 1;
            Cursor.Position = new Point(Cursor.Position.X + -(int)Mathf.Sign(Direction.x), Cursor.Position.Y);
        }

        float dif = (TargetStrength - Strength) * 0.0005f;
        Strength += dif;
        emission = particles.emission;
        emission.rateOverTime = new ParticleSystem.MinMaxCurve(Strength * MaxParticles);
    }
    
    public float Remap (float from, float fromMin, float fromMax, float toMin,  float toMax)
    {
        var fromAbs  =  from - fromMin;
        var fromMaxAbs = fromMax - fromMin;      
       
        var normal = fromAbs / fromMaxAbs;
 
        var toMaxAbs = toMax - toMin;
        var toAbs = toMaxAbs * normal;
 
        var to = toAbs + toMin;
       
        return to;
    }
}