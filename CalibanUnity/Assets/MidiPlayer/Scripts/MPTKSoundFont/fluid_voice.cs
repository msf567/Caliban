//#define DEBUGPERF
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;
using MEC;

namespace MidiPlayerTK
{

    /* for fluid_voice_add_mod */
    public enum fluid_voice_addorover_mod
    {
        FLUID_VOICE_OVERWRITE,
        FLUID_VOICE_ADD,
        FLUID_VOICE_DEFAULT
    }

    public enum fluid_voice_status
    {
        FLUID_VOICE_CLEAN,
        FLUID_VOICE_ON,
        FLUID_VOICE_SUSTAINED,
        FLUID_VOICE_OFF
    }


    public class fluid_voice : MonoBehaviour
    {
        public MidiSynth synth;
        public AudioSource Audiosource;
        public AudioLowPassFilter LowPassFilter;
        public AudioReverbFilter ReverbFilter;
        public AudioChorusFilter ChorusFilter;

        //public bool DspIsStarted;
        /// <summary>
        /// Real time at start of the voice in ms
        /// </summary>
        public double TimeAtStart;
        /// <summary>
        /// Real time from start of the voice in ms
        /// </summary>
        public double TimeFromStart;
        public double LastTimeWrite;
        /// <summary>
        /// Delay in ms between call to fluid_voice_write
        /// </summary>
        public double DeltaTimeWrite;
        static public int LastId;
        public int IdVoice;

        /// <summary>
        ///  MPTK specific - Note duration in ms. Set to -1 to indefinitely
        /// </summary>
        public int Duration;

        // min vol envelope release (to stop clicks) in SoundFont timecents : ~16ms 
        public const int NO_CHANNEL = 0xff;

        /* used for filter turn off optimization - if filter cutoff is above the
           specified value and filter q is below the other value, turn filter off */
        //public const float FLUID_MAX_AUDIBLE_FILTER_FC = 19000.0f;
        //public const float FLUID_MIN_AUDIBLE_FILTER_Q = 1.2f;

        /* Smallest amplitude that can be perceived (full scale is +/- 0.5)
         * 16 bits => 96+4=100 dB dynamic range => 0.00001
         * 0.00001 * 2 is approximately 0.00003 :)
         */
        //public const float FLUID_NOISE_FLOOR = 0.00003f;

        /* these should be the absolute minimum that FluidSynth can deal with */
        //public const int FLUID_MIN_LOOP_SIZE = 2;
        //public const int FLUID_MIN_LOOP_PAD = 0;

        /* min vol envelope release (to stop clicks) in SoundFont timecents */
        public const float FLUID_MIN_VOLENVRELEASE = -7200.0f;/* ~16ms */

        //public const double M_PI = 3.1415926535897932384626433832795;


        public fluid_voice_status status;
        public int chan;             /* the channel number, quick access for channel messages */
        public int key;              /* the key, quick acces for noteoff */
        public int vel;              /* the velocity */
        public fluid_channel channel;
        public HiGen[] gens; //[GEN_LAST];
        public List<HiMod> mods; //[FLUID_NUM_MOD];
        public int mod_count;
        //public bool has_looped;                 /* Flag that is set as soon as the first loop is completed. */
        public HiSample sample;
        //public int check_sample_sanity_flag;   /* Flag that initiates, that sample-related parameters have to be checked. */
        //public double output_rate;        /* the sample rate of the synthesizer */

        //public uint start_time;
        //public uint ticks;

        //public float amp;                /* current linear amplitude */
        //public long /*fluid_phase_t*/ phase;             /* the phase of the sample wave */

        /* Temporary variables used in fluid_voice_write() */

        //public double phase_incr;    /* the phase increment for the next 64 samples */
        //public double amp_incr;      /* amplitude increment value */
        //public /*double**/double[] dsp_buf;      /* buffer to store interpolated sample data to */

        /* End temporary variables */

        /* basic parameters */
        public double pitch_midicents;    /* the pitch in midicents */
        public double pitch;              /* the pitch to AudioSource */
        public double attenuation;        /* the attenuation in centibels */
        //public double min_attenuation_cB; /* Estimate on the smallest possible attenuation during the lifetime of the voice */
        public double root_pitch;

        /* sample and loop start and end points (offset in sample memory).  */
        //public int start;
        //public int end;
        //public int loopstart;
        //public int loopend;    /* Note: first point following the loop (superimposed on loopstart) */

        /* master gain */
        //public double synth_gain;

        /// <summary>
        /// volume enveloppe
        /// </summary>
        public fluid_env_data[] volenv_data; //[FLUID_VOICE_ENVLAST];

        /// <summary>
        /// Count time since the start of the section
        /// </summary>
        public double volenv_count;

        /// <summary>
        /// Current section in the enveloppe
        /// </summary>
        public fluid_voice_envelope_index volenv_section;

        public double volenv_val;

        //public double amplitude_that_reaches_noise_floor_nonloop;
        //public double amplitude_that_reaches_noise_floor_loop;

        /* mod env */
        public fluid_env_data[] modenv_data;
        public double modenv_count;
        public fluid_voice_envelope_index modenv_section;
        public double modenv_val;         /* the value of the modulation envelope */
        public double modenv_to_fc;
        public double modenv_to_pitch;

        /* mod lfo */
        public double modlfo_val;          /* the value of the modulation LFO */
        public double modlfo_delay;       /* the delay of the lfo in samples */
        public double modlfo_incr;         /* the lfo frequency is converted to a per-buffer increment */
        public double modlfo_to_fc;
        public double modlfo_to_pitch;
        public double modlfo_to_vol;

        /* vib lfo */
        public double viblfo_val;        /* the value of the vibrato LFO */
        public double viblfo_delay;      /* the delay of the lfo in samples */
        public double viblfo_incr;       /* the lfo frequency is converted to a per-buffer increment */
        public double viblfo_to_pitch;

        /* resonant filter */
        public double Fres;              /* the resonance frequency, in cents (not absolute cents) */
        public double last_fres;         /* Current resonance frequency of the IIR filter */
                                         /* Serves as a flag: A deviation between fres and last_fres */
                                         /* indicates, that the filter has to be recalculated. */
        public double q_lin;             /* the q-factor on a linear scale */
        public double filter_gain;       /* Gain correction factor, depends on q */
        //public double hist1, hist2;      /* Sample history for the IIR filter */
        //public bool filter_startup;             /* Flag: If set, the filter will be set directly. Else it changes smoothly. */

        /* filter coefficients */
        /* The coefficients are normalized to a0. */
        /* b0 and b2 are identical => b02 */
        //public double b02;             /* b0 / a0 */
        //public double b1;              /* b1 / a0 */
        //public double a1;              /* a0 / a0 */
        //public double a2;              /* a1 / a0 */

        //public double b02_incr;
        //public double b1_incr;
        //public double a1_incr;
        //public double a2_incr;
        //public int filter_coeff_incr_count;

        /* pan */
        public double pan;
        //public double amp_left;
        //public double amp_right;

        public float reverb_send;
        public float chorus_send;

        /* interpolation method, as in fluid_interp in fluidsynth.h */
        //public fluid_interp interp_method;

        /* for debugging */
        //public int debug;
        //TBC double ref;


        static int[] list_of_generators_to_initialize =
             {
                //(int)fluid_gen_type.GEN_STARTADDROFS,                    /* SF2.01 page 48 #0  - Unity load wave from wave file, no real time change possible on wave attribute */
                //(int)fluid_gen_type.GEN_ENDADDROFS,                      /*                #1  - Unity load wave from wave file, no real time change possible on wave attribute */
                //(int)fluid_gen_type.GEN_STARTLOOPADDROFS,                /*                #2  - Unity load wave from wave file, no real time change possible on wave attribute */
                //(int)fluid_gen_type.GEN_ENDLOOPADDROFS,                  /*                #3  - Unity load wave from wave file, no real time change possible on wave attribute */
                /* (int)fluid_gen_type.GEN_STARTADDRCOARSEOFS see comment below [1]        #4  - Unity load wave from wave file, no real time change possible on wave attribute */
                (int)fluid_gen_type.GEN_MODLFOTOPITCH,                   /*                #5   */
                (int)fluid_gen_type.GEN_VIBLFOTOPITCH,                   /*                #6   */
                (int)fluid_gen_type.GEN_MODENVTOPITCH,                   /*                #7   */
                (int)fluid_gen_type.GEN_FILTERFC,                        /*                #8   */
                (int)fluid_gen_type.GEN_FILTERQ,                         /*                #9   */
                (int)fluid_gen_type.GEN_MODLFOTOFILTERFC,                /*                #10  */
                (int)fluid_gen_type.GEN_MODENVTOFILTERFC,                /*                #11  */
                /* (int)fluid_gen_type.GEN_ENDADDRCOARSEOFS [1]                            #12  - Unity load wave from wave file, no real time change possible on wave attribute */
                (int)fluid_gen_type.GEN_MODLFOTOVOL,                     /*                #13  */
                /* not defined                                         #14  */
                (int)fluid_gen_type.GEN_CHORUSSEND,                      /*                #15  */
                (int)fluid_gen_type.GEN_REVERBSEND,                      /*                #16  */
                (int)fluid_gen_type.GEN_PAN,                             /*                #17  */
                /* not defined                                         #18  */
                /* not defined                                         #19  */
                /* not defined                                         #20  */
                (int)fluid_gen_type.GEN_MODLFODELAY,                     /*                #21  */
                (int)fluid_gen_type.GEN_MODLFOFREQ,                      /*                #22  */
                (int)fluid_gen_type.GEN_VIBLFODELAY,                     /*                #23  */
                (int)fluid_gen_type.GEN_VIBLFOFREQ,                      /*                #24  */
                (int)fluid_gen_type.GEN_MODENVDELAY,                     /*                #25  */
                (int)fluid_gen_type.GEN_MODENVATTACK,                    /*                #26  */
                (int)fluid_gen_type.GEN_MODENVHOLD,                      /*                #27  */
                (int)fluid_gen_type.GEN_MODENVDECAY,                     /*                #28  */
                /* (int)fluid_gen_type.GEN_MODENVSUSTAIN [1]                               #29  */
                (int)fluid_gen_type.GEN_MODENVRELEASE,                   /*                #30  */
                /* (int)fluid_gen_type.GEN_KEYTOMODENVHOLD [1]                             #31  */
                /* (int)fluid_gen_type.GEN_KEYTOMODENVDECAY [1]                            #32  */
                (int)fluid_gen_type.GEN_VOLENVDELAY,                     /*                #33  */
                (int)fluid_gen_type.GEN_VOLENVATTACK,                    /*                #34  */
                (int)fluid_gen_type.GEN_VOLENVHOLD,                      /*                #35  */
                (int)fluid_gen_type.GEN_VOLENVDECAY,                     /*                #36  */
                /* (int)fluid_gen_type.GEN_VOLENVSUSTAIN [1]                               #37  */
                (int)fluid_gen_type.GEN_VOLENVRELEASE,                   /*                #38  */
                /* (int)fluid_gen_type.GEN_KEYTOVOLENVHOLD [1]                             #39  */
                /* (int)fluid_gen_type.GEN_KEYTOVOLENVDECAY [1]                            #40  */
                /* (int)fluid_gen_type.GEN_STARTLOOPADDRCOARSEOFS [1]                      #45 - Unity load wave from wave file, no real time change possible on wave attribute */
                (int)fluid_gen_type.GEN_KEYNUM,                          /*                #46  */
                (int)fluid_gen_type.GEN_VELOCITY,                        /*                #47  */
                (int)fluid_gen_type.GEN_ATTENUATION,                     /*                #48  */
                /* (int)fluid_gen_type.GEN_ENDLOOPADDRCOARSEOFS [1]                        #50  - Unity load wave from wave file, no real time change possible on wave attribute */
                /* (int)fluid_gen_type.GEN_COARSETUNE           [1]                        #51  */
                /* (int)fluid_gen_type.GEN_FINETUNE             [1]                        #52  */
                (int)fluid_gen_type.GEN_OVERRIDEROOTKEY,                 /*                #58  */
                (int)fluid_gen_type.GEN_PITCH,                           /*                ---  */
             };

