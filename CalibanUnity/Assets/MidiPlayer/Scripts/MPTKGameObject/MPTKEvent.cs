using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;

namespace MidiPlayerTK
{
    /// <summary>
    /// MIDI command codes
    /// </summary>
    public enum MPTKCommand : byte
    {
        /// <summary>Note Off</summary>
        NoteOff = 0x80,
        /// <summary>Note On</summary>
        NoteOn = 0x90,
        /// <summary>Key After-touch</summary>
        KeyAfterTouch = 0xA0,
        /// <summary>Control change</summary>
        ControlChange = 0xB0,
        /// <summary>Patch change</summary>
        PatchChange = 0xC0,
        /// <summary>Channel after-touch</summary>
        ChannelAfterTouch = 0xD0,
        /// <summary>Pitch wheel change</summary>
        PitchWheelChange = 0xE0,
        /// <summary>Sysex message</summary>
        Sysex = 0xF0,
        /// <summary>Eox (comes at end of a sysex message)</summary>
        Eox = 0xF7,
        /// <summary>Timing clock (used when synchronization is required)</summary>
        TimingClock = 0xF8,
        /// <summary>Start sequence</summary>
        StartSequence = 0xFA,
        /// <summary>Continue sequence</summary>
        ContinueSequence = 0xFB,
        /// <summary>Stop sequence</summary>
        StopSequence = 0xFC,
        /// <summary>Auto-Sensing</summary>
        AutoSensing = 0xFE,
        /// <summary>Meta-event</summary>
        MetaEvent = 0xFF,
    }

    /// <summary>
    /// MidiController enumeration
    /// http://www.midi.org/techspecs/midimessages.php#3
    /// </summary>
    public enum MPTKController : byte
    {
        /// <summary>Bank Select (MSB)</summary>
        BankSelect = 0,
        /// <summary>Modulation (MSB)</summary>
        Modulation = 1,
        /// <summary>Breath Controller</summary>
        BreathController = 2,
        /// <summary>Foot controller (MSB)</summary>
        FootController = 4,
        /// <summary>Main volume</summary>
        MainVolume = 7,
        /// <summary>Pan</summary>
        Pan = 10,
        /// <summary>Expression</summary>
        Expression = 11,
        /// <summary>Bank Select LSB ** not implemented **  </summary>
        BankSelectLsb = 32,
        /// <summary>Sustain</summary>
        Sustain = 64, // 0x40
        /// <summary>Portamento On/Off</summary>
        Portamento = 65,
        /// <summary>Sostenuto On/Off</summary>
        Sostenuto = 66,
        /// <summary>Soft Pedal On/Off</summary>
        SoftPedal = 67,
        /// <summary>Legato Footswitch</summary>
        LegatoFootswitch = 68,
        /// <summary>Reset all controllers</summary>
        ResetAllControllers = 121,
        /// <summary>All notes off</summary>
        AllNotesOff = 123,
        /// <summary>All sound off</summary>
        AllSoundOff = 120, // 0x78,
    }

    /// <summary>
    /// MIDI MetaEvent Type
    /// </summary>
    public enum MPTKMeta : byte
    {
        /// <summary>Track sequence number</summary>
        TrackSequenceNumber = 0x00,
        /// <summary>Text event</summary>
        TextEvent = 0x01,
        /// <summary>Copyright</summary>
        Copyright = 0x02,
        /// <summary>Sequence track name</summary>
        SequenceTrackName = 0x03,
        /// <summary>Track instrument name</summary>
        TrackInstrumentName = 0x04,
        /// <summary>Lyric</summary>
        Lyric = 0x05,
        /// <summary>Marker</summary>
        Marker = 0x06,
        /// <summary>Cue point</summary>
        CuePoint = 0x07,
        /// <summary>Program (patch) name</summary>
        ProgramName = 0x08,
        /// <summary>Device (port) name</summary>
        DeviceName = 0x09,
        /// <summary>MIDI Channel (not official?)</summary>
        MidiChannel = 0x20,
        /// <summary>MIDI Port (not official?)</summary>
        MidiPort = 0x21,
        /// <summary>End track</summary>
        EndTrack = 0x2F,
        /// <summary>Set tempo</summary>
        SetTempo = 0x51,
        /// <summary>SMPTE offset</summary>
        SmpteOffset = 0x54,
        /// <summary>Time signature</summary>
        TimeSignmature = 0x58,
        /// <summary>Key signature</summary>
        KeySignature = 0x59,
        /// <summary>Sequencer specific</summary>
        SequencerSpecific = 0x7F,
    }

