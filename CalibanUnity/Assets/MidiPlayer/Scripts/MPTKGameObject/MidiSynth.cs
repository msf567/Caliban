//#define DEBUGPERF
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Events;

namespace MidiPlayerTK
{
    public enum fluid_loop
    {
        FLUID_UNLOOPED = 0,
        FLUID_LOOP_DURING_RELEASE = 1,
        FLUID_NOTUSED = 2,
        FLUID_LOOP_UNTIL_RELEASE = 3
    }


    public enum fluid_synth_status
    {
        FLUID_SYNTH_CLEAN,
        FLUID_SYNTH_PLAYING,
        FLUID_SYNTH_QUIET,
        FLUID_SYNTH_STOPPED
    }

    // Flags to choose the interpolation method 
    public enum fluid_interp
    {
        // no interpolation: Fastest, but questionable audio quality
        FLUID_INTERP_NONE = 0,
        // Straight-line interpolation: A bit slower, reasonable audio quality
        FLUID_INTERP_LINEAR = 1,
        // Fourth-order interpolation: Requires 50 % of the whole DSP processing time, good quality 
        FLUID_INTERP_DEFAULT = 4,
        FLUID_INTERP_4THORDER = 4,
        FLUID_INTERP_7THORDER = 7,
        FLUID_INTERP_HIGHEST = 7
    }

    /// <summary>
    /// </summary>
    /// Base class for Midi Synthesizer. Migrated from fluidsynth.
    /// It's not recommended to instanciate this class. Instead use MidiFilePlayer or MidiStreamPlayer.
    public class MidiSynth : MonoBehaviour
    {
        /// <summary>
        /// Unity event fired at awake of the synthesizer. Name of the gameobject component is passed as a parameter.
        //! @code
        //! ...
        //! if (!midiStreamPlayer.OnEventSynthAwake.HasEvent())
        //!    midiStreamPlayer.OnEventSynthAwake.AddListener(StartLoadingSynth);
        //! ...
        //! public void StartLoadingSynth(string name)
        //! {
        //!     Debug.LogFormat("Synth {0} loading", name);
        //! }
        //! @endcode
        /// </summary>
        public EventSynthClass OnEventSynthAwake;

        /// <summary>
        /// Unity event fired at start of the synthesizer. Name of the gameobject component is passed as a parameter.
        //! @code
        //! ...
        //! if (!midiStreamPlayer.OnEventStartSynth.HasEvent())
        //!    midiStreamPlayer.OnEventStartSynth.AddListener(EndLoadingSynth);
        //! ...
        //! public void EndLoadingSynth(string name)
        //! {
        //!    Debug.LogFormat("Synth {0} loaded", name);
        //!    midiStreamPlayer.MPTK_PlayEvent(
        //!       new MPTKEvent() { Command = MPTKCommand.PatchChange, Value = CurrentPatchInstrument, Channel = StreamChannel});
        //! }
        //! @endcode
        /// </summary>
        public EventSynthClass OnEventSynthStarted;

        /// <summary>
        /// MaxDistance to use for PauseOnDistance
        /// </summary>
        public virtual float MPTK_MaxDistance
        {
            get
            {
                try
                {
                    return VoiceTemplate.Audiosource.maxDistance;
                }
                catch (System.Exception ex)
                {
                    MidiPlayerGlobal.ErrorDetail(ex);
                }
                return 0;
            }
            set
            {
                try
                {
                    VoiceTemplate.Audiosource.maxDistance = value;
                    if (Voices != null)
                        foreach (fluid_voice audio in Voices)
                            audio.Audiosource.maxDistance = value;
                }
                catch (System.Exception ex)
                {
                    MidiPlayerGlobal.ErrorDetail(ex);
                }
            }
        }

        /// <summary>
        /// Should the Midi playing must be paused if distance between AudioListener and MidiFilePlayer is greater than MaxDistance
        /// </summary>
        public bool MPTK_PauseOnDistance;

        /// <summary>
        /// Should change pan from Midi Events or from SoundFont ? 
        /// </summary>
        public bool MPTK_EnablePanChange;

        /// <summary>
        /// Should play on a weak device (cheaper smartphone) ?
        /// Playing Midi files with WeakDevice activated could cause some bad interpretation of Midi Event, consequently bad sound.
        /// </summary>
        public bool MPTK_WeakDevice;

        /// <summary>
        /// Volume of midi playing. 
        /// Must be >=0 and <= 1
        /// </summary>
        public virtual float MPTK_Volume
        {
            get { return volume; }
            set
            {
                if (volume >= 0f && volume <= 1f) volume = value; else Debug.LogWarning("MidiFilePlayer - Set Volume value not valid : " + value);
            }
        }
        [SerializeField]
        [HideInInspector]
        private float volume = 0.5f;

        /// <summary>
        /// Transpose note from -24 to 24
        /// </summary>
        public virtual int MPTK_Transpose
        {
            get { return transpose; }
            set { if (value >= -24 && value <= 24f) transpose = value; else Debug.LogWarning("MidiFilePlayer - Set Transpose value not valid : " + value); }
        }

        public bool MPTK_LogWave;

        /// <summary>
        /// Define a minimum release time at noteoff in milliseconds. Default 50 ms is a good tradeoff. Below some unpleasant sound coule be heard.
        /// </summary>
        [Range(0f, 500f)]
        public float MPTK_ReleaseTimeMin = 50f;


        //! @cond NODOC

        [Range(0.01f, 5.0f)]
        public float LfoAmpFreq = 1f;

        [Range(0.01f, 5.0f)]
        public float LfoVibFreq = 1f;

        [Range(0.01f, 5.0f)]
        public float LfoVibAmp = 1f;

        [Range(0.01f, 5.0f)]
        public float LfoToFilterMod = 1f;

        [Range(0.01f, 5.0f)]
        public float FilterEnvelopeMod = 1f;

        [Range(-2000f, 3000f)]
        public float FilterOffset = 1000f;

        [Range(0.01f, 5.0f)]
        public float FilterQMod = 1f;

        [Range(0f, 1f)]
        public float ReverbMix = 0f;

        [Range(0f, 1f)]
        public float ChorusMix = 0f;

        [Range(0, 100)]
        [Tooltip("Smooth Volume Change")]
        public int DampVolume = 0;

        /// <summary>
        /// Time from synth init (ms)
        /// </summary>
        [Tooltip("Time Init Synth")]
        public double TimeAtInit;
        [Tooltip("Auto Clean Voice Greater Than")]
        public int AutoCleanVoiceLimit = 50;
        [Tooltip("Auto Clean Voice Older Than")]
        public double AutoCleanVoiceTime = 10000d;

        /// <summary>
        /// Value updated only when playing in Unity (for inspector refresh)
        /// </summary>
        public float distanceEditorModeOnly;


        [SerializeField]
        [HideInInspector]
        public int transpose = 0;

        ///// <summary>
        ///// Modifier for the release time. From 0 to 5. Set to 1 for no effect
        ///// </summary>
        //public virtual float MPTK_ReleaseModifier
        //{
        //    get { return timeToRelease; }
        //    set { if (value >= 0f && value <= 5f) timeToRelease = value; else Debug.LogWarning("MidiFilePlayer - TimeToRelease value not valid : " + value + " must in the range 0 to 5"); }
        //}
        //[SerializeField]
        //[HideInInspector]
        //public float timeToRelease = 0.1f;

