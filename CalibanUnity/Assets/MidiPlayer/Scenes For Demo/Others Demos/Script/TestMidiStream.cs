using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MidiPlayerTK;
using System;

namespace MidiPlayerTK
{
    public class TestMidiStream : MonoBehaviour
    {

        // MPTK component able to play a stream of midi events
        public MidiStreamPlayer midiStreamPlayer;

        [Range(0.1f, 10f)]
        public float DelayTimeChange = 1;

        public bool RandomPlay = true;
        public bool DrumKit = false;

        [Range(0, 127)]
        public int StartNote = 50;

        [Range(0, 127)]
        public int EndNote = 60;

        [Range(0, 127)]
        public int Velocity = 100;

        [Range(0, 16)]
        public int StreamChannel = 0;

        [Range(0, 127)]
        public int CurrentNote;

        [Range(0, 127)]
        public int CurrentPatchInstrument;

        [Range(0, 127)]
        public int CurrentPatchDrum;

        [Range(0, 127)]
        public int PanChange;

        [Range(0, 127)]
        public int ModChange;

        [Range(0, 127)]
        public int ExpChange;

        /// <summary>
        /// Current note playing
        /// </summary>
        private MPTKEvent NotePlaying;

        private float LastTimeChange;

        /// <summary>
        /// Popup to select an instrument
        /// </summary>
        private PopupListItem PopPatchInstrument;
        private PopupListItem PopBankInstrument;
        private PopupListItem PopPatchDrum;
        private PopupListItem PopBankDrum;

        // Manage skin
        public CustomStyle myStyle;

        private Vector2 scrollerWindow = Vector2.zero;
        private int buttonWidth = 250;
        private int sliderwidth = 150;
        private float spaceVertival = 8;
        private float spaceHorizontal = 5;
        private float widthLabel = 120;
        private bool Isplaying = true;

        // Use this for initialization
        void Start()
        {

            if (midiStreamPlayer != null)
            {
                if (!midiStreamPlayer.OnEventSynthStarted.HasEvent())
                    midiStreamPlayer.OnEventSynthStarted.AddListener(StartLoadingSynth);
                if (!midiStreamPlayer.OnEventSynthStarted.HasEvent())
                    midiStreamPlayer.OnEventSynthStarted.AddListener(EndLoadingSynth);
            }
            else
                Debug.LogWarning("No Stream Midi Player associed to this game object");

            PopBankInstrument = new PopupListItem() { Title = "Select A Bank", OnSelect = BankPatchChanged, Tag = "BANK_INST", ColCount = 5, ColWidth = 150, };
            PopPatchInstrument = new PopupListItem() { Title = "Select A Patch", OnSelect = BankPatchChanged, Tag = "PATCH_INST", ColCount = 5, ColWidth = 150, };
            PopBankDrum = new PopupListItem() { Title = "Select A Bank", OnSelect = BankPatchChanged, Tag = "BANK_DRUM", ColCount = 5, ColWidth = 150, };
            PopPatchDrum = new PopupListItem() { Title = "Select A Patch", OnSelect = BankPatchChanged, Tag = "PATCH_DRUM", ColCount = 5, ColWidth = 150, };

            LastTimeChange = Time.realtimeSinceStartup;
            ///midiStreamPlayer.MPTK_Play(new MPTKEvent() { Command = MPTKCommand.PatchChange, Patch = CurrentPatchInstrument, Channel = StreamChannel, });
            CurrentNote = StartNote;
            PanChange = 64;
            LastTimeChange = -9999999f;
        }

