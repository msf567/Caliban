using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MidiPlayerTK;
using System;

namespace MidiPlayerTK
{
    public class GUISelectSoundFont : MonoBehaviour
    {
        static public List<MPTKListItem> SoundFonts = null;
        static private PopupListItem PopSoundFont;
        static private int selectedSf;
        static private Texture buttonIconNote;

        static private void SoundFontChanged(object tag, int midiindex)
        {
            Debug.Log("SoundFontChanged " + midiindex);
            MidiPlayerGlobal.MPTK_SelectSoundFont(MidiPlayerGlobal.MPTK_ListSoundFont[midiindex]);
            selectedSf = midiindex;

            // return true;
        }

        static public void Display(Vector2 scrollerWindow, CustomStyle myStyle)
        {
            SoundFonts = new List<MPTKListItem>();
            foreach (string name in MidiPlayerGlobal.MPTK_ListSoundFont)
            {
                if (name == MidiPlayerGlobal.CurrentMidiSet.ActiveSounFontInfo.Name)
                    selectedSf = SoundFonts.Count;
                SoundFonts.Add(new MPTKListItem() { Index = SoundFonts.Count, Label = name });
            }

            if (PopSoundFont == null)
                PopSoundFont = new PopupListItem()
                {
                    Title = "Select A SoundFont",
                    OnSelect = SoundFontChanged,
                    ColCount = 1,
                    ColWidth = 500,
                };

            if (SoundFonts != null)
            {
                PopSoundFont.Draw(SoundFonts, selectedSf, myStyle);
                GUILayout.BeginHorizontal(myStyle.BacgDemos);

                if (buttonIconNote == null)
                    buttonIconNote = Resources.Load<Texture2D>("Textures/Note");
                if (GUILayout.Button(new GUIContent(buttonIconNote, "Select A SoundFont"), GUILayout.Width(48), GUILayout.Height(48)))
                    PopSoundFont.Show = !PopSoundFont.Show;
                GUILayout.Space(20);
                GUILayout.Label("Current SoundFont: " + MidiPlayerGlobal.CurrentMidiSet.ActiveSounFontInfo.Name, myStyle.TitleLabel2, GUILayout.Height(48));
                GUILayout.EndHorizontal();

                PopSoundFont.Position(ref scrollerWindow);
            }
            else
            {
                GUILayout.Label("No Soundfont found");
            }
        }
    }
}