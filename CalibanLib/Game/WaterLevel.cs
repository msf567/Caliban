using System;
using System.Threading;
using CalibanLib.Transport;
using CalibanLib.Windows;

namespace CalibanLib.Game
{
    public class WaterLevel
    {
        private Thread updateThread;
        private ServerTerminal server;
        private bool closeFlag;

        public WaterLevel()
        {
            GlobalInput.OnGlobalMouseMove += OnGlobalMouseMove;
            GlobalInput.OnGlobalKeyPress += OnGlobalKeyPress;
            CurrentLevel = 100;
        }

        public void Init(ServerTerminal s)
        {
            server = s;
            updateThread = new Thread(UpdateThread);
            updateThread.Start();
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