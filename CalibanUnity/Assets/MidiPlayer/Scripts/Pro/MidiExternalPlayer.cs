
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using System;
using UnityEngine.Events;
using System.Net;
using MEC;

namespace MidiPlayerTK
{
    /// <summary>
    /// PRO Version - Script for the prefab MidiExternalPlayer. See full example TestMidiExternalPlayer.cs with a light sequencer.
    /// Play a midi file from a path on the local deskop or from a web site
    /// </summary>
    public class MidiExternalPlayer : MidiFilePlayer
    {
        /// <summary>
        /// Full path to Midi file or URL to play. must start with file:// or http:// or https://.
        ///! @code
        /// MidiExternalPlayer midiExternalPlayer = FindObjectOfType<MidiExternalPlayer>();
        /// MidiExternalPlayer.MPTK_MidiName = @"C:\Users\xxx\Midi\Bach The Art of Fugue - No1.mid";
        ///     //or
        /// MidiExternalPlayer.MPTK_MidiName = "http://www.midiworld.com/midis/other/bach/bwv1060b.mid";
        /// MidiExternalPlayer.MPTK_Play();
        ///! @endcode
        /// </summary>
        public override string MPTK_MidiName
        {
            get
            {
                return pathmidiNameToPlay;
            }
            set
            {
                pathmidiNameToPlay = value.Trim();
            }
        }
        [SerializeField]
        [HideInInspector]
        private string pathmidiNameToPlay;

        protected override void Awake()
        {
            //Debug.Log("Awake MidiExternalPlayer:" + MPTK_IsPlaying);
            base.Awake();
        }

        protected override void Start()
        {
            base.Start();
        }

        /// <summary>
        /// Play the midi file defined in MPTK_MidiName
        ///! @code
        /// MidiExternalPlayer midiExternalPlayer = FindObjectOfType<MidiExternalPlayer>();
        /// MidiExternalPlayer.MPTK_MidiName = @"C:\Users\xxx\Midi\Bach The Art of Fugue - No1.mid";
        ///     //or
        /// MidiExternalPlayer.MPTK_MidiName = "http://www.midiworld.com/midis/other/bach/bwv1060b.mid";
        /// MidiExternalPlayer.MPTK_Play();
        ///! @endcode
        /// </summary>
        public override void MPTK_Play()
        {
            try
            {
                if (MidiPlayerGlobal.MPTK_SoundFontLoaded)
                {
                    playPause = false;
                    if (!MPTK_IsPlaying)
                    {
                        MPTK_InitSynth();

                        if (string.IsNullOrEmpty(pathmidiNameToPlay))
                            Debug.Log("MPTK_Play: set MPTK_MidiName before playing");
                        else if (!pathmidiNameToPlay.ToLower().StartsWith("file://") &&
                                 !pathmidiNameToPlay.ToLower().StartsWith("http://") &&
                                 !pathmidiNameToPlay.ToLower().StartsWith("https://"))
                            Debug.LogWarning("MPTK_MidiName must start with file:// or http:// or https:// - found: '" + pathmidiNameToPlay + "'");
                        else
                            Timing.RunCoroutine(TheadLoadDataAndPlay());
                    }
                    else
                        Debug.LogWarning("Already playing - " + pathmidiNameToPlay);
                }
                else
                    Debug.LogWarning("Soundfont not loaded");
            }

            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        /// <summary>
        /// Load midi file in background and play
        /// </summary>
        /// <returns></returns>
        private IEnumerator<float> TheadLoadDataAndPlay()
        {
            //for TU
            //pathmidiNameToPlay = @"C:\Users\Thierry\Desktop\BIM\Sound\Midi\Bach The Art of Fugue - No1.mid";
            //pathmidiNameToPlay = "http://www.midishrine.com/midipp/ngc/Animal_Crossing/kk_ballad.mid";
            //pathmidiNameToPlay = "http://www.midiworld.com/download/4000";
            //pathmidiNameToPlay = "http://www.midiworld.com/midis/other/bach/bwv1060b.mid";
            //http://www.midishrine.com/midipp/n64/Zelda_64_-_The_Ocarina_of_Time/kakariko.mid

            // Asynchrone loading of the midi file
            using (WWW www = new WWW(pathmidiNameToPlay))
            {
                //yield return www;
                yield return Timing.WaitUntilDone(www);
                if (www.bytes != null && www.bytes.Length > 4 && System.Text.Encoding.Default.GetString(www.bytes, 0, 4) == "MThd")
                    // Start playing
                    Timing.RunCoroutine(ThreadPlay(www.bytes).CancelWith(gameObject));
                else
                    Debug.LogWarning("Midi not find or not a Midi file - " + pathmidiNameToPlay);
            }
        }

        //// Not used, unity WWW is better !
        //private class WebClient : System.Net.WebClient
        //{
        //    public int Timeout { get; set; }

        //    protected override WebRequest GetWebRequest(Uri uri)
        //    {
        //        WebRequest lWebRequest = base.GetWebRequest(uri);
        //        lWebRequest.Timeout = Timeout;
        //        ((HttpWebRequest)lWebRequest).ReadWriteTimeout = Timeout;
        //        return lWebRequest;
        //    }
        //}
        //private bool IsUri(string path)
        //{
        //    Uri uri;
        //    return Uri.TryCreate(path, UriKind.Absolute, out uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        //}
        // Not used ...
        //private bool IsUrlExist(string url)
        //{
        //    bool exist = true;
        //    try
        //    {
        //        WebRequest req = WebRequest.Create(url);
        //        req.Timeout = 5000; //ms
        //        WebResponse res = req.GetResponse();
        //    }
        //    catch (WebException ex)
        //    {
        //        Debug.Log(ex.Message);
        //        exist = false;
        //    }
        //    return exist;
        //}


        /// <summary>
        /// Index Midi to play or playing - NO EFFECT for external
        /// </summary>
        public override int MPTK_MidiIndex
        {
            get
            {
                Debug.LogWarning("MPTK_MidiIndex not available for MidiExternalPlayer");
                return -1;
            }
            set
            {
                Debug.LogWarning("MPTK_MidiIndex not available for MidiExternalPlayer");
            }
        }

        /// <summary>
        /// Play next Midi - NO EFFECT for external
        /// </summary>
        public override void MPTK_Next()
        {
            try
            {
                Debug.LogWarning("MPTK_Next not available for MidiExternalPlayer");
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        /// <summary>
        /// Play previous Midi - NO EFFECT for external
        /// </summary>
        public override void MPTK_Previous()
        {
            try
            {
                Debug.LogWarning("MPTK_Next not available for MidiExternalPlayer");
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }
    }
}

