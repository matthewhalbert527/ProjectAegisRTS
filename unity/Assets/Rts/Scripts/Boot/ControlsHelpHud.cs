using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Boot
{
    public sealed class ControlsHelpHud : MonoBehaviour
    {
        public GameBootController controller;
        public bool visible;
        public Rect area = new Rect(40f, 40f, 460f, 330f);

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
            GUILayout.Label("Controls");
            GUILayout.Label("Left click: select units and buildings");
            GUILayout.Label("Right click: issue contextual movement or tactical command");
            GUILayout.Label("Space: pause/resume");
            GUILayout.Label("Period or N: single-step while paused");
            GUILayout.Label("M/A/F: move, attack, force-attack command modes");
            GUILayout.Label("B: enter placement for a completed building");
            GUILayout.Label("Escape: cancel placement or clear selection");
            GUILayout.Label("O: toggle objective HUD");
            GUILayout.Label("Backquote and function keys: developer debug panels");
            GUILayout.Space(10f);
            if (GUILayout.Button("Back", GUILayout.Height(32f)) && controller != null)
                controller.HideControls();
            GUILayout.EndArea();
        }
    }
}
