#define SHOWDEFAULT
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
    [CanEditMultipleObjects]
    [CustomEditor(typeof(MidiFilePlayer))]
    public class MidiFilePlayerEditor : Editor
    {
        private SerializedProperty CustomEventListNotesEvent;
        private SerializedProperty CustomEventStartPlayMidi;
        private SerializedProperty CustomEventEndPlayMidi;

        private static MidiFilePlayer instance;

        private static bool showEvents;

#if SHOWDEFAULT
        private static bool showDefault;
#endif

        private SelectMidiWindow winSelectMidi;

        private MidiCommonEditor commonEditor;


        void OnEnable()
        {
            try
            {
                instance = (MidiFilePlayer)target;
                //Debug.Log("MidiFilePlayerEditor OnEnable " + instance.MPTK_MidiIndex);
                //Debug.Log("OnEnable MidiFilePlayerEditor");
                CustomEventStartPlayMidi = serializedObject.FindProperty("OnEventStartPlayMidi");
                CustomEventListNotesEvent = serializedObject.FindProperty("OnEventNotesMidi");
                CustomEventEndPlayMidi = serializedObject.FindProperty("OnEventEndPlayMidi");

                if (!Application.isPlaying)
                {
                    // Load description of available soundfont
                    if (MidiPlayerGlobal.CurrentMidiSet == null || MidiPlayerGlobal.CurrentMidiSet.ActiveSounFontInfo == null)
                    {
                        MidiPlayerGlobal.InitPath();
                        ToolsEditor.LoadMidiSet();
                        ToolsEditor.CheckMidiSet();
                    }
                }

                if (winSelectMidi != null)
                {
                    //Debug.Log("OnEnable winSelectMidi " + winSelectMidi.Title);
                    winSelectMidi.SelectedItem = instance.MPTK_MidiIndex;
                    winSelectMidi.Repaint();
                    winSelectMidi.Focus();
                }

                //EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged;
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        private void EditorApplication_playModeStateChanged(PlayModeStateChange obj)
        {
            //Debug.Log("EditorApplication_playModeStateChanged " + obj.ToString());
        }

        private void OnDisable()
        {
            try
            {
                if (winSelectMidi != null)
                {
                    //Debug.Log("OnDisable winSelectMidi " + winSelectMidi.Title);
                    winSelectMidi.Close();
                }
            }
            catch (Exception)
            {
            }
        }

        public void InitWinSelectMidi(int selected, Action<object, int> select)
        {
            // Get existing open window or if none, make a new one:
            try
            {
                winSelectMidi = EditorWindow.GetWindow<SelectMidiWindow>(true, "Select a Midi File");
                winSelectMidi.SelectWindow = winSelectMidi;
                //SelectMidiWindow.SelectWindow = winSelectMidi;
                winSelectMidi.OnSelect = select;
                winSelectMidi.SelectedItem = selected;
                winSelectMidi.BackgroundColor = new Color(.8f, .8f, .8f, 1f);
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        private void MidiChanged(object tag, int midiindex)
        {
            //Debug.Log("MidiChanged " + midiindex + " for " + tag);
            instance.MPTK_MidiIndex = midiindex;
            instance.MPTK_RePlay();
            MidiCommonEditor.SetSceneChangedIfNeed(instance, true);
        }

        public override void OnInspectorGUI()
        {
            try
            {
                GUI.changed = false;
                GUI.color = Color.white;
                if (commonEditor == null) commonEditor = ScriptableObject.CreateInstance<MidiCommonEditor>();

                Event e = Event.current;

                string soundFontSelected = "No SoundFont selected.";
                if (MidiPlayerGlobal.CurrentMidiSet != null && MidiPlayerGlobal.CurrentMidiSet.ActiveSounFontInfo != null)
                {
                    soundFontSelected = MidiPlayerGlobal.CurrentMidiSet.ActiveSounFontInfo.Name;
                    EditorGUILayout.LabelField(new GUIContent("SoundFont: " + soundFontSelected, MidiPlayerGlobal.HelpDefSoundFont));
                    EditorGUILayout.Separator();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(new GUIContent("Select Midi ", "Select Midi File to play"), GUILayout.Width(150));

                    if (MidiPlayerGlobal.CurrentMidiSet.MidiFiles != null && MidiPlayerGlobal.CurrentMidiSet.MidiFiles.Count > 0)
                    {
                        if (GUILayout.Button(new GUIContent(instance.MPTK_MidiIndex + " - " + instance.MPTK_MidiName, "Selected Midi File to play"), GUILayout.Height(30)))
                            InitWinSelectMidi(instance.MPTK_MidiIndex, MidiChanged);
                    }
                    else
                    {
                        EditorGUILayout.LabelField(MidiPlayerGlobal.ErrorNoMidiFile);
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space();

                    commonEditor.AllPrefab(instance);
                    commonEditor.MidiFileParameters(instance);
                    showEvents = EditorGUILayout.Foldout(showEvents, "Show Midi Events");
                    if (showEvents)
                    {
                        EditorGUILayout.PropertyField(CustomEventStartPlayMidi);
                        EditorGUILayout.PropertyField(CustomEventListNotesEvent);
                        EditorGUILayout.PropertyField(CustomEventEndPlayMidi);
                        serializedObject.ApplyModifiedProperties();
                    }
                    commonEditor.MidiFileInfo(instance);
                    commonEditor.SynthParameters(instance, serializedObject);

#if SHOWDEFAULT
                    showDefault = EditorGUILayout.Foldout(showDefault, "Show default editor");
                    if (showDefault) DrawDefaultInspector();
#endif
                }
                else
                {
                    MidiCommonEditor.ErrorNoSoundFont();
                }

                MidiCommonEditor.SetSceneChangedIfNeed(instance, GUI.changed);
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }


    }
}
