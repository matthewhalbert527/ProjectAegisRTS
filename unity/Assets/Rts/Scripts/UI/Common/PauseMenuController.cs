using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Board;
using ProjectAegisRTS.UnityClient.Scenario;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectAegisRTS.UnityClient.UI.Common
{
    [DefaultExecutionOrder(-500)]
    public sealed class PauseMenuController : MonoBehaviour
    {
        public RtsSimulationDriver driver;
        public VerticalSliceScenarioController scenarioController;
        public PauseMenuHud hud;
        public string bootSceneName = "Stage16_5_Boot";
        public bool blockGameplayInput = true;
        public bool suppressSceneLoadsForValidation;
        public bool suppressApplicationQuitForValidation;

        bool pausedByMenu;
        bool hudInitialized;

        public bool IsOpen { get; private set; }

        public void Initialize(RtsSimulationDriver simulationDriver, VerticalSliceScenarioController controller)
        {
            driver = simulationDriver;
            scenarioController = controller;
            EnsureReferences();
        }

        void Awake()
        {
            EnsureReferences();
        }

        void Start()
        {
            EnsureReferences();
        }

        void Update()
        {
            EnsureReferences();
            if (Input.GetKeyDown(KeyCode.Escape))
                HandleEscapePressed();
        }

        public void HandleEscapePressed()
        {
            EnsureReferences();
            if (driver != null && driver.HasPlacementMode)
            {
                driver.TryCancelPlacement();
                return;
            }

            var boardPlacement = FindAnyObjectByType<BoardPlacementController>();
            if (boardPlacement != null && boardPlacement.IsPlacementModeActive)
            {
                boardPlacement.CancelPlacement();
                return;
            }

            TogglePauseMenu();
        }

        public bool BlocksGameplayInput()
        {
            return IsOpen && blockGameplayInput;
        }

        public void TogglePauseMenu()
        {
            if (IsOpen)
                Resume();
            else
                OpenPauseMenu();
        }

        public void OpenPauseMenu()
        {
            EnsureReferences();
            if (IsOpen)
                return;

            pausedByMenu = driver != null && !driver.IsPaused;
            if (pausedByMenu)
                driver.TogglePause();

            IsOpen = true;
            if (hud != null)
                hud.ShowMain();
        }

        public void Resume()
        {
            EnsureReferences();
            if (!IsOpen)
                return;

            if (hud != null)
                hud.Hide();
            if (pausedByMenu && driver != null && driver.IsPaused)
                driver.TogglePause();

            pausedByMenu = false;
            IsOpen = false;
        }

        public void RestartMission()
        {
            EnsureReferences();
            if (scenarioController != null)
                scenarioController.ResetScenario();
            if (driver != null && driver.IsPaused)
                driver.TogglePause();

            pausedByMenu = false;
            IsOpen = false;
            if (hud != null)
                hud.Hide();
        }

        public void ShowSettings()
        {
            EnsureReferences();
            if (!IsOpen)
                OpenPauseMenu();
            if (hud != null)
                hud.ShowSettings();
        }

        public void ShowControls()
        {
            EnsureReferences();
            if (!IsOpen)
                OpenPauseMenu();
            if (hud != null)
                hud.ShowControls();
        }

        public void QuitToMenu()
        {
            if (suppressSceneLoadsForValidation)
            {
                Debug.Log("Pause menu Quit to Menu requested.");
                return;
            }

            SceneManager.LoadScene(bootSceneName, LoadSceneMode.Single);
        }

        public void QuitGame()
        {
            if (suppressApplicationQuitForValidation)
            {
                Debug.Log("Pause menu Quit Game requested.");
                return;
            }

#if UNITY_EDITOR
            Debug.Log("Pause menu Quit Game requested in editor.");
#else
            Application.Quit();
#endif
        }

        void EnsureReferences()
        {
            if (driver == null)
                driver = FindAnyObjectByType<RtsSimulationDriver>();
            if (scenarioController == null)
                scenarioController = FindAnyObjectByType<VerticalSliceScenarioController>();
            if (hud == null)
                hud = FindAnyObjectByType<PauseMenuHud>();
            if (hud == null)
                hud = gameObject.AddComponent<PauseMenuHud>();
            if (!hudInitialized)
            {
                hud.Initialize(this);
                hudInitialized = true;
            }
        }
    }
}
