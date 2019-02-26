using System;
using System.IO;
using System.Net;
using System.Threading;
using Caliban.Core.ConsoleOutput;
using Caliban.Core.Utility;
using System.Windows.Forms;

namespace Note
{
    internal class NoteProgram
    {

        public static void Main(string[] args)
        {
            foreach(string s in args)
              D.Write("arg:" + s);
         //   if (args.Length == 0)
          //      return;
            Application.EnableVisualStyles();
            Application.Run(new NoteForm(@"A:\\Caliban\\Builds\\Intro.txt"));
          
        }
    }
}