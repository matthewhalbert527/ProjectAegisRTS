using System.Collections;
using ProjectAegisRTS.UnityClient.CameraControls;
using ProjectAegisRTS.UnityClient.Board;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Scenario;
using ProjectAegisRTS.UnityClient.UI.Common;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.Boot
{
    public sealed class PlayerBuildSceneInitializer : MonoBehaviour
    {
        public bool frameCameraOnStart = true;
        public bool startScenarioOnLoad = true;
        public bool hideDebugPanelsOnStart = true;
        public bool cancelPlacementOnStart = true;
        public Vector3 cameraPosition = new Vector3(16f, 30f, -2f);
        public Vector3 cameraRotationEuler = new Vector3(60f, 0f, 0f);
        public float cameraOrthographicSize = 18f;

        DebugHudVisibilityController debugVisibility;

        void Awake()
        {
            ApplyPlayerFacingDefaults();
        }

        IEnumerator Start()
        {
            yield return null;
            ApplyPlayerFacingDefaults();
        }

        public void ApplyPlayerFacingDefaults()
        {
            var boardRoot = GameObject.Find("BoardRoot");
            if (boardRoot != null && !boardRoot.activeSelf)
                boardRoot.SetActive(true);

            if (frameCameraOnStart)
                ConfigureCamera();
            ConfigureLighting();

            if (debugVisibility == null)
                debugVisibility = GetComponent<DebugHudVisibilityController>();
            if (debugVisibility == null)
                debugVisibility = FindAnyObjectByType<DebugHudVisibilityController>();
            if (debugVisibility != null && hideDebugPanelsOnStart)
                debugVisibility.ApplyPlayerFacingDefaults();

            var uiMode = FindAnyObjectByType<PlayerFacingUiModeController>();
            if (uiMode != null)
                uiMode.ApplyModeDefaults();

            if (cancelPlacementOnStart)
                CancelPlacementModes();
            ApplyResponsiveCanvasDefaults();
            ApplyPlayerHudDefaults();

            if (startScenarioOnLoad)
                StartScenarioIfAvailable();

            Debug.Log("Stage 16 player-facing startup complete. BoardRoot active: " + (boardRoot != null && boardRoot.activeInHierarchy) + ". Debug panels default hidden: " + hideDebugPanelsOnStart + ". Placement inactive: " + cancelPlacementOnStart + ".");
        }

        void ConfigureCamera()
        {
            var camera = Camera.main;
            if (camera == null)
                camera = FindAnyObjectByType<Camera>();
            if (camera == null)
            {
                var cameraObject = new GameObject("Main Camera");
                cameraObject.tag = "MainCamera";
                camera = cameraObject.AddComponent<Camera>();
            }

            camera.orthographic = true;
            camera.orthographicSize = cameraOrthographicSize;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.10f, 0.12f, 0.13f, 1f);
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 1000f;
            camera.transform.position = cameraPosition;
            camera.transform.rotation = Quaternion.Euler(cameraRotationEuler);

            var cameraController = camera.GetComponent<RtsCameraController>();
            if (cameraController != null)
            {
                cameraController.preserveConfiguredTransform = true;
                cameraController.orthographicSize = cameraOrthographicSize;
                cameraController.maxHeight = cameraPosition.y;
            }

            if (FindAnyObjectByType<AudioListener>() == null)
                camera.gameObject.AddComponent<AudioListener>();
        }

        static void ConfigureLighting()
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.82f, 0.84f, 0.82f, 1f);

            var light = FindAnyObjectByType<Light>();
            if (light == null)
            {
                var lightObject = new GameObject("Directional Light");
                light = lightObject.AddComponent<Light>();
                light.type = LightType.Directional;
            }

            light.type = LightType.Directional;
            light.intensity = 2.05f;
            light.transform.rotation = Quaternion.Euler(54f, -35f, 0f);
        }

        static void CancelPlacementModes()
        {
            var driver = FindAnyObjectByType<RtsSimulationDriver>();
            if (driver != null)
                driver.TryCancelPlacement();

            var boardPlacement = FindAnyObjectByType<BoardPlacementController>();
            if (boardPlacement != null)
                boardPlacement.SetPlacementMode(false);
        }

        static void ApplyPlayerHudDefaults()
        {
            var objectiveHud = FindAnyObjectByType<PlayerObjectiveHud>();
            if (objectiveHud != null)
            {
                objectiveHud.area = PlayerHudLayout.ObjectiveArea;
                objectiveHud.visible = true;
            }

            var checklistHud = FindAnyObjectByType<VerticalSliceChecklistHud>();
            if (checklistHud != null)
            {
                checklistHud.area = PlayerHudLayout.ChecklistArea;
                checklistHud.visible = true;
            }

            var promptSystem = FindAnyObjectByType<PlayerPromptSystem>();
            if (promptSystem != null)
                promptSystem.visible = true;

            var promptHud = FindAnyObjectByType<PlayerPromptHud>();
            if (promptHud != null)
            {
                promptHud.area = PlayerHudLayout.PromptArea;
                promptHud.visible = true;
            }

            var matchHud = FindAnyObjectByType<MatchObjectiveHud>();
            if (matchHud != null)
            {
                matchHud.area = PlayerHudLayout.MatchArea;
                matchHud.visible = true;
                matchHud.showDebugActions = false;
            }

            var controlsOverlay = FindAnyObjectByType<PlayerControlsOverlay>();
            if (controlsOverlay != null)
                controlsOverlay.visible = false;

            var resultHud = FindAnyObjectByType<MatchResultHud>();
            if (resultHud != null)
                resultHud.visible = true;

            var pauseMenu = FindAnyObjectByType<PauseMenuController>();
            if (pauseMenu != null)
            {
                pauseMenu.blockGameplayInput = true;
                if (pauseMenu.hud != null)
                    pauseMenu.hud.Hide();
            }

            var desktopHud = FindAnyObjectByType<ProjectAegisRTS.UnityClient.UI.Desktop.DesktopRtsHudRoot>();
            if (desktopHud != null)
            {
                desktopHud.showDebugOverlay = false;
                desktopHud.Initialize();
            }

            var statusLog = FindAnyObjectByType<RtsStatusLog>();
            if (statusLog != null)
            {
                statusLog.visible = false;
                statusLog.gameObject.SetActive(false);
            }
        }

        static void ApplyResponsiveCanvasDefaults()
        {
            var canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (var i = 0; i < canvases.Length; i++)
            {
                if (canvases[i] == null)
                    continue;

                var enforcer = canvases[i].GetComponent<ResponsiveCanvasScalerEnforcer>();
                if (enforcer == null)
                    enforcer = canvases[i].gameObject.AddComponent<ResponsiveCanvasScalerEnforcer>();

                enforcer.referenceResolution = new Vector2(1920f, 1080f);
                enforcer.matchWidthOrHeight = 0.5f;
                enforcer.Enforce();
            }
        }

        static void StartScenarioIfAvailable()
        {
            var driver = FindAnyObjectByType<RtsSimulationDriver>();
            if (driver != null)
                driver.UseVerticalSliceDemoWorld = true;

            var scenario = FindAnyObjectByType<VerticalSliceScenarioController>();
            if (scenario != null)
                scenario.StartScenario();
        }
    }
}
