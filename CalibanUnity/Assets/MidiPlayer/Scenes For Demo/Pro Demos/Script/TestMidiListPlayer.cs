using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MidiPlayerTK;
using System;

namespace MidiPlayerTK
{
    public class TestMidiListPlayer : MonoBehaviour
    {
        /// <summary>
        /// MPTK component able to play a Midi list. This PreFab must be present in your scene.
        /// </summary>
        public MidiListPlayer midiListPlayer;

        private void Start()
        {
            if (!HelperDemo.CheckSFExists()) return;

            // Find the Midi external component 
            if (midiListPlayer == null)
            {
                Debug.Log("No MidiListPlayer defined with the editor inspector, try to find one");
                MidiListPlayer fp = FindObjectOfType<MidiListPlayer>();
                if (fp == null)
                    Debug.Log("Can't find a MidiListPlayer in the Hierarchy. No music will be played");
                else
                {
                    midiListPlayer = fp;
                }
            }
        }

        /// <summary>
        /// This method is fired from button to play the next midi in the list
        /// See canvas/button.
        /// </summary>
        public void Next()
        {
            midiListPlayer.MPTK_Next();
        }
    }
}