        /// <summary>
        /// This call is defined from MidiPlayerGlobal event inspector. Run when SF is loaded.
        /// </summary>
        public void EndLoadingSF()
        {
            Debug.Log("End loading SF, MPTK is ready to play");

            //Debug.Log("List of presets available");
            //int i = 0;
            //foreach (string preset in MidiPlayerGlobal.MPTK_ListPreset)
            //    Debug.Log("   " + string.Format("[{0,3:000}] - {1}", i++, preset));
            //i = 0;
            //Debug.Log("List of drums available");
            //foreach (string drum in MidiPlayerGlobal.MPTK_ListDrum)
            //    Debug.Log("   " + string.Format("[{0,3:000}] - {1}", i++, drum));

            Debug.Log("Load statistique");
            Debug.Log("   Time To Load SoundFont: " + Math.Round(MidiPlayerGlobal.MPTK_TimeToLoadSoundFont.TotalSeconds, 3).ToString() + " second");
            Debug.Log("   Time To Load Waves: " + Math.Round(MidiPlayerGlobal.MPTK_TimeToLoadWave.TotalSeconds, 3).ToString() + " second");
            Debug.Log("   Presets Loaded: " + MidiPlayerGlobal.MPTK_CountPresetLoaded);
            Debug.Log("   Waves Loaded: " + MidiPlayerGlobal.MPTK_CountWaveLoaded);

        }
        public void StartLoadingSynth(string name)
        {
            Debug.LogFormat("Synth {0} loading", name);
        }

        public void EndLoadingSynth(string name)
        {
            Debug.LogFormat("Synth {0} loaded", name);
            midiStreamPlayer.MPTK_PlayEvent(new MPTKEvent() { Command = MPTKCommand.PatchChange, Value = CurrentPatchInstrument, Channel = StreamChannel, });
        }

        private void BankPatchChanged(object tag, int index)
        {
            switch ((string)tag)
            {
                case "BANK_INST":
                    MidiPlayerGlobal.MPTK_SelectBankInstrument(index);
                    midiStreamPlayer.MPTK_PlayEvent(new MPTKEvent() { Command = MPTKCommand.ControlChange, Controller = MPTKController.BankSelect, Value = index, Channel = StreamChannel, });
                    break;

                case "PATCH_INST":
                    CurrentPatchInstrument = index;
                    midiStreamPlayer.MPTK_PlayEvent(new MPTKEvent() { Command = MPTKCommand.PatchChange, Value = index, Channel = StreamChannel, });
                    break;

                case "BANK_DRUM":
                    MidiPlayerGlobal.MPTK_SelectBankDrum(index);
                    midiStreamPlayer.MPTK_PlayEvent(new MPTKEvent() { Command = MPTKCommand.ControlChange, Controller = MPTKController.BankSelect, Value = index, Channel = 9, });
                    break;

                case "PATCH_DRUM":
                    CurrentPatchDrum = index;
                    midiStreamPlayer.MPTK_PlayEvent(new MPTKEvent() { Command = MPTKCommand.PatchChange, Value = index, Channel = 9 });
                    break;
            }

        }

