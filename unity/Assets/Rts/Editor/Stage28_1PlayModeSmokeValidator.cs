using System;
using System.Collections.Generic;
using ProjectAegisRTS.Core;
using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.UnityClient.Board;
using ProjectAegisRTS.UnityClient.Boot;
using ProjectAegisRTS.UnityClient.Bootstrap;
using ProjectAegisRTS.UnityClient.CameraControls;
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
    public static class Stage28_1PlayModeSmokeValidator
    {
        static readonly List<string> RedErrors = new List<string>();

        public static void RunStage28_1PlayModeSmokeBatch()
        {
            try
            {
                RunStage28_1PlayModeSmoke();
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

        public static void RunStage28_1PlayModeSmoke()
        {
            RedErrors.Clear();
            Application.logMessageReceived += CaptureRedError;
            try
            {
                ValidateBootRuntime();
                ValidateStage16LayoutAndPlacementSmoke();

                if (RedErrors.Count > 0)
                    throw new InvalidOperationException("Red console errors were produced during Stage 28.1 smoke validation: " + string.Join(" | ", RedErrors.ToArray()));

                Debug.Log("Stage 28.1 play mode smoke validation passed.");
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
                throw new InvalidOperationException("Stage 28.1 boot scene did not open for smoke validation.");

            var menu = Require<MainMenuHud>("MainMenuHud");
            Require<GameBootController>("GameBootController");
            if (!menu.visible)
                throw new InvalidOperationException("Stage 28.1 boot menu is not visible.");
        }

        static void ValidateStage16LayoutAndPlacementSmoke()
        {
            var scene = EditorSceneManager.OpenScene(Stage16SceneCreator.ScenePath);
            if (!scene.IsValid())
                throw new InvalidOperationException("Stage 28.1 Stage16 scene did not open for smoke validation.");

            var bootstrapper = Require<RtsGameBootstrapper>("RtsGameBootstrapper");
            var driver = Require<RtsSimulationDriver>("RtsSimulationDriver");
            var boardRenderer = Require<BoardRenderer>("BoardRenderer");
            var actorRenderer = Require<ActorRenderSystem>("ActorRenderSystem");
            var desktopHud = Require<DesktopRtsHudRoot>("DesktopRtsHudRoot");
            var layout = Require<CncStyleSidebarLayout>("CncStyleSidebarLayout");
            var uiMode = Require<PlayerFacingUiModeController>("PlayerFacingUiModeController");
            var debugVisibility = Require<DebugHudVisibilityController>("DebugHudVisibilityController");
            var placementPanel = Require<PlacementModePanel>("PlacementModePanel");
            var boardPlacement = Require<BoardPlacementController>("BoardPlacementController");
            var boardHud = Require<BoardPlacementHud>("BoardPlacementHud");
            var router = Require<DesktopUiCommandRouter>("DesktopUiCommandRouter");
            var statusLog = Require<RtsStatusLog>("RtsStatusLog");

            bootstrapper.InitializeScene();
            desktopHud.Initialize();
            layout.ApplyLayout();
            uiMode.ApplyPcDesktopMode();
            router.Initialize(driver, statusLog);
            debugVisibility.ApplyPlayerFacingDefaults();
            StepRuntime(driver, boardRenderer, actorRenderer, 6, 0.05f);

            var safeArea = EnsureSafeArea(desktopHud, layout, uiMode);
            var framer = EnsureFramer(safeArea);
            var targetSnapshot = framer.ApplyFramingForScreen(1600, 900);
            if (!targetSnapshot.UsesPcSafeArea || targetSnapshot.RightReservedPx <= 0f || targetSnapshot.LeftReservedPx <= 0f)
                throw new InvalidOperationException("Stage 28.1 1600x900 safe area did not reserve both PC UI columns.");
            if (framer.targetCamera.rect.xMax > 1f - (targetSnapshot.RightReservedPx / targetSnapshot.ScreenWidth) + 0.001f)
                throw new InvalidOperationException("Stage 28.1 1600x900 camera rect can render under the right sidebar.");
            framer.ApplyFraming();
            if (!framer.IsBoardInsideSafeArea(3f))
                throw new InvalidOperationException("Stage 28.1 current-screen board bounds are outside the gameplay safe area.");

            if (!uiMode.IsPcSidebarVisibleForDesktop() || !layout.IsMinimapAboveProductionGrid() || !layout.IsRightSidebarDockedToScreenEdge())
                throw new InvalidOperationException("Stage 28.1 smoke expected the PC right sidebar and top minimap.");
            if (!debugVisibility.AreDebugPanelsHiddenByDefault())
                throw new InvalidOperationException("Stage 28.1 smoke expected debug panels hidden by default.");
            if (boardPlacement.IsPlacementModeActive || boardHud.gameObject.activeInHierarchy || placementPanel.gameObject.activeInHierarchy)
                throw new InvalidOperationException("Stage 28.1 smoke found placement UI visible before building placement.");

            ValidatePowerPlantPlacementFlow(driver, router, boardRenderer, actorRenderer, debugVisibility, placementPanel, boardHud, boardPlacement);
        }

        static void ValidatePowerPlantPlacementFlow(
            RtsSimulationDriver driver,
            DesktopUiCommandRouter router,
            BoardRenderer boardRenderer,
            ActorRenderSystem actorRenderer,
            DebugHudVisibilityController debugVisibility,
            PlacementModePanel placementPanel,
            BoardPlacementHud boardHud,
            BoardPlacementController boardPlacement)
        {
            RequireSuccess(router.QueueProduction("power_plant"), "queue Power Plant");
            StepUntilPendingPlacement(driver, boardRenderer, actorRenderer, "power_plant");
            RequireSuccess(router.QueueProduction("power_plant"), "activate ready Power Plant card");
            debugVisibility.ApplyPlayerFacingDefaults();
            StepRuntime(driver, boardRenderer, actorRenderer, 2, 0.05f);

            if (!driver.HasPlacementMode || driver.PendingPlacementTypeId != "power_plant")
                throw new InvalidOperationException("Stage 28.1 smoke did not enter Power Plant placement mode.");
            if (boardPlacement.IsPlacementModeActive || boardHud.gameObject.activeInHierarchy)
                throw new InvalidOperationException("Stage 28.1 smoke showed Stage3 BoardPlacementHud during PC building placement.");
            if (!placementPanel.gameObject.activeInHierarchy)
                throw new InvalidOperationException("Stage 28.1 smoke expected right-sidebar PlacementModePanel during building placement.");

            var validCell = FindValidPlacementCell(driver, boardRenderer, actorRenderer);
            driver.SetHoveredCell(validCell, true);
            StepRuntime(driver, boardRenderer, actorRenderer, 1, 0.02f);
            PlacementPreviewSnapshot preview;
            if (!driver.TryGetPlacementPreview(out preview) || !preview.CanPlace || preview.PlacementGridScale < 2)
                throw new InvalidOperationException("Stage 28.1 smoke expected a visible valid fine-grid placement preview.");

            RequireSuccess(router.PlaceAtHoveredCell(), "place Power Plant through right-sidebar route");
            StepRuntime(driver, boardRenderer, actorRenderer, 4, 0.05f);
            if (driver.HasPlacementMode || placementPanel.gameObject.activeInHierarchy || boardHud.gameObject.activeInHierarchy)
                throw new InvalidOperationException("Stage 28.1 placement UI did not clear after placing the Power Plant.");
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

            throw new InvalidOperationException("No valid Stage 28.1 placement cell was found.");
        }

        static PcGameplaySafeAreaController EnsureSafeArea(DesktopRtsHudRoot hud, CncStyleSidebarLayout layout, PlayerFacingUiModeController mode)
        {
            var safeArea = UnityEngine.Object.FindFirstObjectByType<PcGameplaySafeAreaController>();
            if (safeArea == null)
                safeArea = hud.gameObject.AddComponent<PcGameplaySafeAreaController>();
            safeArea.desktopHud = hud;
            safeArea.sidebarLayout = layout;
            safeArea.uiModeController = mode;
            safeArea.hudCanvas = hud.canvas;
            safeArea.Refresh();
            return safeArea;
        }

        static PlayerFacingCameraFramer EnsureFramer(PcGameplaySafeAreaController safeArea)
        {
            var camera = Camera.main != null ? Camera.main : UnityEngine.Object.FindFirstObjectByType<Camera>();
            if (camera == null)
                throw new InvalidOperationException("Stage 28.1 smoke requires a camera.");
            var framer = camera.GetComponent<PlayerFacingCameraFramer>();
            if (framer == null)
                framer = camera.gameObject.AddComponent<PlayerFacingCameraFramer>();
            framer.targetCamera = camera;
            framer.safeAreaController = safeArea;
            framer.mapper = UnityEngine.Object.FindFirstObjectByType<BoardCoordinateMapper>();
            framer.uiModeController = UnityEngine.Object.FindFirstObjectByType<PlayerFacingUiModeController>();
            framer.logOnApply = true;
            return framer;
        }

        static WorldSnapshot RequireSnapshot(RtsSimulationDriver driver)
        {
            if (driver == null || driver.LatestSnapshot == null)
                throw new InvalidOperationException("Stage 28.1 expected a current runtime snapshot.");
            return driver.LatestSnapshot;
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

        static void RequireSuccess(RtsCommandResult result, string label)
        {
            if (result == null || !result.Success)
                throw new InvalidOperationException("Stage 28.1 expected success for " + label + ": " + (result == null ? "null" : result.ToString()));
        }

        static void CaptureRedError(string condition, string stackTrace, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
                RedErrors.Add(condition);
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
