using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MidiPlayerTK;
using System;

namespace MidiPlayerTK
{
    public class TestMidiExternalPlayer : MonoBehaviour
    {
        /// <summary>
        /// MPTK component able to play a Midi file from an external source. This PreFab must be present in your scene.
        /// </summary>
        public MidiExternalPlayer midiExternalPlayer;

        private void Start()
        {
            if (!HelperDemo.CheckSFExists()) return;

            // Warning: when defined by script, this event is not triggered at first load of MPTK 
            // because MidiPlayerGlobal is loaded before any other gamecomponent
            if (!MidiPlayerGlobal.OnEventPresetLoaded.HasEvent())
            {
                // To be done in Start event (not Awake)
                MidiPlayerGlobal.OnEventPresetLoaded.AddListener(EndLoadingSF);
            }

            // Find the Midi external component 
            if (midiExternalPlayer == null)
            {
                Debug.Log("No midiExternalPlayer defined with the editor inspector, try to find one");
                MidiExternalPlayer fp = FindObjectOfType<MidiExternalPlayer>();
                if (fp == null)
                    Debug.Log("Can't find a MidiExternalPlayer in the Hierarchy. No music will be played");
                else
                {
                    midiExternalPlayer = fp;
                }
            }

            if (midiExternalPlayer != null)
            {
                // There is two methods to trigger event: 
                //      1) in inpector from the Unity editor 
                //      2) by script, see below
                // ------------------------------------------

                // Event trigger when midi file start playing
                if (!midiExternalPlayer.OnEventStartPlayMidi.HasEvent())
                {
                    // Set event by script
                    Debug.Log("OnEventStartPlayMidi defined by script");
                    midiExternalPlayer.OnEventStartPlayMidi.AddListener(StartPlay);
                }
                else
                    Debug.Log("OnEventStartPlayMidi defined by Unity editor");

                // Event trigger when midi file end playing
                if (!midiExternalPlayer.OnEventEndPlayMidi.HasEvent())
                {
                    // Set event by script
                    Debug.Log("OnEventEndPlayMidi defined by script");
                    midiExternalPlayer.OnEventEndPlayMidi.AddListener(EndPlay);
                }
                else
                    Debug.Log("OnEventStartPlayMidi defined by Unity editor");

                // Event trigger for each group of notes read from midi file
                if (!midiExternalPlayer.OnEventNotesMidi.HasEvent())
                {
                    // Set event by script
                    Debug.Log("OnEventNotesMidi defined by script");
                    midiExternalPlayer.OnEventNotesMidi.AddListener(ReadNotes);
                }
                else
                    Debug.Log("OnEventNotesMidi defined by Unity editor");
            }
        }

        /// <summary>
        /// This call can be defined from MidiPlayerGlobal event inspector. Run when SF is loaded.
        /// Warning: not triggered at first load of MPTK because MidiPlayerGlobal id load before any other gamecomponent
        /// </summary>
        public void EndLoadingSF()
        {
            Debug.Log("End loading SF, MPTK is ready to play");
            Debug.Log("Load statistique");
            Debug.Log("   Time To Load SoundFont: " + Math.Round(MidiPlayerGlobal.MPTK_TimeToLoadSoundFont.TotalSeconds, 3).ToString() + " second");
            Debug.Log("   Time To Load Waves: " + Math.Round(MidiPlayerGlobal.MPTK_TimeToLoadWave.TotalSeconds, 3).ToString() + " second");
            Debug.Log("   Presets Loaded: " + MidiPlayerGlobal.MPTK_CountPresetLoaded);
            Debug.Log("   Waves Loaded: " + MidiPlayerGlobal.MPTK_CountWaveLoaded);
        }

        /// <summary>
        /// Event fired by MidiExternalPlayer when a midi is started (set by Unity Editor in MidiFilePlayer Inspector)
        /// </summary>
        public void StartPlay(string midiname)
        {
            Debug.Log("Start Midi " + midiname);
            midiExternalPlayer.MPTK_Speed = 1f;
            midiExternalPlayer.MPTK_Transpose = 0;
        }

        /// <summary>
        /// Event fired by MidiExternalPlayer when a midi is ended when reach end or stop by MPTK_Stop or Replay with MPTK_Replay
        /// The parameter reason give the origin of the end
        /// </summary>
        public void EndPlay(string midiname, EventEndMidiEnum reason)
        {
            Debug.LogFormat("End playing midi {0} reason:{1}", midiname, reason);
        }

        /// <summary>
        /// Event fired by MidiFilePlayer when a midi notes are available (set by Unity Editor in MidiFilePlayer Inspector)
        /// </summary>
        public void ReadNotes(List<MPTKEvent> notes)
        {
            //Debug.Log("Notes : " + notes.Count);
        }

        /// <summary>
        /// Event fired by MidiFilePlayer when a midi is ended (set by Unity Editor in MidiFilePlayer Inspector)
        /// </summary>
        public void EndPlay()
        {
            Debug.Log("End Midi " + midiExternalPlayer.MPTK_MidiName);
        }

        public void GotoWeb(string uri)
        {
            Application.OpenURL(uri);
        }

        /// <summary>
        /// This method is fired from button (with predefined URI) or inputfield in the screen.
        /// See canvas/button.
        /// </summary>
        /// <param name="uri">uri or path to the midi file</param>
        public void Play(string uri)
        {
            Debug.Log("Play from script:" + uri);
            midiExternalPlayer.MPTK_Stop();
            midiExternalPlayer.MPTK_MidiName = uri;
            midiExternalPlayer.MPTK_Play();
        }
    }
}