        void OnGUI()
        {
            if (!HelperDemo.CheckSFExists()) return;

            // Set custom Style. Good for background color 3E619800
            if (myStyle == null) myStyle = new CustomStyle();

            if (midiStreamPlayer != null)
            {

                scrollerWindow = GUILayout.BeginScrollView(scrollerWindow, false, false, GUILayout.Width(Screen.width));

                // If need, display the popup  before any other UI to avoid trigger it hidden
                PopBankInstrument.Draw(MidiPlayerGlobal.MPTK_ListBank, MidiPlayerGlobal.ImSFCurrent.DefaultBankNumber, myStyle);
                PopPatchInstrument.Draw(MidiPlayerGlobal.MPTK_ListPreset, CurrentPatchInstrument, myStyle);
                PopBankDrum.Draw(MidiPlayerGlobal.MPTK_ListBank, MidiPlayerGlobal.ImSFCurrent.DrumKitBankNumber, myStyle);
                PopPatchDrum.Draw(MidiPlayerGlobal.MPTK_ListPresetDrum, CurrentPatchDrum, myStyle);

                MainMenu.Display("Test Midi Stream - A very simple Generated Music Stream ", myStyle);

                // Display soundfont available and select a new one
                GUISelectSoundFont.Display(scrollerWindow, myStyle);

                // Select bank & Patch for Instrument
                // ----------------------------------
                //GUILayout.Space(spaceVertival);
                //GUILayout.Space(spaceVertival);
                GUILayout.BeginVertical(myStyle.BacgDemos);
                GUILayout.BeginHorizontal();
                GUILayout.Label("Instrument: ", myStyle.TitleLabel3, GUILayout.Width(widthLabel));

                // Open the popup to select a bank
                if (GUILayout.Button(MidiPlayerGlobal.ImSFCurrent.DefaultBankNumber + " - Bank", GUILayout.Width(buttonWidth)))
                    PopBankInstrument.Show = !PopBankInstrument.Show;
                PopBankInstrument.Position(ref scrollerWindow);

                // Open the popup to select an instrument
                if (GUILayout.Button(
                    CurrentPatchInstrument.ToString() + " - " +
                    MidiPlayerGlobal.MPTK_GetPatchName(MidiPlayerGlobal.ImSFCurrent.DefaultBankNumber,
                    CurrentPatchInstrument),
                    GUILayout.Width(buttonWidth)))
                    PopPatchInstrument.Show = !PopPatchInstrument.Show;
                PopPatchInstrument.Position(ref scrollerWindow);

                GUILayout.EndHorizontal();

                // Select bank & Patch for Drum
                // ----------------------------
                GUILayout.Space(spaceVertival);
                GUILayout.BeginHorizontal();
                GUILayout.Label("Drum: ", myStyle.TitleLabel3, GUILayout.Width(widthLabel));

                // Open the popup to select a bank for drum
                if (GUILayout.Button(MidiPlayerGlobal.ImSFCurrent.DrumKitBankNumber + " - Bank", GUILayout.Width(buttonWidth)))
                    PopBankDrum.Show = !PopBankDrum.Show;
                PopBankDrum.Position(ref scrollerWindow);

                // Open the popup to select an instrument for drum
                if (GUILayout.Button(
                    CurrentPatchDrum.ToString() + " - " +
                    MidiPlayerGlobal.MPTK_GetPatchName(MidiPlayerGlobal.ImSFCurrent.DrumKitBankNumber, CurrentPatchDrum),
                    GUILayout.Width(buttonWidth)))
                    PopPatchDrum.Show = !PopPatchDrum.Show;
                PopPatchDrum.Position(ref scrollerWindow);

                GUILayout.EndHorizontal();
                GUILayout.Space(spaceVertival);
                GUILayout.BeginHorizontal(GUILayout.Width(350));
                if (GUILayout.Button("Play Loop", Isplaying ? myStyle.BtSelected : myStyle.BtStandard, GUILayout.Width(buttonWidth * 0.666f)))
                    Isplaying = !Isplaying;
                if (GUILayout.Button("Play One Shot", myStyle.BtStandard, GUILayout.Width(buttonWidth * 0.666f)))
                    PlayOneNote();
                if (GUILayout.Button("Stop One Shot", myStyle.BtStandard, GUILayout.Width(buttonWidth * 0.666f)))
                    StopOneNote();
                if (GUILayout.Button("Clear", myStyle.BtStandard, GUILayout.Width(buttonWidth * 0.666f)))
                    midiStreamPlayer.MPTK_ClearAllSound(true);

                GUILayout.EndHorizontal();

                GUILayout.Space(spaceVertival);
                midiStreamPlayer.MPTK_Volume = Slider("Volume", midiStreamPlayer.MPTK_Volume, 0, 1);

                GUILayout.Space(spaceVertival);
                GUILayout.BeginHorizontal(GUILayout.Width(350));
                DelayTimeChange = Slider("Delay note", DelayTimeChange, 0.1f, 10f);
                GUILayout.EndHorizontal();

                GUILayout.Space(spaceVertival);
                GUILayout.BeginHorizontal(GUILayout.Width(500));
                StartNote = (int)Slider("Start note", StartNote, 0, 127);
                GUILayout.Space(spaceHorizontal);
                EndNote = (int)Slider("End note", EndNote, 0, 127);
                GUILayout.Space(spaceHorizontal);
                CurrentNote = (int)Slider("Current note", CurrentNote, 0, 127);
                GUILayout.EndHorizontal();

                GUILayout.Space(spaceVertival);
                GUILayout.BeginHorizontal(GUILayout.Width(350));
                midiStreamPlayer.MPTK_Transpose = (int)Slider("Transpose", midiStreamPlayer.MPTK_Transpose, -24, 24);
                GUILayout.Space(spaceHorizontal);
                Velocity = (int)Slider("Velocity", (int)Velocity, 0f, 127f);
                GUILayout.EndHorizontal();

                GUILayout.Space(spaceVertival);
                GUILayout.BeginHorizontal(GUILayout.Width(350));
                int panChange = (int)Slider("Panoramic", PanChange, 0, 127);
                if (panChange != PanChange)
                {
                    PanChange = panChange;
                    midiStreamPlayer.MPTK_PlayEvent(new MPTKEvent() { Command = MPTKCommand.ControlChange, Controller = MPTKController.Pan, Value = PanChange, Channel = StreamChannel });
                }

                GUILayout.Space(spaceHorizontal);
                midiStreamPlayer.ReverbMix = Slider("Reverb", midiStreamPlayer.ReverbMix, 0, 1);
                GUILayout.EndHorizontal();

                GUILayout.Space(spaceVertival);
                GUILayout.BeginHorizontal(GUILayout.Width(350));

                int modChange = (int)Slider("Modulation", ModChange, 0, 127);
                if (modChange != ModChange)
                {
                    ModChange = modChange;
                    midiStreamPlayer.MPTK_PlayEvent(new MPTKEvent() { Command = MPTKCommand.ControlChange, Controller = MPTKController.Modulation, Value = ModChange, Channel = StreamChannel });
                }
                GUILayout.Space(spaceHorizontal);
                int expChange = (int)Slider("Expression", ExpChange, 0, 127);
                if (expChange != ExpChange)
                {
                    ExpChange = expChange;
                    midiStreamPlayer.MPTK_PlayEvent(new MPTKEvent() { Command = MPTKCommand.ControlChange, Controller = MPTKController.Expression, Value = ExpChange, Channel = StreamChannel });
                }
                GUILayout.EndHorizontal();


                GUILayout.Space(spaceVertival);
                GUILayout.BeginHorizontal();
                RandomPlay = GUILayout.Toggle(RandomPlay, "   Random Play", GUILayout.Width(widthLabel));
                if (MidiPlayerGlobal.ImSFCurrent.DrumKitBankNumber >= 0)
                {
                    bool newDrumKit = GUILayout.Toggle(DrumKit, "   Drum Kit", GUILayout.Width(widthLabel));
                    if (newDrumKit != DrumKit)
                    {
                        DrumKit = newDrumKit;
                        if (DrumKit)
                            // Set canal to dedicated drum canal (9 if canal start from 0, canal 10 is displayed in log)
                            StreamChannel = 9;
                        else
                            StreamChannel = 0;
                        CurrentPatchInstrument = 0;
                    }
                }
                midiStreamPlayer.MPTK_WeakDevice = GUILayout.Toggle(midiStreamPlayer.MPTK_WeakDevice, "   Weak Device", GUILayout.Width(widthLabel));
                GUILayout.EndHorizontal();
                GUILayout.Space(spaceVertival);
                GUILayout.EndVertical();

                GUILayout.BeginVertical(myStyle.BacgDemos);
                GUILayout.Label("Go to your Hierarchy, select GameObject MidiStreamPlayer: inspector contains a lot of parameters to control the sound.", myStyle.TitleLabel2);
                GUILayout.EndVertical();

                GUILayout.EndScrollView();
            }
            else
            {
                GUILayout.Space(spaceVertival);
                GUILayout.Label("MidiStreamPlayer not defined, check hierarchy.", myStyle.TitleLabel3);
            }

        }
        //public GUISkin
        private float Slider(string title, float val, float min, float max)
        {
            float ret;
            GUILayout.BeginHorizontal();
            GUILayout.Label(title, myStyle.LabelRight, GUILayout.Width(widthLabel), GUILayout.Height(20));
            GUILayout.Label(Math.Round(val, 2).ToString(), myStyle.LabelRight, GUILayout.Width(30), GUILayout.Height(20));
            ret = GUILayout.HorizontalSlider(val, min, max, myStyle.SliderBar, myStyle.SliderThumb, GUILayout.Width(sliderwidth));
            GUILayout.EndHorizontal();
            return ret;
        }

