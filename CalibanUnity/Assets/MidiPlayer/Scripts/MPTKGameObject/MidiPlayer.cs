//#define DEBUGNOTE
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using System;
using UnityEngine.Events;
using MEC;

namespace MidiPlayerTK
{
    /// <summary>
    /// Send event to the midi synthetizer thru thread. Don't instanciate this class, use rather MidiFilePlayer or MidiStreamPlayer.
    /// </summary>
    public class MidiPlayer : MidiSynth
    {
        /// <summary>
        /// Should accept change Preset for Drum canal 10 ? 
        /// Disabled by default. Could sometimes create bad sound with midi files not really compliant with the Midi norm.
        /// </summary>
        public virtual bool MPTK_EnablePresetDrum
        {
            get { return enablePresetDrum; }
            set { enablePresetDrum = value; }
        }

        [SerializeField]
        [HideInInspector]
        private bool enablePresetDrum = false;

        new protected virtual void Awake()
        {
            //Debug.Log("Awake MidiPlayer");
            try
            {
                OnEventSynthAwake.Invoke(this.name);
                MidiPlayerGlobal.InitPath();
                base.Awake();
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        new protected virtual void Start()
        {
            //Debug.Log("Start MidiPlayer");
            try
            {
                base.Start();
                //MPTK_InitSynth();
                OnEventSynthStarted.Invoke(this.name);
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        /// <summary>
        /// Init the synthetizer. Prefabs automatically initialize the synthetizer (see events). It's not usefull to call this method.
        /// </summary>
        /// <param name="channelCount">Number of channel to create</param>
        public void MPTK_InitSynth(int channelCount = 16)
        {
            fluid_synth_init(channelCount);
        }

        /// <summary>
        /// Clear all sound
        /// </summary>
        /// <param name="destroyAudioSource">Destroy also audioSource (default:false)</param>
        //! @code
        //!  if (GUILayout.Button("Clear"))
        //!     midiStreamPlayer.MPTK_ClearAllSound(true);
        //! @endcode
        public void MPTK_ClearAllSound(bool destroyAudioSource = false)
        {
            Timing.RunCoroutine(ThreadClearAllSound(true));
        }

        public IEnumerator<float> ThreadClearAllSound(bool destroyAudioSource = false)
        {
#if DEBUGNOTE
            numberNote = -1;
#endif
            //Debug.Log("ThreadClearAllSound");
            yield return Timing.WaitUntilDone(Timing.RunCoroutine(ThreadReleaseAll()), false);

            if (destroyAudioSource)
            {
                yield return Timing.WaitUntilDone(Timing.RunCoroutine(ThreadWaitAllStop()), false);
                yield return Timing.WaitUntilDone(Timing.RunCoroutine(ThreadDestroyAllVoice()), false);
            }
            yield return 0;
        }

        /// <summary>
        /// Cut the sound gradually
        /// </summary>
        /// <returns></returns>
        private IEnumerator<float> ThreadReleaseAll()
        {
            for (int i = 0; i < Voices.Count; i++)
            {
                //Debug.Log("ReleaseAll " + i);
                if (Voices[i].Audiosource.isPlaying)
                    yield return Timing.WaitUntilDone(Timing.RunCoroutine(Voices[i].Release()));
            }
        }

        /// <summary>
        /// Wait all audio source not playing with time out of 2 seconds
        /// </summary>
        /// <returns></returns>
        private IEnumerator<float> ThreadWaitAllStop()
        {
            //Debug.Log("WaitAllStop");
            int countplaying = 999999;
            DateTime timeout = DateTime.Now + TimeSpan.FromSeconds(2);
            while (countplaying > 0 && timeout > DateTime.Now)
            {
                countplaying = 0;
                foreach (fluid_voice audio in Voices)
                    if (audio.Audiosource.isPlaying)
                    {
                        countplaying++;
                        audio.Audiosource.Stop();
                    }
                //Debug.Log("   " + countplaying + " ");
            }
            yield return 0;
        }

        //! @cond NODOC
        /// Remove AudioSource not playing
        /// </summary>
        protected IEnumerator<float> ThreadDestroyAllVoice()
        {
            try
            {
                fluid_voice[] voicesList = GetComponentsInChildren<fluid_voice>();
                //Debug.LogFormat("DestroyAllVoice {0}", (voicesList != null ? voicesList.Length.ToString() : "no voice found"));

                if (voicesList != null)
                {
                    foreach (fluid_voice voice in voicesList)
                        try
                        {
                            // Debug.Log("Destroy " + voice.IdVoice + " " + (voice.Audiosource.clip != null ? voice.Audiosource.clip.name : "no clip"));
                            // Don't delete audio source template
                            if (voice.name.StartsWith("VoiceId_"))
                                Destroy(voice.gameObject);
                        }
                        catch (System.Exception ex)
                        {
                            MidiPlayerGlobal.ErrorDetail(ex);
                        }
                    Voices.Clear();
                }
                //audiosources = new List<AudioSource>();
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
            yield return 0;

        }

        /// <summary>
        /// Play a list of Midi events 
        /// </summary>
        /// <param name="midievents">List of Midi events to play</param>
        protected void PlayEvents(List<MPTKEvent> midievents)
        {
            if (MidiPlayerGlobal.MPTK_SoundFontLoaded == false)
                return;

            if (midievents != null && midievents.Count < 100)
            {
                foreach (MPTKEvent note in midievents)
                {
#if DEBUGPERF
                    DebugPerf("-----> Init perf:", 0);
#endif
                    //float beforePLay = Time.realtimeSinceStartup;
                    PlayEvent(note);
                    //Debug.Log("Elapsed:" + (Time.realtimeSinceStartup - beforePLay) * 1000f);
#if DEBUGPERF
                    DebugPerf("<---- ClosePerf perf:",2);
#endif
                }
            }
        }
#if DEBUGNOTE
        public int numberNote = -1;
        public int startNote;
        public int countNote;
#endif
        /// <summary>
        /// Play one Midi event
        /// @snippet MusicView.cs Example PlayNote
        /// </summary>
        /// <param name="midievent"></param>
        protected void PlayEvent(MPTKEvent midievent)
        {
            try
            {
                if (MidiPlayerGlobal.CurrentMidiSet == null || MidiPlayerGlobal.CurrentMidiSet.ActiveSounFontInfo == null)
                {
                    Debug.Log("No SoundFont selected for MPTK_PlayNote ");
                    return;
                }

                switch (midievent.Command)
                {
                    case MPTKCommand.NoteOn:
                        if (midievent.Velocity != 0)
                        {
#if DEBUGNOTE
                            numberNote++;
                            if (numberNote < startNote || numberNote > startNote + countNote - 1) return;
#endif
                            //if (note.Channel==4)
                            synth_noteon(midievent);
                        }
                        break;

                    case MPTKCommand.ControlChange:
                        if (MPTK_ApplyRealTimeModulator)
                            Channels[midievent.Channel].fluid_channel_cc(midievent.Controller, midievent.Value); // replace of fluid_synth_cc(note.Channel, note.Controller, (int)note.Value);
                        break;

                    case MPTKCommand.PatchChange:
                        if (midievent.Channel != 9 || MPTK_EnablePresetDrum == true)
                            fluid_synth_program_change(midievent.Channel, midievent.Value);
                        break;
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }
        //! @endcond

    }
}

