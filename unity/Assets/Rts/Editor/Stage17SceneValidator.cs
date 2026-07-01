using System;
using ProjectAegisRTS.UnityClient.Boot;
using ProjectAegisRTS.UnityClient.Bootstrap;
using ProjectAegisRTS.UnityClient.Scenario;
using ProjectAegisRTS.UnityClient.UI.Common;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage17SceneValidator
    {
        public static void ValidateStage17SceneBatch()
        {
            try
            {
                ValidateStage17Scene();
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

        public static void ValidateStage17Scene()
        {
            Stage16_5BuildFlowValidator.ValidateBuildFlow();
            ValidateBootPolish();
            ValidateStage16PlayerPolish();
            Debug.Log("Stage 17 scene validation passed.");
        }

        static void ValidateBootPolish()
        {
            var scene = EditorSceneManager.OpenScene(Stage16_5BuildFlowConfigurator.BootScenePath);
            if (!scene.IsValid())
                throw new InvalidOperationException("Stage 17 boot scene did not open.");

            var controller = Require<GameBootController>("GameBootController");
            var mainMenu = Require<MainMenuHud>("MainMenuHud");
            var controls = Require<ControlsHelpHud>("ControlsHelpHud");
            var options = Require<OptionsMenuHud>("OptionsMenuHud");
            var settings = Require<BuildModeSettings>("BuildModeSettings");

            if (controller.mainMenu != mainMenu || controller.controlsHelp != controls || controller.optionsMenu != options)
                throw new InvalidOperationException("Boot controller menu references are incomplete.");
            if (!mainMenu.visible || controls.visible || options.visible)
                throw new InvalidOperationException("Boot menu visibility defaults are incorrect.");
            if (settings.showDebugPanelsByDefault || !settings.startInBootMenu || !settings.defaultCleanHud)
                throw new InvalidOperationException("Stage 17 boot settings are not player-facing defaults.");
        }

        static void ValidateStage16PlayerPolish()
        {
            var scene = EditorSceneManager.OpenScene(Stage16SceneCreator.ScenePath);
            if (!scene.IsValid())
                throw new InvalidOperationException("Stage 17 Stage16 scene did not open.");

            var bootstrapper = Require<RtsGameBootstrapper>("RtsGameBootstrapper");
            var controller = Require<VerticalSliceScenarioController>("VerticalSliceScenarioController");
            var matchObjective = Require<MatchObjectiveHud>("MatchObjectiveHud");
            var objective = Require<PlayerObjectiveHud>("PlayerObjectiveHud");
            var prompt = Require<PlayerPromptHud>("PlayerPromptHud");
            var controls = Require<PlayerControlsOverlay>("PlayerControlsOverlay");
            var result = Require<MatchResultHud>("MatchResultHud");
            var debugVisibility = Require<DebugHudVisibilityController>("DebugHudVisibilityController");

            if (bootstrapper.verticalSliceScenarioController != controller ||
                bootstrapper.matchObjectiveHud != matchObjective ||
                bootstrapper.playerObjectiveHud != objective ||
                bootstrapper.playerPromptHud != prompt ||
                bootstrapper.playerControlsOverlay != controls ||
                bootstrapper.matchResultHud != result)
                throw new InvalidOperationException("Stage 17 bootstrapper player-facing HUD references are incomplete.");

            if (!matchObjective.visible || matchObjective.showDebugActions)
                throw new InvalidOperationException("Match objective HUD must be visible without debug actions.");
            if (!objective.visible || !prompt.visible || controls.visible || !result.visible)
                throw new InvalidOperationException("Player-facing HUD default visibility is incorrect.");
            if (!debugVisibility.AreDebugPanelsHiddenByDefault())
                throw new InvalidOperationException("Debug panels are visible by default.");
            if (!debugVisibility.IsPlacementUiHiddenByDefault())
                throw new InvalidOperationException("Placement UI is visible by default.");
            if (!debugVisibility.IsPlayerHudVisible())
                throw new InvalidOperationException("Player HUD is not visible by default.");
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