        // Update is called once per frame
        //! [Example MPTK_PlayNotes]
        void Update()
        {
            // Checj that SoundFont is loaded and add a little wait (0.5 s by default) because Unity AudioSource need some time to be started
            if (!MidiPlayerGlobal.MPTK_IsReady())
                return;

            if (midiStreamPlayer != null && Isplaying)
            {
                float time = Time.realtimeSinceStartup - LastTimeChange;
                if (time > DelayTimeChange)
                {
                    // It's time to generate a note
                    LastTimeChange = Time.realtimeSinceStartup;

                    if (RandomPlay)
                    {
                        //
                        // First method to play notes: send a list of notes directly to the MidiStreamPlayer
                        // Useful for a long list of notes when the duration of the note is lnown.
                        //
                        List<MPTKEvent> notes = new List<MPTKEvent>();
                        // Very light random notes generator
                        if (!DrumKit)
                        {
                            // Play 3 notes with no delay
                            int rnd = UnityEngine.Random.Range(-8, 8);
                            notes.Add(CreateNote(60 + rnd, 0));
                            notes.Add(CreateNote(64 + rnd, 0));
                            notes.Add(CreateNote(67 + rnd, 0));
                        }
                        else
                        {
                            // Play 3 hit with a short delay
                            notes.Add(CreateDrum(UnityEngine.Random.Range(0, 127), 0));
                            notes.Add(CreateDrum(UnityEngine.Random.Range(0, 127), 150));
                            notes.Add(CreateDrum(UnityEngine.Random.Range(0, 127), 300));
                        }
                        // Send the note to the player. Notes are plays in a thread, so call returns immediately
                        midiStreamPlayer.MPTK_PlayEvent(notes);
                    }
                    else
                    {
                        //
                        // Second method to play and stop a notes: the duration is not known
                        // Here, a new note stop the previous
                        //
                        if (++CurrentNote > EndNote) CurrentNote = StartNote;
                        if (CurrentNote < StartNote) CurrentNote = StartNote;
                        PlayOneNote();
                    }
                }
            }
        }
        //! [Example MPTK_PlayNotes]

