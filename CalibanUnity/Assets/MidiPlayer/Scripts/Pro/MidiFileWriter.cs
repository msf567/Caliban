using NAudio.Midi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MidiPlayerTK
{
    /// <summary>
    /// PRO Version - Write a midi file from differents sources based on NAudio frawemork. See full example TestMidiWriter.cs with a light sequencer.
    /// </summary>
    public class MidiFileWriter
    {
        /// <summary>
        /// Get the DeltaTicksPerQuarterNote of the loaded midi
        /// </summary>
        public int MPTK_DeltaTicksPerQuarterNote { get { return MidiEvents != null ? MidiEvents.DeltaTicksPerQuarterNote : 0; } }

        /// <summary>
        /// Get the track count of the loaded midi
        /// </summary>
        public int MPTK_TrackCount { get { return MidiEvents != null ? MidiEvents.Tracks : 0; } }


        /// <summary>
        /// Get the midi file type of the loaded midi (0,1,2)
        /// </summary>
        public int MPTK_MidiFileType { get { return MidiEvents != null ? MidiEvents.MidiFileType : -1; } }

        // List of midi events
        private MidiEventCollection MidiEvents;

        /// <summary>
        /// Create an empty MidiFileWriter
        /// </summary>
        public MidiFileWriter()
        {
            try
            {
                MidiEvents = new MidiEventCollection(1, 120);
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        /// <summary>
        /// Create a MidiFileWriter with an empty Midi Event list
        /// </summary>
        /// <param name="deltaTicksPerQuarterNote"></param>
        /// <param name="midiFileType"></param>
        public MidiFileWriter(int deltaTicksPerQuarterNote, int midiFileType)
        {
            try
            {
                MidiEvents = new MidiEventCollection(midiFileType, deltaTicksPerQuarterNote);
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        /// <summary>
        /// Create a MidiFileWriter from a MPTK list of midi events. A midi file must be loaded before from a MidiFilePlayer gameobject (as in example) 
        /// or from a call to MidiFileWriter.MPTK_LoadFromFile(filename).
        /// </summary>
        /// <param name="MidiSorted"></param>
        public bool MPTK_LoadFromMPTK(List<TrackMidiEvent> MidiSorted)
        {
            bool ok = false;
            try
            {
                foreach (TrackMidiEvent tme in MidiSorted)
                    MidiEvents.AddEvent(tme.Event, tme.IndexTrack - 1);
                ok = true;
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
            return ok;
        }

        /// <summary>
        /// Create a MidiFileWriter from a Midi found in MPTK MidiDB
        /// </summary>
        /// <param name="indexMidiDb"></param>
        public bool MPTK_LoadFromMidiDB(int indexMidiDb)
        {
            bool ok = false;
            try
            {
                if (MidiPlayerGlobal.CurrentMidiSet.MidiFiles != null && MidiPlayerGlobal.CurrentMidiSet.MidiFiles.Count > 0)
                {
                    if (indexMidiDb >= 0 && indexMidiDb < MidiPlayerGlobal.CurrentMidiSet.MidiFiles.Count - 1)
                    {
                        string midiname = MidiPlayerGlobal.CurrentMidiSet.MidiFiles[indexMidiDb];
                        TextAsset mididata = Resources.Load<TextAsset>(Path.Combine(MidiPlayerGlobal.MidiFilesDB, midiname));
                        MidiLoad midiLoad = new MidiLoad();
                        midiLoad.KeepNoteOff = true;
                        midiLoad.MPTK_Load(mididata.bytes);
                        //midiLoad.DebugMidiSorted();
                        MidiEvents = midiLoad.midifile.Events;
                        ok = true;
                    }
                    else
                        Debug.LogWarning("Index is out of the MidiDb list");
                }
                else
                    Debug.LogWarning("No MidiDb defined");
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
            return ok;
        }

        /// <summary>
        /// Create tracks
        /// </summary>
        /// <param name="count">number of tracks to create</param>
        public void MPTK_CreateTrack(int count)
        {
            try
            {
                for (int track = 0; track < count; track++)
                    MidiEvents.AddTrack();
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        /// <summary>
        /// Close the track (mandatory for a well formed midi file)
        /// </summary>
        /// <param name="trackNumber">Track number to close</param>
        public void MPTK_EndTrack(int trackNumber)
        {
            try
            {
                long endLastEvent = 0;
                if (MidiEvents[trackNumber].Count > 0)
                {
                    // if no noteon found, get time of the last event
                    endLastEvent = MidiEvents[trackNumber][MidiEvents[trackNumber].Count - 1].AbsoluteTime;

                    // search the last noteon event
                    for (int index = MidiEvents[trackNumber].Count - 1; index >= 0; index--)
                    {
                        if (MidiEvents[trackNumber][index] is NoteOnEvent)
                        {
                            NoteOnEvent lastnoteon = (NoteOnEvent)MidiEvents[trackNumber][index];
                            endLastEvent = lastnoteon.AbsoluteTime + lastnoteon.NoteLength;
                            //Debug.Log("lastnoteon " + lastnoteon.NoteName);
                            break;
                        }
                    }
                }
                //Debug.Log("Close track at " + endLastEvent);
                MidiEvents[trackNumber].Add(new MetaEvent(MetaEventType.EndTrack, 0, endLastEvent));
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        /// <summary>
        /// Add a generic Midi event
        /// </summary>
        /// <param name="track"></param>
        /// <param name="midievent"></param>
        public void MPTK_AddEvent(int track, MidiEvent midievent)
        {
            try
            {
                MidiEvents[track].Add(midievent);

            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        /// <summary>
        /// Add a note event. the corresponding Noteoff is automatically created.
        /// </summary>
        /// <param name="track"></param>
        /// <param name="absoluteTime"></param>
        /// <param name="channel"></param>
        /// <param name="note"></param>
        /// <param name="velocity"></param>
        /// <param name="duration"></param>
        public void MPTK_AddNote(int track, long absoluteTime, int channel, int note, int velocity, int duration)
        {
            try
            {
                MPTK_AddEvent(track, new NoteOnEvent(absoluteTime, channel, note, velocity, duration));
                MPTK_AddEvent(track, new NoteEvent(absoluteTime + duration, channel, MidiCommandCode.NoteOff, note, 0));

            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        public void MPTK_AddChangePreset(int track, long absoluteTime, int channel, int preset)
        {
            try
            {
                MPTK_AddEvent(track, new PatchChangeEvent(absoluteTime, channel, preset));
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        /// <summary>
        /// Convert BPM to duration or a quarter in microsecond
        /// </summary>
        /// <param name="bpm">beat per measure</param>
        /// <returns></returns>
        public static int MPTK_GetMicrosecondsPerQuaterNote(int bpm)
        {
            return 60 * 1000 * 1000 / bpm;
        }

        /// <summary>
        /// Load a Midi file from OS system file (could be dependant of the OS)
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public bool MPTK_LoadFromFile(string filename)
        {
            bool ok = false;
            try
            {
                MidiLoad midiLoad = new MidiLoad();
                midiLoad.KeepNoteOff = true;
                if (midiLoad.MPTK_Load(filename,false))
                {
                    MidiEvents = midiLoad.midifile.Events;
                    ok = true;
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
            return ok;

        }

        /// <summary>
        /// Write Midi file to an OS folder
        /// </summary>
        /// <param name="filename">filename of the midi file</param>
        /// <returns></returns>
        public bool MPTK_WriteToFile(string filename)
        {
            bool ok = false;
            try
            {
                if (MidiEvents != null)
                {
                    MidiFile.Export(filename, MidiEvents);
                    ok = true;
                }
                else
                    Debug.LogWarning("MidiFileWriter - Write - MidiEvents is null");
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
            return ok;
        }

        /// <summary>
        /// Write Midi file to MidiDB. To be used only in edit mode not in a standalone application.
        /// </summary>
        /// <param name="filename">filename of the midi file without any folder and any extension</param>
        /// <returns></returns>
        public bool MPTK_WriteToMidiDB(string filename)
        {
            bool ok = false;
            try
            {
                if (MidiEvents != null)
                {
                    if (Application.isEditor)
                    {
                        string filenameonly = Path.GetFileNameWithoutExtension(filename) + ".bytes";
                        // Build path to midi folder 
                        string pathMidiFile = Path.Combine(Application.dataPath, MidiPlayerGlobal.PathToMidiFile);
                        string filepath = Path.Combine(pathMidiFile, filenameonly);
                        //Debug.Log(filepath);
                        MidiFile.Export(filepath, MidiEvents);
                        //ToolsEditor.CheckMidiSet();
                        //AssetDatabase.Refresh();
                        ok = true;
                    }
                    else
                        Debug.LogWarning("WriteToMidiDB can be call only in editor mode not in a standalone application");
                }
                else
                    Debug.LogWarning("MidiFileWriter - Write - MidiEvents is null");
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
            return ok;
        }

        /// <summary>
        /// For testing purpose
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        private static bool Test(string source, string target)
        {
            bool ok = false;
            try
            {
                MidiFile midifile = new MidiFile(source);
                MidiFile.Export(target, midifile.Events);
                ok = true;
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
            return ok;
        }
    }
}
