using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

namespace Caliban.Unity
{
	
	public class CalibanClientBehaviour : MonoBehaviour
	{
		private CalibanClient client;
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