        private void PlayOneNote()
        {
            // Stop previous note playing
            StopOneNote();

            // Start playint a new note
            NotePlaying = new MPTKEvent()
            {
                Command = MPTKCommand.NoteOn,
                Value = CurrentNote,
                Channel = StreamChannel,
                Duration = 9999999, // 9999 seconds but stop by the new note. See before.
                Velocity = Velocity // Sound can vary depending on the velocity
            };
            midiStreamPlayer.MPTK_PlayEvent(NotePlaying);
        }

        private void StopOneNote()
        {
            if (NotePlaying != null)
            {
                //Debug.Log("Stop note");
                // Stop the note (method to simulate a real human on a keyboard : duration is not known when note is triggered)
                midiStreamPlayer.MPTK_StopEvent(NotePlaying);
                NotePlaying = null;
            }
        }

        /// <summary>
        /// Helper to create a random note (not yet used)
        /// </summary>
        /// <returns></returns>
        private MPTKEvent CreateRandomNote()
        {
            MPTKEvent note = new MPTKEvent()
            {
                Command = MPTKCommand.NoteOn,
                Value = 50 + UnityEngine.Random.Range(0, 4) * 2,
                Duration = UnityEngine.Random.Range(100, 1000),
                Velocity = Velocity,
            };
            return note;
        }

        /// <summary>
        /// Helper to create a note 
        /// </summary>
        /// <returns></returns>
        private MPTKEvent CreateNote(int key, float delay)
        {
            MPTKEvent note = new MPTKEvent()
            {
                Command = MPTKCommand.NoteOn,
                Value = key,
                Duration = DelayTimeChange * 1000f,
                Velocity = Velocity,
            };
            return note;
        }

        /// <summary>
        /// Helper to create a drum hit 
        /// </summary>
        /// <returns></returns>
        private MPTKEvent CreateDrum(int key, float delay)
        {
            MPTKEvent note = new MPTKEvent()
            {
                Command = MPTKCommand.NoteOn,
                Value = key,
                Duration = 0,
                Velocity = Velocity,
            };
            return note;
        }
    }
}