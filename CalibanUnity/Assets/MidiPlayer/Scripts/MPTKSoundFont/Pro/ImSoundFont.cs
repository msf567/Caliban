using MidiPlayerTK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Events;

namespace MidiPlayerTK
{
    /// <summary>
    /// SoundFont adapted to Unity
    /// </summary>
    public partial class ImSoundFont
    {
        /// <summary>
        /// Save an ImSoundFont 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        public void Save(string path, string name, bool onlyXML)
        {
            try
            {
                if (!onlyXML)
                {
                    // Save SF binary data 
                    new SFSave(path + "/" + name + MidiPlayerGlobal.ExtensionSoundFileFileData, HiSf);
                }

                // Build bank selected
                StrBankSelected = "";
                for (int b = 0; b < BankSelected.Length; b++)
                    if (BankSelected[b])
                        StrBankSelected += b + ",";

                var serializer = new XmlSerializer(typeof(ImSoundFont));
                using (var stream = new FileStream(path + "/" + name + MidiPlayerGlobal.ExtensionSoundFileDot, FileMode.Create))
                {
                    serializer.Serialize(stream, this);
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }
    }
}
