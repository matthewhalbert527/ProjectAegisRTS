using System;
using System.Collections.Generic;
using ProjectAegisRTS.Core;
using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.UnityClient.Board;
using ProjectAegisRTS.UnityClient.Boot;
using ProjectAegisRTS.UnityClient.Bootstrap;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Rendering;
using ProjectAegisRTS.UnityClient.UI.Common;
using ProjectAegisRTS.UnityClient.UI.Desktop;
using ProjectAegisRTS.UnityClient.UI.XR;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage28PlayModeSmokeValidator
    {
        static readonly List<string> RedErrors = new List<string>();

        public static void RunStage28PlayModeSmokeBatch()
        {
            try
            {
                RunStage28PlayModeSmoke();
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

        public static void RunStage28PlayModeSmoke()
        {
            RedErrors.Clear();
            Application.logMessageReceived += CaptureRedError;
            try
            {
                ValidateBootRuntime();
                ValidateStage16IntegratedSmoke();

                if (RedErrors.Count > 0)
                    throw new InvalidOperationException("Red console errors were produced during Stage 28 smoke validation: " + string.Join(" | ", RedErrors.ToArray()));

                Debug.Log("Stage 28 play mode smoke validation passed.");
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
                throw new InvalidOperationException("Stage 28 boot scene did not open for smoke validation.");

            var menu = Require<MainMenuHud>("MainMenuHud");
            Require<GameBootController>("GameBootController");
            if (!menu.visible)
                throw new InvalidOperationException("Stage 28 boot menu is not visible.");
        }

        static void ValidateStage16IntegratedSmoke()
        {
            var scene = EditorSceneManager.OpenScene(Stage16SceneCreator.ScenePath);
            if (!scene.IsValid())
                throw new InvalidOperationException("Stage 28 Stage16 scene did not open for smoke validation.");

            var bootstrapper = Require<RtsGameBootstrapper>("RtsGameBootstrapper");
            var driver = Require<RtsSimulationDriver>("RtsSimulationDriver");
            var boardRenderer = Require<BoardRenderer>("BoardRenderer");
            var actorRenderer = Require<ActorRenderSystem>("ActorRenderSystem");
            var desktopHud = Require<DesktopRtsHudRoot>("DesktopRtsHudRoot");
            var layout = Require<CncStyleSidebarLayout>("CncStyleSidebarLayout");
            var uiMode = Require<PlayerFacingUiModeController>("PlayerFacingUiModeController");
            var pause = Require<PauseMenuController>("PauseMenuController");
            var pauseHud = Require<PauseMenuHud>("PauseMenuHud");
            var debugVisibility = Require<DebugHudVisibilityController>("DebugHudVisibilityController");
            var placementPanel = Require<PlacementModePanel>("PlacementModePanel");
            var boardPlacement = Require<BoardPlacementController>("BoardPlacementController");
            var boardHud = Require<BoardPlacementHud>("BoardPlacementHud");
            var router = Require<DesktopUiCommandRouter>("DesktopUiCommandRouter");
            var statusLog = Require<RtsStatusLog>("RtsStatusLog");

            bootstrapper.InitializeScene();
            desktopHud.Initialize();
            layout.ApplyLayout();
            uiMode.ApplyPcDesktopMode();
            router.Initialize(driver, statusLog);
            pause.hud = pauseHud;
            pause.suppressApplicationQuitForValidation = true;
            pause.suppressSceneLoadsForValidation = true;
            pause.Initialize(driver, UnityEngine.Object.FindFirstObjectByType<ProjectAegisRTS.UnityClient.Scenario.VerticalSliceScenarioController>());
            debugVisibility.ApplyPlayerFacingDefaults();
            StepRuntime(driver, boardRenderer, actorRenderer, 6, 0.05f);

            var featureHud = Require<FeatureRegressionHud>("FeatureRegressionHud");
            if (!featureHud.IsHiddenByDefault())
                throw new InvalidOperationException("Stage 28 QA feature overlay must be hidden by default.");
            featureHud.Initialize(driver, router, uiMode, bootstrapper.verticalSliceProgressTracker, bootstrapper.verticalSliceMissionFlowController);
            if (featureHud.BuildAuditSnapshot().Length < 25)
                throw new InvalidOperationException("Stage 28 QA feature overlay did not expose enough command rows.");
            featureHud.ToggleVisibleForValidation();
            if (!featureHud.visible)
                throw new InvalidOperationException("Stage 28 QA feature overlay did not toggle on.");
            featureHud.ToggleVisibleForValidation();
            if (featureHud.visible)
                throw new InvalidOperationException("Stage 28 QA feature overlay did not toggle off.");

            if (!uiMode.IsPcSidebarVisibleForDesktop() || !layout.IsMinimapAboveProductionGrid())
                throw new InvalidOperationException("Stage 28 smoke expected the PC right sidebar and top minimap.");
            if (!uiMode.AreXrBuildMenusHiddenForPc())
                throw new InvalidOperationException("Stage 28 smoke expected XR build menus hidden in PCDesktop mode.");
            if (!debugVisibility.AreDebugPanelsHiddenByDefault())
                throw new InvalidOperationException("Stage 28 smoke expected debug panels hidden by default.");
            if (boardPlacement.IsPlacementModeActive || boardHud.gameObject.activeInHierarchy || placementPanel.gameObject.activeInHierarchy)
                throw new InvalidOperationException("Stage 28 smoke found placement UI visible before building placement.");

            ValidateSelectionAndCommands(driver, router, boardRenderer, actorRenderer);
            ValidateBaseManagement(driver, router, boardRenderer, actorRenderer);
            ValidateSupportEngineerTransport(driver, router, boardRenderer, actorRenderer);
            ValidatePowerPlantPlacement(driver, router, boardRenderer, actorRenderer, debugVisibility, placementPanel, boardHud, boardPlacement);
            ValidatePauseAfterPlacement(driver, pause, pauseHud);
        }

        static void ValidateSelectionAndCommands(RtsSimulationDriver driver, DesktopUiCommandRouter router, BoardRenderer boardRenderer, ActorRenderSystem actorRenderer)
        {
            RequireSuccess(driver.TrySelectFirstOwnedCombatActor(), "select owned combat actor");
            RequireSuccess(router.IssueMoveToCell(new Int2(17, 14)), "move selected combat actor");
            StepRuntime(driver, boardRenderer, actorRenderer, 2, 0.08f);
            RequireSuccess(router.StopSelected(), "stop selected actor");
            RequireSuccess(driver.TrySelectFirstOwnedCombatActor(), "reselect owned combat actor");
            RequireSuccess(router.IssueAttackMoveToCell(new Int2(24, 14)), "attack-move selected actor");

            int enemyActorId;
            if (driver.TryFindFirstEnemyCombatActor(out enemyActorId))
            {
                RequireSuccess(driver.TrySelectFirstOwnedCombatActor(), "select owned combat actor for attack");
                var attack = driver.TryIssueAttackSelectedToActor(enemyActorId);
                if (!attack.Success && attack.Code != "TargetMissing")
                    throw new InvalidOperationException("Stage 28 attack route failed unexpectedly: " + attack);
            }

            router.SetPatrolMode();
            if (router.CurrentMode != DesktopCommandMode.Patrol)
                throw new InvalidOperationException("Stage 28 patrol mode did not route.");
            router.CancelActiveMode();
        }

        static void ValidateBaseManagement(RtsSimulationDriver driver, DesktopUiCommandRouter router, BoardRenderer boardRenderer, ActorRenderSystem actorRenderer)
        {
            var tower = FindActor(RequireSnapshot(driver), "gun_tower", 1);
            if (tower != null)
            {
                RequireSuccess(driver.TryApplyScenarioDamage(tower.ActorId, 40, "stage28_repair_smoke"), "damage gun tower");
                RequireSuccess(driver.SetSelectedActorIds(new[] { tower.ActorId }), "select damaged gun tower");
                RequireSuccess(router.RepairSelected(), "repair selected building");
                StepRuntime(driver, boardRenderer, actorRenderer, 2, 0.1f);
            }

            var barracks = FindActor(RequireSnapshot(driver), "barracks", 1);
            if (barracks != null)
            {
                RequireSuccess(driver.SetSelectedActorIds(new[] { barracks.ActorId }), "select barracks for power toggle");
                RequireSuccess(router.TogglePowerSelected(), "toggle selected building power");
                RequireSuccess(router.TogglePowerSelected(), "restore selected building power");
            }

            var factory = FindActor(RequireSnapshot(driver), "war_factory", 1);
            if (factory != null)
            {
                RequireSuccess(driver.SetSelectedActorIds(new[] { factory.ActorId }), "select war factory for rally");
                router.SetRallyMode();
                RequireSuccess(router.IssueRallyToCell(new Int2(16, 12)), "set rally point");
            }
        }

        static void ValidateSupportEngineerTransport(RtsSimulationDriver driver, DesktopUiCommandRouter router, BoardRenderer boardRenderer, ActorRenderSystem actorRenderer)
        {
            var reveal = driver.TryActivateSupportPowerAtCell("reveal_scan", new Int2(25, 25));
            if (!reveal.Success && reveal.Code != "SupportPowerCooldown")
                throw new InvalidOperationException("Stage 28 reveal scan support-power route failed: " + reveal);

            var engineer = FindActor(RequireSnapshot(driver), "engineer", 1);
            var enemyBuilding = FindFirstEnemyBuilding(RequireSnapshot(driver), 1);
            if (engineer != null && enemyBuilding != null)
            {
                RequireSuccess(driver.SetSelectedActorIds(new[] { engineer.ActorId }), "select engineer");
                router.SetCaptureMode();
                var capture = router.IssueCaptureAtCell(enemyBuilding.CellPosition);
                if (!capture.Success && !capture.Code.Contains("NoPath"))
                    throw new InvalidOperationException("Stage 28 engineer capture route failed unexpectedly: " + capture);
            }

            var transport = FindActor(RequireSnapshot(driver), "apc", 1);
            var infantry = FindActor(RequireSnapshot(driver), "rifle_infantry", 1);
            if (transport != null && infantry != null)
            {
                RequireSuccess(driver.SetSelectedActorIds(new[] { infantry.ActorId }), "select infantry for transport load");
                router.SetLoadTransportMode();
                var load = router.IssueLoadTransportAtCell(transport.CellPosition);
                if (!load.Success && load.Code != "NoPassengerSelection" && !load.Code.Contains("NoPath"))
                    throw new InvalidOperationException("Stage 28 transport load route failed unexpectedly: " + load);
                StepRuntime(driver, boardRenderer, actorRenderer, 1, 0.05f);
            }
        }

        static void ValidatePowerPlantPlacement(
            RtsSimulationDriver driver,
            DesktopUiCommandRouter router,
            BoardRenderer boardRenderer,
            ActorRenderSystem actorRenderer,
            DebugHudVisibilityController debugVisibility,
            PlacementModePanel placementPanel,
            BoardPlacementHud boardHud,
            BoardPlacementController boardPlacement)
        {
            var powerPlantsBefore = CountOwnedActors(driver, "power_plant", 1);
            RequireSuccess(router.QueueProduction("power_plant"), "queue Power Plant");
            StepUntilPendingPlacement(driver, boardRenderer, actorRenderer, "power_plant");
            RequireSuccess(router.QueueProduction("power_plant"), "activate ready Power Plant card");
            debugVisibility.ApplyPlayerFacingDefaults();
            StepRuntime(driver, boardRenderer, actorRenderer, 2, 0.05f);

            if (!driver.HasPlacementMode || driver.PendingPlacementTypeId != "power_plant")
                throw new InvalidOperationException("Stage 28 smoke did not enter Power Plant placement mode.");
            if (boardPlacement.IsPlacementModeActive || boardHud.gameObject.activeInHierarchy)
                throw new InvalidOperationException("Stage 28 smoke showed board setup placement during PC building placement.");
            if (!placementPanel.gameObject.activeInHierarchy)
                throw new InvalidOperationException("Stage 28 smoke expected the right-sidebar PlacementModePanel during building placement.");

            var validCell = FindValidPlacementCell(driver, boardRenderer, actorRenderer);
            driver.SetHoveredCell(validCell, true);
            StepRuntime(driver, boardRenderer, actorRenderer, 1, 0.02f);
            PlacementPreviewSnapshot preview;
            if (!driver.TryGetPlacementPreview(out preview) || !preview.CanPlace || preview.PlacementGridScale < 2)
                throw new InvalidOperationException("Stage 28 smoke expected a valid fine-grid placement preview.");

            RequireSuccess(router.PlaceAtHoveredCell(), "place Power Plant through right-sidebar route");
            debugVisibility.ApplyPlayerFacingDefaults();
            StepRuntime(driver, boardRenderer, actorRenderer, 4, 0.05f);
            if (driver.HasPlacementMode || placementPanel.gameObject.activeInHierarchy || boardHud.gameObject.activeInHierarchy)
                throw new InvalidOperationException("Stage 28 placement UI did not clear after placing the Power Plant.");
            if (CountOwnedActors(driver, "power_plant", 1) <= powerPlantsBefore)
                throw new InvalidOperationException("Stage 28 smoke did not place a new Power Plant.");
        }

        static void ValidatePauseAfterPlacement(RtsSimulationDriver driver, PauseMenuController pause, PauseMenuHud pauseHud)
        {
            pause.HandleEscapePressed();
            if (!pause.IsOpen || !pauseHud.IsVisible || !driver.IsPaused)
                throw new InvalidOperationException("Stage 28 pause menu did not open after placement ended.");
            pause.Resume();
            if (pause.IsOpen || pauseHud.IsVisible || driver.IsPaused)
                throw new InvalidOperationException("Stage 28 pause menu did not resume after placement ended.");
        }

        static void StepUntilPendingPlacement(RtsSimulationDriver driver, BoardRenderer boardRenderer, ActorRenderSystem actorRenderer, string typeId)
        {
            for (var i = 0; i < 600; i++)
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

            throw new InvalidOperationException("No valid Stage 28 placement cell was found.");
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

        static int CountOwnedActors(RtsSimulationDriver driver, string typeId, int ownerId)
        {
            var snapshot = RequireSnapshot(driver);
            var count = 0;
            for (var i = 0; i < snapshot.Actors.Count; i++)
            {
                var actor = snapshot.Actors[i];
                if (actor.OwnerId == ownerId && actor.TypeId == typeId && !actor.IsDestroyed)
                    count++;
            }

            return count;
        }

        static ActorSnapshot FindActor(WorldSnapshot snapshot, string typeId, int ownerId)
        {
            for (var i = 0; i < snapshot.Actors.Count; i++)
            {
                var actor = snapshot.Actors[i];
                if (actor.TypeId == typeId && actor.OwnerId == ownerId && !actor.IsDestroyed)
                    return actor;
            }

            return null;
        }

        static ActorSnapshot FindFirstEnemyBuilding(WorldSnapshot snapshot, int localPlayerId)
        {
            for (var i = 0; i < snapshot.Actors.Count; i++)
            {
                var actor = snapshot.Actors[i];
                if (actor.OwnerId != localPlayerId && actor.OwnerId != 0 && actor.PlacementGridScale > 0 && !actor.IsDestroyed)
                    return actor;
            }

            return null;
        }

        static WorldSnapshot RequireSnapshot(RtsSimulationDriver driver)
        {
            if (driver == null || driver.LatestSnapshot == null)
                throw new InvalidOperationException("Stage 28 expected a current runtime snapshot.");
            return driver.LatestSnapshot;
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

        static void RequireSuccess(RtsCommandResult result, string label)
        {
            if (result == null || !result.Success)
                throw new InvalidOperationException("Stage 28 expected success for " + label + ": " + (result == null ? "null" : result.ToString()));
        }

        static void CaptureRedError(string condition, string stackTrace, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
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
