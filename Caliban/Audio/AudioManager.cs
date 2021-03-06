using System;
using System.Collections.Generic;
using Caliban.Core.Utility;
using NAudio.Wave;

namespace Caliban.Core.Audio
{
    public static class AudioManager
    {
        private static DirectSoundOut output = new DirectSoundOut();
        private static MixingWaveProvider32 mixer;
        private static Dictionary<string, WavePlayer> Sounds = new Dictionary<string, WavePlayer>();
     
        static AudioManager()
        {
            mixer = new MixingWaveProvider32();
            output.Init(mixer);
            output.Play();
        }

        public static void LoadFile(string _filename, string _soundName)
        {
            if (Sounds.ContainsKey(_soundName))
                return;
            try
            {
                Sounds.Add(_soundName, new WavePlayer(_filename));
            }
            catch (Exception e)
            {
                D.Write(e.Message);
            }
        }

        public static void SetVolume(string _soundName, float _vol)
        {
            if (Sounds.ContainsKey(_soundName))
            {
                Sounds[_soundName].Channel.Volume = _vol;
            } 
        }

        public static void PlaySound(string _soundName, bool _looping)
        {
            if (Sounds.ContainsKey(_soundName))
            {
                Sounds[_soundName].LoopStream.EnableLooping = _looping;
                mixer.AddInputStream(Sounds[_soundName].Channel);               
                output.Play();
            }
        }

        public static void StopSound(string _soundName)
        {
            if (Sounds.ContainsKey(_soundName))
                mixer.RemoveInputStream(Sounds[_soundName].Channel);
        }
        
        /*
        public static void LoadFile(Stream _stream, string _soundName)
        {
            if (Sounds.ContainsKey(_soundName))
                return;
            try
            {
                Sounds.Add(_soundName, new WavePlayer(_stream));
            }
            catch (Exception e)
            {
                D.Write(e.Message);
            }
        }
        */
    }
}
