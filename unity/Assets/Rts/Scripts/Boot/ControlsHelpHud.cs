using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Boot
{
    public sealed class ControlsHelpHud : MonoBehaviour
    {
        public GameBootController controller;
        public bool visible;
        public Rect area = new Rect(40f, 40f, 560f, 430f);

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
            GUILayout.Label("Left click selects units and buildings.");
            GUILayout.Label("Right click moves selected units or attacks an enemy under the cursor.");
            GUILayout.Label("Use the right sidebar to build economy, infantry, vehicles, and defenses.");
            GUILayout.Label("Build a refinery near ore, produce combat units, then destroy the enemy base.");
            GUILayout.Space(8f);
            GUILayout.Label("Space: pause or resume");
            GUILayout.Label("Period or N: single-step while paused");
            GUILayout.Label("M / A / F: move, attack, force-attack command modes");
            GUILayout.Label("B: enter placement for a completed building");
            GUILayout.Label("Escape: cancel placement or clear selection");
            GUILayout.Label("O: toggle objective HUD");
            GUILayout.Label("F1 or H: toggle in-match controls overlay");
            GUILayout.Label("Backquote and function keys: developer debug panels");
            GUILayout.Space(10f);
            if (GUILayout.Button("Back", GUILayout.Height(32f)) && controller != null)
                controller.HideControls();
            GUILayout.EndArea();
        }
    }
}
