using System;
using System.IO;
using ProjectAegisRTS.UnityClient.Boot;
using ProjectAegisRTS.UnityClient.Bootstrap;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Rendering;
using ProjectAegisRTS.UnityClient.Scenario;
using ProjectAegisRTS.UnityClient.UI.Common;
using ProjectAegisRTS.UnityClient.UI.Desktop;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage19SceneValidator
    {
        public static void ValidateStage19SceneBatch()
        {
            try
            {
                ValidateStage19Scene();
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

        public static void ValidateStage19Scene()
        {
            Stage18_5FineGridValidator.ValidateStage18_5FineGrid();
            ValidateBuildSettings();
            ValidateBootScene();
            ValidateStage16Scene();
            ValidateMediumAuditScript();
            Debug.Log("Stage 19 scene validation passed.");
        }

        static void ValidateBuildSettings()
        {
            var scenes = EditorBuildSettings.scenes;
            if (scenes.Length < 2 || !scenes[0].enabled || scenes[0].path != Stage16_5BuildFlowConfigurator.BootScenePath)
                throw new InvalidOperationException("Stage 19 requires the boot scene first in Build Settings.");
            if (!scenes[1].enabled || scenes[1].path != Stage16SceneCreator.ScenePath)
                throw new InvalidOperationException("Stage 19 requires Stage16 as the second Build Settings scene.");
        }

        static void ValidateBootScene()
        {
            var scene = EditorSceneManager.OpenScene(Stage16_5BuildFlowConfigurator.BootScenePath);
            if (!scene.IsValid())
                throw new InvalidOperationException("Stage 19 boot scene did not open.");

            var menu = Require<MainMenuHud>("MainMenuHud");
            var controls = Require<ControlsHelpHud>("ControlsHelpHud");
            var options = Require<OptionsMenuHud>("OptionsMenuHud");
            Require<GameBootController>("GameBootController");

            if (!menu.visible || controls.visible || options.visible)
                throw new InvalidOperationException("Stage 19 boot menu visibility defaults are incorrect.");
        }

        static void ValidateStage16Scene()
        {
            var scene = EditorSceneManager.OpenScene(Stage16SceneCreator.ScenePath);
            if (!scene.IsValid())
                throw new InvalidOperationException("Stage 19 Stage16 scene did not open.");

            var bootstrapper = Require<RtsGameBootstrapper>("RtsGameBootstrapper");
            var progress = Require<VerticalSliceProgressTracker>("VerticalSliceProgressTracker");
            var missionFlow = Require<VerticalSliceMissionFlowController>("VerticalSliceMissionFlowController");
            var checklist = Require<VerticalSliceChecklistHud>("VerticalSliceChecklistHud");
            var promptSystem = Require<PlayerPromptSystem>("PlayerPromptSystem");
            var promptHud = Require<PlayerPromptHud>("PlayerPromptHud");
            var objectiveHud = Require<PlayerObjectiveHud>("PlayerObjectiveHud");
            var resultHud = Require<MatchResultHud>("MatchResultHud");
            var debugVisibility = Require<DebugHudVisibilityController>("DebugHudVisibilityController");
            var desktopHud = Require<DesktopRtsHudRoot>("DesktopRtsHudRoot");
            var statusLog = Require<RtsStatusLog>("RtsStatusLog");
            var mapper = Require<BoardCoordinateMapper>("BoardCoordinateMapper");
            var renderer = Require<BoardRenderer>("BoardRenderer");
            Require<ProductionGridController>("ProductionGridController");
            Require<ProductionCategoryTabs>("ProductionCategoryTabs");
            Require<ProductionQueuePanel>("ProductionQueuePanel");
            Require<PlacementModePanel>("PlacementModePanel");

            if (bootstrapper.verticalSliceMissionFlowController != missionFlow ||
                bootstrapper.verticalSliceProgressTracker != progress ||
                bootstrapper.verticalSliceChecklistHud != checklist ||
                bootstrapper.playerPromptSystem != promptSystem ||
                bootstrapper.playerPromptHud != promptHud ||
                bootstrapper.playerObjectiveHud != objectiveHud ||
                bootstrapper.matchResultHud != resultHud)
                throw new InvalidOperationException("Stage 19 bootstrapper player guidance references are incomplete.");

            if (missionFlow.progressTracker != progress)
                throw new InvalidOperationException("Stage 19 mission flow must reference the active progress tracker.");
            if (checklist.missionFlowController != missionFlow || promptSystem.missionFlowController != missionFlow)
                throw new InvalidOperationException("Stage 19 HUD guidance must reference the mission flow controller.");
            if (desktopHud.progressTracker != null && desktopHud.progressTracker != progress)
                throw new InvalidOperationException("Stage 19 desktop HUD references a different progress tracker.");
            if (desktopHud.missionFlowController != null && desktopHud.missionFlowController != missionFlow)
                throw new InvalidOperationException("Stage 19 desktop HUD references a different mission flow controller.");
            if (!checklist.visible || !promptSystem.visible || !promptHud.visible || !objectiveHud.visible || !resultHud.visible)
                throw new InvalidOperationException("Stage 19 player guidance HUD defaults are incorrect.");
            mapper.Configure(bootstrapper.boardWidth, bootstrapper.boardHeight, bootstrapper.boardCellSizeMeters, bootstrapper.boardRoot);
            if (mapper.PlacementGridScale != 2)
                throw new InvalidOperationException("Stage 19 fine placement grid scale must remain 2.");
            if (renderer.FineGridLineCount < 0)
                throw new InvalidOperationException("Stage 19 board renderer fine-grid metadata is invalid.");
            if (desktopHud.showDebugOverlay || statusLog.visible || statusLog.gameObject.activeInHierarchy)
                throw new InvalidOperationException("Stage 19 desktop status log/debug overlay is visible by default.");
            if (!debugVisibility.AreDebugPanelsHiddenByDefault())
                throw new InvalidOperationException("Stage 19 debug panels are visible by default.");
            if (!debugVisibility.IsPlacementUiHiddenByDefault())
                throw new InvalidOperationException("Stage 19 placement UI is visible by default.");
            if (!debugVisibility.IsPlayerHudVisible())
                throw new InvalidOperationException("Stage 19 player HUD is not visible by default.");
        }

        static void ValidateMediumAuditScript()
        {
            var repoRoot = Path.GetFullPath(Path.Combine(Application.dataPath, "..", ".."));
            var auditScript = Path.Combine(repoRoot, "tools", "audit-medium-validation-recursion.ps1");
            if (!File.Exists(auditScript))
                throw new InvalidOperationException("Stage 19 medium recursion audit script is missing.");

            var content = File.ReadAllText(auditScript);
            if (!content.Contains("run-stage19-medium-checks.ps1") ||
                !content.Contains("run-unity-stage19-validation.ps1"))
                throw new InvalidOperationException("Stage 19 medium recursion audit does not include Stage 19.");
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
