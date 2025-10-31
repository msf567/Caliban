using System;
using System.Net.Sockets;
using Caliban.Core.Transport;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

namespace Caliban.Unity
{
    public class CalibanClient : ClientApp
    {
        public bool Ready = false;

        public CalibanClient() : base("Unity")
        {
        }

        protected override void ClientOnConnected(Socket _socket)
        {
            base.ClientOnConnected(_socket);
            Ready = true;
        }

        protected override void ClientOnDisconncted(Socket _socket)
        {
            base.ClientOnDisconncted(_socket);
            Application.Quit();
        }

        protected override void ClientOnMessageReceived(byte[] _message)
        {
            Message m = Messages.Parse(_message);
            DCon.Log("Received message from host: " + m.Type + " | " + m.Value);
            switch (m.Type)
            {
                case MessageType.APP_CLOSE:
                    DCon.Log("Closing App");
                    Application.Quit();
                    break;
                case MessageType.GAME_CLOSE:
                    var asyncOp =  SceneManager.UnloadSceneAsync("Caliban");
                    if (asyncOp != null)
                    {
                        asyncOp.completed += delegate (AsyncOperation op)
                        {
                        };
                    }
                    break;
                case MessageType.GAME_START:
                    SceneManager.LoadSceneAsync("Caliban",LoadSceneMode.Additive);
                    break;
                case MessageType.SANDSTORM_START:
                    Sandstorm.StartFlag = true;
                    break;
                case MessageType.HOOKS_L_CLICK:
                    Sandstorm.GlobalMouseDown();
                    break;
                case MessageType.CHOREO:
                    string trigger = m.Value;
                    if (trigger == "StartIntro")
                    {
                        GameObject.Find("IntroTimeline").GetComponent<PlayableDirector>().Play();
                    }
                    break;
                
            }
        }

        public void SendMessageToHost(MessageType _type,string _message)
        {
            DCon.Log("Sending message to host: " + _type + " | " + _message);
            base.SendMessageToHost(Messages.Build(_type,_message));
        }
    }
}