using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Boot
{
    public sealed class MainMenuHud : MonoBehaviour
    {
        public GameBootController controller;
        public bool visible = true;
        public Rect area = new Rect(40f, 40f, 360f, 248f);

        void Awake()
        {
            if (controller == null)
                controller = FindAnyObjectByType<GameBootController>();
        }

        public void SetVisible(bool value)
        {
            visible = value;
        }

        void OnGUI()
        {
            if (!visible)
                return;

            GUILayout.BeginArea(area, GUI.skin.box);
            GUILayout.Label("Project Aegis RTS");
            GUILayout.Space(10f);
            if (GUILayout.Button("Start Vertical Slice", GUILayout.Height(36f)) && controller != null)
                controller.StartVerticalSlice();
            if (GUILayout.Button("Controls", GUILayout.Height(32f)) && controller != null)
                controller.ShowControls();
            if (GUILayout.Button("Quit", GUILayout.Height(32f)) && controller != null)
                controller.Quit();
            GUILayout.Space(8f);
            GUILayout.Label("Stage 16.5 player build flow");
            GUILayout.EndArea();
        }
    }
}
