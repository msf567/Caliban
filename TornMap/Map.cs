﻿using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Caliban.Core.OS;
using Caliban.Core.Transport;
using Caliban.Core.Windows;
using CLIGL;

namespace TornMap
{
    public class Map : ClientApp
    {
        private bool closeFlag;
        private Random r = new Random(Guid.NewGuid().GetHashCode());
        private string baseClueLoc = "";

        private readonly int windowWidth;
        private const int WindowHeight = 1;
        private float decayLevel = 0.98f;
        private const string Title = "░or█upt░d █ap - █ig█t Cl░ck t█ Dec░d█";


        public Map(string _clientName) : base(_clientName)
        {
            GlobalInput.OnGlobalMouseAction += OnGlobalMouseAction;
            if (TornMapProgram._debug)
            {
                Console.WriteLine("Debug Mode!");
                baseClueLoc =
                    @"A:\Caliban\Desert\sand_234f43\dune_234d\ridge_3248jf\dune_234wfe3\sand_32849ur\dune_234wfe3\sand_32849ur";
            }

            while (baseClueLoc == "")
                Thread.Sleep(50);
            Console.Title = "░or█upt░d █ap";

            windowWidth = baseClueLoc.Length;
            var renderingWindow = new RenderingWindow(Title, windowWidth, WindowHeight);
            var renderingBuffer = new RenderingBuffer(windowWidth, WindowHeight);
            Console.OutputEncoding = Encoding.UTF8;

            //Console.WriteLine(GetString(baseClueLoc));
            while (!closeFlag)
            {
                renderingBuffer.ClearPixelBuffer(RenderingPixel.EmptyPixel);

                if (baseClueLoc != "")
                {
                    DrawCorruptedString(renderingBuffer);
                }

                renderingWindow.Render(renderingBuffer);
                Thread.Sleep(50);
            }
        }

        private void OnGlobalMouseAction(MouseArgs _m)
        {
            if (_m.Message == MouseMessages.WM_RBUTTONDOWN)
            {
                Windows.GetWindowRect(Windows.GetConsoleWindow(), out var myRect);

                if (_m.Point.X > myRect.Left && _m.Point.X < myRect.Right && _m.Point.Y > myRect.Top &&
                    _m.Point.Y < myRect.Bottom)
                {
                    decayLevel -= 0.0075f;
                    if (decayLevel < 0.1f)
                        decayLevel = 0.1f;
                }
            }
        }

        private static readonly string _chars = "abcdefghijklmnopqrstuvwxyz0987654321";

        private void DrawCorruptedString(RenderingBuffer _renderingBuffer)
        {
            for (var x = 0; x < baseClueLoc.Length; x++)
            {
                var myChar = r.NextDouble() < 0.5 ? _chars[r.Next(0, _chars.Length)] : baseClueLoc[x];
                _renderingBuffer.SetPixel(x, 0,
                    r.NextDouble() < decayLevel
                        ? new RenderingPixel(' ', ConsoleColor.Black, ConsoleColor.Black)
                        : new RenderingPixel(myChar, ConsoleColor.White, ConsoleColor.Black));
            }
        }

        protected override void ClientOnConnected(Socket _socket)
        {
            base.ClientOnConnected(_socket);
            Thread.Sleep(100);
            SendMessageToHost(Messages.Build(MessageType.MAP_LOCAITON, AppDomain.CurrentDomain.BaseDirectory));
        }

        protected override void ClientOnMessageReceived(byte[] _message)
        {
            base.ClientOnMessageReceived(_message);
            var m = Messages.Parse(_message);
            if (m.Type == MessageType.GAME_CLOSE)
                closeFlag = true;
            if (m.Type == MessageType.MAP_LOCAITON)
                baseClueLoc = m.Value;
        }
    }
}