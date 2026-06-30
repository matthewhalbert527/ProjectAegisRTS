using System;
using System.Collections.Generic;
using ProjectAegisRTS.Core;
using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.UnityClient.Bootstrap;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Rendering;
using ProjectAegisRTS.UnityClient.Rendering.Map;
using ProjectAegisRTS.UnityClient.UI.Common;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage13PlayModeSmokeValidator
    {
        static readonly List<string> RedErrors = new List<string>();

        public static void RunStage13PlayModeSmokeBatch()
        {
            try
            {
                RunStage13PlayModeSmoke();
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

        public static void RunStage13PlayModeSmoke()
        {
            RedErrors.Clear();
            Application.logMessageReceived += CaptureRedError;
            try
            {
                Stage13SceneValidator.ValidateStage13Scene();
                var scene = EditorSceneManager.OpenScene(Stage13SceneCreator.ScenePath);
                if (!scene.IsValid())
                    throw new InvalidOperationException("Stage 13 scene did not open.");

                var bootstrapper = RequireEnabled<RtsGameBootstrapper>("RtsGameBootstrapper");
                var driver = RequireEnabled<RtsSimulationDriver>("RtsSimulationDriver");
                var boardRenderer = RequireEnabled<BoardRenderer>("BoardRenderer");
                var actorRenderer = RequireEnabled<ActorRenderSystem>("ActorRenderSystem");
                var terrain = RequireEnabled<TerrainDebugRenderer>("TerrainDebugRenderer");
                var path = RequireEnabled<PathDebugRenderer>("PathDebugRenderer");
                var authoring = RequireEnabled<MapAuthoringOverlay>("MapAuthoringOverlay");
                RequireEnabled<MapValidationDebugHud>("MapValidationDebugHud");

                bootstrapper.InitializeScene();
                driver.TryCreateMapTerrainDemoWorld();
                StepRuntime(driver, boardRenderer, actorRenderer, terrain, path, authoring, 4, 0.1f);

                var snapshot = driver.LatestSnapshot;
                if (snapshot == null)
                    throw new InvalidOperationException("Stage 13 did not produce a snapshot.");
                if (snapshot.Map.TerrainCells.Count != 1024)
                    throw new InvalidOperationException("Stage 13 expected 1024 terrain cells.");
                if (!snapshot.Map.IsValid)
                    throw new InvalidOperationException("Stage 13 map validation failed: " + string.Join(" | ", snapshot.Map.ValidationErrors));
                if (!HasTerrainKind(snapshot.Map, "Road") || !HasTerrainKind(snapshot.Map, "Rough") || !HasTerrainKind(snapshot.Map, "Water") || !HasTerrainKind(snapshot.Map, "Cliff"))
                    throw new InvalidOperationException("Stage 13 expected road/rough/water/cliff terrain in the snapshot.");
                if (terrain.HighlightedTerrainCellCount <= 0 || terrain.ImpassableTerrainCellCount <= 0)
                    throw new InvalidOperationException("Stage 13 terrain renderer did not read terrain diagnostics.");
                if (authoring.AuthoringCellCount != 1024)
                    throw new InvalidOperationException("Stage 13 authoring overlay did not read map snapshot.");

                var scoutId = FindActor(snapshot, "scout_rover", 1);
                driver.SetSelectedActorIds(new[] { scoutId });
                var move = driver.TryIssueMoveSelectedToCell(new Int2(18, 6));
                if (!move.Success)
                    throw new InvalidOperationException("Stage 13 expected pathing move success: " + move.Code + " / " + move.Message);
                StepRuntime(driver, boardRenderer, actorRenderer, terrain, path, authoring, 8, 0.1f);

                if (driver.LatestSnapshot.Map.RecentPathQueries.Count == 0)
                    throw new InvalidOperationException("Stage 13 expected recent path query diagnostics.");
                if (path.RecentQueryCount <= 0 || path.PathCellCount <= 0)
                    throw new InvalidOperationException("Stage 13 path renderer did not read path diagnostics.");

                if (RedErrors.Count > 0)
                    throw new InvalidOperationException("Red console errors were produced during Stage 13 smoke validation: " + string.Join(" | ", RedErrors.ToArray()));

                Debug.Log("Stage 13 play mode smoke validation passed.");
            }
            finally
            {
                Application.logMessageReceived -= CaptureRedError;
                RedErrors.Clear();
            }
        }

        static void StepRuntime(RtsSimulationDriver driver, BoardRenderer boardRenderer, ActorRenderSystem actorRenderer, TerrainDebugRenderer terrain, PathDebugRenderer path, MapAuthoringOverlay authoring, int frames, float deltaTime)
        {
            for (var i = 0; i < frames; i++)
            {
                driver.ManualUpdate(deltaTime);
                boardRenderer.UpdateHover(driver.HasHoveredCell ? (Int2?)driver.HoveredCell : null);
                PlacementPreviewSnapshot preview;
                boardRenderer.UpdatePlacementPreview(driver.TryGetPlacementPreview(out preview) ? preview : null);
                actorRenderer.RenderSnapshot(driver.LatestSnapshot, driver.SelectedActorIds, deltaTime);
                terrain.RenderSnapshot(driver.LatestSnapshot);
                path.RenderSnapshot(driver.LatestSnapshot);
                authoring.RenderSnapshot(driver.LatestSnapshot);
            }
        }

        static int FindActor(WorldSnapshot snapshot, string typeId, int ownerId)
        {
            for (var i = 0; i < snapshot.Actors.Count; i++)
                if (snapshot.Actors[i].TypeId == typeId && snapshot.Actors[i].OwnerId == ownerId)
                    return snapshot.Actors[i].ActorId;
            throw new InvalidOperationException("Missing actor " + typeId + " for owner " + ownerId + ".");
        }

        static bool HasTerrainKind(MapSnapshot snapshot, string kind)
        {
            for (var i = 0; i < snapshot.TerrainCells.Count; i++)
                if (snapshot.TerrainCells[i].Kind == kind)
                    return true;
            return false;
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
