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
            SceneManager.LoadScene(verticalSliceSceneName, LoadSceneMode.Single);
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
    }
}
