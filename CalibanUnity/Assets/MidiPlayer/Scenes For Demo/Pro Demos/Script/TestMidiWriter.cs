
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using System;
using UnityEngine.Events;
using NAudio.Midi;
using System.Linq;

namespace MidiPlayerTK
{
    /// <summary>
    /// Script for the prefab MidiFilePlayer. 
    /// Play a selected midi file. 
    /// List of Midi file must be defined with Midi Player Setup (menu tools).
    /// </summary>
    public class TestMidiWriter : MonoBehaviour
    {
        // MPTK component able to play a Midi file
        public List<MidiFilePlayer> MidiFilePlayers;

        // MPTK component able to play a stream of note like midi note generated
        public MidiStreamPlayer midiStreamPlayer;


        // Class use by the light sequencer
        #region ForSequencer

        // A note trigger by the keyboard
        class NoteTime : MPTKEvent
        {
            public double TimeMs; // Time to play a note expressed in 'ms'  
        }

        // One track of the sequencer
        class SequencerTrack
        {
            public bool Started;
            public int BeatsPerMinute;
            public int TicksPerQuarterNote;
            public List<NoteTime> MidiEvents;
            public int IndexLastNotePlayed;
            public int Patch;
            public SequencerTrack()
            {
                IndexLastNotePlayed = 0;
                Started = false;
                MidiEvents = new List<NoteTime>();
                BeatsPerMinute = 100; // slow tempo, one quarter per second
                TicksPerQuarterNote = 120; // a classical value for a Midi. define the precision of the note playing in time
            }

            public void Add(NoteTime evt)
            {
                MidiEvents.Add(evt);
            }

            public double RatioMilliSecondToTick
            {
                get { return TicksPerQuarterNote * BeatsPerMinute / 60000d; }
            }
        }

        // State of key of the keyboard
        class KeyboardState
        {
            public int Key;
            public double TimeNoteOn;
            public Rect Zone;
            public NoteTime Note;
        }

        /// <summary>
        /// Return real time since startup in milliseconds
        /// </summary>
        private double CurrentTimeMs { get { return Time.realtimeSinceStartup * 1000d; } }


        bool SequenceIsPlaying = false;
        SequencerTrack PianoTrack;
        double TimeStartCreateSequence;
        double TimeStartPlay;
        KeyboardState[] KeysState = null;
        double CurrentTimePlaying;

        // Create a popup able to select preset/patch for the sequencer
        private PopupListItem PopPatch;
        private PopupListItem PopMidi;

        #endregion

        int selectedMidi;
        int spaceH = 30;

        // Manage skin
        public CustomStyle myStyle;

        Vector2 scrollerWindow = Vector2.zero;

        void Awake()
        {
            //Debug.Log("Awake MidiExport");
            PianoTrack = new SequencerTrack();
            float StartTime = Time.realtimeSinceStartup;

            PopPatch = new PopupListItem()
            {
                Title = "Select A Patch",
                OnSelect = PatchChanged,
                Tag = "NEWPATCH",
                ColCount = 5,
                ColWidth = 200,
            };

            PopMidi = new PopupListItem()
            {
                Title = "Select A Midi File",
                OnSelect = MidiChanged,
                Tag = "NEWMIDI",
                ColCount = 3,
                ColWidth = 250,
            };

            if (MidiFilePlayers == null || MidiFilePlayers.Count == 0)
            {
                Component[] gameObjects = GetComponentsInChildren<Component>();
                if (gameObjects == null)
                    Debug.LogWarning("No MidiFilePlayer component found in MidiListPlayer.");
                else
                {
                    MidiFilePlayers = new List<MidiFilePlayer>();
                    foreach (Component comp in gameObjects)
                        if (comp is MidiFilePlayer)
                            MidiFilePlayers.Add((MidiFilePlayer)comp);
                }
            }

            // State of each key of the keyboard
            KeysState = new KeyboardState[127];
            for (int key = 0; key < KeysState.Length; key++)
                KeysState[key] = new KeyboardState() { Key = key, Note = null, TimeNoteOn = 0d };
        }
        static private Texture buttonIconFolder;

