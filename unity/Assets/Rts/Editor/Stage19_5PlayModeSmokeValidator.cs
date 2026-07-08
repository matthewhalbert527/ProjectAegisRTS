using System;
using System.Collections.Generic;
using ProjectAegisRTS.Core;
using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.UnityClient.Boot;
using ProjectAegisRTS.UnityClient.Bootstrap;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Rendering;
using ProjectAegisRTS.UnityClient.UI.Common;
using ProjectAegisRTS.UnityClient.UI.Desktop;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage19_5PlayModeSmokeValidator
    {
        static readonly List<string> RedErrors = new List<string>();

        public static void RunStage19_5PlayModeSmokeBatch()
        {
            try
            {
                RunStage19_5PlayModeSmoke();
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

        public static void RunStage19_5PlayModeSmoke()
        {
            RedErrors.Clear();
            Application.logMessageReceived += CaptureRedError;
            try
            {
                Stage19_5SidebarPauseValidator.ValidateStage19_5SidebarPause();
                ValidateBootRuntime();
                ValidateStage16Runtime();

                if (RedErrors.Count > 0)
                    throw new InvalidOperationException("Red console errors were produced during Stage 19.5 smoke validation: " + string.Join(" | ", RedErrors.ToArray()));

                Debug.Log("Stage 19.5 play mode smoke validation passed.");
            }
            finally
            {
                Application.logMessageReceived -= CaptureRedError;
                RedErrors.Clear();
            }
        }

        static void ValidateBootRuntime()
        {
            var scene = EditorSceneManager.OpenScene(Stage16_5BuildFlowConfigurator.BootScenePath);
            if (!scene.IsValid())
                throw new InvalidOperationException("Stage 19.5 boot scene did not open for smoke validation.");

            var menu = RequireEnabled<MainMenuHud>("MainMenuHud");
            RequireEnabled<GameBootController>("GameBootController");
            if (!menu.visible)
                throw new InvalidOperationException("Stage 19.5 boot menu is not visible.");
        }

        static void ValidateStage16Runtime()
        {
            var scene = EditorSceneManager.OpenScene(Stage16SceneCreator.ScenePath);
            if (!scene.IsValid())
                throw new InvalidOperationException("Stage 19.5 Stage16 scene did not open for smoke validation.");

            var bootstrapper = RequireEnabled<RtsGameBootstrapper>("RtsGameBootstrapper");
            var driver = RequireEnabled<RtsSimulationDriver>("RtsSimulationDriver");
            var boardRenderer = RequireEnabled<BoardRenderer>("BoardRenderer");
            var actorRenderer = RequireEnabled<ActorRenderSystem>("ActorRenderSystem");
            var desktopHud = RequireEnabled<DesktopRtsHudRoot>("DesktopRtsHudRoot");
            var layout = RequireComponent<CncStyleSidebarLayout>("CncStyleSidebarLayout");
            var mode = RequireEnabled<PlayerFacingUiModeController>("PlayerFacingUiModeController");
            var pause = RequireEnabled<PauseMenuController>("PauseMenuController");
            var pauseHud = RequireComponent<PauseMenuHud>("PauseMenuHud");
            var debugVisibility = RequireEnabled<DebugHudVisibilityController>("DebugHudVisibilityController");
            var placementPanel = RequireComponent<PlacementModePanel>("PlacementModePanel");

            bootstrapper.InitializeScene();
            desktopHud.Initialize();
            layout.ApplyLayout();
            mode.ApplyModeDefaults();
            StepRuntime(driver, boardRenderer, actorRenderer, 6, 0.05f);

            if (!layout.AreProductionPanelsInRightSidebar() || !layout.IsMinimapAboveProductionGrid())
                throw new InvalidOperationException("Stage 19.5 right sidebar layout is not active in runtime smoke.");
            if (desktopHud.productionGrid == null || desktopHud.productionGrid.transform.childCount == 0)
                throw new InvalidOperationException("Stage 19.5 production cards were not generated on the right sidebar.");
            if (!mode.AreXrBuildMenusHiddenForPc())
                throw new InvalidOperationException("Stage 19.5 left-hand/XR build menu is active in PC runtime smoke.");
            if (!debugVisibility.AreDebugPanelsHiddenByDefault())
                throw new InvalidOperationException("Stage 19.5 debug panels are not hidden by default.");
            if (placementPanel.gameObject.activeInHierarchy)
                throw new InvalidOperationException("Stage 19.5 placement panel is visible before placement mode.");

            var snapshot = RequireSnapshot(driver);
            if (snapshot.Actors.Count == 0)
                throw new InvalidOperationException("Stage 19.5 expected visible actors/buildings.");
            if (snapshot.Economy == null || snapshot.Economy.Resources.Count == 0)
                throw new InvalidOperationException("Stage 19.5 expected visible resources.");

            ValidateSidebarBuildPath(driver, boardRenderer, actorRenderer);
            ValidatePauseMenu(driver, pause, pauseHud);
        }

        static void ValidateSidebarBuildPath(RtsSimulationDriver driver, BoardRenderer boardRenderer, ActorRenderSystem actorRenderer)
        {
            RequireSuccess(driver.TryQueueProduction("power_plant"), "queue power plant from sidebar route");
            StepUntilPendingPlacement(driver, boardRenderer, actorRenderer, "power_plant");
            RequireSuccess(driver.TryEnterPlacementModeForFirstPending(), "enter placement mode");

            var validCell = FindValidPlacementCell(driver, boardRenderer, actorRenderer);
            driver.SetHoveredCell(validCell, true);
            PlacementPreviewSnapshot preview;
            if (!driver.TryGetPlacementPreview(out preview) || !preview.CanPlace)
                throw new InvalidOperationException("Stage 19.5 expected placement preview after sidebar build path.");
            if (preview.FootprintCells.Count == 0)
                throw new InvalidOperationException("Stage 19.5 expected fine-grid footprint cells in placement preview.");

            RequireSuccess(driver.TryCancelPlacement(), "cancel placement after preview");
        }

        static void ValidatePauseMenu(RtsSimulationDriver driver, PauseMenuController pause, PauseMenuHud pauseHud)
        {
            pause.hud = pauseHud;
            pause.suppressSceneLoadsForValidation = true;
            pause.suppressApplicationQuitForValidation = true;
            pause.Initialize(driver, UnityEngine.Object.FindFirstObjectByType<ProjectAegisRTS.UnityClient.Scenario.VerticalSliceScenarioController>());

            pause.OpenPauseMenu();
            if (!pause.IsOpen || !pauseHud.IsVisible || !driver.IsPaused)
                throw new InvalidOperationException("Stage 19.5 pause menu did not open and pause simulation.");
            if (!pause.BlocksGameplayInput())
                throw new InvalidOperationException("Stage 19.5 pause menu does not block gameplay input.");

            pause.ShowSettings();
            if (!pauseHud.IsVisible)
                throw new InvalidOperationException("Stage 19.5 settings pane did not keep pause menu visible.");
            pause.ShowControls();
            if (!pauseHud.IsVisible)
                throw new InvalidOperationException("Stage 19.5 controls pane did not keep pause menu visible.");

            pause.Resume();
            if (pause.IsOpen || pauseHud.IsVisible || driver.IsPaused)
                throw new InvalidOperationException("Stage 19.5 resume did not close pause menu and unpause simulation.");

            pause.OpenPauseMenu();
            pause.RestartMission();
            if (pause.IsOpen || pauseHud.IsVisible || driver.LatestSnapshot == null)
                throw new InvalidOperationException("Stage 19.5 restart did not close pause menu with a live snapshot.");

            pause.OpenPauseMenu();
            pause.QuitToMenu();
            pause.QuitGame();
            pause.Resume();
        }

        static void StepUntilPendingPlacement(RtsSimulationDriver driver, BoardRenderer boardRenderer, ActorRenderSystem actorRenderer, string typeId)
        {
            for (var i = 0; i < 500; i++)
            {
                if (HasPendingPlacement(driver, typeId))
                    return;
                StepRuntime(driver, boardRenderer, actorRenderer, 1, 0.1f);
            }

            throw new InvalidOperationException("Timed out waiting for pending placement: " + typeId);
        }

        static Int2 FindValidPlacementCell(RtsSimulationDriver driver, BoardRenderer boardRenderer, ActorRenderSystem actorRenderer)
        {
            var snapshot = RequireSnapshot(driver);
            for (var y = 0; y < snapshot.Map.PlacementHeight; y++)
            {
                for (var x = 0; x < snapshot.Map.PlacementWidth; x++)
                {
                    var candidate = new Int2(x, y);
                    driver.SetHoveredCell(candidate, true);
                    StepRuntime(driver, boardRenderer, actorRenderer, 1, 0.02f);
                    PlacementPreviewSnapshot preview;
                    if (driver.TryGetPlacementPreview(out preview) && preview.CanPlace)
                        return candidate;
                }
            }

            throw new InvalidOperationException("No valid Stage 19.5 placement cell was found.");
        }

        static bool HasPendingPlacement(RtsSimulationDriver driver, string typeId)
        {
            var player = driver.GetLocalPlayerSnapshot();
            if (player == null)
                return false;

            for (var i = 0; i < player.Production.Count; i++)
                if (player.Production[i].TypeId == typeId && player.Production[i].State == "CompletedPendingPlacement")
                    return true;
            return false;
        }

        static void StepRuntime(RtsSimulationDriver driver, BoardRenderer boardRenderer, ActorRenderSystem actorRenderer, int frames, float deltaTime)
        {
            for (var i = 0; i < frames; i++)
            {
                driver.ManualUpdate(deltaTime);
                boardRenderer.UpdateHover(driver.HasHoveredCell ? (Int2?)driver.HoveredCell : null, driver.HoveredCellIsPlacementCell);
                PlacementPreviewSnapshot preview;
                boardRenderer.UpdatePlacementPreview(driver.TryGetPlacementPreview(out preview) ? preview : null);
                actorRenderer.RenderSnapshot(driver.LatestSnapshot, driver.SelectedActorIds, deltaTime);
            }
        }

        static WorldSnapshot RequireSnapshot(RtsSimulationDriver driver)
        {
            if (driver.LatestSnapshot == null)
                throw new InvalidOperationException("Stage 19.5 expected a simulation snapshot.");
            return driver.LatestSnapshot;
        }

        static void RequireSuccess(RtsCommandResult result, string label)
        {
            if (result == null || !result.Success)
                throw new InvalidOperationException("Stage 19.5 expected success for " + label + ": " + (result == null ? "null" : result.ToString()));
        }

        static T RequireEnabled<T>(string label) where T : Behaviour
        {
            var component = UnityEngine.Object.FindFirstObjectByType<T>();
            if (component == null)
                throw new InvalidOperationException("Missing component: " + label);
            if (!component.isActiveAndEnabled)
                throw new InvalidOperationException("Component is disabled: " + label);
            return component;
        }

        static T RequireComponent<T>(string label) where T : Component
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

        static void CaptureRedError(string condition, string stackTrace, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
                RedErrors.Add(condition);
        }
    }
}
