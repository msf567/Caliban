﻿using NAudio.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MidiPlayerTK
{
    /// <summary>
    /// Midi event list (NAUdio format)
    /// </summary>
    public class TrackMidiEvent
    {
        public int IndexTrack;
        public long AbsoluteQuantize;
        public MidiEvent Event;
    }
}
