using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Boot
{
    public sealed class MainMenuHud : MonoBehaviour
    {
        public GameBootController controller;
        public bool visible = true;
        public Rect area = new Rect(48f, 48f, 520f, 410f);

        const float MinimumAreaWidth = 520f;
        const float MinimumAreaHeight = 410f;

        void Awake()
        {
            NormalizeArea();
            if (controller == null)
                controller = FindAnyObjectByType<GameBootController>();
        }

        void OnValidate()
        {
            NormalizeArea();
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
            GUILayout.Label("ProjectAegisRTS");
            GUILayout.Label("Development Prototype");
            GUILayout.Label("Vertical Slice Build");
            GUILayout.Label("Skirmish: " + (controller == null ? "Normal" : controller.SelectedSkirmishDifficultyLabel));
            GUILayout.Space(12f);
            if (GUILayout.Button("Start Vertical Slice", GUILayout.Height(36f)) && controller != null)
                controller.StartVerticalSlice();
            if (GUILayout.Button("Controls", GUILayout.Height(32f)) && controller != null)
                controller.ShowControls();
            if (GUILayout.Button("Options", GUILayout.Height(32f)) && controller != null)
                controller.ShowOptions();
            if (GUILayout.Button("Quit", GUILayout.Height(32f)) && controller != null)
                controller.Quit();
            GUILayout.Space(8f);
            GUILayout.Label("Stage 27 skirmish playability pass");
            BootHudLayout.EndArea(previousMatrix);
        }

        void NormalizeArea()
        {
            area.width = Mathf.Max(area.width, MinimumAreaWidth);
            area.height = Mathf.Max(area.height, MinimumAreaHeight);
        }
    }
}
