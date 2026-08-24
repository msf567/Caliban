using System;

namespace Caliban.Core.Cinematics
{
    public readonly struct CinematicCue
    {
        public readonly TimeSpan Time;
        public readonly string Label;

        public CinematicCue(TimeSpan _time, string _label)
        {
            Time = _time;
            Label = _label;
        }
    }
}