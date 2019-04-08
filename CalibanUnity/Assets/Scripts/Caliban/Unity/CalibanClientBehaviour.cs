using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Caliban.Unity
{
	public class CalibanClientBehaviour : MonoBehaviour
	{
		private CalibanClient client;
		void Awake()
		{
			if (Application.isEditor)
				return;
			
			Process[] pname = Process.GetProcessesByName("CALIBAN");
			if (pname.Length == 0)
				Application.Quit();

			client = new CalibanClient();
		}

		private void Update()
		{
			if (Application.isEditor)
				return;
			if(!client.IsConnected)
				Application.Quit();
		}
	}
}