        public fluid_channel[] Channels;          /** the channels */
        public List<fluid_voice> Voices;              /** the synthesis processes */
        public fluid_voice VoiceTemplate;

        /* fluid_settings_old_t settings_old;  the old synthesizer settings */
        //TBC fluid_settings_t* settings;         /** the synthesizer settings */
        //int polyphony;                     /** maximum polyphony */
        //public const int FLUID_BUFSIZE = 64;
        //public const uint DRUM_INST_MASK = 0x80000000;

        public bool VerboseSynth;
        public bool VerboseVoice;
        public bool VerboseGenerator;
        public bool VerboseCalcGen;
        public bool VerboseController;
        public bool VerboseEnvVolume;
        public bool VerboseEnvModulation;
        public bool VerboseFilter;
        public bool VerboseVolume;

        [HideInInspector]
        public double TimeResolution = 20d;

        public bool MPTK_PlayOnlyFirstWave;
        public bool MPTK_ApplyFilter;
        public bool MPTK_ApplyReverb;
        public bool MPTK_ApplyChorus;
        public bool MPTK_ApplyRealTimeModulator;
        public bool MPTK_ApplyModLfo;
        public bool MPTK_ApplyVibLfo;

        //public double CutoffVolume = 0f;

        //double sample_rate;                /** The sample rate */
        //public int midi_channels = 16;                 /** the number of MIDI channels (>= 16) */
        //int audio_channels;                /** the number of audio channels (1 channel=left+right) */
        //int audio_groups;                  /** the number of (stereo) 'sub'groups from the synth. Typically equal to audio_channels. */
        //fluid_synth_status state;                /** the synthesizer state */
        //int start;                /** the start in msec, as returned by system clock */

        //double gain;                        /** master gain */
        //int nbuf;                           /** How many audio buffers are used? (depends on nr of audio channels / groups)*/

        //fluid_tuning_t[][] tuning;           /** 128 banks of 128 programs for the tunings */
        //fluid_tuning_t cur_tuning;         /** current tuning in the iteration */

        // The midi router. Could be done nicer.
        //Indicates, whether the audio thread is currently running.Note: This simple scheme does -not- provide 100 % protection against thread problems, for example from MIDI thread and shell thread
        //fluid_mutex_t busy;
        //fluid_midi_router_t* midi_router;


        // has the synth module been initialized? 
        public static int fluid_synth_initialized = 0;

        //default modulators SF2.01 page 52 ff:
        //There is a set of predefined default modulators. They have to be explicitly overridden by the sound font in order to turn them off.

        public static HiMod default_vel2att_mod = new HiMod();        /* SF2.01 section 8.4.1  */
        public static HiMod default_vel2filter_mod = new HiMod();     /* SF2.01 section 8.4.2  */
        public static HiMod default_at2viblfo_mod = new HiMod();      /* SF2.01 section 8.4.3  */
        public static HiMod default_mod2viblfo_mod = new HiMod();     /* SF2.01 section 8.4.4  */
        public static HiMod default_att_mod = new HiMod();            /* SF2.01 section 8.4.5  */
        public static HiMod default_pan_mod = new HiMod();            /* SF2.01 section 8.4.6  */
        public static HiMod default_expr_mod = new HiMod();           /* SF2.01 section 8.4.7  */
        public static HiMod default_reverb_mod = new HiMod();         /* SF2.01 section 8.4.8  */
        public static HiMod default_chorus_mod = new HiMod();         /* SF2.01 section 8.4.9  */
        public static HiMod default_pitch_bend_mod = new HiMod();     /* SF2.01 section 8.4.10 */


        /* reverb presets */
        //        static fluid_revmodel_presets_t revmodel_preset[] = {
        //	/* name */    /* roomsize */ /* damp */ /* width */ /* level */
        //	{ "Test 1",          0.2f,      0.0f,       0.5f,       0.9f },
        //    { "Test 2",          0.4f,      0.2f,       0.5f,       0.8f },
        //    { "Test 3",          0.6f,      0.4f,       0.5f,       0.7f },
        //    { "Test 4",          0.8f,      0.7f,       0.5f,       0.6f },
        //    { "Test 5",          0.8f,      1.0f,       0.5f,       0.5f },
        //    { NULL, 0.0f, 0.0f, 0.0f, 0.0f }
        //};

        /// <summary>
        /// From fluid_sys.c - fluid_utime() returns the time in micro seconds. this time should only be used to measure duration(relative times). 
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        //double fluid_utime()
        //{
        //    //fprintf(stderr, "fluid_cpu_frequency:%f fluid_utime:%f\n", fluid_cpu_frequency, rdtsc() / fluid_cpu_frequency);

        //    //return (rdtsc() / fluid_cpu_frequency);
        //    return AudioSettings.dspTime;
        //}

        /// <summary>
        /// returns the current time in milliseconds. This time should only be used in relative time measurements.
        /// </summary>
        /// <returns></returns>
        //int fluid_curtime()
        //{
        //    // replace GetTickCount() :Retrieves the number of milliseconds that have elapsed since the system was started, up to 49.7 days.
        //    return System.Environment.TickCount;
        //}

