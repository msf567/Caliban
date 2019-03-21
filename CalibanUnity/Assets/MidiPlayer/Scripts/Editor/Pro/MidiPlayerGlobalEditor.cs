using UnityEngine;
using UnityEditor;

using System;

using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace MidiPlayerTK
{
    /// <summary>
    /// Inspector for the midi global player component
    /// </summary>
    [CustomEditor(typeof(MidiPlayerGlobal))]
    public class MidiPlayerGlobalEditor : Editor
    {
        private SerializedProperty CustomOnEventPresetLoaded;
        private bool showEvents;
        private static MidiPlayerGlobal instance;

        void OnEnable()
        {
            try
            {
                //Debug.Log("OnEnable MidiFilePlayerEditor");
                CustomOnEventPresetLoaded = serializedObject.FindProperty("InstanceOnEventPresetLoaded");

                instance = (MidiPlayerGlobal)target;
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        public override void OnInspectorGUI()
        {
            try
            {
                GUI.changed = false;
                GUI.color = Color.white;

                string soundFontSelected = ".";
                if (MidiPlayerGlobal.CurrentMidiSet != null && MidiPlayerGlobal.CurrentMidiSet.ActiveSounFontInfo != null)
                {
                    SoundFontInfo sfi = MidiPlayerGlobal.CurrentMidiSet.ActiveSounFontInfo;

                    EditorGUILayout.Separator();
                    // Display popup to change SoundFont
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(new GUIContent("Current SoundFont", "SoundFont selected to play sound"), GUILayout.Width(150));

                    soundFontSelected = sfi.Name;
                    // SF is loaded in a coroutine, forbidden in edit mode
                    int selectedSFIndex = MidiPlayerGlobal.MPTK_ListSoundFont.FindIndex(s => s == soundFontSelected);
                    int newSelectSF = EditorGUILayout.Popup(selectedSFIndex, MidiPlayerGlobal.MPTK_ListSoundFont.ToArray());
                    if (newSelectSF != selectedSFIndex)
                    {
                        MidiPlayerGlobal.MPTK_SelectSoundFont(MidiPlayerGlobal.MPTK_ListSoundFont[newSelectSF]);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    MidiCommonEditor.ErrorNoSoundFont();
                }

                EditorGUILayout.Separator();
                showEvents = EditorGUILayout.Foldout(showEvents, "Show Global Events");
                if (showEvents)
                {
                    EditorGUILayout.PropertyField(CustomOnEventPresetLoaded);
                    serializedObject.ApplyModifiedProperties();
                }

                showDefault = EditorGUILayout.Foldout(showDefault, "Show default editor");
                if (showDefault) DrawDefaultInspector();

                if (GUI.changed) EditorUtility.SetDirty(instance);
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }
        private static bool showDefault = false;


    }

}
