using System.Diagnostics;
using System.Threading;
using Caliban.Transport;
using Caliban.Windows;

namespace Caliban.Game
{
    public class WaterLevel
    {
        private Thread updateThread;
        private ServerTerminal server;
        private bool closeFlag;

        public WaterLevel(ServerTerminal s)
        {
            server = s;
            CurrentLevel = 100;
            GlobalInput.OnGlobalMouseMove += OnGlobalMouseMove;
            GlobalInput.OnGlobalKeyPress += OnGlobalKeyPress;
            updateThread = new Thread(UpdateThread);
            updateThread.Start();
            Process.Start("WaterMeter.exe");
        }

        private void UpdateThread()
        {
            while (!closeFlag)
            {
                server.SendMessageToClient("WaterMeter",
                    Messages.Build(MessageType.WATERLEVEL_SET, CurrentLevel.ToString()));
                Thread.Sleep(300);
            }
        }

        public float CurrentLevel { get; set; }


        private void OnGlobalMouseMove(MouseArgs e)
        {
            CurrentLevel -= 0.01f;
        }

        private void OnGlobalKeyPress(string key)
        {
            //CurrentLevel -= 0.2f;
        }

        public void Dispose()
        {
            closeFlag = true;
        }
    }
}