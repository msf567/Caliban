
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using System;
using UnityEngine.Events;

namespace MidiPlayerTK
{
    /// <summary>
    /// PRO Version -  Script for the prefab MidiListPlayer. 
    /// Play a list of pre-selected midi file from the dedicated inspector.
    /// List of Midi files must exists in MidiDB. See Midi Player Setup (Unity menu MPTK).
    /// </summary>
    public class MidiListPlayer : MonoBehaviour
    {
        /// <summary>
        /// Define a midi to be added in the list
        /// </summary>
        [Serializable]
        public class MPTK_MidiPlayItem
        {
            /// <summary>
            /// Midi Name. Use the exact name defined in Unity resources folder MidiDB without any path or extension.
            /// Tips: Add Midi files to your project with the Unity menu MPTK or add it directly in the ressource folder and open Midi File Setup to automatically integrate Midi in MPTK.
            /// </summary>
            public string MidiName;
            /// <summary>
            /// Select or unselect this Midi in the Inspector to apply actions (reorder, delete, ...)
            /// </summary>
            public bool Selected;
            /// <summary>
            /// Position of the Midi in the list. Use method MPTK_ReIndexMidi() recalculate the index.
            /// </summary>
            public int Index;
        }

        /// <summary>
        /// Play list
        /// </summary>
        public List<MPTK_MidiPlayItem> MPTK_PlayList;

        /// <summary>
        /// Play a specific Midi in the list.
        /// </summary>
        public int MPTK_PlayIndex
        {
            get { return playIndex; }
            set
            {
                if (MPTK_MidiFilePlayer_1 != null)
                {
                    if (MPTK_PlayList == null || MPTK_PlayList.Count == 0)
                        Debug.LogWarning("No Play List defined");
                    else if (value < 0 || value >= MPTK_PlayList.Count)
                        Debug.LogWarning("Index to play " + value + " not correct");
                    else
                    {
                        playIndex = value;
                        //Debug.Log("PlayIndex, Index to play " + index + " "+ MPTK_PlayList[index].MidiName);
                        MPTK_MidiFilePlayer_1.MPTK_MidiName = MPTK_PlayList[playIndex].MidiName;
                        MPTK_MidiFilePlayer_1.MPTK_RePlay();
                    }
                }
            }
        }

        /// <summary>
        /// Should the Midi start playing when application start ?
        /// </summary>
        public virtual bool MPTK_PlayOnStart { get { return playOnStart; } set { playOnStart = value; } }

        /// <summary>
        /// Should automatically restart when Midi reach the end ?
        /// </summary>
        public virtual bool MPTK_Loop { get { return loop; } set { loop = value; } }


        /// <summary>
        /// Is Midi file playing is paused ?
        /// </summary>
        public virtual bool MPTK_IsPaused { get { if (MPTK_MidiFilePlayer_1 != null) return MPTK_MidiFilePlayer_1.MPTK_IsPaused; else return false; } }

        /// <summary>
        /// Is Midi file is playing ?
        /// </summary>
        public virtual bool MPTK_IsPlaying { get { if (MPTK_MidiFilePlayer_1 != null) return MPTK_MidiFilePlayer_1.MPTK_IsPlaying; else return false; } }

        /// <summary>
        /// Define unity event to trigger at start
        /// </summary>
        [HideInInspector]
        public UnityEvent OnEventStartPlayMidi;

        /// <summary>
        /// Define unity event to trigger at end
        /// </summary>
        [HideInInspector]
        public UnityEvent OnEventEndPlayMidi;

        /// <summary>
        /// MidiFilePlayer to play the Midi
        /// </summary>
        public MidiFilePlayer MPTK_MidiFilePlayer_1;


        [SerializeField]
        [HideInInspector]
        private bool playOnStart = false, loop = false;

        [SerializeField]
        [HideInInspector]
        private int playIndex;

        void Awake()
        {
            //Debug.Log("Awake midiIsPlaying:" + MPTK_IsPlaying);
            MPTK_MidiFilePlayer_1 = GetComponentInChildren<MidiFilePlayer>();
            if (MPTK_MidiFilePlayer_1 == null)
                Debug.LogWarning("No MidiFilePlayer component found in MidiListPlayer.");
            else
            {
                if (!MPTK_MidiFilePlayer_1.OnEventNotesMidi.HasEvent())
                {
                    // No listener defined, set now by script. NotesToPlay will be called for each new notes read from Midi file
                    //Debug.Log("No OnEventNotesMidi defined, set by script");
                    MPTK_MidiFilePlayer_1.OnEventEndPlayMidi.AddListener(EventEndPlayMidi);
                }
            }
        }

