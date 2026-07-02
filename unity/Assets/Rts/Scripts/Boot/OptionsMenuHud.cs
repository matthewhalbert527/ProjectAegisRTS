using ProjectAegisRTS.UnityClient.CoreBridge;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Boot
{
    public sealed class OptionsMenuHud : MonoBehaviour
    {
        const string MasterVolumeKey = "ProjectAegisRTS.MasterVolume";
        const string DebugPanelsKey = "ProjectAegisRTS.ShowDebugPanelsByDefault";
        const float MinimumAreaWidth = 560f;
        const float MinimumAreaHeight = 640f;

        public GameBootController controller;
        public BuildModeSettings settings;
        public PlayerDisplaySettings displaySettings;
        public bool visible;
        public bool displaySectionEnabled = true;
        public Rect area = new Rect(40f, 40f, 560f, 640f);

        int selectedWidth = 1600;
        int selectedHeight = 900;
        FullScreenMode selectedFullscreenMode = FullScreenMode.Windowed;
        string selectedSkirmishDifficultyId = "normal";
        float masterVolume = 1f;
        bool showDebugPanelsByDefault;

        void Awake()
        {
            NormalizeArea();
            if (controller == null)
                controller = FindAnyObjectByType<GameBootController>();
            if (settings == null)
                settings = FindAnyObjectByType<BuildModeSettings>();
            if (displaySettings == null)
                displaySettings = FindAnyObjectByType<PlayerDisplaySettings>();
            LoadPreferences();
            ApplyAudioDebugPreferences(false);
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

            GUILayout.BeginArea(area, GUI.skin.box);
            GUILayout.Label("Options");
            GUILayout.Label("Prototype settings for local testing. These are safe placeholders, not final game settings.");
            GUILayout.Space(8f);

            if (displaySectionEnabled)
                DrawDisplaySection();

            DrawSkirmishSection();

            GUILayout.Label("Master volume: " + Mathf.RoundToInt(masterVolume * 100f) + "%");
            var newVolume = GUILayout.HorizontalSlider(masterVolume, 0f, 1f);
            if (Mathf.Abs(newVolume - masterVolume) > 0.001f)
            {
                masterVolume = newVolume;
                ApplyAudioDebugPreferences(true);
            }

            var newDebugDefault = GUILayout.Toggle(showDebugPanelsByDefault, "Show debug panels by default");
            if (newDebugDefault != showDebugPanelsByDefault)
            {
                showDebugPanelsByDefault = newDebugDefault;
                ApplyAudioDebugPreferences(true);
            }

            var hotkeysEnabled = settings == null || settings.enableDeveloperHotkeys;
            GUILayout.Label("Debug hotkeys: " + (hotkeysEnabled ? "enabled" : "disabled"));
            GUILayout.Label("Reset settings returns this prototype build to clean player-facing defaults.");

            GUILayout.Space(10f);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Reset Settings", GUILayout.Height(32f)))
                ResetPreferences();
            if (GUILayout.Button("Reset Display Settings", GUILayout.Height(32f)))
                ResetDisplaySettings();
            if (GUILayout.Button("Back", GUILayout.Height(32f)) && controller != null)
                controller.HideOptions();
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        void DrawDisplaySection()
        {
            GUILayout.Label("Display");
            GUILayout.BeginHorizontal();
            if (GUILayout.Toggle(selectedFullscreenMode == FullScreenMode.Windowed, "Windowed"))
                selectedFullscreenMode = FullScreenMode.Windowed;
            if (GUILayout.Toggle(selectedFullscreenMode == FullScreenMode.FullScreenWindow, "Fullscreen Window"))
                selectedFullscreenMode = FullScreenMode.FullScreenWindow;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            DrawResolutionButton(1280, 720);
            DrawResolutionButton(1600, 900);
            DrawResolutionButton(1920, 1080);
            GUILayout.EndHorizontal();

            GUILayout.Label("Selected: " + selectedWidth + "x" + selectedHeight + " " + selectedFullscreenMode);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Apply Display", GUILayout.Height(30f)))
                ApplyDisplaySettings();
            GUILayout.EndHorizontal();
            GUILayout.Space(8f);
        }

        void DrawResolutionButton(int width, int height)
        {
            var label = width + "x" + height;
            var selected = selectedWidth == width && selectedHeight == height;
            if (GUILayout.Button(selected ? "[" + label + "]" : label, GUILayout.Height(28f)))
            {
                selectedWidth = width;
                selectedHeight = height;
            }
        }

        void DrawSkirmishSection()
        {
            GUILayout.Label("Skirmish");
            GUILayout.BeginHorizontal();
            DrawDifficultyButton("easy", "Easy");
            DrawDifficultyButton("normal", "Normal");
            DrawDifficultyButton("hard", "Hard");
            GUILayout.EndHorizontal();
            GUILayout.Label("Selected difficulty: " + RtsSimulationDriver.GetSkirmishDifficultyLabel(selectedSkirmishDifficultyId));
            GUILayout.Space(8f);
        }

        void NormalizeArea()
        {
            area.width = Mathf.Max(area.width, MinimumAreaWidth);
            area.height = Mathf.Max(area.height, MinimumAreaHeight);
        }

        void DrawDifficultyButton(string difficultyId, string label)
        {
            var selected = selectedSkirmishDifficultyId == difficultyId;
            if (GUILayout.Button(selected ? "[" + label + "]" : label, GUILayout.Height(28f)))
            {
                selectedSkirmishDifficultyId = RtsSimulationDriver.NormalizeSkirmishDifficultyId(difficultyId);
                ApplySkirmishDifficultyPreference();
            }
        }

        void LoadPreferences()
        {
            int savedWidth;
            int savedHeight;
            FullScreenMode savedMode;
            if (PlayerDisplaySettings.TryLoadSavedPreference(out savedWidth, out savedHeight, out savedMode))
            {
                selectedWidth = savedWidth;
                selectedHeight = savedHeight;
                selectedFullscreenMode = savedMode;
            }
            else if (displaySettings != null)
            {
                selectedWidth = displaySettings.defaultWindowWidth;
                selectedHeight = displaySettings.defaultWindowHeight;
                selectedFullscreenMode = displaySettings.preferredFullscreenMode;
            }
            else
            {
                selectedWidth = Mathf.Max(Screen.width, 1600);
                selectedHeight = Mathf.Max(Screen.height, 900);
                selectedFullscreenMode = Screen.fullScreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
            }

            masterVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(MasterVolumeKey, AudioListener.volume));
            showDebugPanelsByDefault = PlayerPrefs.GetInt(DebugPanelsKey, settings != null && settings.showDebugPanelsByDefault ? 1 : 0) != 0;
            selectedSkirmishDifficultyId = RtsSimulationDriver.NormalizeSkirmishDifficultyId(PlayerPrefs.GetString(RtsSimulationDriver.SkirmishDifficultyPlayerPrefsKey, controller == null ? "normal" : controller.SelectedSkirmishDifficultyId));
        }

        void ApplyAudioDebugPreferences(bool save)
        {
            AudioListener.volume = Mathf.Clamp01(masterVolume);
            if (settings != null)
                settings.showDebugPanelsByDefault = showDebugPanelsByDefault;

            if (!save)
                return;

            PlayerPrefs.SetFloat(MasterVolumeKey, masterVolume);
            PlayerPrefs.SetInt(DebugPanelsKey, showDebugPanelsByDefault ? 1 : 0);
            PlayerPrefs.Save();
        }

        void ApplySkirmishDifficultyPreference()
        {
            selectedSkirmishDifficultyId = RtsSimulationDriver.NormalizeSkirmishDifficultyId(selectedSkirmishDifficultyId);
            if (controller != null)
            {
                controller.SetSkirmishDifficulty(selectedSkirmishDifficultyId);
                return;
            }

            PlayerPrefs.SetString(RtsSimulationDriver.SkirmishDifficultyPlayerPrefsKey, selectedSkirmishDifficultyId);
            PlayerPrefs.Save();
        }

        void ApplyDisplaySettings()
        {
            if (displaySettings == null)
                displaySettings = FindAnyObjectByType<PlayerDisplaySettings>();
            if (displaySettings == null)
                displaySettings = gameObject.AddComponent<PlayerDisplaySettings>();

            displaySettings.ApplyAndSaveDisplaySettings(selectedWidth, selectedHeight, selectedFullscreenMode);
        }

        void ResetPreferences()
        {
            masterVolume = 1f;
            showDebugPanelsByDefault = false;
            selectedSkirmishDifficultyId = "normal";
            ApplyAudioDebugPreferences(true);
            ApplySkirmishDifficultyPreference();
            ResetDisplaySettings();
        }

        void ResetDisplaySettings()
        {
            if (displaySettings == null)
                displaySettings = FindAnyObjectByType<PlayerDisplaySettings>();
            if (displaySettings == null)
                displaySettings = gameObject.AddComponent<PlayerDisplaySettings>();

            displaySettings.ResetDisplayPreferencesAndApply();
            selectedWidth = displaySettings.defaultWindowWidth;
            selectedHeight = displaySettings.defaultWindowHeight;
            selectedFullscreenMode = displaySettings.preferredFullscreenMode;
        }
    }
}