        void OnGUI()
        {
            if (!HelperDemo.CheckSFExists()) return;

            // Set custom Style. Good for background color 3E619800
            if (myStyle == null) myStyle = new CustomStyle();

            scrollerWindow = GUILayout.BeginScrollView(scrollerWindow, false, false, GUILayout.Width(Screen.width));

            // Display popup in first to avoid activate other layout behind
            PopPatch.Draw(MidiPlayerGlobal.MPTK_ListPreset, PianoTrack.Patch, myStyle);

            // Display popup in first to avoid activate other layout behind
            PopMidi.Draw(MidiPlayerGlobal.MPTK_ListMidi, selectedMidi, myStyle);

            MainMenu.Display("Demonstation of four methods to create a Midi file", myStyle);

            //
            // Open the result directory
            // -------------------------
            GUILayout.BeginHorizontal(myStyle.BacgDemos);
            if (buttonIconFolder == null)
                buttonIconFolder = Resources.Load<Texture2D>("Textures/computer");
            if (GUILayout.Button(new GUIContent(buttonIconFolder, "Open the directory"), GUILayout.Width(48), GUILayout.Height(48)))
                Application.OpenURL(Application.persistentDataPath);
            GUILayout.Space(spaceH);
            GUILayout.Label("Midi Files are created here " + Application.persistentDataPath, myStyle.TitleLabel2, GUILayout.Height(48));
            GUILayout.EndHorizontal();

            //
            // Export from a list of MidiFilePlayer
            // ------------------------------------
            ExportFromListOfMidiFilePlayer();

            //
            // Create midi file from a generated midi
            // --------------------------------------
            CreateMidiFromGeneratedNote();

            //
            // Export midi file from the MPTK MidiDB list
            // ------------------------------------------
            ExportMidiFromMidiDBList();

            //
            // Export midi file from a real time generated note
            // ------------------------------------------
            CreateMidiFromPiano();

            GUILayout.EndScrollView();

        }

