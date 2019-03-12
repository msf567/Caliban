using System;
using System.Media;
using System.Net.Configuration;
using Caliban.Core.Audio;
using Caliban.Core.Utility;

namespace Caliban.Core.Game
{
    public class SoundClue : Clue
    {
        public SoundClue(string _clueLocation) : base(_clueLocation)
        {
            AudioManager.LoadFile("demoClue.wav", "DemoClue");
           // AudioManager.LoadFile(Treasures.Treasures.GetStream("demoClue.wav"), "DemoClue");
            AudioManager.SetVolume("DemoClue", 0.0f);
            AudioManager.PlaySound("DemoClue", true);
        }

        public override void Dispose()
        {
            AudioManager.StopSound("DemoClue");
        }

        public void FolderNav(string _folder)
        {
            if (clueSteps.Contains(_folder))
            {
                int level = (clueSteps.Count - clueSteps.FindIndex(a => a == _folder));

                float soundLevel = 0;
                if (level <= 4)
                {
                    soundLevel = (3 - Math.Abs(level)) + 1;
                }
                
                AudioManager.SetVolume("DemoClue",( soundLevel / 3.0f) / 4.0f);
                AudioManager.SetVolume("IntroMusic", 1 - ( soundLevel / 3.0f) );
            }

            //D.Write("Clue folder nav to " + _folder + " against " + clueLocation.FullName);
        }
    }
}