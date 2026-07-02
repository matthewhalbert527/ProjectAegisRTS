using System;
using System.Collections.Generic;
using ProjectAegisRTS.Core;
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
    public static class Stage26PlayModeSmokeValidator
    {
        static readonly List<string> RedErrors = new List<string>();

        public static void RunStage26PlayModeSmokeBatch()
        {
            try
            {
                RunStage26PlayModeSmoke();
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

        public static void RunStage26PlayModeSmoke()
        {
            RedErrors.Clear();
            Application.logMessageReceived += CaptureRedError;
            try
            {
                Stage26AirNavalValidator.ValidateStage26AirNaval();
                ValidateStage16RuntimeAirNaval();

                if (RedErrors.Count > 0)
                    throw new InvalidOperationException("Red console errors were produced during Stage 26 smoke validation: " + string.Join(" | ", RedErrors.ToArray()));

                Debug.Log("Stage 26 play mode smoke validation passed.");
            }
            finally
            {
                Application.logMessageReceived -= CaptureRedError;
                RedErrors.Clear();
            }
        }

        static void ValidateStage16RuntimeAirNaval()
        {
            var scene = EditorSceneManager.OpenScene(Stage16SceneCreator.ScenePath);
            if (!scene.IsValid())
                throw new InvalidOperationException("Stage 26 Stage16 scene did not open for runtime smoke.");

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
                throw new InvalidOperationException("Stage 26 debug panels became visible during smoke validation.");

            var aircraft = FindActor(RequireSnapshot(driver), "attack_aircraft", 1);
            if (aircraft == null)
                throw new InvalidOperationException("Stage 26 aircraft missing from vertical-slice smoke snapshot.");

            var dockedState = FindAircraft(RequireSnapshot(driver), aircraft.ActorId);
            if (dockedState == null || dockedState.DockedAirfieldActorId <= 0 || dockedState.AltitudeSubCells != 0)
                throw new InvalidOperationException("Stage 26 aircraft should begin docked at zero core altitude.");

            RequireSuccess(driver.TrySelectFirstOwnedActorOfType("attack_aircraft"), "select attack aircraft");
            RequireSuccess(driver.TryIssueMoveSelectedToCell(new Int2(22, 6)), "move attack aircraft over water lane");
            StepRuntime(driver, boardRenderer, actorRenderer, 12, 0.05f);

            var airborneState = FindAircraft(RequireSnapshot(driver), aircraft.ActorId);
            if (airborneState == null || airborneState.DockedAirfieldActorId != 0 || airborneState.AltitudeSubCells <= 0 || !airborneState.IsAirborne)
                throw new InvalidOperationException("Stage 26 aircraft did not switch to airborne snapshot state after a move order.");

            ActorViewBehaviour view;
            if (!actorRenderer.TryGetActorView(aircraft.ActorId, out view) || view == null || view.AircraftMotion == null || !view.AircraftMotion.UsesLogicAltitude || view.AircraftMotion.IsDocked || view.AircraftMotion.AltitudeOffset <= 0.1f)
                throw new InvalidOperationException("Stage 26 aircraft visual altitude did not follow the core aircraft snapshot.");
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
                throw new InvalidOperationException("Stage 26 expected a current runtime snapshot.");
            return driver.LatestSnapshot;
        }

        static ActorSnapshot FindActor(WorldSnapshot snapshot, string typeId, int ownerId)
        {
            for (var i = 0; i < snapshot.Actors.Count; i++)
                if (snapshot.Actors[i].TypeId == typeId && snapshot.Actors[i].OwnerId == ownerId)
                    return snapshot.Actors[i];
            return null;
        }

        static AircraftSnapshot FindAircraft(WorldSnapshot snapshot, int actorId)
        {
            for (var i = 0; i < snapshot.Aircraft.Count; i++)
                if (snapshot.Aircraft[i].ActorId == actorId)
                    return snapshot.Aircraft[i];
            return null;
        }

        static void RequireSuccess(RtsCommandResult result, string action)
        {
            if (result == null || !result.Success)
                throw new InvalidOperationException("Stage 26 failed to " + action + ": " + (result == null ? "no result" : result.ToString()));
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
