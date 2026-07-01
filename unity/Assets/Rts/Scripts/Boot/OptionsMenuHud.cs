using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Boot
{
    public sealed class OptionsMenuHud : MonoBehaviour
    {
        const string FullscreenKey = "ProjectAegisRTS.Fullscreen";
        const string MasterVolumeKey = "ProjectAegisRTS.MasterVolume";
        const string DebugPanelsKey = "ProjectAegisRTS.ShowDebugPanelsByDefault";

        public GameBootController controller;
        public BuildModeSettings settings;
        public bool visible;
        public Rect area = new Rect(40f, 40f, 500f, 360f);

        bool fullscreenPreference;
        float masterVolume = 1f;
        bool showDebugPanelsByDefault;

        void Awake()
        {
            if (controller == null)
                controller = FindAnyObjectByType<GameBootController>();
            if (settings == null)
                settings = FindAnyObjectByType<BuildModeSettings>();
            LoadPreferences();
            ApplyPreferences(false);
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

            var newFullscreen = GUILayout.Toggle(fullscreenPreference, "Fullscreen");
            if (newFullscreen != fullscreenPreference)
            {
                fullscreenPreference = newFullscreen;
                ApplyPreferences(true);
            }

            GUILayout.Label("Master volume: " + Mathf.RoundToInt(masterVolume * 100f) + "%");
            var newVolume = GUILayout.HorizontalSlider(masterVolume, 0f, 1f);
            if (Mathf.Abs(newVolume - masterVolume) > 0.001f)
            {
                masterVolume = newVolume;
                ApplyPreferences(true);
            }

            var newDebugDefault = GUILayout.Toggle(showDebugPanelsByDefault, "Show debug panels by default");
            if (newDebugDefault != showDebugPanelsByDefault)
            {
                showDebugPanelsByDefault = newDebugDefault;
                ApplyPreferences(true);
            }

            var hotkeysEnabled = settings == null || settings.enableDeveloperHotkeys;
            GUILayout.Label("Debug hotkeys: " + (hotkeysEnabled ? "enabled" : "disabled"));
            GUILayout.Label("Reset settings returns this prototype build to clean player-facing defaults.");

            GUILayout.Space(10f);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Reset Settings", GUILayout.Height(32f)))
                ResetPreferences();
            if (GUILayout.Button("Back", GUILayout.Height(32f)) && controller != null)
                controller.HideOptions();
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        void LoadPreferences()
        {
            fullscreenPreference = PlayerPrefs.GetInt(FullscreenKey, Screen.fullScreen ? 1 : 0) != 0;
            masterVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(MasterVolumeKey, AudioListener.volume));
            showDebugPanelsByDefault = PlayerPrefs.GetInt(DebugPanelsKey, settings != null && settings.showDebugPanelsByDefault ? 1 : 0) != 0;
        }

        void ApplyPreferences(bool save)
        {
            Screen.fullScreen = fullscreenPreference;
            AudioListener.volume = Mathf.Clamp01(masterVolume);
            if (settings != null)
                settings.showDebugPanelsByDefault = showDebugPanelsByDefault;

            if (!save)
                return;

            PlayerPrefs.SetInt(FullscreenKey, fullscreenPreference ? 1 : 0);
            PlayerPrefs.SetFloat(MasterVolumeKey, masterVolume);
            PlayerPrefs.SetInt(DebugPanelsKey, showDebugPanelsByDefault ? 1 : 0);
            PlayerPrefs.Save();
        }

        void ResetPreferences()
        {
            fullscreenPreference = false;
            masterVolume = 1f;
            showDebugPanelsByDefault = false;
            ApplyPreferences(true);
        }
    }
}
