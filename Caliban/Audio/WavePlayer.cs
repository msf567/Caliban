using NAudio.Wave;

namespace Caliban.Core.Audio
{
    public class WavePlayer
    {
        WaveFileReader Reader;
        public WaveChannel32 Channel { get; set; }
        public LoopStream Stream;
        string FileName { get; set; }

        public WavePlayer (string FileName)
        {
            this.FileName = FileName;
            Reader = new WaveFileReader(FileName);
            Stream = new LoopStream(Reader);
            Channel = new WaveChannel32(Stream) { PadWithZeroes = false };
        }

        public void Dispose()
        {
            if (Channel != null)
            {
                Channel.Dispose();
                Reader.Dispose();
            }
        }

    }
}