        public void Awake()
        {
            //Debug.Log("Awake fluid_synth_t");

            //fluid_synth_init();

            //AudioConfiguration GetConfiguration = AudioSettings.GetConfiguration();

            //polyphony = GetConfiguration.numRealVoices;
            //sample_rate = GetConfiguration.sampleRate;
            //midi_channels = 16;
            //audio_channels = 2;
            //audio_groups = 1;
            //gain = 1f;

            //TBC register the callbacks
            //fluid_settings_register_num(settings, "synth.gain",0.2f, 0.0f, 10.0f, 0,(fluid_num_update_t)fluid_synth_update_gain, synth);
            //fluid_settings_register_int(settings, "synth.polyphony",polyphony, 16, 4096, 0,(fluid_int_update_t)fluid_synth_update_polyphony,synth);


            /* The number of buffers is determined by the higher number of nr
             * groups / nr audio channels.  If LADSPA is unused, they should be
             * the same. */
            //nbuf = audio_channels;
            //if (audio_groups > nbuf)
            //{
            //    nbuf = audio_groups;
            //}

            /* as soon as the synth is created it starts playing. */
            //state = fluid_synth_status.FLUID_SYNTH_PLAYING;

            /* allocate all channel objects */
            ////////channels = new fluid_channel_t[midi_channels];
            ////////for (int i = 0; i < channels.Length; i++)
            ////////    channels[i] = new fluid_channel_t(this, i);

            ////////voices = new List<fluid_voice_t>();

            ////////if (VoiceTemplate == null)
            ////////{
            ////////    Debug.LogWarning("No voice template defined in synth, search one");
            ////////    VoiceTemplate = FindObjectOfType<fluid_voice_t>();
            ////////    if (VoiceTemplate == null)
            ////////    {
            ////////        Debug.LogWarning("No voice template found");
            ////////    }
            ////////}


            ///* Allocate the sample buffers */
            //left_buf = NULL;
            //right_buf = NULL;
            //fx_left_buf = NULL;
            //fx_right_buf = NULL;

            ///* Left and right audio buffers */
            //left_buf = FLUID_ARRAY(fluid_real_t *, nbuf);
            //right_buf = FLUID_ARRAY(fluid_real_t *, nbuf);

            //if ((left_buf == NULL) || (right_buf == NULL))
            //{
            //    FLUID_LOG(FLUID_ERR, "Out of memory");
            //    goto error_recovery;
            //}

            //FLUID_MEMSET(left_buf, 0, nbuf * sizeof(fluid_real_t*));
            //FLUID_MEMSET(right_buf, 0, nbuf * sizeof(fluid_real_t*));

            //for (i = 0; i < nbuf; i++)
            //{

            //    left_buf[i] = FLUID_ARRAY(fluid_real_t, FLUID_BUFSIZE);
            //    right_buf[i] = FLUID_ARRAY(fluid_real_t, FLUID_BUFSIZE);

            //    if ((left_buf[i] == NULL) || (right_buf[i] == NULL))
            //    {
            //        FLUID_LOG(FLUID_ERR, "Out of memory");
            //        goto error_recovery;
            //    }
            //}

            ///* Effects audio buffers */

            //fx_left_buf = FLUID_ARRAY(fluid_real_t *, effects_channels);
            //fx_right_buf = FLUID_ARRAY(fluid_real_t *, effects_channels);

            //if ((fx_left_buf == NULL) || (fx_right_buf == NULL))
            //{
            //    FLUID_LOG(FLUID_ERR, "Out of memory");
            //    goto error_recovery;
            //}

            //FLUID_MEMSET(fx_left_buf, 0, 2 * sizeof(fluid_real_t*));
            //FLUID_MEMSET(fx_right_buf, 0, 2 * sizeof(fluid_real_t*));

            //for (i = 0; i < effects_channels; i++)
            //{
            //    fx_left_buf[i] = FLUID_ARRAY(fluid_real_t, FLUID_BUFSIZE);
            //    fx_right_buf[i] = FLUID_ARRAY(fluid_real_t, FLUID_BUFSIZE);

            //    if ((fx_left_buf[i] == NULL) || (fx_right_buf[i] == NULL))
            //    {
            //        FLUID_LOG(FLUID_ERR, "Out of memory");
            //        goto error_recovery;
            //    }
            //}

            ///* allocate the reverb module */
            //reverb = new_fluid_revmodel();
            //if (reverb == NULL)
            //{
            //    FLUID_LOG(FLUID_ERR, "Out of memory");
            //    goto error_recovery;
            //}

            //fluid_synth_set_reverb(synth,
            //    FLUID_REVERB_DEFAULT_ROOMSIZE,
            //    FLUID_REVERB_DEFAULT_DAMP,
            //    FLUID_REVERB_DEFAULT_WIDTH,
            //    FLUID_REVERB_DEFAULT_LEVEL);

            ///* allocate the chorus module */
            //chorus = new_fluid_chorus(sample_rate);
            //if (chorus == NULL)
            //{
            //    FLUID_LOG(FLUID_ERR, "Out of memory");
            //    goto error_recovery;
            //}

            //cur = FLUID_BUFSIZE;
            //dither_index = 0;

            /* FIXME */
            //start = fluid_curtime();
        }

        protected virtual void Start()
        {
            if (VerboseSynth) Debug.Log("Start fluid_synth_t");
            if (VoiceTemplate == null)
                Debug.Log("No VoiceTemplate defined in MidiPlayer. Check MidiPlayer in your Hierarchy.");
            // VoiceTemplate.Audiosource.Play();
        }

