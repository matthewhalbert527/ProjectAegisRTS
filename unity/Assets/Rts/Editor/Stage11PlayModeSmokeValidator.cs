using System;
using System.Collections.Generic;
using ProjectAegisRTS.Core;
using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.UnityClient.Bootstrap;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Rendering;
using ProjectAegisRTS.UnityClient.Rendering.Visibility;
using ProjectAegisRTS.UnityClient.UI.Common;
using ProjectAegisRTS.Visibility;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage11PlayModeSmokeValidator
    {
        static readonly List<string> RedErrors = new List<string>();

        public static void RunStage11PlayModeSmokeBatch()
        {
            try
            {
                RunStage11PlayModeSmoke();
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

        public static void RunStage11PlayModeSmoke()
        {
            RedErrors.Clear();
            Application.logMessageReceived += CaptureRedError;
            try
            {
                Stage11SceneValidator.ValidateStage11Scene();
                var scene = EditorSceneManager.OpenScene(Stage11SceneCreator.ScenePath);
                if (!scene.IsValid())
                    throw new InvalidOperationException("Stage 11 scene did not open.");

                var bootstrapper = RequireEnabled<RtsGameBootstrapper>("RtsGameBootstrapper");
                var driver = RequireEnabled<RtsSimulationDriver>("RtsSimulationDriver");
                var boardRenderer = RequireEnabled<BoardRenderer>("BoardRenderer");
                var actorRenderer = RequireEnabled<ActorRenderSystem>("ActorRenderSystem");
                var fogOverlay = RequireEnabled<FogOverlayRenderer>("FogOverlayRenderer");
                var visibilityDebug = RequireEnabled<VisibilityDebugRenderer>("VisibilityDebugRenderer");
                var radar = RequireEnabled<RadarSnapshotAdapter>("RadarSnapshotAdapter");
                var minimap = RequireEnabled<MinimapRenderSystem>("MinimapRenderSystem");
                RequireEnabled<FogDebugHud>("FogDebugHud");

                bootstrapper.InitializeScene();
                driver.TryCreateFogRadarDemoWorld();
                StepRuntime(driver, boardRenderer, actorRenderer, fogOverlay, visibilityDebug, radar, minimap, 4, 0.1f);

                var snapshot = driver.LatestSnapshot;
                if (snapshot == null)
                    throw new InvalidOperationException("Stage 11 did not produce a snapshot.");
                if (snapshot.Fog.Cells.Count == 0)
                    throw new InvalidOperationException("Stage 11 fog snapshot is empty.");
                if (!snapshot.Radar.IsActive)
                    throw new InvalidOperationException("Stage 11 radar snapshot is not active.");
                if (snapshot.Minimap.ActorDots.Count == 0)
                    throw new InvalidOperationException("Stage 11 minimap actor dots are missing.");
                if (!HasVisibleCell(snapshot, new Int2(8, 8)))
                    throw new InvalidOperationException("Stage 11 expected scout cell to be visible.");
                if (ContainsActorType(snapshot, "medium_tank", 2))
                    throw new InvalidOperationException("Stage 11 hidden enemy appeared in perspective actor snapshot.");
                if (!ContainsActorType(snapshot, "rifle_infantry", 2))
                    throw new InvalidOperationException("Stage 11 visible enemy did not appear in perspective actor snapshot.");

                var scoutId = FindActorId(snapshot, "scout_rover", 1);
                if (scoutId <= 0)
                    throw new InvalidOperationException("Stage 11 scout rover missing.");

                driver.SetSelectedActorIds(new[] { scoutId });
                var move = driver.TryIssueMoveSelectedToCell(new Int2(20, 20));
                if (!move.Success)
                    throw new InvalidOperationException("Stage 11 scout move failed: " + move);
                StepRuntime(driver, boardRenderer, actorRenderer, fogOverlay, visibilityDebug, radar, minimap, 220, 0.1f);
                snapshot = driver.LatestSnapshot;

                if (!HasExploredCell(snapshot, new Int2(16, 8)))
                    throw new InvalidOperationException("Stage 11 explored cell did not persist after scout moved away.");
                if (fogOverlay.UnexploredCellCount <= 0 || fogOverlay.ExploredCellCount <= 0 || fogOverlay.VisibleCellCount <= 0)
                    throw new InvalidOperationException("Stage 11 fog overlay did not report expected cell counts.");
                if (visibilityDebug.VisibleCellSampleCount <= 0)
                    throw new InvalidOperationException("Stage 11 visibility debug renderer did not see visible cells.");
                if (!radar.IsRadarActive || radar.ProviderActorId <= 0)
                    throw new InvalidOperationException("Stage 11 radar adapter did not read active radar.");
                if (minimap.ActorDotCount <= 0)
                    throw new InvalidOperationException("Stage 11 minimap renderer did not create actor dots.");

                if (RedErrors.Count > 0)
                    throw new InvalidOperationException("Red console errors were produced during Stage 11 smoke validation: " + string.Join(" | ", RedErrors.ToArray()));

                Debug.Log("Stage 11 play mode smoke validation passed.");
            }
            finally
            {
                Application.logMessageReceived -= CaptureRedError;
                RedErrors.Clear();
            }
        }

        static void StepRuntime(RtsSimulationDriver driver, BoardRenderer boardRenderer, ActorRenderSystem actorRenderer, FogOverlayRenderer fogOverlay, VisibilityDebugRenderer visibilityDebug, RadarSnapshotAdapter radar, MinimapRenderSystem minimap, int frames, float deltaTime)
        {
            for (var i = 0; i < frames; i++)
            {
                driver.ManualUpdate(deltaTime);
                boardRenderer.UpdateHover(driver.HasHoveredCell ? (Int2?)driver.HoveredCell : null);
                PlacementPreviewSnapshot preview;
                boardRenderer.UpdatePlacementPreview(driver.TryGetPlacementPreview(out preview) ? preview : null);
                actorRenderer.RenderSnapshot(driver.LatestSnapshot, driver.SelectedActorIds, deltaTime);
                fogOverlay.RenderSnapshot(driver.LatestSnapshot);
                visibilityDebug.RenderSnapshot(driver.LatestSnapshot);
                radar.Apply(driver.LatestSnapshot);
                minimap.RenderSnapshot(driver.LatestSnapshot);
            }
        }

        static bool HasVisibleCell(WorldSnapshot snapshot, Int2 cell)
        {
            for (var i = 0; i < snapshot.Fog.Cells.Count; i++)
                if (snapshot.Fog.Cells[i].Cell.Equals(cell) && snapshot.Fog.Cells[i].Visibility == CellVisibility.Visible)
                    return true;
            return false;
        }

        static bool HasExploredCell(WorldSnapshot snapshot, Int2 cell)
        {
            for (var i = 0; i < snapshot.Fog.Cells.Count; i++)
                if (snapshot.Fog.Cells[i].Cell.Equals(cell) && snapshot.Fog.Cells[i].Visibility == CellVisibility.Explored)
                    return true;
            return false;
        }

        static bool ContainsActorType(WorldSnapshot snapshot, string typeId, int ownerId)
        {
            for (var i = 0; i < snapshot.Actors.Count; i++)
                if (snapshot.Actors[i].TypeId == typeId && snapshot.Actors[i].OwnerId == ownerId)
                    return true;
            return false;
        }

        static int FindActorId(WorldSnapshot snapshot, string typeId, int ownerId)
        {
            for (var i = 0; i < snapshot.Actors.Count; i++)
                if (snapshot.Actors[i].TypeId == typeId && snapshot.Actors[i].OwnerId == ownerId)
                    return snapshot.Actors[i].ActorId;
            return 0;
        }

        static T RequireEnabled<T>(string label) where T : Behaviour
        {
            var component = UnityEngine.Object.FindFirstObjectByType<T>();
            if (component == null)
                throw new InvalidOperationException("Missing component: " + label);
            if (!component.enabled)
                throw new InvalidOperationException("Component is disabled: " + label);
            return component;
        }

        static void CaptureRedError(string condition, string stackTrace, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
                RedErrors.Add(condition);
        }
    }
}
