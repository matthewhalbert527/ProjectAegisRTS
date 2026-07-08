using ProjectAegisRTS.UnityClient.CoreBridge;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectAegisRTS.UnityClient.Boot
{
    public sealed class GameBootController : MonoBehaviour
    {
        public string verticalSliceSceneName = "Stage16_PlayableVerticalSlice";
        public BuildModeSettings settings;
        public MainMenuHud mainMenu;
        public ControlsHelpHud controlsHelp;
        public OptionsMenuHud optionsMenu;
        public string selectedSkirmishDifficultyId = "normal";
        public bool generatedSkirmishEnabled;
        public int generatedSkirmishSeed = 34034;

        public string SelectedSkirmishDifficultyId { get { return selectedSkirmishDifficultyId; } }
        public string SelectedSkirmishDifficultyLabel { get { return RtsSimulationDriver.GetSkirmishDifficultyLabel(selectedSkirmishDifficultyId); } }
        public bool GeneratedSkirmishEnabled { get { return generatedSkirmishEnabled; } }
        public int GeneratedSkirmishSeed { get { return generatedSkirmishSeed; } }

        void Awake()
        {
            if (settings == null)
                settings = FindAnyObjectByType<BuildModeSettings>();
            if (mainMenu == null)
                mainMenu = FindAnyObjectByType<MainMenuHud>();
            if (controlsHelp == null)
                controlsHelp = FindAnyObjectByType<ControlsHelpHud>();
            if (optionsMenu == null)
                optionsMenu = FindAnyObjectByType<OptionsMenuHud>();

            selectedSkirmishDifficultyId = RtsSimulationDriver.NormalizeSkirmishDifficultyId(PlayerPrefs.GetString(RtsSimulationDriver.SkirmishDifficultyPlayerPrefsKey, selectedSkirmishDifficultyId));
            generatedSkirmishEnabled = PlayerPrefs.GetInt(RtsSimulationDriver.GeneratedSkirmishEnabledPlayerPrefsKey, generatedSkirmishEnabled ? 1 : 0) != 0;
            generatedSkirmishSeed = RtsSimulationDriver.NormalizeGeneratedSkirmishSeed(PlayerPrefs.GetInt(RtsSimulationDriver.GeneratedSkirmishSeedPlayerPrefsKey, generatedSkirmishSeed));
            ShowMainMenu(settings == null || settings.startInBootMenu);
        }

        public void ShowMainMenu()
        {
            ShowMainMenu(true);
        }

        void ShowMainMenu(bool showMenu)
        {
            if (mainMenu != null)
                mainMenu.SetVisible(showMenu);
            if (controlsHelp != null)
                controlsHelp.SetVisible(false);
            if (optionsMenu != null)
                optionsMenu.SetVisible(false);
        }

        public void StartVerticalSlice()
        {
            SaveSkirmishDifficulty();
            SaveGeneratedSkirmishSettings();
            SceneManager.LoadScene(verticalSliceSceneName, LoadSceneMode.Single);
        }

        public void SetSkirmishDifficulty(string difficultyId)
        {
            selectedSkirmishDifficultyId = RtsSimulationDriver.NormalizeSkirmishDifficultyId(difficultyId);
            SaveSkirmishDifficulty();
        }

        public void SetGeneratedSkirmishEnabled(bool enabled)
        {
            generatedSkirmishEnabled = enabled;
            SaveGeneratedSkirmishSettings();
        }

        public void SetGeneratedSkirmishSeed(int seed)
        {
            generatedSkirmishSeed = RtsSimulationDriver.NormalizeGeneratedSkirmishSeed(seed);
            SaveGeneratedSkirmishSettings();
        }

        public void RandomizeGeneratedSkirmishSeed()
        {
            generatedSkirmishSeed = RtsSimulationDriver.NormalizeGeneratedSkirmishSeed(unchecked(System.Environment.TickCount ^ (int)(System.DateTime.UtcNow.Ticks & 0x7FFFFFFF)));
            SaveGeneratedSkirmishSettings();
        }

        public void ShowControls()
        {
            if (mainMenu != null)
                mainMenu.SetVisible(false);
            if (controlsHelp != null)
                controlsHelp.SetVisible(true);
            if (optionsMenu != null)
                optionsMenu.SetVisible(false);
        }

        public void ShowOptions()
        {
            if (mainMenu != null)
                mainMenu.SetVisible(false);
            if (controlsHelp != null)
                controlsHelp.SetVisible(false);
            if (optionsMenu != null)
                optionsMenu.SetVisible(true);
        }

        public void HideControls()
        {
            ShowMainMenu();
        }

        public void HideOptions()
        {
            ShowMainMenu();
        }

        public void Quit()
        {
            Application.Quit();
        }

        void SaveSkirmishDifficulty()
        {
            PlayerPrefs.SetString(RtsSimulationDriver.SkirmishDifficultyPlayerPrefsKey, selectedSkirmishDifficultyId);
            PlayerPrefs.Save();
        }

        void SaveGeneratedSkirmishSettings()
        {
            PlayerPrefs.SetInt(RtsSimulationDriver.GeneratedSkirmishEnabledPlayerPrefsKey, generatedSkirmishEnabled ? 1 : 0);
            PlayerPrefs.SetInt(RtsSimulationDriver.GeneratedSkirmishSeedPlayerPrefsKey, generatedSkirmishSeed);
            PlayerPrefs.Save();
        }
    }
}
