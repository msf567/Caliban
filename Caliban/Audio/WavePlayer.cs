using System;
using System.IO;
using NAudio.Wave;

namespace Caliban.Core.Audio
{
    public class WavePlayer
    {
        WaveStream Stream;
        public WaveChannel32 Channel { get; set; }
        public LoopStream LoopStream;

        public WavePlayer (string FileName)
        {
            Stream = new WaveFileReader(FileName);
            LoopStream = new LoopStream(Stream);
            Channel = new WaveChannel32(LoopStream) { PadWithZeroes = false };
        }

        public WavePlayer(Stream _stream)
        {
            MemoryStream ms = new MemoryStream(StreamToBytes(_stream));
            Stream = new WaveFileReader(ms);
            //Stream = new RawSourceWaveStream(_stream, new WaveFormat());
            LoopStream = new LoopStream(Stream);
            Channel = new WaveChannel32(LoopStream) { PadWithZeroes = false };
        }

        public void Dispose()
        {
            if (Channel != null)
            {
                Channel.Dispose();
                Stream.Dispose();
            }
        }
        
        public static byte[] StreamToBytes(System.IO.Stream stream)
        {
            long originalPosition = 0;

            if(stream.CanSeek)
            {
                originalPosition = stream.Position;
                stream.Position = 0;
            }

            try
            {
                byte[] readBuffer = new byte[4096];

                int totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;

                    if (totalBytesRead == readBuffer.Length)
                    {
                        int nextByte = stream.ReadByte();
                        if (nextByte != -1)
                        {
                            byte[] temp = new byte[readBuffer.Length * 2];
                            Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                            Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                            readBuffer = temp;
                            totalBytesRead++;
                        }
                    }
                }

                byte[] buffer = readBuffer;
                if (readBuffer.Length != totalBytesRead)
                {
                    buffer = new byte[totalBytesRead];
                    Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
                }
                return buffer;
            }
            finally
            {
                if(stream.CanSeek)
                {
                    stream.Position = originalPosition; 
                }
            }
        }

    }
}