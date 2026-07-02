using System;
using System.Collections.Generic;
using ProjectAegisRTS.Core;
using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.UnityClient.Board;
using ProjectAegisRTS.UnityClient.Boot;
using ProjectAegisRTS.UnityClient.Bootstrap;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Rendering;
using ProjectAegisRTS.UnityClient.UI.Common;
using ProjectAegisRTS.UnityClient.UI.Desktop;
using ProjectAegisRTS.UnityClient.UI.XR;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage27_1PlayModeSmokeValidator
    {
        static readonly List<string> RedErrors = new List<string>();

        public static void RunStage27_1PlayModeSmokeBatch()
        {
            try
            {
                RunStage27_1PlayModeSmoke();
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

        public static void RunStage27_1PlayModeSmoke()
        {
            RedErrors.Clear();
            Application.logMessageReceived += CaptureRedError;
            try
            {
                ValidateBootRuntime();
                ValidateStage16PcPlacementSmoke();

                if (RedErrors.Count > 0)
                    throw new InvalidOperationException("Red console errors were produced during Stage 27.1 smoke validation: " + string.Join(" | ", RedErrors.ToArray()));

                Debug.Log("Stage 27.1 play mode smoke validation passed.");
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
                throw new InvalidOperationException("Stage 27.1 boot scene did not open for smoke validation.");

            var menu = RequireComponent<MainMenuHud>("MainMenuHud");
            RequireComponent<GameBootController>("GameBootController");
            if (!menu.visible)
                throw new InvalidOperationException("Stage 27.1 boot menu is not visible.");
        }

        static void ValidateStage16PcPlacementSmoke()
        {
            var scene = EditorSceneManager.OpenScene(Stage16SceneCreator.ScenePath);
            if (!scene.IsValid())
                throw new InvalidOperationException("Stage 27.1 Stage16 scene did not open for smoke validation.");

            var bootstrapper = RequireComponent<RtsGameBootstrapper>("RtsGameBootstrapper");
            var driver = RequireComponent<RtsSimulationDriver>("RtsSimulationDriver");
            var boardRenderer = RequireComponent<BoardRenderer>("BoardRenderer");
            var actorRenderer = RequireComponent<ActorRenderSystem>("ActorRenderSystem");
            var desktopHud = RequireComponent<DesktopRtsHudRoot>("DesktopRtsHudRoot");
            var layout = RequireComponent<CncStyleSidebarLayout>("CncStyleSidebarLayout");
            var mode = RequireComponent<PlayerFacingUiModeController>("PlayerFacingUiModeController");
            var pause = RequireComponent<PauseMenuController>("PauseMenuController");
            var pauseHud = RequireComponent<PauseMenuHud>("PauseMenuHud");
            var debugVisibility = RequireComponent<DebugHudVisibilityController>("DebugHudVisibilityController");
            var placementPanel = RequireComponent<PlacementModePanel>("PlacementModePanel");
            var boardPlacement = RequireComponent<BoardPlacementController>("BoardPlacementController");
            var boardHud = RequireComponent<BoardPlacementHud>("BoardPlacementHud");
            var router = RequireComponent<DesktopUiCommandRouter>("DesktopUiCommandRouter");
            var statusLog = RequireComponent<RtsStatusLog>("RtsStatusLog");

            bootstrapper.InitializeScene();
            desktopHud.Initialize();
            layout.ApplyLayout();
            mode.ApplyPcDesktopMode();
            router.Initialize(driver, statusLog);
            pause.hud = pauseHud;
            pause.suppressApplicationQuitForValidation = true;
            pause.suppressSceneLoadsForValidation = true;
            pause.Initialize(driver, UnityEngine.Object.FindFirstObjectByType<ProjectAegisRTS.UnityClient.Scenario.VerticalSliceScenarioController>());
            debugVisibility.ApplyPlayerFacingDefaults();
            StepRuntime(driver, boardRenderer, actorRenderer, 6, 0.05f);

            ValidateDesktopHudDocking(desktopHud, layout);
            if (!mode.IsPcSidebarVisibleForDesktop() || !layout.IsMinimapAboveProductionGrid())
                throw new InvalidOperationException("Stage 27.1 smoke expected the PC right sidebar and top minimap.");
            if (!mode.AreXrBuildMenusHiddenForPc())
                throw new InvalidOperationException("Stage 27.1 smoke expected XR build menus hidden in PCDesktop mode.");
            if (boardHud.gameObject.activeInHierarchy || !debugVisibility.IsBoardPlacementHudHiddenInPcDesktop())
                throw new InvalidOperationException("Stage 27.1 smoke found BoardPlacementHud visible before building placement.");

            var powerPlantsBefore = CountOwnedActors(driver, "power_plant", 1);
            RequireSuccess(router.QueueProduction("power_plant"), "queue Power Plant from production grid route");
            StepUntilPendingPlacement(driver, boardRenderer, actorRenderer, "power_plant");
            RequireSuccess(router.QueueProduction("power_plant"), "activate ready Power Plant production card");
            debugVisibility.ApplyPlayerFacingDefaults();
            StepRuntime(driver, boardRenderer, actorRenderer, 2, 0.05f);

            if (!driver.HasPlacementMode || driver.PendingPlacementTypeId != "power_plant")
                throw new InvalidOperationException("Stage 27.1 smoke did not enter building placement mode.");
            if (boardPlacement.IsPlacementModeActive)
                throw new InvalidOperationException("Stage 27.1 smoke entered board setup placement instead of building placement.");
            if (!placementPanel.gameObject.activeInHierarchy)
                throw new InvalidOperationException("Stage 27.1 smoke expected right-sidebar PlacementModePanel during building placement.");
            if (boardHud.gameObject.activeInHierarchy)
                throw new InvalidOperationException("Stage 27.1 smoke showed the Stage3 BoardPlacementHud during building placement.");
            Int2 suggestedCell;
            if (!driver.TryFindSuggestedPlacementCell(out suggestedCell))
                throw new InvalidOperationException("Stage 27.1 smoke expected a suggested legal placement cell for the completed Power Plant.");

            var validCell = FindValidPlacementCell(driver, boardRenderer, actorRenderer);
            driver.SetHoveredCell(validCell, true);
            StepRuntime(driver, boardRenderer, actorRenderer, 1, 0.02f);
            PlacementPreviewSnapshot preview;
            if (!driver.TryGetPlacementPreview(out preview) || !preview.CanPlace || preview.FootprintCells.Count == 0)
                throw new InvalidOperationException("Stage 27.1 smoke expected a valid fine-grid placement preview.");

            pause.HandleEscapePressed();
            debugVisibility.ApplyPlayerFacingDefaults();
            StepRuntime(driver, boardRenderer, actorRenderer, 1, 0.02f);
            if (driver.HasPlacementMode || pause.IsOpen || driver.IsPaused)
                throw new InvalidOperationException("Stage 27.1 Esc should cancel building placement before opening pause.");
            if (placementPanel.gameObject.activeInHierarchy || boardHud.gameObject.activeInHierarchy)
                throw new InvalidOperationException("Stage 27.1 Esc did not hide placement UI after cancel.");

            RequireSuccess(router.QueueProduction("power_plant"), "reactivate ready Power Plant for placement");
            debugVisibility.ApplyPlayerFacingDefaults();
            var coarseHoverCell = FindValidCoarsePlacementHoverCell(driver, boardRenderer, actorRenderer);
            driver.SetHoveredCell(coarseHoverCell, false);
            StepRuntime(driver, boardRenderer, actorRenderer, 1, 0.02f);
            if (!driver.TryGetPlacementPreview(out preview) || !preview.CanPlace)
                throw new InvalidOperationException("Stage 27.1 smoke expected coarse desktop hover to produce a valid fine-grid placement preview.");
            RequireSuccess(router.PlaceAtHoveredCell(), "place Power Plant through right-sidebar command router");
            debugVisibility.ApplyPlayerFacingDefaults();
            StepRuntime(driver, boardRenderer, actorRenderer, 4, 0.05f);
            if (driver.HasPlacementMode)
                throw new InvalidOperationException("Stage 27.1 placement remained active after placing the Power Plant.");
            if (CountOwnedActors(driver, "power_plant", 1) <= powerPlantsBefore)
                throw new InvalidOperationException("Stage 27.1 smoke did not place a new Power Plant.");
            if (boardHud.gameObject.activeInHierarchy)
                throw new InvalidOperationException("Stage 27.1 BoardPlacementHud became visible after building placement.");

            pause.HandleEscapePressed();
            if (!pause.IsOpen || !pauseHud.IsVisible || !driver.IsPaused)
                throw new InvalidOperationException("Stage 27.1 pause menu did not open normally after placement ended.");
            pause.Resume();
            if (pause.IsOpen || pauseHud.IsVisible || driver.IsPaused)
                throw new InvalidOperationException("Stage 27.1 pause menu did not resume normally after placement ended.");
        }

        static void StepUntilPendingPlacement(RtsSimulationDriver driver, BoardRenderer boardRenderer, ActorRenderSystem actorRenderer, string typeId)
        {
            for (var i = 0; i < 600; i++)
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

            throw new InvalidOperationException("No valid Stage 27.1 placement cell was found.");
        }

        static Int2 FindValidCoarsePlacementHoverCell(RtsSimulationDriver driver, BoardRenderer boardRenderer, ActorRenderSystem actorRenderer)
        {
            var snapshot = RequireSnapshot(driver);
            for (var y = 0; y < snapshot.Map.Height; y++)
            {
                for (var x = 0; x < snapshot.Map.Width; x++)
                {
                    var candidate = new Int2(x, y);
                    driver.SetHoveredCell(candidate, false);
                    StepRuntime(driver, boardRenderer, actorRenderer, 1, 0.02f);
                    PlacementPreviewSnapshot preview;
                    if (driver.TryGetPlacementPreview(out preview) && preview.CanPlace)
                        return candidate;
                }
            }

            throw new InvalidOperationException("No valid Stage 27.1 coarse desktop hover placement cell was found.");
        }

        static void ValidateDesktopHudDocking(DesktopRtsHudRoot desktopHud, CncStyleSidebarLayout layout)
        {
            if (desktopHud == null || layout == null)
                throw new InvalidOperationException("Stage 27.1 smoke desktop HUD docking check needs a HUD root and layout.");

            var canvas = desktopHud.GetComponentInParent<Canvas>();
            if (canvas == null || canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                throw new InvalidOperationException("Stage 27.1 smoke desktop HUD must run on a Screen Space Overlay canvas.");

            var rootRect = desktopHud.GetComponent<RectTransform>();
            if (rootRect == null ||
                !Approximately(rootRect.anchorMin.x, 0f) ||
                !Approximately(rootRect.anchorMin.y, 0f) ||
                !Approximately(rootRect.anchorMax.x, 1f) ||
                !Approximately(rootRect.anchorMax.y, 1f) ||
                !Approximately(rootRect.offsetMin.x, 0f) ||
                !Approximately(rootRect.offsetMin.y, 0f) ||
                !Approximately(rootRect.offsetMax.x, 0f) ||
                !Approximately(rootRect.offsetMax.y, 0f))
                throw new InvalidOperationException("Stage 27.1 smoke desktop HUD root must stretch to the full player canvas.");

            if (!layout.IsRightSidebarDockedToScreenEdge())
                throw new InvalidOperationException("Stage 27.1 smoke PC right sidebar must be docked to the screen's right edge.");
        }

        static bool Approximately(float left, float right)
        {
            return Mathf.Abs(left - right) <= 0.01f;
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

        static int CountOwnedActors(RtsSimulationDriver driver, string typeId, int ownerId)
        {
            var snapshot = RequireSnapshot(driver);
            var count = 0;
            for (var i = 0; i < snapshot.Actors.Count; i++)
            {
                var actor = snapshot.Actors[i];
                if (actor.OwnerId == ownerId && actor.TypeId == typeId && !actor.IsDestroyed)
                    count++;
            }

            return count;
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
            if (driver == null || driver.LatestSnapshot == null)
                throw new InvalidOperationException("Stage 27.1 expected a current runtime snapshot.");
            return driver.LatestSnapshot;
        }

        static void RequireSuccess(RtsCommandResult result, string label)
        {
            if (result == null || !result.Success)
                throw new InvalidOperationException("Stage 27.1 expected success for " + label + ": " + (result == null ? "null" : result.ToString()));
        }

        static void CaptureRedError(string condition, string stackTrace, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
                RedErrors.Add(condition);
        }

        static T RequireComponent<T>(string label) where T : Component
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