        // Does all the initialization for this module.
        public void fluid_synth_init(int channelCount = 16)
        {
            fluid_synth_initialized++;
            TimeAtInit = Time.realtimeSinceStartup * 1000d;
            fluid_voice.LastId = 0;

#if TRAP_ON_FPE
            /* Turn on floating point exception traps */
            feenableexcept(FE_DIVBYZERO | FE_UNDERFLOW | FE_OVERFLOW | FE_INVALID);
#endif
            Channels = new fluid_channel[channelCount];
            for (int i = 0; i < Channels.Length; i++)
                Channels[i] = new fluid_channel(this, i);

            if (VerboseSynth) Debug.LogFormat("fluid_synth_init. Init: {0}, Channels: {1}", fluid_synth_initialized, Channels.Length);

            Voices = new List<fluid_voice>();

            if (VoiceTemplate == null)
            {
                Debug.LogWarning("No voice template defined in synth, search one");
                VoiceTemplate = FindObjectOfType<fluid_voice>();
                if (VoiceTemplate == null)
                {
                    Debug.LogWarning("No voice template found");
                }
            }


            //channels = new fluid_channel_t[16];
            //for (int i = 0; i < channels.Length; i++)
            //    channels[i] = new fluid_channel_t();

            //voices = new List<fluid_voice_t>();

            fluid_conv.fluid_conversion_config();

            //TBC fluid_dsp_float_config();
            //fluid_sys_config();
            //init_dither(); // pour fluid_synth_write_s16 ?

            /* SF2.01 page 53 section 8.4.1: MIDI Note-On Velocity to Initial Attenuation */
            fluid_mod_set_source1(default_vel2att_mod, /* The modulator we are programming here */
                (int)fluid_mod_src.FLUID_MOD_VELOCITY,    /* Source. VELOCITY corresponds to 'index=2'. */
                (int)fluid_mod_flags.FLUID_MOD_GC           /* Not a MIDI continuous controller */
                | (int)fluid_mod_flags.FLUID_MOD_CONCAVE    /* Curve shape. Corresponds to 'type=1' */
                | (int)fluid_mod_flags.FLUID_MOD_UNIPOLAR   /* Polarity. Corresponds to 'P=0' */
                | (int)fluid_mod_flags.FLUID_MOD_NEGATIVE   /* Direction. Corresponds to 'D=1' */
            );
            fluid_mod_set_source2(default_vel2att_mod, 0, 0); /* No 2nd source */
            fluid_mod_set_dest(default_vel2att_mod, (int)fluid_gen_type.GEN_ATTENUATION);  /* Target: Initial attenuation */
            fluid_mod_set_amount(default_vel2att_mod, 960.0);          /* Modulation amount: 960 */

            /* SF2.01 page 53 section 8.4.2: MIDI Note-On Velocity to Filter Cutoff
             * Have to make a design decision here. The specs don't make any sense this way or another.
             * One sound font, 'Kingston Piano', which has been praised for its quality, tries to
             * override this modulator with an amount of 0 and positive polarity (instead of what
             * the specs say, D=1) for the secondary source.
             * So if we change the polarity to 'positive', one of the best free sound fonts works...
             */
            fluid_mod_set_source1(default_vel2filter_mod, (int)fluid_mod_src.FLUID_MOD_VELOCITY, /* Index=2 */
                (int)fluid_mod_flags.FLUID_MOD_GC                        /* CC=0 */
                | (int)fluid_mod_flags.FLUID_MOD_LINEAR                  /* type=0 */
                | (int)fluid_mod_flags.FLUID_MOD_UNIPOLAR                /* P=0 */
                | (int)fluid_mod_flags.FLUID_MOD_NEGATIVE                /* D=1 */
            );
            fluid_mod_set_source2(default_vel2filter_mod, (int)fluid_mod_src.FLUID_MOD_VELOCITY, /* Index=2 */
                (int)fluid_mod_flags.FLUID_MOD_GC                                 /* CC=0 */
                | (int)fluid_mod_flags.FLUID_MOD_SWITCH                           /* type=3 */
                | (int)fluid_mod_flags.FLUID_MOD_UNIPOLAR                         /* P=0 */
                                                                                  // do not remove       | FLUID_MOD_NEGATIVE                         /* D=1 */
                | (int)fluid_mod_flags.FLUID_MOD_POSITIVE                         /* D=0 */
            );
            fluid_mod_set_dest(default_vel2filter_mod, (int)fluid_gen_type.GEN_FILTERFC);        /* Target: Initial filter cutoff */
            fluid_mod_set_amount(default_vel2filter_mod, -2400);

            /* SF2.01 page 53 section 8.4.3: MIDI Channel pressure to Vibrato LFO pitch depth */
            fluid_mod_set_source1(default_at2viblfo_mod, (int)fluid_mod_src.FLUID_MOD_CHANNELPRESSURE, /* Index=13 */
                (int)fluid_mod_flags.FLUID_MOD_GC                        /* CC=0 */
                | (int)fluid_mod_flags.FLUID_MOD_LINEAR                  /* type=0 */
                | (int)fluid_mod_flags.FLUID_MOD_UNIPOLAR                /* P=0 */
                | (int)fluid_mod_flags.FLUID_MOD_POSITIVE                /* D=0 */
            );
            fluid_mod_set_source2(default_at2viblfo_mod, 0, 0); /* no second source */
            fluid_mod_set_dest(default_at2viblfo_mod, (int)fluid_gen_type.GEN_VIBLFOTOPITCH);        /* Target: Vib. LFO => pitch */
            fluid_mod_set_amount(default_at2viblfo_mod, 50);

            /* SF2.01 page 53 section 8.4.4: Mod wheel (Controller 1) to Vibrato LFO pitch depth */
            fluid_mod_set_source1(default_mod2viblfo_mod, 1, /* Index=1 */
                (int)fluid_mod_flags.FLUID_MOD_CC                        /* CC=1 */
                | (int)fluid_mod_flags.FLUID_MOD_LINEAR                  /* type=0 */
                | (int)fluid_mod_flags.FLUID_MOD_UNIPOLAR                /* P=0 */
                | (int)fluid_mod_flags.FLUID_MOD_POSITIVE                /* D=0 */
            );
            fluid_mod_set_source2(default_mod2viblfo_mod, 0, 0); /* no second source */
            fluid_mod_set_dest(default_mod2viblfo_mod, (int)fluid_gen_type.GEN_VIBLFOTOPITCH);        /* Target: Vib. LFO => pitch */
            fluid_mod_set_amount(default_mod2viblfo_mod, 50);

            /* SF2.01 page 55 section 8.4.5: MIDI continuous controller 7 to initial attenuation*/
            fluid_mod_set_source1(default_att_mod, 7,                     /* index=7 */
                (int)fluid_mod_flags.FLUID_MOD_CC                              /* CC=1 */
                | (int)fluid_mod_flags.FLUID_MOD_CONCAVE                       /* type=1 */
                | (int)fluid_mod_flags.FLUID_MOD_UNIPOLAR                      /* P=0 */
                | (int)fluid_mod_flags.FLUID_MOD_NEGATIVE                      /* D=1 */
            );
            fluid_mod_set_source2(default_att_mod, 0, 0);                 /* No second source */
            fluid_mod_set_dest(default_att_mod, (int)fluid_gen_type.GEN_ATTENUATION);         /* Target: Initial attenuation */
            fluid_mod_set_amount(default_att_mod, 960.0);                 /* Amount: 960 */

            /* SF2.01 page 55 section 8.4.6 MIDI continuous controller 10 to Pan Position */
            fluid_mod_set_source1(default_pan_mod, 10,                    /* index=10 */
                (int)fluid_mod_flags.FLUID_MOD_CC                              /* CC=1 */
                | (int)fluid_mod_flags.FLUID_MOD_LINEAR                        /* type=0 */
                | (int)fluid_mod_flags.FLUID_MOD_BIPOLAR                       /* P=1 */
                | (int)fluid_mod_flags.FLUID_MOD_POSITIVE                      /* D=0 */
            );
            fluid_mod_set_source2(default_pan_mod, 0, 0);                 /* No second source */
            fluid_mod_set_dest(default_pan_mod, (int)fluid_gen_type.GEN_PAN);

            // Target: pan - Amount: 500. The SF specs $8.4.6, p. 55 syas: "Amount = 1000 tenths of a percent". 
            // The center value (64) corresponds to 50%, so it follows that amount = 50% x 1000/% = 500. 
            fluid_mod_set_amount(default_pan_mod, 500.0);


            /* SF2.01 page 55 section 8.4.7: MIDI continuous controller 11 to initial attenuation*/
            fluid_mod_set_source1(default_expr_mod, 11,                     /* index=11 */
                (int)fluid_mod_flags.FLUID_MOD_CC                              /* CC=1 */
                | (int)fluid_mod_flags.FLUID_MOD_CONCAVE                       /* type=1 */
                | (int)fluid_mod_flags.FLUID_MOD_UNIPOLAR                      /* P=0 */
                | (int)fluid_mod_flags.FLUID_MOD_NEGATIVE                      /* D=1 */
            );
            fluid_mod_set_source2(default_expr_mod, 0, 0);                 /* No second source */
            fluid_mod_set_dest(default_expr_mod, (int)fluid_gen_type.GEN_ATTENUATION);         /* Target: Initial attenuation */
            fluid_mod_set_amount(default_expr_mod, 960.0);                 /* Amount: 960 */



            /* SF2.01 page 55 section 8.4.8: MIDI continuous controller 91 to Reverb send */
            fluid_mod_set_source1(default_reverb_mod, 91,                 /* index=91 */
                (int)fluid_mod_flags.FLUID_MOD_CC                              /* CC=1 */
                | (int)fluid_mod_flags.FLUID_MOD_LINEAR                        /* type=0 */
                | (int)fluid_mod_flags.FLUID_MOD_UNIPOLAR                      /* P=0 */
                | (int)fluid_mod_flags.FLUID_MOD_POSITIVE                      /* D=0 */
            );
            fluid_mod_set_source2(default_reverb_mod, 0, 0);              /* No second source */
            fluid_mod_set_dest(default_reverb_mod, (int)fluid_gen_type.GEN_REVERBSEND);       /* Target: Reverb send */
            fluid_mod_set_amount(default_reverb_mod, 200);                /* Amount: 200 ('tenths of a percent') */



            /* SF2.01 page 55 section 8.4.9: MIDI continuous controller 93 to Reverb send */
            fluid_mod_set_source1(default_chorus_mod, 93,                 /* index=93 */
                (int)fluid_mod_flags.FLUID_MOD_CC                              /* CC=1 */
                | (int)fluid_mod_flags.FLUID_MOD_LINEAR                        /* type=0 */
                | (int)fluid_mod_flags.FLUID_MOD_UNIPOLAR                      /* P=0 */
                | (int)fluid_mod_flags.FLUID_MOD_POSITIVE                      /* D=0 */
            );
            fluid_mod_set_source2(default_chorus_mod, 0, 0);              /* No second source */
            fluid_mod_set_dest(default_chorus_mod, (int)fluid_gen_type.GEN_CHORUSSEND);       /* Target: Chorus */
            fluid_mod_set_amount(default_chorus_mod, 200);                /* Amount: 200 ('tenths of a percent') */



            /* SF2.01 page 57 section 8.4.10 MIDI Pitch Wheel to Initial Pitch ... */
            fluid_mod_set_source1(default_pitch_bend_mod, (int)fluid_mod_src.FLUID_MOD_PITCHWHEEL, /* Index=14 */
                (int)fluid_mod_flags.FLUID_MOD_GC                              /* CC =0 */
                | (int)fluid_mod_flags.FLUID_MOD_LINEAR                        /* type=0 */
                | (int)fluid_mod_flags.FLUID_MOD_BIPOLAR                       /* P=1 */
                | (int)fluid_mod_flags.FLUID_MOD_POSITIVE                      /* D=0 */
            );
            fluid_mod_set_source2(default_pitch_bend_mod, (int)fluid_mod_src.FLUID_MOD_PITCHWHEELSENS,  /* Index = 16 */
                (int)fluid_mod_flags.FLUID_MOD_GC                                        /* CC=0 */
                | (int)fluid_mod_flags.FLUID_MOD_LINEAR                                  /* type=0 */
                | (int)fluid_mod_flags.FLUID_MOD_UNIPOLAR                                /* P=0 */
                | (int)fluid_mod_flags.FLUID_MOD_POSITIVE                                /* D=0 */
            );
            fluid_mod_set_dest(default_pitch_bend_mod, (int)fluid_gen_type.GEN_PITCH);                 /* Destination: Initial pitch */
            fluid_mod_set_amount(default_pitch_bend_mod, 12700.0);                 /* Amount: 12700 cents */
        }


