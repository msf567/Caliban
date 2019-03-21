using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using NAudio.Midi;
using System;
using System.IO;
using System.Linq;

namespace MidiPlayerTK
{
    /// <summary>
    /// Base class for loading a Midi file. No seqencer, no synthetizer. 
    /// Usefull to load all tje Midi events from a Midi.
    /// </summary>
    public class MidiLoad
    {
        //! @cond NODOC
        public MidiFile midifile;
        public List<TrackMidiEvent> MidiSorted;
        public bool EndMidiEvent;
        public double QuarterPerMinuteValue;
        public string SequenceTrackName = "";
        public string ProgramName = "";
        public string TrackInstrumentName = "";
        public string TextEvent = "";
        public string Copyright = "";
        public double TickLengthMs;
        //! @endcond

        /// <summary>
        /// Duration of the midi. Updated when ChangeSpeed is called.
        /// </summary>
        public TimeSpan MPTK_Duration;

        /// <summary>
        /// Last tick position in Midi: Time of the last midi event in sequence expressed in number of "ticks". MPTK_TickLast / MPTK_DeltaTicksPerQuarterNote equal the duration time of a quarter-note regardless the defined tempo.
        /// </summary>
        public long MPTK_TickLast;

        /// <summary>
        /// Current tick position in Midi: Time of the current midi event expressed in number of "ticks". MPTK_TickCurrent / MPTK_DeltaTicksPerQuarterNote equal the duration time of a quarter-note regardless the defined tempo.
        /// </summary>
        public long MPTK_TickCurrent;

        /// <summary>
        /// From TimeSignature event: The numerator counts the number of beats in a measure. For example a numerator of 4 means that each bar contains four beats. This is important to know because usually the first beat of each bar has extra emphasis.
        /// http://www.deluge.co/?q=midi-tempo-bpm
        /// </summary>
        public int MPTK_NumberBeatsMeasure;

        /// <summary>
        /// From TimeSignature event: number of quarter notes in a beat. Equal 2 Power TimeSigDenominator.
        /// http://www.deluge.co/?q=midi-tempo-bpm
        /// </summary>
        public int MPTK_NumberQuarterBeat;

        /// <summary>
        /// From TimeSignature event: The numerator counts the number of beats in a measure. For example a numerator of 4 means that each bar contains four beats. This is important to know because usually the first beat of each bar has extra emphasis. In MIDI the denominator value is stored in a special format. i.e. the real denominator = 2^[dd]
        /// http://www.deluge.co/?q=midi-tempo-bpm
        /// </summary>
        public int MPTK_TimeSigNumerator;

        /// <summary>
        /// From TimeSignature event: The denominator specifies the number of quarter notes in a beat. 2 represents a quarter-note, 3 represents an eighth-note, etc. . 
        /// http://www.deluge.co/?q=midi-tempo-bpm
        /// </summary>
        public int MPTK_TimeSigDenominator;

        /// <summary>
        /// From TimeSignature event: The standard MIDI clock ticks every 24 times every quarter note (crotchet) so a [cc] value of 24 would mean that the metronome clicks once every quarter note. A [cc] value of 6 would mean that the metronome clicks once every 1/8th of a note (quaver).
        /// http://www.deluge.co/?q=midi-tempo-bpm
        /// </summary>
        public int MPTK_TicksInMetronomeClick;

        /// <summary>
        /// From TimeSignature event: This value specifies the number of 1/32nds of a note happen every MIDI quarter note. It is usually 8 which means that a quarter note happens every quarter note.
        /// http://www.deluge.co/?q=midi-tempo-bpm
        /// </summary>
        public int MPTK_No32ndNotesInQuarterNote;

        /// <summary>
        /// From the SetTempo event: The tempo is given in micro seconds per quarter beat. To convert this to BPM we needs to use the following equation:BPM = 60,000,000/[tt tt tt]
        /// http://www.deluge.co/?q=midi-tempo-bpm
        /// </summary>
        public int MPTK_MicrosecondsPerQuarterNote;

        /// <summary>
        /// Midi Header: Delta Ticks Per Quarter Note. Represent the duration time in "ticks" which make up a quarter-note. For instance, if 96, then a duration of an eighth-note in the file would be 48.
        /// </summary>
        public int MPTK_DeltaTicksPerQuarterNote;

