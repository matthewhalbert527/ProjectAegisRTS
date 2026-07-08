using System;
using System.Collections.Generic;
using System.IO;
using ProjectAegisRTS.UnityClient.Boot;
using ProjectAegisRTS.UnityClient.UI.Common;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage21_5DisplaySettingsConfigurator
    {
        public const int DefaultWindowWidth = 1600;
        public const int DefaultWindowHeight = 900;
        public const int MinimumWindowWidth = 1280;
        public const int MinimumWindowHeight = 720;
        const string WindowWidthArgument = "-stage21_5WindowWidth";
        const string WindowHeightArgument = "-stage21_5WindowHeight";
        const string FullscreenModeArgument = "-stage21_5FullscreenMode";

        [MenuItem("ProjectAegisRTS/Stage 21.5/Configure Display Settings")]
        public static void ConfigureDisplaySettingsMenu()
        {
            ConfigureDisplaySettings();
        }

        public static void ConfigureDisplaySettingsBatch()
        {
            try
            {
                ConfigureDisplaySettings();
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

        public static void ConfigureDisplaySettings()
        {
            var width = Mathf.Max(GetIntArgument(WindowWidthArgument, DefaultWindowWidth), MinimumWindowWidth);
            var height = Mathf.Max(GetIntArgument(WindowHeightArgument, DefaultWindowHeight), MinimumWindowHeight);
            var mode = GetFullscreenModeArgument(FullscreenModeArgument, FullScreenMode.Windowed);

            Stage16_5BuildFlowConfigurator.ConfigureBuildFlow();
            ConfigureStandalonePlayerSettings(width, height, mode);
            ConfigureBootScene(width, height, mode);
            ConfigureStage16Scene(width, height, mode);
            ConfigureBuildSettings();
            AssetDatabase.SaveAssets();
            Debug.Log("Stage 21.5 display settings configured. default=" + width + "x" + height + " mode=" + mode + " minimum=" + MinimumWindowWidth + "x" + MinimumWindowHeight + ".");
        }

        static void ConfigureStandalonePlayerSettings(int width, int height, FullScreenMode mode)
        {
            PlayerSettings.defaultScreenWidth = width;
            PlayerSettings.defaultScreenHeight = height;
            PlayerSettings.defaultIsNativeResolution = false;
            PlayerSettings.fullScreenMode = mode;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.runInBackground = true;
        }

        static void ConfigureBootScene(int width, int height, FullScreenMode mode)
        {
            var scene = EditorSceneManager.OpenScene(Stage16_5BuildFlowConfigurator.BootScenePath);
            if (!scene.IsValid())
                throw new InvalidOperationException("Boot scene did not open for Stage 21.5 display configuration.");

            var bootObject = GameObject.Find("Stage16_5 Boot");
            if (bootObject == null)
                throw new InvalidOperationException("Boot scene is missing Stage16_5 Boot object.");

            var display = GetOrAdd<PlayerDisplaySettings>(bootObject);
            ConfigureDisplayComponent(display, width, height, mode);
            var initializer = GetOrAdd<PlayerDisplaySettingsInitializer>(bootObject);
            initializer.settings = display;
            initializer.applyOnAwake = true;

            var options = GetOrAdd<OptionsMenuHud>(bootObject);
            options.displaySettings = display;
            options.displaySectionEnabled = true;
            options.area = new Rect(40f, 40f, 560f, 560f);

            EnforceSceneCanvases();
            EditorSceneManager.SaveScene(scene, Stage16_5BuildFlowConfigurator.BootScenePath);
        }

        static void ConfigureStage16Scene(int width, int height, FullScreenMode mode)
        {
            var scene = EditorSceneManager.OpenScene(Stage16SceneCreator.ScenePath);
            if (!scene.IsValid())
                throw new InvalidOperationException("Stage16 scene did not open for Stage 21.5 display configuration.");

            var game = GameObject.Find("RtsGame");
            if (game == null)
                throw new InvalidOperationException("Stage16 scene is missing RtsGame.");

            var display = GetOrAdd<PlayerDisplaySettings>(game);
            ConfigureDisplayComponent(display, width, height, mode);
            var initializer = GetOrAdd<PlayerDisplaySettingsInitializer>(game);
            initializer.settings = display;
            initializer.applyOnAwake = true;

            var playerInitializer = GetOrAdd<PlayerBuildSceneInitializer>(game);
            playerInitializer.frameCameraOnStart = true;
            playerInitializer.startScenarioOnLoad = true;
            playerInitializer.hideDebugPanelsOnStart = true;
            playerInitializer.cancelPlacementOnStart = true;
            playerInitializer.cameraOrthographicSize = 18f;

            EnforceSceneCanvases();
            EditorSceneManager.SaveScene(scene, Stage16SceneCreator.ScenePath);
        }

        static void ConfigureDisplayComponent(PlayerDisplaySettings display, int width, int height, FullScreenMode mode)
        {
            display.defaultWindowWidth = width;
            display.defaultWindowHeight = height;
            display.minimumWindowWidth = MinimumWindowWidth;
            display.minimumWindowHeight = MinimumWindowHeight;
            display.preferredFullscreenMode = mode;
            display.preserveValidPlayerPreferences = true;
            display.applyInEditor = false;
            display.logStartupDisplayMetrics = true;
        }

        static void EnforceSceneCanvases()
        {
            var canvases = UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (var i = 0; i < canvases.Length; i++)
            {
                if (canvases[i] == null)
                    continue;

                var enforcer = GetOrAdd<ResponsiveCanvasScalerEnforcer>(canvases[i].gameObject);
                enforcer.referenceResolution = new Vector2(1920f, 1080f);
                enforcer.matchWidthOrHeight = 0.5f;
                enforcer.enforceOnAwake = true;
                enforcer.enforceOnStart = true;
                enforcer.logAdjustments = true;
                ResponsiveCanvasScalerEnforcer.EnforceCanvas(canvases[i], enforcer.referenceResolution, enforcer.matchWidthOrHeight, false);
            }
        }

        static void ConfigureBuildSettings()
        {
            var paths = new[]
            {
                Stage16_5BuildFlowConfigurator.BootScenePath,
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
                Stage15SceneCreator.ScenePath,
                "Assets/Rts/Scenes/Stage20_MvpProductionVisuals.unity",
                "Assets/Rts/Scenes/Stage21_MvpVisualQaReview.unity"
            };

            var scenes = new List<EditorBuildSettingsScene>();
            for (var i = 0; i < paths.Length; i++)
                if (File.Exists(paths[i]) && !ContainsScene(scenes, paths[i]))
                    scenes.Add(new EditorBuildSettingsScene(paths[i], true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        static bool ContainsScene(List<EditorBuildSettingsScene> scenes, string path)
        {
            for (var i = 0; i < scenes.Count; i++)
                if (scenes[i].path == path)
                    return true;
            return false;
        }

        static T GetOrAdd<T>(GameObject target) where T : Component
        {
            var component = target.GetComponent<T>();
            return component != null ? component : target.AddComponent<T>();
        }

        static int GetIntArgument(string name, int fallback)
        {
            var args = Environment.GetCommandLineArgs();
            for (var i = 0; i < args.Length - 1; i++)
            {
                if (!string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
                    continue;

                int value;
                return int.TryParse(args[i + 1], out value) ? value : fallback;
            }

            return fallback;
        }

        static FullScreenMode GetFullscreenModeArgument(string name, FullScreenMode fallback)
        {
            var args = Environment.GetCommandLineArgs();
            for (var i = 0; i < args.Length - 1; i++)
            {
                if (!string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
                    continue;

                FullScreenMode mode;
                return Enum.TryParse(args[i + 1], true, out mode) ? mode : fallback;
            }

            return fallback;
        }
    }
}
