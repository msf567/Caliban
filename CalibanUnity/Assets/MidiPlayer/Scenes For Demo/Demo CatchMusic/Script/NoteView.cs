﻿using MidiPlayerTK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Demo CatchMusic
/// </summary>
namespace MPTKDemoCatchMusic
{
    
    /// <summary>
    /// Defined behavior of a note
    /// </summary>
    public class NoteView : MonoBehaviour
    {
        public static bool FirstNotePlayed = false;
        public MPTKEvent note;
        public MidiStreamPlayer midiStreamPlayer;
        public bool played = false;
        public Material MatPlayed;
        public float zOriginal;
        // 
        /// <summary>
        /// Update
        /// </summary>
        public void Update()
        {
            //! @code
            //! midiFilePlayer.MPTK_PlayNote(note);
            //! FirstNotePlayed = true;
            //! @endcode
            if (!played && transform.position.x < -45f)
            {
                played = true;
                // If original z is not the same, the value will be changed, too bad for the ears ...
                int delta = Mathf.CeilToInt(zOriginal - transform.position.z + 0.5f);
                //! [Example PlayNote]
                note.Value += delta;
                // Now play the note
                midiStreamPlayer.MPTK_PlayEvent(note);
                //! [Example PlayNote]
                FirstNotePlayed = true;

                gameObject.GetComponent<Renderer>().material = MatPlayed;// .color = Color.red;
            }
            if (transform.position.y < -30f)
            {
                Destroy(this.gameObject);
            }

        }
        void FixedUpdate()
        {
            // Move the note along the X axis
            float translation = Time.fixedDeltaTime * MusicView.Speed;
            transform.Translate(-translation, 0, 0);
        }
    }
}