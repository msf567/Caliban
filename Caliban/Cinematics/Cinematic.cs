using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using NAudio.Wave;
using Caliban.Core.Audio;
using Caliban.Core.Transport;
using Caliban.Core.Debug;
using Treasures.Resources;

namespace Caliban.Core.Cinematics
{
    public class Cinematic
    {
        private readonly ServerTerminal server;
        private readonly string name;
        private readonly List<CinematicCue> cues;
        private readonly bool[] dispatched;

        private MemoryStream memoryStream;
        private WaveFileReader reader;
        private WaveChannel32 channel;
        private IWavePlayer output;

        private readonly object sync = new object();
        private volatile bool playing;
        private Thread playbackThread;
        private bool disposed;

        public bool IsPlaying => playing;

        public Cinematic(ServerTerminal _server, string _cinematicName)
        {
            server = _server;
            name = _cinematicName;
            cues = new List<CinematicCue>();

            try
            {
                string scriptText = TreasureManager.GetResourceText("Cinematics." + name + ".Script.txt");
                cues = CinematicScriptParser.Parse(scriptText);
            }
            catch (Exception e)
            {
                D.Write("Cinematic '" + name + "' failed to load script: " + e.Message);
                cues = new List<CinematicCue>();
            }

            dispatched = new bool[cues.Count];

            try
            {
                using (Stream trackStream = TreasureManager.GetStream("Cinematics." + name + ".Track.wav"))
                {
                    if (trackStream == null)
                    {
                        D.Write("Cinematic '" + name + "' track resource not found.");
                    }
                    else
                    {
                        memoryStream = new MemoryStream(WavePlayer.StreamToBytes(trackStream));
                        reader = new WaveFileReader(memoryStream);
                        channel = new WaveChannel32(reader) { PadWithZeroes = false };
                        output = new WaveOutEvent();
                        output.Init(channel);
                    }
                }
            }
            catch (Exception e)
            {
                D.Write("Cinematic '" + name + "' failed to load track: " + e.Message);
                DisposeAudio();
            }
        }

        public void Play()
        {
            lock (sync)
            {
                if (playing || disposed || channel == null || output == null)
                    return;

                for (int i = 0; i < dispatched.Length; i++)
                    dispatched[i] = false;

                try
                {
                    channel.Position = 0;
                }
                catch (Exception)
                {
                    // ignored
                }

                playing = true;
                playbackThread = new Thread(PlaybackLoop)
                {
                    IsBackground = true,
                    Name = "Cinematic_" + name
                };
                playbackThread.Start();

                try
                {
                    output.Play();
                }
                catch (Exception e)
                {
                    D.Write("Cinematic '" + name + "' failed to start audio: " + e.Message);
                }
            }
        }

        private void PlaybackLoop()
        {
            bool reachedEnd = false;
            try
            {
                while (playing)
                {
                    TimeSpan pos = channel.CurrentTime;

                    for (int i = 0; i < cues.Count; i++)
                    {
                        if (dispatched[i] || cues[i].Time > pos)
                            continue;

                        dispatched[i] = true;
                        try
                        {
                            server.SendMessageToSelf(Messages.Build(MessageType.CHOREO, cues[i].Label));
                        }
                        catch (Exception e)
                        {
                            D.Write("Cinematic '" + name + "' dispatch failed: " + e.Message);
                        }
                    }

                    if (pos >= channel.TotalTime)
                    {
                        reachedEnd = true;
                        break;
                    }

                    Thread.Sleep(20);
                }
            }
            catch (Exception e)
            {
                D.Write("Cinematic '" + name + "' playback error: " + e.Message);
            }

            if (reachedEnd)
                Stop();
        }

        public void Stop()
        {
            Thread threadToJoin;
            lock (sync)
            {
                if (disposed)
                    return;

                playing = false;
                threadToJoin = playbackThread;
                playbackThread = null;
            }

            if (threadToJoin != null && threadToJoin != Thread.CurrentThread)
                threadToJoin.Join(1000);

            DisposeAudio();
        }

        private void DisposeAudio()
        {
            lock (sync)
            {
                if (disposed)
                    return;
                disposed = true;
            }

            try
            {
                output?.Stop();
            }
            catch (Exception)
            {
                /* ignored */
            }

            try
            {
                output?.Dispose();
            }
            catch (Exception)
            {
                /* ignored */
            }

            try
            {
                channel?.Dispose();
            }
            catch (Exception)
            {
                /* ignored */
            }

            try
            {
                reader?.Dispose();
            }
            catch (Exception)
            {
                /* ignored */
            }

            try
            {
                memoryStream?.Dispose();
            }
            catch (Exception)
            {
                /* ignored */
            }

            output = null;
            channel = null;
            reader = null;
            memoryStream = null;
        }
    }
}