        /*
         * fluid_mod_set_source1
         */
        void fluid_mod_set_source1(HiMod mod, int src, int flags)
        {
            mod.Src1 = (byte)src;
            mod.Flags1 = (byte)flags;
        }

        /*
         * fluid_mod_set_source2
         */
        void fluid_mod_set_source2(HiMod mod, int src, int flags)
        {
            mod.Src2 = (byte)src;
            mod.Flags2 = (byte)flags;
        }

        /*
         * fluid_mod_set_dest
         */
        void fluid_mod_set_dest(HiMod mod, int dest)
        {
            mod.Dest = (byte)dest;
        }

        /*
         * fluid_mod_set_amount
         */
        void fluid_mod_set_amount(HiMod mod, double amount)
        {
            mod.Amount = (double)amount;
        }


        /// <summary>
        /// Allocate a synthesis voice. This function is called by a
        /// soundfont's preset in response to a noteon event.
        /// The returned voice comes with default modulators installed(velocity-to-attenuation, velocity to filter, ...)
        /// Note: A single noteon event may create any number of voices, when the preset is layered. Typically 1 (mono) or 2 (stereo).
        /// </summary>
        /// <param name="synth"></param>
        /// <param name="sample"></param>
        /// <param name="chan"></param>
        /// <param name="key"></param>
        /// <param name="vel"></param>
        /// <returns></returns>
        public fluid_voice fluid_synth_alloc_voice(HiSample sample, int chan, int key, int vel)
        {
            fluid_voice voice = null;
            fluid_channel channel = null;

            /*   fluid_mutex_lock(synth.busy); /\* Don't interfere with the audio thread *\/ */
            /*   fluid_mutex_unlock(synth.busy); */

            // check if there's an available synthesis process
            for (int indexVoice = 0; indexVoice < Voices.Count;)
            {
                fluid_voice v = Voices[indexVoice];
                if (v != null && v.gameObject.activeInHierarchy)
                {
                    // Search audioclip not playing with the same wave
                    if (v.status == fluid_voice_status.FLUID_VOICE_OFF && !v.Audiosource.isPlaying && v.Audiosource.clip != null)
                    {
                        if (v.Audiosource.clip.name == sample.Name)
                        {
                            voice = v;
                            if (VerboseVoice) Debug.LogFormat("   Reuse voice {0} {1}", v.IdVoice, v.status);
                            break;
                        }
                        else if (Voices.Count > AutoCleanVoiceLimit)
                        {
                            // Is it an older voice ?
                            //if ((Time.realtimeSinceStartup * 1000d - v.TimeAtStart) > AutoCleanVoiceTime)
                            if ((AudioSettings.dspTime * 1000d - v.TimeAtStart) > AutoCleanVoiceTime)
                            {
                                if (VerboseVoice)
                                    Debug.LogFormat("   Remove voice {0} {1} {2:00} {3:00}", Voices.Count, v.IdVoice, v.TimeAtStart, (Time.realtimeSinceStartup * 1000d - v.TimeAtStart));
                                Destroy(v.gameObject);
                                Voices.RemoveAt(indexVoice);
                            }
                            else
                                indexVoice++;
                        }
                        else
                            indexVoice++;
                    }
                    else
                        indexVoice++;
                }
                else
                    indexVoice++;
            }
#if DEBUGPERF
            DebugPerf("After find existing voice:");
#endif
            // No find existing voice, instanciate a new one
            if (voice == null)
            {
                voice = Instantiate<fluid_voice>(VoiceTemplate);
                voice.transform.position = VoiceTemplate.transform.position;
                voice.transform.SetParent(VoiceTemplate.transform.parent);
                voice.name = "VoiceId_" + voice.IdVoice;

                AudioClip clip = DicAudioClip.Get(sample.Name);
                if (clip == null || clip.loadState != AudioDataLoadState.Loaded)
                {
                    if (VerboseVoice)
                        Debug.LogFormat("Clip {0} not ready to play or not found", sample.Name);
                    return null;
                }

                // Assign sound to audioclip
                voice.Audiosource.clip = clip;
                Voices.Add(voice);
#if DEBUGPERF
                DebugPerf("After instanciate voice:");
#endif
            }

            voice.Audiosource.spatialBlend = MPTK_PauseOnDistance ? 1f : 0f;

            //fluid_global.FLUID_LOG(fluid_log_level.FLUID_INFO, "noteon chan:{0} key:{1} vel:{2} time{3}", chan, key, vel, (fluid_curtime() - start) / 1000.0f);

            if (chan < 0 || chan >= Channels.Length)
            {
                Debug.LogFormat("Channel out of range chan:{0}", chan);
                chan = 0;
            }
            channel = Channels[chan];
            voice.fluid_voice_init(sample, channel, key, vel/*, gain*/);
#if DEBUGPERF
            DebugPerf("After fluid_voice_init:");
#endif
            return voice;
        }

