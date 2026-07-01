using System;
using System.Collections.Generic;
using System.IO;
using ProjectAegisRTS.UnityClient.Boot;
using ProjectAegisRTS.UnityClient.Bootstrap;
using ProjectAegisRTS.UnityClient.Board;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Scenario;
using ProjectAegisRTS.UnityClient.UI.Common;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage16_5BuildFlowConfigurator
    {
        public const string BootScenePath = "Assets/Rts/Scenes/Stage16_5_Boot.unity";
        const string BuildPathArgument = "-stage16WindowsPlayerPath";
        const string DefaultWindowsPlayerPath = "../../build/windows-player-stage16/ProjectAegisRTS.exe";

        [MenuItem("ProjectAegisRTS/Stage 16.5/Configure Player Build Flow")]
        public static void ConfigurePlayerBuildFlowMenu()
        {
            ConfigureBuildFlow();
        }

        public static void ConfigureBuildFlowBatch()
        {
            try
            {
                ConfigureBuildFlow();
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

        public static void BuildWindowsPlayerBatch()
        {
            try
            {
                ConfigureBuildFlow();
                var outputPath = GetCommandLineArgument(BuildPathArgument);
                if (string.IsNullOrWhiteSpace(outputPath))
                    outputPath = Path.GetFullPath(Path.Combine(Application.dataPath, DefaultWindowsPlayerPath));
                outputPath = Path.GetFullPath(outputPath);

                var outputDirectory = Path.GetDirectoryName(outputPath);
                if (string.IsNullOrWhiteSpace(outputDirectory))
                    throw new InvalidOperationException("Windows player output path must include a directory.");
                Directory.CreateDirectory(outputDirectory);

                var options = new BuildPlayerOptions
                {
                    scenes = new[] { BootScenePath, Stage16SceneCreator.ScenePath },
                    locationPathName = outputPath,
                    target = BuildTarget.StandaloneWindows64,
                    options = BuildOptions.None
                };

                var report = BuildPipeline.BuildPlayer(options);
                if (report.summary.result != BuildResult.Succeeded)
                    throw new InvalidOperationException("Stage 16.5 Windows player build failed with result " + report.summary.result + ".");

                Debug.Log("Stage 16.5 Windows player build succeeded: " + outputPath + " (" + report.summary.totalSize + " bytes).");
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

        public static void ConfigureBuildFlow()
        {
            CreateOrUpdateBootScene();
            Stage16SceneCreator.CreateOrUpdateStage16Scene();
            ConfigureStage16SceneDefaults();
            ConfigureBuildSettings();
            AssetDatabase.SaveAssets();
            Debug.Log("Stage 16.5 player build flow configured.");
        }

        static void CreateOrUpdateBootScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Stage16_5_Boot";

            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.02f, 0.03f, 0.04f, 1f);
            cameraObject.AddComponent<AudioListener>();

            var lightObject = new GameObject("Directional Light");
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 0.85f;
            lightObject.transform.rotation = Quaternion.Euler(50f, -35f, 0f);

            var bootObject = new GameObject("Stage16_5 Boot");
            var settings = bootObject.AddComponent<BuildModeSettings>();
            settings.showDebugPanelsByDefault = false;
            settings.startInBootMenu = true;
            settings.defaultCleanHud = true;
            settings.enableDeveloperHotkeys = true;

            var controller = bootObject.AddComponent<GameBootController>();
            var mainMenu = bootObject.AddComponent<MainMenuHud>();
            var controls = bootObject.AddComponent<ControlsHelpHud>();
            controller.settings = settings;
            controller.mainMenu = mainMenu;
            controller.controlsHelp = controls;
            mainMenu.controller = controller;
            controls.controller = controller;
            mainMenu.visible = true;
            controls.visible = false;

            EnsureEventSystem();
            EnsureCanvas();

            EditorSceneManager.SaveScene(scene, BootScenePath);
        }

        static void ConfigureStage16SceneDefaults()
        {
            var scene = EditorSceneManager.OpenScene(Stage16SceneCreator.ScenePath);
            if (!scene.IsValid())
                throw new InvalidOperationException("Stage 16 scene did not open for player build flow configuration.");

            var game = GameObject.Find("RtsGame");
            if (game == null)
                throw new InvalidOperationException("Stage 16 scene is missing RtsGame.");

            var driver = GetOrAdd<RtsSimulationDriver>(game);
            driver.UseVerticalSliceDemoWorld = true;
            driver.UsePlayerPerspectiveSnapshot = true;

            var initializer = GetOrAdd<PlayerBuildSceneInitializer>(game);
            initializer.frameCameraOnStart = true;
            initializer.startScenarioOnLoad = true;
            initializer.hideDebugPanelsOnStart = true;
            initializer.cancelPlacementOnStart = true;

            var debugVisibility = GetOrAdd<DebugHudVisibilityController>(game);
            debugVisibility.showDebugPanelsByDefault = false;
            debugVisibility.hideDebugPanelsOnStart = true;
            debugVisibility.keepPlacementPanelsSynced = true;
            debugVisibility.ApplyPlayerFacingDefaults();

            var controller = GetOrAdd<VerticalSliceScenarioController>(game);
            controller.startOnInitialize = true;
            controller.resetWorldOnInitialize = true;

            var objectiveHud = GetOrAdd<MatchObjectiveHud>(game);
            objectiveHud.visible = true;
            objectiveHud.showDebugActions = false;

            var systemsHud = GetOrAdd<IntegratedSystemsStatusHud>(game);
            systemsHud.visible = false;

            var bootstrapper = GetOrAdd<RtsGameBootstrapper>(game);
            bootstrapper.startPaused = false;

            var boardPlacement = UnityEngine.Object.FindFirstObjectByType<BoardPlacementController>();
            if (boardPlacement != null)
                boardPlacement.SetPlacementMode(false);

            ConfigureCamera(Camera.main != null ? Camera.main : UnityEngine.Object.FindFirstObjectByType<Camera>());
            EditorSceneManager.SaveScene(scene, Stage16SceneCreator.ScenePath);
        }

        static void ConfigureCamera(Camera camera)
        {
            if (camera == null)
                return;

            camera.orthographic = true;
            camera.orthographicSize = 28f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 1000f;
            camera.transform.position = new Vector3(16f, 38f, -26f);
            camera.transform.rotation = Quaternion.Euler(60f, 0f, 0f);
            if (UnityEngine.Object.FindFirstObjectByType<AudioListener>() == null)
                camera.gameObject.AddComponent<AudioListener>();
        }

        static void ConfigureBuildSettings()
        {
            var paths = new[]
            {
                BootScenePath,
                Stage16SceneCreator.ScenePath,
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
                Stage15SceneCreator.ScenePath
            };

            var scenes = new List<EditorBuildSettingsScene>();
            for (var i = 0; i < paths.Length; i++)
                if (File.Exists(paths[i]) && !ContainsScene(scenes, paths[i]))
                    scenes.Add(new EditorBuildSettingsScene(paths[i], true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        static void EnsureEventSystem()
        {
            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        static void EnsureCanvas()
        {
            var canvasObject = new GameObject("Boot Canvas");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<GraphicRaycaster>();
            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
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

        static string GetCommandLineArgument(string name)
        {
            var arguments = Environment.GetCommandLineArgs();
            for (var i = 0; i < arguments.Length - 1; i++)
                if (string.Equals(arguments[i], name, StringComparison.OrdinalIgnoreCase))
                    return arguments[i + 1];
            return string.Empty;
        }
    }
}
