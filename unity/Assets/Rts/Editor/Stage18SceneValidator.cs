using System;
using ProjectAegisRTS.UnityClient.Boot;
using ProjectAegisRTS.UnityClient.Bootstrap;
using ProjectAegisRTS.UnityClient.Scenario;
using ProjectAegisRTS.UnityClient.UI.Common;
using ProjectAegisRTS.UnityClient.UI.Desktop;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage18SceneValidator
    {
        public static void ValidateStage18SceneBatch()
        {
            try
            {
                ValidateStage18Scene();
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

        public static void ValidateStage18Scene()
        {
            Stage17SceneValidator.ValidateStage17Scene();
            ValidateBuildSettings();
            ValidateBootScene();
            ValidateStage16Scene();
            Debug.Log("Stage 18 scene validation passed.");
        }

        static void ValidateBuildSettings()
        {
            var scenes = EditorBuildSettings.scenes;
            if (scenes.Length < 2 || !scenes[0].enabled || scenes[0].path != Stage16_5BuildFlowConfigurator.BootScenePath)
                throw new InvalidOperationException("Stage 18 requires the boot scene first in Build Settings.");
            if (!scenes[1].enabled || scenes[1].path != Stage16SceneCreator.ScenePath)
                throw new InvalidOperationException("Stage 18 requires Stage16 as the second Build Settings scene.");
        }

        static void ValidateBootScene()
        {
            var scene = EditorSceneManager.OpenScene(Stage16_5BuildFlowConfigurator.BootScenePath);
            if (!scene.IsValid())
                throw new InvalidOperationException("Stage 18 boot scene did not open.");

            var menu = Require<MainMenuHud>("MainMenuHud");
            var controls = Require<ControlsHelpHud>("ControlsHelpHud");
            var options = Require<OptionsMenuHud>("OptionsMenuHud");
            Require<GameBootController>("GameBootController");

            if (!menu.visible || controls.visible || options.visible)
                throw new InvalidOperationException("Stage 18 boot menu visibility defaults are incorrect.");
        }

        static void ValidateStage16Scene()
        {
            var scene = EditorSceneManager.OpenScene(Stage16SceneCreator.ScenePath);
            if (!scene.IsValid())
                throw new InvalidOperationException("Stage 18 Stage16 scene did not open.");

            var bootstrapper = Require<RtsGameBootstrapper>("RtsGameBootstrapper");
            var progress = Require<VerticalSliceProgressTracker>("VerticalSliceProgressTracker");
            var checklist = Require<VerticalSliceChecklistHud>("VerticalSliceChecklistHud");
            var promptSystem = Require<PlayerPromptSystem>("PlayerPromptSystem");
            var promptHud = Require<PlayerPromptHud>("PlayerPromptHud");
            var objectiveHud = Require<PlayerObjectiveHud>("PlayerObjectiveHud");
            var resultHud = Require<MatchResultHud>("MatchResultHud");
            var debugVisibility = Require<DebugHudVisibilityController>("DebugHudVisibilityController");
            var desktopHud = Require<DesktopRtsHudRoot>("DesktopRtsHudRoot");
            var statusLog = Require<RtsStatusLog>("RtsStatusLog");
            Require<ProductionGridController>("ProductionGridController");
            Require<ProductionCategoryTabs>("ProductionCategoryTabs");
            Require<ProductionQueuePanel>("ProductionQueuePanel");
            Require<PlacementModePanel>("PlacementModePanel");

            if (bootstrapper.verticalSliceProgressTracker != progress ||
                bootstrapper.verticalSliceChecklistHud != checklist ||
                bootstrapper.playerPromptSystem != promptSystem ||
                bootstrapper.playerPromptHud != promptHud ||
                bootstrapper.playerObjectiveHud != objectiveHud ||
                bootstrapper.matchResultHud != resultHud)
                throw new InvalidOperationException("Stage 18 bootstrapper player guidance references are incomplete.");

            if (desktopHud.progressTracker != null && desktopHud.progressTracker != progress)
                throw new InvalidOperationException("Stage 18 desktop HUD references a different progress tracker.");
            if (!checklist.visible || !promptSystem.visible || !promptHud.visible || !objectiveHud.visible || !resultHud.visible)
                throw new InvalidOperationException("Stage 18 player guidance HUD defaults are incorrect.");
            if (objectiveHud.area != PlayerHudLayout.ObjectiveArea ||
                checklist.area != PlayerHudLayout.ChecklistArea ||
                promptHud.area != PlayerHudLayout.PromptArea)
                throw new InvalidOperationException("Stage 18 player guidance HUD layout defaults are incorrect.");
            if (!LayoutIsSeparated(objectiveHud.area, promptHud.area, checklist.area))
                throw new InvalidOperationException("Stage 18 player HUD layout overlaps.");
            if (desktopHud.showDebugOverlay || statusLog.visible || statusLog.gameObject.activeInHierarchy)
                throw new InvalidOperationException("Stage 18 desktop status log/debug overlay is visible by default.");
            if (!debugVisibility.AreDebugPanelsHiddenByDefault())
                throw new InvalidOperationException("Stage 18 debug panels are visible by default.");
            if (!debugVisibility.IsPlacementUiHiddenByDefault())
                throw new InvalidOperationException("Stage 18 placement UI is visible by default.");
            if (!debugVisibility.IsPlayerHudVisible())
                throw new InvalidOperationException("Stage 18 player HUD is not visible by default.");
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

        static bool LayoutIsSeparated(Rect objective, Rect prompt, Rect checklist)
        {
            return !objective.Overlaps(prompt) &&
                !objective.Overlaps(checklist) &&
                !prompt.Overlaps(checklist);
        }
    }
}
