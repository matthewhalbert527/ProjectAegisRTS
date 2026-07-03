using System;
using System.IO;
using ProjectAegisRTS.UnityClient.Board;
using ProjectAegisRTS.UnityClient.Bootstrap;
using ProjectAegisRTS.UnityClient.CameraControls;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Rendering;
using ProjectAegisRTS.UnityClient.Rendering.TerrainPieces;
using ProjectAegisRTS.UnityClient.Rendering.Visuals;
using ProjectAegisRTS.UnityClient.UI.Common;
using ProjectAegisRTS.UnityClient.UI.Desktop;
using ProjectAegisRTS.UnityClient.UI.XR;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage32SceneValidator
    {
        public static void ValidateStage32SceneBatch()
        {
            try
            {
                ValidateStage32Scene();
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

        public static void ValidateStage32Scene()
        {
            Stage32TerrainPieceValidator.ValidateStage32TerrainPieces();
            ValidateReviewScene();
            ValidateStage16Integration();
            Debug.Log("Stage 32 scene validation passed.");
        }

        static void ValidateReviewScene()
        {
            if (!File.Exists(Path.Combine(Stage8ActorCatalog.RepoRoot, "unity", Stage32SceneCreator.ScenePath)))
                Stage32SceneCreator.CreateOrUpdateStage32Scene();

            var scene = EditorSceneManager.OpenScene(Stage32SceneCreator.ScenePath);
            if (!scene.IsValid())
                throw new InvalidOperationException("Stage 32 review scene did not open.");

            var controller = Require<Stage32TerrainSetDressingReviewController>("Stage32TerrainSetDressingReviewController");
            var hud = Require<Stage32TerrainPieceQaHud>("Stage32TerrainPieceQaHud");
            Require<LightingProfileApplier>("LightingProfileApplier");
            Require<BattlefieldAtmosphereController>("BattlefieldAtmosphereController");
            controller.EnsureReviewScene();

            if (controller.PieceCount < 64)
                throw new InvalidOperationException("Stage 32 review scene did not arrange the terrain-piece library.");
            if (controller.MaterialSwatchCount < 18)
                throw new InvalidOperationException("Stage 32 review scene material swatches are missing.");
            if (controller.FootprintReferenceCount < 16)
                throw new InvalidOperationException("Stage 32 fine-grid footprint reference is missing.");
            if (!hud.visible)
                throw new InvalidOperationException("Stage 32 QA HUD should be visible in the review scene.");

            var camera = Camera.main != null ? Camera.main : UnityEngine.Object.FindFirstObjectByType<Camera>();
            if (camera == null || !camera.orthographic)
                throw new InvalidOperationException("Stage 32 review scene requires an orthographic screenshot camera.");
        }

        static void ValidateStage16Integration()
        {
            Stage16SceneCreator.CreateOrUpdateStage16Scene();
            var scene = EditorSceneManager.OpenScene(Stage16SceneCreator.ScenePath);
            if (!scene.IsValid())
                throw new InvalidOperationException("Stage 32 Stage16 scene did not open.");

            var bootstrapper = Require<RtsGameBootstrapper>("RtsGameBootstrapper");
            var driver = Require<RtsSimulationDriver>("RtsSimulationDriver");
            var boardRenderer = Require<BoardRenderer>("BoardRenderer");
            var actorRenderer = Require<ActorRenderSystem>("ActorRenderSystem");
            var desktopHud = Require<DesktopRtsHudRoot>("DesktopRtsHudRoot");
            var layout = Require<CncStyleSidebarLayout>("CncStyleSidebarLayout");
            var uiMode = Require<PlayerFacingUiModeController>("PlayerFacingUiModeController");
            var debugVisibility = Require<DebugHudVisibilityController>("DebugHudVisibilityController");
            var boardPlacement = Require<BoardPlacementController>("BoardPlacementController");
            var boardHud = Require<BoardPlacementHud>("BoardPlacementHud");
            var placementPanel = Require<PlacementModePanel>("PlacementModePanel");
            var safeArea = Require<PcGameplaySafeAreaController>("PcGameplaySafeAreaController");
            var framer = Camera.main != null ? Camera.main.GetComponent<PlayerFacingCameraFramer>() : null;
            var layer = Require<TerrainSetDressingRuntimeLayer>("TerrainSetDressingRuntimeLayer");

            bootstrapper.InitializeScene();
            desktopHud.Initialize();
            layout.ApplyLayout();
            uiMode.ApplyPcDesktopMode();
            debugVisibility.ApplyPlayerFacingDefaults();
            layer.EnsureInitialized();
            StepRuntime(driver, boardRenderer, actorRenderer, 4, 0.05f);

            if (!uiMode.IsPcSidebarVisibleForDesktop() || !layout.IsRightSidebarDockedToScreenEdge() || !layout.AreProductionPanelsInRightSidebar())
                throw new InvalidOperationException("Stage 32 PCDesktop right sidebar/minimap layout regressed.");
            if (!debugVisibility.AreDebugPanelsHiddenByDefault())
                throw new InvalidOperationException("Stage 32 debug panels must remain hidden by default.");
            if (boardPlacement.IsPlacementModeActive || boardHud.gameObject.activeInHierarchy || placementPanel.gameObject.activeInHierarchy)
                throw new InvalidOperationException("Stage 32 must preserve Stage27.1 placement HUD separation before building placement.");
            if (layer.renderer == null || layer.renderer.RenderedPieceCount < 32)
                throw new InvalidOperationException("Stage 32 player-facing set dressing did not render enough pieces.");
            if (!layer.renderer.LastRenderWasVisualOnly)
                throw new InvalidOperationException("Stage 32 set dressing profile must remain visual-only.");
            if (framer == null)
                throw new InvalidOperationException("Stage 32 expected the PC safe-area camera framer to remain on the main camera.");

            safeArea.Refresh();
            framer.ApplyFraming();
            if (!framer.IsBoardInsideSafeArea(3f))
                throw new InvalidOperationException("Stage 32 board safe area failed after terrain set dressing. bounds=" + framer.GetBoardScreenBounds() + " safe=" + safeArea.GameplayViewportRect);
        }

        static void StepRuntime(RtsSimulationDriver driver, BoardRenderer boardRenderer, ActorRenderSystem actorRenderer, int frames, float deltaTime)
        {
            for (var i = 0; i < frames; i++)
            {
                driver.ManualUpdate(deltaTime);
                boardRenderer.UpdateHover(driver.HasHoveredCell ? (ProjectAegisRTS.Core.Int2?)driver.HoveredCell : null, driver.HoveredCellIsPlacementCell);
                ProjectAegisRTS.Snapshots.PlacementPreviewSnapshot preview;
                boardRenderer.UpdatePlacementPreview(driver.TryGetPlacementPreview(out preview) ? preview : null);
                actorRenderer.RenderSnapshot(driver.LatestSnapshot, driver.SelectedActorIds, deltaTime);
            }
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
