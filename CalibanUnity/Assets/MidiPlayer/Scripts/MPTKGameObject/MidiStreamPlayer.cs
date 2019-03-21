//#define DEBUGPERF
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
    /// Play generated notes. 
    /// Any Midi file is necessary rather create music from your own algorithm with MPTK_PlayEvent().
    /// Duration can be set in the MPTKEvent, but a note can also be stopped with MPTK_StopEvent().
    /// </summary>
    public class MidiStreamPlayer : MidiPlayer
    {
        new void Awake()
        {
            base.Awake();
        }

        new void Start()
        {
            try
            {
                MPTK_InitSynth();
                base.Start();
                // Always enabled for midi stream
                MPTK_EnablePresetDrum = true;
                ThreadDestroyAllVoice();
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        /// <summary>
        /// Play one midi event with a thread so the call return immediately.
        ///! @snippet MusicView.cs Example PlayNote
        /// </summary>
        public virtual void MPTK_PlayEvent(MPTKEvent note)
        {
            try
            {
                if (MidiPlayerGlobal.MPTK_SoundFontLoaded)
                {
                    Timing.RunCoroutine(TheadPlay(note));
                }
                else
                    Debug.LogWarningFormat("SoundFont not yet loaded, Midi Event cannot be processed Code:{0} Channel:{1}", note.Command, note.Channel);
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        /// <summary>
        /// Play a list of midi events with a thread so the call return immediately.
        /// @snippet TestMidiStream.cs Example MPTK_PlayNotes
        /// </summary>
        public virtual void MPTK_PlayEvent(List<MPTKEvent> notes)
        {
            try
            {
                if (MidiPlayerGlobal.MPTK_SoundFontLoaded)
                {
                    Timing.RunCoroutine(TheadPlay(notes));
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        private IEnumerator<float> TheadPlay(MPTKEvent note)
        {
            if (note != null)
            {
                try
                {
                    if (!MPTK_PauseOnDistance || MidiPlayerGlobal.MPTK_DistanceToListener(this.transform) <= VoiceTemplate.Audiosource.maxDistance)
                    {
#if DEBUGPERF
                        DebugPerf("-----> Init perf:", 0);
#endif
                        PlayEvent(note);
#if DEBUGPERF
                        DebugPerf("<---- ClosePerf perf:", 2);
#endif
                    }
                }
                catch (System.Exception ex)
                {
                    MidiPlayerGlobal.ErrorDetail(ex);
                }
            }
            yield return 0;
        }

        private IEnumerator<float> TheadPlay(List<MPTKEvent> notes)
        {
            if (notes != null && notes.Count > 0)
            {
                try
                {
                    try
                    {
                        if (!MPTK_PauseOnDistance || MidiPlayerGlobal.MPTK_DistanceToListener(this.transform) <= VoiceTemplate.Audiosource.maxDistance)
                        {
                            PlayEvents(notes);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        MidiPlayerGlobal.ErrorDetail(ex);
                    }
                }
                catch (System.Exception ex)
                {
                    MidiPlayerGlobal.ErrorDetail(ex);
                }
            }
            yield return 0;

        }

        /// <summary>
        /// Stop playing the note. All waves associated to the note are stop by sending a noteoff.
        /// </summary>
        /// <param name="pnote"></param>
        public virtual void MPTK_StopEvent(MPTKEvent pnote)
        {
            try
            {
                if (pnote != null && pnote.Voices != null)
                {
                    foreach (fluid_voice note in pnote.Voices)
                        if (note.volenv_section != fluid_voice_envelope_index.FLUID_VOICE_ENVRELEASE &&
                            note.status != fluid_voice_status.FLUID_VOICE_OFF)
                            note.fluid_voice_noteoff();
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }
    }
}

