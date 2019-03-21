using MidiPlayerTK;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MidiPlayerTK
{
    public class MainMenu : MonoBehaviour
    {
        static string sceneMainMenu = "ScenesDemonstration";
        static private Texture buttonIconHome;
        static MainMenu instance;

        public void Awake()
        {
            instance = this;
        }

        static public void Display(string title, CustomStyle myStyle)
        {
            GUILayout.BeginHorizontal(myStyle.BacgDemos);
            if (buttonIconHome == null)
                buttonIconHome = Resources.Load<Texture2D>("Textures/home");
            if (GUILayout.Button(new GUIContent(buttonIconHome, "Go to main menu"), GUILayout.Width(48), GUILayout.Height(48)))
                instance.GoToMainMenu();
            GUILayout.Space(20);
            GUILayout.Label(title, myStyle.TitleLabel1, GUILayout.Height(48));
            GUILayout.EndHorizontal();
        }

        public void GoToMainMenu()
        {
            int index = SceneUtility.GetBuildIndexByScenePath(sceneMainMenu);
            if (index < 0)
            {
                Debug.LogWarning("To avoid interacting with your project, MPTK doesn't add MPTK scenes in the Build Settings.");
                Debug.LogWarning("Add these scenes with “File/Build Settings” if you want a full functionality of the demonstrator.");
            }
            else
                SceneManager.LoadScene(index, LoadSceneMode.Single);
        }
    }
}