using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Events;
using System;
using System.Collections.ObjectModel;
using MEC;

namespace MidiPlayerTK
{
    /// <summary>
    /// Singleton class to manage all global features of MPTK.
    /// </summary>
    public class MidiPlayerGlobal : MonoBehaviour
    {
        private static MidiPlayerGlobal instance;
        private MidiPlayerGlobal()
        {
            HelperNoteLabel.Init();
        }
        public static MidiPlayerGlobal Instance { get { return instance; } }

        public const string SoundfontsDB = "SoundfontsDB";
        public const string MidiFilesDB = "MidiDB";
        public const string SongFilesDB = "SongDB";
        public const string ExtensionMidiFile = ".bytes";
        public const string ExtensionSoundFileDot = ".txt";
        public const string ExtensionSoundFileFileData = "_data.bytes";
        public const string FilenameMidiSet = "MidiSet";
        public const string PathSF2 = "SoundFont";
        public const string PathToWave = "wave";
        public const string ErrorNoSoundFont = "No SoundFont found. Add or Select SoundFont from the Unity menu 'MPTK' or alt-f";
        public const string ErrorNoMidiFile ="No Midi defined. Add Midi file from the Unity menu 'MPTK' or alt-m";
        public const string HelpDefSoundFont = "Add or Select SoundFont from the Unity menu 'MPTK' or alt-f";

        /// <summary>
        /// This path could change depending your project. Change the path before any actions in MPTK.
        /// </summary>
        static public string MPTK_PathToResources = "MidiPlayer/Resources/";

        // Initialized with InitPath()
        static public string PathToSoundfonts;
        static public string PathToMidiFile;
        static public string PathToMidiSet;


        private static TimeSpan timeToLoadSoundFont = TimeSpan.MaxValue;
        /// <summary>
        /// Load time for the current SoundFont
        /// </summary>
        public static TimeSpan MPTK_TimeToLoadSoundFont
        {
            get
            {
                return timeToLoadSoundFont;
            }
        }

        private static TimeSpan timeToLoadWave = TimeSpan.MaxValue;
        /// <summary>
        /// Load time for the wave
        /// </summary>
        public static TimeSpan MPTK_TimeToLoadWave
        {
            get
            {
                return timeToLoadWave;
            }
        }

        /// <summary>
        /// Count of preset loaded
        /// </summary>
        public static int MPTK_CountPresetLoaded
        {
            get
            {
                int count = 0;
                if (ImSFCurrent.DefaultBankNumber >= 0 && ImSFCurrent.DefaultBankNumber < ImSFCurrent.Banks.Length)
                    count = ImSFCurrent.Banks[ImSFCurrent.DefaultBankNumber].PatchCount;
                if (ImSFCurrent.DrumKitBankNumber >= 0 && ImSFCurrent.DrumKitBankNumber < ImSFCurrent.Banks.Length)
                    count += ImSFCurrent.Banks[ImSFCurrent.DrumKitBankNumber].PatchCount;
                return count;
            }
        }

        /// <summary>
        /// Count of wave loaded
        /// </summary>
        public static int MPTK_CountWaveLoaded;

        //! @cond NODOC
        /// <summary>
        /// Current SoundFont loaded
        /// </summary>
        public static ImSoundFont ImSFCurrent;
        //! @endcond

        /// <summary>
        /// Event triggered when Soundfont is loaded
        /// </summary>
        private UnityEvent InstanceOnEventPresetLoaded = new UnityEvent();

        /// <summary>
        /// Event triggered at end of loading a soundfont.
        /// Warning: when defined by script, this event is not triggered at first load of MPTK 
        /// because MidiPlayerGlobal is loaded before any other gamecomponent.
        /// Set this event in the Inspector of MidiPlayerGlobal to get at first load this information.
        /// </summary>
        public static UnityEvent OnEventPresetLoaded
        {
            get { return Instance != null ? Instance.InstanceOnEventPresetLoaded : null; }
            set { Instance.InstanceOnEventPresetLoaded = value; }
        }

        /// <summary>
        /// True if soundfont is loaded
        /// </summary>
        public static bool MPTK_SoundFontLoaded = false;

        //! @cond NODOC
        /// <summary>
        /// Current Midi Set loaded
        /// </summary>
        public static MidiSet CurrentMidiSet;
        //! @endcond

