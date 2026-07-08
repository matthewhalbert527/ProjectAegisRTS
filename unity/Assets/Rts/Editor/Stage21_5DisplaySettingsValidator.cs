using System;
using ProjectAegisRTS.UnityClient.Boot;
using ProjectAegisRTS.UnityClient.UI.Common;
using ProjectAegisRTS.UnityClient.UI.Desktop;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage21_5DisplaySettingsValidator
    {
        public static void ValidateDisplaySettingsBatch()
        {
            try
            {
                ValidateDisplaySettings();
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

        public static void ValidateDisplaySettings()
        {
            ValidatePlayerSettings();
            ValidateBuildSettings();
            ValidateBootScene();
            ValidateStage16Scene();
            Stage4SceneValidator.ValidateStage4Scene();
            Stage5SceneValidator.ValidateStage5Scene();
            Debug.Log("Stage 21.5 display settings validation passed.");
        }

        static void ValidatePlayerSettings()
        {
            if (PlayerSettings.defaultIsNativeResolution)
                throw new InvalidOperationException("Standalone player must not use native resolution as the only default; Stage 21.5 needs a usable window default.");
            if (PlayerSettings.defaultScreenWidth < Stage21_5DisplaySettingsConfigurator.DefaultWindowWidth || PlayerSettings.defaultScreenHeight < Stage21_5DisplaySettingsConfigurator.DefaultWindowHeight)
                throw new InvalidOperationException("Standalone player default resolution is below the Stage 21.5 default.");
            if (PlayerSettings.fullScreenMode != FullScreenMode.Windowed && PlayerSettings.fullScreenMode != FullScreenMode.FullScreenWindow)
                throw new InvalidOperationException("Standalone player fullscreen mode must be Windowed or FullScreenWindow.");
            if (!PlayerSettings.resizableWindow)
                throw new InvalidOperationException("Standalone player window should be resizable for the PC prototype.");
        }

        static void ValidateBuildSettings()
        {
            var scenes = EditorBuildSettings.scenes;
            if (scenes.Length < 2)
                throw new InvalidOperationException("Build Settings must include Boot and Stage16 scenes.");
            if (scenes[0].path != Stage16_5BuildFlowConfigurator.BootScenePath)
                throw new InvalidOperationException("Boot scene must remain first in Build Settings.");
            if (scenes[1].path != Stage16SceneCreator.ScenePath)
                throw new InvalidOperationException("Stage16 scene must remain second in Build Settings.");

            RequireBuildScene("Assets/Rts/Scenes/Stage20_MvpProductionVisuals.unity");
            RequireBuildScene("Assets/Rts/Scenes/Stage21_MvpVisualQaReview.unity");
        }

        static void ValidateBootScene()
        {
            var scene = EditorSceneManager.OpenScene(Stage16_5BuildFlowConfigurator.BootScenePath);
            if (!scene.IsValid())
                throw new InvalidOperationException("Boot scene did not open for Stage 21.5 validation.");

            var display = Require<PlayerDisplaySettings>("PlayerDisplaySettings");
            ValidateDisplayComponent(display);
            var initializer = Require<PlayerDisplaySettingsInitializer>("PlayerDisplaySettingsInitializer");
            if (initializer.settings != display)
                throw new InvalidOperationException("Boot PlayerDisplaySettingsInitializer must reference PlayerDisplaySettings.");

            var options = Require<OptionsMenuHud>("OptionsMenuHud");
            if (!options.displaySectionEnabled)
                throw new InvalidOperationException("Options menu display section must be enabled.");
            if (options.displaySettings != display)
                throw new InvalidOperationException("Options menu must reference PlayerDisplaySettings.");
            if (options.area.width < 540f || options.area.height < 520f)
                throw new InvalidOperationException("Options menu area is too small for display controls.");

            ValidateSceneCanvases("Boot");
        }

        static void ValidateStage16Scene()
        {
            var scene = EditorSceneManager.OpenScene(Stage16SceneCreator.ScenePath);
            if (!scene.IsValid())
                throw new InvalidOperationException("Stage16 scene did not open for Stage 21.5 validation.");

            var display = Require<PlayerDisplaySettings>("PlayerDisplaySettings");
            ValidateDisplayComponent(display);
            var initializer = Require<PlayerDisplaySettingsInitializer>("PlayerDisplaySettingsInitializer");
            if (initializer.settings != display)
                throw new InvalidOperationException("Stage16 PlayerDisplaySettingsInitializer must reference PlayerDisplaySettings.");
            Require<PlayerBuildSceneInitializer>("PlayerBuildSceneInitializer");
            Require<DesktopRtsHudRoot>("DesktopRtsHudRoot");
            Require<CncStyleSidebarLayout>("CncStyleSidebarLayout");
            ValidateSceneCanvases("Stage16");
        }

        static void ValidateDisplayComponent(PlayerDisplaySettings display)
        {
            if (display.defaultWindowWidth < Stage21_5DisplaySettingsConfigurator.DefaultWindowWidth || display.defaultWindowHeight < Stage21_5DisplaySettingsConfigurator.DefaultWindowHeight)
                throw new InvalidOperationException("PlayerDisplaySettings default resolution must be at least 1600x900.");
            if (display.minimumWindowWidth < Stage21_5DisplaySettingsConfigurator.MinimumWindowWidth || display.minimumWindowHeight < Stage21_5DisplaySettingsConfigurator.MinimumWindowHeight)
                throw new InvalidOperationException("PlayerDisplaySettings minimum resolution must be at least 1280x720.");
            if (display.preferredFullscreenMode != FullScreenMode.Windowed && display.preferredFullscreenMode != FullScreenMode.FullScreenWindow)
                throw new InvalidOperationException("PlayerDisplaySettings preferred mode must be Windowed or FullScreenWindow.");
            if (!display.logStartupDisplayMetrics)
                throw new InvalidOperationException("PlayerDisplaySettings must log startup display metrics.");
        }

        static void ValidateSceneCanvases(string label)
        {
            var canvases = UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (canvases.Length == 0)
                throw new InvalidOperationException(label + " scene must have at least one Canvas.");

            for (var i = 0; i < canvases.Length; i++)
            {
                var canvas = canvases[i];
                var scaler = canvas.GetComponent<CanvasScaler>();
                if (scaler == null)
                    throw new InvalidOperationException(label + " canvas is missing CanvasScaler: " + canvas.name);
                if (scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize)
                    throw new InvalidOperationException(label + " canvas must use Scale With Screen Size: " + canvas.name);
                if (scaler.referenceResolution != new Vector2(1920f, 1080f))
                    throw new InvalidOperationException(label + " canvas reference resolution must be 1920x1080: " + canvas.name);
                if (Mathf.Abs(scaler.matchWidthOrHeight - 0.5f) > 0.001f)
                    throw new InvalidOperationException(label + " canvas match width/height must be 0.5: " + canvas.name);
                if (canvas.GetComponent<ResponsiveCanvasScalerEnforcer>() == null)
                    throw new InvalidOperationException(label + " canvas is missing ResponsiveCanvasScalerEnforcer: " + canvas.name);
            }
        }

        static void RequireBuildScene(string path)
        {
            var scenes = EditorBuildSettings.scenes;
            for (var i = 0; i < scenes.Length; i++)
                if (scenes[i].path == path && scenes[i].enabled)
                    return;
            throw new InvalidOperationException("Build Settings missing enabled scene: " + path);
        }

        static T Require<T>(string label) where T : Component
        {
            var component = UnityEngine.Object.FindFirstObjectByType<T>();
            if (component == null)
            {
                var all = Resources.FindObjectsOfTypeAll<T>();
                for (var i = 0; i < all.Length; i++)
                {
                    if (all[i] != null && all[i].gameObject != null && all[i].gameObject.scene.IsValid())
                    {
                        component = all[i];
                        break;
                    }
                }
            }
            if (component == null)
                throw new InvalidOperationException("Missing component: " + label);
            return component;
        }
    }
}
