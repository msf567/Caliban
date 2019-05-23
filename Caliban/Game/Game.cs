using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
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
        private World.World world;

        public static Game CurrentGame;
        public GameState State = GameState.NOT_STARTED;

        public delegate void GameStateChange(GameState _state);

        public static GameStateChange OnGameStateChange;

        public Game(ServerTerminal _server)
        {
            
            server = _server;
            server.MessageReceived += ServerOnMessageReceived;
           

            if (D.debugMode)
                D.Init(server);
            SetState(GameState.NOT_STARTED);
        }

        public void Start()
        {
            server.BroadcastMessage(Messages.Build(MessageType.GAME_START,""));
            SetState(GameState.IN_PROGRESS);
            waterManager = new WaterManager(server);
            world = new World.World(server);
            updateLoop = new Thread(Update);
            updateLoop.SetApartmentState(ApartmentState.STA);
            updateLoop.Start();
            OpenExplorer();
            //ModuleLoader.LoadModuleAndWait("CU.exe", "CalibanUnity", D.debugMode ? "debug" : "");
        }

        private void Update()
        {
            while (State == GameState.IN_PROGRESS)
            {
                world?.Update();
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

        public void Close(bool _closeExplorers)
        {
            SetState(GameState.NOT_STARTED);

            server.BroadcastMessage(Messages.Build(MessageType.GAME_CLOSE, ""));
            Thread.Sleep(1000);

            waterManager?.Dispose();
            world?.Dispose();

            ModuleLoader.Clean();
           
            if (_closeExplorers)
                CloseExplorers();
        }

        private void ServerOnMessageReceived(Socket _socket, byte[] _message)
        {
            var msg = Messages.Parse(_message);
            switch (msg.Type)
            {
                case MessageType.MAP_REVEAL:
                    server.BroadcastMessage(Messages.Build(MessageType.SANDSTORM_START, ""));
                    break;
                case MessageType.GAME_CLOSE:
                    break;
                case MessageType.GAME_WIN:
                    Win();
                    break;
                case MessageType.GAME_LOSE:
                    Lose();
                    break;
            }
        }

        private void OpenExplorer()
        {
            Process.Start(WorldParameters.WorldRoot.FullName);
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

        private void SetState(GameState _state)
        {
            State = _state;
            OnGameStateChange?.Invoke(State);
        }
    }
}