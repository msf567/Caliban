using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

namespace Caliban.Unity
{

    public class CalibanClientBehaviour : MonoBehaviour, INotificationReceiver
	{
		public static CalibanClient client;

        public void OnNotify(Playable origin,INotification notification,object context)
        {
			if (notification is ChoreoMarker)
			{
				string command = (notification as ChoreoMarker).Command;
                client.SendMessageToHost(Core.Transport.MessageType.CHOREO,command);
            }
        }

        void Awake()
		{
			DontDestroyOnLoad(this);

			if (Application.isEditor)
			return;
				
			Process[] pname = Process.GetProcessesByName("CALIBAN");
			if (pname.Length == 0)
				Application.Quit();

			client = new CalibanClient();
		}

		private void Update()
		{
			if(Input.GetKeyDown("z"))
				SceneManager.LoadSceneAsync("Caliban", LoadSceneMode.Additive);
			if(Input.GetKeyDown("x"))
				SceneManager.UnloadSceneAsync("Caliban");
			if (Application.isEditor)
				return;
			if(!client.IsConnected)
				Application.Quit();
			
			Process[] pname = Process.GetProcessesByName("CALIBAN");
			if (pname.Length == 0)
				Application.Quit();
		}
	}
}
