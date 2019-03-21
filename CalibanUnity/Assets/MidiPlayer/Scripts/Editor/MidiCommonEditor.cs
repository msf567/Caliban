#define MPTK_PRO
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
    public class MidiCommonEditor : ScriptableObject
    {
        static private bool showMidiInfo;
        static private bool showSynthParameter;
        static private bool showMidiParameter;
        static private bool showSynthEvents;

        private TextArea taSequence;
        private TextArea taProgram;
        private TextArea taInstrument;
        private TextArea taText;
        private TextArea taCopyright;
        private SerializedProperty CustomEventSynthAwake;
        private SerializedProperty CustomEventSynthStarted;

        //                                         Level=0            1           2           4             8      
        static private string[] popupQuantization = { "None", "Quarter Note", "Eighth Note", "16th Note", "32th Note", "64th Note" };

        public void AllPrefab(MidiPlayer instance)
        {
            float volume = EditorGUILayout.Slider(new GUIContent("Volume", "Set global volume for this midi playing"), instance.MPTK_Volume, 0f, 1f);
            if (instance.MPTK_Volume != volume)
                instance.MPTK_Volume = volume;
            EditorGUILayout.BeginHorizontal();
            string tooltipDistance = "Playing is paused if distance between AudioListener and this component is greater than MaxDistance";
            instance.MPTK_PauseOnDistance = EditorGUILayout.Toggle(new GUIContent("Pause With Distance", tooltipDistance), instance.MPTK_PauseOnDistance);
            EditorGUILayout.LabelField(new GUIContent("Current:" + Math.Round(instance.distanceEditorModeOnly, 2), tooltipDistance));
            EditorGUILayout.EndHorizontal();

            float distance = EditorGUILayout.Slider(new GUIContent("Max Distance", tooltipDistance), instance.MPTK_MaxDistance, 0f, 500f);
            if (instance.MPTK_MaxDistance != distance)
                instance.MPTK_MaxDistance = distance;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Transpose");
            instance.MPTK_Transpose = EditorGUILayout.IntSlider(instance.MPTK_Transpose, -24, 24);
            EditorGUILayout.EndHorizontal();
        }

        public void MidiFileParameters(MidiFilePlayer instance)
        {
            instance.MPTK_PlayOnStart = EditorGUILayout.Toggle(new GUIContent("Play At Startup", "Start playing midi when the application starts"), instance.MPTK_PlayOnStart);
            instance.MPTK_DirectSendToPlayer = EditorGUILayout.Toggle(new GUIContent("Send To Synth", "Midi events are send to the midi player directly"), instance.MPTK_DirectSendToPlayer);

            instance.MPTK_Loop = EditorGUILayout.Toggle(new GUIContent("Loop", "Enable loop on midi play"), instance.MPTK_Loop);

            if (EditorApplication.isPlaying)
            {
                EditorGUILayout.Separator();
                string infotime = "Real time since startup and total duration regarding the current tempo";
                EditorGUILayout.LabelField(new GUIContent("Time", infotime), new GUIContent(instance.playTimeEditorModeOnly + " / " + instance.durationEditorModeOnly, infotime));

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(new GUIContent("Position", "Set real time position since the startup regarding the current tempo"));
                float currentpos = (float)instance.MPTK_Position;
                float pos = EditorGUILayout.Slider(currentpos, 0f, (float)instance.MPTK_Duration.TotalMilliseconds);
                if (currentpos != pos)
                    instance.MPTK_Position = pos;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Separator();
                string infotick = "Tick count since startup and total duration regardless the current tempo";
                EditorGUILayout.LabelField(new GUIContent("Ticks", infotick), new GUIContent(instance.MPTK_TickCurrent + " / " + instance.MPTK_TickLast, infotime));

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(new GUIContent("Position", "Set tick position since the startup regardless the current tempo"));
                float currenttick = (float)instance.MPTK_TickCurrent;
                long ticks = Convert.ToInt64(EditorGUILayout.Slider(currenttick, 0f, (float)instance.MPTK_TickLast));
                if (currenttick != ticks)
                    instance.MPTK_TickCurrent = ticks;
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Separator();

                EditorGUILayout.BeginHorizontal();
                if (instance.MPTK_IsPlaying && !instance.MPTK_IsPaused)
                    GUI.color = ToolsEditor.ButtonColor;
                if (GUILayout.Button(new GUIContent("Play", "")))
                    instance.MPTK_Play();
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
#if MPTK_PRO
                if (!(instance is MidiExternalPlayer))
                {
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button(new GUIContent("Previous", "")))
                        instance.MPTK_Previous();
                    if (GUILayout.Button(new GUIContent("Next", "")))
                        instance.MPTK_Next();
                    EditorGUILayout.EndHorizontal();
                }
#endif
            }

            showMidiParameter = EditorGUILayout.Foldout(showMidiParameter, "Show Midi Parameters");
            if (showMidiParameter)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Quantization", ""), GUILayout.Width(150));
                int newLevel = EditorGUILayout.Popup(instance.MPTK_Quantization, popupQuantization);
                if (newLevel != instance.MPTK_Quantization && newLevel >= 0 && newLevel < popupQuantization.Length)
                    instance.MPTK_Quantization = newLevel;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Speed");
                float speed = EditorGUILayout.Slider(instance.MPTK_Speed, 0.1f, 5f);
                if (instance.MPTK_Speed != speed)
                    instance.MPTK_Speed = speed;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                instance.MPTK_EnableChangeTempo = EditorGUILayout.Toggle(new GUIContent("Tempo Change", "Enable midi event tempo change when playing."), instance.MPTK_EnableChangeTempo);
                EditorGUILayout.LabelField(new GUIContent("Current:" + Math.Round(instance.MPTK_Tempo, 0), "Current tempo defined in Midi"));
                EditorGUILayout.EndHorizontal();

                instance.MPTK_EnablePresetDrum = EditorGUILayout.Toggle(new GUIContent("Drum Preset Change", "Enable Preset change on the canal 10 for drum. By default disabled, could sometimes create bad sound with midi files not really compliant with the Midi norm."), instance.MPTK_EnablePresetDrum);
                instance.MPTK_KeepNoteOff = EditorGUILayout.Toggle(new GUIContent("Keep Midi NoteOff", "Keep Midi NoteOff and NoteOn with Velocity=0 (need to restart the playing Midi)"), instance.MPTK_KeepNoteOff);
                instance.MPTK_LogEvents = EditorGUILayout.Toggle(new GUIContent("Log Midi Events", "Log information about each midi events read."), instance.MPTK_LogEvents);
            }
        }

        public void MidiFileInfo(MidiFilePlayer instance)
        {
            showMidiInfo = EditorGUILayout.Foldout(showMidiInfo, "Show Midi Info");
            if (showMidiInfo)
            {
                if (!string.IsNullOrEmpty(instance.MPTK_SequenceTrackName))
                {
                    if (taSequence == null) taSequence = new TextArea("Sequence");
                    taSequence.Display(instance.MPTK_SequenceTrackName);
                }

                if (!string.IsNullOrEmpty(instance.MPTK_ProgramName))
                {
                    if (taProgram == null) taProgram = new TextArea("Program");
                    taProgram.Display(instance.MPTK_ProgramName);
                }

                if (!string.IsNullOrEmpty(instance.MPTK_TrackInstrumentName))
                {
                    if (taInstrument == null) taInstrument = new TextArea("Instrument");
                    taInstrument.Display(instance.MPTK_TrackInstrumentName);
                }

                if (!string.IsNullOrEmpty(instance.MPTK_TextEvent))
                {
                    if (taText == null) taText = new TextArea("TextEvent");
                    taText.Display(instance.MPTK_TextEvent);
                }

                if (!string.IsNullOrEmpty(instance.MPTK_Copyright))
                {
                    if (taCopyright == null) taCopyright = new TextArea("Copyright");
                    taCopyright.Display(instance.MPTK_Copyright);
                }
            }
        }

        public void SynthParameters(MidiPlayer instance, SerializedObject sobject)
        {
            showSynthParameter = EditorGUILayout.Foldout(showSynthParameter, "Show Synth Parameters");
            if (showSynthParameter)
            {


                //EditorGUILayout.BeginHorizontal();
                //EditorGUILayout.PrefixLabel(new GUIContent("Cutoff Volume", "Level of volume when the sound is cut. Could be usefull on weak device to rise this value, but sound experience is less good. "));
                //instance.CutoffVolume = EditorGUILayout.Slider((float)instance.CutoffVolume*1000f, 0.1f, 5f)/1000f;
                //EditorGUILayout.EndHorizontal();

                instance.MPTK_PlayOnlyFirstWave = EditorGUILayout.Toggle(new GUIContent("Play Only First Wave", "Some Instrument in Preset are using more of one wave at the same time. If checked, play only the first wave, usefull on weak device, but sound experience is less good."), instance.MPTK_PlayOnlyFirstWave);
                instance.MPTK_WeakDevice = EditorGUILayout.Toggle(new GUIContent("Weak Device", "Playing Midi files with WeakDevice activated could cause some bad interpretation of Midi Event, consequently bad sound."), instance.MPTK_WeakDevice);
                instance.MPTK_EnablePanChange = EditorGUILayout.Toggle(new GUIContent("Pan Change", "Enable midi event pan change when playing. Uncheck if you want to manage Pan in your application."), instance.MPTK_EnablePanChange);

                instance.MPTK_ApplyRealTimeModulator = EditorGUILayout.Toggle(new GUIContent("Apply Modulator", "Real-Time change Modulator from Midi and ADSR enveloppe Modulator parameters from SoundFont could have an impact on CPU. Initial value of Modulator set at Note On are keep. Uncheck to gain some % CPU on weak device."), instance.MPTK_ApplyRealTimeModulator);
                instance.MPTK_ApplyModLfo = EditorGUILayout.Toggle(new GUIContent("Apply Mod LFO", "LFO modulation are defined in SoudFont. Uncheck to gain some % CPU on weak device."), instance.MPTK_ApplyModLfo);
                instance.MPTK_ApplyVibLfo = EditorGUILayout.Toggle(new GUIContent("Apply Vib LFO", "LFO vibrato are defined in SoudFont. Uncheck to gain some % CPU on weak device."), instance.MPTK_ApplyVibLfo);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(new GUIContent("Release Time Minimum", "A default release time is defined at the preset level in the SoundFont. Setting a minimum time (milliseconds) to avoid abrupt sound stop and remove unpleasant sound. 50ms is a good tradoff for most of the case."));
                instance.MPTK_ReleaseTimeMin = EditorGUILayout.Slider(instance.MPTK_ReleaseTimeMin, 0f, 500f);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();
                instance.MPTK_ApplyFilter = EditorGUILayout.Toggle(new GUIContent("Apply Filter", "Low pass filter is defined in each preset of the SoudFont. Uncheck to gain some % CPU on weak device."), instance.MPTK_ApplyFilter);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(new GUIContent("Offset Filter Freq.", "Offset on the SF frequency set for this preset. 1000 seems a good value with the Unity filter ..."));
                instance.FilterOffset = EditorGUILayout.Slider(instance.FilterOffset, -2000f, 3000f);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();
                instance.MPTK_ApplyReverb = EditorGUILayout.Toggle(new GUIContent("Apply Reverb", "Reverb is defined in each preset of the SoudFont."), instance.MPTK_ApplyReverb);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(new GUIContent("Supersede Reverb", "If greater than 0, this parameter supersedes the value found in SF. 0:low dry signal 1: high dry signal"));
                instance.ReverbMix = EditorGUILayout.Slider(instance.ReverbMix, 0f, 1f);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();
                instance.MPTK_ApplyChorus = EditorGUILayout.Toggle(new GUIContent("Apply Chorus", "Chorus is defined in each preset of the SoudFont."), instance.MPTK_ApplyChorus);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(new GUIContent("Supersede Chorus", "If greater than 0, this parameter supersedes the value found in SF. 0:low original signal 1: high original signal"));
                instance.ChorusMix = EditorGUILayout.Slider(instance.ChorusMix, 0f, 1f);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();
                instance.MPTK_LogWave = EditorGUILayout.Toggle(new GUIContent("Log Waves", "Log information about wave found for a NoteOn event."), instance.MPTK_LogWave);
                EditorGUILayout.Space();

                EditorGUI.indentLevel++;
                showSynthEvents = EditorGUILayout.Foldout(showSynthEvents, "Show Synth Events");
                if (showSynthEvents)
                {

                    if (CustomEventSynthAwake == null)
                        CustomEventSynthAwake = sobject.FindProperty("OnEventSynthAwake");
                    EditorGUILayout.PropertyField(CustomEventSynthAwake);

                    if (CustomEventSynthStarted == null)
                        CustomEventSynthStarted = sobject.FindProperty("OnEventSynthStarted");
                    EditorGUILayout.PropertyField(CustomEventSynthStarted);

                    sobject.ApplyModifiedProperties();
                }
                EditorGUI.indentLevel--;
            }
        }
        public static void ErrorNoSoundFont()
        {
            GUIStyle labError = new GUIStyle("Label");
            labError.normal.background = SetColor(new Texture2D(2, 2), new Color(0.9f, 0.9f, 0.9f));
            labError.normal.textColor = new Color(0.8f, 0.1f, 0.1f);
            labError.alignment = TextAnchor.MiddleLeft;
            labError.wordWrap = true;
            labError.fontSize = 12;
            Texture buttonIconFolder = Resources.Load<Texture2D>("Textures/question-mark");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(MidiPlayerGlobal.ErrorNoSoundFont, labError, GUILayout.Height(40f));
            if (GUILayout.Button(new GUIContent(buttonIconFolder, "Help"), GUILayout.Width(40f), GUILayout.Height(40f)))
                Application.OpenURL("https://paxstellar.fr/setup-mptk-quick-start-v2/");
            EditorGUILayout.EndHorizontal();
            MidiPlayerGlobal.InitPath();
            ToolsEditor.LoadMidiSet();
            ToolsEditor.CheckMidiSet();
            Debug.Log(MidiPlayerGlobal.ErrorNoSoundFont);
        }
        public static Texture2D SetColor(Texture2D tex2, Color32 color)
        {
            var fillColorArray = tex2.GetPixels32();
            for (var i = 0; i < fillColorArray.Length; ++i)
                fillColorArray[i] = color;
            tex2.SetPixels32(fillColorArray);
            tex2.Apply();
            return tex2;
        }
        public static void SetSceneChangedIfNeed(UnityEngine.Object instance, bool changed)
        {
            if (changed)
            {
                EditorUtility.SetDirty(instance);
                if (!Application.isPlaying)
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            }
        }
    }
}