    /// <summary>
    /// Midi Event class for MPTK. Usage to generate Midi Music with MidiStreamPlayer or to read midi events from a Midi file with MidiLoad 
    /// or to recevice midi events from MidiFilePlayer OnEventNotesMidi.
    /// </summary>
    public class MPTKEvent
    {
        /// <summary>
        /// Time in Midi Tick (part of a Beat) of the Event since the start of playing the midi file. This time is independant of the Tempo or Speed. Not used for MidiStreamPlayer.
        /// </summary>
        public long Tick;

        /// <summary>
        /// Midi Command code. Defined the type of message (Note On, Control Change, Patch Change...)
        /// </summary>
        public MPTKCommand Command;

        /// <summary>
        /// Controller code. When the Command is ControlChange, contains the code fo the controller to change (Modulation, Pan, Bank Select ...). Value will contains the value of the controller.
        /// </summary>
        public MPTKController Controller;

        /// <summary>
        /// MetaEvent Code. When the Command is MetaEvent, contains the code of the meta event (Lyric, TimeSignature, ...). . Info will contains the value of the meta.
        /// </summary>
        public MPTKMeta Meta;

        /// <summary>
        /// Information hold by textual meta event when Command=MetaEvent
        /// </summary>
        public string Info;

        /// <summary>
        /// Contains a value between 0 and 127 in relation with the Command. For:
        ///! @li @c   Command = NoteOn then Value contains midi note
        ///! @li @c   Command = ControlChange then Value contains controller value
        ///! @li @c   Command = PatchChange then Value contains patch value
        /// </summary>
        public int Value;

        /// <summary>
        /// Midi channel fom 0 to 15 (9 for drum)
        /// </summary>
        public int Channel;

        /// <summary>
        /// Velocity between 0 and 127
        /// </summary>
        public int Velocity;

        /// <summary>
        /// Duration of the note in millisecond
        /// </summary>
        public double Duration;

        /// <summary>
        /// Duration of the note in Midi Tick. MidiFilePlayer.MPTK_NoteLength can be used to convert this duration. Not used for MidiStreamPlayer.
        /// https://en.wikipedia.org/wiki/Note_value
        /// </summary>
        public int Length;

        /// <summary>
        /// Note length as https://en.wikipedia.org/wiki/Note_value
        /// </summary>
        public enum EnumLength { Whole, Half, Quarter, Eighth, Sixteenth }

        /// <summary>
        /// List of voices associated to this Event for playing a NoteOn event.
        /// </summary>
        public List<fluid_voice> Voices;

        public MPTKEvent()
        {
            Command = MPTKCommand.NoteOn;
        }

        /// <summary>
        /// Play a note which is stoppable. DEPRECATED in V2. Replaced by MPTK_PlayEvent in MidiStreamPlayer.
        /// </summary>
        /// <param name="streamPlayer">A MidiStreamPlayer component</param>
        public void Play(MidiStreamPlayer streamPlayer)
        {
            Debug.LogWarning("Play() is deprecated in V2, replaced by MPTK_PlayEvent in MidiStreamPlayer.");
        }

        /// <summary>
        /// Stop the note. DEPRECATED in V2. Replaced by MPTK_StopEvent in MidiStreamPlayer.
        /// </summary>
        public void Stop()
        {
            Debug.LogWarning("Stop() is deprecated in V2, replaced by MPTK_StopEvent in MidiStreamPlayer.");
        }
    }
}
