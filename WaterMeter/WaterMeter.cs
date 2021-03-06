using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using Caliban.Core.OS;
using Caliban.Core.Transport;
using Caliban.Core.Utility;
using CLIGL;
using Message = Caliban.Core.Transport.Message;

namespace WaterMeter
{
    public class WaterMeter : ClientApp
    {
        public const string TITLE = "Water Meter";
        private static Random r;

        private float waterLevel;
        private bool initialized;
        private bool closeFlag = false;
        private RenderingWindow window;
        private RenderingBuffer buffer;
        public readonly int Width;
        public readonly int Height;

        public WaterMeter(int _w, int _h) : base("WaterMeter")
        {
            Width = _w;
            Height = _h;
            ConfigureWindow();

            r = new Random(Guid.NewGuid().GetHashCode());

            Thread t = new Thread(UpdateThread);
            t.Start();
        }

        private void ConfigureWindow()
        {
            IntPtr hwnd = Process.GetCurrentProcess().MainWindowHandle;
            int sWidth = Screen.PrimaryScreen.Bounds.Width;

            window = new RenderingWindow(TITLE, Width, Height);
            buffer = new RenderingBuffer(Width, Height);

            var style = Windows.GetWindowLong(hwnd, Windows.GWL_STYLE);
            Windows.SetWindowLong(hwnd, Windows.GWL_STYLE, (style & ~Windows.WS_CAPTION));

            Windows.SetWindowPos(hwnd, IntPtr.Zero, 0, -10, 0, 0, Windows.Swp.NOSIZE);
        }

        private void UpdateThread()
        {
            while (!closeFlag)
            {
                if (!IsReady)
                    SetClientReady();
                RenderWaterLevel();
                
                  
                Thread.Sleep(100);
                if (!IsConnected)
                    closeFlag = true;
            }
        }

        private void RenderWaterLevel()
        {
            int waterHeight = (int) Math.Floor((waterLevel / 100.0f) * Height);
            if (initialized)
            {
                string waterLevelString = Math.Ceiling((waterLevel)).ToString();
                buffer.ClearPixelBuffer(RenderingPixel.EmptyPixel);
                buffer.SetRectangle(0, 0, Width, Height,
                    new RenderingPixel(
                        ' ',
                        ConsoleColor.Black,
                        ConsoleColor.Black));
                buffer.SetRectangle(0, Height - waterHeight, Width, waterHeight,
                    new RenderingPixel(
                        '.',
                        ConsoleColor.Blue,
                        ConsoleColor.DarkBlue));
                buffer.SetString(0, 0, waterLevelString, ConsoleColor.White, ConsoleColor.Black);
            }
            else
                buffer.SetRectangle(0, 0, Width, Height,
                    new RenderingPixel(' ', ConsoleColor.Black, ConsoleColor.Black));

            window.Render(buffer);
        }

        #region networking

        protected override void ClientOnConnected(Socket _socket)
        {
            base.ClientOnConnected(_socket);
            Thread.Sleep(500);
            SendMessageToHost(Messages.Build(MessageType.WATERLEVEL_GET, ""));
        }

        protected override void ClientOnMessageReceived(byte[] _message)
        {
            Message m = Messages.Parse(_message);
            switch (m.Type)
            {
                case MessageType.GAME_CLOSE:
                    closeFlag = true;
                    break;
                case MessageType.WATERLEVEL_SET:
                    initialized = true;
                    waterLevel = (float) Math.Floor(float.Parse(m.Value)).Clamp(0, 100);
                    break;
            }
        }

        #endregion
    }
}