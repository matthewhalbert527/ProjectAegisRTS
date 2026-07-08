using System;
using System.IO;
using ProjectAegisRTS.UnityClient.Boot;
using ProjectAegisRTS.UnityClient.UI.Common;
using ProjectAegisRTS.UnityClient.UI.Desktop;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage19_5SidebarPauseValidator
    {
        public static void ValidateStage19_5SidebarPauseBatch()
        {
            try
            {
                ValidateStage19_5SidebarPause();
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

        public static void ValidateStage19_5SidebarPause()
        {
            Stage19SceneValidator.ValidateStage19Scene();
            ValidateBuildSettings();
            ValidateBootScene();
            ValidateStage16Scene();
            ValidateMediumAuditScript();
            Debug.Log("Stage 19.5 sidebar/pause validation passed.");
        }

        static void ValidateBuildSettings()
        {
            var scenes = EditorBuildSettings.scenes;
            if (scenes.Length < 2 || !scenes[0].enabled || scenes[0].path != Stage16_5BuildFlowConfigurator.BootScenePath)
                throw new InvalidOperationException("Stage 19.5 requires the boot scene first in Build Settings.");
            if (!scenes[1].enabled || scenes[1].path != Stage16SceneCreator.ScenePath)
                throw new InvalidOperationException("Stage 19.5 requires Stage16 as the second Build Settings scene.");
        }

        static void ValidateBootScene()
        {
            var scene = EditorSceneManager.OpenScene(Stage16_5BuildFlowConfigurator.BootScenePath);
            if (!scene.IsValid())
                throw new InvalidOperationException("Stage 19.5 boot scene did not open.");

            Require<MainMenuHud>("MainMenuHud");
            Require<ControlsHelpHud>("ControlsHelpHud");
            Require<OptionsMenuHud>("OptionsMenuHud");
            Require<GameBootController>("GameBootController");
        }

        static void ValidateStage16Scene()
        {
            var scene = EditorSceneManager.OpenScene(Stage16SceneCreator.ScenePath);
            if (!scene.IsValid())
                throw new InvalidOperationException("Stage 19.5 Stage16 scene did not open.");

            var desktopHud = Require<DesktopRtsHudRoot>("DesktopRtsHudRoot");
            var layout = Require<CncStyleSidebarLayout>("CncStyleSidebarLayout");
            var sidebar = Require<DesktopSidebarController>("DesktopSidebarController");
            var minimap = Require<MinimapPlaceholderController>("MinimapPlaceholderController");
            var tabs = Require<ProductionCategoryTabs>("ProductionCategoryTabs");
            var grid = Require<ProductionGridController>("ProductionGridController");
            var queue = Require<ProductionQueuePanel>("ProductionQueuePanel");
            var placement = Require<PlacementModePanel>("PlacementModePanel");
            var selection = Require<SelectionPanelController>("SelectionPanelController");
            var commandBar = Require<CommandBarController>("CommandBarController");
            var objective = Require<PlayerObjectiveHud>("PlayerObjectiveHud");
            var checklist = Require<VerticalSliceChecklistHud>("VerticalSliceChecklistHud");
            var pause = Require<PauseMenuController>("PauseMenuController");
            var pauseHud = Require<PauseMenuHud>("PauseMenuHud");
            var mode = Require<PlayerFacingUiModeController>("PlayerFacingUiModeController");
            var debugVisibility = Require<DebugHudVisibilityController>("DebugHudVisibilityController");

            desktopHud.cncSidebarLayout = layout;
            desktopHud.Initialize();
            layout.Initialize(desktopHud, sidebar, tabs, grid, queue, placement, selection, commandBar, minimap);
            mode.ApplyModeDefaults();
            debugVisibility.ApplyPlayerFacingDefaults();

            if (layout.rightSidebarRoot == null || !layout.rightSidebarRoot.gameObject.activeInHierarchy)
                throw new InvalidOperationException("Stage 19.5 right sidebar root is not visible.");
            if (!layout.AreProductionPanelsInRightSidebar())
                throw new InvalidOperationException("Stage 19.5 production, selection, and command panels must be parented under the right sidebar.");
            if (!layout.IsMinimapAboveProductionGrid())
                throw new InvalidOperationException("Stage 19.5 minimap must be above the production grid.");
            if (objective.transform.IsChildOf(layout.rightSidebarRoot) || checklist.transform.IsChildOf(layout.rightSidebarRoot))
                throw new InvalidOperationException("Stage 19.5 objective/checklist HUDs must not be the right production sidebar.");
            if (!mode.AreXrBuildMenusHiddenForPc())
                throw new InvalidOperationException("Stage 19.5 XR/left-hand build menus must be hidden in PC player-facing mode.");
            if (desktopHud.showDebugOverlay || !debugVisibility.AreDebugPanelsHiddenByDefault())
                throw new InvalidOperationException("Stage 19.5 debug panels must be hidden by default.");
            if (placement.gameObject.activeInHierarchy || !debugVisibility.IsPlacementUiHiddenByDefault())
                throw new InvalidOperationException("Stage 19.5 placement UI must be hidden before placement mode.");

            pause.hud = pauseHud;
            pauseHud.Initialize(pause);
            if (!pauseHud.HasRequiredButtons())
                throw new InvalidOperationException("Stage 19.5 pause menu is missing required buttons.");
            if (pauseHud.IsVisible)
                throw new InvalidOperationException("Stage 19.5 pause menu must start hidden.");
        }

        static void ValidateMediumAuditScript()
        {
            var repoRoot = Path.GetFullPath(Path.Combine(Application.dataPath, "..", ".."));
            var auditScript = Path.Combine(repoRoot, "tools", "audit-medium-validation-recursion.ps1");
            if (!File.Exists(auditScript))
                throw new InvalidOperationException("Stage 19.5 medium recursion audit script is missing.");

            var content = File.ReadAllText(auditScript);
            if (!content.Contains("run-stage19-5-medium-checks.ps1") ||
                !content.Contains("run-unity-stage19-5-validation.ps1"))
                throw new InvalidOperationException("Stage 19.5 medium recursion audit does not include Stage 19.5.");
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
