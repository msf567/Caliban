using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;
using NAudio.Midi;
using System;
using System.IO;
using MidiPlayerTK;

namespace MidiPlayerTK
{
    public class TPatchUsed
    {
        public TBankUsed[] BankUsed;
        public int DefaultBankNumber = -1;
        public int DrumKitBankNumber = -1;

        public TPatchUsed()
        {
            BankUsed = new TBankUsed[130];
        }
    }

    public class TBankUsed
    {
        public TNoteUsed[] PatchUsed;
        public TBankUsed()
        {
            PatchUsed = new TNoteUsed[128];
        }
    }

    public class TNoteUsed
    {
        public int[] Note;
        public TNoteUsed()
        {
            Note = new int[128];
        }
    }

    /// <summary>
    /// Scan midifiles and returns patchs used
    /// </summary>
    public class MidiOptim
    {
        /// <summary>
        /// Scan midifiles and returns patchs used
        /// </summary>
        /// <param name="Info"></param>
        /// <returns></returns>
        static public TPatchUsed PatchUsed(BuilderInfo Info)
        {
            TPatchUsed filters = new TPatchUsed();
            try
            {
                filters.DefaultBankNumber = MidiPlayerGlobal.ImSFCurrent.DefaultBankNumber;
                filters.DrumKitBankNumber = MidiPlayerGlobal.ImSFCurrent.DrumKitBankNumber;

                if (MidiPlayerGlobal.CurrentMidiSet.MidiFiles == null || MidiPlayerGlobal.CurrentMidiSet.MidiFiles.Count == 0)
                {
                    Info.Add("No Midi files defined, can't optimize");
                    filters = null;
                }
                else

                    foreach (string midifilepath in MidiPlayerGlobal.CurrentMidiSet.MidiFiles)
                    {
                        Info.Add("   Scan " + midifilepath);

                        int[] currentPatch = new int[16];
                        MidiLoad midifile = new MidiLoad();
                        midifile.KeepNoteOff = false;
                        midifile.MPTK_Load(midifilepath);
                        if (midifile != null)
                        {
                            foreach (TrackMidiEvent trackEvent in midifile.MidiSorted)
                            {

                                if (trackEvent.Event.CommandCode == MidiCommandCode.NoteOn)
                                {
                                    if (((NoteOnEvent)trackEvent.Event).OffEvent != null)
                                    {
                                        //infoTrackMidi[e.Channel].Events.Add((NoteOnEvent)e);
                                        NoteOnEvent noteon = (NoteOnEvent)trackEvent.Event;
                                        if (noteon.OffEvent != null)
                                        {
                                            int banknumber = trackEvent.Event.Channel == 10 ? filters.DrumKitBankNumber : filters.DefaultBankNumber;
                                            int patchnumber = currentPatch[trackEvent.Event.Channel - 1];
                                            if (banknumber >= 0)
                                            {
                                                if (filters.BankUsed[banknumber] == null)
                                                    filters.BankUsed[banknumber] = new TBankUsed();

                                                if (filters.BankUsed[banknumber].PatchUsed[patchnumber] == null)
                                                    filters.BankUsed[banknumber].PatchUsed[patchnumber] = new TNoteUsed();

                                                filters.BankUsed[banknumber].PatchUsed[patchnumber].Note[noteon.NoteNumber]++;
                                            }
                                        }
                                    }
                                }
                                else if (trackEvent.Event.CommandCode == MidiCommandCode.PatchChange)
                                {
                                    PatchChangeEvent change = (PatchChangeEvent)trackEvent.Event;
                                    // Always use patch 0 for drum kit
                                    currentPatch[trackEvent.Event.Channel - 1] = trackEvent.Event.Channel == 10 ? 0 : change.Patch;
                                }
                            }
                        }
                        else
                        {
                        }
                    }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
            return filters;
        }
    }
}

