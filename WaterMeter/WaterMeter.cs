using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using Caliban.Transport;
using Caliban.Utility;
using Caliban.Windows;
using CLIGL;
using Message = Caliban.Transport.Message;

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
            ConfigureWindow();

            r = new Random(Guid.NewGuid().GetHashCode());


            Thread t = new Thread(UpdateThread);
            t.Start();
        }

        private void ConfigureWindow()
        {
            IntPtr hwnd = Process.GetCurrentProcess().MainWindowHandle;
            int SWidth = Screen.PrimaryScreen.Bounds.Width;

            window = new RenderingWindow(TITLE, Width, Height);
            buffer = new RenderingBuffer(Width, Height);

            var style = Windows.GetWindowLong(hwnd, Windows.GWL_STYLE);
            Windows.SetWindowLong(hwnd, Windows.GWL_STYLE, (style & ~Windows.WS_CAPTION));

            Windows.SetWindowPos(hwnd, IntPtr.Zero, 0, -10, 0, 0, Windows.SWP.NOSIZE);
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
                string waterLevelString = Math.Ceiling((_waterLevel)).ToString();
                buffer.ClearPixelBuffer(RenderingPixel.EmptyPixel);
                buffer.SetRectangle(0, 0, Width, Height,
                    new RenderingPixel(
                        '.',
                        ConsoleColor.Blue,
                        ConsoleColor.DarkBlue));
                buffer.SetString(0, 0, waterLevelString, ConsoleColor.White, ConsoleColor.Black);
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