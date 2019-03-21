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
			Process[] pname = Process.GetProcessesByName("CALIBAN");
			if (pname.Length == 0)
				Application.Quit();

			client = new CalibanClient();
			D.Write("Created client!");
		}

		private void Update()
		{
			if(!client.IsConnected && client.Ready)
				Application.Quit();
		}
	}
}
