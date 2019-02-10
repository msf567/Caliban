using System;
using System.ComponentModel;
using CSCore;
using CSCore.Codecs;
using CSCore.CoreAudioAPI;
using CSCore.SoundOut;

namespace DesertGenerator
{
    public class MusicPlayer : IDisposable
    {
        private ISoundOut _soundOut;
        private IWaveSource _waveSource;

		public event EventHandler<PlaybackStoppedEventArgs> PlaybackStopped;

        public PlaybackState PlaybackState => _soundOut?.PlaybackState ?? PlaybackState.Stopped;

        public TimeSpan Position
        {
            get => _waveSource?.GetPosition() ?? TimeSpan.Zero;
            set => _waveSource?.SetPosition(value);
        }

        public TimeSpan Length => _waveSource?.GetLength() ?? TimeSpan.Zero;

        public int Volume
        {
            get => _soundOut != null ? Math.Min(100, Math.Max((int)(_soundOut.Volume * 100), 0)) : 100;
            set
            {
                if (_soundOut != null)
                {
                    _soundOut.Volume = Math.Min(1.0f, Math.Max(value / 100f, 0f));
                }
            }
        }

        public void Open(string filename, MMDevice device)
        {
            CleanupPlayback();

            _waveSource =
                CodecFactory.Instance.GetCodec(filename)
                    .ToSampleSource()
                    .ToMono()
                    .ToWaveSource();
            _soundOut = new WasapiOut() {Latency = 100, Device = device};
            _soundOut.Initialize(_waveSource);
			if (PlaybackStopped != null) _soundOut.Stopped += PlaybackStopped;
        }

        public void Play()
        {
            _soundOut?.Play();
        }

        public void Pause()
        {
            _soundOut?.Pause();
        }

        public void Stop()
        {
            _soundOut?.Stop();
        }

        private void CleanupPlayback()
        {
            if (_soundOut != null)
            {
                _soundOut.Dispose();
                _soundOut = null;
            }

            if (_waveSource == null) return;
            _waveSource.Dispose();
            _waveSource = null;
        }

        protected void Dispose(bool disposing)
        {
            CleanupPlayback();
        }

        public void Dispose()
        {
            _soundOut?.Dispose();
            _waveSource?.Dispose();
        }
    }
}