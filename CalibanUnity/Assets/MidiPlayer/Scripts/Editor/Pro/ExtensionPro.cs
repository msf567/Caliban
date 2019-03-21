using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace MidiPlayerTK
{
    static public class ExtensionSoundFontSetupWindows
    {
        public static void SelectSf(this SoundFontSetupWindow sfsw, int i)
        {
            MidiPlayerGlobal.CurrentMidiSet.SetActiveSoundFont(i);
            string soundPath = Path.Combine(Application.dataPath + "/", MidiPlayerGlobal.PathToSoundfonts);
            soundPath = Path.Combine(soundPath + "/", MidiPlayerGlobal.CurrentMidiSet.ActiveSounFontInfo.Name);

            MidiPlayerGlobal.LoadCurrentSF();

            MidiPlayerGlobal.CurrentMidiSet.Save();
            if (MidiPlayerGlobal.ImSFCurrent != null)
            {
                //KeepAllPatchs = MidiPlayerGlobal.ImSFCurrent.KeepAllPatchs;
                //KeepAllZones = MidiPlayerGlobal.ImSFCurrent.KeepAllZones;
                //RemoveUnusedWaves = MidiPlayerGlobal.ImSFCurrent.RemoveUnusedWaves;
                if (Application.isPlaying)
                {
                    MidiPlayerGlobal.MPTK_SelectSoundFont(null);
                }
            }
        }

        public static void DeleteSf(this SoundFontSetupWindow sfsw)
        {
            string soundFontPath = Path.Combine(Application.dataPath + "/", MidiPlayerGlobal.PathToSoundfonts);
            string path = Path.Combine(soundFontPath, MidiPlayerGlobal.ImSFCurrent.SoundFontName);
            if (!string.IsNullOrEmpty(path) && EditorUtility.DisplayDialog("Remove SoundFont " + MidiPlayerGlobal.ImSFCurrent.SoundFontName, "If you click ok, the content of this folder will be deleted:\n\n" + path, "ok", "cancel"))
            {
                try
                {
                    Directory.Delete(path, true);
                    File.Delete(path + ".meta");

                }
                catch (Exception ex)
                {
                    Debug.Log("Remove SF " + ex.Message);
                }
                AssetDatabase.Refresh();
                ToolsEditor.CheckMidiSet();
            }
        }

    }
}
