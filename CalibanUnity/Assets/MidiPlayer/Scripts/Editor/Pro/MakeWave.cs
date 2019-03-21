using System.Collections;
using System.Diagnostics;
using System.IO;
using MidiPlayerTK;

namespace MidiPlayerTK
{

    public class CreateWave
    {
        struct WaveHeader
        {
            public byte[] riffID;
            public uint size;
            public byte[] waveID;
            public byte[] formatID;
            public uint formatSize;
            public ushort format;
            public ushort channels;
            public uint sampleRate;
            public uint bytePerSec;
            public ushort blockSize;
            public ushort bit;
            public byte[] ID;
            public uint dataSize;
        }

        struct SampleHeader
        {
            public byte[] sampleID;
            public uint size;
            public uint manunfacturer;
            public uint product;
            public uint samplePeriod;
            public uint midiUnityNote;
            public uint midiPitchFraction;
            public uint smpteFormat;
            public uint smpteOffset;
            public uint numSampleLoops;
            public uint samplerData;

            public uint cuePointId;
            public uint lpType;
            public uint lpStart;
            public uint lpEnd;
            public uint lpFraction;
            public uint lpPlayCount;
        }

        public bool Extract(string filePath, HiSample shdr, byte[] smplData)
        {
            bool loop = false;
            try
            {
                //WriteRawFile(filePath, smplData, shdr.start, shdr.end);
                //return;
                //if (filePath.Contains("Xylophone")) System.Diagnostics.Debugger.Break();

                uint loopstart = (uint)((int)shdr.LoopStart + shdr.Correctedloopstart + 32768 * shdr.Correctedcoarseloopstart);
                uint loopend = (uint)((int)shdr.LoopEnd + shdr.Correctedloopend + 32768 * shdr.Correctedcoarseloopend);

                uint loopPre = (uint)((loopstart - shdr.Start));
                uint loopLength = (uint)((loopend - loopstart));
                uint loopPost;
                if (shdr.End > loopend)
                    loopPost = (uint)((shdr.End - loopend));
                else
                    loopPost = (uint)((loopend - shdr.End));

                if (loopLength == 0 || (loopLength + loopPost) >= (shdr.End - shdr.Start))
                {
                    //UnityEngine.Debug.Log("Create wave " + loopstart + " " + filePath);
                    //UnityEngine.Debug.Log("   loopstart:" + ((int)loopstart) + " SampleRate:" + shdr.SampleRate + " PitchCorrection:" + shdr.PitchCorrection);
                    //UnityEngine.Debug.Log("   No loop");
                    //UnityEngine.Debug.Log("   loopPre:" + loopPre + " loopLength:" + loopLength + " loopPost:" + loopPost);
                    byte[] samples = new byte[(shdr.End - shdr.Start) * 2];
                    System.Array.Copy(smplData, shdr.Start * 2, samples, 0, (shdr.End - shdr.Start) * 2);
                    WriteWaveFile(filePath, samples, shdr.SampleRate, 0, 0);
                }
                else if (loopLength < 512)
                {
                    //UnityEngine.Debug.Log("   loopLength < 512");
                    loop = true;
                    byte[] samples = new byte[(loopPre + loopLength * 32 + loopPost) * 2];

                    System.Array.Copy(smplData, (shdr.Start) * 2, samples, 0, (loopPre) * 2);
                    for (int i = 0; i < 32; i++)
                    {
                        System.Array.Copy(smplData, (loopstart) * 2, samples, (loopPre) * 2 + (loopLength) * 2 * i, (loopLength) * 2);
                    }
                    System.Array.Copy(smplData, (loopend) * 2, samples, (loopPre) * 2 + (loopLength) * 2 * 32, (loopPost) * 2);

                    WriteWaveFile(filePath, samples, shdr.SampleRate,
                        (uint)((loopstart - shdr.Start)),
                        (uint)((loopstart - shdr.Start) + loopLength * 32) - 1);

                }
                else if (loopLength < 1024)
                {
                    //UnityEngine.Debug.Log("   loopLength < 1024");
                    loop = true;

                    byte[] samples = new byte[(loopPre + loopLength * 16 + loopPost) * 2];

                    System.Array.Copy(smplData, (shdr.Start) * 2, samples, 0, (loopPre) * 2);
                    for (int i = 0; i < 16; i++)
                    {
                        System.Array.Copy(smplData, (loopstart) * 2, samples, (loopPre) * 2 + (loopLength) * 2 * i, (loopLength) * 2);
                    }
                    System.Array.Copy(smplData, (loopend) * 2, samples, (loopPre) * 2 + (loopLength) * 2 * 16, (loopPost) * 2);

                    WriteWaveFile(filePath, samples, shdr.SampleRate,
                        (uint)((loopstart - shdr.Start)),
                        (uint)((loopstart - shdr.Start) + loopLength * 16) - 1);

                }
                else
                {
                    //UnityEngine.Debug.Log("   loopLength >= 1024");
                    loop = true;
                    byte[] samples = new byte[(shdr.End - shdr.Start) * 2];

                    System.Array.Copy(smplData, shdr.Start * 2, samples, 0, (shdr.End - shdr.Start) * 2);
                    WriteWaveFile(filePath, samples, shdr.SampleRate,
                        (uint)((loopstart - shdr.Start)),
                        (uint)((loopstart - shdr.Start)) + (uint)(loopend - loopstart));

                    //WriteRawFile(filePath, samples, 
                    //    (uint)((loopstart - shdr.start)),
                    //    (uint)((loopstart - shdr.start)) + (uint)(loopend - loopstart));
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
            return loop;
        }

        public static string EscapeConvert(string name)
        {
            foreach (char i in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(i, '_');
            }
            name = name.Replace('#', 'd');
            name = name.Replace(' ', '-');
            return name;
        }

        public void WriteWaveFile(string filePath, byte[] waveSamples, uint rate, uint lpStart, uint lpEnd)
        {
            try
            {
                if (Directory.Exists(Path.GetDirectoryName(filePath)) == false)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                }

                WaveHeader Header = new WaveHeader();
                Header.riffID = new byte[4];
                Header.riffID[0] = (byte)'R';
                Header.riffID[1] = (byte)'I';
                Header.riffID[2] = (byte)'F';
                Header.riffID[3] = (byte)'F';
                Header.waveID = new byte[4];
                Header.waveID[0] = (byte)'W';
                Header.waveID[1] = (byte)'A';
                Header.waveID[2] = (byte)'V';
                Header.waveID[3] = (byte)'E';
                Header.formatID = new byte[4];
                Header.formatID[0] = (byte)'f';
                Header.formatID[1] = (byte)'m';
                Header.formatID[2] = (byte)'t';
                Header.formatID[3] = (byte)' ';
                Header.formatSize = 0x10;
                Header.format = 1;
                Header.channels = 1;
                Header.sampleRate = rate;
                Header.bytePerSec = 2;
                Header.blockSize = 4;
                Header.bit = 16;
                Header.ID = new byte[4];
                Header.ID[0] = (byte)'d';
                Header.ID[1] = (byte)'a';
                Header.ID[2] = (byte)'t';
                Header.ID[3] = (byte)'a';


                Header.dataSize = (uint)waveSamples.Length;
                Header.size = (uint)Header.dataSize + 16 + 4;

                if (lpEnd != 0)
                {
                    Header.size += 0x44;
                }

                using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                using (BinaryWriter bw = new BinaryWriter(fs))
                {
                    try
                    {
                        bw.Write(Header.riffID);
                        bw.Write(Header.size);
                        bw.Write(Header.waveID);
                        bw.Write(Header.formatID);
                        bw.Write(Header.formatSize);
                        bw.Write(Header.format);
                        bw.Write(Header.channels);
                        bw.Write(Header.sampleRate);
                        bw.Write(Header.bytePerSec);
                        bw.Write(Header.blockSize);
                        bw.Write(Header.bit);
                        bw.Write(Header.ID);
                        bw.Write(Header.dataSize);

                        bw.Write(waveSamples);

                        if (lpEnd != 0)
                        {
                            SampleHeader smplHeader = new SampleHeader();
                            smplHeader.sampleID = new byte[4];
                            smplHeader.sampleID[0] = (byte)'s';
                            smplHeader.sampleID[1] = (byte)'m';
                            smplHeader.sampleID[2] = (byte)'p';
                            smplHeader.sampleID[3] = (byte)'l';

                            smplHeader.size = 0x3c;
                            smplHeader.manunfacturer = 0;
                            smplHeader.product = 0;
                            smplHeader.samplePeriod = 0;
                            smplHeader.midiUnityNote = 0x3C;
                            smplHeader.midiPitchFraction = 0;
                            smplHeader.smpteFormat = 0;
                            smplHeader.smpteOffset = 0;
                            smplHeader.numSampleLoops = 1;
                            smplHeader.samplerData = 0;

                            smplHeader.cuePointId = 0;
                            smplHeader.lpType = 0;
                            smplHeader.lpStart = lpStart;
                            smplHeader.lpEnd = lpEnd;
                            smplHeader.lpFraction = 0;
                            smplHeader.lpPlayCount = 0;

                            bw.Write(smplHeader.sampleID);
                            bw.Write(smplHeader.size);
                            bw.Write(smplHeader.manunfacturer);
                            bw.Write(smplHeader.product);
                            bw.Write(smplHeader.samplePeriod);
                            bw.Write(smplHeader.midiUnityNote);
                            bw.Write(smplHeader.midiPitchFraction);
                            bw.Write(smplHeader.smpteFormat);
                            bw.Write(smplHeader.smpteOffset);
                            bw.Write(smplHeader.numSampleLoops);
                            bw.Write(smplHeader.samplerData);
                            bw.Write(smplHeader.cuePointId);
                            bw.Write(smplHeader.lpType);
                            bw.Write(smplHeader.lpStart);
                            bw.Write(smplHeader.lpEnd);
                            bw.Write(smplHeader.lpFraction);
                            bw.Write(smplHeader.lpPlayCount);
                        }
                    }
                    finally
                    {
                        if (bw != null)
                        {
                            bw.Close();
                        }
                        if (fs != null)
                        {
                            fs.Close();
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }
        public void WriteRawFile(string filePath, byte[] waveSamples, uint lpStart, uint lpEnd)
        {
            try
            {
                if (Directory.Exists(Path.GetDirectoryName(filePath)) == false)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                }

                using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                using (BinaryWriter bw = new BinaryWriter(fs))
                {
                    try
                    {
                        bw.Write(waveSamples, (int)lpStart, (int)(lpEnd - lpStart));
                    }
                    finally
                    {
                        if (bw != null)
                        {
                            bw.Close();
                        }
                        if (fs != null)
                        {
                            fs.Close();
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }
    }
}