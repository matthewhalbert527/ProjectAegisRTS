using System;
using System.Collections.Generic;
using ProjectAegisRTS.Core;
using ProjectAegisRTS.Data;
using ProjectAegisRTS.Match;
using ProjectAegisRTS.Scenarios;
using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.UnityClient.Boot;
using ProjectAegisRTS.UnityClient.Bootstrap;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Rendering;
using ProjectAegisRTS.UnityClient.Scenario;
using ProjectAegisRTS.UnityClient.UI.Common;
using ProjectAegisRTS.UnityClient.UI.Desktop;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage19PlayModeSmokeValidator
    {
        static readonly List<string> RedErrors = new List<string>();

        public static void RunStage19PlayModeSmokeBatch()
        {
            try
            {
                RunStage19PlayModeSmoke();
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

        public static void RunStage19PlayModeSmoke()
        {
            RedErrors.Clear();
            Application.logMessageReceived += CaptureRedError;
            try
            {
                Stage19SceneValidator.ValidateStage19Scene();
                ValidateBootRuntime();
                ValidateStage16Runtime();

                if (RedErrors.Count > 0)
                    throw new InvalidOperationException("Red console errors were produced during Stage 19 smoke validation: " + string.Join(" | ", RedErrors.ToArray()));

                Debug.Log("Stage 19 play mode smoke validation passed.");
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
                throw new InvalidOperationException("Stage 19 boot scene did not open for smoke validation.");

            var menu = RequireEnabled<MainMenuHud>("MainMenuHud");
            RequireEnabled<GameBootController>("GameBootController");
            if (!menu.visible)
                throw new InvalidOperationException("Stage 19 boot menu is not visible.");
        }

        static void ValidateStage16Runtime()
        {
            var scene = EditorSceneManager.OpenScene(Stage16SceneCreator.ScenePath);
            if (!scene.IsValid())
                throw new InvalidOperationException("Stage 19 Stage16 scene did not open for smoke validation.");

            var bootstrapper = RequireEnabled<RtsGameBootstrapper>("RtsGameBootstrapper");
            var driver = RequireEnabled<RtsSimulationDriver>("RtsSimulationDriver");
            var mapper = RequireEnabled<BoardCoordinateMapper>("BoardCoordinateMapper");
            var boardRenderer = RequireEnabled<BoardRenderer>("BoardRenderer");
            var actorRenderer = RequireEnabled<ActorRenderSystem>("ActorRenderSystem");
            var missionFlow = RequireEnabled<VerticalSliceMissionFlowController>("VerticalSliceMissionFlowController");
            var progress = RequireEnabled<VerticalSliceProgressTracker>("VerticalSliceProgressTracker");
            var checklist = RequireEnabled<VerticalSliceChecklistHud>("VerticalSliceChecklistHud");
            var promptSystem = RequireEnabled<PlayerPromptSystem>("PlayerPromptSystem");
            var promptHud = RequireEnabled<PlayerPromptHud>("PlayerPromptHud");
            var objectiveHud = RequireEnabled<PlayerObjectiveHud>("PlayerObjectiveHud");
            var resultHud = RequireEnabled<MatchResultHud>("MatchResultHud");
            var debugVisibility = RequireEnabled<DebugHudVisibilityController>("DebugHudVisibilityController");
            var desktopHud = RequireEnabled<DesktopRtsHudRoot>("DesktopRtsHudRoot");
            var placementPanel = RequireComponent<PlacementModePanel>("PlacementModePanel");

            bootstrapper.InitializeScene();
            StepRuntime(driver, boardRenderer, actorRenderer, 6, 0.05f);

            if (mapper.PlacementGridScale != 2 || mapper.PlacementBoardWidth != mapper.BoardWidth * 2)
                throw new InvalidOperationException("Stage 19 mapper did not initialize the fine placement grid.");
            if (boardRenderer.FineGridLineCount <= 0)
                throw new InvalidOperationException("Stage 19 board renderer did not create fine grid lines.");
            if (!checklist.visible || !promptSystem.visible || !promptHud.visible || !objectiveHud.visible || !resultHud.visible)
                throw new InvalidOperationException("Stage 19 player HUDs are not visible.");
            if (desktopHud.showDebugOverlay)
                throw new InvalidOperationException("Stage 19 debug overlay is visible by default.");
            if (!debugVisibility.AreDebugPanelsHiddenByDefault())
                throw new InvalidOperationException("Stage 19 debug panels are not hidden by default.");
            if (placementPanel.gameObject.activeInHierarchy)
                throw new InvalidOperationException("Stage 19 placement UI is visible before placement mode.");

            var initialSnapshot = RequireSnapshot(driver);
            if (initialSnapshot.Actors.Count == 0)
                throw new InvalidOperationException("Stage 19 expected visible actors/buildings.");
            if (initialSnapshot.Economy == null || initialSnapshot.Economy.Resources.Count == 0)
                throw new InvalidOperationException("Stage 19 expected visible resources.");

            missionFlow.Refresh();
            if (string.IsNullOrEmpty(missionFlow.CurrentBeatId) || string.IsNullOrEmpty(missionFlow.CurrentInstructionText))
                throw new InvalidOperationException("Stage 19 mission flow did not expose a current beat and instruction.");
            if (missionFlow.CurrentBeatIndex < 1)
                throw new InvalidOperationException("Stage 19 mission flow did not advance past the welcome beat after scene start.");

            RequireSuccess(driver.TrySelectFirstOwnedActorOfType("fabrication_hub"), "select Fabrication Hub");
            progress.Refresh();
            if (!progress.hasSelectedFabricationHub)
                throw new InvalidOperationException("Stage 19 progress tracker did not detect Fabrication Hub selection.");

            ValidateFineGridPlacement(driver, boardRenderer, actorRenderer, progress);
            ValidateNormalVictoryPath(driver, boardRenderer, actorRenderer, progress);
        }

        static void ValidateFineGridPlacement(RtsSimulationDriver driver, BoardRenderer boardRenderer, ActorRenderSystem actorRenderer, VerticalSliceProgressTracker progress)
        {
            RequireSuccess(driver.TryQueueProduction("power_plant"), "queue power plant");
            StepUntilPendingPlacement(driver, boardRenderer, actorRenderer, "power_plant");
            RequireSuccess(driver.TryEnterPlacementModeForFirstPending(), "enter placement mode");

            var validCell = FindValidPlacementCell(driver, boardRenderer, actorRenderer);
            driver.SetHoveredCell(validCell, true);
            PlacementPreviewSnapshot preview;
            if (!driver.TryGetPlacementPreview(out preview) || !preview.CanPlace)
                throw new InvalidOperationException("Stage 19 expected a valid fine-grid placement preview.");
            if (preview.PlacementFootprintCells.X != 4 || preview.PlacementFootprintCells.Y != 4 || preview.FootprintCells.Count != 16)
                throw new InvalidOperationException("Stage 19 power plant preview did not expose the 4x4 fine footprint.");

            boardRenderer.UpdateHover(validCell, true);
            boardRenderer.UpdatePlacementPreview(preview);
            RequireSuccess(driver.TryPlacePendingBuildingAtCell(validCell), "place power plant");
            StepRuntime(driver, boardRenderer, actorRenderer, 4, 0.05f);

            progress.Refresh();
            if (!progress.hasPlacedPowerPlant)
                throw new InvalidOperationException("Stage 19 progress tracker did not detect fine-grid power plant placement.");
        }

        static void ValidateNormalVictoryPath(RtsSimulationDriver driver, BoardRenderer boardRenderer, ActorRenderSystem actorRenderer, VerticalSliceProgressTracker progress)
        {
            RequireSuccess(driver.TrySelectOwnedCombatGroup(), "select combat group");
            RequireSuccess(driver.TryIssueMoveSelectedToCell(new Int2(20, 53)), "move combat group to enemy staging cell");
            StepRuntime(driver, boardRenderer, actorRenderer, 700, 0.05f);

            var enemyHub = FindActor(driver.LatestSnapshot, "fabrication_hub", 2);
            if (enemyHub == null)
                throw new InvalidOperationException("Stage 19 expected enemy Fabrication Hub to be visible after scouting.");

            var attackIds = FindOwnedCombatActorsInRange(driver, enemyHub);
            if (attackIds.Count < 3)
                throw new InvalidOperationException("Stage 19 expected at least three combat units in enemy hub attack range.");

            RequireSuccess(driver.SetSelectedActorIds(attackIds), "select in-range combat group");
            RequireSuccess(driver.TryIssueAttackSelectedToActor(enemyHub.ActorId), "attack enemy Fabrication Hub");

            for (var i = 0; i < 1000; i++)
            {
                StepRuntime(driver, boardRenderer, actorRenderer, 1, 0.05f);
                var snapshot = RequireSnapshot(driver);
                if (snapshot.Match.Phase == MatchPhase.Won)
                    break;
            }

            var finalSnapshot = RequireSnapshot(driver);
            progress.Refresh();
            if (finalSnapshot.Match.Phase != MatchPhase.Won || finalSnapshot.Match.LocalPlayerOutcome != PlayerOutcome.Victory)
                throw new InvalidOperationException("Stage 19 normal combat did not reach a victory match outcome.");
            if (ObjectiveState(finalSnapshot, "destroy_enemy_base") != ScenarioObjectiveState.Completed)
                throw new InvalidOperationException("Stage 19 destroy objective did not complete after victory.");
            if (!progress.hasWon || !progress.enemyBaseDestroyed)
                throw new InvalidOperationException("Stage 19 progress tracker did not agree with match victory.");
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

            throw new InvalidOperationException("No valid Stage 19 placement cell was found.");
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

        static List<int> FindOwnedCombatActorsInRange(RtsSimulationDriver driver, ActorSnapshot target)
        {
            var result = new List<int>();
            var snapshot = RequireSnapshot(driver);
            for (var i = 0; i < snapshot.Actors.Count; i++)
            {
                var actor = snapshot.Actors[i];
                if (actor.OwnerId != driver.PlayerId || actor.IsDestroyed)
                    continue;

                ActorDefinition definition;
                if (!driver.TryGetDefinition(actor.TypeId, out definition) || !(definition is UnitDefinition) || definition.Weapon == null || !definition.Weapon.CanTargetBuildings)
                    continue;

                if (actor.CellPosition.ManhattanDistanceTo(target.CellPosition) <= definition.Weapon.RangeCells)
                    result.Add(actor.ActorId);
            }

            result.Sort();
            return result;
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

        static ScenarioObjectiveState ObjectiveState(WorldSnapshot snapshot, string objectiveId)
        {
            if (snapshot == null || snapshot.Scenario == null)
                return ScenarioObjectiveState.Inactive;

            for (var i = 0; i < snapshot.Scenario.Objectives.Count; i++)
                if (snapshot.Scenario.Objectives[i].ObjectiveId == objectiveId)
                    return snapshot.Scenario.Objectives[i].State;

            return ScenarioObjectiveState.Inactive;
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
                throw new InvalidOperationException("Stage 19 expected a simulation snapshot.");
            return driver.LatestSnapshot;
        }

        static void RequireSuccess(RtsCommandResult result, string label)
        {
            if (result == null || !result.Success)
                throw new InvalidOperationException("Stage 19 expected success for " + label + ": " + (result == null ? "null" : result.ToString()));
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