        public void fluid_synth_kill_by_exclusive_class(fluid_voice new_voice)
        {
            ////fluid_synth_t* synth
            ///** Kill all voices on a given channel, which belong into
            //    excl_class.  This function is called by a SoundFont's preset in
            //    response to a noteon event.  If one noteon event results in
            //    several voice processes (stereo samples), ignore_ID must name
            //    the voice ID of the first generated voice (so that it is not
            //    stopped). The first voice uses ignore_ID=-1, which will
            //    terminate all voices on a channel belonging into the exclusive
            //    class excl_class.
            //*/

            //int i;
            //int excl_class = _GEN(new_voice, GEN_EXCLUSIVECLASS);

            ///* Check if the voice belongs to an exclusive class. In that case,
            //   previous notes from the same class are released. */

            ///* Excl. class 0: No exclusive class */
            //if (excl_class == 0)
            //{
            //    return;
            //}

            ////  FLUID_LOG(FLUID_INFO, "Voice belongs to exclusive class (class=%d, ignore_id=%d)", excl_class, ignore_ID);

            ///* Kill all notes on the same channel with the same exclusive class */

            //for (i = 0; i < synth.polyphony; i++)
            //{
            //    fluid_voice_t* existing_voice = synth.voice[i];

            //    /* Existing voice does not play? Leave it alone. */
            //    if (!_PLAYING(existing_voice))
            //    {
            //        continue;
            //    }

            //    /* An exclusive class is valid for a whole channel (or preset).
            //     * Is the voice on a different channel? Leave it alone. */
            //    if (existing_voice.chan != new_voice.chan)
            //    {
            //        continue;
            //    }

            //    /* Existing voice has a different (or no) exclusive class? Leave it alone. */
            //    if ((int)_GEN(existing_voice, GEN_EXCLUSIVECLASS) != excl_class)
            //    {
            //        continue;
            //    }

            //    /* Existing voice is a voice process belonging to this noteon
            //     * event (for example: stereo sample)?  Leave it alone. */
            //    if (fluid_voice_get_id(existing_voice) == fluid_voice_get_id(new_voice))
            //    {
            //        continue;
            //    }

            //    //    FLUID_LOG(FLUID_INFO, "Releasing previous voice of exclusive class (class=%d, id=%d)",
            //    //     (int)_GEN(existing_voice, GEN_EXCLUSIVECLASS), (int)fluid_voice_get_id(existing_voice));

            //    fluid_voice_kill_excl(existing_voice);
            //}
        }
        /// <summary>
        ///  Start a synthesis voice. This function is called by a soundfont's preset in response to a noteon event after the voice  has been allocated with fluid_synth_alloc_voice() and initialized.
        /// Exclusive classes are processed here.
        /// </summary>
        /// <param name="synth"></param>
        /// <param name="voice"></param>

        //public void fluid_synth_start_voice(fluid_voice_t voice)
        //{
        //    //fluid_synth_t synth
        //    /*   fluid_mutex_lock(synth.busy); /\* Don't interfere with the audio thread *\/ */
        //    /*   fluid_mutex_unlock(synth.busy); */

        //    /* Find the exclusive class of this voice. If set, kill all voices
        //     * that match the exclusive class and are younger than the first
        //     * voice process created by this noteon event. */
        //    fluid_synth_kill_by_exclusive_class(voice);

        //    /* Start the new voice */
        //    voice.fluid_voice_start();
        //}

        public HiPreset fluid_synth_find_preset(int banknum, int prognum)
        {
            ImSoundFont sfont = MidiPlayerGlobal.ImSFCurrent;
            if (banknum >= 0 && banknum < sfont.Banks.Length &&
                sfont.Banks[banknum] != null &&
                sfont.Banks[banknum].defpresets != null &&
                prognum < sfont.Banks[banknum].defpresets.Length &&
                sfont.Banks[banknum].defpresets[prognum] != null)
                return sfont.Banks[banknum].defpresets[prognum];

            // Not find, return the first available
            foreach (ImBank bank in sfont.Banks)
                if (bank != null)
                    foreach (HiPreset preset in bank.defpresets)
                        if (preset != null)
                            return preset;
            return null;
        }

