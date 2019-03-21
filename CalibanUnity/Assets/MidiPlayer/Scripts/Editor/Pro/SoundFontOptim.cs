#define LOG_SF
using MidiPlayerTK;
using NAudio.Midi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace MidiPlayerTK
{
    /// <summary>
    /// SoundFont adapted to Unity
    /// </summary>
    static public class SoundFontOptim
    {
        private static string lastDirectory = "";
        /// <summary>
        /// Add a new SoundFont from PC
        /// </summary>
        public static void AddSoundFont(this SoundFontSetupWindow sfsw)
        {
            try
            {
                string path = EditorUtility.OpenFilePanel("Open and import SoundFont", lastDirectory, "sf2");
                if (!string.IsNullOrEmpty(path))
                {
                    lastDirectory = Path.GetDirectoryName(path);
                    string soundFontName = Path.GetFileNameWithoutExtension(path);
                    string pathSF2Save = Path.Combine(Application.persistentDataPath, MidiPlayerGlobal.PathSF2);
                    if (!Directory.Exists(pathSF2Save))
                        Directory.CreateDirectory(pathSF2Save);
                    pathSF2Save = Path.Combine(pathSF2Save, soundFontName + ".sf2");
                    // Create a copy of the SF2 for future action
                    File.Copy(path, pathSF2Save, true);
                    SoundFontSetupWindow.LogInfo.Add("Copy SoundFont to " + pathSF2Save);
                    // Build path to IMSF folder 
                    string imSFPath = Path.Combine(Application.dataPath + "/", MidiPlayerGlobal.PathToSoundfonts);
                    imSFPath = Path.Combine(imSFPath, soundFontName);
                    if (!Directory.Exists(imSFPath))
                        Directory.CreateDirectory(imSFPath);

                    //
                    // Load SF2 and build ImSF
                    // -----------------------
                    MidiPlayerGlobal.ImSFCurrent = new ImSoundFont();

                    // By default keep all banks when first loading of SF
                    MidiPlayerGlobal.ImSFCurrent.SelectAllBanks();

                    ExtractFromOriginalSoundFont(MidiPlayerGlobal.ImSFCurrent, path, SoundFontSetupWindow.LogInfo);

                    // Search default bank to select
                    MidiPlayerGlobal.ImSFCurrent.DefaultBankNumber = MidiPlayerGlobal.ImSFCurrent.FirstBank();
                    MidiPlayerGlobal.ImSFCurrent.DrumKitBankNumber = MidiPlayerGlobal.ImSFCurrent.LastBank();

                    // Is this sf already exists in MPTK Config ?
                    int indexSF = MidiPlayerGlobal.CurrentMidiSet.SoundFonts.FindIndex(s => s.Name == MidiPlayerGlobal.ImSFCurrent.SoundFontName);

                    if (indexSF < 0)
                    {
                        // SF not exists
                        SoundFontInfo sfi = new SoundFontInfo()
                        {
                            Name = MidiPlayerGlobal.ImSFCurrent.SoundFontName,
                            SF2Path = pathSF2Save,
                        };
                        MidiPlayerGlobal.CurrentMidiSet.SoundFonts.Add(sfi);
                        indexSF = MidiPlayerGlobal.CurrentMidiSet.SoundFonts.Count - 1;
                    }
                    MidiPlayerGlobal.CurrentMidiSet.SetActiveSoundFont(indexSF);
                    
                    // Save MPTK SoundFont : xml + binary
                    sfsw.SaveCurrentIMSF(false);

                    // Save MPTK config: list of SoundFont and Midi
                    MidiPlayerGlobal.CurrentMidiSet.Save();

                    AssetDatabase.Refresh();
                }
            }
            catch (Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        /// <summary>
        /// Optimize selected SoundFont
        /// </summary>
        public static void OptimizeSoundFont(this SoundFontSetupWindow sfsw)
        {
            try
            {
                if (MidiPlayerGlobal.ImSFCurrent != null)
                {
                    MidiPlayerGlobal.CurrentMidiSet.ActiveSounFontInfo.PatchCount = 0;
                    MidiPlayerGlobal.CurrentMidiSet.ActiveSounFontInfo.WaveCount = 0;
                    MidiPlayerGlobal.CurrentMidiSet.ActiveSounFontInfo.WaveSize = 0;
                    ImSoundFont imsf = MidiPlayerGlobal.ImSFCurrent;
                    TPatchUsed filters = null;
                    if (!SoundFontSetupWindow.KeepAllPatchs)
                        filters = CreateFiltersFromMidiList(SoundFontSetupWindow.LogInfo, SoundFontSetupWindow.KeepAllPatchs, SoundFontSetupWindow.KeepAllZones);
                    // Build path to wave
                    string pathToWave = Path.Combine(Application.dataPath + "/", MidiPlayerGlobal.PathToSoundfonts);
                    pathToWave = Path.Combine(pathToWave + "/", imsf.SoundFontName);
                    pathToWave = Path.Combine(pathToWave + "/", MidiPlayerGlobal.PathToWave);

                    // Load original SF2 and build ImSF
                    List<string> waveSelected = new List<string>();

                    // Reload original soundfont and convert to the full format
                    ExtractFromOriginalSoundFont(
                       imsf,
                        MidiPlayerGlobal.CurrentMidiSet.ActiveSounFontInfo.SF2Path,
                        SoundFontSetupWindow.LogInfo,
                        pathToWave,
                        filters,
                        waveSelected);

                    // Remove useless wave and calculate stat
                    if (Directory.Exists(pathToWave))
                    {
                        string[] waveFiles = Directory.GetFiles(pathToWave, "*.wav", SearchOption.AllDirectories);
                        // Remove unused wave
                        if (SoundFontSetupWindow.RemoveUnusedWaves)
                            foreach (string waveFile in waveFiles)
                            {
                                string name = Path.GetFileNameWithoutExtension(waveFile);
                                bool found = false;
                                foreach (string selected in waveSelected)
                                {
                                    if (selected.Contains(name))
                                    {
                                        found = true;
                                        break;
                                    }
                                }
                                if (!found)
                                {
                                    DeleteResource(waveFile);
                                    Debug.Log("delete " + waveFile);
                                }
                            }

                        // Calculate size of wave
                        waveFiles = Directory.GetFiles(pathToWave, "*.wav", SearchOption.AllDirectories);
                        long size = 0;
                        foreach (string waveFile in waveFiles)
                            size += new FileInfo(waveFile).Length;
                        //imsf.WaveSize = size;
                        //imsf.WaveCount = waveFiles.Length;

                        MidiPlayerGlobal.CurrentMidiSet.ActiveSounFontInfo.WaveSize = size;
                        MidiPlayerGlobal.CurrentMidiSet.ActiveSounFontInfo.WaveCount = waveFiles.Length;

                        SoundFontSetupWindow.LogInfo.Add("Wave saved");
                        SoundFontSetupWindow.LogInfo.Add("   Count = " + waveFiles.Length);
                        if (size < 1000000)
                            SoundFontSetupWindow.LogInfo.Add("   Size = " + Math.Round((double)size / 1000d, 2) + " Ko");
                        else
                            SoundFontSetupWindow.LogInfo.Add("   Size = " + Math.Round((double)size / 1000000d, 2) + " Mo");
                    }
                    else
                        SoundFontSetupWindow.LogInfo.Add("/!\\ No wave created, midifile will not played /!\\ ");

                    // Add some information to imsf
                    SoundFontSetupWindow.LogInfo.Add("Save SoundFont");
                    //Optim.DebugImSf(MidiPlayerGlobal.ImSFCurrent, 0, 48, true, true);

                    // Remove patch and bank discarded from sf
                    for (int p = 0; p < imsf.HiSf.preset.Length; p++)
                    {
                        HiPreset preset = imsf.HiSf.preset[p];
                        if (!imsf.BankSelected[preset.Bank])
                            imsf.HiSf.preset[p] = null;
                    }

                    foreach (ImBank bank in imsf.Banks)
                        if (bank != null)
                            MidiPlayerGlobal.CurrentMidiSet.ActiveSounFontInfo.PatchCount += bank.PatchCount;

                    // Save MPTK SoundFont : xml + binary
                    sfsw.SaveCurrentIMSF(false);

                    // Save MPTK config: list of SoundFont and Midi
                    MidiPlayerGlobal.CurrentMidiSet.Save();

                    SoundFontSetupWindow.LogInfo.Add("View logs and soundfont detail here : " + Application.persistentDataPath);

                    if (SoundFontSetupWindow.LogDetailSoundFont)
                        SFFile.DumpSFToFile(MidiPlayerGlobal.ImSFCurrent.HiSf, Path.Combine(Application.persistentDataPath, MidiPlayerGlobal.ImSFCurrent.SoundFontName + MidiPlayerGlobal.ExtensionSoundFileDot));

                    // refresh for new wave
                    AssetDatabase.Refresh();
                    MidiPlayerGlobal.LoadCurrentSF();
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        private static TPatchUsed CreateFiltersFromMidiList(BuilderInfo OptimInfo, bool KeepAllPatchs, bool KeepAllZones)
        {
            //ImSfDebug.SortImSf(MidiPlayerGlobal.ImSFCurrent, -1, -1, true, true);
            //return;
            OptimInfo.Add("Optimize " + MidiPlayerGlobal.ImSFCurrent.SoundFontName);

            TPatchUsed filters;
            if (!KeepAllPatchs)
            {
                OptimInfo.Add("Scan Midifile");
                // Calculate patchs to keep depending patch used by midi files
                filters = MidiOptim.PatchUsed(OptimInfo);
                if (filters == null)
                    return null;
            }
            else
            {
                // Select all patchs 
                filters = new TPatchUsed();
                filters.DefaultBankNumber = MidiPlayerGlobal.ImSFCurrent.DefaultBankNumber;
                filters.DrumKitBankNumber = MidiPlayerGlobal.ImSFCurrent.DrumKitBankNumber;
                if (filters.DefaultBankNumber >= 0 && filters.DefaultBankNumber < filters.BankUsed.Length)
                    filters.BankUsed[filters.DefaultBankNumber] = new TBankUsed();
                if (filters.DrumKitBankNumber >= 0 && filters.DrumKitBankNumber < filters.BankUsed.Length)
                    filters.BankUsed[filters.DrumKitBankNumber] = new TBankUsed();
            }

            // Uncomment for testing purpose
            //int patch = 0;
            //filters.BankUsed[0] = new TBankUsed();
            //filters.BankUsed[0].PatchUsed[patch] = new TNoteUsed();
            //for (int n = 0; n < 128; n++)
            //    filters.BankUsed[0].PatchUsed[patch].Note[n] = 1;

            if (KeepAllPatchs)
            {
                OptimInfo.Add("Keep all patchs");
                for (int b = 0; b < filters.BankUsed.Length; b++)
                {
                    TBankUsed bank = filters.BankUsed[b];
                    if (bank != null)
                    {
                        int patchEnd = bank.PatchUsed.Length - 1;
                        // If drum kit bank, keep only patch 0
                        if (filters.DefaultBankNumber != filters.DrumKitBankNumber && filters.DrumKitBankNumber != -1 && b == filters.DrumKitBankNumber)
                            patchEnd = 0;

                        // For each patch, use all notes
                        for (int p = 0; p <= patchEnd; p++)
                        {
                            if (bank.PatchUsed[p] == null)
                                bank.PatchUsed[p] = new TNoteUsed();
                            for (int n = 0; n < bank.PatchUsed[p].Note.Length; n++)
                                bank.PatchUsed[p].Note[n] = 1;
                        }
                    }
                }
            }

            if (KeepAllZones)
            {
                OptimInfo.Add("Keep all zones (notes and velocities) for selected patch");

                foreach (TBankUsed bank in filters.BankUsed)
                {
                    if (bank != null)
                    {
                        // For each patch, use all notes
                        foreach (TNoteUsed patch in bank.PatchUsed)
                        {
                            if (patch != null)
                                for (int n = 0; n < patch.Note.Length; n++)
                                    patch.Note[n]++;
                        }
                    }
                }
            }

            if (!KeepAllZones)
            {
                OptimInfo.Add("Remove unused patch");

                // Display bank, patch to filters
                int indexBank = 0;
                foreach (TBankUsed bank in filters.BankUsed)
                {
                    if (bank != null)
                    {
                        int indexPatch = 0;
                        foreach (TNoteUsed patch in bank.PatchUsed)
                        {
                            if (patch != null)
                                OptimInfo.Add(string.Format("   Keep bank/patch [{0:000},{1:000}] {2}", indexBank, indexPatch, GetPatchName(indexBank, indexPatch, MidiPlayerGlobal.ImSFCurrent.DrumKitBankNumber)));
                            indexPatch++;
                        }
                    }
                    indexBank++;
                }
            }
            return filters;
        }

        static private void DeleteResource(string filepath)
        {
            try
            {
                Debug.Log("Delete " + filepath);
                File.Delete(filepath);
                // delete also meta
                string meta = filepath + ".meta";
                Debug.Log("Delete " + meta);
                File.Delete(meta);

            }
            catch (Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        public static string CheckAndSetBank(this SoundFontSetupWindow sfsw)
        {
            try
            {
                if (MidiPlayerGlobal.ImSFCurrent.DefaultBankNumber == -1)
                    return "Error: no default bank defined for instrument";
                else
                {
                    MidiPlayerGlobal.ImSFCurrent.BankSelected[MidiPlayerGlobal.ImSFCurrent.DefaultBankNumber] = true;
                }

                if (MidiPlayerGlobal.ImSFCurrent.DrumKitBankNumber == -1)
                    return "Warning: no default bank defined for drum, ";
                else
                {
                    MidiPlayerGlobal.ImSFCurrent.BankSelected[MidiPlayerGlobal.ImSFCurrent.DrumKitBankNumber] = true;
                }
            }
            catch (Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
            return null;
        }

        /// <summary>
        ///Save MPTK SoundFont : xml + binary
        /// </summary>
        public static void SaveCurrentIMSF(this SoundFontSetupWindow sfsw, bool onlyXML)
        {
            try
            {
                string soundPath = Path.Combine(Application.dataPath + "/", MidiPlayerGlobal.PathToSoundfonts);
                soundPath = Path.Combine(soundPath + "/", MidiPlayerGlobal.CurrentMidiSet.ActiveSounFontInfo.Name);
                MidiPlayerGlobal.ImSFCurrent.Save(soundPath, MidiPlayerGlobal.CurrentMidiSet.ActiveSounFontInfo.Name, onlyXML);
            }
            catch (Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        static private string GetPatchName(int ib, int ip, int bankdk)
        {
            if (ib == bankdk)
                return "Drum Kit";
            else
                return PatchChangeEvent.GetPatchName(ip);
        }


        /// <summary>
        /// Create a simplified soundfont from a SF2 file
        /// </summary>
        /// <param name="pathSF"></param>
        /// <param name="wavePath"></param>
        /// <param name="filter "></param>
        /// <param name="info"></param>
        /// <param name="waveSelected"></param>
        /// <param name="addEvenIfNoKeyVel"></param>
        public static void ExtractFromOriginalSoundFont(
            ImSoundFont imsf,
            string pathSF,
            BuilderInfo LogInfo,
            string wavePath = null,
            TPatchUsed filter = null,
            List<string> waveSelected = null,
            bool addEvenIfNoKeyVel = false)
        {
            try
            {
                SFLoad load = new SFLoad(pathSF, SFFile.SfSource.SF2);
                imsf.HiSf = load.SfData;
                imsf.SoundFontName = Path.GetFileNameWithoutExtension(pathSF);
                LogInfo.Add("Load original SoundFont from:");
                LogInfo.Add(pathSF);
                imsf.Banks = new ImBank[ImSoundFont.MAXBANKPRESET];
                LoadBanks(imsf, filter, LogInfo);
                // Extract wave
                ExtractWave(imsf, wavePath, waveSelected, LogInfo);
                CountPatch(imsf);
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        private static void ExtractWave(ImSoundFont imsf, string wavePath, List<string> waveSelected, BuilderInfo LogInfo)
        {
            if (imsf != null && !string.IsNullOrEmpty(wavePath))
            {
                CreateWave createwave = new CreateWave();

                foreach (ImBank b in imsf.Banks)
                {
                    if (b != null)
                    {
                        foreach (HiPreset p in b.defpresets)
                        {
                            if (p != null)
                            {
                                foreach (HiZone pzone in p.Zone)
                                {
                                    HiInstrument inst = null;
                                    if (pzone.Index >= 0)
                                        inst = imsf.HiSf.inst[pzone.Index];

                                    if (inst != null)
                                    {
                                        foreach (HiZone izone in inst.Zone)
                                        {
                                            if (izone.Index >= 0)
                                            {
                                                HiSample sample = imsf.HiSf.Samples[izone.Index];
                                                if (!string.IsNullOrEmpty(sample.Name))
                                                {
                                                    string path = System.IO.Path.Combine(wavePath, sample.Name) + ".wav";
                                                    if (!File.Exists(path))
                                                    {
                                                        LogInfo.Add("   Add a new wave " + sample.Name);
                                                        createwave.Extract(path, sample, imsf.HiSf.SampleData);
                                                    }
                                                    //else if (LogToUnity != null)
                                                    //    LogToUnity.Add("   Wave already exists " + sample.Name);

                                                    if (waveSelected != null)
                                                        waveSelected.Add(sample.Name);
                                                    //Debug.Log(string.Format("                  Sample '{0,20}' Id:{1,3} OP:{2,3} PC{3,3} [{4,5},{5,5}] [{6,5},{7,5}]", sample.Name, izone.index, sample.origpitch, sample.pitchadj, izone.keylo, izone.keyhi, izone.vello, izone.velhi));
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void LoadBanks(ImSoundFont imsf, TPatchUsed filter, BuilderInfo LogInfo)
        {
            foreach (HiPreset p in imsf.HiSf.preset)
            {
                // If bank selected in view
                if (imsf.BankSelected[p.Bank])
                {
                    // If patch selected in filter
                    if (filter == null || (filter.BankUsed[p.Bank] != null && filter.BankUsed[p.Bank].PatchUsed[p.Num] != null))
                    {
                        if (imsf.Banks[p.Bank] == null)
                        {
                            // New bank, create it
                            imsf.Banks[p.Bank] = new ImBank()
                            {
                                BankNumber = p.Bank,
                                defpresets = new HiPreset[ImSoundFont.MAXBANKPRESET]
                            };
                        }

                        // Sort preset by number of patch
                        imsf.Banks[p.Bank].defpresets[p.Num] = p;
                    }
                }
            }
        }

        static private void CountPatch(ImSoundFont sf)
        {
            try
            {
                // Global count
                //sf.PatchCount = 0;
                foreach (ImBank bank in sf.Banks)
                    if (bank != null)
                    {
                        bank.PatchCount = 0;
                        foreach (HiPreset preset in bank.defpresets)
                            if (preset != null)
                            {
                                // Bank count
                                bank.PatchCount++;
                            }
                        // sf.PatchCount += bank.PatchCount;
                    }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }
    }
}
