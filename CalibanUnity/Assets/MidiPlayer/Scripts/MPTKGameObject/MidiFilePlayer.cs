
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
    /// Script for the prefab MidiFilePlayer. 
    /// Play a selected midi file. 
    /// List of Midi file must be defined with Midi Player Setup (see Unity menu MPTK).
    /// </summary>
    public class MidiFilePlayer : MidiPlayer
    {
        /// <summary>
        /// Midi name to play. Use the exact name defined in Unity resources folder MidiDB without any path or extension.
        /// Tips: Add Midi files to your project with the Unity menu MPTK or add it directly in the ressource folder and open Midi File Setup to automatically integrate Midi in MPTK.
        //! @code
        //! midiFilePlayer.MPTK_MidiName = "Albinoni - Adagio";
        //! @endcode
        /// </summary>
        public virtual string MPTK_MidiName
        {
            get
            {
                //Debug.Log("MPTK_MidiName get " + midiNameToPlay);
                return midiNameToPlay;
            }
            set
            {
                //Debug.Log("MPTK_MidiName set " + value);
                midiIndexToPlay = MidiPlayerGlobal.MPTK_FindMidi(value);
                //Debug.Log("MPTK_MidiName set index= " + midiIndexToPlay);
                midiNameToPlay = value;
            }
        }
        [SerializeField]
        [HideInInspector]
        private string midiNameToPlay;

        /// <summary>
        /// Index Midi. Find the Index of Midi file from the popup in MidiFilePlayer inspector.
        /// Tips: Add Midi files to your project with the Unity menu MPTK or add it directly in the ressource folder and open Midi File Setup to automatically integrate Midi in MPTK.
        /// return -1 if not found
        /// </summary>
        /// <param name="index"></param>
        public virtual int MPTK_MidiIndex
        {
            get
            {
                try
                {
                    //int index = MidiPlayerGlobal.MPTK_FindMidi(MPTK_MidiName);
                    //Debug.Log("MPTK_MidiIndex get " + midiIndexToPlay);
                    return midiIndexToPlay;
                }
                catch (System.Exception ex)
                {
                    MidiPlayerGlobal.ErrorDetail(ex);
                }
                return -1;
            }
            set
            {
                //! @code
                //! midiFilePlayer.MPTK_MidiIndex = 1;
                //! @endcode
                try
                {
                    //Debug.Log("MPTK_MidiIndex set " + value);
                    if (value >= 0 && value < MidiPlayerGlobal.CurrentMidiSet.MidiFiles.Count)
                    {
                        MPTK_MidiName = MidiPlayerGlobal.CurrentMidiSet.MidiFiles[value];
                        // useless, set when set midi name : 
                        midiIndexToPlay = value;
                    }
                    else
                        Debug.LogWarning("MidiFilePlayer - Set MidiIndex value not valid : " + value);
                }
                catch (System.Exception ex)
                {
                    MidiPlayerGlobal.ErrorDetail(ex);
                }
            }
        }

        [SerializeField]
        [HideInInspector]
        private int midiIndexToPlay;

        /// <summary>
        /// Should the Midi start playing when application start ?
        /// </summary>
        public virtual bool MPTK_PlayOnStart { get { return playOnStart; } set { playOnStart = value; } }

        /// <summary>
        /// Should automatically restart when Midi reach the end ?
        /// </summary>
        public virtual bool MPTK_Loop { get { return loop; } set { loop = value; } }

        /// <summary>
        /// Get default tempo defined in Midi file or modified with Speed. 
        /// Return QuarterPerMinuteValue similar to BPM (Beat Per Measure)
        /// </summary>
        public virtual double MPTK_Tempo { get { if (miditoplay != null) return miditoplay.QuarterPerMinuteValue; else return 0d; } }


        public string MPTK_SequenceTrackName { get { return miditoplay != null ? miditoplay.SequenceTrackName : ""; } }
        public string MPTK_ProgramName { get { return miditoplay != null ? miditoplay.ProgramName : ""; } }
        public string MPTK_TrackInstrumentName { get { return miditoplay != null ? miditoplay.TrackInstrumentName : ""; } }
        public string MPTK_TextEvent { get { return miditoplay != null ? miditoplay.TextEvent : ""; } }
        public string MPTK_Copyright { get { return miditoplay != null ? miditoplay.Copyright : ""; } }

        /// <summary>
        /// Speed of playing. 
        /// Between 0.1 (10%) to 5.0 (500%). 
        /// Set to 1 for normal speed. 
        /// </summary>
        public virtual float MPTK_Speed
        {
            get { return speed; }
            set
            {
                try
                {
                    if (value >= 0.1f && value <= 5.0f)
                    {
                        MPTK_Pause(0.3f);
                        speed = value;
                        if (miditoplay != null)
                            miditoplay.ChangeSpeed(speed);
                    }
                    else
                        Debug.LogWarning("MidiFilePlayer - Set Speed value not valid : " + value);
                }
                catch (System.Exception ex)
                {
                    MidiPlayerGlobal.ErrorDetail(ex);
                }
            }
        }

        /// <summary>
        /// Set or Get midi position time from 0 to lenght time of midi playing (in millisecond)
        /// </summary>
        public virtual double MPTK_Position
        {
            get { return (float)timeFromStartPlay; }
            set
            {
                try
                {
                    //Debug.Log("Set MPTK_Position:" + value);

                    if (value >= 0f && value <= (float)MPTK_Duration.TotalMilliseconds)
                    {
                        MPTK_Pause(0.2f);
                        timeFromStartPlay = value;
                        if (miditoplay != null)
                        {
                            int nextpos = miditoplay.CalculateNextPosEvents(timeFromStartPlay);
                            // Read only Change Patch and Tempo
                            List<MPTKEvent> midievents = miditoplay.ReadChangeFromStart(nextpos);
                            if (midievents != null)
                            {
                                foreach (MPTKEvent midievent in midievents)
                                {
                                    if (midievent.Command == MPTKCommand.MetaEvent && midievent.Meta == MPTKMeta.SetTempo && MPTK_EnableChangeTempo)
                                    {
                                        miditoplay.ChangeTempo(midievent.Duration);
                                        //Debug.Log(BuildInfoTrack(trackEvent) + string.Format("SetTempo   {0} MicrosecondsPerQuarterNote:{1}", tempo.Tempo, tempo.MicrosecondsPerQuarterNote));
                                    }
                                    else
                                        PlayEvent(midievent);
                                }
                            }
                        }
                    }
                    else
                        Debug.LogWarning("MidiFilePlayer - Set Position value not valid : " + value);
                }
                catch (System.Exception ex)
                {
                    MidiPlayerGlobal.ErrorDetail(ex);
                }
            }
        }

        [SerializeField]
        [HideInInspector]
        private float speed = 1f;

        /// <summary>
        /// Is Midi file playing is paused ?
        /// </summary>
        public virtual bool MPTK_IsPaused { get { return playPause; } }

        /// <summary>
        /// Is Midi file is playing ?
        /// </summary>
        public virtual bool MPTK_IsPlaying { get { return midiIsPlaying; } }

        /// <summary>
        /// Value updated only when playing in Unity (for inspector refresh)
        /// </summary>
        //! @cond NODOC
        public string durationEditorModeOnly;
        //! @endcond

        /// <summary>
        /// Get duration of current Midi with current tempo
        /// </summary>
        public virtual TimeSpan MPTK_Duration { get { try { if (miditoplay != null) return miditoplay.MPTK_Duration; } catch (System.Exception ex) { MidiPlayerGlobal.ErrorDetail(ex); } return TimeSpan.Zero; } }

        /// <summary>
        /// Last tick position in Midi: Value of the tick for the last midi event in sequence expressed in number of "ticks". MPTK_TickLast / MPTK_DeltaTicksPerQuarterNote equal the duration time of a quarter-note regardless the defined tempo.
        /// </summary>
        public virtual long MPTK_TickLast { get { return miditoplay != null ? miditoplay.MPTK_TickLast : 0; } }

        /// <summary>
        /// Current tick position in Midi: Time of the current midi event expressed in number of "ticks". MPTK_TickCurrent / MPTK_DeltaTicksPerQuarterNote equal the duration time of a quarter-note regardless the defined tempo.
        /// </summary>
        public virtual long MPTK_TickCurrent
        {
            get
            {
                return miditoplay != null ? miditoplay.MPTK_TickCurrent : 0;
            }
            set
            {
                try
                {
                    if (miditoplay != null)
                    {
                        //Debug.Log("Set MPTK_TickCurrent:" +value);

                        long position = value;
                        if (position < 0) position = 0;
                        if (position > MPTK_TickLast) position = MPTK_TickLast;
                        MPTK_Position = miditoplay.MPTK_ConvertTickToTime(position);
                    }
                }
                catch (System.Exception ex)
                {
                    MidiPlayerGlobal.ErrorDetail(ex);
                }
            }
        }


        /// <summary>
        /// Lenght in millisecond of a quarter
        /// </summary>
        public virtual double MPTK_PulseLenght { get { try { if (miditoplay != null) return miditoplay.TickLengthMs; } catch (System.Exception ex) { MidiPlayerGlobal.ErrorDetail(ex); } return 0d; } }

        /// <summary>
        /// Updated only when playing in Unity (for inspector refresh)
        /// </summary>
        //! @cond NODOC
        public string playTimeEditorModeOnly;
        //! @endcond

        /// <summary>
        /// Time from the start of playing the current midi
        /// </summary>
        public virtual TimeSpan MPTK_PlayTime { get { try { return TimeSpan.FromMilliseconds(timeFromStartPlay); } catch (System.Exception ex) { MidiPlayerGlobal.ErrorDetail(ex); } return TimeSpan.Zero; } }

        /// <summary>
        /// Log midi events
        /// </summary>
        public virtual bool MPTK_LogEvents
        {
            get { return logEvents; }
            set { logEvents = value; }
        }

        /// <summary>
        /// Should accept change tempo from Midi Events ? 
        /// </summary>
        public virtual bool MPTK_EnableChangeTempo
        {
            get { return enableChangeTempo; }
            set { enableChangeTempo = value; }
        }

        /// <summary>
        /// Should keep note off event Events ? 
        /// </summary>
        public virtual bool MPTK_KeepNoteOff
        {
            get { return keepNoteOff; }
            set { keepNoteOff = value; }
        }

        /// <summary>
        /// If true (default) then Midi events are sent automatically to the midi player.
        /// Set to false if you want to process events without playing sound. 
        /// OnEventNotesMidi Unity Event can be used to process each notes.
        /// </summary>
        public virtual bool MPTK_DirectSendToPlayer
        {
            get { return sendNotes; }
            set { sendNotes = value; }
        }

        /// <summary>
        /// Define unity event to trigger when notes available from the Midi file.
        ///! @code
        /// MidiFilePlayer midiFilePlayer = FindObjectOfType<MidiFilePlayer>(); 
        ///         ...
        /// if (!midiFilePlayer.OnEventNotesMidi.HasEvent())
        /// {
        ///    // No listener defined, set now by script. NotesToPlay will be called for each new notes read from Midi file
        ///    midiFilePlayer.OnEventNotesMidi.AddListener(NotesToPlay);
        /// }
        ///         ...
        /// public void NotesToPlay(List<MPTKEvent> notes)
        /// {
        ///    Debug.Log(notes.Count);
        ///    foreach (MPTKEvent midievent in notes)
        ///    {
        ///         ...
        ///    }
        /// }
        ///! @endcode
        /// </summary>
        [HideInInspector]
        public EventNotesMidiClass OnEventNotesMidi;


        /// <summary>
        /// Define unity event to trigger at start of playing the Midi.
        ///! @code
        /// MidiFilePlayer midiFilePlayer = FindObjectOfType<MidiFilePlayer>();
        ///         ...
        /// if (!midiFilePlayer.OnEventStartPlayMidi.HasEvent())
        /// {
        ///    // No listener defined, set now by script. StartPlay will be called.
        ///    midiFilePlayer.OnEventStartPlayMidi.AddListener(StartPlay);
        /// }
        ///         ...
        /// public void StartPlay(string midiname)
        /// {
        ///    Debug.LogFormat("Start playing midi {0}", midiname);
        /// }
        /// @endcode
        /// </summary>
        [HideInInspector]
        public EventStartMidiClass OnEventStartPlayMidi;

        /// <summary>
        /// Define unity event to trigger at end of playing the midi.
        ///! @code
        /// MidiFilePlayer midiFilePlayer = FindObjectOfType<MidiFilePlayer>();
        ///         ...
        /// if (!midiFilePlayer.OnEventEndPlayMidi.HasEvent())
        /// {
        ///    // No listener defined, set now by script. EndPlay will be called.
        ///    midiFilePlayer.OnEventEndPlayMidi.AddListener(EndPlay);
        /// }
        ///         ...
        /// public void EndPlay(string midiname, EventEndMidiEnum reason)
        /// {
        ///    Debug.LogFormat("End playing midi {0} reason:{1}", midiname, reason);
        /// }
        /// @endcode
        /// </summary>
        [HideInInspector]
        public EventEndMidiClass OnEventEndPlayMidi;

        /// <summary>
        /// Level of quantization : 
        ///! @li @c     0 = None 
        ///! @li @c     1 = Quarter Note
        ///! @li @c     2 = Eighth Note
        ///! @li @c     3 = 16th Note
        ///! @li @c     4 = 32th Note
        ///! @li @c     5 = 64th Note
        /// </summary>
        public virtual int MPTK_Quantization
        {
            get { return quantization; }
            set
            {
                try
                {
                    if (value >= 0 && value <= 5)
                    {
                        quantization = value;
                        miditoplay.ChangeQuantization(quantization);
                    }
                    else
                        Debug.LogWarning("MidiFilePlayer - Set Quantization value not valid : " + value);
                }
                catch (System.Exception ex)
                {
                    MidiPlayerGlobal.ErrorDetail(ex);
                }
            }
        }
        [SerializeField]
        [HideInInspector]
        private int quantization = 0;


        [SerializeField]
        [HideInInspector]
        private bool playOnStart = false, replayMidi = false, stopMidi = false,
            midiIsPlaying = false, loop = false, sendNotes = true,
            logEvents = false, enableChangeTempo = true, keepNoteOff = false;

        [SerializeField]
        [HideInInspector]
        protected bool playPause = false;

        //private float delayMilliSeconde = 10f;
        private float timeToPauseMilliSeconde = -1f;
        public double timeFromStartPlay = 0d;
        private MidiLoad miditoplay;

        /// <summary>
        /// Get all the raw midi events available in the midi file
        /// </summary>
        public virtual List<TrackMidiEvent> MPTK_MidiEvents
        {
            get
            {
                List<TrackMidiEvent> tme = null;
                try
                {
                    tme = miditoplay.MidiSorted;
                }
                catch (System.Exception ex)
                {
                    MidiPlayerGlobal.ErrorDetail(ex);
                }
                return tme;
            }
        }

        /// <summary>
        /// Delta Ticks Per Quarter Note. Indicate the duration time in "ticks" which make up a quarter-note. For instance, if 96, then a duration of an eighth-note in the file would be 48.
        /// </summary>
        public virtual int MPTK_DeltaTicksPerQuarterNote
        {
            get
            {
                int DeltaTicksPerQuarterNote = 0;
                try
                {
                    DeltaTicksPerQuarterNote = miditoplay.MPTK_DeltaTicksPerQuarterNote;
                }
                catch (System.Exception ex)
                {
                    MidiPlayerGlobal.ErrorDetail(ex);
                }
                return DeltaTicksPerQuarterNote;
            }
        }

        new void Awake()
        {
            //Debug.Log("Awake MidiFilePlayer midiIsPlaying:" + midiIsPlaying);
            midiIsPlaying = false;
            base.Awake();
        }

        new void Start()
        {
            //Debug.Log("Start MidiFilePlayer midiIsPlaying:" + midiIsPlaying + " MPTK_PlayOnStart:" + MPTK_PlayOnStart);
            base.Start();
            try
            {
                //Debug.Log("   midiIsPlaying:" + midiIsPlaying + " MPTK_PlayOnStart:" + MPTK_PlayOnStart);
                if (MPTK_PlayOnStart)
                {
                    Timing.RunCoroutine(TheadPlayIfReady());
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        //void OnApplicationPause(bool pauseStatus)
        //{
        //    Debug.Log("OnApplicationPause pauseStatus:" + pauseStatus);
        //    if (pauseStatus)
        //        MPTK_Pause();
        //}

          
        void OnApplicationQuit()
        {
            //Debug.Log("OnApplicationQuit " + Time.time + " seconds");
            MPTK_Stop();
        }

        protected IEnumerator<float> TheadPlayIfReady()
        {
            while (!MidiPlayerGlobal.MPTK_SoundFontLoaded)
                yield return Timing.WaitForSeconds(0.2f);

            // Wait a few of millisecond to let app to start (usefull when play on start)
            yield return Timing.WaitForSeconds(0.2f);

            MPTK_Play();
        }

        /// <summary>
        /// Play the midi file defined with MPTK_MidiName or MPTK_MidiIndex
        /// </summary>
        public virtual void MPTK_Play()
        {
            try
            {
                if (MidiPlayerGlobal.MPTK_SoundFontLoaded)
                {
                    playPause = false;
                    if (!midiIsPlaying)
                    {
                        // Load description of available soundfont
                        if (MidiPlayerGlobal.ImSFCurrent != null && MidiPlayerGlobal.CurrentMidiSet != null && MidiPlayerGlobal.CurrentMidiSet.MidiFiles != null && MidiPlayerGlobal.CurrentMidiSet.MidiFiles.Count > 0)
                        {
                            MPTK_InitSynth();

                            if (VerboseSynth)
                                Debug.Log(MPTK_MidiName);
                            if (string.IsNullOrEmpty(MPTK_MidiName))
                                MPTK_MidiName = MidiPlayerGlobal.CurrentMidiSet.MidiFiles[0];
                            int selectedMidi = MidiPlayerGlobal.CurrentMidiSet.MidiFiles.FindIndex(s => s == MPTK_MidiName);
                            if (selectedMidi < 0)
                            {
                                Debug.LogWarning("MidiFilePlayer - MidiFile " + MPTK_MidiName + " not found. Try with the first in list.");
                                selectedMidi = 0;
                                MPTK_MidiName = MidiPlayerGlobal.CurrentMidiSet.MidiFiles[0];
                            }

                            Timing.RunCoroutine(ThreadPlay().CancelWith(gameObject));
                            //StartCoroutine(TestWithDelay());
                        }
                        else
                            Debug.LogWarning(MidiPlayerGlobal.ErrorNoMidiFile);
                    }
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
            midiIsPlaying = false;
            playPause = false;
            stopMidi = true;
            Timing.RunCoroutine(ThreadClearAllSound(true));
        }

        /// <summary>
        /// Restart playing of the current midi file
        /// </summary>
        public virtual void MPTK_RePlay()
        {
            try
            {
                playPause = false;
                if (midiIsPlaying)
                {
                    ThreadClearAllSound();
                    replayMidi = true;
                }
                else
                    MPTK_Play();
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
                //Debug.Log("Pause " + playPause + " to " + !playPause);
                // Pause the thread ThreadPlay
                timeToPauseMilliSeconde = timeToPauseMS;
                playPause = true;
                Timing.RunCoroutine(ThreadClearAllSound());
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        /// <summary>
        /// Play next Midi from the list of midi defined in MPTK (see Unity menu Midi)
        /// </summary>
        public virtual void MPTK_Next()
        {
            try
            {
                if (MidiPlayerGlobal.CurrentMidiSet.MidiFiles != null && MidiPlayerGlobal.CurrentMidiSet.MidiFiles.Count > 0)
                {
                    int selectedMidi = 0;
                    //Debug.Log("Next search " + MPTK_MidiName);
                    if (!string.IsNullOrEmpty(MPTK_MidiName))
                        selectedMidi = MidiPlayerGlobal.CurrentMidiSet.MidiFiles.FindIndex(s => s == MPTK_MidiName);
                    if (selectedMidi >= 0)
                    {
                        selectedMidi++;
                        if (selectedMidi >= MidiPlayerGlobal.CurrentMidiSet.MidiFiles.Count)
                            selectedMidi = 0;
                        MPTK_MidiName = MidiPlayerGlobal.CurrentMidiSet.MidiFiles[selectedMidi];
                        //Debug.Log("Next found " + MPTK_MidiName);
                        MPTK_RePlay();
                    }
                }
                else
                    Debug.LogWarning(MidiPlayerGlobal.ErrorNoMidiFile);
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        /// <summary>
        /// Play previous Midi from the list of midi defined in MPTK (see Unity menu Midi)
        /// </summary>
        public virtual void MPTK_Previous()
        {
            try
            {
                if (MidiPlayerGlobal.CurrentMidiSet.MidiFiles != null && MidiPlayerGlobal.CurrentMidiSet.MidiFiles.Count > 0)
                {
                    int selectedMidi = 0;
                    if (!string.IsNullOrEmpty(MPTK_MidiName))
                        selectedMidi = MidiPlayerGlobal.CurrentMidiSet.MidiFiles.FindIndex(s => s == MPTK_MidiName);
                    if (selectedMidi >= 0)
                    {
                        selectedMidi--;
                        if (selectedMidi < 0)
                            selectedMidi = MidiPlayerGlobal.CurrentMidiSet.MidiFiles.Count - 1;
                        MPTK_MidiName = MidiPlayerGlobal.CurrentMidiSet.MidiFiles[selectedMidi];
                        MPTK_RePlay();
                    }
                }
                else
                    Debug.LogWarning(MidiPlayerGlobal.ErrorNoMidiFile);
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        private float lastTimePlay = 0;
        /// <summary>
        /// In case of delay in the application, resync is usefull to avoid multi tock play at the same time
        /// </summary>
        public void MPTK_ReSyncTime()
        {
            lastTimePlay = Time.realtimeSinceStartup;
        }

        /// <summary>
        /// Return note length as https://en.wikipedia.org/wiki/Note_value 
        /// </summary>
        /// <param name="note"></param>
        /// <returns>MPTKEvent.EnumLength</returns>
        public MPTKEvent.EnumLength MPTK_NoteLength(MPTKEvent note)
        {
            if (miditoplay != null)
                return miditoplay.NoteLength(note);
            return MPTKEvent.EnumLength.Sixteenth;
        }

        protected IEnumerator<float> ThreadPlay(byte[] midiBytesToPlay = null)
        {
            midiIsPlaying = true;
            stopMidi = false;
            replayMidi = false;
            bool first = true;
            //Debug.Log("Start play");
            try
            {
                miditoplay = new MidiLoad();

                // No midi byte array, try to load from MidiFilesDN from resource
                if (midiBytesToPlay == null || midiBytesToPlay.Length == 0)
                {
                    TextAsset mididata = Resources.Load<TextAsset>(Path.Combine(MidiPlayerGlobal.MidiFilesDB, MPTK_MidiName));
                    midiBytesToPlay = mididata.bytes;
                }

                miditoplay.KeepNoteOff = MPTK_KeepNoteOff;
                miditoplay.MPTK_Load(midiBytesToPlay);
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }

            if (miditoplay != null)
            {
                yield return Timing.WaitUntilDone(Timing.RunCoroutine(ThreadClearAllSound(true)), false);

                try
                {
                    OnEventStartPlayMidi.Invoke(MPTK_MidiName);
                }
                catch (System.Exception ex)
                {
                    MidiPlayerGlobal.ErrorDetail(ex);
                }

                try
                {
                    miditoplay.ChangeSpeed(MPTK_Speed);
                    miditoplay.ChangeQuantization(MPTK_Quantization);

                }
                catch (System.Exception ex)
                {
                    MidiPlayerGlobal.ErrorDetail(ex);
                }

                lastTimePlay = Time.realtimeSinceStartup;
                timeFromStartPlay = 0d;
                // Loop on each events midi
                do
                {
                    miditoplay.LogEvents = MPTK_LogEvents;

                    if (MPTK_PauseOnDistance)
                    {
                        distanceEditorModeOnly = MidiPlayerGlobal.MPTK_DistanceToListener(this.transform);
                        if (distanceEditorModeOnly > VoiceTemplate.Audiosource.maxDistance)
                        {
                            lastTimePlay = Time.realtimeSinceStartup;
                            yield return Timing.WaitForSeconds(0.2f);
                            continue;
                        }
                    }

                    if (playPause)
                    {
                        lastTimePlay = Time.realtimeSinceStartup;
                        yield return Timing.WaitForSeconds(0.2f);
                        if (miditoplay.EndMidiEvent || replayMidi || stopMidi)
                        {
                            break;
                        }
                        if (timeToPauseMilliSeconde > -1f)
                        {
                            timeToPauseMilliSeconde -= 0.2f;
                            if (timeToPauseMilliSeconde <= 0f)
                                playPause = false;
                        }
                        continue;
                    }

                    if (!first)
                    {
                        timeFromStartPlay += (Time.realtimeSinceStartup - lastTimePlay) * 1000d;
                    }
                    else
                    {
                        timeFromStartPlay = 0d;
                        first = false;
                    }

                    //Debug.Log("---------------- " + timeFromStartPlay );
                    // Read midi events until this time
                    List<MPTKEvent> midievents = miditoplay.ReadMidiEvents(timeFromStartPlay);

                    if (miditoplay.EndMidiEvent || replayMidi || stopMidi)
                    {
                        break;
                    }

                    // Play notes read
                    if (midievents != null && midievents.Count > 0)
                    {
                        try
                        {
                            if (OnEventNotesMidi != null)
                                OnEventNotesMidi.Invoke(midievents);
                        }
                        catch (System.Exception ex)
                        {
                            MidiPlayerGlobal.ErrorDetail(ex);
                        }

                        //float beforePLay = Time.realtimeSinceStartup;
                        //Debug.Log("---------------- play count:" + midievents.Count + " Start:" + timeFromStartPlay + " Delta:" + (Time.realtimeSinceStartup - lastTimePlay) * 1000f);
                        if (MPTK_DirectSendToPlayer)
                        {
                            foreach (MPTKEvent midievent in midievents)
                            {
                                if (midievent.Command == MPTKCommand.MetaEvent && midievent.Meta == MPTKMeta.SetTempo && MPTK_EnableChangeTempo)
                                {
                                    miditoplay.ChangeTempo(midievent.Duration);
                                    //Debug.Log(BuildInfoTrack(trackEvent) + string.Format("SetTempo   {0} MicrosecondsPerQuarterNote:{1}", tempo.Tempo, tempo.MicrosecondsPerQuarterNote));
                                }
                                else
                                    PlayEvent(midievent);
                            }
                        }
                        //Debug.Log("---------------- played count:" + notes.Count + " Start:" + timeFromStartPlay + " Delta:" + (Time.realtimeSinceStartup - lastTimePlay) * 1000f + " Elapsed:" + (Time.realtimeSinceStartup - beforePLay) * 1000f);
                    }

                    lastTimePlay = Time.realtimeSinceStartup;

                    if (Application.isEditor)
                    {
                        TimeSpan times = TimeSpan.FromMilliseconds(timeFromStartPlay);
                        playTimeEditorModeOnly = string.Format("{0:00}:{1:00}:{2:00}:{3:000}", times.Hours, times.Minutes, times.Seconds, times.Milliseconds);
                        durationEditorModeOnly = string.Format("{0:00}:{1:00}:{2:00}:{3:000}", MPTK_Duration.Hours, MPTK_Duration.Minutes, MPTK_Duration.Seconds, MPTK_Duration.Milliseconds);
                    }

                    yield return -1;// new WaitForSeconds(delayMilliSeconde / 1000f);// 0.01f);
                }
                while (true);
            }
            else
                Debug.LogWarning("MidiFilePlayer/ThreadPlay - Midi Load error");

            midiIsPlaying = false;

            try
            {
                EventEndMidiEnum reason = EventEndMidiEnum.MidiEnd;
                if (stopMidi) reason = EventEndMidiEnum.ApiStop;
                if (replayMidi) reason = EventEndMidiEnum.Replay;
                OnEventEndPlayMidi.Invoke(MPTK_MidiName, reason);

                if ((MPTK_Loop || replayMidi) && !stopMidi)
                    MPTK_Play();
                //stopMidiToPlay = false;
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
            //Debug.Log("Stop play");
        }
    }
}