        private static string WavePath;
        private static AudioListener AudioListener;
        private static bool Initialized = false;

        public static void InitPath()
        {
            if (string.IsNullOrEmpty(MPTK_PathToResources))
                Debug.Log("MPTK_PathToResources not defined");
            else
            {
                PathToSoundfonts = MPTK_PathToResources + SoundfontsDB;
                PathToMidiFile = MPTK_PathToResources + MidiFilesDB;
                PathToMidiSet = MPTK_PathToResources + FilenameMidiSet + ExtensionSoundFileDot;
            }
        }

        void Awake()
        {
            InitPath();

            //Debug.Log("Awake MidiPlayerGlobal");
            if (instance != null && instance != this)
                Destroy(gameObject);    // remove previous instance
            else
            {
                //DontDestroyOnLoad(gameObject);
                instance = this;
                Timing.RunCoroutine(instance.InitThread());
            }
        }

        void OnApplicationQuit()
        {
            //Debug.Log("MPTK ending after" + Time.time + " seconds");
        }


        /// <summary>
        /// List of Soundfont(s) available
        /// </summary>
        public static List<string> MPTK_ListSoundFont
        {
            get
            {
                if (CurrentMidiSet != null && CurrentMidiSet.SoundFonts != null)
                {
                    List<string> sfNames = new List<string>();
                    foreach (SoundFontInfo sfi in CurrentMidiSet.SoundFonts)
                        sfNames.Add(sfi.Name);
                    return sfNames;
                }
                else
                    return null;
            }
        }

        /// <summary>
        /// List of midi(s) available
        /// </summary>
        public static List<MPTKListItem> MPTK_ListMidi;

        /// <summary>
        /// Get the list of presets available for instruments for the selected bank
        /// </summary>
        public static List<MPTKListItem> MPTK_ListPreset;

        /// <summary>
        /// Get the list of banks available 
        /// </summary>
        public static List<MPTKListItem> MPTK_ListBank;

        /// <summary>
        /// Get the list of presets available for instrument
        /// </summary>
        public static List<MPTKListItem> MPTK_ListPresetDrum;

        /// <summary>
        /// Get the list of presets available
        /// </summary>
        public static List<MPTKListItem> MPTK_ListDrum;

        /// <summary>
        /// Call by the first MidiPlayer awake
        /// </summary>
        //public static void Init()
        //{
        //    Instance.StartCoroutine(Instance.InitThread());
        //}

        /// <summary>
        /// Call by the first MidiPlayer awake
        /// </summary>
        private IEnumerator<float> InitThread()
        {
            if (!Initialized)
            {
                //Debug.Log("MidiPlayerGlobal InitThread");
                Initialized = true;
                ImSFCurrent = null;

                try
                {
                    AudioListener = Component.FindObjectOfType<AudioListener>();
                    if (AudioListener == null)
                    {
                        Debug.LogWarning("No audio listener found. Add one and only one AudioListener component to your hierarchy.");
                        //return;
                    }
                }
                catch (System.Exception ex)
                {
                    MidiPlayerGlobal.ErrorDetail(ex);
                }

                try
                {
                    AudioListener[] listeners = Component.FindObjectsOfType<AudioListener>();
                    if (listeners != null && listeners.Length > 1)
                    {
                        Debug.LogWarning("More than one audio listener found. Some unexpected behaviors could happen.");
                    }
                }
                catch (System.Exception ex)
                {
                    MidiPlayerGlobal.ErrorDetail(ex);
                }

                try
                {
                    LoadMidiSetFromRsc();
                    DicAudioClip.Init();
                }
                catch (System.Exception ex)
                {
                    MidiPlayerGlobal.ErrorDetail(ex);
                }

                if (CurrentMidiSet == null)
                {
                    Debug.LogWarning(MidiPlayerGlobal.ErrorNoMidiFile);
                    yield return Timing.WaitForOneFrame;
                }
                else if (CurrentMidiSet.ActiveSounFontInfo == null)
                {
                    Debug.Log(MidiPlayerGlobal.ErrorNoSoundFont);
                    yield return Timing.WaitForOneFrame;
                }

                BuildMidiList();
                LoadCurrentSF();
            }
        }

        private static float startupdate = float.MinValue;

