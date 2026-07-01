using System;
using System.Collections.Generic;
using ProjectAegisRTS.UnityClient.Boot;
using ProjectAegisRTS.UnityClient.Bootstrap;
using ProjectAegisRTS.UnityClient.CameraControls;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Scenario;
using ProjectAegisRTS.UnityClient.UI.Common;
using ProjectAegisRTS.UnityClient.UI.Desktop;
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
            var objectiveHud = GetOrAdd<MatchObjectiveHud>(game);
            var playerObjectiveHud = GetOrAdd<PlayerObjectiveHud>(game);
            var playerPromptHud = GetOrAdd<PlayerPromptHud>(game);
            var playerControlsOverlay = GetOrAdd<PlayerControlsOverlay>(game);
            var matchResultHud = GetOrAdd<MatchResultHud>(game);
            var systemsHud = GetOrAdd<IntegratedSystemsStatusHud>(game);
            var debugActions = GetOrAdd<VerticalSliceDebugActions>(game);
            var playerInitializer = GetOrAdd<PlayerBuildSceneInitializer>(game);
            var debugVisibility = GetOrAdd<DebugHudVisibilityController>(game);
            var bootstrapper = GetOrAdd<RtsGameBootstrapper>(game);
            EnsureDesktopHud(bootstrapper, driver);

            controller.driver = driver;
            controller.objectiveHud = objectiveHud;
            controller.statusHud = systemsHud;
            controller.debugActions = debugActions;
            controller.startOnInitialize = true;
            controller.resetWorldOnInitialize = true;

            objectiveHud.driver = driver;
            objectiveHud.scenarioController = controller;
            objectiveHud.debugActions = debugActions;
            objectiveHud.visible = true;
            objectiveHud.showDebugActions = false;

            playerObjectiveHud.driver = driver;
            playerObjectiveHud.visible = true;

            playerPromptHud.driver = driver;
            playerPromptHud.visible = true;

            playerControlsOverlay.visible = false;

            matchResultHud.driver = driver;
            matchResultHud.scenarioController = controller;
            matchResultHud.visible = true;

            systemsHud.driver = driver;
            systemsHud.visible = false;

            debugActions.driver = driver;
            debugActions.creditGrantAmount = 1000;

            playerInitializer.frameCameraOnStart = true;
            playerInitializer.startScenarioOnLoad = true;
            playerInitializer.hideDebugPanelsOnStart = true;
            playerInitializer.cancelPlacementOnStart = true;
            playerInitializer.cameraPosition = new Vector3(16f, 34f, -4f);
            playerInitializer.cameraRotationEuler = new Vector3(60f, 0f, 0f);
            playerInitializer.cameraOrthographicSize = 22f;

            debugVisibility.showDebugPanelsByDefault = false;
            debugVisibility.hideDebugPanelsOnStart = true;
            debugVisibility.keepPlacementPanelsSynced = true;
            debugVisibility.ApplyPlayerFacingDefaults();

            bootstrapper.simulationDriver = driver;
            bootstrapper.verticalSliceScenarioController = controller;
            bootstrapper.matchObjectiveHud = objectiveHud;
            bootstrapper.playerObjectiveHud = playerObjectiveHud;
            bootstrapper.playerPromptHud = playerPromptHud;
            bootstrapper.playerControlsOverlay = playerControlsOverlay;
            bootstrapper.matchResultHud = matchResultHud;
            bootstrapper.integratedSystemsStatusHud = systemsHud;
            bootstrapper.verticalSliceDebugActions = debugActions;
            bootstrapper.startPaused = false;

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
            camera.orthographicSize = 22f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.07f, 0.085f, 0.095f, 1f);
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 1000f;
            camera.transform.position = new Vector3(16f, 34f, -4f);
            camera.transform.rotation = Quaternion.Euler(60f, 0f, 0f);

            var cameraController = camera.GetComponent<RtsCameraController>();
            if (cameraController != null)
            {
                cameraController.preserveConfiguredTransform = true;
                cameraController.orthographicSize = 22f;
                cameraController.maxHeight = 34f;
            }

            if (UnityEngine.Object.FindFirstObjectByType<AudioListener>() == null)
                camera.gameObject.AddComponent<AudioListener>();

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.66f, 0.70f, 0.72f, 1f);
            var light = UnityEngine.Object.FindFirstObjectByType<Light>();
            if (light != null)
            {
                light.type = LightType.Directional;
                light.intensity = 1.65f;
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

        static void EnsureDesktopHud(RtsGameBootstrapper bootstrapper, RtsSimulationDriver driver)
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
            hudRoot.bootstrapper = bootstrapper;
            hudRoot.driver = driver;
            hudRoot.canvas = canvas;
            hudRoot.commandRouter = router;
            hudRoot.showDebugOverlay = false;
            hudRoot.Initialize();
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