        public bool LogEvents;
        public bool KeepNoteOff;

        private long Quantization;
        private long CurrentTick;
        private double Speed = 1d;
        private double LastTimeFromStartMS;

        // <summary>
        /// Last position played by tracks
        /// </summary>
        private int NextPosEvent;
        private static string[] NoteNames = new string[] { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };

        /// <summary>
        /// Load Midi from midi MPTK referential (Unity resource). 
        /// </summary>
        /// <param name="index"></param>
        public bool MPTK_Load(int index)
        {
            bool ok = true;
            try
            {
                if (index >= 0 && index < MidiPlayerGlobal.CurrentMidiSet.MidiFiles.Count)
                {
                    string midiname = MidiPlayerGlobal.CurrentMidiSet.MidiFiles[index];
                    TextAsset mididata = Resources.Load<TextAsset>(Path.Combine(MidiPlayerGlobal.MidiFilesDB, midiname));
                    midifile = new MidiFile(mididata.bytes, false);
                    if (midifile != null)
                        AnalyseMidi();
                }
                else
                {
                    Debug.LogWarning("MidiLoad - index out of range " + index);
                    ok = false;
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
                ok = false;
            }
            return ok;
        }

        /// <summary>
        /// Load Midi from an array of bytes
        /// </summary>
        /// <param name="datamidi">byte arry midi</param>
        public bool MPTK_Load(byte[] datamidi)
        {
            bool ok = true;
            try
            {
                midifile = new MidiFile(datamidi, false);
                if (midifile != null)
                    AnalyseMidi();
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
                ok = false;
            }
            return ok;
        }

        /// <summary>
        /// Load Midi from a Midi file from Unity resources. The Midi file must be present in Unity MidiDB ressource folder.
        /// </summary>
        /// <param name="midiname">midi file name without path and extension</param>
        public bool MPTK_Load(string midiname)
        {
            bool ok = true;
            try
            {
                string pathfilename = BuildOSPath(midiname);
                midifile = new MidiFile(pathfilename, false);
                if (midifile != null)
                    AnalyseMidi();
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
                ok = false;
            }
            return ok;
        }

        /// <summary>
        /// Load Midi from a folder from a folder anywhere on the desktop.
        /// </summary>
        /// <param name="pathfilename">complete path + filename to the Midi file</param>
        /// <param name="strict">if true, check strict compliance with the Midi norm</param>
        /// <returns></returns>
        public bool MPTK_Load(string pathfilename, bool strict)
        {
            bool ok = true;
            try
            {
                midifile = new MidiFile(pathfilename, strict);
                if (midifile != null)
                    AnalyseMidi();
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
                ok = false;
            }
            return ok;
        }


        /// <summary>
        /// Read the list of midi events available in the Midi from a ticks position to an end position.
        /// </summary>
        /// <param name="fromTicks">ticks start</param>
        /// <param name="toTicks">ticks end</param>
        /// <returns></returns>
        public List<MPTKEvent> MPTK_ReadMidiEvents(long fromTicks = 0, long toTicks = long.MaxValue)
        {
            List<MPTKEvent> midievents = new List<MPTKEvent>();
            try
            {
                if (midifile != null)
                {
                    foreach (TrackMidiEvent trackEvent in MidiSorted)
                    {
                        if (Quantization != 0)
                            trackEvent.AbsoluteQuantize = ((trackEvent.Event.AbsoluteTime + Quantization / 2) / Quantization) * Quantization;
                        else
                            trackEvent.AbsoluteQuantize = trackEvent.Event.AbsoluteTime;

                        //Debug.Log("ReadMidiEvents - timeFromStartMS:" + Convert.ToInt32(timeFromStartMS) + " LastTimeFromStartMS:" + Convert.ToInt32(LastTimeFromStartMS) + " CurrentPulse:" + CurrentPulse + " AbsoluteQuantize:" + trackEvent.AbsoluteQuantize);

                        if (trackEvent.AbsoluteQuantize >= fromTicks && trackEvent.AbsoluteQuantize <= toTicks)
                        {
                            ConvertToEvent(midievents, trackEvent);
                        }
                        if (trackEvent.AbsoluteQuantize > toTicks)
                            break;
                        MPTK_TickCurrent = trackEvent.AbsoluteQuantize;
                        MPTK_TickLast = trackEvent.AbsoluteQuantize;

                    }
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
            return midievents;
        }

        /// <summary>
        /// Convert the tick duration to a real time duration in millisecond regarding the current tempo.
        /// </summary>
        /// <param name="tick">duration in ticks</param>
        /// <returns>duration in milliseconds</returns>
        public double MPTK_ConvertTickToTime(long tick)
        {
            return tick * TickLengthMs;
        }

        /// <summary>
        /// Convert a real time duration in millisecond to a number of tick regarding the current tempo.
        /// </summary>
        /// <param name="time">duration in milliseconds</param>
        /// <returns>duration in ticks</returns>
        public long MPTK_ConvertTimeToTick(double time)
        {
            if (TickLengthMs != 0d)
                return Convert.ToInt64(time / TickLengthMs);
            else
                return 0;
        }

        // No doc until end of file
        //! @cond NODOC

        /// <summary>
        /// Build OS path to the midi file
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        static public string BuildOSPath(string filename)
        {
            try
            {
                string pathMidiFolder = Path.Combine(Application.dataPath, MidiPlayerGlobal.PathToMidiFile);
                string pathfilename = Path.Combine(pathMidiFolder, filename + MidiPlayerGlobal.ExtensionMidiFile);
                return pathfilename;
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
            return null;
        }

        private void AnalyseMidi()
        {
            try
            {
                MPTK_TickLast = 0;
                MPTK_TickCurrent = 0;
                CurrentTick = 0;
                NextPosEvent = 0;
                LastTimeFromStartMS = 0;
                QuarterPerMinuteValue = double.NegativeInfinity;

                SequenceTrackName = "";
                ProgramName = "";
                TrackInstrumentName = "";
                TextEvent = "";
                Copyright = "";

                // Get midi events from midifile.Events
                MidiSorted = GetEvents();

                // If there is no tempo event, set a default value
                if (QuarterPerMinuteValue < 0d)
                    ChangeTempo(120d);

                MPTK_DeltaTicksPerQuarterNote = midifile.DeltaTicksPerQuarterNote;

            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        private List<TrackMidiEvent> GetEvents()
        {
            int countTracks = 0;

            try
            {
                List<TrackMidiEvent> events = new List<TrackMidiEvent>();
                foreach (IList<MidiEvent> track in midifile.Events)
                {
                    countTracks++;
                    foreach (MidiEvent e in track)
                    {
                        try
                        {
                            bool keepEvent = false;
                            switch (e.CommandCode)
                            {
                                case MidiCommandCode.NoteOn:
                                    //Debug.Log("NoteOn");
                                    if (KeepNoteOff)
                                        // keep note even if no offevent
                                        keepEvent = true;
                                    else
                                    if (((NoteOnEvent)e).OffEvent != null)
                                        keepEvent = true;
                                    break;
                                case MidiCommandCode.NoteOff:
                                    //Debug.Log("NoteOff");
                                    if (KeepNoteOff)
                                        keepEvent = true;
                                    break;
                                case MidiCommandCode.ControlChange:
                                    //ControlChangeEvent ctrl = (ControlChangeEvent)e;
                                    //Debug.Log("NoteOff");
                                    keepEvent = true;
                                    break;
                                case MidiCommandCode.PatchChange:
                                    keepEvent = true;
                                    break;
                                case MidiCommandCode.MetaEvent:
                                    MetaEvent meta = (MetaEvent)e;
                                    switch (meta.MetaEventType)
                                    {
                                        case MetaEventType.SetTempo:
                                            // Set the first tempo value find
                                            if (MPTK_MicrosecondsPerQuarterNote == 0)
                                            {
                                                ChangeTempo(((TempoEvent)e).Tempo);
                                                MPTK_MicrosecondsPerQuarterNote = ((TempoEvent)e).MicrosecondsPerQuarterNote;
                                            }
                                            break;
                                        case MetaEventType.TimeSignature:
                                            AnalyzeTimeSignature(meta);
                                            break;
                                    }
                                    keepEvent = true;
                                    break;
                            }
                            if (keepEvent)
                                events.Add(new TrackMidiEvent() { IndexTrack = countTracks, Event = e.Clone() });
                        }
                        catch (System.Exception ex)
                        {
                            MidiPlayerGlobal.ErrorDetail(ex);
                            //List<TrackMidiEvent> MidiSorted = events.OrderBy(o => o.Event.AbsoluteTime).ToList();
                            return events.OrderBy(o => o.Event.AbsoluteTime).ToList();
                        }
                    }
                }

                /// Sort midi event by time
                List<TrackMidiEvent> MidiSorted = events.OrderBy(o => o.Event.AbsoluteTime).ToList();
                if (MidiSorted.Count > 0)
                    MPTK_TickLast = MidiSorted[MidiSorted.Count - 1].Event.AbsoluteTime;
                else
                    MPTK_TickLast = 0;

                return MidiSorted;
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
            return null;
        }

        /// <summary>
        /// Change speed to play. 1=normal speed
        /// </summary>
        /// <param name="speed"></param>
        public void ChangeSpeed(float speed)
        {
            try
            {
                //Debug.Log("ChangeSpeed " + speed);
                Speed = speed;
                if (QuarterPerMinuteValue > 0d)
                {
                    ChangeTempo(QuarterPerMinuteValue);
                    //CancelNextReadEvents = true;
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        public void ChangeQuantization(int level = 4)
        {
            try
            {
                if (level <= 0)
                    Quantization = 0;
                else
                    Quantization = midifile.DeltaTicksPerQuarterNote / level;
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        /// <summary>
        /// Calculate PulseLenghtMS from QuarterPerMinute value
        /// </summary>
        /// <param name="tempo"></param>
        public void ChangeTempo(double tempo)
        {
            try
            {
                QuarterPerMinuteValue = tempo;
                TickLengthMs = (1000d / ((QuarterPerMinuteValue * midifile.DeltaTicksPerQuarterNote) / 60f)) / Speed;
                //The BPM measures how many quarter notes happen in a minute. To work out the length of each pulse we can use the following formula: Pulse Length = 60 / (BPM * PPQN)
                //16  Sixteen Double croche

                if (LogEvents)
                {
                    Debug.Log("ChangeTempo");
                    Debug.Log("     QuarterPerMinuteValue :" + QuarterPerMinuteValue);
                    Debug.Log("     Speed :" + Speed);
                    Debug.Log("     DeltaTicksPerQuarterNote :" + midifile.DeltaTicksPerQuarterNote);
                    Debug.Log("     Pulse length in ms :" + TickLengthMs);
                }

                // Update total time of midi play
                if (MidiSorted != null && MidiSorted.Count > 0)
                {
                    MPTK_Duration = TimeSpan.FromMilliseconds(MidiSorted[MidiSorted.Count - 1].Event.AbsoluteTime * TickLengthMs);
                    if (LogEvents)
                        Debug.Log("     Duration :" + MPTK_Duration.TotalSeconds);
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        public int CalculateNextPosEvents(double timeFromStartMS)
        {
            if (MidiSorted != null)
            {
                CurrentTick = MPTK_ConvertTimeToTick(timeFromStartMS);
                //Debug.Log(">>> CalculateNextPosEvents - CurrentPulse:" + CurrentTick + " CurrentNextPosEvent:" + NextPosEvent + " LastTimeFromStartMS:" + LastTimeFromStartMS + " timeFromStartMS:" + Convert.ToInt32(timeFromStartMS));
                if (CurrentTick == 0)
                {
                    NextPosEvent = 0;
                    LastTimeFromStartMS = 0;
                }
                else
                {
                    LastTimeFromStartMS = timeFromStartMS;
                    for (int currentPosEvent = 0; currentPosEvent < MidiSorted.Count; currentPosEvent++)
                    {
                        TrackMidiEvent trackEvent = MidiSorted[currentPosEvent];
                        //Debug.Log("CurrentPulse:" + CurrentPulse + " trackEvent:" + trackEvent.AbsoluteQuantize);

                        if (trackEvent.Event.AbsoluteTime > CurrentTick)// && CurrentPulse < nexttrackEvent.Event.AbsoluteTime )
                        {
                            NextPosEvent = currentPosEvent;
                            //Debug.Log("     CalculateNextPosEvents - NextPosEvent:" + NextPosEvent + " trackEvent:" + trackEvent.Event.AbsoluteTime + " timeFromStartMS:" + Convert.ToInt32(timeFromStartMS));
                            //Debug.Log("NextPosEvent:" + NextPosEvent);
                            break;
                        }
                        //if (currentPosEvent == MidiSorted.Count - 1) Debug.Log("Last CalculateNextPosEvents - currentPosEvent:" + currentPosEvent + " trackEvent:" + trackEvent.Event.AbsoluteTime + " timeFromStartMS:" + Convert.ToInt32(timeFromStartMS));
                    }
                }
                //Debug.Log("<<< CalculateNextPosEvents NextPosEvent:" + NextPosEvent);
            }
            return NextPosEvent;
        }

        /// <summary>
        /// Read a list of midi event available for the current time
        /// </summary>
        /// <param name="timeFromStartMS"></param>
        /// <returns></returns>
        public List<MPTKEvent> ReadMidiEvents(double timeFromStartMS)
        {
            List<MPTKEvent> midievents = null;
            try
            {
                EndMidiEvent = false;
                if (midifile != null)
                {
                    if (NextPosEvent < MidiSorted.Count)
                    {
                        // The BPM measures how many quarter notes happen in a minute. To work out the length of each pulse we can use the following formula: 
                        // Pulse Length = 60 / (BPM * PPQN)
                        // Calculate current pulse to play
                        CurrentTick += Convert.ToInt64((timeFromStartMS - LastTimeFromStartMS) / TickLengthMs);

                        LastTimeFromStartMS = timeFromStartMS;
                        // From the last position played
                        for (int currentPosEvent = NextPosEvent; currentPosEvent < MidiSorted.Count; currentPosEvent++)
                        {
                            TrackMidiEvent trackEvent = MidiSorted[currentPosEvent];
                            if (Quantization != 0)
                                trackEvent.AbsoluteQuantize = ((trackEvent.Event.AbsoluteTime + Quantization / 2) / Quantization) * Quantization;
                            else
                                trackEvent.AbsoluteQuantize = trackEvent.Event.AbsoluteTime;

                            //Debug.Log("ReadMidiEvents - timeFromStartMS:" + Convert.ToInt32(timeFromStartMS) + " LastTimeFromStartMS:" + Convert.ToInt32(LastTimeFromStartMS) + " CurrentPulse:" + CurrentPulse + " AbsoluteQuantize:" + trackEvent.AbsoluteQuantize);

                            if (trackEvent.AbsoluteQuantize <= CurrentTick)
                            {
                                NextPosEvent = currentPosEvent + 1;
                                if (midievents == null) midievents = new List<MPTKEvent>();
                                if (ConvertToEvent(midievents, trackEvent))
                                    break; ;
                            }
                            else
                                // Out of time, exit for loop
                                break;
                        }
                    }
                    else
                    {
                        // End of midi events
                        EndMidiEvent = true;
                    }
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }

            if (midievents != null && midievents.Count > 0)
            {
                MPTK_TickCurrent = midievents.Last().Tick;
            }


            return midievents;
        }

        private bool ConvertToEvent(List<MPTKEvent> midievents, TrackMidiEvent trackEvent)
        {
            bool exitLoop = false;
            MPTKEvent midievent = null;
            switch (trackEvent.Event.CommandCode)
            {
                case MidiCommandCode.NoteOn:

                    if (((NoteOnEvent)trackEvent.Event).OffEvent != null)
                    {
                        NoteOnEvent noteon = (NoteOnEvent)trackEvent.Event;
                        //Debug.Log(string.Format("Track:{0} NoteNumber:{1,3:000} AbsoluteTime:{2,6:000000} NoteLength:{3,6:000000} OffDeltaTime:{4,6:000000} ", track, noteon.NoteNumber, noteon.AbsoluteTime, noteon.NoteLength, noteon.OffEvent.DeltaTime));
                        midievent = new MPTKEvent()
                        {
                            Tick = trackEvent.AbsoluteQuantize,
                            Command = MPTKCommand.NoteOn,
                            Value = noteon.NoteNumber,
                            Channel = trackEvent.Event.Channel - 1,
                            Velocity = noteon.Velocity,
                            Duration = noteon.NoteLength * TickLengthMs,
                            Length = noteon.NoteLength,
                        };
                        midievents.Add(midievent);
                        if (LogEvents)
                        {
                            string notename = (midievent.Channel != 9) ?
                                String.Format("{0}{1}", NoteNames[midievent.Value % 12], midievent.Value / 12) :
                                "Drum";
                            Debug.Log(BuildInfoTrack(trackEvent) + string.Format("NoteOn  {0,3:000}\t{1,-4}\tLenght:{2,5}\t{3}\tVeloc:{4,3}",
                                midievent.Value, notename, noteon.NoteLength, NoteLength(midievent), noteon.Velocity));
                        }
                    }
                    break;

                case MidiCommandCode.NoteOff:
                    // no need, noteoff are associated with noteon
                    break;

                case MidiCommandCode.ControlChange:

                    ControlChangeEvent controlchange = (ControlChangeEvent)trackEvent.Event;
                    midievent = new MPTKEvent()
                    {
                        Tick = trackEvent.AbsoluteQuantize,
                        Command = MPTKCommand.ControlChange,
                        Channel = trackEvent.Event.Channel - 1,
                        Controller = (MPTKController)controlchange.Controller,
                        Value = controlchange.ControllerValue,

                    };
                    midievents.Add(midievent);

                    // Other midi event
                    if (LogEvents)
                        Debug.Log(BuildInfoTrack(trackEvent) + string.Format("Control {0} {1}", controlchange.Controller, controlchange.ControllerValue));

                    break;

                case MidiCommandCode.PatchChange:
                    PatchChangeEvent change = (PatchChangeEvent)trackEvent.Event;
                    midievent = new MPTKEvent()
                    {
                        Tick = trackEvent.AbsoluteQuantize,
                        Command = MPTKCommand.PatchChange,
                        Channel = trackEvent.Event.Channel - 1,
                        Value = change.Patch,
                    };
                    midievents.Add(midievent);
                    if (LogEvents)
                        Debug.Log(BuildInfoTrack(trackEvent) + string.Format("Patch   {0,3:000} {1}", change.Patch, PatchChangeEvent.GetPatchName(change.Patch)));
                    break;

                case MidiCommandCode.MetaEvent:
                    MetaEvent meta = (MetaEvent)trackEvent.Event;
                    midievent = new MPTKEvent()
                    {
                        Tick = trackEvent.AbsoluteQuantize,
                        Command = MPTKCommand.MetaEvent,
                        Channel = trackEvent.Event.Channel - 1,
                        Meta = (MPTKMeta)meta.MetaEventType,
                    };

                    switch (meta.MetaEventType)
                    {
                        case MetaEventType.TimeSignature:
                            AnalyzeTimeSignature(meta);
                            break;

                        case MetaEventType.SetTempo:
                            TempoEvent tempo = (TempoEvent)meta;
                            // Tempo change will be done in MidiFilePlayer
                            midievent.Duration = tempo.Tempo;
                            MPTK_MicrosecondsPerQuarterNote = tempo.MicrosecondsPerQuarterNote;
                            // Force exit loop
                            exitLoop = true;
                            break;

                        case MetaEventType.SequenceTrackName:
                            midievent.Info = ((TextEvent)meta).Text;
                            if (!string.IsNullOrEmpty(SequenceTrackName)) SequenceTrackName += "\n";
                            SequenceTrackName += string.Format("T{0,2:00} {1}", trackEvent.IndexTrack, midievent.Info);
                            //if (LogEvents) Debug.Log(BuildInfoTrack(trackEvent) + string.Format("Sequence   '{0}'", note.Info));
                            break;

                        case MetaEventType.ProgramName:
                            midievent.Info = ((TextEvent)meta).Text;
                            ProgramName += midievent.Info + " ";
                            //if (LogEvents) Debug.Log(BuildInfoTrack(trackEvent) + string.Format("Program   '{0}'", note.Info));
                            break;

                        case MetaEventType.TrackInstrumentName:
                            midievent.Info = ((TextEvent)meta).Text;
                            if (!string.IsNullOrEmpty(TrackInstrumentName)) TrackInstrumentName += "\n";
                            TrackInstrumentName += string.Format("T{0,2:00} {1}", trackEvent.IndexTrack, midievent.Info);
                            //if (LogEvents) Debug.Log(BuildInfoTrack(trackEvent) + string.Format("Text      '{0}'", ((TextEvent)meta).Text));
                            break;

                        case MetaEventType.TextEvent:
                            midievent.Info = ((TextEvent)meta).Text;
                            TextEvent += midievent.Info + " ";
                            //if (LogEvents) Debug.Log(BuildInfoTrack(trackEvent) + string.Format("Sequence  '{0}'", ((TextEvent)meta).Text));
                            break;

                        case MetaEventType.Copyright:
                            midievent.Info = ((TextEvent)meta).Text;
                            Copyright += midievent.Info + " ";
                            //if (LogEvents) Debug.Log(BuildInfoTrack(trackEvent) + string.Format("Copyright '{0}'", ((TextEvent)meta).Text));
                            break;

                        case MetaEventType.Lyric: // lyric
                            midievent.Info = ((TextEvent)meta).Text;
                            break;

                        case MetaEventType.Marker: // marker
                        case MetaEventType.CuePoint: // cue point
                        case MetaEventType.DeviceName:
                            break;
                    }

                    if (LogEvents && !string.IsNullOrEmpty(midievent.Info))
                        Debug.Log(BuildInfoTrack(trackEvent) + string.Format("Meta    {0,15} '{1}'", midievent.Meta, midievent.Info));

                    midievents.Add(midievent);
                    //Debug.Log(BuildInfoTrack(trackEvent) + string.Format("Meta {0} {1}", meta.MetaEventType, meta.ToString()));
                    break;

                default:
                    // Other midi event
                    if (LogEvents)
                        Debug.Log(BuildInfoTrack(trackEvent) + string.Format("Other   {0,15} Not handle by MPTK", trackEvent.Event.CommandCode));
                    break;
            }

            return exitLoop;
        }

        /// <summary>
        /// https://en.wikipedia.org/wiki/Note_value
        /// </summary>
        /// <param name="note"></param>
        /// <returns></returns>
        public MPTKEvent.EnumLength NoteLength(MPTKEvent note)
        {
            if (midifile != null)
            {
                if (note.Length >= midifile.DeltaTicksPerQuarterNote * 4)
                    return MPTKEvent.EnumLength.Whole;
                else if (note.Length >= midifile.DeltaTicksPerQuarterNote * 2)
                    return MPTKEvent.EnumLength.Half;
                else if (note.Length >= midifile.DeltaTicksPerQuarterNote)
                    return MPTKEvent.EnumLength.Quarter;
                else if (note.Length >= midifile.DeltaTicksPerQuarterNote / 2)
                    return MPTKEvent.EnumLength.Eighth;
            }
            return MPTKEvent.EnumLength.Sixteenth;
        }

        private void AnalyzeTimeSignature(MetaEvent meta)
        {
            TimeSignatureEvent timesig = (TimeSignatureEvent)meta;
            // Numerator: counts the number of beats in a measure. 
            // For example a numerator of 4 means that each bar contains four beats. 
            MPTK_TimeSigNumerator = timesig.Numerator;
            // Denominator: number of quarter notes in a beat.0=ronde, 1=blanche, 2=quarter, 3=eighth, etc. 
            MPTK_TimeSigDenominator = timesig.Denominator;
            MPTK_NumberBeatsMeasure = timesig.Numerator;
            MPTK_NumberQuarterBeat = System.Convert.ToInt32(System.Math.Pow(2, timesig.Denominator));
            MPTK_TicksInMetronomeClick = timesig.TicksInMetronomeClick;
            MPTK_No32ndNotesInQuarterNote = timesig.No32ndNotesInQuarterNote;
        }

        /// <summary>
        /// Read midi event Tempo and Patch change from start
        /// </summary>
        /// <param name="timeFromStartMS"></param>
        /// <returns></returns>
        public List<MPTKEvent> ReadChangeFromStart(int position)
        {
            List<MPTKEvent> midievents = new List<MPTKEvent>(); ;
            try
            {
                if (midifile != null)
                {
                    if (position < 0 || position >= MidiSorted.Count)
                        position = MidiSorted.Count - 1;

                    for (int currentPosEvent = 0; currentPosEvent < position; currentPosEvent++)
                    {
                        TrackMidiEvent trackEvent = MidiSorted[currentPosEvent];
                        MPTKEvent midievent = null;
                        switch (trackEvent.Event.CommandCode)
                        {
                            case MidiCommandCode.ControlChange:
                                ControlChangeEvent controlchange = (ControlChangeEvent)trackEvent.Event;
                                midievent = new MPTKEvent()
                                {
                                    Tick = trackEvent.AbsoluteQuantize,
                                    Command = MPTKCommand.ControlChange,
                                    Channel = trackEvent.Event.Channel - 1,
                                    Controller = (MPTKController)controlchange.Controller,
                                    Value = controlchange.ControllerValue,

                                };
                                break;
                            case MidiCommandCode.PatchChange:
                                PatchChangeEvent change = (PatchChangeEvent)trackEvent.Event;
                                midievent = new MPTKEvent()
                                {
                                    Tick = trackEvent.AbsoluteQuantize,
                                    Command = MPTKCommand.PatchChange,
                                    Channel = trackEvent.Event.Channel - 1,
                                    Value = change.Patch,
                                };
                                break;

                            case MidiCommandCode.MetaEvent:
                                MetaEvent meta = (MetaEvent)trackEvent.Event;
                                if (meta.MetaEventType == MetaEventType.SetTempo)
                                {
                                    TempoEvent tempo = (TempoEvent)meta;
                                    midievent = new MPTKEvent()
                                    {
                                        Tick = trackEvent.AbsoluteQuantize,
                                        Command = MPTKCommand.MetaEvent,
                                        Channel = trackEvent.Event.Channel - 1,
                                        Meta = (MPTKMeta)meta.MetaEventType,
                                        Duration = tempo.Tempo,
                                    };
                                }
                                break;
                        }
                        if (midievent != null)
                            midievents.Add(midievent);
                    }

                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
            return midievents;
        }

        private string BuildInfoTrack(TrackMidiEvent e)
        {
            return string.Format("[A:{0,5:00000} Q:{1,5:00000} P:{2,5:00000}] [T:{3,2:00} C:{4,2:00}] ", e.Event.AbsoluteTime, e.AbsoluteQuantize, CurrentTick, e.IndexTrack, e.Event.Channel);
        }

        public void DebugTrack()
        {
            int itrck = 0;
            foreach (IList<MidiEvent> track in midifile.Events)
            {
                itrck++;
                foreach (MidiEvent midievent in track)
                {
                    string info = string.Format("Track:{0} Channel:{1,2:00} Command:{2} AbsoluteTime:{3:0000000} ", itrck, midievent.Channel, midievent.CommandCode, midievent.AbsoluteTime);
                    if (midievent.CommandCode == MidiCommandCode.NoteOn)
                    {
                        NoteOnEvent noteon = (NoteOnEvent)midievent;
                        if (noteon.OffEvent == null)
                            info += string.Format(" OffEvent null");
                        else
                            info += string.Format(" OffEvent.DeltaTimeChannel:{0:0000.00} ", noteon.OffEvent.DeltaTime);
                    }
                    Debug.Log(info);
                }
            }
        }
        public void DebugMidiSorted()
        {
            foreach (TrackMidiEvent midievent in MidiSorted)
            {
                string info = string.Format("Track:{0} Channel:{1,2:00} Command:{2} AbsoluteTime:{3:0000000} ", midievent.IndexTrack, midievent.Event.Channel, midievent.Event.CommandCode, midievent.Event.AbsoluteTime);
                switch (midievent.Event.CommandCode)
                {
                    case MidiCommandCode.NoteOn:
                        NoteOnEvent noteon = (NoteOnEvent)midievent.Event;
                        if (noteon.Velocity == 0)
                            info += string.Format(" Velocity 0");
                        if (noteon.OffEvent == null)
                            info += string.Format(" OffEvent null");
                        else
                            info += string.Format(" OffEvent.DeltaTimeChannel:{0:0000.00} ", noteon.OffEvent.DeltaTime);
                        break;
                }
                Debug.Log(info);
            }
        }
        //! @endcond
    }
}

