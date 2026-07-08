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

            var previousMatrix = BootHudLayout.BeginArea(area);
            GUILayout.Label("Controls");
            GUILayout.Label("Left click selects units and buildings.");
            GUILayout.Label("Right click moves selected units or attacks an enemy under the cursor.");
            GUILayout.Label("Use the right sidebar to build economy, infantry, vehicles, and defenses.");
            GUILayout.Label("Build a refinery near ore, produce combat units, then destroy the enemy base.");
            GUILayout.Label("Follow the checklist: power, refinery, harvest, barracks, infantry, war factory, tank, scout, attack.");
            GUILayout.Space(8f);
            GUILayout.Label("Escape: open pause menu");
            GUILayout.Label("Space: quick pause or resume");
            GUILayout.Label("Period or N: developer single-step while paused");
            GUILayout.Label("M / A / F: move, attack, force-attack command modes");
            GUILayout.Label("B: enter placement for a completed building");
            GUILayout.Label("O: toggle objective HUD");
            GUILayout.Label("C: toggle build-order checklist");
            GUILayout.Label("P: toggle next-step prompt");
            GUILayout.Label("F1 or H: toggle in-match controls overlay");
            GUILayout.Label("F8-F12: developer debug panels when enabled");
            GUILayout.Space(10f);
            if (GUILayout.Button("Back", GUILayout.Height(32f)) && controller != null)
                controller.HideControls();
            BootHudLayout.EndArea(previousMatrix);
        }
    }
}
