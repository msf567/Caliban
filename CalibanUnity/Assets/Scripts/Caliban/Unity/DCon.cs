using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Caliban.Unity
{
    public class DCon : MonoBehaviour
    {
        private void Awake()
        {
            textBox = GetComponent<Text>();
        }

        private static Text textBox;

        public static void Log(string m)
        {
           // textBox.text += Environment.NewLine + m;
        }
    }
}