using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using Caliban.Core.Transport;
using Caliban.Core.Utility;
using Caliban.Core.Windows;
using Caliban.Core.World;
using Caliban.Core.Debug;

namespace Caliban.Core.Game
{
    public enum GameState //TODO turn this into a struct for savestate & cheat reason etc
    {
        WON,
        LOST,
        CHEATED, //TODO implement cheat reason and display it
        IN_PROGRESS,
        NOT_STARTED
    }

    public class Game
    {
        private readonly ServerTerminal server;
        private Thread updateLoop;
        private Player player;
        private World.World world;

        public static Game CurrentGame;
        public GameState State = GameState.NOT_STARTED;

        public delegate void GameStateChange(GameState _state, string cheatReason);

        public static GameStateChange OnGameStateChange;

        public Game(ServerTerminal _server)
        {
            server = _server;
            server.MessageReceived += ServerOnMessageReceived;

            SetState(GameState.NOT_STARTED);

            GlobalInput.OnGlobalMouseAction += OnGlobalMouseAction;
        }

        public void Start()
        {
            server.BroadcastMessage(MessageType.GAME_START, "");
            SetState(GameState.IN_PROGRESS);
            player = new Player(server);
            world = new World.World(server);
            updateLoop = new Thread(Update);
            updateLoop.SetApartmentState(ApartmentState.STA);
            updateLoop.Start();
            OpenExplorer();
        }

        private void OnGlobalMouseAction(MouseArgs _e)
        {
            if (_e.Message == MouseMessages.WM_LBUTTONDOWN)
            {
                server.BroadcastMessage(MessageType.HOOKS_L_CLICK, "");
            }
        }

        private void Update()
        {
            while (State == GameState.IN_PROGRESS)
            {
                world?.Update();
                player?.Update();
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

        public void CheatFlag(string CheatMessage)
        {
            SetState(GameState.CHEATED, CheatMessage);
        }

        public void Close(bool _closeExplorers)
        {
            SetState(GameState.NOT_STARTED);

            server.BroadcastMessage(MessageType.GAME_CLOSE, "");
            Thread.Sleep(1000);

            player?.Dispose();
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
                case MessageType.HOOKS_L_CLICK:
                    D.Write("CLick!");
                    break;
                case MessageType.MAP_REVEAL:
                    server.BroadcastMessage(MessageType.SANDSTORM_START, "");
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
            Process.Start(new ProcessStartInfo
            {
                FileName = WorldParameters.WorldRoot.FullName,
                UseShellExecute = true
            });
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

        private void SetState(GameState _state, string _cheatReason = "")
        {
            State = _state;
            OnGameStateChange?.Invoke(State, _cheatReason);
        }
    }
}