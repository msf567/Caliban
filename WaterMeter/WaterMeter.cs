using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using CalibanLib.ConsoleOutput;
using CalibanLib.Transport;
using CalibanLib.Utility;
using CalibanLib.Windows;
using CLIGL;
using Message = CalibanLib.Transport.Message;

namespace WaterMeter
{
    public class WaterMeter : ClientApp
    {
        public const string TITLE = "Water Meter";
        private static Random r;

        private float _waterLevel;
        private bool _initialized;
        private bool closeFlag = false;
        private RenderingWindow window;
        private RenderingBuffer buffer;
        public readonly int Width;
        public readonly int Height;

        public WaterMeter(int w, int h) : base("WaterMeter")
        {
            Width = w;
            Height = h;

            r = new Random(Guid.NewGuid().GetHashCode());
            window = new RenderingWindow(TITLE, Width, Height);

            buffer = new RenderingBuffer(Width, Height);
            Thread t = new Thread(UpdateThread);
            t.Start();
            Console.SetWindowPosition(0, 0);
        }

        private void UpdateThread()
        {
            while (!closeFlag)
            {
                RenderWaterLevel();
                Thread.Sleep(100);
            }
        }

        private void RenderWaterLevel()
        {
            if (_initialized)
            {
                string waterLevelString =Math.Ceiling((_waterLevel)).ToString();
                buffer.ClearPixelBuffer(RenderingPixel.EmptyPixel);
                buffer.SetRectangle(0, 0, Width, Height,
                    new RenderingPixel(
                        '.',
                        ConsoleColor.Blue,
                        ConsoleColor.DarkBlue));
                buffer.SetString(0,0,waterLevelString,ConsoleColor.White,ConsoleColor.Black);
            }
            else
                buffer.SetRectangle(0, 0, Width, Height,
                    new RenderingPixel(
                        ' ',
                        ConsoleColor.Black,
                        ConsoleColor.Black));

            window.Render(buffer);
        }

        #region networking

        protected override void ClientOnConnected(Socket socket)
        {
            base.ClientOnConnected(socket);
            Thread.Sleep(500);
            SendMessageToHost(Messages.Build(MessageType.WATERLEVEL_GET, ""));
        }

        protected override void ClientOnMessageReceived(byte[] message)
        {
            Message m = Messages.Parse(message);
            switch (m.Type)
            {
                case MessageType.GAME_CLOSE:
                    closeFlag = true;
                    break;
                case MessageType.WATERLEVEL_SET:
                    _initialized = true;
                    _waterLevel = (float) Math.Floor(float.Parse(m.Param)).Clamp(0, 100);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion

    }
}