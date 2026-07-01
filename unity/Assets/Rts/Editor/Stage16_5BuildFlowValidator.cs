using System;
using ProjectAegisRTS.UnityClient.Boot;
using ProjectAegisRTS.UnityClient.UI.Common;
using ProjectAegisRTS.UnityClient.UI.Desktop;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage16_5BuildFlowValidator
    {
        public static void ValidateBuildFlowBatch()
        {
            try
            {
                ValidateBuildFlow();
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

        public static void ValidateBuildFlow()
        {
            if (!System.IO.File.Exists(Stage16_5BuildFlowConfigurator.BootScenePath))
                throw new InvalidOperationException("Stage 16.5 boot scene missing: " + Stage16_5BuildFlowConfigurator.BootScenePath);
            if (!System.IO.File.Exists(Stage16SceneCreator.ScenePath))
                throw new InvalidOperationException("Stage 16 scene missing: " + Stage16SceneCreator.ScenePath);

            ValidateBuildSettings();
            ValidateBootScene();
            ValidateStage16PlayerDefaults();
            Debug.Log("Stage 16.5 build flow validation passed.");
        }

        static void ValidateBuildSettings()
        {
            var scenes = EditorBuildSettings.scenes;
            var firstEnabled = -1;
            var bootEnabled = -1;
            var stage16Enabled = -1;

            for (var i = 0; i < scenes.Length; i++)
            {
                if (!scenes[i].enabled)
                    continue;
                if (firstEnabled < 0)
                    firstEnabled = i;
                if (scenes[i].path == Stage16_5BuildFlowConfigurator.BootScenePath)
                    bootEnabled = i;
                if (scenes[i].path == Stage16SceneCreator.ScenePath)
                    stage16Enabled = i;
            }

            if (bootEnabled < 0)
                throw new InvalidOperationException("Stage 16.5 boot scene is not enabled in Build Settings.");
            if (stage16Enabled < 0)
                throw new InvalidOperationException("Stage 16 scene is not enabled in Build Settings.");
            if (bootEnabled != firstEnabled)
                throw new InvalidOperationException("Boot scene must be the first enabled Build Settings scene.");
            if (stage16Enabled != bootEnabled + 1)
                throw new InvalidOperationException("Stage 16 scene must be the second enabled Build Settings scene.");
        }

        static void ValidateBootScene()
        {
            var scene = EditorSceneManager.OpenScene(Stage16_5BuildFlowConfigurator.BootScenePath);
            if (!scene.IsValid())
                throw new InvalidOperationException("Stage 16.5 boot scene did not open.");

            var controller = Require<GameBootController>("GameBootController");
            var menu = Require<MainMenuHud>("MainMenuHud");
            Require<ControlsHelpHud>("ControlsHelpHud");
            var options = Require<OptionsMenuHud>("OptionsMenuHud");
            var settings = Require<BuildModeSettings>("BuildModeSettings");

            if (controller.verticalSliceSceneName != "Stage16_PlayableVerticalSlice")
                throw new InvalidOperationException("GameBootController must load Stage16_PlayableVerticalSlice.");
            if (!menu.visible)
                throw new InvalidOperationException("Main menu must be visible by default.");
            if (options.visible)
                throw new InvalidOperationException("Options menu must be hidden by default.");
            if (settings.showDebugPanelsByDefault || !settings.startInBootMenu || !settings.defaultCleanHud || !settings.enableDeveloperHotkeys)
                throw new InvalidOperationException("Stage 16.5 build mode settings do not match player-build defaults.");
        }

        static void ValidateStage16PlayerDefaults()
        {
            Stage16SceneValidator.ValidateStage16Scene();

            var debugVisibility = Require<DebugHudVisibilityController>("DebugHudVisibilityController");
            var initializer = Require<PlayerBuildSceneInitializer>("PlayerBuildSceneInitializer");
            var objectiveHud = Require<MatchObjectiveHud>("MatchObjectiveHud");
            var playerObjectiveHud = Require<PlayerObjectiveHud>("PlayerObjectiveHud");
            var playerPromptHud = Require<PlayerPromptHud>("PlayerPromptHud");
            var controlsOverlay = Require<PlayerControlsOverlay>("PlayerControlsOverlay");
            var matchResultHud = Require<MatchResultHud>("MatchResultHud");
            var systemsHud = Require<IntegratedSystemsStatusHud>("IntegratedSystemsStatusHud");
            Require<DesktopRtsHudRoot>("DesktopRtsHudRoot");

            if (debugVisibility.showDebugPanelsByDefault)
                throw new InvalidOperationException("Stage 16 debug panels are not hidden by default.");
            if (!initializer.hideDebugPanelsOnStart || !initializer.cancelPlacementOnStart || !initializer.frameCameraOnStart)
                throw new InvalidOperationException("Stage 16 player initializer is missing player-facing defaults.");
            if (!objectiveHud.visible || objectiveHud.showDebugActions)
                throw new InvalidOperationException("Stage 16 objective HUD/default debug action state is incorrect.");
            if (!playerObjectiveHud.visible || !playerPromptHud.visible || controlsOverlay.visible || !matchResultHud.visible)
                throw new InvalidOperationException("Stage 16 player HUD/default overlay state is incorrect.");
            if (systemsHud.visible)
                throw new InvalidOperationException("Stage 16 integrated systems debug HUD is visible by default.");
            if (!debugVisibility.AreDebugPanelsHiddenByDefault())
                throw new InvalidOperationException("Stage 16 debug panels are not hidden by the visibility controller.");
            if (!debugVisibility.IsPlayerHudVisible())
                throw new InvalidOperationException("Stage 16 player HUD is not visible according to the visibility controller.");
            if (AnyPlacementPanelActive())
                throw new InvalidOperationException("Stage 16 placement UI must be hidden by default.");
            if (!debugVisibility.IsPlacementUiHiddenByDefault())
                throw new InvalidOperationException("Stage 16 placement UI is not hidden according to the visibility controller.");
        }

        static bool AnyPlacementPanelActive()
        {
            var behaviours = Resources.FindObjectsOfTypeAll<MonoBehaviour>();
            for (var i = 0; i < behaviours.Length; i++)
            {
                var behaviour = behaviours[i];
                if (behaviour == null || behaviour.gameObject == null || !behaviour.gameObject.scene.IsValid())
                    continue;

                var name = behaviour.GetType().Name;
                if ((name == "BoardPlacementHud" || name == "PlacementModePanel" || name == "LeftHandPlacementPanel") && behaviour.gameObject.activeInHierarchy)
                    return true;
            }

            return false;
        }

        static T Require<T>(string label) where T : Component
        {
            var active = UnityEngine.Object.FindFirstObjectByType<T>();
            if (active != null)
                return active;

            var all = Resources.FindObjectsOfTypeAll<T>();
            for (var i = 0; i < all.Length; i++)
                if (all[i] != null && all[i].gameObject != null && all[i].gameObject.scene.IsValid())
                    return all[i];

            throw new InvalidOperationException("Missing component: " + label);
        }
    }
}
