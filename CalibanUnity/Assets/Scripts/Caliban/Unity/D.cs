using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Caliban.Unity
{
    public class D : MonoBehaviour
    {
        private static Text text;
        private static string buffer = "";
        private void Awake()
        {
            string[] args = Environment.GetCommandLineArgs ();
            if(!args.Contains("debug"))
                Destroy(gameObject);
            
            text = transform.Find("Text").GetComponent<Text>();
        }
    
        public static void Write(string s)
        {
            string newLine = s + Environment.NewLine;
            buffer += newLine;
            text.text += newLine;
        }

        private void OnApplicationQuit()
        {
            File.WriteAllText("UnityLog.txt", buffer);
        }
    }
}