        static int[] list_of_weakgenerators_to_initialize =
     {
                //(int)fluid_gen_type.GEN_STARTADDROFS,                    /* SF2.01 page 48 #0  - Unity load wave from wave file, no real time change possible on wave attribute */
                //(int)fluid_gen_type.GEN_ENDADDROFS,                      /*                #1  - Unity load wave from wave file, no real time change possible on wave attribute */
                //(int)fluid_gen_type.GEN_STARTLOOPADDROFS,                /*                #2  - Unity load wave from wave file, no real time change possible on wave attribute */
                //(int)fluid_gen_type.GEN_ENDLOOPADDROFS,                  /*                #3  - Unity load wave from wave file, no real time change possible on wave attribute */
                /* (int)fluid_gen_type.GEN_STARTADDRCOARSEOFS see comment below [1]        #4  - Unity load wave from wave file, no real time change possible on wave attribute */
                //(int)fluid_gen_type.GEN_MODLFOTOPITCH,                   /*                #5   */
                //(int)fluid_gen_type.GEN_VIBLFOTOPITCH,                   /*                #6   */
                //(int)fluid_gen_type.GEN_MODENVTOPITCH,                   /*                #7   */
                //(int)fluid_gen_type.GEN_FILTERFC,                        /*                #8   */
                //(int)fluid_gen_type.GEN_FILTERQ,                         /*                #9   */
                //(int)fluid_gen_type.GEN_MODLFOTOFILTERFC,                /*                #10  */
                //(int)fluid_gen_type.GEN_MODENVTOFILTERFC,                /*                #11  */
                /* (int)fluid_gen_type.GEN_ENDADDRCOARSEOFS [1]                            #12  - Unity load wave from wave file, no real time change possible on wave attribute */
                //(int)fluid_gen_type.GEN_MODLFOTOVOL,                     /*                #13  */
                /* not defined                                         #14  */
                //(int)fluid_gen_type.GEN_CHORUSSEND,                      /*                #15  */
                //(int)fluid_gen_type.GEN_REVERBSEND,                      /*                #16  */
                (int)fluid_gen_type.GEN_PAN,                             /*                #17  */
                /* not defined                                         #18  */
                /* not defined                                         #19  */
                /* not defined                                         #20  */
                //(int)fluid_gen_type.GEN_MODLFODELAY,                     /*                #21  */
                //(int)fluid_gen_type.GEN_MODLFOFREQ,                      /*                #22  */
                //(int)fluid_gen_type.GEN_VIBLFODELAY,                     /*                #23  */
                //(int)fluid_gen_type.GEN_VIBLFOFREQ,                      /*                #24  */
                //(int)fluid_gen_type.GEN_MODENVDELAY,                     /*                #25  */
                //(int)fluid_gen_type.GEN_MODENVATTACK,                    /*                #26  */
                //(int)fluid_gen_type.GEN_MODENVHOLD,                      /*                #27  */
                //(int)fluid_gen_type.GEN_MODENVDECAY,                     /*                #28  */
                /* (int)fluid_gen_type.GEN_MODENVSUSTAIN [1]                               #29  */
                //(int)fluid_gen_type.GEN_MODENVRELEASE,                   /*                #30  */
                /* (int)fluid_gen_type.GEN_KEYTOMODENVHOLD [1]                             #31  */
                /* (int)fluid_gen_type.GEN_KEYTOMODENVDECAY [1]                            #32  */
                //(int)fluid_gen_type.GEN_VOLENVDELAY,                     /*                #33  */
                //(int)fluid_gen_type.GEN_VOLENVATTACK,                    /*                #34  */
                //(int)fluid_gen_type.GEN_VOLENVHOLD,                      /*                #35  */
                //(int)fluid_gen_type.GEN_VOLENVDECAY,                     /*                #36  */
                /* (int)fluid_gen_type.GEN_VOLENVSUSTAIN [1]                               #37  */
                //(int)fluid_gen_type.GEN_VOLENVRELEASE,                   /*                #38  */
                /* (int)fluid_gen_type.GEN_KEYTOVOLENVHOLD [1]                             #39  */
                /* (int)fluid_gen_type.GEN_KEYTOVOLENVDECAY [1]                            #40  */
                /* (int)fluid_gen_type.GEN_STARTLOOPADDRCOARSEOFS [1]                      #45 - Unity load wave from wave file, no real time change possible on wave attribute */
                (int)fluid_gen_type.GEN_KEYNUM,                          /*                #46  */
                (int)fluid_gen_type.GEN_VELOCITY,                        /*                #47  */
                (int)fluid_gen_type.GEN_ATTENUATION,                     /*                #48  */
                /* (int)fluid_gen_type.GEN_ENDLOOPADDRCOARSEOFS [1]                        #50  - Unity load wave from wave file, no real time change possible on wave attribute */
                /* (int)fluid_gen_type.GEN_COARSETUNE           [1]                        #51  */
                /* (int)fluid_gen_type.GEN_FINETUNE             [1]                        #52  */
                (int)fluid_gen_type.GEN_OVERRIDEROOTKEY,                 /*                #58  */
                (int)fluid_gen_type.GEN_PITCH,                           /*                ---  */
             };


        const float _ratioHalfTone = 1.0594630943592952645618252949463f;

        private bool weakDevice;
        //public fluid_voice_t(fluid_synth_t psynth, HiSample psample, AudioClip clip)
        //{

        //}

        public void Awake()
        {
            //Debug.Log("Awake fluid_voice");
            IdVoice = LastId++;
            if (Audiosource == null)
                Debug.LogWarning("No AudioSource attached to the voice. Check VoiceTemplate in your Hierarchy.");
        }

        public void Start()
        {
            //Debug.Log("Start fluid_voice");
            //Audiosource.PlayOneShot(new AudioClip(), 0);
        }

        /// <summary>
        /// Defined default voice value even when reuse of a voice
        /// </summary>
        /// <param name="psample"></param>
        /// <param name="pchannel"></param>
        /// <param name="pkey"></param>
        /// <param name="pvel"></param>
        public void fluid_voice_init(HiSample psample, fluid_channel pchannel, int pkey, int pvel/*, double gain*/)
        {
            /* Note: The voice parameters will be initialized later, when the
             * generators have been retrieved from the sound font. Here, only
             * the 'working memory' of the voice (position in envelopes, history
             * of IIR filters, position in sample etc) is initialized. */

            // Play note
            // Timing.RunCoroutine(PlayNote(audioSelected, note.Drum, smpl, note, timeToRelease));
            weakDevice = synth.MPTK_WeakDevice;

            sample = psample;

            gens = new HiGen[Enum.GetNames(typeof(fluid_gen_type)).Length];
            for (int i = 0; i < gens.Length; i++)
            {
                gens[i] = new HiGen();
                gens[i].type = (fluid_gen_type)i;
                gens[i].flags = fluid_gen_flags.GEN_UNUSED;
            }
#if DEBUGPERF
            synth.DebugPerf("After HiGen GetNames:");
#endif
            if (!weakDevice)
            {
                modenv_data = new fluid_env_data[Enum.GetNames(typeof(fluid_voice_envelope_index)).Length];
                for (int i = 0; i < modenv_data.Length; i++)
                    modenv_data[i] = new fluid_env_data();

                volenv_data = new fluid_env_data[Enum.GetNames(typeof(fluid_voice_envelope_index)).Length];
                for (int i = 0; i < volenv_data.Length; i++)
                    volenv_data[i] = new fluid_env_data();

                // The 'sustain' and 'finished' segments of the volume / modulation envelope are constant. 
                // They are never affected by any modulator or generator. 
                // Therefore it is enough to initialize them once during the lifetime of the synth.

                volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVSUSTAIN].count = 0xffffffff; // infiny until note off or duration is over
                volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVSUSTAIN].coeff = 1d;
                volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVSUSTAIN].incr = 0d;          // Volume remmains constant during sustain phase
                volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVSUSTAIN].min = -1d;          // not used for sustain (constant volume)
                volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVSUSTAIN].max = 1d; //2d;     // not used for sustain (constant volume)

                volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVFINISHED].count = 0xffffffff;
                volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVFINISHED].coeff = 0d;
                volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVFINISHED].incr = 0d;
                volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVFINISHED].min = -1d;
                volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVFINISHED].max = 1d;

                modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVSUSTAIN].count = 0xffffffff;
                modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVSUSTAIN].coeff = 1d;
                modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVSUSTAIN].incr = 0d;
                modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVSUSTAIN].min = -1d;
                modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVSUSTAIN].max = 1d; //2d;

                modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVFINISHED].count = 0xffffffff;
                modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVFINISHED].coeff = 0d;
                modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVFINISHED].incr = 0d;
                modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVFINISHED].min = -1d;
                modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVFINISHED].max = 1d;
            }
            channel = pchannel;
            chan = channel.channum;
            key = pkey;
            vel = pvel;
            mod_count = 0;
            //start_time = pstart_time;
            //ticks = 0;
            //has_looped = false; /* Will be set during voice_write when the 2nd loop point is reached */
            last_fres = -1; /* The filter coefficients have to be calculated later in the DSP loop. */
            //filter_startup = true; /* Set the filter immediately, don't fade between old and new settings */
            //interp_method = channel.interp_method;

            /* vol env initialization */
            volenv_count = 0;
            volenv_section = (fluid_voice_envelope_index)0;
            volenv_val = 0d;
            //amp = 0.0f; /* The last value of the volume envelope, used to calculate the volume increment during processing */

            /* mod env initialization*/
            modenv_count = 0;
            modenv_section = (fluid_voice_envelope_index)0;
            modenv_val = 0d;

            /* mod lfo */
            modlfo_val = 0d;/* Fixme: Retrieve from any other existing voice on this channel to keep LFOs in unison? */

            /* vib lfo */
            viblfo_val = 0d; /* Fixme: See mod lfo */

            /* Clear sample history in filter */
            //hist1 = 0;
            //hist2 = 0;

            /* Set all the generators to their default value, according to SF
             * 2.01 section 8.1.3 (page 48). The value of NRPN messages are
             * copied from the channel to the voice's generators. The sound font
             * loader overwrites them. The generator values are later converted
             * into voice parameters in
             * fluid_voice_calculate_runtime_synthesis_parameters.  */
            HiGen.fluid_gen_init(gens, channel);
#if DEBUGPERF
            synth.DebugPerf("After fluid_gen_init voice:");
#endif
            //synth_gain = gain;
            ///* avoid division by zero later*/
            //if (synth_gain < 0.0000001)
            //{
            //    synth_gain = 0.0000001;
            //}

            /* add the default modulators to the synthesis process. */
            mods = new List<HiMod>();
            fluid_voice_add_mod(MidiSynth.default_vel2att_mod, fluid_voice_addorover_mod.FLUID_VOICE_DEFAULT);    /* SF2.01 $8.4.1  */
            fluid_voice_add_mod(MidiSynth.default_vel2filter_mod, fluid_voice_addorover_mod.FLUID_VOICE_DEFAULT); /* SF2.01 $8.4.2  */
            fluid_voice_add_mod(MidiSynth.default_at2viblfo_mod, fluid_voice_addorover_mod.FLUID_VOICE_DEFAULT);  /* SF2.01 $8.4.3  */
            fluid_voice_add_mod(MidiSynth.default_mod2viblfo_mod, fluid_voice_addorover_mod.FLUID_VOICE_DEFAULT); /* SF2.01 $8.4.4  */
            fluid_voice_add_mod(MidiSynth.default_att_mod, fluid_voice_addorover_mod.FLUID_VOICE_DEFAULT);        /* SF2.01 $8.4.5  */
            fluid_voice_add_mod(MidiSynth.default_pan_mod, fluid_voice_addorover_mod.FLUID_VOICE_DEFAULT);        /* SF2.01 $8.4.6  */
            fluid_voice_add_mod(MidiSynth.default_expr_mod, fluid_voice_addorover_mod.FLUID_VOICE_DEFAULT);       /* SF2.01 $8.4.7  */
            fluid_voice_add_mod(MidiSynth.default_reverb_mod, fluid_voice_addorover_mod.FLUID_VOICE_DEFAULT);     /* SF2.01 $8.4.8  */
            fluid_voice_add_mod(MidiSynth.default_chorus_mod, fluid_voice_addorover_mod.FLUID_VOICE_DEFAULT);     /* SF2.01 $8.4.9  */
            fluid_voice_add_mod(MidiSynth.default_pitch_bend_mod, fluid_voice_addorover_mod.FLUID_VOICE_DEFAULT); /* SF2.01 $8.4.10 */
#if DEBUGPERF
            synth.DebugPerf("After fluid_voice_add_mod:");
