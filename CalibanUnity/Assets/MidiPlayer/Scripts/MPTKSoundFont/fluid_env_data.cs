using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MidiPlayerTK
{

    /*
     * envelope data
     */
    public class fluid_env_data
    {
        public double count;
        public double coeff;
        public double incr;
        public double min;
        public double max;
        public override string ToString()
        {
            return string.Format("count:{0,-20:F4} coeff:{1,-12:F4} incr:{2,-12:F6} min:{3,-12:F4} max:{4,-12:F4}", count, coeff, incr, min, max);
        }
    }

    /* Indices for envelope tables */
    public enum fluid_voice_envelope_index
    {
        FLUID_VOICE_ENVDELAY,
        FLUID_VOICE_ENVATTACK,
        FLUID_VOICE_ENVHOLD,
        FLUID_VOICE_ENVDECAY,
        FLUID_VOICE_ENVSUSTAIN,
        FLUID_VOICE_ENVRELEASE,
        FLUID_VOICE_ENVFINISHED,
        FLUID_VOICE_ENVLAST
    }
}