        /// <summary>
        /// Check if SoudFont is loaded. Add a default wait time because Unity AudioSource need a delay to be really ready to play. Hummm, like a diesel motor ?
        /// </summary>
        /// <param name="delay"></param>
        /// <returns></returns>
        public static bool MPTK_IsReady(float delay = 0.5f)
        {
            if (startupdate < 0f)
                startupdate = Time.realtimeSinceStartup;
            if (!MPTK_SoundFontLoaded)
                return false;
            //Debug.Log(Time.realtimeSinceStartup);
            if (Time.realtimeSinceStartup - startupdate < 0.5f)
                return false;
            return true;
        }
        /// <summary>
        /// Changing the current Soundfont on fly. If some Midis are playing they are restarted.
        /// </summary>
        /// <param name="name">SoundFont name</param>
        public static void MPTK_SelectSoundFont(string name)
        {
            if (Application.isPlaying)
                Timing.RunCoroutine(SelectSoundFontThread(name));
            else
                SelectSoundFont(name);
        }

        /// <summary>
        /// Change default current bank on fly
        /// </summary>
        /// <param name="nbank">Number of the SoundFont Bank to load for instrument.</param>
        public static void MPTK_SelectBankInstrument(int nbank)
        {
            if (nbank >= 0 && nbank < ImSFCurrent.Banks.Length)
                if (ImSFCurrent.Banks[nbank] != null)
                {
                    ImSFCurrent.DefaultBankNumber = nbank;
                    BuildPresetList(true);
                }
                else
                    Debug.LogWarningFormat("MPTK_SelectBankInstrument: bank {0} is not defined", nbank);
            else
                Debug.LogWarningFormat("MPTK_SelectBankInstrument: bank {0} outside of range", nbank);
        }

        /// <summary>
        /// Change current bank on fly
        /// </summary>
        /// <param name="nbank">Number of the SoundFont Bank to load for drum.</param>
        public static void MPTK_SelectBankDrum(int nbank)
        {
            if (nbank >= 0 && nbank < ImSFCurrent.Banks.Length)
                if (ImSFCurrent.Banks[nbank] != null)
                {
                    ImSFCurrent.DrumKitBankNumber = nbank;
                    BuildPresetList(false);
                    //BuildDrumList();
                }
                else
                    Debug.LogWarningFormat("MPTK_SelectBankDrum: bank {0} is not defined", nbank);
            else
                Debug.LogWarningFormat("MPTK_SelectBankDrum: bank {0} outside of range", nbank);
        }

        /// <summary>
        /// Select and load a SF when playing
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static IEnumerator<float> SelectSoundFontThread(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                int index = CurrentMidiSet.SoundFonts.FindIndex(s => s.Name == name);
                if (index >= 0)
                {
                    MidiPlayerGlobal.CurrentMidiSet.SetActiveSoundFont(index);
                    MidiPlayerGlobal.CurrentMidiSet.Save();
                }
                else
                {
                    Debug.LogWarning("SoundFont not found: " + name);
                    yield return 0;
                }
            }
            // Load selected soundfont
            yield return Timing.WaitUntilDone(Timing.RunCoroutine(LoadSoundFontThread()));
        }

