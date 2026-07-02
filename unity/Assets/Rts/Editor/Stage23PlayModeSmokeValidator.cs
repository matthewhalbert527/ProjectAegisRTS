using System;
using System.Collections.Generic;
using ProjectAegisRTS.Core;
using ProjectAegisRTS.Power;
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
    public static class Stage23PlayModeSmokeValidator
    {
        static readonly List<string> RedErrors = new List<string>();

        public static void RunStage23PlayModeSmokeBatch()
        {
            try
            {
                RunStage23PlayModeSmoke();
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

        public static void RunStage23PlayModeSmoke()
        {
            RedErrors.Clear();
            Application.logMessageReceived += CaptureRedError;
            try
            {
                Stage23BaseManagementValidator.ValidateStage23BaseManagement();
                ValidateStage16RuntimeBaseManagement();

                if (RedErrors.Count > 0)
                    throw new InvalidOperationException("Red console errors were produced during Stage 23 smoke validation: " + string.Join(" | ", RedErrors.ToArray()));

                Debug.Log("Stage 23 play mode smoke validation passed.");
            }
            finally
            {
                Application.logMessageReceived -= CaptureRedError;
                RedErrors.Clear();
            }
        }

        static void ValidateStage16RuntimeBaseManagement()
        {
            var scene = EditorSceneManager.OpenScene(Stage16SceneCreator.ScenePath);
            if (!scene.IsValid())
                throw new InvalidOperationException("Stage 23 Stage16 scene did not open for runtime base-management smoke.");

            var bootstrapper = Require<RtsGameBootstrapper>("RtsGameBootstrapper");
            var driver = Require<RtsSimulationDriver>("RtsSimulationDriver");
            var boardRenderer = Require<BoardRenderer>("BoardRenderer");
            var actorRenderer = Require<ActorRenderSystem>("ActorRenderSystem");
            var desktopHud = Require<DesktopRtsHudRoot>("DesktopRtsHudRoot");
            var router = Require<DesktopUiCommandRouter>("DesktopUiCommandRouter");
            var layout = Require<CncStyleSidebarLayout>("CncStyleSidebarLayout");
            var placementPanel = Require<PlacementModePanel>("PlacementModePanel");
            var debugVisibility = Require<DebugHudVisibilityController>("DebugHudVisibilityController");

            bootstrapper.InitializeScene();
            desktopHud.Initialize();
            layout.ApplyLayout();
            StepRuntime(driver, boardRenderer, actorRenderer, 4, 0.05f);

            if (!debugVisibility.AreDebugPanelsHiddenByDefault())
                throw new InvalidOperationException("Stage 23 debug panels became visible during smoke validation.");
            if (placementPanel.gameObject.activeInHierarchy)
                throw new InvalidOperationException("Stage 23 placement UI became visible during base-management smoke.");

            ValidateRepair(driver, boardRenderer, actorRenderer, router);
            ValidatePowerToggle(driver, router);
            ValidateRallyPoint(driver, router);
            ValidateSell(driver, router);

            StepRuntime(driver, boardRenderer, actorRenderer, 2, 0.05f);
        }

        static void ValidateRepair(RtsSimulationDriver driver, BoardRenderer boardRenderer, ActorRenderSystem actorRenderer, DesktopUiCommandRouter router)
        {
            var tower = FindSnapshotActor(driver, "gun_tower", 1);
            RequireSuccess(driver.TryApplyScenarioDamage(tower.ActorId, 80, "stage23_repair_smoke"), "damage owned gun tower");
            tower = FindSnapshotActor(driver, "gun_tower", 1);
            var damagedHealth = tower.Health;
            RequireSuccess(driver.SetSelectedActorIds(new[] { tower.ActorId }), "select damaged gun tower");
            RequireSuccess(router.RepairSelected(), "repair selected building");
            StepRuntime(driver, boardRenderer, actorRenderer, 3, 0.12f);

            var repaired = FindSnapshotActor(driver, "gun_tower", 1);
            if (repaired.Health <= damagedHealth)
                throw new InvalidOperationException("Stage 23 repair did not restore building health.");
            if (!repaired.IsRepairing && repaired.Health < repaired.MaxHealth)
                throw new InvalidOperationException("Stage 23 repair cleared before the building reached max health.");
            if (repaired.RepairSpentCredits <= 0)
                throw new InvalidOperationException("Stage 23 repair did not spend credits.");
        }

        static void ValidatePowerToggle(RtsSimulationDriver driver, DesktopUiCommandRouter router)
        {
            var barracks = FindSnapshotActor(driver, "barracks", 1);
            var playerBefore = FindPlayer(driver, 1);
            RequireSuccess(driver.SetSelectedActorIds(new[] { barracks.ActorId }), "select barracks");
            RequireSuccess(router.TogglePowerSelected(), "power toggle selected building");

            var poweredOff = FindSnapshotActor(driver, "barracks", 1);
            var playerAfter = FindPlayer(driver, 1);
            if (!poweredOff.IsManuallyPoweredOff || poweredOff.IsPowered)
                throw new InvalidOperationException("Stage 23 power toggle did not mark the selected building powered off.");
            if (playerBefore.Power == null || playerAfter.Power == null || playerAfter.Power.Consumed >= playerBefore.Power.Consumed)
                throw new InvalidOperationException("Stage 23 power toggle did not lower player power consumption.");
            if (playerAfter.Power.State == PlayerPowerState.Offline)
                throw new InvalidOperationException("Stage 23 power toggle unexpectedly drove the player fully offline.");
        }

        static void ValidateRallyPoint(RtsSimulationDriver driver, DesktopUiCommandRouter router)
        {
            var factory = FindSnapshotActor(driver, "war_factory", 1);
            var rally = new Int2(16, 12);
            RequireSuccess(driver.SetSelectedActorIds(new[] { factory.ActorId }), "select war factory");
            router.SetRallyMode();
            if (router.CurrentMode != DesktopCommandMode.Rally)
                throw new InvalidOperationException("Stage 23 router did not enter rally mode.");
            RequireSuccess(router.IssueRallyToCell(rally), "set rally point");

            var updatedFactory = FindSnapshotActor(driver, "war_factory", 1);
            if (!updatedFactory.RallyPoint.Equals(rally))
                throw new InvalidOperationException("Stage 23 rally point was not exposed in the actor snapshot.");
        }

        static void ValidateSell(RtsSimulationDriver driver, DesktopUiCommandRouter router)
        {
            var tower = FindSnapshotActor(driver, "gun_tower", 1);
            var creditsBefore = FindPlayer(driver, 1).Credits;
            RequireSuccess(driver.SetSelectedActorIds(new[] { tower.ActorId }), "select gun tower for sell");
            RequireSuccess(router.SellSelected(), "sell selected building");

            if (TryFindSnapshotActor(driver, tower.ActorId, out var sold))
                throw new InvalidOperationException("Stage 23 sold building still appears in the snapshot: " + sold.ActorId);
            if (FindPlayer(driver, 1).Credits <= creditsBefore)
                throw new InvalidOperationException("Stage 23 sell command did not refund credits.");
            if (driver.SelectedActorIds.Count != 0)
                throw new InvalidOperationException("Stage 23 sell command did not clear stale building selection.");
        }

        static ActorSnapshot FindSnapshotActor(RtsSimulationDriver driver, string typeId, int ownerId)
        {
            var snapshot = RequireSnapshot(driver);
            for (var i = 0; i < snapshot.Actors.Count; i++)
            {
                var actor = snapshot.Actors[i];
                if (actor.TypeId == typeId && actor.OwnerId == ownerId && !actor.IsDestroyed)
                    return actor;
            }

            throw new InvalidOperationException("Stage 23 actor not found: " + typeId + " owner " + ownerId);
        }

        static bool TryFindSnapshotActor(RtsSimulationDriver driver, int actorId, out ActorSnapshot actor)
        {
            actor = null;
            var snapshot = RequireSnapshot(driver);
            for (var i = 0; i < snapshot.Actors.Count; i++)
            {
                if (snapshot.Actors[i].ActorId == actorId)
                {
                    actor = snapshot.Actors[i];
                    return true;
                }
            }

            return false;
        }

        static PlayerSnapshot FindPlayer(RtsSimulationDriver driver, int playerId)
        {
            var snapshot = RequireSnapshot(driver);
            for (var i = 0; i < snapshot.Players.Count; i++)
                if (snapshot.Players[i].PlayerId == playerId)
                    return snapshot.Players[i];

            throw new InvalidOperationException("Stage 23 player snapshot not found: " + playerId);
        }

        static WorldSnapshot RequireSnapshot(RtsSimulationDriver driver)
        {
            if (driver.LatestSnapshot == null)
                throw new InvalidOperationException("Stage 23 runtime snapshot is missing.");
            return driver.LatestSnapshot;
        }

        static void RequireSuccess(RtsCommandResult result, string label)
        {
            if (result == null || !result.Success)
                throw new InvalidOperationException("Stage 23 command failed during smoke validation: " + label + " -> " + (result == null ? "null" : result.ToString()));
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
