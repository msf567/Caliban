using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MidiPlayerTK
{

    public class SFSave
    {
        public SFData SfData;
        private BinaryWriter fd;
        //public SFSave(Stream sfFile)
        //{
        //    fd = new BinaryWriter(sfFile);
        //}

        public SFSave(string fileName, SFData sf)
        {
            SfData = sf;
            using (Stream sfFile = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                using (fd = new BinaryWriter(sfFile))
                    SaveBody();
            }
        }

        private string ChnkIdStr(int id)
        {
            return SFFile.idlist[id];
        }

        private void WriteChunk(File_Chunk_ID id, uint size)
        {
            byte[] bytes = ByteEncoding.Instance.GetBytes(ChnkIdStr((int)id));
            fd.Write(bytes, 0, bytes.Length);
            fd.Write(size);
        }

        private void WriteId(File_Chunk_ID id)
        {
            byte[] bytes = ByteEncoding.Instance.GetBytes(ChnkIdStr((int)id));
            fd.Write(bytes, 0, bytes.Length);
        }

        private void WriteStr(string var)
        {
            byte[] bytes = ByteEncoding.Instance.GetBytes(var);
            fd.Write(bytes, 0, bytes.Length);
        }

        private void WriteStrSize(string var, int size)
        {
            byte[] bytes = ByteEncoding.Instance.GetBytes(var);
            fd.Write(bytes, 0, bytes.Length);
            for (int i = 0; i < size - bytes.Length; i++)
                fd.Write((byte)0);
        }

        private void WriteZero(int size)
        {
            for (int i = 0; i < size; i++)
                fd.Write((byte)0);
        }

        private void ChunkSize(int size)
        {
            fd.BaseStream.Seek((long)(-(size - 4)), SeekOrigin.Current);
            fd.Write((uint)size - 8);
            fd.BaseStream.Seek((long)(size - 8), SeekOrigin.Current);
        }


        private void SaveBody()
        {
            int total = 0, size = 0;
            long curpos;
            WriteChunk(File_Chunk_ID.RIFF_ID, SFFile.zero_size); /* write RIFF chunk */
            WriteId(File_Chunk_ID.SFBK_ID);   /* write SFBK ID */

            total = 12;

            /* info chunk */
            curpos = fd.BaseStream.Position;
            WriteChunk(File_Chunk_ID.LIST_ID, SFFile.zero_size);
            WriteId(File_Chunk_ID.INFO_ID);
            size = SaveInfo(size);
            size += 12;
            total += size;
            ChunkSize(size);
            if (SFFile.Verbose) SFFile.Log(SFFile.LogLevel.Info, "INFO_ID writed:{0} size:{1} total:{2}", fd.BaseStream.Position - curpos, size, total);

            ///* sample chunk */
            curpos = fd.BaseStream.Position;
            WriteChunk(File_Chunk_ID.LIST_ID, SFFile.zero_size);
            WriteId(File_Chunk_ID.SDTA_ID);
            size = SaveSDta(size);
            size += 12;
            total += size;
            ChunkSize(size);
            if (SFFile.Verbose) SFFile.Log(SFFile.LogLevel.Info, "SDTA_ID writed:{0} size:{1} total:{2}", fd.BaseStream.Position - curpos, size, total);

            /* pdta monster chunk */
            curpos = fd.BaseStream.Position;
            WriteChunk(File_Chunk_ID.LIST_ID, SFFile.zero_size);
            WriteId(File_Chunk_ID.PDTA_ID);
            size = ProcessPDta(size);
            size += 12;
            total += size;
            ChunkSize(size);
            if (SFFile.Verbose) SFFile.Log(SFFile.LogLevel.Info, "PDTA_ID writed:{0} size:{1} total:{2}", fd.BaseStream.Position - curpos, size, total);

            total -= 8;
            fd.Seek(4, SeekOrigin.Begin);//            safe_fseek(fd, 4L, SEEK_SET);
            fd.Write((uint)total);
            fd.Flush();                /* flush sfont file buffer (fd may be kept open) */
        }

        private int SaveInfo(int size)
        {
            WriteChunk(File_Chunk_ID.IFIL_ID, 4); /* write sf version info chunk */
            fd.Write((ushort)SfData.version.major);
            fd.Write((ushort)SfData.version.minor);

            size = 12;
            foreach (SFInfo info in SfData.info)
            {               /* loop over infos */
                long lastpos = fd.BaseStream.Position;
                //byte[] bytes = Encoding.UTF8.GetBytes(info.Text);
                byte[] bytes = ByteEncoding.Instance.GetBytes(info.Text);
                int x = bytes.Length + 1; /* length of info string + \0 */
                x += x % 2;     /* make length even */
                WriteChunk(info.id, (uint)x);  /* write info chunk */
                fd.Write(bytes, 0, bytes.Length);
                if (x > bytes.Length)
                    WriteZero(x - bytes.Length);
                size += x + 8;     /* size of info string + chunk */

                if (SFFile.Verbose)
                {
                    if (SFFile.Verbose) SFFile.Log(SFFile.LogLevel.Info, "   INFO {0} '{1}' Conv. string to bytes: {2} --> {3} size:{4} file pos advance:{5}",
                        info.id, info.Text, info.Text.Length, bytes.Length, x + 8, fd.BaseStream.Position - lastpos);
                }
            }
            return size;
        }

        private int SaveSDta(int size)
        {
            int newsampos;

            size = 0;

            /* write sample chunk */
            WriteChunk(File_Chunk_ID.SMPL_ID, SFFile.zero_size);
            newsampos = (int)fd.BaseStream.Position;  /* get new sample file position */
            size += 8;         /* for sample chunk id (top of function) */
            ChunkSize(size);   /* update chunk size field */

            SfData.samplepos = (uint)newsampos;  /* assign new sample data file position */
            return size;
        }

        private int ProcessPDta(int size)
        {
            int x = 0;
            long curpos;
            size = 0;

            curpos = fd.BaseStream.Position;
            WriteChunk(File_Chunk_ID.PHDR_ID, SFFile.zero_size);
            x = SavePHdr(x);
            size = (x += 8);
            ChunkSize(x);
            if (SFFile.Verbose) SFFile.Log(SFFile.LogLevel.Info, "PHDR_ID writed:{0} size:{1} total:{2}", fd.BaseStream.Position - curpos, x, size);

            curpos = fd.BaseStream.Position;
            WriteChunk(File_Chunk_ID.PBAG_ID, SFFile.zero_size);
            x = SavePBAG(x);
            size += (x += 8);
            ChunkSize(x);
            if (SFFile.Verbose) SFFile.Log(SFFile.LogLevel.Info, "PBAG_ID writed:{0} size:{1} total:{2}", fd.BaseStream.Position - curpos, x, size);

            curpos = fd.BaseStream.Position;
            WriteChunk(File_Chunk_ID.PMOD_ID, SFFile.zero_size);
            x = SavePMod(x);
            size += (x += 8);
            ChunkSize(x);
            if (SFFile.Verbose) SFFile.Log(SFFile.LogLevel.Info, "PMOD_ID writed:{0} size:{1} total:{2}", fd.BaseStream.Position - curpos, x, size);

            curpos = fd.BaseStream.Position;
            WriteChunk(File_Chunk_ID.PGEN_ID, SFFile.zero_size);
            x = SavePGen(x);
            size += (x += 8);
            ChunkSize(x);
            if (SFFile.Verbose) SFFile.Log(SFFile.LogLevel.Info, "PGEN_ID writed:{0} size:{1} total:{2}", fd.BaseStream.Position - curpos, x, size);

            curpos = fd.BaseStream.Position;
            WriteChunk(File_Chunk_ID.IHDR_ID, SFFile.zero_size);
            x = SaveIHdr(x);
            size += (x += 8);
            ChunkSize(x);
            if (SFFile.Verbose) SFFile.Log(SFFile.LogLevel.Info, "IHDR_ID writed:{0} size:{1} total:{2}", fd.BaseStream.Position - curpos, x, size);

            curpos = fd.BaseStream.Position;
            WriteChunk(File_Chunk_ID.IBAG_ID, SFFile.zero_size);
            x = SaveIBag(x);
            size += (x += 8);
            ChunkSize(x);
            if (SFFile.Verbose) SFFile.Log(SFFile.LogLevel.Info, "IBAG_ID writed:{0} size:{1} total:{2}", fd.BaseStream.Position - curpos, x, size);

            curpos = fd.BaseStream.Position;
            WriteChunk(File_Chunk_ID.IMOD_ID, SFFile.zero_size);
            x = SaveIMod(x);
            size += (x += 8);
            ChunkSize(x);
            if (SFFile.Verbose) SFFile.Log(SFFile.LogLevel.Info, "IMOD_ID writed:{0} size:{1} total:{2}", fd.BaseStream.Position - curpos, x, size);

            curpos = fd.BaseStream.Position;
            WriteChunk(File_Chunk_ID.IGEN_ID, SFFile.zero_size);
            x = SaveIGen(x);
            size += (x += 8);
            ChunkSize(x);
            if (SFFile.Verbose) SFFile.Log(SFFile.LogLevel.Info, "IGEN_ID writed:{0} size:{1} total:{2}", fd.BaseStream.Position - curpos, x, size);

            curpos = fd.BaseStream.Position;
            WriteChunk(File_Chunk_ID.SHDR_ID, SFFile.zero_size);
            x = SaveSHdr(x);
            size += (x += 8);
            ChunkSize(x);
            if (SFFile.Verbose) SFFile.Log(SFFile.LogLevel.Info, "SHDR_ID writed:{0} size:{1} total:{2}", fd.BaseStream.Position - curpos, x, size);

            return size;
        }

        /* zero non-used characters in a Preset, Instrument or Sample name string */
        private void zero_namestr(string name)
        {
            int i = name.Length;
            if (i < 21)
                name += new string('\0', 21 - i);
        }

        /* preset header writer */
        private int SavePHdr(int size)
        {
            ushort ndx = 0;
            //byte[] eop = Encoding.ASCII.GetBytes("EOP");

            size = SFFile.SFPHDRSIZE;     /* for the terminal record */
            foreach (HiPreset pr in SfData.preset)
            {
                if (pr != null)
                {
                    //zero_namestr(pr.name);
                    WriteStrSize(pr.Name, 20);
                    fd.Write((ushort)pr.Num);
                    fd.Write((ushort)pr.Bank);
                    fd.Write(ndx);
                    fd.Write((uint)pr.Libr);
                    fd.Write((uint)pr.Genre);
                    fd.Write((uint)pr.Morph);

                    ndx += (ushort)pr.Zone.Length;
                    size += SFFile.SFPHDRSIZE;
                }
            }

            WriteStrSize("EOP", 4); //safe_fwrite(eop, 4);
            WriteZero(20);
            fd.Write(ndx);
            WriteZero(12);
            return size;
        }

        /* preset bag writer */
        private int SavePBAG(int size)
        {
            ushort genndx = 0, modndx = 0;

            size = SFFile.SFBAGSIZE;      /* for terminal record */
            foreach (HiPreset p in SfData.preset)
            {
                if (p != null)
                {
                    /* loop through presets */
                    // p2 = ((SFPreset*)(p.data)).zone;
                    foreach (HiZone z in p.Zone)
                    {           /* loop through zones */
                        fd.Write(genndx);
                        fd.Write(modndx);

                        if (z.gens != null)
                            genndx += (ushort)z.gens.Length;
                        //     if (z.instsamp > 0) genndx++;

                        if (z.mods != null)
                            modndx += (ushort)z.mods.Length;

                        size += SFFile.SFBAGSIZE;
                    }
                }
            }

            fd.Write(genndx);     /* terminal record */
            fd.Write(modndx);
            return size;
        }

        /* preset modulator writer */
        private int SavePMod(int size)
        {
            size = SFFile.SFMODSIZE;      /* for terminal record */
            foreach (HiPreset p in SfData.preset)
            {
                if (p != null)
                {
                    foreach (HiZone z in p.Zone)
                    {
                        /* traverse this preset's zones */
                        if (z.mods != null)
                            foreach (HiMod m in z.mods)
                            {
                                /* load zone's modulators */
                                fd.Write((ushort)m.SfSrc);
                                fd.Write((ushort)m.Dest);
                                fd.Write((short)m.Amount);
                                fd.Write((ushort)m.SfAmtSrc);
                                fd.Write((ushort)m.SfTrans);
                                size += SFFile.SFMODSIZE;
                            }
                    }
                }
            }
            WriteZero(SFFile.SFMODSIZE);
            return size;
        }

        /* preset generator writer */
        private int SavePGen(int size)
        {
            uint dummy;

            size = SFFile.SFGENSIZE;      /* for terminal record */
            foreach (HiPreset p in SfData.preset)
            {
                if (p != null)
                {
                    foreach (HiZone z in p.Zone)
                    {
                        if (z.gens != null)
                        {
                            /* traverse this preset's zones */
                            //SFFile.Log(SFFile.LogLevel.FLUID_INFO, "Preset:{0} gen count {1}", p.name, z.gen.Length);
                            foreach (HiGen g in z.gens)
                            {
                                //if (g != null)
                                {
                                    /* traverse zone's generators */
                                    fd.Write((ushort)g.type);
                                    if (g.type == fluid_gen_type.GEN_KEYRANGE || g.type == fluid_gen_type.GEN_VELRANGE)
                                    {
                                        fd.Write((sbyte)g.Amount.Lo);
                                        fd.Write((sbyte)g.Amount.Hi);
                                    }
                                    else
                                        fd.Write((ushort)g.Amount.Sword);
                                    size += SFFile.SFGENSIZE;
                                }
                            }
                        }
                        //if (z.instsamp > 0)
                        //{
                        //    fd.Write((ushort)fluid_gen_type.Gen_Instrument);
                        //    fd.Write((ushort)z.instsamp);  //(guint16)dummy = g_slist_position(sf.inst, z.instsamp);
                        //    size += SFFile.SFGENSIZE;
                        //}
                    }
                }
            }

            dummy = 0;
            fd.Write(dummy);      /* terminal record */

            return size;
        }

        /* instrument header writer */
        private int SaveIHdr(int size)
        {
            ushort ndx = 0;

            size = SFFile.SFIHDRSIZE;     /* for terminal record */
            foreach (HiInstrument inst in SfData.inst)
            {
                /* Traverse instruments */
                zero_namestr(inst.Name);
                WriteStrSize(inst.Name, 20);
                fd.Write(ndx);

                if (inst.Zone != null)
                    ndx += (ushort)inst.Zone.Length;
                size += SFFile.SFIHDRSIZE;
            }

            WriteStrSize("EOI", 4); //gchar eoi[] = "EOI"; safe_fwrite(eoi, 4);
            WriteZero(16);
            fd.Write(ndx);
            return size;
        }

        /* instrument bag writer */
        private int SaveIBag(int size)
        {
            ushort genndx = 0, modndx = 0;

            size = SFFile.SFBAGSIZE;
            foreach (HiInstrument p in SfData.inst)
            {
                /* loop through instruments */
                foreach (HiZone z in p.Zone)
                {           /* loop through zones */
                    fd.Write(genndx);
                    fd.Write(modndx);

                    if (z.gens != null)
                        genndx += (ushort)z.gens.Length;
                    //if (z.instsamp > 0) genndx++;

                    if (z.mods != null)
                        modndx += (ushort)z.mods.Length;

                    size += SFFile.SFBAGSIZE;
                }
            }

            fd.Write(genndx);     /* terminal record */
            fd.Write(modndx);

            return size;
        }

        /* instrument modulator writer */
        private int SaveIMod(int size)
        {
            size = SFFile.SFMODSIZE;      /* for terminal record */
            foreach (HiInstrument p in SfData.inst)
            {
                /* loop through instruments */
                foreach (HiZone z in p.Zone)
                {           /* loop through zones */
                    /* traverse this instrument's zones */
                    if (z.mods != null)
                        foreach (HiMod m in z.mods)
                        {
                            /* load zone's modulators */
                            fd.Write((ushort)m.SfSrc);
                            fd.Write((ushort)m.Dest);
                            fd.Write((short)m.Amount);
                            fd.Write((ushort)m.SfAmtSrc);
                            fd.Write((ushort)m.SfTrans);

                            size += SFFile.SFMODSIZE;
                        }
                }
            }

            WriteZero(SFFile.SFMODSIZE);

            return size;
        }


        /* instrument generator writer */
        private int SaveIGen(int size)
        {
            uint dummy;

            size = SFFile.SFGENSIZE;      /* for terminal record */
            foreach (HiInstrument p in SfData.inst)
            {
                /* loop through instruments */
                foreach (HiZone z in p.Zone)
                {           /* loop through zones */
                    /* traverse this instrument's zones */
                    if (z.gens != null)
                    {
                        foreach (HiGen g in z.gens)
                        {
                            /* traverse zone's generators */
                            fd.Write((ushort)g.type);
                            if (g.Amount != null)
                            {
                                if (g.type == fluid_gen_type.GEN_KEYRANGE || g.type == fluid_gen_type.GEN_VELRANGE)
                                {
                                    fd.Write((sbyte)g.Amount.Lo);
                                    fd.Write((sbyte)g.Amount.Hi);
                                }
                                else
                                    fd.Write((ushort)g.Amount.Sword);

                            }
                            else
                            {
                                fd.Write((ushort)0);
                                //SFFile.Log(SFFile.LogLevel.Warn, "Generator without amount. Preset:{0} Gen:{1}", p.name, g.id);
                            }
                            size += SFFile.SFGENSIZE;
                        }
                    }
                    //if (z.instsamp > 0)
                    //{
                    //    fd.Write((ushort)fluid_gen_type.Gen_SampleId);
                    //    fd.Write((ushort)z.instsamp); // (guint16)dummy = g_slist_position(sf.sample, z.instsamp);
                    //    size += SFFile.SFGENSIZE;
                    //}
                }
            }

            dummy = 0;
            fd.Write(dummy);      /* terminal record */

            return size;
        }

        /* sample header writer */
        private int SaveSHdr(int size)
        {
            ushort dumzero = 0;
            uint dummy;      /* WRITE macros must have variable */

            size = SFFile.SFSHDRSIZE;     /* terminal record */

            foreach (HiSample s in SfData.Samples)
            {
                zero_namestr(s.Name);
                WriteStrSize(s.Name, 20);
                fd.Write((uint)s.Start);
                /*
                  sample end, loopstart and loopend are offsets, SHDR uses absolute values,
                  so add start. Sample end adds an additonal 1 because SHDR end should point to
                  first point after sample, whereas we have it point to last sample
                */
                dummy = s.End + s.Start + 1;
                fd.Write((uint)dummy);
                dummy = s.LoopStart + s.Start;
                fd.Write((uint)dummy);
                dummy = s.LoopEnd + s.Start;
                fd.Write((uint)dummy);
                fd.Write((uint)s.SampleRate);
                fd.Write((sbyte)s.OrigPitch);
                fd.Write((sbyte)s.PitchAdj);
                fd.Write(dumzero);    /* put 0 in sample link */
                fd.Write((ushort)s.SampleType);

                size += SFFile.SFSHDRSIZE;
            }

            WriteStrSize("EOS", 4);     //gchar eos[] = "EOS"; safe_fwrite(eos, 4);
            WriteZero(SFFile.SFSHDRSIZE - 4);

            return size;
        }

    }
}
