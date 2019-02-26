﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Media;
using System.Threading;
using System.Windows.Forms;
using Caliban.Core.ConsoleOutput;
using Caliban.Core.Utility;

namespace Note
{
    public partial class NoteForm : Form
    {
        static bool closeFlag;
        private static int count = 0;
        private string notePath = "";
        private Thread writeThread = null;
        private SoundPlayer sound = new SoundPlayer(Properties.Resources.blip);
        private Random r = new Random(Guid.NewGuid().GetHashCode());

        public NoteForm(string _notePath)
        {
            InitializeComponent();
            notePath = _notePath;
            sound.Load();
        }

        private void NoteForm_Load(object sender, System.EventArgs e)
        {
            noteText.Text = "";
            writeThread = new Thread(WriteText);
            writeThread.Start();
        }

        void WriteText()
        {
            var lines = File.ReadLines(notePath);
            bool titleSet = false;
            bool sizeSet = false;
            foreach (var line in lines)
            {
                if (!titleSet)
                {
                    SetTitle(line);
                    titleSet = true;
                    continue;
                }

                if (!sizeSet)
                {
                    var size = line.Split(' ');
                    SetSize(new Size(int.Parse(size[0]), int.Parse(size[1])));
                    sizeSet = true;
                    continue;
                }

                int i = 0;
                var words = line.Split(' ');
                while (i < words.Length)
                {
                    int time = r.Next(100, 180);
                    for (int x = 0; x < 4; x++)
                    {
                        if (i <= words.Length - 1)
                            AppendText(words[i] + " ");
                        i++;
                    }

                    sound.Play();
                    Thread.Sleep(time);
                }

                Thread.Sleep(600);
                AppendText(Environment.NewLine);
            }

            while (!closeFlag)
            {
                Thread.Sleep(50);
                if (count++ > 1000)
                    closeFlag = true;
            }

            Close();
        }

        private void noteText_TextChanged(object sender, EventArgs e)
        {
            Size size = TextRenderer.MeasureText(noteText.Text, noteText.Font);
            if (size.Width > noteText.Width)
                noteText.Width = size.Width;
            if (size.Height > noteText.Height)
                noteText.Height = size.Height;
        }

        private void NoteForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            closeFlag = true;
            writeThread.Abort();
        }

        delegate void StringArgReturningVoidDelegate(string text);

        delegate void SizeArgReturningVoidDelegate(Size size);

        private void SetTitle(string text)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.InvokeRequired)
            {
                StringArgReturningVoidDelegate d = new StringArgReturningVoidDelegate(SetTitle);
                this.Invoke(d, new object[] {text});
            }
            else
            {
                this.Text = text;
            }
        }

        private void AppendText(string text)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.InvokeRequired)
            {
                StringArgReturningVoidDelegate d = new StringArgReturningVoidDelegate(AppendText);
                this.Invoke(d, new object[] {text});
            }
            else
            {
                this.noteText.AppendText(text);
            }
        }

        private void SetSize(Size size)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.InvokeRequired)
            {
                SizeArgReturningVoidDelegate d = new SizeArgReturningVoidDelegate(SetSize);
                this.Invoke(d, new object[] {size});
            }
            else
            {
                this.Size = size;
            }
        }
    }
}