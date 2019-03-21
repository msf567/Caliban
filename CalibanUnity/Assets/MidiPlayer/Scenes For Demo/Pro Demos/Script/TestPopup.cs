using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MidiPlayerTK
{
    public class TestPopup : MonoBehaviour
    {
        public PopupListItem PopPatch;
        public CustomStyle myStyle;
        public Vector2 scrollerWindow;
        public List<MPTKListItem> listItem;
        [Range(0,1000)]
        public int countItem;
        public int selectedItem;
        public int Width = -1;
        public int Height = -1;
        [Range(0,20)]
        public int ColCount=5;
        public int ColWidth = 200;
        public int ColHeight = 30;
        public int CountRows;

        // Use this for initialization
        void Start()
        {
            listItem = new List<MPTKListItem>();
            PopPatch = new PopupListItem()
            {
                Title = "Select A Midi File",
                OnSelect = NewPopup,
                Tag = "FORTEST",
            };
        }

        private void NewPopup(object tag, int index)
        {
            Debug.Log("NewPopup " + index + " for " + tag);
            selectedItem = index;
            // return true;
        }

        void OnGUI()
        {
            // Set custom Style. Good for background color 3E619800
            if (myStyle == null)
                myStyle = new CustomStyle();
            if (countItem!=listItem.Count)
            {
                listItem = new List<MPTKListItem>();
                for (int i = 0; i < countItem; i++)
                    listItem.Add(new MPTKListItem() { Index = i, Label = i.ToString(), });
            }

            scrollerWindow = GUILayout.BeginScrollView(scrollerWindow, false, false, GUILayout.Width(Screen.width));

            PopPatch.ColCount = ColCount;
            PopPatch.ColHeight = ColHeight;
            PopPatch.ColWidth = ColWidth;

            // Display popup in first to avoid activate other layout behind
            PopPatch.Draw(listItem, selectedItem, myStyle);

            CountRows = PopPatch.CountRow;
            GUILayout.Space(200);
            // Open the popup to select a midi
            if (GUILayout.Button("Test", GUILayout.Width(500), GUILayout.Height(40)))
                PopPatch.Show = !PopPatch.Show;
            PopPatch.Position(ref scrollerWindow);

            GUILayout.EndScrollView();
        }
        // Update is called once per frame
        void Update()
        {

        }
    }
}