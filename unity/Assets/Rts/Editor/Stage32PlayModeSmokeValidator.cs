using System;
using System.Collections.Generic;
using ProjectAegisRTS.UnityClient.Board;
using ProjectAegisRTS.UnityClient.Bootstrap;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Rendering;
using ProjectAegisRTS.UnityClient.Rendering.TerrainPieces;
using ProjectAegisRTS.UnityClient.UI.Common;
using ProjectAegisRTS.UnityClient.UI.Desktop;
using ProjectAegisRTS.UnityClient.UI.XR;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage32PlayModeSmokeValidator
    {
        static readonly List<string> RedErrors = new List<string>();

        public static void RunStage32PlayModeSmokeBatch()
        {
            try
            {
                RunStage32PlayModeSmoke();
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

        public static void RunStage32PlayModeSmoke()
        {
            RedErrors.Clear();
            Application.logMessageReceived += CaptureRedError;
            try
            {
                Stage32SceneValidator.ValidateStage32Scene();
                ValidatePlayerFacingRuntime();
                if (RedErrors.Count > 0)
                    throw new InvalidOperationException("Red console errors were produced during Stage 32 smoke: " + string.Join(" | ", RedErrors.ToArray()));

                Debug.Log("Stage 32 play-mode smoke validation passed.");
            }
            finally
            {
                Application.logMessageReceived -= CaptureRedError;
                RedErrors.Clear();
            }
        }

        static void ValidatePlayerFacingRuntime()
        {
            var scene = EditorSceneManager.OpenScene(Stage16SceneCreator.ScenePath);
            if (!scene.IsValid())
                throw new InvalidOperationException("Stage 32 smoke could not open Stage16.");

            var bootstrapper = Require<RtsGameBootstrapper>("RtsGameBootstrapper");
            var driver = Require<RtsSimulationDriver>("RtsSimulationDriver");
            var boardRenderer = Require<BoardRenderer>("BoardRenderer");
            var actorRenderer = Require<ActorRenderSystem>("ActorRenderSystem");
            var layer = Require<TerrainSetDressingRuntimeLayer>("TerrainSetDressingRuntimeLayer");
            var desktopHud = Require<DesktopRtsHudRoot>("DesktopRtsHudRoot");
            var layout = Require<CncStyleSidebarLayout>("CncStyleSidebarLayout");
            var mode = Require<PlayerFacingUiModeController>("PlayerFacingUiModeController");
            var debugVisibility = Require<DebugHudVisibilityController>("DebugHudVisibilityController");
            var boardHud = Require<BoardPlacementHud>("BoardPlacementHud");

            bootstrapper.InitializeScene();
            desktopHud.Initialize();
            layout.ApplyLayout();
            mode.ApplyPcDesktopMode();
            debugVisibility.ApplyPlayerFacingDefaults();
            layer.EnsureInitialized();
            StepRuntime(driver, boardRenderer, actorRenderer, 8, 0.05f);

            if (layer.renderer == null || layer.renderer.RenderedPieceCount < 32)
                throw new InvalidOperationException("Stage 32 smoke expected player-facing set dressing pieces.");
            ValidatePlayerFacingSourceArtRender(layer);
            if (!layout.IsRightSidebarDockedToScreenEdge() || !mode.IsPcSidebarVisibleForDesktop())
                throw new InvalidOperationException("Stage 32 smoke expected PCDesktop sidebar to remain visible.");
            if (boardHud.gameObject.activeInHierarchy)
                throw new InvalidOperationException("Stage 32 smoke expected Stage3 board-placement HUD to remain hidden.");
            if (!debugVisibility.AreDebugPanelsHiddenByDefault())
                throw new InvalidOperationException("Stage 32 smoke expected debug panels hidden by default.");
        }

        static void ValidatePlayerFacingSourceArtRender(TerrainSetDressingRuntimeLayer layer)
        {
            if (!Stage32TerrainArtIngestionGenerator.HasPlayerFacingSourceArt())
                return;
            if (layer.renderer == null || layer.renderer.visualRoot == null)
                throw new InvalidOperationException("Stage 32 smoke cannot validate source-art terrain because the rendered terrain root is missing.");

            var tags = layer.renderer.visualRoot.GetComponentsInChildren<TerrainArtSourceTag>(true);
            var valid = 0;
            for (var i = 0; i < tags.Length; i++)
                if (tags[i] != null && tags[i].IsPlayerFacingSourceArt())
                    valid++;

            if (valid < Stage32TerrainArtIngestionGenerator.MinimumPlayerFacingSourceReplacements)
                throw new InvalidOperationException("Stage 32 smoke expected imported Batch01 source-art terrain. Rendered source-art pieces: " + valid + ".");
            if (valid < layer.renderer.RenderedPieceCount)
                throw new InvalidOperationException("Stage 32 smoke rendered proxy-only terrain while Batch01 source art is available. Rendered=" + layer.renderer.RenderedPieceCount + " sourceArt=" + valid + ".");
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

        static void CaptureRedError(string condition, string stackTrace, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
                RedErrors.Add(condition);
        }
    }
}
