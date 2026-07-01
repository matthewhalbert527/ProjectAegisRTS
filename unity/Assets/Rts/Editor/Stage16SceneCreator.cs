using System;
using System.Collections.Generic;
using ProjectAegisRTS.UnityClient.Boot;
using ProjectAegisRTS.UnityClient.Bootstrap;
using ProjectAegisRTS.UnityClient.CameraControls;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.InputControls.Desktop;
using ProjectAegisRTS.UnityClient.InputControls.XR;
using ProjectAegisRTS.UnityClient.Rendering;
using ProjectAegisRTS.UnityClient.Scenario;
using ProjectAegisRTS.UnityClient.Selection;
using ProjectAegisRTS.UnityClient.UI.Common;
using ProjectAegisRTS.UnityClient.UI.Desktop;
using ProjectAegisRTS.UnityClient.UI.XR.LeftHand;
using ProjectAegisRTS.UnityClient.UI.XR.RightHand;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage16SceneCreator
    {
        public const string ScenePath = "Assets/Rts/Scenes/Stage16_PlayableVerticalSlice.unity";

        [MenuItem("ProjectAegisRTS/Stage 16/Create Playable Vertical Slice Scene")]
        public static void CreateStage16SceneMenu()
        {
            CreateOrUpdateStage16Scene();
        }

        public static void CreateStage16SceneBatch()
        {
            try
            {
                CreateOrUpdateStage16Scene();
                if (Application.isBatchMode)
                    EditorApplication.Exit(0);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                if (Application.isBatchMode)
                    EditorApplication.Exit(1);
                throw;
            }
        }

        public static void CreateOrUpdateStage16Scene()
        {
            if (System.IO.File.Exists(Stage15SceneCreator.ScenePath))
                EditorSceneManager.OpenScene(Stage15SceneCreator.ScenePath);
            else
                Stage15SceneCreator.CreateOrUpdateStage15Scene();

            var scene = EditorSceneManager.GetActiveScene();
            var game = RequireObject("RtsGame");
            ConfigureCamera(Camera.main != null ? Camera.main : UnityEngine.Object.FindFirstObjectByType<Camera>());

            var driver = GetOrAdd<RtsSimulationDriver>(game);
            driver.UseVerticalSliceDemoWorld = true;
            driver.UseMapTerrainDemoWorld = false;
            driver.UseAiSkirmishDemoWorld = false;
            driver.UseFogRadarDemoWorld = false;
            driver.UseEconomyDemoWorld = false;
            driver.UseCombatDemoWorld = false;
            driver.UsePlayerPerspectiveSnapshot = true;

            var controller = GetOrAdd<VerticalSliceScenarioController>(game);
            var progressTracker = GetOrAdd<VerticalSliceProgressTracker>(game);
            var missionFlow = GetOrAdd<VerticalSliceMissionFlowController>(game);
            var objectiveHud = GetOrAdd<MatchObjectiveHud>(game);
            var playerObjectiveHud = GetOrAdd<PlayerObjectiveHud>(game);
            var checklistHud = GetOrAdd<VerticalSliceChecklistHud>(game);
            var promptSystem = GetOrAdd<PlayerPromptSystem>(game);
            var playerPromptHud = GetOrAdd<PlayerPromptHud>(game);
            var playerControlsOverlay = GetOrAdd<PlayerControlsOverlay>(game);
            var matchResultHud = GetOrAdd<MatchResultHud>(game);
            var pauseMenuController = GetOrAdd<PauseMenuController>(game);
            var pauseMenuHud = GetOrAdd<PauseMenuHud>(game);
            var uiMode = GetOrAdd<PlayerFacingUiModeController>(game);
            var systemsHud = GetOrAdd<IntegratedSystemsStatusHud>(game);
            var debugActions = GetOrAdd<VerticalSliceDebugActions>(game);
            var playerInitializer = GetOrAdd<PlayerBuildSceneInitializer>(game);
            var debugVisibility = GetOrAdd<DebugHudVisibilityController>(game);
            var bootstrapper = GetOrAdd<RtsGameBootstrapper>(game);
            EnsureDesktopHud(bootstrapper, driver, progressTracker, missionFlow);
            EnsureDualHandCompatibility(game, driver);

            controller.driver = driver;
            controller.objectiveHud = objectiveHud;
            controller.statusHud = systemsHud;
            controller.debugActions = debugActions;
            controller.startOnInitialize = true;
            controller.resetWorldOnInitialize = true;

            progressTracker.driver = driver;
            missionFlow.driver = driver;
            missionFlow.progressTracker = progressTracker;

            objectiveHud.driver = driver;
            objectiveHud.scenarioController = controller;
            objectiveHud.debugActions = debugActions;
            objectiveHud.visible = true;
            objectiveHud.showDebugActions = false;

            playerObjectiveHud.driver = driver;
            playerObjectiveHud.progressTracker = progressTracker;
            playerObjectiveHud.visible = true;

            checklistHud.driver = driver;
            checklistHud.missionFlowController = missionFlow;
            checklistHud.progressTracker = progressTracker;
            checklistHud.visible = true;

            promptSystem.driver = driver;
            promptSystem.missionFlowController = missionFlow;
            promptSystem.progressTracker = progressTracker;
            promptSystem.visible = true;

            playerPromptHud.driver = driver;
            playerPromptHud.promptSystem = promptSystem;
            playerPromptHud.visible = true;

            playerControlsOverlay.visible = false;

            matchResultHud.driver = driver;
            matchResultHud.scenarioController = controller;
            matchResultHud.visible = true;

            pauseMenuController.driver = driver;
            pauseMenuController.scenarioController = controller;
            pauseMenuController.hud = pauseMenuHud;
            pauseMenuController.blockGameplayInput = true;
            pauseMenuController.suppressSceneLoadsForValidation = false;
            pauseMenuController.suppressApplicationQuitForValidation = false;
            pauseMenuHud.Initialize(pauseMenuController);

            systemsHud.driver = driver;
            systemsHud.visible = false;

            debugActions.driver = driver;
            debugActions.creditGrantAmount = 1000;

            playerInitializer.frameCameraOnStart = true;
            playerInitializer.startScenarioOnLoad = true;
            playerInitializer.hideDebugPanelsOnStart = true;
            playerInitializer.cancelPlacementOnStart = true;
            playerInitializer.cameraPosition = new Vector3(16f, 30f, -2f);
            playerInitializer.cameraRotationEuler = new Vector3(60f, 0f, 0f);
            playerInitializer.cameraOrthographicSize = 18f;

            debugVisibility.showDebugPanelsByDefault = false;
            debugVisibility.hideDebugPanelsOnStart = true;
            debugVisibility.keepPlacementPanelsSynced = true;
            debugVisibility.ApplyPlayerFacingDefaults();

            uiMode.pcPlayerFacingMode = true;
            uiMode.allowSimulatedXrMenusInPcMode = false;
            uiMode.debugVisibility = debugVisibility;

            bootstrapper.simulationDriver = driver;
            bootstrapper.verticalSliceScenarioController = controller;
            bootstrapper.verticalSliceMissionFlowController = missionFlow;
            bootstrapper.verticalSliceProgressTracker = progressTracker;
            bootstrapper.matchObjectiveHud = objectiveHud;
            bootstrapper.playerObjectiveHud = playerObjectiveHud;
            bootstrapper.verticalSliceChecklistHud = checklistHud;
            bootstrapper.playerPromptSystem = promptSystem;
            bootstrapper.playerPromptHud = playerPromptHud;
            bootstrapper.playerControlsOverlay = playerControlsOverlay;
            bootstrapper.matchResultHud = matchResultHud;
            bootstrapper.pauseMenuController = pauseMenuController;
            bootstrapper.pauseMenuHud = pauseMenuHud;
            bootstrapper.playerFacingUiModeController = uiMode;
            bootstrapper.integratedSystemsStatusHud = systemsHud;
            bootstrapper.verticalSliceDebugActions = debugActions;
            bootstrapper.startPaused = false;
            uiMode.ApplyModeDefaults();

            EditorSceneManager.SaveScene(scene, ScenePath);
            UpdateBuildScenes();
            AssetDatabase.SaveAssets();
            Debug.Log("Created Stage 16 scene at " + ScenePath);
        }

        static void ConfigureCamera(Camera camera)
        {
            if (camera == null)
                return;
            camera.orthographic = true;
            camera.orthographicSize = 18f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.10f, 0.12f, 0.13f, 1f);
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 1000f;
            camera.transform.position = new Vector3(16f, 30f, -2f);
            camera.transform.rotation = Quaternion.Euler(60f, 0f, 0f);

            var cameraController = camera.GetComponent<RtsCameraController>();
            if (cameraController != null)
            {
                cameraController.preserveConfiguredTransform = true;
                cameraController.orthographicSize = 18f;
                cameraController.maxHeight = 30f;
            }

            if (UnityEngine.Object.FindFirstObjectByType<AudioListener>() == null)
                camera.gameObject.AddComponent<AudioListener>();

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.82f, 0.84f, 0.82f, 1f);
            var light = UnityEngine.Object.FindFirstObjectByType<Light>();
            if (light != null)
            {
                light.type = LightType.Directional;
                light.intensity = 2.05f;
                light.transform.rotation = Quaternion.Euler(54f, -35f, 0f);
            }
        }

        static void UpdateBuildScenes()
        {
            var stagePaths = new[]
            {
                "Assets/Rts/Scenes/Stage1_DesktopBoard.unity",
                "Assets/Rts/Scenes/Stage2_PCSidebar.unity",
                "Assets/Rts/Scenes/Stage3_XRBoardPlacement.unity",
                "Assets/Rts/Scenes/Stage4_LeftHandBuildSelection.unity",
                "Assets/Rts/Scenes/Stage5_DualHandCommand.unity",
                "Assets/Rts/Scenes/Stage6_MovementVisualization.unity",
                Stage7SceneCreator.ScenePath,
                Stage8SceneCreator.ScenePath,
                Stage9SceneCreator.ScenePath,
                Stage10SceneCreator.ScenePath,
                Stage11SceneCreator.ScenePath,
                Stage12SceneCreator.ScenePath,
                Stage13SceneCreator.ScenePath,
                Stage14SceneCreator.ScenePath,
                Stage15SceneCreator.ScenePath,
                ScenePath
            };

            var scenes = new List<EditorBuildSettingsScene>();
            var bootScenePath = "Assets/Rts/Scenes/Stage16_5_Boot.unity";
            if (System.IO.File.Exists(bootScenePath))
            {
                scenes.Add(new EditorBuildSettingsScene(bootScenePath, true));
                scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
            }

            for (var i = 0; i < stagePaths.Length; i++)
                if (System.IO.File.Exists(stagePaths[i]) && !ContainsScene(scenes, stagePaths[i]))
                    scenes.Add(new EditorBuildSettingsScene(stagePaths[i], true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        static void EnsureDesktopHud(RtsGameBootstrapper bootstrapper, RtsSimulationDriver driver, VerticalSliceProgressTracker progressTracker, VerticalSliceMissionFlowController missionFlow)
        {
            EnsureEventSystem();
            var canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
                canvas = CreateCanvas();

            var hudRootObject = GameObject.Find("DesktopRtsHudRoot");
            if (hudRootObject == null)
                hudRootObject = CreateRectChild(canvas.transform, "DesktopRtsHudRoot", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var hudRoot = GetOrAdd<DesktopRtsHudRoot>(hudRootObject);
            var router = GetOrAdd<DesktopUiCommandRouter>(hudRootObject);
            var layout = GetOrAdd<CncStyleSidebarLayout>(hudRootObject);
            hudRoot.bootstrapper = bootstrapper;
            hudRoot.driver = driver;
            hudRoot.missionFlowController = missionFlow;
            hudRoot.progressTracker = progressTracker;
            hudRoot.canvas = canvas;
            hudRoot.commandRouter = router;
            hudRoot.cncSidebarLayout = layout;
            hudRoot.showDebugOverlay = false;
            hudRoot.Initialize();
        }

        static void EnsureDualHandCompatibility(GameObject game, RtsSimulationDriver driver)
        {
            var mapper = UnityEngine.Object.FindFirstObjectByType<BoardCoordinateMapper>();
            var boardRenderer = UnityEngine.Object.FindFirstObjectByType<BoardRenderer>();
            var camera = Camera.main != null ? Camera.main : UnityEngine.Object.FindFirstObjectByType<Camera>();
            var statusLog = UnityEngine.Object.FindFirstObjectByType<RtsStatusLog>();

            var leftObject = GameObject.Find("Stage4 Left Hand Controllers");
            if (leftObject == null)
                leftObject = new GameObject("Stage4 Left Hand Controllers");
            var leftDesktop = GetOrAdd<DesktopLeftHandInputSource>(leftObject);
            leftDesktop.sceneCamera = camera;
            var leftXr = GetOrAdd<XrLeftHandInputAdapter>(leftObject);
            leftXr.adapterEnabled = false;
            var leftRouter = GetOrAdd<LeftHandCommandRouter>(leftObject);
            var buildMenu = GetOrAdd<LeftHandBuildMenuController>(leftObject);
            var selection = GetOrAdd<LeftHandSelectionController>(leftObject);
            var lasso = GetOrAdd<LeftHandLassoSelectionController>(leftObject);
            var leftCoordinator = GetOrAdd<Stage4ModeCoordinator>(leftObject);

            leftRouter.driver = driver;
            leftRouter.statusLog = statusLog;
            leftRouter.buildMenu = buildMenu;
            leftRouter.selectionController = selection;
            leftRouter.modeCoordinator = leftCoordinator;
            buildMenu.driver = driver;
            buildMenu.commandRouter = leftRouter;
            selection.driver = driver;
            selection.mapper = mapper;
            selection.commandRouter = leftRouter;
            selection.statusLog = statusLog;
            lasso.driver = driver;
            lasso.mapper = mapper;
            lasso.boardRenderer = boardRenderer;
            lasso.commandRouter = leftRouter;
            leftCoordinator.driver = driver;
            leftCoordinator.mapper = mapper;
            leftCoordinator.boardRenderer = boardRenderer;
            leftCoordinator.desktopInput = leftDesktop;
            leftCoordinator.xrInput = leftXr;
            leftCoordinator.buildMenu = buildMenu;
            leftCoordinator.commandRouter = leftRouter;
            leftCoordinator.selectionController = selection;
            leftCoordinator.lassoController = lasso;

            var preview = game.GetComponent<CommandPreviewRenderer>();
            if (preview == null)
                preview = game.AddComponent<CommandPreviewRenderer>();
            preview.mapper = mapper;

            var rightObject = GameObject.Find("Stage5 Right Hand Controllers");
            if (rightObject == null)
                rightObject = new GameObject("Stage5 Right Hand Controllers");
            var rightDesktop = GetOrAdd<DesktopRightHandInputSource>(rightObject);
            rightDesktop.sceneCamera = camera;
            var rightXr = GetOrAdd<XrRightHandInputAdapter>(rightObject);
            rightXr.adapterEnabled = false;
            var rightRouter = GetOrAdd<RightHandCommandRouter>(rightObject);
            var rightReticle = GetOrAdd<RightHandCommandReticle>(rightObject);
            var rightCoordinator = GetOrAdd<Stage5DualHandModeCoordinator>(rightObject);

            rightRouter.driver = driver;
            rightRouter.statusLog = statusLog;
            rightRouter.previewRenderer = preview;
            rightReticle.previewRenderer = preview;
            rightCoordinator.driver = driver;
            rightCoordinator.mapper = mapper;
            rightCoordinator.boardRenderer = boardRenderer;
            rightCoordinator.leftHandCoordinator = leftCoordinator;
            rightCoordinator.desktopInput = rightDesktop;
            rightCoordinator.xrInput = rightXr;
            rightCoordinator.commandRouter = rightRouter;
            rightCoordinator.commandReticle = rightReticle;
        }

        static void EnsureEventSystem()
        {
            if (UnityEngine.Object.FindFirstObjectByType<EventSystem>() != null)
                return;

            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        static Canvas CreateCanvas()
        {
            var canvasObject = new GameObject("Stage16 Canvas");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<GraphicRaycaster>();
            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        static GameObject CreateRectChild(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            var rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            return obj;
        }

        static GameObject RequireObject(string name)
        {
            var obj = GameObject.Find(name);
            if (obj == null)
                throw new InvalidOperationException("Missing GameObject: " + name);
            return obj;
        }

        static T GetOrAdd<T>(GameObject target) where T : Component
        {
            var component = target.GetComponent<T>();
            return component != null ? component : target.AddComponent<T>();
        }

        static bool ContainsScene(List<EditorBuildSettingsScene> scenes, string path)
        {
            for (var i = 0; i < scenes.Count; i++)
                if (scenes[i].path == path)
                    return true;
            return false;
        }
    }
}