        public void EventEndPlayMidi(string midiname, EventEndMidiEnum reason)
        {
            //Debug.LogFormat("End playing midi. Reason:{0}", reason);
            if (reason == EventEndMidiEnum.MidiEnd)
            {
                if (MPTK_PlayIndex < MPTK_PlayList.Count - 1)
                {
                    MPTK_PlayIndex++;
                }
                else if (MPTK_Loop)
                {
                    MPTK_PlayIndex = 0;
                }
            }
        }

        void Start()
        {
            try
            {
                if (MPTK_PlayOnStart)
                {
                    // Find first enabled
                    foreach (MPTK_MidiPlayItem item in MPTK_PlayList)
                        if (item.Selected)
                        {
                            MPTK_PlayIndex = item.Index;
                            break;
                        }
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        /// <summary>
        /// Add a Midi name to the list. Use the exact name defined in Unity resources folder MidiDB without any path or extension.
        /// Tips: Add Midi files to your project with the Unity menu MPTK or add it directly in the ressource folder and open Midi File Setup to automatically integrate Midi in MPTK.
        //! @code
        //! midiListPlayer.MPTK_AddMidi("Albinoni - Adagio");
        //! @endcode
        /// </summary>
        public virtual void MPTK_AddMidi(string name)
        {
            MPTK_PlayList.Add(new MPTK_MidiPlayItem() { MidiName = name, Selected = true, Index = MPTK_PlayList.Count });
        }

        /// <summary>
        /// Remove a Midi name from the list. Use the exact name defined in Unity resources folder MidiDB without any path or extension.
        //! @code
        //! midiListPlayer.MPTK_RemoveMidi("Albinoni - Adagio");
        //! @endcode
        public virtual void MPTK_RemoveMidi(string name)
        {
            int index = MPTK_PlayList.FindIndex(s => s.MidiName == name);
            if (index >= 0)
                MPTK_PlayList.RemoveAt(index);
            MPTK_ReIndexMidi();
        }

        /// <summary>
        /// Recalculate the index of the midi from the list.
        /// </summary>
        public virtual void MPTK_ReIndexMidi()
        {
            int index = 0;
            foreach (MPTK_MidiPlayItem item in MPTK_PlayList)
                item.Index = index++;
        }

        /// <summary>
        /// Play the midi file defined in MPTK_MidiName
        /// </summary>
        public virtual void MPTK_Play()
        {
            try
            {
                if (MidiPlayerGlobal.MPTK_SoundFontLoaded)
                {
                    // Load description of available soundfont
                    if (MidiPlayerGlobal.ImSFCurrent != null && MidiPlayerGlobal.CurrentMidiSet != null && MidiPlayerGlobal.CurrentMidiSet.MidiFiles != null && MidiPlayerGlobal.CurrentMidiSet.MidiFiles.Count > 0)
                        MPTK_PlayIndex = MPTK_PlayIndex;
                    else
                        Debug.LogWarning(MidiPlayerGlobal.ErrorNoMidiFile);
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        /// <summary>
        /// Stop playing
        /// </summary>
        public virtual void MPTK_Stop()
        {
            try
            {
                if (MPTK_MidiFilePlayer_1 != null)
                {
                    MPTK_MidiFilePlayer_1.MPTK_Stop();
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        /// <summary>
        /// Restart playing the current midi file
        /// </summary>
        public virtual void MPTK_RePlay()
        {
            try
            {
                if (MPTK_MidiFilePlayer_1 != null)
                    MPTK_MidiFilePlayer_1.MPTK_RePlay();
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }


        /// <summary>
        /// Pause the current playing
        /// </summary>
        /// <param name="timeToPauseMS">time to pause in milliseconds. default: indefinitely</param>
        public virtual void MPTK_Pause(float timeToPauseMS = -1f)
        {
            try
            {
                if (MPTK_MidiFilePlayer_1 != null)
                {
                    MPTK_MidiFilePlayer_1.MPTK_Pause();
                }

            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        /// <summary>
        /// Play next Midi in list
        /// </summary>
        public virtual void MPTK_Next()
        {
            try
            {
                if (MPTK_MidiFilePlayer_1 != null)
                {
                    if (MPTK_PlayIndex < MPTK_PlayList.Count - 1)
                        MPTK_PlayIndex++;
                    else
                        MPTK_PlayIndex = 0;
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        /// <summary>
        /// Play previous Midi in list
        /// </summary>
        public virtual void MPTK_Previous()
        {
            try
            {
                if (MPTK_MidiFilePlayer_1 != null)
                {
                    if (MPTK_PlayIndex > 0)
                        MPTK_PlayIndex--;
                    else
                        MPTK_PlayIndex = MPTK_PlayList.Count - 1;
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }
    }
}

