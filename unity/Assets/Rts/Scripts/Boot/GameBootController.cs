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

        void Awake()
        {
            if (settings == null)
                settings = FindAnyObjectByType<BuildModeSettings>();
            if (mainMenu == null)
                mainMenu = FindAnyObjectByType<MainMenuHud>();
            if (controlsHelp == null)
                controlsHelp = FindAnyObjectByType<ControlsHelpHud>();

            if (mainMenu != null)
                mainMenu.SetVisible(settings == null || settings.startInBootMenu);
            if (controlsHelp != null)
                controlsHelp.SetVisible(false);
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
        }

        public void HideControls()
        {
            if (controlsHelp != null)
                controlsHelp.SetVisible(false);
            if (mainMenu != null)
                mainMenu.SetVisible(true);
        }

        public void Quit()
        {
            Application.Quit();
        }
    }
}
