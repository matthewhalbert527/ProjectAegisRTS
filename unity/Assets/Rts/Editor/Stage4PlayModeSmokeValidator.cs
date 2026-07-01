using System;
using System.Collections.Generic;
using ProjectAegisRTS.Core;
using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.UnityClient.Bootstrap;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.InputControls.XR;
using ProjectAegisRTS.UnityClient.Rendering;
using ProjectAegisRTS.UnityClient.Selection;
using ProjectAegisRTS.UnityClient.UI.XR.LeftHand;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage4PlayModeSmokeValidator
    {
        static readonly List<string> RedErrors = new List<string>();

        public static void RunStage4PlayModeSmokeBatch()
        {
            try
            {
                RunStage4PlayModeSmoke();
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

        public static void RunStage4PlayModeSmoke()
        {
            RedErrors.Clear();
            Application.logMessageReceived += CaptureRedError;
            try
            {
                Stage4SceneValidator.ValidateStage4Scene();
                var scene = EditorSceneManager.OpenScene(Stage4SceneCreator.ScenePath);
                if (!scene.IsValid())
                    throw new InvalidOperationException("Stage 4 scene did not open.");

                var bootstrapper = RequireEnabled<RtsGameBootstrapper>("RtsGameBootstrapper");
                var driver = RequireEnabled<RtsSimulationDriver>("RtsSimulationDriver");
                var boardRenderer = RequireEnabled<BoardRenderer>("BoardRenderer");
                var actorRenderer = RequireEnabled<ActorRenderSystem>("ActorRenderSystem");
                var mapper = RequireEnabled<BoardCoordinateMapper>("BoardCoordinateMapper");
                var rig = RequireEnabled<SimulatedLeftHandRig>("SimulatedLeftHandRig");
                var buildMenu = RequireEnabled<LeftHandBuildMenuController>("LeftHandBuildMenuController");
                var router = RequireEnabled<LeftHandCommandRouter>("LeftHandCommandRouter");
                var coordinator = RequireEnabled<Stage4ModeCoordinator>("Stage4ModeCoordinator");
                var selection = RequireEnabled<LeftHandSelectionController>("LeftHandSelectionController");
                var lasso = RequireEnabled<LeftHandLassoSelectionController>("LeftHandLassoSelectionController");

                bootstrapper.InitializeScene();
                rig.EnsureRig();
                coordinator.InitializeIfNeeded();
                StepRuntime(driver, boardRenderer, actorRenderer, 8, 0.1f);

                var boardRoot = GameObject.Find("BoardRoot");
                if (boardRoot == null || boardRoot.transform.Find("Board Surface") == null)
                    throw new InvalidOperationException("Stage 4 board visuals were not generated.");

                var actorViews = GameObject.Find("Actor Views");
                if (actorViews == null || actorViews.transform.childCount == 0)
                    throw new InvalidOperationException("Stage 4 actor visuals were not generated.");

                if (driver.LatestSnapshot == null || driver.LatestSnapshot.Actors.Count == 0)
                    throw new InvalidOperationException("Stage 4 simulation snapshot missing actors.");

                var tick = driver.LatestSnapshot.Tick;
                StepRuntime(driver, boardRenderer, actorRenderer, 4, 0.1f);
                if (driver.LatestSnapshot.Tick <= tick)
                    throw new InvalidOperationException("Stage 4 simulation tick did not advance.");

                buildMenu.OpenMenu();
                if (!buildMenu.IsOpen)
                    throw new InvalidOperationException("Stage 4 left-hand menu did not open.");
                buildMenu.CloseMenu();
                if (buildMenu.IsOpen)
                    throw new InvalidOperationException("Stage 4 left-hand menu did not close.");
                buildMenu.OpenMenu();
                buildMenu.SetCategory(LeftHandBuildCategory.Defenses);
                if (buildMenu.ActiveCategory != LeftHandBuildCategory.Defenses)
                    throw new InvalidOperationException("Stage 4 category switch failed.");
                buildMenu.SetCategory(LeftHandBuildCategory.Buildings);
                if (buildMenu.GetActiveCategoryItems().Count == 0)
                    throw new InvalidOperationException("Stage 4 build item list did not populate.");

                var queueResult = router.QueueProduction("power_plant");
                if (!queueResult.Success)
                    throw new InvalidOperationException("Stage 4 queue power_plant failed: " + queueResult);

                StepUntilPendingPlacement(driver, boardRenderer, actorRenderer, "power_plant");
                var enterPlacement = router.EnterPlacementMode("power_plant");
                if (!enterPlacement.Success || !driver.HasPlacementMode)
                    throw new InvalidOperationException("Stage 4 placement mode did not activate: " + enterPlacement);

                PlacementPreviewSnapshot preview;
                var invalid = router.ConfirmPlacementAtCell(new Int2(-1, -1));
                if (invalid.Success)
                    throw new InvalidOperationException("Stage 4 invalid placement unexpectedly succeeded.");

                var validCell = FindValidPlacementCell(driver, out preview);
                boardRenderer.SetPlacementPreview(preview.TypeId, preview.FootprintCells, preview.TopLeftCell, preview.CanPlace, preview.ErrorCode);
                boardRenderer.ClearPlacementPreview();
                var placed = router.ConfirmPlacementAtCell(validCell);
                if (!placed.Success)
                    throw new InvalidOperationException("Stage 4 valid placement failed: " + placed);

                var firstActor = driver.LatestSnapshot.Actors[0];
                selection.RefreshCandidatesForCell(firstActor.CellPosition);
                if (selection.Candidates.Count == 0)
                    throw new InvalidOperationException("Stage 4 selection candidate search returned no candidates.");
                selection.CycleCandidate(1);
                var selected = selection.SelectCurrentCandidate(false);
                if (!selected.Success || driver.SelectedActorIds.Count == 0)
                    throw new InvalidOperationException("Stage 4 selecting candidate failed: " + selected);

                lasso.StartLasso(firstActor.CellPosition);
                if (!lasso.IsActive)
                    throw new InvalidOperationException("Stage 4 lasso did not start.");
                lasso.UpdateLasso(firstActor.CellPosition);
                lasso.CancelLasso();
                if (lasso.IsActive)
                    throw new InvalidOperationException("Stage 4 lasso did not cancel.");

                coordinator.SetMode(LeftHandCommandMode.SelectionRay);
                coordinator.CancelActiveMode();

                if (RedErrors.Count > 0)
                    throw new InvalidOperationException("Red console errors were produced during Stage 4 smoke validation: " + string.Join(" | ", RedErrors.ToArray()));

                Debug.Log("Stage 4 play mode smoke validation passed.");
            }
            finally
            {
                Application.logMessageReceived -= CaptureRedError;
                RedErrors.Clear();
            }
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

        static Int2 FindValidPlacementCell(RtsSimulationDriver driver, out PlacementPreviewSnapshot preview)
        {
            for (var y = 0; y < 32; y++)
            {
                for (var x = 0; x < 32; x++)
                {
                    var cell = new Int2(x, y);
                    if (driver.IsPlacementValidAtCell(cell, out preview))
                        return preview.TopLeftCell;
                }
            }

            preview = null;
            throw new InvalidOperationException("No valid Stage 4 placement cell was found.");
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
