using System;
using System.Collections.Generic;
using ProjectAegisRTS.Core;
using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.UnityClient.Bootstrap;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage18_5PlayModeSmokeValidator
    {
        static readonly List<string> RedErrors = new List<string>();

        public static void RunStage18_5PlayModeSmokeBatch()
        {
            try
            {
                RunStage18_5PlayModeSmoke();
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

        public static void RunStage18_5PlayModeSmoke()
        {
            RedErrors.Clear();
            Application.logMessageReceived += CaptureRedError;
            try
            {
                Stage18_5FineGridValidator.ValidateStage18_5FineGrid();
                ValidateRuntimeFinePlacement();

                if (RedErrors.Count > 0)
                    throw new InvalidOperationException("Red console errors were produced during Stage 18.5 smoke validation: " + string.Join(" | ", RedErrors.ToArray()));

                Debug.Log("Stage 18.5 play mode smoke validation passed.");
            }
            finally
            {
                Application.logMessageReceived -= CaptureRedError;
                RedErrors.Clear();
            }
        }

        static void ValidateRuntimeFinePlacement()
        {
            var scene = EditorSceneManager.OpenScene(Stage16SceneCreator.ScenePath);
            if (!scene.IsValid())
                throw new InvalidOperationException("Stage 18.5 Stage16 scene did not open for smoke validation.");

            var bootstrapper = RequireEnabled<RtsGameBootstrapper>("RtsGameBootstrapper");
            var driver = RequireEnabled<RtsSimulationDriver>("RtsSimulationDriver");
            var mapper = RequireEnabled<BoardCoordinateMapper>("BoardCoordinateMapper");
            var boardRenderer = RequireEnabled<BoardRenderer>("BoardRenderer");
            var actorRenderer = RequireEnabled<ActorRenderSystem>("ActorRenderSystem");

            bootstrapper.InitializeScene();
            StepRuntime(driver, boardRenderer, actorRenderer, 4, 0.05f);

            if (mapper.PlacementGridScale != 2 || mapper.PlacementBoardWidth != mapper.BoardWidth * 2 || mapper.PlacementBoardHeight != mapper.BoardHeight * 2)
                throw new InvalidOperationException("Stage 18.5 mapper did not initialize the doubled placement grid.");
            if (Mathf.Abs(mapper.PlacementCellSizeMeters - mapper.CellSizeMeters * 0.5f) > 0.001f)
                throw new InvalidOperationException("Stage 18.5 mapper fine cell size is not half a coarse cell.");
            if (boardRenderer.FineGridLineCount <= 0)
                throw new InvalidOperationException("Stage 18.5 board renderer did not create fine grid lines.");

            var initialSnapshot = RequireSnapshot(driver);
            if (initialSnapshot.Map.PlacementGridScale != 2 || initialSnapshot.Map.PlacementWidth != initialSnapshot.Map.Width * 2)
                throw new InvalidOperationException("Stage 18.5 runtime snapshot did not expose fine placement dimensions.");

            RequireSuccess(driver.TryQueueProduction("power_plant"), "queue power plant");
            StepUntilPendingPlacement(driver, boardRenderer, actorRenderer, "power_plant");
            RequireSuccess(driver.TryEnterPlacementModeForFirstPending(), "enter fine placement mode");

            var hub = FindActor(driver.LatestSnapshot, "fabrication_hub", driver.PlayerId);
            if (hub == null)
                throw new InvalidOperationException("Stage 18.5 expected player fabrication hub snapshot.");

            var overlappingCell = hub.PlacementTopLeftCell + new Int2(1, 0);
            driver.SetHoveredCell(overlappingCell, true);
            PlacementPreviewSnapshot overlapPreview;
            if (!driver.TryGetPlacementPreview(out overlapPreview) || overlapPreview.CanPlace || overlapPreview.ErrorCode != "OccupiedCell")
                throw new InvalidOperationException("Stage 18.5 expected occupied fine-cell preview to be rejected.");

            PlacementPreviewSnapshot validPreview;
            var validCell = FindValidHalfOffsetPlacementCell(driver, boardRenderer, actorRenderer, out validPreview);
            if (validCell.X % 2 == 0 && validCell.Y % 2 == 0)
                throw new InvalidOperationException("Stage 18.5 expected a half-offset fine placement cell.");
            if (validPreview.PlacementFootprintCells.X != 4 || validPreview.PlacementFootprintCells.Y != 4 || validPreview.FootprintCells.Count != 16)
                throw new InvalidOperationException("Stage 18.5 power plant preview did not expose a 4x4 fine footprint.");

            boardRenderer.UpdateHover(validCell, true);
            boardRenderer.UpdatePlacementPreview(validPreview);
            RequireSuccess(driver.TryPlacePendingBuildingAtCell(validCell), "place power plant at half fine offset");
            StepRuntime(driver, boardRenderer, actorRenderer, 3, 0.05f);

            var placed = FindActorAtPlacementCell(driver.LatestSnapshot, "power_plant", driver.PlayerId, validCell);
            if (placed == null)
                throw new InvalidOperationException("Stage 18.5 placed power plant snapshot did not retain fine placement top-left.");
            if (!placed.PlacementFootprintCells.Equals(new Int2(4, 4)))
                throw new InvalidOperationException("Stage 18.5 placed power plant snapshot did not retain 4x4 fine footprint.");
            if (!driver.TrySelectActorAtCell(placed.CellPosition).Success || driver.SelectedActorIds.Count == 0)
                throw new InvalidOperationException("Stage 18.5 placed fine-offset building was not selectable through coarse compatibility cells.");
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

        static Int2 FindValidHalfOffsetPlacementCell(RtsSimulationDriver driver, BoardRenderer boardRenderer, ActorRenderSystem actorRenderer, out PlacementPreviewSnapshot preview)
        {
            var snapshot = RequireSnapshot(driver);
            for (var y = 0; y < snapshot.Map.PlacementHeight; y++)
            {
                for (var x = 0; x < snapshot.Map.PlacementWidth; x++)
                {
                    if (x % 2 == 0 && y % 2 == 0)
                        continue;

                    var candidate = new Int2(x, y);
                    driver.SetHoveredCell(candidate, true);
                    StepRuntime(driver, boardRenderer, actorRenderer, 1, 0.02f);
                    if (driver.TryGetPlacementPreview(out preview) && preview.CanPlace)
                        return candidate;
                }
            }

            preview = null;
            throw new InvalidOperationException("No valid Stage 18.5 half-offset placement cell was found.");
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

        static ActorSnapshot FindActor(WorldSnapshot snapshot, string typeId, int ownerId)
        {
            if (snapshot == null)
                return null;

            for (var i = 0; i < snapshot.Actors.Count; i++)
                if (snapshot.Actors[i].TypeId == typeId && snapshot.Actors[i].OwnerId == ownerId && !snapshot.Actors[i].IsDestroyed)
                    return snapshot.Actors[i];
            return null;
        }

        static ActorSnapshot FindActorAtPlacementCell(WorldSnapshot snapshot, string typeId, int ownerId, Int2 placementCell)
        {
            if (snapshot == null)
                return null;

            for (var i = 0; i < snapshot.Actors.Count; i++)
            {
                var actor = snapshot.Actors[i];
                if (actor.TypeId == typeId && actor.OwnerId == ownerId && actor.PlacementTopLeftCell.Equals(placementCell) && !actor.IsDestroyed)
                    return actor;
            }

            return null;
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
                throw new InvalidOperationException("Stage 18.5 expected a simulation snapshot.");
            return driver.LatestSnapshot;
        }

        static void RequireSuccess(RtsCommandResult result, string label)
        {
            if (result == null || !result.Success)
                throw new InvalidOperationException("Stage 18.5 expected success for " + label + ": " + (result == null ? "null" : result.ToString()));
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

        static void CaptureRedError(string condition, string stackTrace, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
                RedErrors.Add(condition);
        }
    }
}