        // ------------------------------------------------------------------------------------------------------------------------------------
        // First ------------------------------------------------------------------------------------------------------------------------------
        // ------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 1 / Export from a list of MidiFilePlayer
        /// </summary>
        private void ExportFromListOfMidiFilePlayer()
        {
            GUILayout.BeginVertical(myStyle.BacgDemos);
            GUILayout.Label("1) From all MidiFilePlayer components found. The Midi must playing to exports the file.", myStyle.TitleLabel2);
            GUILayout.BeginHorizontal();
            GUILayout.Space(spaceH);
            string midiname;
            bool isPlaying = false;
            if (MidiFilePlayers != null && MidiFilePlayers.Count > 0)
            {
                midiname = ((MidiFilePlayer)MidiFilePlayers[0]).MPTK_MidiName;
                isPlaying = ((MidiFilePlayer)MidiFilePlayers[0]).MPTK_IsPlaying;
            }
            else
                midiname = "No MidiFilePlayer found";
            GUILayout.Label(midiname + (isPlaying ? " is playing" : " not playing"));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(spaceH);
            if (GUILayout.Button("Export from MidiFilePlayer components", GUILayout.Width(400)))
            {
                foreach (Component ms in MidiFilePlayers)
                    if (ms is MidiFilePlayer)
                    {
                        MidiFilePlayer mfp = ((MidiFilePlayer)ms);
                        string filename = Path.Combine(Application.persistentDataPath, mfp.MPTK_MidiName + "_MFP.mid");
                        MidiFileWriter mfw = new MidiFileWriter(mfp.MPTK_DeltaTicksPerQuarterNote, 1);
                        mfw.MPTK_LoadFromMPTK(mfp.MPTK_MidiEvents);
                        mfw.MPTK_WriteToFile(filename);
                    }
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

        }


        // ----------------------------------------------------------------------------------------------------------------------------------
        // 2nd ------------------------------------------------------------------------------------------------------------------------------
        // ----------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 2/ Create midi file from a generated midi
        /// </summary>
        private void CreateMidiFromGeneratedNote()
        {
            GUILayout.BeginVertical(myStyle.BacgDemos);
            GUILayout.Label("2) From a generated Midi sequence.", myStyle.TitleLabel2);
            GUILayout.BeginHorizontal();
            GUILayout.Space(spaceH);
            GUILayout.Label("Four notes will be created");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Space(spaceH);
            if (GUILayout.Button("Create a sequence of notes and write to the Midi file", GUILayout.Width(400)))
            {
                int beatsPerMinute = 60; // slow tempo, one quarter per second
                int ticksPerQuarterNote = 120; // a classical value for a Midi. define the precision of the note playing in time
                long absoluteTime = 0; // Time to play a note expressed in 'ticksPerQuarterNote'  
                int noteDuration = ticksPerQuarterNote; // Length to the a note expressed in 'ticksPerQuarterNote'  
                long spaceBetweenNotes = ticksPerQuarterNote; // in this example, all notes are quarter plays sequentially
                int channel = 1;
                int velocity = 100;

                // Create a midi file writer. Idea from here, thank to them.
                // http://opensebj.blogspot.com/2009/09/naudio-tutorial-7-basics-of-midi-files.html
                // https://deejaygraham.github.io/2012/09/22/using-naudio-to-generate-midi/

                MidiFileWriter mfw = new MidiFileWriter(ticksPerQuarterNote, 1);

                // First track (index=0) is a general midi information track. By convention contains no noteon
                // Second track (index=1) will contains the notes
                mfw.MPTK_CreateTrack(2);

                // Some textual information added to the track 0
                mfw.MPTK_AddEvent(0, new TextEvent("Midi Generated by MPTK/NAudio", MetaEventType.SequenceTrackName, absoluteTime++));

                // TimeSignatureEvent (not mandatory) exposes 
                //      Numerator(number of beats in a bar), 
                //      Denominator(which is confusingly in 'beat units' so 1 means 2, 2 means 4(crochet), 3 means 8(quaver), 4 means 16 and 5 means 32), 
                // as well as TicksInMetronomeClick and No32ndNotesInQuarterNote.
                mfw.MPTK_AddEvent(0, new TimeSignatureEvent(absoluteTime++, 4, 2, 24, 32));

                // Default tempo of playing (not mandatory)
                mfw.MPTK_AddEvent(0, new TempoEvent(MidiFileWriter.MPTK_GetMicrosecondsPerQuaterNote(beatsPerMinute), absoluteTime++));

                // Patch/preset to use for channel 0. Generally 0 means Gran Piano
                mfw.MPTK_AddEvent(0, new PatchChangeEvent(absoluteTime++, channel, 0));

                // Add four notes : C C# D D#
                for (int note = 60; note <= 63; note++)
                {
                    mfw.MPTK_AddNote(1, absoluteTime, channel, note, velocity, noteDuration);
                    absoluteTime += spaceBetweenNotes;
                }

                // It's mandatory to close track
                mfw.MPTK_EndTrack(0);
                mfw.MPTK_EndTrack(1);

                // build the path + filename to the midi
                string filename = Path.Combine(Application.persistentDataPath, "Generated Midi.mid");

                // wrtite the midi file
                mfw.MPTK_WriteToFile(filename);
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        // -------------------------------------------------------------------------------------------------------------------------------------------
        // Third method ------------------------------------------------------------------------------------------------------------------------------
        // -------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 3 / Export midi file from the MPTK MidiDB list
        /// </summary>
        private void ExportMidiFromMidiDBList()
        {
            GUILayout.BeginVertical(myStyle.BacgDemos);
            GUILayout.Label("3) From the MPTK MidiDB list.", myStyle.TitleLabel2);
            GUILayout.BeginHorizontal();
            GUILayout.Space(spaceH);
            if (MidiPlayerGlobal.CurrentMidiSet != null && MidiPlayerGlobal.CurrentMidiSet.MidiFiles != null && MidiPlayerGlobal.CurrentMidiSet.MidiFiles.Count > 0)
            {
                // Open the popup to select a midi
                if (GUILayout.Button("Current Midi file: " + MidiPlayerGlobal.CurrentMidiSet.MidiFiles[selectedMidi], GUILayout.Width(400)))
                    PopMidi.Show = !PopMidi.Show;
                PopMidi.Position(ref scrollerWindow);


                //// Create the list of midi available
                //scrollMidiList = GUILayout.BeginScrollView(scrollMidiList, false, false, myStyle.HScroll, myStyle.VScroll, myStyle.BackgWindow, GUILayout.Width(400), GUILayout.Height(120));
                //int index = 0;
                //foreach (string s in MidiPlayerGlobal.CurrentMidiSet.MidiFiles)
                //{
                //    GUIStyle styleBt = myStyle.BtStandard;
                //    if (selectedMidi == index) styleBt = myStyle.BtSelected;
                //    if (GUILayout.Button(s, styleBt)) selectedMidi = index;
                //    index++;
                //}
                //GUILayout.EndScrollView();
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Space(spaceH);
            if (GUILayout.Button("Export from MidiDb to a Midi file", GUILayout.Width(400)))
            {
                // Create a midi file writer
                MidiFileWriter mfw = new MidiFileWriter();
                // Load the selected midi in writer
                mfw.MPTK_LoadFromMidiDB(selectedMidi);
                // build th path + filename to the midi
                string filename = Path.Combine(Application.persistentDataPath, MidiPlayerGlobal.CurrentMidiSet.MidiFiles[selectedMidi] + "_DB.mid");
                // wrtite the midi file
                mfw.MPTK_WriteToFile(filename);
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        private void MidiChanged(object tag, int midiindex)
        {
            Debug.Log("MidiChanged " + midiindex + " for " + tag);
            selectedMidi = midiindex;
            // return true;
        }
        // -----------------------------------------------------------------------------------------------------------------------------------------
        // 4th method ------------------------------------------------------------------------------------------------------------------------------
        // -----------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 4 / create a midi file from a real time notes stream
        /// </summary>
        private void CreateMidiFromPiano()
        {
            GUILayout.BeginVertical(myStyle.BacgDemos);
            GUILayout.Label("4) From a real-time notes stream.", myStyle.TitleLabel2);

            // Detect mouse event to create note
            CheckKeyboardEvent();

            GUILayout.BeginHorizontal();
            GUILayout.Space(spaceH);

            // Select a preset
            int ambitus = 3;

            // Get position of button which triggers the popup
            //Event e = Event.current;
            //if (e.type == EventType.Repaint)
            //{
            //    Rect lastRect = GUILayoutUtility.GetLastRect();
            //    PopPatch.Position = new Vector2(
            //        lastRect.x - scrollerWindow.x,
            //        lastRect.y - PopPatch.RealRect.height - lastRect.height - scrollerWindow.y);
            //}

            // Create the keyboard
            for (int key = 48; key < 48 + ambitus * 12; key++)
            {
                // Create a key
                GUILayout.Button(
                    HelperNoteLabel.LabelFromMidi(key),
                    HelperNoteLabel.IsSharp(key) ? myStyle.KeyBlack : myStyle.KeyWhite,
                    GUILayout.Width(25),
                    GUILayout.Height(HelperNoteLabel.IsSharp(key) ? 40 : 50));

                // Get last key position
                Event e = Event.current;
                if (e.type == EventType.Repaint)
                    KeysState[key].Zone = GUILayoutUtility.GetLastRect();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(spaceH);

            // Reset and create a new sequence of notes
            if (GUILayout.Button("New Sequence", GUILayout.Width(100), GUILayout.Height(30)))
            {
                PianoTrack = new SequencerTrack();
            }

            GUILayout.Space(spaceH);
            if (MidiPlayerGlobal.CurrentMidiSet != null && MidiPlayerGlobal.CurrentMidiSet.ActiveSounFontInfo != null)
            {
                string name = "Patch not defined";
                if (MidiPlayerGlobal.MPTK_ListPreset != null && PianoTrack.Patch >= 0 && PianoTrack.Patch < MidiPlayerGlobal.MPTK_ListPreset.Count)
                    if (PianoTrack.Patch < MidiPlayerGlobal.MPTK_ListPreset.Count)
                        if (MidiPlayerGlobal.MPTK_ListPreset[PianoTrack.Patch] != null)
                            name = MidiPlayerGlobal.MPTK_ListPreset[PianoTrack.Patch].Label;
                if (GUILayout.Button(name, GUILayout.Width(200), GUILayout.Height(30)))
                    PopPatch.Show = !PopPatch.Show;
                PopPatch.Position(ref scrollerWindow);
            }

            // Play or stop sequence
            GUIStyle styleBt = myStyle.BtStandard;
            string btAction = "Play";
            if (SequenceIsPlaying)
            {
                styleBt = myStyle.BtSelected;
                btAction = "Stop";
            }

            GUILayout.Space(spaceH);
            if (GUILayout.Button(btAction, styleBt, GUILayout.Width(100), GUILayout.Height(30)))
                if (SequenceIsPlaying)
                    StopPlaying();
                else
                    StartPlaying();

            // Info sequence
            string infoSequence = "";
            if (PianoTrack != null)
                //DateTime.FromOADate(CurrentTimePlaying).ToLongTimeString()+" " +
                if (SequenceIsPlaying)
                    infoSequence = "Playing: " + (PianoTrack.IndexLastNotePlayed + 1) + "/" + PianoTrack.MidiEvents.Count;
                else
                    infoSequence = "Length: " + PianoTrack.MidiEvents.Count;
            GUILayout.Label(infoSequence, myStyle.LabelCentered, GUILayout.Width(200), GUILayout.Height(30));
            GUILayout.Space(spaceH);
            // Write the sequence as a midi file
            if (GUILayout.Button("Write the sequence of notes to a Midi file", GUILayout.Width(300), GUILayout.Height(30)))
            {
                CreateMidiFromSequence();
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        private void StartPlaying()
        {
            TimeStartPlay = CurrentTimeMs - 100d; // Let 100ms before playing
            PianoTrack.IndexLastNotePlayed = -1;
            Debug.Log("StartPlaying - TimeStartPlay:" + (int)TimeStartPlay);
            SequenceIsPlaying = true;
        }

        private void StopPlaying()
        {
            SequenceIsPlaying = false;
        }

        /// <summary>
        /// Check mouse down and up and create noteon noteoff
        /// </summary>
        private void CheckKeyboardEvent()
        {
            Event e = Event.current;
            //if (e.type != EventType.Layout && e.type != EventType.Repaint) Debug.Log(e.type + " " + e.mousePosition + " isMouse:" + e.isMouse + " isKey:" + e.isKey + " keyCode:" + e.keyCode + " modifiers:" + e.modifiers + " displayIndex:" + e.displayIndex);

            if (e.type == EventType.KeyDown || e.type == EventType.KeyUp)
            {
                if (e.keyCode >= KeyCode.Keypad0 && e.keyCode <= KeyCode.Keypad9)
                {
                    KeyboardState ks = KeysState[e.keyCode - KeyCode.Keypad0 + 48];
                    if (e.type == EventType.KeyDown)
                    {
                        if (ks.Note == null)
                        {
                            // Start the sequence at the first note detected
                            if (!PianoTrack.Started) StartSequence();

                            // Create a new note
                            NewNote(ks);
                        }
                    }
                    else if (e.type == EventType.KeyUp)
                    {
                        StopNote(ks);
                        //KeysState[e.keyCode - KeyCode.Keypad0 + 48] = null;
                    }
                }
                e.Use();
            }

            if (e.type == EventType.MouseDown || e.type == EventType.MouseUp)
            {
                bool foundKey = false;
                foreach (KeyboardState ks in KeysState)
                {
                    if (ks != null)
                    {
                        if (ks.Zone.Contains(e.mousePosition))
                        {
                            foundKey = true;
                            if (e.type == EventType.MouseDown)
                            {
                                // Start the sequence at the first note detected
                                if (!PianoTrack.Started) StartSequence();

                                // Stop note in case of forgotten playing note !
                                StopNote(ks);

                                // Create a new note
                                NewNote(ks);
                            }
                            else if (e.type == EventType.MouseUp)
                            {
                                if (ks.Note != null)
                                    // Mouse Up inside button note with an existing noteon
                                    StopNote(ks);
                                else
                                    // Mouse Up inside button note but without noteon
                                    StopAllNotes();
                            }
                            break;
                        }
                    }
                }
                // Mouse Up outside all button note
                if (!foundKey && e.type == EventType.MouseUp)
                    StopAllNotes();

            }
        }

        /// <summary>
        /// Start sequence
        /// </summary>
        private void StartSequence()
        {
            TimeStartCreateSequence = CurrentTimeMs;
            Debug.Log("TimeStartCreateSequence:" + (int)TimeStartCreateSequence);
            PianoTrack.Started = true;
        }

        private void PatchChanged(object tag, int patch)
        {
            if (!PianoTrack.Started) StartSequence();

            PianoTrack.Patch = patch;
            NoteTime patchchange = new NoteTime() { Command = MPTKCommand.PatchChange, TimeMs = CurrentTimeMs - TimeStartCreateSequence, Value = patch, Channel = 1, };
            RemoveLongSilence(patchchange);
            PianoTrack.Add(patchchange);

            // Play the change directly when bt patch is hit (could have bad interact if sequence is playing in the same time !)
            midiStreamPlayer.MPTK_PlayEvent(patchchange);
        }

        /// <summary>
        /// Create a new note and play
        /// </summary>
        /// <param name="ks"></param>
        private void NewNote(KeyboardState ks)
        {
            ks.TimeNoteOn = CurrentTimeMs - TimeStartCreateSequence;
            Debug.Log("NewNote TimeNoteOn:" + (int)ks.TimeNoteOn + " " + ks.Key);
            ks.Note = new NoteTime()
            {
                Command = MPTKCommand.NoteOn,
                Value = ks.Key,
                TimeMs = ks.TimeNoteOn, // time to start playing the note
                Channel = 1,
                Duration = 99999, // real duration will be set when StopNote will be called
                Velocity = 100
            };

            // Play the note directly when keyboard is hit (could have bad interact if sequence is playing in the same time !)
            midiStreamPlayer.MPTK_PlayEvent(ks.Note);
        }

        /// <summary>
        /// Stop note and store time & duration in milliseconds
        /// </summary>
        /// <param name="ks"></param>
        private void StopNote(KeyboardState ks)
        {
            if (ks.Note != null)
            {

                //ks.Notes.TimeMs = ks.TimeNoteOn;
                ks.Note.Duration = CurrentTimeMs - TimeStartCreateSequence - ks.TimeNoteOn;
                Debug.Log("StopNote TimeMs:" + (int)ks.Note.TimeMs + " Duration:" + (int)ks.Note.Duration);

                // Add to the sequencer
                RemoveLongSilence(ks.Note);
                PianoTrack.Add(ks.Note);

                // Stop the note directly when keyboard is hit (could have bad interact if sequence is playing in the same time !)
                midiStreamPlayer.MPTK_StopEvent(ks.Note);

                ks.Note = null;
            }
        }

        private void StopAllNotes()
        {
            foreach (KeyboardState ks in KeysState)
                if (ks != null)
                    StopNote(ks);
        }

        private void RemoveLongSilence(NoteTime evt)
        {
            // It's a light sequencer ! Avoid long time between two events
            if (PianoTrack.MidiEvents.Count > 0)
            {
                bool isplaying = false;
                // If any note is playing
                foreach (KeyboardState ks in KeysState)
                    if (ks.Note != null && ks.Note != evt)
                        // A note is playing
                        isplaying = true;
                if (!isplaying)
                {
                    NoteTime lastnote = PianoTrack.MidiEvents.Last();
                    if ((evt.TimeMs - lastnote.TimeMs) > 2000)
                    {
                        evt.TimeMs = lastnote.TimeMs + lastnote.Duration + 500d; // add a little delay from the last note
                        Debug.Log("****** new TimeMs: " + evt.TimeMs);
                    }
                }
                else
                    Debug.Log("is playing: " + evt.TimeMs);

            }
        }

        /// <summary>
        /// Create a Midi file from the PianoTrack sequence
        /// </summary>
        private void CreateMidiFromSequence()
        {
            // Create a midi file writer. 
            MidiFileWriter mfw = new MidiFileWriter(PianoTrack.TicksPerQuarterNote, 1);

            // First track (index=0) is a general midi information track. By convention contains no noteon
            // Second track (index=1) will contains the notes
            mfw.MPTK_CreateTrack(2);

            // Some textual information added to the track 0
            mfw.MPTK_AddEvent(0, new TextEvent("Midi Sequence by MPTK/NAudio", MetaEventType.SequenceTrackName, 0));

            // TimeSignatureEvent (not mandatory) exposes 
            //      Numerator(number of beats in a bar), 
            //      Denominator(which is confusingly in 'beat units' so 1 means 2, 2 means 4(crochet), 3 means 8(quaver), 4 means 16 and 5 means 32), 
            //      as well as TicksInMetronomeClick and No32ndNotesInQuarterNote.
            mfw.MPTK_AddEvent(0, new TimeSignatureEvent(0, 4, 2, 24, 32));

            // Default tempo of playing (not mandatory)
            mfw.MPTK_AddEvent(0, new TempoEvent(MidiFileWriter.MPTK_GetMicrosecondsPerQuaterNote(PianoTrack.BeatsPerMinute), 0));

            foreach (NoteTime note in PianoTrack.MidiEvents)
            {
                // Concert time in ms to midi ticks
                long absoluteTime = (long)(note.TimeMs * PianoTrack.RatioMilliSecondToTick);
                switch (note.Command)
                {
                    case MPTKCommand.PatchChange:
                        // Preset to use for channel 1. Generally 0 means Grand Piano. Use track 0 for patch change (not mandatory !)
                        mfw.MPTK_AddEvent(0, new PatchChangeEvent(absoluteTime, note.Channel, note.Value));
                        break;
                    case MPTKCommand.NoteOn:
                        // Add a note on track 1
                        mfw.MPTK_AddNote(1, absoluteTime, note.Channel, note.Value, note.Velocity, (int)(note.Duration * PianoTrack.RatioMilliSecondToTick));
                        break;
                    case MPTKCommand.ControlChange:
                        // Add a control change
                        mfw.MPTK_AddEvent(1, new ControlChangeEvent(absoluteTime, note.Channel, MidiController.Sustain,127));
                        break;
                }
            }

            // It's mandatory to close track
            mfw.MPTK_EndTrack(0);
            mfw.MPTK_EndTrack(1);

            // build the path + filename to the midi
            string filename = Path.Combine(Application.persistentDataPath, "Generated Midi.mid");
            Debug.Log("Write Midi " + filename);
            // wrtite the midi file
            mfw.MPTK_WriteToFile(filename);
        }



        public void Update()
        {
            if (SequenceIsPlaying)
            {
                // try to simulate a sequencer. Warning: using Update for this function is not a good idea beacause
                // delay between update is too long to create a real sequencer. You have to use CoRoutine for that.
                if (PianoTrack != null && PianoTrack.MidiEvents != null && PianoTrack.MidiEvents.Count > 0)
                {
                    // Calculate the current time
                    CurrentTimePlaying = CurrentTimeMs - TimeStartPlay;
                    NoteTime lastKn = PianoTrack.MidiEvents[PianoTrack.MidiEvents.Count - 1];

                    // reach end of sequence, loop after 1000 ms of delay
                    if (CurrentTimePlaying > lastKn.TimeMs + lastKn.Duration + 1000d)
                    {
                        StartPlaying();
                        CurrentTimePlaying = CurrentTimeMs - TimeStartPlay;
                    }

                    // Search the next note to play
                    for (int index = PianoTrack.IndexLastNotePlayed + 1; index < PianoTrack.MidiEvents.Count; index++)
                    {
                        if (CurrentTimePlaying > PianoTrack.MidiEvents[index].TimeMs)
                        {
                            PianoTrack.IndexLastNotePlayed = index;
                            // Play the note
                            midiStreamPlayer.MPTK_PlayEvent(PianoTrack.MidiEvents[index]);
                            Debug.Log("Play midi event. Index:" + index + " Time:" + (int)PianoTrack.MidiEvents[index].TimeMs + " Duration:" + (int)PianoTrack.MidiEvents[index].Duration + " Command: " + PianoTrack.MidiEvents[index].Command);
                            break;
                        }
                    }
                }
            }
        }
    }
}