        public void synth_noteon(MPTKEvent note)
        {
            HiSample sample;
            fluid_voice voice;
            List<HiMod> mod_list = new List<HiMod>();

            int key = note.Value + MPTK_Transpose;
            int vel = note.Velocity;
            HiPreset preset;
            //DebugPerf("Begin synth_noteon:");

            // Use the preset defined in the channel
            preset = Channels[note.Channel].preset;
            if (preset == null)
            {
                Debug.LogWarningFormat("No preset associated to this channel {0}, cancel playing note: {1}", note.Channel, note.Value);
                return;
            }

            ImSoundFont sfont = MidiPlayerGlobal.ImSFCurrent;
            note.Voices = new List<fluid_voice>();

            // run thru all the zones of this preset 
            foreach (HiZone preset_zone in preset.Zone)
            {
                // check if the note falls into the key and velocity range of this preset 
                if ((preset_zone.KeyLo <= key) &&
                    (preset_zone.KeyHi >= key) &&
                    (preset_zone.VelLo <= vel) &&
                    (preset_zone.VelHi >= vel))
                {
                    if (preset_zone.Index >= 0)
                    {
                        HiInstrument inst = sfont.HiSf.inst[preset_zone.Index];
                        HiZone global_inst_zone = inst.GlobalZone;

                        // run thru all the zones of this instrument */
                        foreach (HiZone inst_zone in inst.Zone)
                        {

                            if (inst_zone.Index < 0 || inst_zone.Index >= sfont.HiSf.Samples.Length)
                                continue;

                            // make sure this instrument zone has a valid sample
                            sample = sfont.HiSf.Samples[inst_zone.Index];
                            if (sample == null)
                                continue;

                            // check if the note falls into the key and velocity range of this instrument

                            if ((inst_zone.KeyLo <= key) &&
                                (inst_zone.KeyHi >= key) &&
                                (inst_zone.VelLo <= vel) &&
                                (inst_zone.VelHi >= vel))
                            {
                                //
                                // Find a sample to play
                                //
                                //Debug.Log("   Found Instrument '" + inst.name + "' index:" + inst_zone.index + " '" + sfont.hisf.Samples[inst_zone.index].Name + "'");
                                //DebugPerf("After found instrument:");

                                voice = fluid_synth_alloc_voice(sample, note.Channel, key, vel);

#if DEBUGPERF
                                DebugPerf("After fluid_synth_alloc_voice:");
#endif

                                if (voice == null) return;


                                note.Voices.Add(voice);
                                voice.Duration = (int)note.Duration;

                                //
                                // Instrument level - Generator
                                // ----------------------------

                                // Global zone

                                // SF 2.01 section 9.4 'bullet' 4: A generator in a local instrument zone supersedes a global instrument zone generator.  
                                // Both cases supersede the default generator. The generator not defined in this instrument do nothing, leave it at the default.

                                if (global_inst_zone != null && global_inst_zone.gens != null)
                                    foreach (HiGen gen in global_inst_zone.gens)
                                    {
                                        //fluid_voice_gen_set(voice, i, global_inst_zone.gen[i].val);
                                        voice.gens[(int)gen.type].Val = gen.Val;
                                        voice.gens[(int)gen.type].flags = fluid_gen_flags.GEN_SET;
                                    }

                                // Local zone
                                if (inst_zone.gens != null && inst_zone.gens != null)
                                    foreach (HiGen gen in inst_zone.gens)
                                    {
                                        //fluid_voice_gen_set(voice, i, global_inst_zone.gen[i].val);
                                        voice.gens[(int)gen.type].Val = gen.Val;
                                        voice.gens[(int)gen.type].flags = fluid_gen_flags.GEN_SET;
                                    }

                                //
                                // Instrument level - Modulators
                                // -----------------------------

                                /// Global zone
                                mod_list = new List<HiMod>();
                                if (global_inst_zone != null && global_inst_zone.mods != null)
                                {
                                    foreach (HiMod mod in global_inst_zone.mods)
                                        mod_list.Add(mod);
                                    //HiMod.DebugLog("      Instrument Global Mods ", global_inst_zone.mods);
                                }
                                //HiMod.DebugLog("      Instrument Local Mods ", inst_zone.mods);

                                // Local zone
                                if (inst_zone.mods != null)
                                    foreach (HiMod mod in inst_zone.mods)
                                    {
                                        // 'Identical' modulators will be deleted by setting their list entry to NULL.  The list length is known. 
                                        // NULL entries will be ignored later.  SF2.01 section 9.5.1 page 69, 'bullet' 3 defines 'identical'.

                                        foreach (HiMod mod1 in mod_list)
                                        {
                                            // fluid_mod_test_identity(mod, mod_list[i]))
                                            if ((mod1.Dest == mod.Dest) &&
                                                (mod1.Src1 == mod.Src1) &&
                                                (mod1.Src2 == mod.Src2) &&
                                                (mod1.Flags1 == mod.Flags1) &&
                                                (mod1.Flags2 == mod.Flags2))
                                            {
                                                mod1.Amount = mod.Amount;
                                                break;
                                            }
                                        }
                                    }

                                // Add instrument modulators (global / local) to the voice.
                                // Instrument modulators -supersede- existing (default) modulators.  SF 2.01 page 69, 'bullet' 6
                                foreach (HiMod mod1 in mod_list)
                                    voice.fluid_voice_add_mod(mod1, fluid_voice_addorover_mod.FLUID_VOICE_OVERWRITE);

                                //
                                // Preset level - Generators
                                // -------------------------

                                //  Local zone
                                if (preset_zone.gens != null && preset_zone.gens != null)
                                    foreach (HiGen gen in preset_zone.gens)
                                    {
                                        //fluid_voice_gen_incr(voice, i, preset.global_zone.gen[i].val);
                                        //if (gen.type==fluid_gen_type.GEN_VOLENVATTACK)
                                        voice.gens[(int)gen.type].Val += gen.Val;
                                        voice.gens[(int)gen.type].flags = fluid_gen_flags.GEN_SET;
                                    }

                                // Global zone
                                if (preset.GlobalZone != null && preset.GlobalZone.gens != null)
                                {
                                    foreach (HiGen gen in preset.GlobalZone.gens)
                                    {
                                        // If not incremented in local, increment in global
                                        if (voice.gens[(int)gen.type].flags != fluid_gen_flags.GEN_SET)
                                            //fluid_voice_gen_incr(voice, i, preset.global_zone.gen[i].val);
                                            voice.gens[(int)gen.type].Val += gen.Val;
                                        voice.gens[(int)gen.type].flags = fluid_gen_flags.GEN_SET;
                                    }
                                }


                                //
                                // Preset level - Modulators
                                // -------------------------

                                // Global zone
                                mod_list = new List<HiMod>();
                                if (preset.GlobalZone != null && preset.GlobalZone.mods != null)
                                {
                                    foreach (HiMod mod in preset.GlobalZone.mods)
                                        mod_list.Add(mod);
                                    //HiMod.DebugLog("      Preset Global Mods ", preset.global_zone.mods);
                                }
                                //HiMod.DebugLog("      Preset Local Mods ", preset_zone.mods);

                                // Local zone
                                if (preset_zone.mods != null)
                                    foreach (HiMod mod in preset_zone.mods)
                                    {
                                        // 'Identical' modulators will be deleted by setting their list entry to NULL.  The list length is known. 
                                        // NULL entries will be ignored later.  SF2.01 section 9.5.1 page 69, 'bullet' 3 defines 'identical'.

                                        foreach (HiMod mod1 in mod_list)
                                        {
                                            // fluid_mod_test_identity(mod, mod_list[i]))
                                            if ((mod1.Dest == mod.Dest) &&
                                                (mod1.Src1 == mod.Src1) &&
                                                (mod1.Src2 == mod.Src2) &&
                                                (mod1.Flags1 == mod.Flags1) &&
                                                (mod1.Flags2 == mod.Flags2))
                                            {
                                                mod1.Amount = mod.Amount;
                                                break;
                                            }
                                        }
                                    }

                                // Add preset modulators (global / local) to the voice.
                                foreach (HiMod mod1 in mod_list)
                                    if (mod1.Amount != 0d)
                                        voice.fluid_voice_add_mod(mod1, fluid_voice_addorover_mod.FLUID_VOICE_OVERWRITE);

#if DEBUGPERF
                                DebugPerf("After genmod init:");
#endif

                                /* add the synthesis process to the synthesis loop. */
                                //fluid_synth_t synth
                                /*   fluid_mutex_lock(synth.busy); /\* Don't interfere with the audio thread *\/ */
                                /*   fluid_mutex_unlock(synth.busy); */

                                /* Find the exclusive class of this voice. If set, kill all voices
                                 * that match the exclusive class and are younger than the first
                                 * voice process created by this noteon event. */
                                fluid_synth_kill_by_exclusive_class(voice);

                                /* Start the new voice */
                                voice.fluid_voice_start();

#if DEBUGPERF
                                DebugPerf("After fluid_voice_start:");
#endif

                                if (MPTK_LogWave)
                                    Debug.LogFormat("NoteOn [C:{0:00} B:{1:000} P:{2:000}]\t{3,-21}\tKey:{4,-3}\tVel:{5,-3}\tDuration:{6:0.000}\tInstr:{7,-21}\t\tWave:{8,-21}\tAtt:{9:0.00}\tPan:{10:0.00}",
                                    note.Channel + 1, Channels[note.Channel].banknum, Channels[note.Channel].prognum, preset.Name, key, vel, note.Duration,
                                    inst.Name,
                                    sfont.HiSf.Samples[inst_zone.Index].Name,
                                    fluid_conv.fluid_atten2amp(voice.attenuation),
                                    voice.Audiosource.panStereo
                                );

                                if (VerboseGenerator)
                                    foreach (HiGen gen in voice.gens)
                                        if (gen != null && gen.flags == fluid_gen_flags.GEN_SET)
                                            Debug.LogFormat("Gen Id:{0,-50}\tValue:{1:0.00}\t\tMod:{2:0.00}", gen.type, gen.Val, gen.Mod);

                                /* Store the ID of the first voice that was created by this noteon event.
                                 * Exclusive class may only terminate older voices.
                                 * That avoids killing voices, which have just been created.
                                 * (a noteon event can create several voice processes with the same exclusive
                                 * class - for example when using stereo samples)
                                 */
                            }
                            if (MPTK_PlayOnlyFirstWave && note.Voices.Count > 0)
                                return;
                        }
                    }

                }
            }
#if DEBUGPERF
            DebugPerf("After synth_noteon:");
#endif
            if (MPTK_LogWave && note.Voices.Count == 0)
                Debug.LogFormat("NoteOn [{0:00} {1:000} {2:000}]\t{3,-21}\tKey:{4,-3}\tVel:{5,-3}\tDuration:{6:0.000}\tInstr:{7,-21}",
                note.Channel, Channels[note.Channel].banknum, Channels[note.Channel].prognum, preset.Name, key, vel, note.Duration, "*** no wave found ***");
        }

        public void fluid_synth_allnotesoff()
        {
            for (int chan = 0; chan < Channels.Length; chan++)
                fluid_synth_noteoff(chan, -1);
        }

