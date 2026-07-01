using UnityEngine;

namespace ProjectAegisRTS.UnityClient.UI.Common
{
    public sealed class PlayerControlsOverlay : MonoBehaviour
    {
        public bool visible;
        public KeyCode primaryToggleKey = KeyCode.F1;
        public KeyCode secondaryToggleKey = KeyCode.H;
        public Rect area = new Rect(0f, 0f, 560f, 390f);

        void Update()
        {
            if (Input.GetKeyDown(primaryToggleKey) || Input.GetKeyDown(secondaryToggleKey))
                visible = !visible;
        }

        public void SetVisible(bool value)
        {
            visible = value;
        }

        void OnGUI()
        {
            if (!visible)
                return;

            var centered = new Rect(
                (Screen.width - area.width) * 0.5f,
                (Screen.height - area.height) * 0.5f,
                area.width,
                area.height);

            GUILayout.BeginArea(centered, GUI.skin.box);
            GUILayout.Label("Controls");
            GUILayout.Label("Left click selects units and buildings.");
            GUILayout.Label("Right click moves selected units or attacks an enemy under the cursor.");
            GUILayout.Label("Use the right sidebar to queue buildings and units.");
            GUILayout.Label("Build a refinery near ore, keep power positive, produce combat units, then attack the enemy base.");
            GUILayout.Space(8f);
            GUILayout.Label("Space: pause or resume");
            GUILayout.Label("Period or N: single-step while paused");
            GUILayout.Label("M / A / F: move, attack, force-attack modes");
            GUILayout.Label("B: enter placement for a completed building");
            GUILayout.Label("Escape: cancel placement or clear selection");
            GUILayout.Label("O: toggle objective HUD");
            GUILayout.Label("C: toggle build-order checklist");
            GUILayout.Label("P: toggle next-step prompt");
            GUILayout.Label("F1 or H: close this overlay");
            GUILayout.Space(8f);
            if (GUILayout.Button("Close", GUILayout.Height(32f)))
                visible = false;
            GUILayout.EndArea();
        }
    }
}
