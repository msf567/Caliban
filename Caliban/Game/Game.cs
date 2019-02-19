using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Windows.Forms;
using Caliban.Core.Desert;
using Caliban.Core.Transport;
using Caliban.Core.Utility;

namespace Caliban.Core.Game
{
    public enum GameState
    {
        WON,
        LOST,
        IN_PROGRESS,
        NOT_STARTED
    }

    public class Game
    {
        private readonly ServerTerminal server;
        private WaterLevel waterLevel;
        private DesertManager desMan;
        public static Game CurrentGame = new Game(false);
        public GameState state = GameState.NOT_STARTED;

        public delegate void GameStateChange(GameState state);

        public static GameStateChange OnGameStateChange;

        public Game(bool _debug)
        {
            server = new ServerTerminal();
            server.MessageReceived += ServerOnMessageReceived;
            server.StartListen(5678);
            if (_debug)
                D.Init(server);
            SetState(GameState.NOT_STARTED);
        }

        private void Win()
        {
           SetState(GameState.WON);
        }

        private void Lose()
        {
         SetState(GameState.LOST);
        }

        public void Start()
        {
            waterLevel = new WaterLevel(server);
            desMan = new DesertManager(server);
           SetState(GameState.IN_PROGRESS);
           DesertGenerator.GenerateDesert();

            Process.Start("Note.exe", "intro.txt");
        }

        public void Close()
        {
            waterLevel?.Dispose();
            server.BroadcastMessage(Messages.Build(MessageType.GAME_CLOSE, ""));
            server.Close();
               DesertGenerator.ClearDesert();
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