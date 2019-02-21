using System;
using System.Collections.Generic;
using System.Media;

namespace Caliban.Core.Audio
{
    public static class AudioPlayer
    {
        private static Dictionary<string, SoundPlayer> SoundPlayers = new Dictionary<string, SoundPlayer>();

        public static void LoadFile(string _filename, string _soundName)
        {
            if (SoundPlayers.ContainsKey(_soundName))
                return;
            SoundPlayer s = new SoundPlayer();
            s.SoundLocation = _filename;
            try
            {
                s.Load();
                SoundPlayers.Add(_soundName, s);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static void PlaySound(string _soundName, bool _looping = false)
        {
            if (SoundPlayers.ContainsKey(_soundName))
                if (_looping)
                    SoundPlayers[_soundName].PlayLooping();
                else
                    SoundPlayers[_soundName].Play();
        }

        public static void StopSound(string _soundName)
        {
            if(SoundPlayers.ContainsKey(_soundName))
                SoundPlayers[_soundName].Stop();
        }
    }
}