        public void fluid_synth_noteoff(int pchan, int pkey)
        {
            foreach (fluid_voice voice in Voices)
            {
                // A voice is 'ON', if it has not yet received a noteoff event. Sending a noteoff event will advance the envelopes to  section 5 (release). 
                //#define _ON(voice)  ((voice)->status == FLUID_VOICE_ON && (voice)->volenv_section < FLUID_VOICE_ENVRELEASE)
                if (voice.status == fluid_voice_status.FLUID_VOICE_ON &&
                    voice.volenv_section < fluid_voice_envelope_index.FLUID_VOICE_ENVRELEASE &&
                    voice.chan == pchan &&
                    (pkey == -1 || voice.key == pkey))
                {
                    //fluid_global.FLUID_LOG(fluid_log_level.FLUID_INFO, "noteoff chan:{0} key:{1} vel:{2} time{3}", voice.chan, voice.key, voice.vel, (fluid_curtime() - start) / 1000.0f);
                    voice.fluid_voice_noteoff();
                }
            }
        }

        public void fluid_synth_soundoff(int pchan)
        {
            foreach (fluid_voice voice in Voices)
            {
                // A voice is 'ON', if it has not yet received a noteoff event. Sending a noteoff event will advance the envelopes to  section 5 (release). 
                //#define _ON(voice)  ((voice)->status == FLUID_VOICE_ON && (voice)->volenv_section < FLUID_VOICE_ENVRELEASE)
                if (voice.status == fluid_voice_status.FLUID_VOICE_ON &&
                    voice.volenv_section < fluid_voice_envelope_index.FLUID_VOICE_ENVRELEASE &&
                    voice.chan == pchan)
                {
                    //fluid_global.FLUID_LOG(fluid_log_level.FLUID_INFO, "noteoff chan:{0} key:{1} vel:{2} time{3}", voice.chan, voice.key, voice.vel, (fluid_curtime() - start) / 1000.0f);
                    voice.fluid_voice_off();
                }
            }
        }

        /*
         * fluid_synth_damp_voices
         */
        public void fluid_synth_damp_voices(int pchan)
        {
            foreach (fluid_voice voice in Voices)
            {
                //#define _SUSTAINED(voice)  ((voice)->status == FLUID_VOICE_SUSTAINED)
                if (voice.chan == pchan && voice.status == fluid_voice_status.FLUID_VOICE_SUSTAINED)
                    voice.fluid_voice_noteoff();
            }
        }

        /*
         * fluid_synth_cc - call directly
         */
        public void fluid_synth_cc(int chan, MPTKController num, int val)
        {
            /*   fluid_mutex_lock(synth->busy); /\* Don't interfere with the audio thread *\/ */
            /*   fluid_mutex_unlock(synth->busy); */

            /* check the ranges of the arguments */
            //if ((chan < 0) || (chan >= synth->midi_channels))
            //{
            //    FLUID_LOG(FLUID_WARN, "Channel out of range");
            //    return FLUID_FAILED;
            //}
            //if ((num < 0) || (num >= 128))
            //{
            //    FLUID_LOG(FLUID_WARN, "Ctrl out of range");
            //    return FLUID_FAILED;
            //}
            //if ((val < 0) || (val >= 128))
            //{
            //    FLUID_LOG(FLUID_WARN, "Value out of range");
            //    return FLUID_FAILED;
            //}

            /* set the controller value in the channel */
            Channels[chan].fluid_channel_cc(num, val);
        }

        /// <summary>
        /// tell all synthesis activ voices on this channel to update their synthesis parameters after a control change.
        /// </summary>
        /// <param name="chan"></param>
        /// <param name="is_cc"></param>
        /// <param name="ctrl"></param>
        public void fluid_synth_modulate_voices(int chan, int is_cc, int ctrl)
        {
            foreach (fluid_voice voice in Voices)
                if (voice.chan == chan && voice.status != fluid_voice_status.FLUID_VOICE_OFF)
                    voice.fluid_voice_modulate(is_cc, ctrl);
        }

        /// <summary>
        /// Tell all synthesis processes on this channel to update their synthesis parameters after an all control off message (i.e. all controller have been reset to their default value).
        /// </summary>
        /// <param name="chan"></param>
        public void fluid_synth_modulate_voices_all(int chan)
        {
            foreach (fluid_voice voice in Voices)
                if (voice.chan == chan)
                    voice.fluid_voice_modulate_all();
        }


        /*
         * fluid_synth_program_change
         */
        public void fluid_synth_program_change(int pchan, int prognum)
        {
            fluid_channel channel;
            HiPreset preset;
            int banknum;

            //if ((prognum >= 0) && (prognum < FLUID_NUM_PROGRAMS) && (pchan >= 0) && (pchan < synth->midi_channels))
            {

                channel = Channels[pchan];
                banknum = channel.banknum; //fluid_channel_get_banknum
                channel.prognum = prognum; // fluid_channel_set_prognum
                if (VerboseVoice) Debug.LogFormat("ProgramChange\tChannel:{0}\tBank:{1}\tPreset:{2}", pchan, banknum, prognum);

                // special handling of channel 10 (or 9 counting from 0). channel 10 is the percussion channel. 
                if (channel.channum == 9)
                {
                    // try to search the drum instrument first
                    preset = fluid_synth_find_preset(banknum, prognum);

                    //// if that fails try to search the melodic instrument
                    //if (preset == null)
                    //{
                    //    preset = fluid_synth_find_preset(banknum, prognum);
                    //}

                }
                else
                {
                    preset = fluid_synth_find_preset(banknum, prognum);
                }

                //sfont_id = preset ? fluid_sfont_get_id(preset->sfont) : 0;
                //fluid_channel_set_sfontnum(channel, sfont_id);
                channel.preset = preset; // fluid_channel_set_preset
            }
        }


        /*
         * fluid_synth_pitch_bend
         */
        void fluid_synth_pitch_bend(int chan, int val)
        {
            if (MPTK_ApplyRealTimeModulator)
            {
                /*   fluid_mutex_lock(synth->busy); /\* Don't interfere with the audio thread *\/ */
                /*   fluid_mutex_unlock(synth->busy); */

                /* check the ranges of the arguments */
                if (chan < 0 || chan >= Channels.Length)
                {
                    Debug.LogFormat("Channel out of range chan:{0}", chan);
                    return;
                }

                /* set the pitch-bend value in the channel */
                Channels[chan].fluid_channel_pitch_bend(val);
            }
        }

        //! @endcond

#if DEBUGPERF

        float perf_time_before;
        float perf_time_cumul;
        public List<string> perfs;

        public void DebugPerf(string info, int mode = 1)
        {
            // Init
            if (mode == 0)
            {
                perfs = new List<string>();
                perf_time_before = Time.realtimeSinceStartup;
                perf_time_cumul = 0;
            }
            if (perfs != null)
            {
                float delta = (Time.realtimeSinceStartup - perf_time_before) * 1000f;
                perf_time_before = Time.realtimeSinceStartup;
                perf_time_cumul += delta;
                string perf = string.Format("{0,-30} {1,5:F5} {2,5:F5}", info, delta, perf_time_cumul);
                perfs.Add(perf);
                // Debug.Log(perf);
            }
            // Close
            if (mode == 2)
            {
                //foreach (string p in perfs) Debug.Log(p);
                Debug.Log(perfs.Last());
            }
        }
#endif
    }
}
