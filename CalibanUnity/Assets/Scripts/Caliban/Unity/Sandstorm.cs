using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using Application = UnityEngine.Application;
using Cursor = System.Windows.Forms.Cursor;
using Screen = UnityEngine.Screen;

namespace Caliban.Unity
{
    public class Sandstorm : MonoBehaviour
    {
        public RawImage sandstormRenderImage;
        public Camera sandstormCamera;
        private RenderTexture sandstormDrawTexture;
        public int SandstormTextureScale = 4;
        public AudioMixer mixer;
        public float MaxParticles = 500;
        public Vector2 Direction;

        public float MaxStrength = 1;

        public Transform windTrans;
        public ParticleSystem particles;
        private ParticleSystem.EmissionModule emission;
        [SerializeField] private float Strength;
        private static float TargetStrength;
        private float buildUp;
        private static float HoldOn;

        public static bool StartFlag = false;

        private void Awake()
        {
            sandstormDrawTexture = new RenderTexture(Screen.width / SandstormTextureScale, Screen.height / SandstormTextureScale, 0);
            //sandstormDrawTexture.format = RenderTextureFormat.RGBAUShort;
            sandstormDrawTexture.filterMode = FilterMode.Point;
            sandstormDrawTexture.useMipMap = false;
            sandstormDrawTexture.wrapMode = TextureWrapMode.Clamp;
            sandstormDrawTexture.anisoLevel = 0;
            sandstormDrawTexture.autoGenerateMips = false;
            sandstormDrawTexture.name = "SandstormDrawTexture";
            sandstormDrawTexture.Create();
            sandstormRenderImage.texture = sandstormDrawTexture;
            sandstormCamera.targetTexture = sandstormDrawTexture;
        }

        public void Start()
        {
            Debug.Log("Started");
            DCon.Log("assigning delegate");

            emission = particles.emission;
        }

        private void OnDestroy()
        {
        }

        public static void GlobalMouseDown()
        {
            //DCon.Log("click!");
            HoldOn = 0.25f;
        }

        private void StartSandstorm()
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

            if (StartFlag)
            {
                StartSandstorm();
                StartFlag = false;
            }

            if (HoldOn > 0)
            {
                HoldOn -= Time.deltaTime;
                return;
            }

            float TargetDirection = Mathf.Sign(Mathf.PerlinNoise(Time.time / 20.0f, 6) - 0.5f);
            Direction.x += (TargetDirection - Direction.x) * 0.001f;
            float windRot = Remap(Mathf.Sign(Direction.x), -1, 1, 90, 270);
            Mathf.Clamp(windRot, 90, 270);

            windTrans.eulerAngles = new Vector3(0, windRot, 0);
            Strength *= Mathf.Abs(TargetDirection);

            buildUp += (Strength * MaxStrength);
            while (buildUp > 1)
            {
                buildUp -= 1;
                if (!Application.isEditor)
                    Cursor.Position = new Point(Cursor.Position.X + -(int)Mathf.Sign(Direction.x), Cursor.Position.Y);
            }

            float dif = (TargetStrength - Strength) * 0.0005f;
            Strength += dif;
            emission = particles.emission;
            emission.rateOverTime = new ParticleSystem.MinMaxCurve(Strength * MaxParticles);
        }

        public float Remap(float from, float fromMin, float fromMax, float toMin, float toMax)
        {
            var fromAbs = from - fromMin;
            var fromMaxAbs = fromMax - fromMin;

            var normal = fromAbs / fromMaxAbs;

            var toMaxAbs = toMax - toMin;
            var toAbs = toMaxAbs * normal;

            var to = toAbs + toMin;

            return to;
        }
    }
}