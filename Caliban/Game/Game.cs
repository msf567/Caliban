using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using Caliban.Core.Audio;
using Caliban.Core.Transport;
using Caliban.Core.Utility;
using Caliban.Core.World;

namespace Caliban.Core.Game
{
    public enum GameState
    {
        WON,
        LOST,
        CHEATED,
        IN_PROGRESS,
        NOT_STARTED
    }

    public class Game
    {
        private readonly ServerTerminal server;
        private Thread updateLoop;
        private WaterManager waterManager;
        private Desert desert;

        public static Game CurrentGame = new Game(false);
        public GameState state = GameState.NOT_STARTED;

        public delegate void GameStateChange(GameState state);

        public static GameStateChange OnGameStateChange;
        private static bool closeFlag;

        public Game(bool _debug)
        {
            server = new ServerTerminal();
            server.MessageReceived += ServerOnMessageReceived;
            server.StartListen(5678);

            if (_debug)
                D.Init(server);
            SetState(GameState.NOT_STARTED);
        }


        public void Start()
        {
            SetState(GameState.IN_PROGRESS);
            waterManager = new WaterManager(server);
            desert = new Desert(server);
            updateLoop = new Thread(Update);
            updateLoop.SetApartmentState(ApartmentState.STA);
            updateLoop.Start();
            OpenExplorer();
        }

        private void OpenExplorer()
        {
            Process.Start(DesertParameters.DesertRoot.FullName);
        }

        private void CloseExplorers()
        {
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                OS.Windows.EnumWindowsDelegate childProc = OS.Windows.CloseExplorerWindowsCallback;
                OS.Windows.EnumWindows(childProc, IntPtr.Zero);
            }).Start();
        }

        private void Update()
        {
            closeFlag = false;
            while (!closeFlag)
            {
                desert?.Update();
                waterManager.Update();
                Thread.Sleep(50);
            }
        }

        private void Win()
        {
            SetState(GameState.WON);
        }

        private void Lose()
        {
            SetState(GameState.LOST);
        }

        public void CheatFlag()
        {
            SetState(GameState.CHEATED);
        }

        public void Close()
        {
            server.BroadcastMessage(Messages.Build(MessageType.GAME_CLOSE, ""));
            D.Write("Closing Game");
            Thread.Sleep(500);
            SetState(GameState.NOT_STARTED);
            closeFlag = true;
            waterManager?.Dispose();
            server.Close();
            desert?.Dispose();
            CloseExplorers();
            D.Write("Closed Game");
        }

        private void ServerOnMessageReceived(Socket _socket, byte[] _message)
        {
            var msg = Messages.Parse(_message);
            switch (msg.Type)
            {
                case MessageType.GAME_CLOSE:
                    break;
                case MessageType.GAME_WIN:
                    Win();
                    break;
                case MessageType.GAME_LOSE:
                    Lose();
                    break;
                default:
                    break;
            }
        }

        private void SetState(GameState _state)
        {
            state = _state;
            OnGameStateChange?.Invoke(state);
        }
    }
}