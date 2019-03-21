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
    [CustomEditor(typeof(MidiListPlayer))]
    public class MidiListPlayerEditor : Editor
    {
        private SerializedProperty CustomEventStartPlayMidi;
        private SerializedProperty CustomEventEndPlayMidi;

        private static MidiListPlayer instance;

        private static bool showEvents = false;
        private Rect dragMidiZone;

        private Vector2 scrollPlayList;
        private static GUILayoutOption miniButtonWidth = GUILayout.Width(20f);
        private Texture buttonIconUpArrow;
        private Texture buttonIconDnArrow;
        private Texture buttonIconDelete;
        private Texture buttonIconSelect;
        private Texture buttonIconUnSelect;
        private MessagesEditor messages;

        // Manage skin
        public CustomStyle myStyle;

        void OnEnable()
        {
            try
            {
                messages = new MessagesEditor();

                //Load a Texture (Assets/Resources/Textures/texture01.png)
                buttonIconUpArrow = Resources.Load<Texture2D>("Textures/008-up-arrow");
                buttonIconDnArrow = Resources.Load<Texture2D>("Textures/037-down-arrow");
                buttonIconDelete = Resources.Load<Texture2D>("Textures/Delete_32x32");
                buttonIconSelect = Resources.Load<Texture2D>("Textures/040-confirm");
                buttonIconUnSelect = Resources.Load<Texture2D>("Textures/038-delete");

                //Debug.Log("OnEnable MidiFilePlayerEditor");
                CustomEventStartPlayMidi = serializedObject.FindProperty("OnEventStartPlayMidi");
                CustomEventEndPlayMidi = serializedObject.FindProperty("OnEventEndPlayMidi");

                instance = (MidiListPlayer)target;
                // Load description of available soundfont
                if (MidiPlayerGlobal.CurrentMidiSet == null || MidiPlayerGlobal.CurrentMidiSet.ActiveSounFontInfo == null)
                {
                    MidiPlayerGlobal.InitPath();
                    ToolsEditor.LoadMidiSet();
                    ToolsEditor.CheckMidiSet();
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        public override void OnInspectorGUI()
        {
            // Set custom Style. Good for background color 3E619800
            if (myStyle == null) myStyle = new CustomStyle();

            try
            {
                GUI.changed = false;
                GUI.color = Color.white;

                string soundFontSelected = "No SoundFont selected.";
                if (MidiPlayerGlobal.CurrentMidiSet != null && MidiPlayerGlobal.CurrentMidiSet.ActiveSounFontInfo != null)
                {
                    soundFontSelected = MidiPlayerGlobal.CurrentMidiSet.ActiveSounFontInfo.Name;
                    EditorGUILayout.LabelField(new GUIContent("SoundFont: " + soundFontSelected, MidiPlayerGlobal.HelpDefSoundFont));
                    EditorGUILayout.Separator();

                    //
                    // Define popup list with Midi playlist
                    //
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(new GUIContent("Current Midi ", "Select Midi File to play"), GUILayout.Width(100));
                    //if (GUILayout.Button(new GUIContent("Refresh", "Reload all Midi from resource folder"))) MidiPlayerGlobal.CheckMidiSet();
                    if (instance.MPTK_PlayList != null && instance.MPTK_PlayList.Count > 0)
                    {
                        string[] arrayMidi = new string[instance.MPTK_PlayList.Count];
                        int pos = 0;
                        foreach (MidiListPlayer.MPTK_MidiPlayItem midiname in instance.MPTK_PlayList)
                            arrayMidi[pos++] = midiname.MidiName;
                        int newSelectPlayIndex = EditorGUILayout.Popup(instance.MPTK_PlayIndex, arrayMidi);
                        // Is midifile has changed ?
                        if (newSelectPlayIndex != instance.MPTK_PlayIndex)
                        {
                            instance.MPTK_PlayIndex = newSelectPlayIndex;
                        }
                    }
                    else
                    {
                        EditorGUILayout.LabelField("No Midi play list defined");
                    }
                    EditorGUILayout.EndHorizontal();

                    //
                    // Defined drop zone and List
                    //
                    EditorGUILayout.LabelField(
                        "To create or update the play list, drag Midi files from <YourProject>/MidiPlayer/Resources/MidiDB to the zone just below.",
                       myStyle.DragZone, GUILayout.Height(40));
                    Event e = Event.current;

                    scrollPlayList = EditorGUILayout.BeginScrollView(scrollPlayList, false, false,
                       myStyle.HScroll, myStyle.VScroll, myStyle.BackgMidiList, GUILayout.MaxHeight(200));

                    if (instance.MPTK_PlayList != null)
                        for (int i = 0; i < instance.MPTK_PlayList.Count; i++)
                        {
                            MidiListPlayer.MPTK_MidiPlayItem playing = instance.MPTK_PlayList[i];
                            EditorGUILayout.BeginHorizontal(i == instance.MPTK_PlayIndex ? myStyle.ItemSelected : myStyle.ItemNotSelected);
                            EditorGUILayout.LabelField(playing.MidiName);
                            playing.Selected = EditorGUILayout.Toggle(playing.Selected);
                            EditorGUILayout.EndHorizontal();
                        }

                    EditorGUILayout.EndScrollView();
                    if (e.type == EventType.Repaint)
                    //if (e.type != EventType.Layout && e.type != EventType.Repaint)
                    {
                        // defined Scroll view as drag zone
                        dragMidiZone = GUILayoutUtility.GetLastRect();
                    }

                    //
                    // Action button on the Midi list
                    //
                    EditorGUILayout.BeginHorizontal();

                    // Select all midi in the list
                    if (GUILayout.Button(new GUIContent(buttonIconSelect, "Select all Midi"), EditorStyles.miniButtonRight, miniButtonWidth, GUILayout.Height(20f)))
                    {
                        foreach (MidiListPlayer.MPTK_MidiPlayItem item in instance.MPTK_PlayList)
                            item.Selected = true;
                    }

                    // UnSelect all midi in the list
                    if (GUILayout.Button(new GUIContent(buttonIconUnSelect, "Unselect all Midi"), EditorStyles.miniButtonRight, miniButtonWidth, GUILayout.Height(20f)))
                    {
                        foreach (MidiListPlayer.MPTK_MidiPlayItem item in instance.MPTK_PlayList)
                            item.Selected = false;
                    }

                    GUILayout.Space(20);
                    // Move midi down in the list
                    if (GUILayout.Button(new GUIContent(buttonIconDnArrow, "Move down selected Midi"), EditorStyles.miniButtonRight, miniButtonWidth, GUILayout.Height(20f)))
                    {
                        for (int i = instance.MPTK_PlayList.Count - 2; i >= 0; i--)
                            if (instance.MPTK_PlayList[i].Selected)
                            {
                                MidiListPlayer.MPTK_MidiPlayItem next = instance.MPTK_PlayList[i + 1];
                                instance.MPTK_PlayList[i + 1] = instance.MPTK_PlayList[i];
                                instance.MPTK_PlayList[i] = next;
                            }
                        instance.MPTK_ReIndexMidi();
                    }

                    // Move midi up in the list
                    if (GUILayout.Button(new GUIContent(buttonIconUpArrow, "Move up selected Midi"), EditorStyles.miniButtonRight, miniButtonWidth, GUILayout.Height(20f)))
                    {
                        for (int i = 1; i < instance.MPTK_PlayList.Count; i++)
                            if (instance.MPTK_PlayList[i].Selected)
                            {
                                MidiListPlayer.MPTK_MidiPlayItem prev = instance.MPTK_PlayList[i - 1];
                                instance.MPTK_PlayList[i - 1] = instance.MPTK_PlayList[i];
                                instance.MPTK_PlayList[i] = prev;
                            }
                        instance.MPTK_ReIndexMidi();
                    }
                    GUILayout.Space(20);

                    // remove all midis selected from the list
                    // int count = -1;
                    if (GUILayout.Button(new GUIContent(buttonIconDelete, "Remove selected Midi from the list"), EditorStyles.miniButtonRight, miniButtonWidth, GUILayout.Height(20f)))
                    {
                        int count = 0;
                        for (int i = 0; i < instance.MPTK_PlayList.Count;)
                            if (instance.MPTK_PlayList[i].Selected)
                            {
                                instance.MPTK_PlayList.RemoveAt(i);
                                count++;
                            }
                            else
                                i++;
                        if (count == 0)
                            messages.Add("No Midi is selected in the list", MessageType.Warning);
                        else if (count > 0)
                            messages.Add(count + " Midi removed from the playing list");
                        instance.MPTK_ReIndexMidi();
                    }
                    EditorGUILayout.EndHorizontal();

                    GUILayout.Space(15);
                    //EditorList.Show(serializedObject.FindProperty("MPTK_PlayList"), EditorListOption.ListLabel | EditorListOption.Buttons);
                    //serializedObject.ApplyModifiedProperties();

                    instance.MPTK_PlayOnStart = EditorGUILayout.Toggle(new GUIContent("Play On Start", "Start playing midi when component starts"), instance.MPTK_PlayOnStart);
                    instance.MPTK_Loop = EditorGUILayout.Toggle(new GUIContent("Loop on the list", "Enable loop on midi play list"), instance.MPTK_Loop);
                    if (EditorApplication.isPlaying)
                    {
                        EditorGUILayout.Separator();
                        EditorGUILayout.BeginHorizontal();

                        if (instance.MPTK_IsPlaying && !instance.MPTK_IsPaused)
                            GUI.color = ToolsEditor.ButtonColor;
                        if (GUILayout.Button(new GUIContent("Play", "")))
                        {
                            if (instance.MPTK_PlayList.Count == 0)
                                messages.Add("Playing list is empty. Add Midi file.");
                            else
                                instance.MPTK_Play();
                        }
                        GUI.color = Color.white;

                        if (instance.MPTK_IsPaused)
                            GUI.color = ToolsEditor.ButtonColor;
                        if (GUILayout.Button(new GUIContent("Pause", "")))
                            if (instance.MPTK_IsPaused)
                                instance.MPTK_Play();
                            else
                                instance.MPTK_Pause();
                        GUI.color = Color.white;

                        if (GUILayout.Button(new GUIContent("Stop", "")))
                            instance.MPTK_Stop();

                        if (GUILayout.Button(new GUIContent("Restart", "")))
                            instance.MPTK_RePlay();
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button(new GUIContent("Previous", "")))
                            instance.MPTK_Previous();
                        if (GUILayout.Button(new GUIContent("Next", "")))
                            instance.MPTK_Next();
                        EditorGUILayout.EndHorizontal();
                    }

                    showEvents = EditorGUILayout.Foldout(showEvents, "Show Midi Events");
                    if (showEvents)
                    {
                        EditorGUILayout.PropertyField(CustomEventStartPlayMidi);
                        EditorGUILayout.PropertyField(CustomEventEndPlayMidi);
                        serializedObject.ApplyModifiedProperties();
                    }

                    if (DragAndDropMidiFiles())
                        scrollPlayList = new Vector2(0, 10000);

                    messages.Display();

                    showDefault = EditorGUILayout.Foldout(showDefault, "Show default editor");
                    if (showDefault) DrawDefaultInspector();
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
        private static bool showDefault = true;

        private bool DragAndDropMidiFiles()
        {
            bool filesDropped = false;
            Event e = Event.current;

            // If mouse is dragging, we have a focused task, and we are not already dragging a task
            // Seems not useful ...
            //if ((e.type == EventType.MouseDrag))
            //{
            //    // Clear out drag data (doesn't seem to do much)
            //    DragAndDrop.PrepareStartDrag();
            //    DragAndDrop.paths = null;
            //    DragAndDrop.objectReferences = new UnityEngine.Object[0];
            //    // Start the actual drag (don't know what the name is for yet)
            //    DragAndDrop.StartDrag("Copy Task");
            //    // Use the event, else the drag won't start
            //    e.Use();
            //}

            if (e.type == EventType.DragUpdated)
            {
                //Debug.Log("" + e.type + " " + e.mousePosition);
                if (DragAndDrop.objectReferences != null && DragAndDrop.objectReferences.Length > 0)
                {
                    if (dragMidiZone.x != 0 && dragMidiZone.y != 0)
                    {
                        if (dragMidiZone.Contains(e.mousePosition) && DragAndDrop.objectReferences[0].ToString() == "MThd")
                            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                        else
                            DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                    }
                }
            }

            //if (e.type == EventType.DragExited)
            //{
            //    Debug.Log("" + e.type + " " + e.mousePosition);
            //}

            if (e.type == EventType.DragPerform)
            {
                //Debug.Log("" + e.type + " " + e.mousePosition);
                if (instance.MPTK_PlayList == null)
                {
                    Debug.Log("new List<MPTK_MidiListPlayer.MPTK_MidiPlayItem>();");
                    instance.MPTK_PlayList = new List<MidiListPlayer.MPTK_MidiPlayItem>();
                }
                if (DragAndDrop.objectReferences != null && DragAndDrop.objectReferences.Length > 0)
                {
                    if (dragMidiZone.x != 0 && dragMidiZone.y != 0)
                    {
                        if (dragMidiZone.Contains(e.mousePosition))
                        {
                            foreach (UnityEngine.Object o in DragAndDrop.objectReferences)
                                if (o.ToString() == "MThd")
                                {
                                    instance.MPTK_AddMidi(o.name);
                                    GUI.changed = true;
                                    filesDropped = true;
                                }
                        }
                    }
                }
            }
            return filesDropped;
        }
    }
}