#endif
            /* For a looped sample, this value will be overwritten as soon as the
             * loop parameters are initialized (they may depend on modulators).
             * This value can be kept, it is a worst-case estimate.
             */

            //amplitude_that_reaches_noise_floor_nonloop = FLUID_NOISE_FLOOR / synth_gain;
            //amplitude_that_reaches_noise_floor_loop = FLUID_NOISE_FLOOR / synth_gain;

            // Increment the reference count of the sample to prevent the unloading of the soundfont while this voice is playing.
            //sample.refcount++;
        }



        /// <summary>
        ///  Adds a modulator to the voice.  "mode" indicates, what to do, if an identical modulator exists already.
        /// mode == FLUID_VOICE_ADD: Identical modulators on preset level are added
        /// mode == FLUID_VOICE_OVERWRITE: Identical modulators on instrument level are overwritten
        /// mode == FLUID_VOICE_DEFAULT: This is a default modulator, there can be no identical modulator.Don't check.
        /// </summary>
        /// <param name="pmod"></param>
        /// <param name="mode"></param>
        public void fluid_voice_add_mod(HiMod pmod, fluid_voice_addorover_mod mode)
        {
            if (mode == fluid_voice_addorover_mod.FLUID_VOICE_ADD || mode == fluid_voice_addorover_mod.FLUID_VOICE_OVERWRITE)
            {
                foreach (HiMod mod1 in this.mods)
                {
                    /* if identical modulator exists, add them */
                    //fluid_mod_test_identity(mod1, mod))
                    if ((mod1.Dest == pmod.Dest) &&
                        (mod1.Src1 == pmod.Src1) &&
                        (mod1.Src2 == pmod.Src2) &&
                        (mod1.Flags1 == pmod.Flags1) &&
                        (mod1.Flags2 == pmod.Flags2))
                    {
                        if (mode == fluid_voice_addorover_mod.FLUID_VOICE_ADD)
                            mod1.Amount += pmod.Amount;
                        else
                            mod1.Amount = pmod.Amount;
                        return;
                    }
                }
            }

            // Add a new modulator (No existing modulator to add / overwrite).
            // Also, default modulators (FLUID_VOICE_DEFAULT) are added without checking, if the same modulator already exists. 
            //if (this.mod.Count < FLUID_NUM_MOD)
            {
                HiMod mod1 = new HiMod();
                mod1.Amount = pmod.Amount;
                mod1.Dest = pmod.Dest;
                mod1.Flags1 = pmod.Flags1;
                mod1.Flags2 = pmod.Flags2;
                mod1.Src1 = pmod.Src1;
                mod1.Src2 = pmod.Src2;
                this.mods.Add(mod1);
            }
        }


        public void fluid_voice_start()
        {
            status = fluid_voice_status.FLUID_VOICE_ON;

            // The maximum volume of the loop is calculated and cached once for each sample with its nominal loop settings. 
            // This happens, when the sample is used for the first time.

            fluid_voice_calculate_runtime_synthesis_parameters();
#if DEBUGPERF
            synth.DebugPerf("After synthesis_parameters:");
#endif
            if (!weakDevice)
            {
                if (synth.VerboseEnvVolume)
                    for (int i = 0; i < (int)fluid_voice_envelope_index.FLUID_VOICE_ENVLAST; i++)
                        Debug.LogFormat("Volume Env. {0} {1,24} {2}", i, (fluid_voice_envelope_index)i, volenv_data[i].ToString());

                if (synth.VerboseEnvModulation)
                    for (int i = 0; i < (int)fluid_voice_envelope_index.FLUID_VOICE_ENVLAST; i++)
                        Debug.LogFormat("Modulation Env. {0} {1,24} {2}", i, (fluid_voice_envelope_index)i, modenv_data[i].ToString());

                // Precalculate env. volume
                fluid_env_data env_data = volenv_data[(int)volenv_section];
                while (env_data.count <= 0d && (int)volenv_section < volenv_data.Length)
                {
                    double lastmax = env_data.max; ;
                    volenv_section++;
                    env_data = volenv_data[(int)volenv_section];
                    volenv_count = 0d;
                    volenv_val = lastmax;
                    if (synth.VerboseEnvVolume) Debug.LogFormat("Modulation Precalculate Env. Count --> section:{0}  new count:{1} volenv_val:{2}", (int)volenv_section, env_data.count, volenv_val);
                }

                // Precalculate env. modulation
                env_data = modenv_data[(int)modenv_section];
                while (env_data.count <= 0d && (int)modenv_section < modenv_data.Length)
                {
                    double lastmax = env_data.max;
                    modenv_section++;
                    env_data = modenv_data[(int)modenv_section];
                    modenv_count = 0d;
                    modenv_val = lastmax;
                    if (synth.VerboseEnvModulation) Debug.LogFormat("Modulation Precalculate Env. Count --> section:{0}  new count:{1} modenv_val:{2}", (int)modenv_section, env_data.count, modenv_val);
                }

                // Force setting of the phase at the first DSP loop run This cannot be done earlier, because it depends on modulators.
                //check_sample_sanity_flag = 1 << 1; //FLUID_SAMPLESANITY_STARTUP (1 << 1) 
            }
            else
                volenv_val = 1;

            Audiosource.volume = synth.MPTK_Volume * (float)(fluid_conv.fluid_atten2amp(attenuation) * fluid_conv.fluid_cb2amp(960.0f * (1d - volenv_val)));
            Audiosource.loop = ((gens[(int)fluid_gen_type.GEN_SAMPLEMODE].Val == (double)fluid_loop.FLUID_LOOP_UNTIL_RELEASE) || (gens[(int)fluid_gen_type.GEN_SAMPLEMODE].Val == (double)fluid_loop.FLUID_LOOP_DURING_RELEASE));

            if (synth.VerboseVoice) Debug.LogFormat("   fluid_voice_start Audiosource volume:{0:0.000} loop:{1} pitch:{2:0.000}", Audiosource.volume, Audiosource.loop, Audiosource.pitch);

            Timing.RunCoroutine(ThreadPlayNote());//,Segment.FixedUpdate);
        }

        /// <summary>
        /// in this function we calculate the values of all the parameters. the parameters are converted to their most useful unit for the DSP algorithm, 
        /// for example, number of samples instead of timecents.
        /// Some parameters keep their "perceptual" unit and conversion will be done in the DSP function.
        /// This is the case, for example, for the pitch since it is modulated by the controllers in cents.
        /// </summary>
        void fluid_voice_calculate_runtime_synthesis_parameters()
        {
            // When the voice is made ready for the synthesis process, a lot of voice-internal parameters have to be calculated.
            // At this point, the sound font has already set the -nominal- value for all generators (excluding GEN_PITCH). 
            // Most generators can be modulated - they include a nominal value and an offset (which changes with velocity, note number, channel parameters like
            // aftertouch, mod wheel...) 
            // Now this offset will be calculated as follows:
            //  - Process each modulator once.
            //  - Calculate its output value.
            //  - Find the target generator.
            //  - Add the output value to the modulation value of the generator.
            // Note: The generators have been initialized with fluid_gen_set_default_values.

            //foreach (HiMod m in mods) Debug.Log(m.ToString());


            foreach (HiMod m in mods)
            {
                //if (m.dest == (int)fluid_gen_type.GEN_FILTERFC)
                //    Debug.Log("GEN_FILTERFC");

                gens[m.Dest].Mod += m.fluid_mod_get_value(channel, this);
            }

            // The GEN_PITCH is a hack to fit the pitch bend controller into the modulator paradigm.  
            // Now the nominal pitch of the key is set.
            // Note about SCALETUNE: SF2.01 8.1.3 says, that this generator is a non-realtime parameter. So we don't allow modulation (as opposed
            // to _GEN(voice, GEN_SCALETUNE) When the scale tuning is varied, one key remains fixed. Here C3 (MIDI number 60) is used.
            if (channel.tuning != null)
            {
                gens[(int)fluid_gen_type.GEN_PITCH].Val = channel.tuning.pitch[60] + (gens[(int)fluid_gen_type.GEN_SCALETUNE].Val / 100.0f * channel.tuning.pitch[key] - channel.tuning.pitch[60]);
            }
            else
            {
                gens[(int)fluid_gen_type.GEN_PITCH].Val = (gens[(int)fluid_gen_type.GEN_SCALETUNE].Val * (key - 60.0f) + 100.0f * 60.0f);
            }

            /* Now the generators are initialized, nominal and modulation value.
             * The voice parameters (which depend on generators) are calculated
             * with fluid_voice_update_param. Processing the list of generator
             * changes will calculate each voice parameter once.
             *
             * Note [1]: Some voice parameters depend on several generators. For
             * example, the pitch depends on GEN_COARSETUNE, GEN_FINETUNE and
             * GEN_PITCH.  voice.pitch.  Unnecessary recalculation is avoided
             * by removing all but one generator from the list of voice
             * parameters.  Same with GEN_XXX and GEN_XXXCOARSE: the
             * initialisation list contains only GEN_XXX.
             */

            // Calculate the voice parameter(s) dependent on each generator.
            if (!weakDevice)
                foreach (int igen in list_of_generators_to_initialize)
                    fluid_voice_update_param(igen);
            else
                foreach (int igen in list_of_weakgenerators_to_initialize)
                    fluid_voice_update_param(igen);

            // Make an estimate on how loud this voice can get at any time (attenuation). */
            //min_attenuation_cB = fluid_voice_get_lower_boundary_for_attenuation();
        }


        /*
         * fluid_voice_get_lower_boundary_for_attenuation
         *
         * Purpose:
         *
         * A lower boundary for the attenuation (as in 'the minimum
         * attenuation of this voice, with volume pedals, modulators
         * etc. resulting in minimum attenuation, cannot fall below x cB) is
         * calculated.  This has to be called during fluid_voice_init, after
         * all modulators have been run on the voice once.  Also,
         * voice.attenuation has to be initialized.
         */
        double fluid_voice_get_lower_boundary_for_attenuation()
        {
            double possible_att_reduction_cB = 0;
            double lower_bound;

            foreach (HiMod m in mods)
            {
                // Modulator has attenuation as target and can change over time? 
                if ((m.Dest == (int)fluid_gen_type.GEN_ATTENUATION)
                    && ((m.Flags1 & (byte)fluid_mod_flags.FLUID_MOD_CC) > 0 || (m.Flags2 & (byte)fluid_mod_flags.FLUID_MOD_CC) > 0))
                {

                    double current_val = m.fluid_mod_get_value(channel, this);
                    double v = Math.Abs(m.Amount);

                    if ((m.Src1 == (byte)fluid_mod_src.FLUID_MOD_PITCHWHEEL)
                        || (m.Flags1 & (byte)fluid_mod_flags.FLUID_MOD_BIPOLAR) > 0
                        || (m.Flags2 & (byte)fluid_mod_flags.FLUID_MOD_BIPOLAR) > 0
                        || (m.Amount < 0))
                    {
                        /* Can this modulator produce a negative contribution? */
                        v *= -1.0;
                    }
                    else
                    {
                        /* No negative value possible. But still, the minimum contribution is 0. */
                        v = 0;
                    }

                    /* For example:
                     * - current_val=100
                     * - min_val=-4000
                     * - possible_att_reduction_cB += 4100
                     */
                    if (current_val > v)
                    {
                        possible_att_reduction_cB += (current_val - v);
                    }
                }
            }

            lower_bound = attenuation - possible_att_reduction_cB;

            /* SF2.01 specs do not allow negative attenuation */
            if (lower_bound < 0)
            {
                lower_bound = 0;
            }
            return lower_bound;
        }
        /// <summary>
        /// The value of a generator (gen) has changed.  (The different generators are listed in fluidsynth.h, or in SF2.01 page 48-49). Now the dependent 'voice' parameters are calculated.
        /// fluid_voice_update_param can be called during the setup of the  voice (to calculate the initial value for a voice parameter), or
        /// during its operation (a generator has been changed due to real-time parameter modifications like pitch-bend).
        /// Note: The generator holds three values: The base value .val, an offset caused by modulators .mod, and an offset caused by the
        /// NRPN system. _GEN(voice, generator_enumerator) returns the sum of all three.
        /// From fluid_midi_send_event NOTE_ON -. synth_noteon -. fluid_voice_start -. fluid_voice_calculate_runtime_synthesis_parameters
        /// From fluid_midi_send_event CONTROL_CHANGE -. fluid_synth_cc -. fluid_channel_cc Default      -. fluid_synth_modulate_voices     -. fluid_voice_modulate
        /// From fluid_midi_send_event CONTROL_CHANGE -. fluid_synth_cc -. fluid_channel_cc ALL_CTRL_OFF -. fluid_synth_modulate_voices_all -. fluid_voice_modulate_all
        /// </summary>
        /// <param name="igen"></param>
        void fluid_voice_update_param(int igen)
        {
            //Debug.Log("fluid_voice_update_param " + (fluid_gen_type)igen);
            switch (igen)
            {
                //_GEN(_voice, _n)   gen[_n].val + gen[_n].mod + gen[_n].nrpn)

                case (int)fluid_gen_type.GEN_PAN:
                    /* range checking is done in the fluid_pan function: range from -500 to 500 */
                    pan = gens[igen].Val + gens[igen].Mod /*+ gens[igen].nrpn*/;
                    //amp_left = fluid_conv.fluid_pan(pan, true);// * synth_gain / 32768.0f;
                    //amp_right = fluid_conv.fluid_pan(pan, false);// * synth_gain / 32768.0f;
                    if (synth.MPTK_EnablePanChange)
                        Audiosource.panStereo = Mathf.Lerp(-1f, 1f, ((float)pan + 500) / 1000f);
                    //Audiosource.panStereo = (((float)pan + 500) / 1000f) < 0f ? -1f : 1f;
                    else
                        Audiosource.panStereo = 0f;
                    if (synth.VerboseCalcGen)
                        //Debug.LogFormat("Calc {0} synth_gain={1:0.00} pan={2:0.00} amp_left={3:0.00} amp_right={4:0.00} Audiosource.panStereo={5:0.00}",
                        //    (fluid_gen_type)igen, synth_gain, pan, amp_left, amp_right, Audiosource.panStereo);
                        Debug.LogFormat("Calc {0} pan={1:0.00} Audiosource.panStereo={2:0.00}", (fluid_gen_type)igen, pan, Audiosource.panStereo);
                    break;

                case (int)fluid_gen_type.GEN_ATTENUATION:
                    attenuation = gens[igen].Val + gens[igen].Mod /*+ gens[igen].nrpn*/;
                    // Range: SF2.01 section 8.1.3 # 48 Motivation for range checking:OHPiano.SF2 sets initial attenuation to a whooping -96 dB
                    attenuation = attenuation < 0.0 ? 0.0 : attenuation > 14440.0 ? 1440.0 : attenuation;
                    if (synth.VerboseCalcGen) Debug.LogFormat("Calc {0} attenuation={1:0.00} ", (fluid_gen_type)igen, attenuation);
                    break;

                // The pitch is calculated from the current note 
                case (int)fluid_gen_type.GEN_PITCH:
                case (int)fluid_gen_type.GEN_COARSETUNE:
                case (int)fluid_gen_type.GEN_FINETUNE:
                    /* The testing for allowed range is done in 'fluid_ct2hz' */
                    pitch_midicents = (gens[(int)fluid_gen_type.GEN_PITCH].Val + gens[(int)fluid_gen_type.GEN_PITCH].Mod /*+ gens[(int)fluid_gen_type.GEN_PITCH].nrpn*/)
                        + 100.0f * (gens[(int)fluid_gen_type.GEN_COARSETUNE].Val + gens[(int)fluid_gen_type.GEN_COARSETUNE].Mod /*+ gens[(int)fluid_gen_type.GEN_COARSETUNE].nrpn*/)
                        + gens[(int)fluid_gen_type.GEN_FINETUNE].Val + gens[(int)fluid_gen_type.GEN_FINETUNE].Mod /*+ gens[(int)fluid_gen_type.GEN_FINETUNE].nrpn*/;
                    pitch = Mathf.Pow(_ratioHalfTone, (float)((pitch_midicents - root_pitch) / 100d));
                    Audiosource.pitch = (float)pitch;
                    if (synth.VerboseCalcGen) Debug.LogFormat("Calc {0} pitch_midicents={1:0.00} Audiosource.pitch={2:0.00} ", (fluid_gen_type)igen, pitch_midicents, pitch);
                    break;

                case (int)fluid_gen_type.GEN_REVERBSEND:
                    /* The generator unit is 'tenths of a percent'. */
                    reverb_send = (float)(gens[igen].Val + gens[igen].Mod /*+ gens[igen].nrpn*/) / 1000f;
                    reverb_send = reverb_send < 0f ? 0f : reverb_send > 1f ? 1f : reverb_send;
                    if (synth.VerboseCalcGen)
                        Debug.LogFormat("Calc {0} reverb_send={1:0.00}", (fluid_gen_type)igen, reverb_send);
                    break;

                case (int)fluid_gen_type.GEN_CHORUSSEND:
                    /* The generator unit is 'tenths of a percent'. */
                    chorus_send = (float)(gens[igen].Val + gens[igen].Mod /*+ gens[igen].nrpn*/) / 1000f;
                    chorus_send = chorus_send < 0f ? 0f : chorus_send > 1f ? 1f : chorus_send;
                    if (synth.VerboseCalcGen)
                        Debug.LogFormat("Calc {0} chorus_send={1:0.00}", (fluid_gen_type)igen, chorus_send);
                    break;

                case (int)fluid_gen_type.GEN_OVERRIDEROOTKEY:
                    // This is a non-realtime parameter. Therefore the .mod part of the generator can be neglected.
                    //* NOTE: origpitch sets MIDI root note while pitchadj is a fine tuning amount which offsets the original rate.  
                    // This means that the fine tuning is inverted with respect to the root note (so subtract it, not add).
                    if (gens[igen].Val > -1)
                    {   //FIXME: use flag instead of -1
                        root_pitch = gens[igen].Val * 100.0f - sample.PitchAdj;
                    }
                    else
                    {
                        root_pitch = sample.OrigPitch * 100.0f - sample.PitchAdj;
                    }
                    if (synth.VerboseCalcGen) Debug.LogFormat("Calc {0} root_pitch={1:0.00}", (fluid_gen_type)igen, root_pitch);
                    break;

                case (int)fluid_gen_type.GEN_FILTERFC:
                    // The resonance frequency is converted from absolute cents to midicents .val and .mod are both used, this permits real-time
                    // modulation.  The allowed range is tested in the 'fluid_ct2hz' function [PH,20021214]
                    Fres = gens[igen].Val + gens[igen].Mod /*+ gens[igen].nrpn*/;
                    /* The synthesis loop will have to recalculate the filter
                     * coefficients. */
                    last_fres = -1.0f;
                    if (synth.VerboseCalcGen) Debug.LogFormat("Calc {0} Fres={1:0.00} val={2:0.00} mod={3:0.00}", (fluid_gen_type)igen, Fres, gens[igen].Val, gens[igen].Mod);
                    break;

                case (int)fluid_gen_type.GEN_FILTERQ:
                    // The generator contains 'centibels' (1/10 dB) => divide by 10 to obtain dB
                    double q_dB = (gens[igen].Val + gens[igen].Mod /*+ gens[igen].nrpn*/) / 10.0f;

                    // Range: SF2.01 section 8.1.3 # 8 (convert from cB to dB => /10)
                    q_dB = q_dB < 0.0 ? 0.0 : q_dB > 96.0 ? 96.0 : q_dB;


                    // Short version: Modify the Q definition in a way, that a Q of 0 dB leads to no resonance hump in the freq. response.
                    // Long version: From SF2.01, page 39, item 9 (initialFilterQ):
                    // "The gain at the cutoff frequency may be less than zero when zero is specified".  Assume q_dB=0 / q_lin=1: If we would leave
                    // q as it is, then this results in a 3 dB hump slightly below fc. At fc, the gain is exactly the DC gain (0 dB).  What is
                    // (probably) meant here is that the filter does not show a resonance hump for q_dB=0. In this case, the corresponding
                    // q_lin is 1/sqrt(2)=0.707.  The filter should have 3 dB of attenuation at fc now.  In this case Q_dB is the height of the
                    // resonance peak not over the DC gain, but over the frequency response of a non-resonant filter.  This idea is implemented as follows:
                    q_dB -= 3.01f;

                    // The 'sound font' Q is defined in dB. The filter needs a linear q. Convert.
                    q_lin = Math.Pow(10.0f, q_dB / 20.0f);

                    // SF 2.01 page 59: The SoundFont specs ask for a gain reduction equal to half the height of the resonance peak (Q).  For example, for a 10 dB
                    //  resonance peak, the gain is reduced by 5 dB.  This is done by multiplying the total gain with sqrt(1/Q).  `Sqrt' divides dB by 2 
                    // (100 lin = 40 dB, 10 lin = 20 dB, 3.16 lin = 10 dB etc)
                    //  The gain is later factored into the 'b' coefficients  (numerator of the filter equation).  This gain factor depends
                    //  only on Q, so this is the right place to calculate it.
                    filter_gain = 1.0 / Math.Sqrt(q_lin);

                    // The synthesis loop will have to recalculate the filter coefficients. */
                    last_fres = -1.0;
                    if (synth.VerboseCalcGen) Debug.LogFormat("Calc {0} filter_gain={1:0.00} q_lin={2:0.00} q_dB={3:0.00}", (fluid_gen_type)igen, filter_gain, q_lin, q_dB);
                    break;

                case (int)fluid_gen_type.GEN_MODLFOTOPITCH:
                    modlfo_to_pitch = gens[igen].Val + gens[igen].Mod /*+ gens[igen].nrpn*/;
                    modlfo_to_pitch = modlfo_to_pitch < -12000.0 ? -12000.0 : modlfo_to_pitch > 12000.0 ? 12000.0 : modlfo_to_pitch;
                    break;

                case (int)fluid_gen_type.GEN_MODLFOTOVOL:
                    modlfo_to_vol = gens[igen].Val + gens[igen].Mod /*+ gens[igen].nrpn*/;
                    modlfo_to_vol = modlfo_to_vol < -960.0 ? -960.0 : modlfo_to_vol > 960.0 ? 960.0 : modlfo_to_vol;
                    if (synth.VerboseCalcGen) Debug.LogFormat("Calc {0} modlfo_to_vol={1:0.00}", (fluid_gen_type)igen, modlfo_to_vol);
                    break;

                case (int)fluid_gen_type.GEN_MODLFOTOFILTERFC:
                    modlfo_to_fc = gens[igen].Val + gens[igen].Mod /*+ gens[igen].nrpn*/;
                    modlfo_to_fc = modlfo_to_fc < -12000.0 ? -12000.0 : modlfo_to_fc > 12000.0 ? 12000.0 : modlfo_to_fc;
                    break;

                case (int)fluid_gen_type.GEN_MODLFODELAY:
                    {
                        double x = gens[igen].Val + gens[igen].Mod /*+ gens[igen].nrpn*/;
                        //x = x < -12000.0 ? -12000.0 : x > 5000.0 ? 5000.0 : x;
                        modlfo_delay = fluid_conv.fluid_tc2msec_delay(x);
                        if (synth.VerboseCalcGen) Debug.LogFormat("Calc {0} modlfo_delay={1:0.00}", (fluid_gen_type)igen, modlfo_delay);
                    }
                    break;

                case (int)fluid_gen_type.GEN_MODLFOFREQ:
                    {
                        //the frequency is converted into a delta value, per buffer of FLUID_BUFSIZE samples - the delay into a sample delay
                        double x = gens[igen].Val + gens[igen].Mod /*+ gens[igen].nrpn*/;
                        x = x < -16000.0 ? -16000.0 : x > 4500.0 ? 4500.0 : x;
                        //modlfo_incr = (4.0f * fluid_synth_t.FLUID_BUFSIZE * fluid_conv.fluid_act2hz(x) / output_rate);
                        modlfo_incr = fluid_conv.fluid_act2hz(x) * 1.2d;
                        if (synth.VerboseCalcGen) Debug.LogFormat("Calc {0} modlfo_incr={1:0.00} x={2:0.00}", (fluid_gen_type)igen, modlfo_incr, x);
                    }
                    break;

                case (int)fluid_gen_type.GEN_VIBLFOFREQ:
                    {
                        // the frequency is converted into a delta value, per buffer of FLUID_BUFSIZE samples the delay into a sample delay
                        double x = gens[igen].Val + gens[igen].Mod /*+ gens[igen].nrpn*/;
                        x = x < -16000.0 ? -16000.0 : x > 4500.0 ? 4500.0 : x;
                        //viblfo_incr = (4.0f * fluid_synth_t.FLUID_BUFSIZE * fluid_conv.fluid_act2hz(x) / output_rate);
                        viblfo_incr = fluid_conv.fluid_act2hz(x) * 1.2d;
                        if (synth.VerboseCalcGen) Debug.LogFormat("Calc {0} viblfo_incr={1:0.00} x={2:0.00}", (fluid_gen_type)igen, viblfo_incr, x);
                    }
                    break;

                case (int)fluid_gen_type.GEN_VIBLFODELAY:
                    {
                        double x = gens[igen].Val + gens[igen].Mod /*+ gens[igen].nrpn*/;
                        //x = x < -12000.0 ? -12000.0 : x > 5000.0 ? 5000.0 : x;
                        //viblfo_delay = (uint)(output_rate * fluid_conv.fluid_tc2sec_delay(x));
                        viblfo_delay = fluid_conv.fluid_tc2msec_delay(x);
                        if (synth.VerboseCalcGen) Debug.LogFormat("Calc {0} viblfo_delay={1:0.00} x={2:0.00}", (fluid_gen_type)igen, viblfo_delay, x);
                    }
                    break;

                case (int)fluid_gen_type.GEN_VIBLFOTOPITCH:
                    viblfo_to_pitch = gens[igen].Val + gens[igen].Mod /*+ gens[igen].nrpn*/;
                    viblfo_to_pitch = viblfo_to_pitch < -12000.0 ? -12000.0 : viblfo_to_pitch > 12000.0 ? 12000.0 : viblfo_to_pitch;
                    if (synth.VerboseCalcGen) Debug.LogFormat("Calc {0} viblfo_to_pitch={1:0.00} val={2:0.00} mod={3:0.00}", (fluid_gen_type)igen, viblfo_to_pitch, gens[igen].Val, gens[igen].Mod);
                    break;

                case (int)fluid_gen_type.GEN_KEYNUM:
                    {
                        // GEN_KEYNUM: SF2.01 page 46, item 46
                        // If this generator is active, it forces the key number to its value.  Non-realtime controller.
                        // There is a flag, which should indicate, whether a generator is enabled or not.  But here we rely on the default value of -1.
                        int x = Convert.ToInt32(gens[igen].Val + gens[igen].Mod /*+ gens[igen].nrpn*/);
                        if (x >= 0) key = x;
                    }
                    break;

                case (int)fluid_gen_type.GEN_VELOCITY:
                    {
                        // GEN_VELOCITY: SF2.01 page 46, item 47
                        // If this generator is active, it forces the velocity to its value. Non-realtime controller.
                        // There is a flag, which should indicate, whether a generator is enabled or not. But here we rely on the default value of -1. 
                        int x = Convert.ToInt32(gens[igen].Val + gens[igen].Mod /*+ gens[igen].nrpn*/);
                        if (x >= 0) vel = x;
                        //Debug.Log(string.Format("fluid_voice_update_param {0} vel={1} ", (fluid_gen_type)igen, x));

                    }
                    break;

                case (int)fluid_gen_type.GEN_MODENVTOPITCH:
                    modenv_to_pitch = gens[igen].Val + gens[igen].Mod /*+ gens[igen].nrpn*/;
                    modenv_to_pitch = modenv_to_pitch < -12000.0 ? -12000.0 : modenv_to_pitch > 12000.0 ? 12000.0 : modenv_to_pitch;
                    break;

                case (int)fluid_gen_type.GEN_MODENVTOFILTERFC:
                    // Range: SF2.01 section 8.1.3 # 1
                    // Motivation for range checking:Filter is reported to make funny noises now 
                    modenv_to_fc = gens[igen].Val + gens[igen].Mod /*+ gens[igen].nrpn*/;
                    modenv_to_fc = modenv_to_fc < -12000.0 ? -12000.0 : modenv_to_fc > 12000.0 ? 12000.0 : modenv_to_fc;
                    break;

                // sample start and ends points
                // Range checking is initiated via the check_sample_sanity flag, because it is impossible to check here:
                // During the voice setup, all modulators are processed, while the voice is inactive. Therefore, illegal settings may
                // occur during the setup (for example: First move the loop end point ahead of the loop start point => invalid, then move the loop start point forward => valid again.
                // Unity adaptation: wave are played from a wave file not from a global data buffer. It's not possible de change these
                // value after importing the SoudFont. Only loop address are taken in account whrn importing the SF
                //case (int)fluid_gen_type.GEN_STARTADDROFS:              /* SF2.01 section 8.1.3 # 0 */
                //case (int)fluid_gen_type.GEN_STARTADDRCOARSEOFS:        /* SF2.01 section 8.1.3 # 4 */
                //    if (sample != null)
                //    {
                //        start = (int)(sample.start
                //            + (int)gens[(int)fluid_gen_type.GEN_STARTADDROFS].val + gens[(int)fluid_gen_type.GEN_STARTADDROFS].mod + gens[(int)fluid_gen_type.GEN_STARTADDROFS].nrpn
                //            + 32768 * (int)gens[(int)fluid_gen_type.GEN_STARTADDRCOARSEOFS].val + gens[(int)fluid_gen_type.GEN_STARTADDRCOARSEOFS].mod + gens[(int)fluid_gen_type.GEN_STARTADDRCOARSEOFS].nrpn);
                //        check_sample_sanity_flag = 1 << 0; //? FLUID_SAMPLESANITY_CHECK(1 << 0)
                //        if (synth.VerboseCalcGen) Debug.LogFormat("Calc {0} start={1} val={2:0.00} mod={3:0.00}", (fluid_gen_type)igen, start, gens[igen].val, gens[igen].mod);
                //    }
                //    break;
                //case (int)fluid_gen_type.GEN_ENDADDROFS:                 /* SF2.01 section 8.1.3 # 1 */
                //case (int)fluid_gen_type.GEN_ENDADDRCOARSEOFS:           /* SF2.01 section 8.1.3 # 12 */
                //    if (sample != null)
                //    {
                //        end = (int)(sample.end
                //            + (int)gens[(int)fluid_gen_type.GEN_ENDADDROFS].val + gens[(int)fluid_gen_type.GEN_ENDADDROFS].mod + gens[(int)fluid_gen_type.GEN_ENDADDROFS].nrpn
                //            + 32768 * (int)gens[(int)fluid_gen_type.GEN_ENDADDRCOARSEOFS].val + gens[(int)fluid_gen_type.GEN_ENDADDRCOARSEOFS].mod + gens[(int)fluid_gen_type.GEN_ENDADDRCOARSEOFS].nrpn);
                //        check_sample_sanity_flag = 1 << 0; //? FLUID_SAMPLESANITY_CHECK(1 << 0)
                //        if (synth.VerboseCalcGen) Debug.LogFormat("Calc {0} end={1} val={2:0.00} mod={3:0.00}", (fluid_gen_type)igen, end, gens[igen].val, gens[igen].mod);
                //    }
                //    break;
                //case (int)fluid_gen_type.GEN_STARTLOOPADDROFS:           /* SF2.01 section 8.1.3 # 2 */
                //case (int)fluid_gen_type.GEN_STARTLOOPADDRCOARSEOFS:     /* SF2.01 section 8.1.3 # 45 */
                //    if (sample != null)
                //    {
                //        loopstart =
                //            (int)(sample.loopstart +
                //            (int)gens[(int)fluid_gen_type.GEN_STARTLOOPADDROFS].val +
                //            gens[(int)fluid_gen_type.GEN_STARTLOOPADDROFS].mod +
                //            gens[(int)fluid_gen_type.GEN_STARTLOOPADDROFS].nrpn +
                //            32768 * (int)gens[(int)fluid_gen_type.GEN_STARTLOOPADDRCOARSEOFS].val +
                //            gens[(int)fluid_gen_type.GEN_STARTLOOPADDRCOARSEOFS].mod +
                //            gens[(int)fluid_gen_type.GEN_STARTLOOPADDRCOARSEOFS].nrpn);
                //        check_sample_sanity_flag = 1 << 0; //? FLUID_SAMPLESANITY_CHECK(1 << 0)
                //        if (synth.VerboseCalcGen) Debug.LogFormat("Calc {0} loopstart={1} val={2:0.00} mod={3:0.00}", (fluid_gen_type)igen, loopstart, gens[igen].val, gens[igen].mod);
                //    }
                //    break;

                //case (int)fluid_gen_type.GEN_ENDLOOPADDROFS:             /* SF2.01 section 8.1.3 # 3 */
                //case (int)fluid_gen_type.GEN_ENDLOOPADDRCOARSEOFS:       /* SF2.01 section 8.1.3 # 50 */
                //    if (sample != null)
                //    {
                //        loopend =
                //            (int)(sample.loopend +
                //            (int)gens[(int)fluid_gen_type.GEN_ENDLOOPADDROFS].val +
                //            gens[(int)fluid_gen_type.GEN_ENDLOOPADDROFS].mod +
                //            gens[(int)fluid_gen_type.GEN_ENDLOOPADDROFS].nrpn
                //            + 32768 * (int)gens[(int)fluid_gen_type.GEN_ENDLOOPADDRCOARSEOFS].val +
                //            gens[(int)fluid_gen_type.GEN_ENDLOOPADDRCOARSEOFS].mod +
                //            gens[(int)fluid_gen_type.GEN_ENDLOOPADDRCOARSEOFS].nrpn);
                //        check_sample_sanity_flag = 1 << 0; //? FLUID_SAMPLESANITY_CHECK(1 << 0)
                //        if (synth.VerboseCalcGen) Debug.LogFormat("Calc {0} loopend={1} val={2:0.00} mod={3:0.00}", (fluid_gen_type)igen, loopend, gens[igen].val, gens[igen].mod);
                //    }
                //    break;

                // Conversion functions differ in range limit
                //#define NUM_BUFFERS_DELAY(_v)   (unsigned int) (output_rate * fluid_tc2sec_delay(_v) / FLUID_BUFSIZE)
                //#define NUM_BUFFERS_ATTACK(_v)  (unsigned int) (output_rate * fluid_tc2sec_attack(_v) / FLUID_BUFSIZE)
                //#define NUM_BUFFERS_RELEASE(_v) (unsigned int) (output_rate * fluid_tc2sec_release(_v) / FLUID_BUFSIZE)

                // volume envelope
                // - delay and hold times are converted to absolute number of samples
                // - sustain is converted to its absolute value
                // - attack, decay and release are converted to their increment per sample
                case (int)fluid_gen_type.GEN_VOLENVDELAY:                /* SF2.01 section 8.1.3 # 33 */
                    {
                        double x = gens[igen].Val + gens[igen].Mod /*+ gens[igen].nrpn*/;
                        //x = x < -12000.0 ? -12000.0 : x > 5000.0 ? 5000.0 : x;

                        //uint oldcount = Convert.ToUInt32(output_rate * fluid_conv.fluid_tc2sec_delay(x) / fluid_synth_t.FLUID_BUFSIZE);
                        double count = fluid_conv.fluid_tc2msec_delay(x);
                        volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVDELAY].count = count < synth.TimeResolution ? 0d : count;
                        volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVDELAY].coeff = 0d;
                        volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVDELAY].incr = 0d;
                        volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVDELAY].min = -1d;
                        volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVDELAY].max = 1d;
                        if (synth.VerboseCalcGen) Debug.Log(string.Format("Calc {0} count={1} ms. ", (fluid_gen_type)igen, volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVDELAY].count));

                    }
                    break;

                case (int)fluid_gen_type.GEN_VOLENVATTACK:               /* SF2.01 section 8.1.3 # 34 */
                    {
                        double x = gens[igen].Val + gens[igen].Mod /*+ gens[igen].nrpn*/;
                        //x = x < -12000.0 ? -12000.0 : x > 8000.0 ? 8000.0 : x;
                        //uint count = 1 + Convert.ToUInt32(output_rate * fluid_conv.fluid_tc2sec_attack(x) / fluid_synth_t.FLUID_BUFSIZE);
                        double count = fluid_conv.fluid_tc2msec_attack(x);
                        volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVATTACK].count = count < synth.TimeResolution ? 0d : count;
                        volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVATTACK].coeff = 1d;
                        volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVATTACK].incr = count != 0 ? 1d / count : 0d; // increment of env_data.incr * DeltaTimeWrite at each step
                        volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVATTACK].min = -1d;
                        volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVATTACK].max = 1d;
                        if (synth.VerboseCalcGen) Debug.Log(string.Format("Calc {0} x={1:0.00} count={2} ms. incr={3}", (fluid_gen_type)igen, x, volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVATTACK].count, volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVATTACK].incr));
                    }
                    break;

                case (int)fluid_gen_type.GEN_VOLENVHOLD:                 /* SF2.01 section 8.1.3 # 35 */
                case (int)fluid_gen_type.GEN_KEYTOVOLENVHOLD:            /* SF2.01 section 8.1.3 # 39 */
                    {
                        double count = calculate_hold_decay_ms((int)fluid_gen_type.GEN_VOLENVHOLD, (int)fluid_gen_type.GEN_KEYTOVOLENVHOLD, false); /* 0 means: hold */
                        volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVHOLD].count = count < synth.TimeResolution ? 0d : count;
                        volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVHOLD].coeff = 1d;
                        volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVHOLD].incr = 0d; // Volume stay stable during hold phase
                        volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVHOLD].min = -1d;
                        volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVHOLD].max = 1d;//2d;
                        if (synth.VerboseCalcGen) Debug.Log(string.Format("Calc {0} count={1} ms. ", (fluid_gen_type)igen, volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVHOLD].count));
                    }
                    break;

                case (int)fluid_gen_type.GEN_VOLENVDECAY:               /* SF2.01 section 8.1.3 # 36 */
                case (int)fluid_gen_type.GEN_VOLENVSUSTAIN:             /* SF2.01 section 8.1.3 # 37 */
                case (int)fluid_gen_type.GEN_KEYTOVOLENVDECAY:          /* SF2.01 section 8.1.3 # 40 */
                    {
                        double y = 1.0f - 0.001f * (gens[(int)fluid_gen_type.GEN_VOLENVSUSTAIN].Val + gens[(int)fluid_gen_type.GEN_VOLENVSUSTAIN].Mod/* + gens[(int)fluid_gen_type.GEN_VOLENVSUSTAIN].nrpn*/);
                        y = y < 0.0 ? 0.0 : y > 1.0 ? 1.0 : y;
                        double count = calculate_hold_decay_ms((int)fluid_gen_type.GEN_VOLENVDECAY, (int)fluid_gen_type.GEN_KEYTOVOLENVDECAY, true); /* 1 for decay */
                        volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVDECAY].count = count < synth.TimeResolution ? 0d : count;
                        volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVDECAY].coeff = 1.0f;
                        volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVDECAY].incr = count > 0 ? -1d / count : 0d;
                        volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVDECAY].min = y; // Value to reach 
                        volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVDECAY].max = 1d; //2d;
                        if (synth.VerboseCalcGen) Debug.Log(string.Format("Calc {0} y={1:0.00} count={2} ms. incr={3}", (fluid_gen_type)igen, y, volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVDECAY].count, volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVDECAY].incr));
                    }
                    break;

                case (int)fluid_gen_type.GEN_VOLENVRELEASE:             /* SF2.01 section 8.1.3 # 38 */
                    {
                        double x = gens[igen].Val + gens[igen].Mod /*+ gens[igen].nrpn*/;
                        x = x < FLUID_MIN_VOLENVRELEASE ? FLUID_MIN_VOLENVRELEASE : x > 8000.0 ? 8000.0 : x;
                        //uint count = 1 + (uint)(output_rate * fluid_conv.fluid_tc2sec_release(x) / fluid_synth_t.FLUID_BUFSIZE);
                        double rt = fluid_conv.fluid_tc2msec_release(x);
                        double count = rt < synth.MPTK_ReleaseTimeMin ? synth.MPTK_ReleaseTimeMin : rt;
                        volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVRELEASE].count = count < synth.TimeResolution ? 0d : count;
                        volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVRELEASE].coeff = 1.0f;
                        volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVRELEASE].incr = count != 0 ? -1d / count : 0d;
                        volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVRELEASE].min = 0d;
                        volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVRELEASE].max = 1d;
                        if (synth.VerboseCalcGen) Debug.Log(string.Format("Calc {0} x={1:0.00} count={2} ms. incr={3} tc2msec_release(x)={4}", (fluid_gen_type)igen, x, volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVRELEASE].count, volenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVRELEASE].incr, rt));
                    }
                    break;

                //
                // Modulation envelope
                //
                case (int)fluid_gen_type.GEN_MODENVDELAY:               /* SF2.01 section 8.1.3 # 25 */
                    {
                        double x = gens[igen].Val + gens[igen].Mod /*+ gens[igen].nrpn*/;
                        //x = x < -12000.0 ? -12000.0 : x > 5000.0 ? 5000.0 : x;
                        //uint count = (uint)(output_rate * fluid_conv.fluid_tc2sec_delay(x) / fluid_synth_t.FLUID_BUFSIZE);
                        double count = fluid_conv.fluid_tc2msec_delay(x);
                        modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVDELAY].count = count < synth.TimeResolution ? 0d : count;
                        modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVDELAY].coeff = 0.0f;
                        modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVDELAY].incr = 0.0f;
                        modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVDELAY].min = -1.0f;
                        modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVDELAY].max = 1.0f;
                        if (synth.VerboseCalcGen) Debug.Log(string.Format("Calc {0} x={1:0.00} count={2}", (fluid_gen_type)igen, x, modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVDELAY].count));
                    }
                    break;

                case (int)fluid_gen_type.GEN_MODENVATTACK:               /* SF2.01 section 8.1.3 # 26 */
                    {
                        double x = gens[igen].Val + gens[igen].Mod /*+ gens[igen].nrpn*/;
                        //x = x < -12000.0 ? -12000.0 : x > 8000.0 ? 8000.0 : x;
                        //uint count = 1 + (uint)(output_rate * fluid_conv.fluid_tc2sec_attack(x) / fluid_synth_t.FLUID_BUFSIZE);
                        double count = fluid_conv.fluid_tc2msec_attack(x);
                        modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVATTACK].count = count < synth.TimeResolution ? 0d : count;
                        modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVATTACK].coeff = 1.0f;
                        modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVATTACK].incr = count > 0 ? 1.0f / count : 0.0f;
                        modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVATTACK].min = -1.0f;
                        modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVATTACK].max = 1.0f;
                        if (synth.VerboseCalcGen) Debug.Log(string.Format("Calc {0} count={1} ms. incr={2}", (fluid_gen_type)igen, count, modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVATTACK].incr));
                    }
                    break;

                case (int)fluid_gen_type.GEN_MODENVHOLD:               /* SF2.01 section 8.1.3 # 27 */
                case (int)fluid_gen_type.GEN_KEYTOMODENVHOLD:          /* SF2.01 section 8.1.3 # 31 */
                    {
                        double count = calculate_hold_decay_ms((int)fluid_gen_type.GEN_MODENVHOLD, (int)fluid_gen_type.GEN_KEYTOMODENVHOLD, false);
                        modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVHOLD].count = count < synth.TimeResolution ? 0d : count;
                        modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVHOLD].coeff = 1.0f;
                        modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVHOLD].incr = 0.0f;
                        modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVHOLD].min = -1.0f;
                        modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVHOLD].max = 1.0f;
                        if (synth.VerboseCalcGen) Debug.Log(string.Format("Calc {0} count={1} ms ", (fluid_gen_type)igen, count));
                    }
                    break;

                case (int)fluid_gen_type.GEN_MODENVDECAY:                                   /* SF 2.01 section 8.1.3 # 28 */
                case (int)fluid_gen_type.GEN_MODENVSUSTAIN:                                 /* SF 2.01 section 8.1.3 # 29 */
                case (int)fluid_gen_type.GEN_KEYTOMODENVDECAY:                              /* SF 2.01 section 8.1.3 # 32 */
                    {
                        double y = 1.0 - 0.001 * (gens[(int)fluid_gen_type.GEN_MODENVSUSTAIN].Val + gens[(int)fluid_gen_type.GEN_MODENVSUSTAIN].Mod/* + gens[(int)fluid_gen_type.GEN_MODENVSUSTAIN].nrpn*/);
                        y = y < 0.0 ? 0.0 : y > 1.0 ? 1.0 : y;
                        double count = calculate_hold_decay_ms((int)fluid_gen_type.GEN_MODENVDECAY, (int)fluid_gen_type.GEN_KEYTOMODENVDECAY, true); /* 1 for decay */
                        modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVDECAY].count = count < synth.TimeResolution ? 0d : count;
                        modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVDECAY].coeff = 1.0f;
                        modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVDECAY].incr = count > 0 ? -1.0f / count : 0.0f;
                        modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVDECAY].min = y;
                        modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVDECAY].max = 1.0f;
                        if (synth.VerboseCalcGen) Debug.Log(string.Format("Calc {0} y={1:0.00} count={2} ms. incr={3} ", (fluid_gen_type)igen, y, modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVDECAY].count, modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVDECAY].incr));
                    }
                    break;

                case (int)fluid_gen_type.GEN_MODENVRELEASE:                                  /* SF 2.01 section 8.1.3 # 30 */
                    {
                        double x = gens[igen].Val + gens[igen].Mod /*+ gens[igen].nrpn*/;
                        //x = x < -12000.0 ? -12000.0 : x > 8000.0 ? 8000.0 : x;
                        //uint count = 1 + (uint)(output_rate * fluid_conv.fluid_tc2sec_release(x) / fluid_synth_t.FLUID_BUFSIZE);
                        double count = fluid_conv.fluid_tc2msec_release(x);
                        modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVRELEASE].count = count < synth.TimeResolution ? 0d : count;
                        modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVRELEASE].coeff = 1.0f;
                        modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVRELEASE].incr = count != 0 ? -1.0f / count : 0.0;
                        modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVRELEASE].min = 0.0f;
                        modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVRELEASE].max = 1.0f;
                        if (synth.VerboseCalcGen) Debug.Log(string.Format("Calc {0} x={1:0.00} count={2} ms. incr={3}", (fluid_gen_type)igen, x, modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVRELEASE].count, modenv_data[(int)fluid_voice_envelope_index.FLUID_VOICE_ENVRELEASE].incr));
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Returns the number of DSP loops, that correspond to the hold (is_decay=0) or decay (is_decay=1) time.
        /// gen_base=GEN_VOLENVHOLD, GEN_VOLENVDECAY, GEN_MODENVHOLD, GEN_MODENVDECAY gen_key2base=GEN_KEYTOVOLENVHOLD, GEN_KEYTOVOLENVDECAY, GEN_KEYTOMODENVHOLD, GEN_KEYTOMODENVDECAY
        /// </summary>
        /// <param name="gen_base"></param>
        /// <param name="gen_key2base"></param>
        /// <param name="is_decay"></param>
        /// <returns></returns>
        double calculate_hold_decay_ms(int gen_base, int gen_key2base, bool is_decay)
        {
            // SF2.01 section 8.4.3 # 31, 32, 39, 40
            // GEN_KEYTOxxxENVxxx uses key 60 as 'origin'.
            // The unit of the generator is timecents per key number.
            // If KEYTOxxxENVxxx is 100, a key one octave over key 60 (72) will cause (60-72)*100=-1200 timecents of time variation. The time is cut in half.
            double timecents = (gens[gen_base].Val + gens[gen_base].Mod /*+ gens[gen_base].nrpn*/) +
                                (gens[gen_key2base].Val + gens[gen_key2base].Mod /*+ gens[gen_key2base].nrpn*/) * (60.0 - key);

            // Range checking 
            if (is_decay)
            {
                // SF 2.01 section 8.1.3 # 28, 36 
                if (timecents > 8000.0)
                {
                    timecents = 8000.0;
                }
            }
            else
            {
                // SF 2.01 section 8.1.3 # 27, 35 
                if (timecents > 5000)
                {
                    timecents = 5000.0;
                }
                // SF 2.01 section 8.1.2 # 27, 35: The most negative number indicates no hold time
                if (timecents <= -32768.0)
                {
                    return 0;
                }
            }
            // SF 2.01 section 8.1.3 # 27, 28, 35, 36 
            if (timecents < -12000.0)
            {
                timecents = -12000.0;
            }

            //fluid_conv.fluid_tc2sec(timecents);
            double seconds = (double)Math.Pow(2.0, (double)timecents / 1200.0);

            // Each DSP loop processes FLUID_BUFSIZE samples. Round to next full number of buffers 
            //buffers = Convert.ToInt32((output_rate * seconds) / (double)fluid_synth_t.FLUID_BUFSIZE + 0.5);

            return seconds * 1000d;
        }

        /// <summary>
        /// Returns the number  of seconds, that correspond to the hold (is_decay=0) or decay (is_decay=1) time.
        /// gen_base=GEN_VOLENVHOLD, GEN_VOLENVDECAY, GEN_MODENVHOLD, GEN_MODENVDECAY gen_key2base=GEN_KEYTOVOLENVHOLD, GEN_KEYTOVOLENVDECAY, GEN_KEYTOMODENVHOLD, GEN_KEYTOMODENVDECAY
        /// </summary>
        /// <param name="gen_base"></param>
        /// <param name="gen_key2base"></param>
        /// <param name="is_decay"></param>
        /// <returns></returns>
        //double calculate_hold_decay_mseconds(int gen_base, int gen_key2base, bool is_decay)
        //{
        //    // SF2.01 section 8.4.3 # 31, 32, 39, 40
        //    // GEN_KEYTOxxxENVxxx uses key 60 as 'origin'.
        //    // The unit of the generator is timecents per key number.
        //    // If KEYTOxxxENVxxx is 100, a key one octave over key 60 (72) will cause (60-72)*100=-1200 timecents of time variation. The time is cut in half.
        //    double timecents = (gens[gen_base].Val + gens[gen_base].Mod /*+ gens[gen_base].nrpn*/) +
        //         (gens[gen_key2base].Val + gens[gen_key2base].Mod /*+ gens[gen_key2base].nrpn*/) * (60.0 - key);

        //    // Range checking 
        //    if (is_decay)
        //    {
        //        // SF 2.01 section 8.1.3 # 28, 36 
        //        if (timecents > 8000.0)
        //        {
        //            timecents = 8000.0;
        //        }
        //    }
        //    else
        //    {
        //        // SF 2.01 section 8.1.3 # 27, 35 
        //        if (timecents > 5000)
        //        {
        //            timecents = 5000.0;
        //        }
        //        // SF 2.01 section 8.1.2 # 27, 35: The most negative number indicates no hold time
        //        if (timecents <= -32768.0)
        //        {
        //            return 0;
        //        }
        //    }
        //    // SF 2.01 section 8.1.3 # 27, 28, 35, 36 
        //    if (timecents < -12000.0)
        //    {
        //        timecents = -12000.0;
        //    }
        //    return fluid_conv.fluid_tc2msec(timecents);
        //}

        /**
         * fluid_voice_modulate
         *
         * In this implementation, I want to make sure that all controllers
         * are event based: the parameter values of the DSP algorithm should
         * only be updates when a controller event arrived and not at every
         * iteration of the audio cycle (which would probably be feasible if
         * the synth was made in silicon).
         *
         * The update is done in three steps:
         *
         * - first, we look for all the modulators that have the changed
         * controller as a source. This will yield a list of generators that
         * will be changed because of the controller event.
         *
         * - For every changed generator, calculate its new value. This is the
         * sum of its original value plus the values of al the attached
         * modulators.
         *
         * - For every changed generator, convert its value to the correct
         * unit of the corresponding DSP parameter
         *
         * @fn int fluid_voice_modulate(fluid_voice_t* voice, int cc, int ctrl, int val)
         * @param voice the synthesis voice
         * @param cc flag to distinguish between a continous control and a channel control (pitch bend, ...)
         * @param ctrl the control number
         * */


        public void fluid_voice_modulate(int cc, int ctrl)
        {
            //if (synth.VerboseVoice)
            //{
            //    fluid_global.FLUID_LOG(fluid_log_level.FLUID_INFO, "Chan={0}, CC={1}, Src={2}", channel.channum, cc, pctrl);
            //}
            foreach (HiMod m in mods)
            {
                // step 1: find all the modulators that have the changed controller as input source.

                if ((((m.Src1 == ctrl) && ((m.Flags1 & (byte)fluid_mod_flags.FLUID_MOD_CC) != 0) && (cc != 0)) ||
                    (((m.Src1 == ctrl) && ((m.Flags1 & (byte)fluid_mod_flags.FLUID_MOD_CC) == 0) && (cc == 0)))) ||
                    (((m.Src2 == ctrl) && ((m.Flags2 & (byte)fluid_mod_flags.FLUID_MOD_CC) != 0) && (cc != 0)) ||
                    (((m.Src2 == ctrl) && ((m.Flags2 & (byte)fluid_mod_flags.FLUID_MOD_CC) == 0) && (cc == 0)))))
                {

                    int igen = m.Dest; //fluid_mod_get_dest
                    double modval = 0.0;

                    // step 2: for every changed modulator, calculate the modulation value of its associated generator
                    foreach (HiMod m1 in mods)
                    {
                        if (m1.Dest == igen) //fluid_mod_has_dest(mod, gen)((mod).dest == gen)
                        {
                            modval += m1.fluid_mod_get_value(channel, this);
                        }
                    }

                    gens[igen].Mod = modval; //fluid_gen_set_mod(_gen, _val)  { (_gen).mod = (double)(_val); }

                    // step 3: now that we have the new value of the generator, recalculate the parameter values that are derived from the generator */
                    if (synth.VerboseController)
                    {
                        Debug.LogFormat("Modulate Chan={0} CC={1} Controller={2} {3} Value:{4:0.2}", channel.channum, cc, (fluid_mod_src)ctrl, (fluid_gen_type)igen, modval);
                    }
                    fluid_voice_update_param(igen);
                }
            }
        }

        /// <summary>
        /// Update all the modulators. This function is called after a ALL_CTRL_OFF MIDI message has been received (CC 121). 
        /// </summary>
        /// <param name="voice"></param>
        /// <returns></returns>
        public void fluid_voice_modulate_all()
        {
            //fluid_mod_t* mod;
            //int i, k, gen;
            //fluid_real_t modval;

            //Loop through all the modulators.
            //    FIXME: we should loop through the set of generators instead of the set of modulators. We risk to call 'fluid_voice_update_param'
            //    several times for the same generator if several modulators have that generator as destination. It's not an error, just a wast of
            //    energy (think polution, global warming, unhappy musicians, ...) 

            foreach (HiMod m in mods)
            {
                gens[m.Dest].Mod += m.fluid_mod_get_value(channel, this);
                int igen = m.Dest; //fluid_mod_get_dest
                double modval = 0.0;
                // Accumulate the modulation values of all the modulators with destination generator 'gen'
                foreach (HiMod m1 in mods)
                {
                    if (m1.Dest == igen) //fluid_mod_has_dest(mod, gen)((mod).dest == gen)
                    {
                        modval += m1.fluid_mod_get_value(channel, this);
                    }
                }
                gens[igen].Mod = modval; //fluid_gen_set_mod(_gen, _val)  { (_gen).mod = (double)(_val); }

                // Update the parameter values that are depend on the generator 'gen'
                fluid_voice_update_param(igen);
            }
        }

        protected IEnumerator<float> ThreadPlayNote()
        {
            TimeAtStart = Time.realtimeSinceStartup * 1000d;
            TimeFromStart = 0d;
            LastTimeWrite = TimeAtStart;

            if (Audiosource != null && Audiosource.gameObject.activeInHierarchy)
            {
                Audiosource.Play();

                while (status != fluid_voice_status.FLUID_VOICE_OFF)
                {
                    if (Audiosource == null || !Audiosource.gameObject.activeInHierarchy)
                        break;
                    if (!weakDevice)
                        fluid_voice_write();
                    else
                        fluid_weakvoice_write();
                    // Wait next iteration
                    yield return 0;
                }
            }
            try
            {
                //Debug.Log("Stop AudioSource " + Audiosource.clip.name + " vol:" + Audiosource.volume);
                Audiosource.Stop();
            }
            catch (Exception)
            {
            }
        }

        //double last_modlfo_val_supp_1;
        //double last_modvib_val_supp_1;
        /*
         * fluid_voice_write
         *
         * This is where it all happens. This function is called by the
         * synthesizer to generate the sound samples. The synthesizer passes
         * four audio buffers: left, right, reverb out, and chorus out.
         *
         * The biggest part of this function sets the correct values for all
         * the dsp parameters (all the control data boil down to only a few
         * dsp parameters). The dsp routine is #included in several places (fluid_dsp_core.c).
         */
        public void fluid_voice_write()
        {
            DeltaTimeWrite = Time.deltaTime * 1000d;
            LastTimeWrite = Time.realtimeSinceStartup * 1000d;
            TimeFromStart = LastTimeWrite - TimeAtStart;

            //DebugEnveloppe("fluid_voice_write:" + status);

            // Debug.LogFormat("{0} TimeFromStart:{1:0.000000} DeltaTimeWrite:{2:0.000000} Time.deltaTime:{3:0.000000} {4}", IdVoice, TimeFromStart, DeltaTimeWrite, Time.deltaTime*1000f, modenv_section);

            if (Duration >= 0 && volenv_section != fluid_voice_envelope_index.FLUID_VOICE_ENVRELEASE && TimeFromStart > Duration)
            {
                //if (synth.VerboseEnveloppe) DebugEnveloppe("Over duration");
                fluid_voice_noteoff();
            }

            //******************* volume env **********************
            //-----------------------------------------------------

            fluid_env_data env_data = volenv_data[(int)volenv_section];

            // skip to the next section of the envelope if necessary
            while (volenv_count >= env_data.count)
            {
                volenv_section++;
                env_data = volenv_data[(int)volenv_section];
                volenv_count = 0d;
                //volenv_val = env_data.max;
                if (synth.VerboseEnvVolume) DebugVolEnv("Next");
            }

            // calculate the envelope value and check for valid range 
            double x = env_data.coeff * volenv_val + env_data.incr * DeltaTimeWrite;
            //Debug.LogFormat("Time:{0,4:F2} Delta:{1,2:F3} Count:{2,2:F2} / {3,2:F2} Calcul with coeff:{4,0:F0} modenv_val:{5,1:F3} incr:{6,1:F6} --> x:{7,1:F3} section:{8}",
            //    TimeFromStartPlayNote, DeltaTimeWrite, volenv_count, env_data.count, env_data.coeff, volenv_val, env_data.incr, x, volenv_section);

            if (x < env_data.min)
            {
                x = env_data.min;
                //volenv_section++;
                //volenv_count = 0d;
                if (synth.VerboseEnvVolume) DebugVolEnv("Min");
            }
            else if (x > env_data.max)
            {
                x = env_data.max;
                //volenv_section++;
                //volenv_count = 0d;
                if (synth.VerboseEnvVolume) DebugVolEnv("Max");
            }
            volenv_val = x;
            volenv_count += DeltaTimeWrite;
            //DebugVolEnv("Env");


            if (volenv_section == fluid_voice_envelope_index.FLUID_VOICE_ENVFINISHED)
            {
                fluid_voice_off();
                return;
            }

            //******************* modulation env **********************
            //---------------------------------------------------------

            if (synth.MPTK_ApplyRealTimeModulator)
            {
                env_data = modenv_data[(int)modenv_section];

                // skip to the next section of the envelope if necessary
                while (modenv_count >= env_data.count)
                {
                    modenv_section++;
                    env_data = modenv_data[(int)modenv_section];
                    modenv_count = 0d;
                    //Debug.LogFormat("Time:{0:0.000} Delta:{1:0.000} Count --> section:{2}  new count:{3}", TimeFromStartPlayNote, DeltaTimeWrite, (int)modenv_section, env_data.count);
                    if (synth.VerboseEnvModulation) DebugModEnv("Next");
                }

                // calculate the envelope value and check for valid range
                x = env_data.coeff * modenv_val + env_data.incr * DeltaTimeWrite;
                //Debug.LogFormat("Time:{0:0.000} Delta:{1:0.000} Calcul --> coeff:{2} modenv_val:{3:0.000} incr:{4} --> x:{5} section:{6}", TimeFromStartPlayNote, DeltaTimeWrite, env_data.coeff, modenv_val, env_data.incr, x, (int)modenv_section);


                if (x < env_data.min)
                {
                    x = env_data.min;
                    //modenv_section++;
                    //modenv_count = 0d;
                    //Debug.LogFormat("Time:{0:0.000} Delta:{1:0.000} Min  --> section:{2}", TimeFromStartPlayNote, DeltaTimeWrite, (int)modenv_section);
                    if (synth.VerboseEnvModulation) DebugModEnv("Min");
                }
                else if (x > env_data.max)
                {
                    x = env_data.max;
                    //modenv_section++;
                    //modenv_count = 0d;
                    //Debug.LogFormat("Time:{0:0.000} Delta:{1:0.000} Max  --> section:{2}", TimeFromStartPlayNote, DeltaTimeWrite, (int)modenv_section);
                    if (synth.VerboseEnvModulation) DebugModEnv("Max");
                }

                modenv_val = x;
                modenv_count += DeltaTimeWrite;
                //Debug.LogFormat("Time:{0:0.000} Delta:{1:0.000} End  --> modenv_count:{2}  modenv_val:{3} section:{4}", TimeFromStartPlayNote, DeltaTimeWrite, modenv_count, modenv_val, (int)modenv_section);

            }

            //******************* modulation lfo **********************
            //---------------------------------------------------------

            if (synth.MPTK_ApplyModLfo)
            {
                if (TimeFromStart >= modlfo_delay)
                {
                    modlfo_val += (modlfo_incr * synth.LfoAmpFreq) / DeltaTimeWrite;

                    if (modlfo_val > 1d)
                    {
                        //DebugLFO("delay modlfo_val > 1d freq:" + (TimeFromStartPlayNote - last_modlfo_val_supp_1).ToString());
                        //last_modlfo_val_supp_1 = TimeFromStartPlayNote;
                        modlfo_incr = -modlfo_incr;
                        modlfo_val = 2d - modlfo_val;
                    }
                    else if (modlfo_val < -1d)
                    {
                        //DebugLFO("modlfo_val < -1d");
                        modlfo_incr = -modlfo_incr;
                        modlfo_val = -2d - modlfo_val;
                    }
                    //DebugLFO("TimeFromStartPlayNote >= modlfo_delay");
                }
                //else DebugLFO("TimeFromStartPlayNote < modlfo_delay");
            }

            //******************* vibrato lfo **********************
            //------------------------------------------------------

            if (synth.MPTK_ApplyVibLfo)
            {
                if (TimeFromStart >= viblfo_delay)
                {

                    viblfo_val += (viblfo_incr * synth.LfoVibFreq) / DeltaTimeWrite;
                    //DebugVib("viblfo_delay");

                    if (viblfo_val > 1d)
                    {
                        //DebugVib("viblfo_val > 1 freq:" + (TimeFromStartPlayNote - last_modvib_val_supp_1).ToString());
                        //last_modvib_val_supp_1 = TimeFromStartPlayNote;
                        viblfo_incr = -viblfo_incr;
                        viblfo_val = 2d - viblfo_val;
                    }
                    else if (viblfo_val < -1.0)
                    {
                        //DebugVib("viblfo_val < -1");
                        viblfo_incr = -viblfo_incr;
                        viblfo_val = -2d - viblfo_val;
                    }
                }
                //else DebugVib("TimeFromStartPlayNote < viblfo_delay");fluid_ct2hz_real
                Audiosource.pitch = (float)(pitch * (1d + viblfo_val * viblfo_to_pitch * synth.LfoVibAmp / 2000d));
                //Audiosource.pitch = (float)fluid_conv.fluid_ct2hz_real(pitch * (1d + viblfo_val * viblfo_to_pitch * synth.LfoVibAmp));
            }

            /******************* amplitude **********************/

            /* calculate final amplitude
             * - initial gain
             * - amplitude envelope
             */

            if (volenv_section == fluid_voice_envelope_index.FLUID_VOICE_ENVDELAY)
            {

                //if (synth.VerboseEnveloppe) DebugEnveloppe("volenv_section == FLUID_VOICE_ENVDELAY");
                // The volume amplitude is in delay phase. No sound is produced.
                return;
            }

            float amp;

            if (volenv_section == fluid_voice_envelope_index.FLUID_VOICE_ENVATTACK)
            {
                // The envelope is in the attack section: ramp linearly to max value. A positive modlfo_to_vol should increase volume (negative attenuation).
                if (synth.MPTK_ApplyModLfo)
                    amp = (float)(fluid_conv.fluid_atten2amp(attenuation) * fluid_conv.fluid_cb2amp(modlfo_val * -modlfo_to_vol) * volenv_val) * synth.MPTK_Volume;
                else
                    amp = (float)(fluid_conv.fluid_atten2amp(attenuation) * volenv_val) * synth.MPTK_Volume;
            }
            else
            {
                if (volenv_section == fluid_voice_envelope_index.FLUID_VOICE_ENVSUSTAIN || volenv_section == fluid_voice_envelope_index.FLUID_VOICE_ENVRELEASE)
                {
                    if (volenv_val <= 0f)
                    {
                        //if (synth.VerboseEnveloppe) DebugEnveloppe("amp <=" + synth.CutoffVolume);
                        fluid_voice_off();
                        return;
                    }
                }

                if (synth.MPTK_ApplyModLfo)
                    amp = (float)(fluid_conv.fluid_atten2amp(attenuation) * fluid_conv.fluid_cb2amp(960.0f * (1d - volenv_val) + modlfo_val * -modlfo_to_vol)) * synth.MPTK_Volume;
                else
                    amp = (float)(fluid_conv.fluid_atten2amp(attenuation) * fluid_conv.fluid_cb2amp(960.0f * (1d - volenv_val))) * synth.MPTK_Volume;

            }
            if (!Mathf.Approximately(Audiosource.volume, amp))
            {
                if (synth.DampVolume <= 0)
                    Audiosource.volume = amp;
                else
                {
                    float velocity = 0f;
                    Audiosource.volume = Mathf.SmoothDamp(Audiosource.volume, amp, ref velocity, synth.DampVolume * 0.001f);
                }
                if (synth.VerboseVolume)
                    Debug.LogFormat("Volume [Id:{0:4}] {1} TimeDSP:{2:00000.000} Delta:{3:0.000} Att::{4,0:F2} modlfo_val:{5,0:F2} modlfo_to_vol:{6,0:F2} volenv_val:{7,0:F2} modenv_val:{8,0:F2} Amp:{9,0:F3} --> volume:{10,0:F3}",
                        IdVoice, "", TimeFromStart, DeltaTimeWrite,
                        attenuation, modlfo_val, modlfo_to_vol, volenv_val, modenv_val, amp, Audiosource.volume);
            }
            //else
            //    Debug.LogFormat("[{0:4}] {1} TimeDSP:{2:00000.000} Delta:{3:0.000} Att::{4,0:F2} modlfo_val:{5,0:F2} modlfo_to_vol:{6,0:F2} volenv_val:{7,0:F2} modenv_val:{8,0:F2} Amp:{9,0:F3} --> volume:{10,0:F3}",
            //        IdVoice, "", TimeFromStartPlayNote, DeltaTimeWrite,
            //        attenuation, modlfo_val, modlfo_to_vol, volenv_val, modenv_val, amp, Audiosource.volume);

            if (synth.MPTK_ApplyFilter)
            {
                if (LowPassFilter != null)
                {
                    if (!LowPassFilter.enabled) LowPassFilter.enabled = true;
                    calculateFilter();
                }
            }
            else if (LowPassFilter != null)
                if (LowPassFilter.enabled) LowPassFilter.enabled = false;

            if (synth.MPTK_ApplyReverb)
            {
                if (ReverbFilter != null)
                {
                    if (!ReverbFilter.enabled) ReverbFilter.enabled = true;
                    if (synth.ReverbMix == 0f)
                        ReverbFilter.dryLevel = Mathf.Lerp(0f, -10000f, reverb_send);
                    else
                        ReverbFilter.dryLevel = Mathf.Lerp(0f, -10000f, synth.ReverbMix);
                }
            }
            else if (ReverbFilter != null)
                if (ReverbFilter.enabled) ReverbFilter.enabled = false;

            if (synth.MPTK_ApplyChorus)
            {
                if (ChorusFilter != null)
                {
                    if (!ChorusFilter.enabled) ChorusFilter.enabled = true;
                    if (synth.ChorusMix == 0f)
                        ChorusFilter.dryMix = chorus_send;
                    else
                        ChorusFilter.dryMix = synth.ChorusMix;
                }
            }
            else if (ChorusFilter != null)
                if (ChorusFilter.enabled) ChorusFilter.enabled = false;
        }

        /// <summary>
        ///  week device voice: take care only of duration and release time
        /// </summary>
        public void fluid_weakvoice_write()
        {
            DeltaTimeWrite = Time.deltaTime * 1000d;
            LastTimeWrite = Time.realtimeSinceStartup * 1000d;
            TimeFromStart = LastTimeWrite - TimeAtStart;

            if (DeltaTimeWrite <= 0d) return;
            if (volenv_section != fluid_voice_envelope_index.FLUID_VOICE_ENVRELEASE)
            {
                if (Duration >= 0 && TimeFromStart > Duration)
                {
                    fluid_voice_noteoff();
                }
                else
                    return;
            }

            if (Audiosource.volume <= 0f || synth.MPTK_ReleaseTimeMin <= 0)
            {
                fluid_voice_off();
                return;
            }
            Audiosource.volume = Mathf.Lerp(Audiosource.volume, 0f, (float)(volenv_count / synth.MPTK_ReleaseTimeMin));

            if (synth.VerboseEnvVolume)
                Debug.LogFormat("Volume - [{0:4}] TimeDSP:{1:0.000} Delta:{2:0.000} synth.ReleaseTimeMin:{3:0.000} volenv_count:{4:0.000} Volume:{5:0.0000}",
                    IdVoice, TimeFromStart, DeltaTimeWrite, synth.MPTK_ReleaseTimeMin, volenv_count, Audiosource.volume);

            volenv_count += DeltaTimeWrite;

        }

        private double calculateFilter()
        {
            /*************** resonant filter ******************/

            /* calculate the frequency of the resonant filter in Hz */
            double localfres = fluid_conv.fluid_ct2hz(this.Fres + modlfo_val * modlfo_to_fc * synth.LfoToFilterMod + modenv_val * modenv_to_fc * synth.FilterEnvelopeMod) + synth.FilterOffset;

            if (synth.VerboseFilter)
                Debug.LogFormat("[{0:4}] {1} TimeDSP:{2:00000.000} Delta:{3:0.000} Fres:{4} modlfo_val:{5:0.000} modlfo_to_fc:{6:0.000} modenv_val:{7:0.000}  modenv_to_fc:{8:0.000} --> localfres:{9:0.000} q_lin:{10:0.000}",
                    IdVoice, "", TimeFromStart, DeltaTimeWrite,
                    Fres, modlfo_val, modlfo_to_fc, modenv_val, modenv_to_fc, localfres, q_lin * synth.FilterQMod);

            /* FIXME - Still potential for a click during turn on, can we interpolate
               between 20khz cutoff and 0 Q? */

            /* I removed the optimization of turning the filter off when the
             * resonance frequence is above the maximum frequency. Instead, the
             * filter frequency is set to a maximum of 0.45 times the sampling
             * rate. For a 44100 kHz sampling rate, this amounts to 19845
             * Hz. The reason is that there were problems with anti-aliasing when the
             * synthesizer was run at lower sampling rates. Thanks to Stephan
             * Tassart for pointing me to this bug. By turning the filter on and
             * clipping the maximum filter frequency at 0.45*srate, the filter
             * is used as an anti-aliasing filter. */

            //if (fres > 0.45f)// * output_rate)
            //    fres = 0.45f;// * output_rate;
            //else if (fres < 5)
            //    fres = 5;

            /* if filter enabled and there is a significant frequency change.. */
            if ((Math.Abs(localfres - last_fres) > 0.01d))
            {
                if (LowPassFilter != null)
                {
                    LowPassFilter.cutoffFrequency = (float)localfres;
                    LowPassFilter.lowpassResonanceQ = (float)(q_lin * synth.FilterQMod);
                }
                /* The filter coefficients have to be recalculated (filter
                * parameters have changed). Recalculation for various reasons is
                * forced by setting last_fres to -1.  The flag filter_startup
                * indicates, that the DSP loop runs for the first time, in this
                * case, the filter is set directly, instead of smoothly fading
                * between old and new settings.
                *
                * Those equations from Robert Bristow-Johnson's `Cookbook
                * formulae for audio EQ biquad filter coefficients', obtained
                * from Harmony-central.com / Computer / Programming. They are
                * the result of the bilinear transform on an analogue filter
                * prototype. To quote, `BLT frequency warping has been taken
                * into account for both significant frequency relocation and for
                * bandwidth readjustment'. */

                //double omega = (2d * M_PI * (fres / 44100.0d));
                //double sin_coeff = Math.Sin(omega);
                //double cos_coeff = Math.Cos(omega);
                //double alpha_coeff = sin_coeff / (2.0d * q_lin);
                //double a0_inv = 1.0d / (1.0d + alpha_coeff);

                /* Calculate the filter coefficients. All coefficients are
                 * normalized by a0. Think of `a1' as `a1/a0'.
                 *
                 * Here a couple of multiplications are saved by reusing common expressions.
                 * The original equations should be:
                 *  b0=(1.-cos_coeff)*a0_inv*0.5*filter_gain;
                 *  b1=(1.-cos_coeff)*a0_inv*filter_gain;
                 *  b2=(1.-cos_coeff)*a0_inv*0.5*filter_gain; */

                //double a1_temp = -2d * cos_coeff * a0_inv;
                //double a2_temp = (1d - alpha_coeff) * a0_inv;
                //double b1_temp = (1d - cos_coeff) * a0_inv * filter_gain;
                ///* both b0 -and- b2 */
                //double b02_temp = b1_temp * 0.5f;

                //if (filter_startup)
                //{
                //    /* The filter is calculated, because the voice was started up.
                //     * In this case set the filter coefficients without delay.
                //     */
                //    a1 = a1_temp;
                //    a2 = a2_temp;
                //    b02 = b02_temp;
                //    b1 = b1_temp;
                //    filter_coeff_incr_count = 0;
                //    filter_startup = false;
                //    //       printf("Setting initial filter coefficients.\n");
                //}
                //else
                //{

                //    /* The filter frequency is changed.  Calculate an increment
                //     * factor, so that the new setting is reached after one buffer
                //     * length. x_incr is added to the current value FLUID_BUFSIZE
                //     * times. The length is arbitrarily chosen. Longer than one
                //     * buffer will sacrifice some performance, though.  Note: If
                //     * the filter is still too 'grainy', then increase this number
                //     * at will.
                //     */

                //    a1_incr = (a1_temp - a1) / fluid_synth_t.FLUID_BUFSIZE;
                //    a2_incr = (a2_temp - a2) / fluid_synth_t.FLUID_BUFSIZE;
                //    b02_incr = (b02_temp - b02) / fluid_synth_t.FLUID_BUFSIZE;
                //    b1_incr = (b1_temp - b1) / fluid_synth_t.FLUID_BUFSIZE;
                //    /* Have to add the increments filter_coeff_incr_count times. */
                //    filter_coeff_incr_count = fluid_synth_t.FLUID_BUFSIZE;
                //}
                last_fres = localfres;
            }

            return localfres;
        }


        ///* No interpolation. Just take the sample, which is closest to
        //  * the playback pointer.  Questionable quality, but very
        //  * efficient. */
        //int
        //fluid_dsp_float_interpolate_none()
        //{
        //    //fluid_phase_t dsp_phase = phase;
        //    //fluid_phase_t dsp_phase_incr, end_phase;
        //    //short int* dsp_data = sample->data;
        //    fluid_real_t* dsp_buf = dsp_buf;
        //    fluid_real_t dsp_amp = amp;
        //    fluid_real_t dsp_amp_incr = amp_incr;
        //    unsigned int dsp_i = 0;
        //    unsigned int dsp_phase_index;
        //    unsigned int end_index;
        //    int looping;

        //    /* Convert playback "speed" floating point value to phase index/fract */
        //    //fluid_phase_set_float(dsp_phase_incr, phase_incr);

        //    /* voice is currently looping? */
        //    looping = _SAMPLEMODE(voice) == FLUID_LOOP_DURING_RELEASE
        //      || (_SAMPLEMODE(voice) == FLUID_LOOP_UNTIL_RELEASE
        //      && volenv_section < FLUID_VOICE_ENVRELEASE);

        //    end_index = looping ? loopend - 1 : end;

        //    while (1)
        //    {
        //        dsp_phase_index = fluid_phase_index_round(dsp_phase);   /* round to nearest point */

        //        /* interpolate sequence of sample points */
        //        for (; dsp_i < FLUID_BUFSIZE && dsp_phase_index <= end_index; dsp_i++)
        //        {
        //            dsp_buf[dsp_i] = dsp_amp * dsp_data[dsp_phase_index];

        //            /* increment phase and amplitude */
        //            fluid_phase_incr(dsp_phase, dsp_phase_incr);
        //            dsp_phase_index = fluid_phase_index_round(dsp_phase);   /* round to nearest point */
        //            dsp_amp += dsp_amp_incr;
        //        }

        //        /* break out if not looping (buffer may not be full) */
        //        if (!looping) break;

        //        /* go back to loop start */
        //        if (dsp_phase_index > end_index)
        //        {
        //            fluid_phase_sub_int(dsp_phase, loopend - loopstart);
        //            has_looped = 1;
        //        }

        //        /* break out if filled buffer */
        //        if (dsp_i >= FLUID_BUFSIZE) break;
        //    }

        //    phase = dsp_phase;
        //    amp = dsp_amp;

        //    return (dsp_i);

        //}

        public IEnumerator<float> Release()
        {
            //fluid_voice_noteoff();
            //Debug.Log("Release " + IdVoice);
            fluid_voice_noteoff(true);
            yield return 0;
        }

        /// <summary>
        /// Move phase enveloppe to release
        /// </summary>
        public void fluid_voice_noteoff(bool force = false)
        {
            //fluid_profile(FLUID_PROF_VOICE_NOTE, ref);
            if (!weakDevice)
            {
                if (!force && channel != null && channel.cc[(int)MPTKController.Sustain] >= 64)
                {
                    status = fluid_voice_status.FLUID_VOICE_SUSTAINED;
                }
                else
                {
                    if (volenv_section == fluid_voice_envelope_index.FLUID_VOICE_ENVATTACK)
                    {
                        // A voice is turned off during the attack section of the volume envelope.  
                        // The attack section ramps up linearly with amplitude. 
                        // The other sections use logarithmic scaling. 
                        // Calculate new volenv_val to achieve equivalent amplitude during the release phase for seamless volume transition.

                        if (volenv_val > 0)
                        {
                            double env_value;
                            if (synth.MPTK_ApplyModLfo)
                            {
                                double lfo = modlfo_val * -modlfo_to_vol;
                                double amp = volenv_val * Mathf.Pow(10f, (float)lfo / -200f);
                                env_value = -((-200 * Mathf.Log((float)amp) / Mathf.Log(10f) - lfo) / 960.0 - 1);
                            }
                            else
                            {
                                env_value = -((-200 * Mathf.Log((float)volenv_val) / Mathf.Log(10f)) / 960.0 - 1);
                            }
                            volenv_val = env_value > 1d ? 1d : env_value < 0d ? 0d : env_value;
                        }
                    }
                    volenv_section = fluid_voice_envelope_index.FLUID_VOICE_ENVRELEASE;
                    volenv_count = 0;
                    if (synth.VerboseEnvVolume) DebugVolEnv("fluid_voice_noteoff");

                    modenv_section = fluid_voice_envelope_index.FLUID_VOICE_ENVRELEASE;
                    modenv_count = 0;
                    if (synth.VerboseEnvModulation) DebugModEnv("fluid_voice_noteoff");
                }
            }
            else
            {
                volenv_section = fluid_voice_envelope_index.FLUID_VOICE_ENVRELEASE;
                volenv_count = 0;
            }
        }

        /*
        * fluid_voice_off
        *
        * Purpose:
        * Turns off a voice, meaning that it is not processed
        * anymore by the DSP loop.
        */
        public void fluid_voice_off()
        {
            chan = NO_CHANNEL;
            volenv_section = fluid_voice_envelope_index.FLUID_VOICE_ENVFINISHED;
            if (synth.VerboseEnvVolume) DebugVolEnv("fluid_voice_off");
            volenv_count = 0;
            Audiosource.volume = 0;
            //Audiosource.panStereo = 0;
            modenv_section = fluid_voice_envelope_index.FLUID_VOICE_ENVFINISHED;
            modenv_count = 0;
            status = fluid_voice_status.FLUID_VOICE_OFF;
            //Audiosource.Stop();
        }


        /* Purpose:
         *
         * Make sure, that sample start / end point and loop points are in
         * proper order. When starting up, calculate the initial phase.
         */
        //        void fluid_voice_check_sample_sanity()
        //        {
        //            int min_index_nonloop = (int)sample.start;
        //            int max_index_nonloop = (int)sample.end;

        //            /* make sure we have enough samples surrounding the loop */
        //            int min_index_loop = (int)sample.start + FLUID_MIN_LOOP_PAD;
        //            int max_index_loop = (int)sample.end - FLUID_MIN_LOOP_PAD + 1;  /* 'end' is last valid sample, loopend can be + 1 */

        //            if (check_sample_sanity_flag != 0)
        //            {
        //                return;
        //            }

        //#if DEBUG_S
        //	printf("Sample from %i to %i\n", sample.start, sample.end);
        //	printf("Sample loop from %i %i\n", sample.loopstart, sample.loopend);
        //	printf("Playback from %i to %i\n", start, end);
        //	printf("Playback loop from %i to %i\n", loopstart, loopend);
        //#endif

        //            /* Keep the start point within the sample data */
        //            if (start < min_index_nonloop)
        //            {
        //                start = min_index_nonloop;
        //            }
        //            else if (start > max_index_nonloop)
        //            {
        //                start = max_index_nonloop;
        //            }

        //            /* Keep the end point within the sample data */
        //            if (end < min_index_nonloop)
        //            {
        //                end = min_index_nonloop;
        //            }
        //            else if (end > max_index_nonloop)
        //            {
        //                end = max_index_nonloop;
        //            }

        //            /* Keep start and end point in the right order */
        //            if (start > end)
        //            {
        //                int temp = start;
        //                start = end;
        //                end = temp;
        //                /*FLUID_LOG(FLUID_DBG, "Loop / sample sanity check: Changing order of start / end points!"); */
        //            }

        //            /* Zero length? */
        //            if (start == end)
        //            {
        //                fluid_voice_off();
        //                return;
        //            }

        //            ////#define _SAMPLEMODE(voice) ((int)(voice)->gen[GEN_SAMPLEMODE].val)

        //            if ((gens[(int)fluid_gen_type.GEN_SAMPLEMODE].val == (double)fluid_loop.FLUID_LOOP_UNTIL_RELEASE) ||
        //                (gens[(int)fluid_gen_type.GEN_SAMPLEMODE].val == (double)fluid_loop.FLUID_LOOP_DURING_RELEASE))
        //            {
        //                /* Keep the loop start point within the sample data */
        //                if (loopstart < min_index_loop)
        //                {
        //                    loopstart = min_index_loop;
        //                }
        //                else if (loopstart > max_index_loop)
        //                {
        //                    loopstart = max_index_loop;
        //                }

        //                /* Keep the loop end point within the sample data */
        //                if (loopend < min_index_loop)
        //                {
        //                    loopend = min_index_loop;
        //                }
        //                else if (loopend > max_index_loop)
        //                {
        //                    loopend = max_index_loop;
        //                }

        //                /* Keep loop start and end point in the right order */
        //                if (loopstart > loopend)
        //                {
        //                    int temp = loopstart;
        //                    loopstart = loopend;
        //                    loopend = temp;
        //                    /*FLUID_LOG(FLUID_DBG, "Loop / sample sanity check: Changing order of loop points!"); */
        //                }

        //                /* Loop too short? Then don't loop. */
        //                if (loopend < loopstart + FLUID_MIN_LOOP_SIZE)
        //                {
        //                    gens[(int)fluid_gen_type.GEN_SAMPLEMODE].val = (double)fluid_loop.FLUID_UNLOOPED;
        //                }

        //                /* The loop points may have changed. Obtain a new estimate for the loop volume. */
        //                /* Is the voice loop within the sample loop? */
        //                //if ((int)loopstart >= (int)sample.loopstart
        //                //    && (int)loopend <= (int)sample.loopend)
        //                //{
        //                //    /* Is there a valid peak amplitude available for the loop? */
        //                //    if (sample.amplitude_that_reaches_noise_floor_is_valid)
        //                //    {
        //                //        amplitude_that_reaches_noise_floor_loop = sample.amplitude_that_reaches_noise_floor / synth_gain;
        //                //    }
        //                //    else
        //                //    {
        //                //        /* Worst case */
        //                //        amplitude_that_reaches_noise_floor_loop = amplitude_that_reaches_noise_floor_nonloop;
        //                //    }
        //                //}

        //            } /* if sample mode is looped */

        //            /* Run startup specific code (only once, when the voice is started) */
        //            //if (check_sample_sanity_flag & FLUID_SAMPLESANITY_STARTUP)
        //            //{
        //            //    if (max_index_loop - min_index_loop < FLUID_MIN_LOOP_SIZE)
        //            //    {
        //            //        if ((_SAMPLEMODE(voice) == FLUID_LOOP_UNTIL_RELEASE)
        //            //            || (_SAMPLEMODE(voice) == FLUID_LOOP_DURING_RELEASE))
        //            //        {
        //            //            gen[GEN_SAMPLEMODE].val = FLUID_UNLOOPED;
        //            //        }
        //            //    }

        //            //    /* Set the initial phase of the voice (using the result from the
        //            //   start offset modulators). */
        //            //    fluid_phase_set_int(phase, start);
        //            //} /* if startup */


        //            //// Is this voice run in loop mode, or does it run straight to the end of the waveform data? 
        //            ////#define _SAMPLEMODE(voice) ((int)(voice)->gen[GEN_SAMPLEMODE].val)
        //            //if (((gens[(int)fluid_gen_type.GEN_SAMPLEMODE].val == (int)fluid_loop.FLUID_LOOP_UNTIL_RELEASE) && (volenv_section < fluid_voice_envelope_index_t.FLUID_VOICE_ENVRELEASE))
        //            //    || (gens[(int)fluid_gen_type.GEN_SAMPLEMODE].val == (int)fluid_loop.FLUID_LOOP_DURING_RELEASE))
        //            //{
        //            //    /* Yes, it will loop as soon as it reaches the loop point.  In
        //            //     * this case we must prevent, that the playback pointer (phase)
        //            //     * happens to end up beyond the 2nd loop point, because the
        //            //     * point has moved.  The DSP algorithm is unable to cope with
        //            //     * that situation.  So if the phase is beyond the 2nd loop
        //            //     * point, set it to the start of the loop. No way to avoid some
        //            //     * noise here.  Note: If the sample pointer ends up -before the
        //            //     * first loop point- instead, then the DSP loop will just play
        //            //     * the sample, enter the loop and proceed as expected => no
        //            //     * actions required.
        //            //     */
        //            //    int index_in_sample = fluid_phase_index(phase);
        //            //    if (index_in_sample >= loopend)
        //            //    {
        //            //        /* FLUID_LOG(FLUID_DBG, "Loop / sample sanity check: Phase after 2nd loop point!"); */
        //            //        fluid_phase_set_int(phase, loopstart);
        //            //    }
        //            //}
        //            /*    FLUID_LOG(FLUID_DBG, "Loop / sample sanity check: Sample from %i to %i, loop from %i to %i", start, end, loopstart, loopend); */

        //            /* Sample sanity has been assured. Don't check again, until some
        //               sample parameter is changed by modulation. */
        //            check_sample_sanity_flag = 0;
        //        }

        private void DebugVolEnv(string info)
        {
            if (!weakDevice)
                Debug.LogFormat("Volume - [{0:4}] {1} TimeDSP:{2:0.000} Delta:{3:0.000} section:{4} volenv_val:{5:0.000} volenv_count:{6} incr:{7:0.0000}",
                   IdVoice, info, TimeFromStart, DeltaTimeWrite,
                   volenv_section, volenv_val, volenv_data[(int)volenv_section].count, volenv_data[(int)volenv_section].incr);
        }

        private void DebugModEnv(string info)
        {
            if (!weakDevice)
                Debug.LogFormat("Modulation - [{0:4}] {1} TimeDSP:{2:0.000} Delta:{3:0.000} section:{4} modenv_val:{5:0.000} modenv_count:{6} incr:{7:0.0000}",
               IdVoice, info, TimeFromStart, DeltaTimeWrite,
               modenv_section, modenv_val, modenv_data[(int)modenv_section].count, modenv_data[(int)modenv_section].incr);
        }

        private void DebugLFO(string info)
        {
            Debug.LogFormat("[{0:4}] {1} TimeDSP:{2:00000.000} Delta:{3:0.000} modlfo_delay:{4} modlfo_incr:{5:0.000} modlfo_val:{6:0.000} modlfo_to_vol:{7:0.000}",
               IdVoice, info, TimeFromStart, DeltaTimeWrite, modlfo_delay, modlfo_incr, modlfo_val, modlfo_to_vol);
        }
        private void DebugVib(string info)
        {
            Debug.LogFormat("[{0:4}] {1} TimeDSP:{2:00000.000} Delta:{3:0.000} viblfo_delay:{4} viblfo_incr:{5:0.000} viblfo_val:{6:0.000} viblfo_to_pitch:{7:0.000} --> pitch mod:{8:0.000}",
               IdVoice, info, TimeFromStart, DeltaTimeWrite, viblfo_delay, viblfo_incr, viblfo_val, viblfo_to_pitch, (float)(1d + viblfo_val * viblfo_to_pitch / 1000d));
        }
    }
}
