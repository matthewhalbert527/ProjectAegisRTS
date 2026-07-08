using System;
using System.Collections.Generic;
using ProjectAegisRTS.Core;
using ProjectAegisRTS.Data;
using ProjectAegisRTS.Snapshots;
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
    public static class Stage25PlayModeSmokeValidator
    {
        static readonly List<string> RedErrors = new List<string>();

        public static void RunStage25PlayModeSmokeBatch()
        {
            try
            {
                RunStage25PlayModeSmoke();
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

        public static void RunStage25PlayModeSmoke()
        {
            RedErrors.Clear();
            Application.logMessageReceived += CaptureRedError;
            try
            {
                Stage25EngineerTransportValidator.ValidateStage25EngineerTransport();
                ValidateStage16RuntimeEngineerTransport();

                if (RedErrors.Count > 0)
                    throw new InvalidOperationException("Red console errors were produced during Stage 25 smoke validation: " + string.Join(" | ", RedErrors.ToArray()));

                Debug.Log("Stage 25 play mode smoke validation passed.");
            }
            finally
            {
                Application.logMessageReceived -= CaptureRedError;
                RedErrors.Clear();
            }
        }

        static void ValidateStage16RuntimeEngineerTransport()
        {
            var scene = EditorSceneManager.OpenScene(Stage16SceneCreator.ScenePath);
            if (!scene.IsValid())
                throw new InvalidOperationException("Stage 25 Stage16 scene did not open for runtime smoke.");

            var bootstrapper = Require<RtsGameBootstrapper>("RtsGameBootstrapper");
            var driver = Require<RtsSimulationDriver>("RtsSimulationDriver");
            var boardRenderer = Require<BoardRenderer>("BoardRenderer");
            var actorRenderer = Require<ActorRenderSystem>("ActorRenderSystem");
            var desktopHud = Require<DesktopRtsHudRoot>("DesktopRtsHudRoot");
            var layout = Require<CncStyleSidebarLayout>("CncStyleSidebarLayout");
            var debugVisibility = Require<DebugHudVisibilityController>("DebugHudVisibilityController");

            bootstrapper.InitializeScene();
            desktopHud.Initialize();
            layout.ApplyLayout();
            StepRuntime(driver, boardRenderer, actorRenderer, 4, 0.05f);

            if (!debugVisibility.AreDebugPanelsHiddenByDefault())
                throw new InvalidOperationException("Stage 25 debug panels became visible during smoke validation.");

            var apc = FindActor(RequireSnapshot(driver), "apc", 1);
            if (apc == null)
                throw new InvalidOperationException("Stage 25 APC missing from vertical-slice smoke snapshot.");

            RequireSuccess(driver.TryActivateSupportPowerAtCell("reveal_scan", new Int2(22, 20)), "reveal enemy base for capture smoke");
            Int2 captureTargetCell;
            if (!TryFindEnemyCaptureableBuildingCell(RequireSnapshot(driver), driver.Rules, driver.PlayerId, out captureTargetCell))
                throw new InvalidOperationException("Stage 25 capture smoke could not find a visible enemy captureable building after Reveal Scan.");

            RequireSuccess(driver.TrySelectFirstOwnedActorOfType("engineer"), "select engineer");
            var capture = driver.TryCaptureSelectedAtCell(captureTargetCell);
            if (!capture.Success)
                throw new InvalidOperationException("Stage 25 capture routing failed: " + capture);

            RequireSuccess(driver.TrySelectFirstOwnedActorOfType("rifle_infantry"), "select infantry passenger");
            RequireSuccess(driver.TryLoadSelectedIntoTransportAtCell(apc.CellPosition), "load infantry into APC");

            if (!WaitForTransportPassengerCount(driver, boardRenderer, actorRenderer, apc.ActorId, 1, 180))
                throw new InvalidOperationException("Stage 25 transport snapshot did not show loaded passenger.");

            RequireSuccess(driver.TryUnloadSelectedTransportAtCell(new Int2(apc.CellPosition.X + 1, apc.CellPosition.Y)), "unload APC");
            StepRuntime(driver, boardRenderer, actorRenderer, 2, 0.05f);

            var unloaded = FindTransport(RequireSnapshot(driver), apc.ActorId);
            if (unloaded == null || unloaded.PassengerActorIds.Count != 0)
                throw new InvalidOperationException("Stage 25 transport snapshot did not clear passengers after unload.");
        }

        static void StepRuntime(RtsSimulationDriver driver, BoardRenderer boardRenderer, ActorRenderSystem actorRenderer, int steps, float deltaSeconds)
        {
            for (var i = 0; i < steps; i++)
            {
                driver.ManualUpdate(deltaSeconds);
                boardRenderer.UpdateHover(driver.HasHoveredCell ? (Int2?)driver.HoveredCell : null, driver.HoveredCellIsPlacementCell);
                PlacementPreviewSnapshot preview;
                boardRenderer.UpdatePlacementPreview(driver.TryGetPlacementPreview(out preview) ? preview : null);
                actorRenderer.RenderSnapshot(driver.LatestSnapshot, driver.SelectedActorIds, deltaSeconds);
            }
        }

        static WorldSnapshot RequireSnapshot(RtsSimulationDriver driver)
        {
            if (driver == null || driver.LatestSnapshot == null)
                throw new InvalidOperationException("Stage 25 expected a current runtime snapshot.");
            return driver.LatestSnapshot;
        }

        static ActorSnapshot FindActor(WorldSnapshot snapshot, string typeId, int ownerId)
        {
            for (var i = 0; i < snapshot.Actors.Count; i++)
                if (snapshot.Actors[i].TypeId == typeId && snapshot.Actors[i].OwnerId == ownerId)
                    return snapshot.Actors[i];
            return null;
        }

        static TransportSnapshot FindTransport(WorldSnapshot snapshot, int actorId)
        {
            for (var i = 0; i < snapshot.Transports.Count; i++)
                if (snapshot.Transports[i].ActorId == actorId)
                    return snapshot.Transports[i];
            return null;
        }

        static bool WaitForTransportPassengerCount(RtsSimulationDriver driver, BoardRenderer boardRenderer, ActorRenderSystem actorRenderer, int actorId, int expectedMinimumPassengers, int maxSteps)
        {
            for (var i = 0; i < maxSteps; i++)
            {
                StepRuntime(driver, boardRenderer, actorRenderer, 1, 0.05f);
                var transport = FindTransport(RequireSnapshot(driver), actorId);
                if (transport != null && transport.PassengerActorIds.Count >= expectedMinimumPassengers)
                    return true;
            }

            return false;
        }

        static bool TryFindEnemyCaptureableBuildingCell(WorldSnapshot snapshot, RtsRules rules, int playerId, out Int2 cell)
        {
            cell = Int2.Zero;
            if (snapshot == null || rules == null)
                return false;

            for (var i = 0; i < snapshot.Actors.Count; i++)
            {
                var actor = snapshot.Actors[i];
                ActorDefinition definition;
                if (actor.OwnerId == playerId || actor.IsDestroyed || !rules.TryGetDefinition(actor.TypeId, out definition))
                    continue;

                var building = definition as BuildingDefinition;
                if (building == null || building.Captureable == null || !building.Captureable.CanBeCaptured)
                    continue;

                cell = actor.CellPosition;
                return true;
            }

            return false;
        }

        static void RequireSuccess(RtsCommandResult result, string action)
        {
            if (result == null || !result.Success)
                throw new InvalidOperationException("Stage 25 failed to " + action + ": " + (result == null ? "no result" : result.ToString()));
        }

        static void CaptureRedError(string condition, string stackTrace, LogType type)
        {
            if (type == LogType.Exception || type == LogType.Error || type == LogType.Assert)
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