        /// <summary>
        /// Select and load a SF when editor
        /// </summary>
        /// <param name="name"></param>
        private static void SelectSoundFont(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                int index = CurrentMidiSet.SoundFonts.FindIndex(s => s.Name == name);
                if (index >= 0)
                {
                    MidiPlayerGlobal.CurrentMidiSet.SetActiveSoundFont(index);
                    MidiPlayerGlobal.CurrentMidiSet.Save();
                    // Load selected soundfont
                    LoadSoundFont();
                }
                else
                {
                    Debug.LogWarning("SoundFont not found " + name);
                }
            }
        }

        /// <summary>
        /// Loading of a SoundFonttwhen playing using a thread
        /// </summary>
        /// <returns></returns>
        private static IEnumerator<float> LoadSoundFontThread()
        {
            if (MidiPlayerGlobal.ImSFCurrent != null)
            {
                //Debug.Log("Load MidiPlayerGlobal.ImSFCurrent: " + MidiPlayerGlobal.ImSFCurrent.SoundFontName);
                //Debug.Log("Load CurrentMidiSet.ActiveSounFontInfo: " + CurrentMidiSet.ActiveSounFontInfo.Name);

                MidiPlayer[] midiplayers = FindObjectsOfType<MidiPlayer>();
                MPTK_SoundFontLoaded = false;
                if (Application.isPlaying)
                {
                    if (midiplayers != null)
                    {
                        foreach (MidiPlayer mp in midiplayers)
                        {
                            if (mp is MidiFilePlayer)
                            {
                                MidiFilePlayer mfp = (MidiFilePlayer)mp;
                                if (!mfp.MPTK_IsPaused)
                                    mfp.MPTK_Pause();
                                yield return Timing.WaitUntilDone(Timing.RunCoroutine(mp.ThreadClearAllSound(true)));
                            }
                        }
                    }
                    DicAudioClip.Init();
                }
                LoadCurrentSF();

                //Debug.Log("   Time To Load SoundFont: " + Math.Round(MidiPlayerGlobal.MPTK_TimeToLoadSoundFont.TotalSeconds, 3).ToString() + " second");
                //Debug.Log("   Time To Load Waves: " + Math.Round(MidiPlayerGlobal.MPTK_TimeToLoadWave.TotalSeconds, 3).ToString() + " second");

                if (midiplayers != null)
                {
                    foreach (MidiPlayer mp in midiplayers)
                    {
                        if (mp is MidiFilePlayer)
                        {
                            MidiFilePlayer mfp = (MidiFilePlayer)mp;
                            if (mfp.MPTK_IsPaused)
                            {
                                mfp.MPTK_InitSynth();
                                mfp.MPTK_RePlay();
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Load a SF when editor
        /// </summary>
        private static void LoadSoundFont()
        {
            if (MidiPlayerGlobal.ImSFCurrent != null)
            {
                Debug.Log("Load MidiPlayerGlobal.ImSFCurrent: " + MidiPlayerGlobal.ImSFCurrent.SoundFontName);
                Debug.Log("Load CurrentMidiSet.ActiveSounFontInfo: " + CurrentMidiSet.ActiveSounFontInfo.Name);

                LoadCurrentSF();
                Debug.Log("Time To Load SoundFont: " + Math.Round(MidiPlayerGlobal.MPTK_TimeToLoadSoundFont.TotalSeconds, 3).ToString() + " second");
                if (Application.isPlaying)
                    Debug.Log("Time To Load Waves: " + Math.Round(MidiPlayerGlobal.MPTK_TimeToLoadWave.TotalSeconds, 3).ToString() + " second");
            }
        }

        //! @cond NODOC

        /// <summary>
        /// Core function to load a SF when playing or from editor from the Unity asset
        /// </summary>
        public static void LoadCurrentSF()
        {
            MPTK_SoundFontLoaded = false;
            // Load simplfied soundfont
            try
            {
                DateTime start = DateTime.Now;
                if (CurrentMidiSet == null)
                {
                    Debug.Log(MidiPlayerGlobal.ErrorNoSoundFont);
                }
                else
                {
                    SoundFontInfo sfi = CurrentMidiSet.ActiveSounFontInfo;
                    if (sfi == null)
                        Debug.Log(MidiPlayerGlobal.ErrorNoSoundFont);
                    else
                    {
                        //Debug.Log("Start loading " + sfi.Name);

                        // Path to the soundfonts directory for this SF, start from resource folder
                        string pathToImSF = Path.Combine(SoundfontsDB + "/", sfi.Name);

                        WavePath = Path.Combine(pathToImSF + "/", PathToWave);
                        // Load all presets defined in the sf
                        ImSFCurrent = ImSoundFont.Load(pathToImSF, sfi.Name);

                        // Add
                        if (ImSFCurrent == null)
                        {
                            Debug.LogWarning("Error loading " + sfi.Name ?? "name not defined");
                        }
                        else
                        {
                            BuildBankList();
                            BuildPresetList(true);
                            BuildPresetList(false);
                            //BuildDrumList();
                            timeToLoadSoundFont = DateTime.Now - start;

                            //Debug.Log("End loading SoundFont " + timeToLoadSoundFont.TotalSeconds + " seconds");

                        }

                    }
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }

            if (ImSFCurrent == null)
            {
                Debug.LogWarning("SoundFont not loaded.");
                return;
            }

            // Load samples only in run mode
            if (Application.isPlaying)
            {
                try
                {
                    MPTK_CountWaveLoaded = 0;
                    DateTime start = DateTime.Now;
                    HiLoadSamples();
                    timeToLoadWave = DateTime.Now - start;
                    //Debug.Log("End loading Waves " + timeToLoadWave.TotalSeconds + " seconds");
                }
                catch (System.Exception ex)
                {
                    MidiPlayerGlobal.ErrorDetail(ex);
                }
            }
            if (ImSFCurrent != null)
                MPTK_SoundFontLoaded = true;

            if (OnEventPresetLoaded != null) OnEventPresetLoaded.Invoke();
        }
        //! @endcond

        /// <summary>
        /// Load samples associated to a patch for High SF
        /// </summary>
        private static void HiLoadSamples()
        {
            try
            {
                float start = Time.realtimeSinceStartup;
                //Debug.Log(">>> Load Sample");
                if (ImSFCurrent != null)
                {
                    foreach (HiSample smpl in ImSFCurrent.HiSf.Samples)
                    {
                        if (smpl.Name != null)
                        {
                            if (!DicAudioClip.Exist(smpl.Name))
                            {
                                string path = WavePath + "/" + Path.GetFileNameWithoutExtension(smpl.Name);// + ".wav";
                                AudioClip ac = Resources.Load<AudioClip>(path);
                                if (ac != null)
                                {
                                    //Debug.Log("Wave load " + path);
                                    DicAudioClip.Add(smpl.Name, ac);
                                    MPTK_CountWaveLoaded++;
                                }
                                //else Debug.LogWarning("Wave " + smpl.WaveFile + " not found");
                            }
                        }
                    }
                }
                else Debug.Log("SoundFont not loaded ");
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        /// <summary>
        /// Build list of presets found in the SoundFont
        /// </summary>
        static private void BuildBankList()
        {
            MPTK_ListBank = new List<MPTKListItem>();
            try
            {
                //Debug.Log(">>> Load Preset - b:" + ibank + " p:" + ipatch);
                if (ImSFCurrent != null && CurrentMidiSet != null)
                {
                    foreach (ImBank bank in ImSFCurrent.Banks)
                    {
                        if (bank != null)
                            MPTK_ListBank.Add(new MPTKListItem() { Index = bank.BankNumber, Label = "Bank " + bank.BankNumber });
                        else
                            MPTK_ListBank.Add(null);
                    }
                }
                else
                {
                    Debug.Log(MidiPlayerGlobal.ErrorNoSoundFont);
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        /// <summary>
        /// Build list of presets found in the SoundFont
        /// </summary>
        static private void BuildPresetList(bool forInstrument)
        {
            List<MPTKListItem> presets = new List<MPTKListItem>();
            try
            {
                //Debug.Log(">>> Load Preset - b:" + ibank + " p:" + ipatch);
                if (ImSFCurrent != null)
                {
                    if ((forInstrument && ImSFCurrent.DefaultBankNumber >= 0 && ImSFCurrent.DefaultBankNumber < ImSFCurrent.Banks.Length) ||
                       (!forInstrument && ImSFCurrent.DrumKitBankNumber >= 0 && ImSFCurrent.DrumKitBankNumber < ImSFCurrent.Banks.Length))
                    {
                        int ibank = forInstrument ? ImSFCurrent.DefaultBankNumber : ImSFCurrent.DrumKitBankNumber;
                        if (ImSFCurrent.Banks[ibank] != null)
                        {
                            ImSFCurrent.Banks[ibank].PatchCount = 0;
                            for (int ipreset = 0; ipreset < ImSFCurrent.Banks[ibank].defpresets.Length; ipreset++)
                            {
                                HiPreset p = ImSFCurrent.Banks[ibank].defpresets[ipreset];
                                if (p != null)
                                {
                                    presets.Add(new MPTKListItem() { Index = p.Num, Label = p.Num + " - " + p.Name });
                                    ImSFCurrent.Banks[ibank].PatchCount++;
                                }
                                //else
                                //    presets.Add(null);
                            }
                        }
                        else
                        {
                            Debug.LogWarningFormat("Default bank {0} for {1} is not defined.", ibank, forInstrument ? "Instrument" : "Drum");
                        }
                    }
                    else
                    {
                        Debug.LogWarningFormat("No default bank defined for {0}.", forInstrument ? "Instrument" : "Drum");
                    }

                    // Global count
                    //ImSFCurrent.PatchCount = 0;
                    foreach (ImBank bank in ImSFCurrent.Banks)
                    {
                        if (bank != null)
                        {
                            bank.PatchCount = 0;
                            foreach (HiPreset preset in bank.defpresets)
                            {
                                if (preset != null)
                                {
                                    // Bank count
                                    bank.PatchCount++;
                                }
                            }
                            //ImSFCurrent.PatchCount += bank.PatchCount;
                        }
                    }
                }
                else
                {
                    Debug.Log(MidiPlayerGlobal.ErrorNoSoundFont);
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
            if (forInstrument)
                MPTK_ListPreset = presets;
            else
                MPTK_ListPresetDrum = presets;
        }

        public static string MPTK_GetPatchName(int bank, int patch)
        {
            string name = "";
            if (ImSFCurrent != null && ImSFCurrent.Banks != null)
            {
                if (bank >= 0 && bank < ImSFCurrent.Banks.Length && ImSFCurrent.Banks[bank] != null)
                {
                    if (ImSFCurrent.Banks[bank].defpresets != null)
                    {
                        if (patch >= 0 && patch < ImSFCurrent.Banks[bank].defpresets.Length && ImSFCurrent.Banks[bank].defpresets[patch] != null)
                            name = ImSFCurrent.Banks[bank].defpresets[patch].Name;
                    }
                }
            }
            return name;
        }

        //! @cond NODOC
        public static void BuildMidiList()
        {
            MPTK_ListMidi = new List<MPTKListItem>();
            if (CurrentMidiSet != null && CurrentMidiSet.MidiFiles != null)
                foreach (string name in CurrentMidiSet.MidiFiles)
                    MPTK_ListMidi.Add(new MPTKListItem() { Index = MPTK_ListMidi.Count, Label = name });
            //Debug.Log("Midi file list loaded: " + MPTK_ListMidi.Count);
        }
        //! @endcond

        /// <summary>
        /// Find index of a Midi by name. Use the exact name defined in Unity resources folder MidiDB without any path or extension.
        /// Tips: Add Midi files to your project with the Unity menu MPTK or add it directly in the ressource folder and open Midi File Setup to automatically integrate Midi in MPTK.
        /// </summary>
        /// <param name="name">name of the midi without path nor extension</param>
        /// <returns>-1 if not found else return the index of the midi.</returns>
        public static int MPTK_FindMidi(string name)
        {
            int index = -1;
            try
            {
                if (!string.IsNullOrEmpty(name))
                    if (CurrentMidiSet != null && CurrentMidiSet.MidiFiles != null)
                        index = CurrentMidiSet.MidiFiles.FindIndex(s => s == name);

            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
            return index;
        }

        /// <summary>
        /// Calculate distance with the AudioListener.
        /// </summary>
        /// <param name="trf">Transform of the object to calculate the distance.</param>
        /// <returns></returns>
        public static float MPTK_DistanceToListener(Transform trf)
        {
            float distance = 0f;
            try
            {
                if (AudioListener != null)
                {
                    distance = Vector3.Distance(AudioListener.transform.position, trf.position);
                    //Debug.Log("Camera:" + AudioListener.name + " " + distance);
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }

            return distance;
        }

        /// <summary>
        /// Load setup MPTK from resource
        /// </summary>
        private static void LoadMidiSetFromRsc()
        {
            try
            {
                TextAsset sf = Resources.Load<TextAsset>(MidiPlayerGlobal.FilenameMidiSet);
                if (sf == null)
                    Debug.LogWarning(MidiPlayerGlobal.ErrorNoMidiFile);
                else
                {
                    //UnityEngine.Debug.Log(sf.text);
                    CurrentMidiSet = MidiSet.LoadRsc(sf.text);
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        //! @cond NODOC
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

        public static void ErrorDetail(System.Exception ex)
        {
            Debug.LogWarning("MPTK Error " + ex.Message);
            Debug.LogWarning("   " + ex.TargetSite ?? "");
            var st = new System.Diagnostics.StackTrace(ex, true);
            if (st != null)
            {
                var frames = st.GetFrames();
                if (frames != null)
                {
                    foreach (var frame in frames)
                    {
                        if (frame.GetFileLineNumber() < 1)
                            continue;
                        Debug.LogWarning("   " + frame.GetFileName() + " " + frame.GetMethod().Name + " " + frame.GetFileLineNumber());
                    }
                }
                else
                    Debug.LogWarning("   " + ex.StackTrace ?? "");
            }
            else
                Debug.LogWarning("   " + ex.StackTrace ?? "");
        }
        //! @endcond
    }
}
