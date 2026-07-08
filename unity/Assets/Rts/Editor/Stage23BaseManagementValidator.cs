using System;
using System.IO;
using ProjectAegisRTS.UnityClient.Bootstrap;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.InputControls;
using ProjectAegisRTS.UnityClient.UI.Common;
using ProjectAegisRTS.UnityClient.UI.Desktop;
using ProjectAegisRTS.UnityClient.UI.XR.LeftHand;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage23BaseManagementValidator
    {
        public static void ValidateStage23BaseManagementBatch()
        {
            try
            {
                ValidateStage23BaseManagement();
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

        public static void ValidateStage23BaseManagement()
        {
            Stage22CommandMatrixValidator.ValidateStage22CommandMatrix();
            ValidateStage16BaseManagementUi();
            ValidateMediumAuditScript();
            Debug.Log("Stage 23 base management validation passed.");
        }

        static void ValidateStage16BaseManagementUi()
        {
            var scene = EditorSceneManager.OpenScene(Stage16SceneCreator.ScenePath);
            if (!scene.IsValid())
                throw new InvalidOperationException("Stage 23 Stage16 scene did not open.");

            var bootstrapper = Require<RtsGameBootstrapper>("RtsGameBootstrapper");
            var driver = Require<RtsSimulationDriver>("RtsSimulationDriver");
            var input = Require<RtsDesktopInputController>("RtsDesktopInputController");
            var desktopHud = Require<DesktopRtsHudRoot>("DesktopRtsHudRoot");
            var layout = Require<CncStyleSidebarLayout>("CncStyleSidebarLayout");
            var commandBar = Require<CommandBarController>("CommandBarController");
            var placementPanel = Require<PlacementModePanel>("PlacementModePanel");
            var debugVisibility = Require<DebugHudVisibilityController>("DebugHudVisibilityController");
            var leftRouter = Require<LeftHandCommandRouter>("LeftHandCommandRouter");

            bootstrapper.InitializeScene();
            desktopHud.Initialize();
            layout.ApplyLayout();

            if (driver.LatestSnapshot == null || driver.LatestSnapshot.Actors.Count == 0)
                throw new InvalidOperationException("Stage 23 needs a live runtime snapshot for base-management validation.");
            if (input == null || leftRouter == null)
                throw new InvalidOperationException("Stage 23 command routers are missing.");
            if (!layout.AreProductionPanelsInRightSidebar())
                throw new InvalidOperationException("Stage 23 command bar must remain part of the PC right-sidebar layout.");
            if (desktopHud.showDebugOverlay || !debugVisibility.AreDebugPanelsHiddenByDefault())
                throw new InvalidOperationException("Stage 23 debug panels must remain hidden by default.");
            if (placementPanel.gameObject.activeInHierarchy)
                throw new InvalidOperationException("Stage 23 placement UI must remain hidden until placement mode.");
            if (!Enum.IsDefined(typeof(DesktopCommandMode), DesktopCommandMode.Rally))
                throw new InvalidOperationException("Stage 23 rally command mode is missing.");

            ValidateCommandButtons(commandBar);
        }

        static void ValidateCommandButtons(CommandBarController commandBar)
        {
            var required = new[]
            {
                "Repair",
                "Sell",
                "Power",
                "Rally"
            };

            var buttons = commandBar.GetComponentsInChildren<Button>(true);
            for (var i = 0; i < required.Length; i++)
            {
                var found = false;
                for (var b = 0; b < buttons.Length; b++)
                {
                    if (buttons[b] != null && buttons[b].gameObject.name == required[i])
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                    throw new InvalidOperationException("Stage 23 command bar missing button: " + required[i]);
            }

            var grid = commandBar.GetComponentInChildren<GridLayoutGroup>(true);
            if (grid == null)
                throw new InvalidOperationException("Stage 23 command bar missing GridLayoutGroup.");
            if (grid.constraint != GridLayoutGroup.Constraint.FixedColumnCount || (grid.constraintCount != 4 && grid.constraintCount != 8))
                throw new InvalidOperationException("Stage 23 command bar must use a compact fixed-column matrix.");
            if (grid.cellSize.x > 86f || grid.cellSize.y > 24f)
                throw new InvalidOperationException("Stage 23 command buttons are too large for the sidebar matrix.");
        }

        static void ValidateMediumAuditScript()
        {
            var repoRoot = Path.GetFullPath(Path.Combine(Application.dataPath, "..", ".."));
            var auditScript = Path.Combine(repoRoot, "tools", "audit-medium-validation-recursion.ps1");
            if (!File.Exists(auditScript))
                throw new InvalidOperationException("Stage 23 medium recursion audit script is missing.");

            var content = File.ReadAllText(auditScript);
            if (!content.Contains("run-stage23-medium-checks.ps1") ||
                !content.Contains("run-unity-stage23-validation.ps1"))
                throw new InvalidOperationException("Stage 23 medium recursion audit does not include Stage 23.");
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
