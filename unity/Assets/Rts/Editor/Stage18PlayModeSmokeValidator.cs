using System;
using System.Collections.Generic;
using ProjectAegisRTS.Core;
using ProjectAegisRTS.Match;
using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.UnityClient.Boot;
using ProjectAegisRTS.UnityClient.Bootstrap;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Feedback;
using ProjectAegisRTS.UnityClient.Performance;
using ProjectAegisRTS.UnityClient.Rendering;
using ProjectAegisRTS.UnityClient.Rendering.Ai;
using ProjectAegisRTS.UnityClient.Rendering.Combat;
using ProjectAegisRTS.UnityClient.Rendering.Economy;
using ProjectAegisRTS.UnityClient.Rendering.Map;
using ProjectAegisRTS.UnityClient.Rendering.Visibility;
using ProjectAegisRTS.UnityClient.Scenario;
using ProjectAegisRTS.UnityClient.UI.Common;
using ProjectAegisRTS.UnityClient.UI.Desktop;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage18PlayModeSmokeValidator
    {
        static readonly List<string> RedErrors = new List<string>();

        public static void RunStage18PlayModeSmokeBatch()
        {
            try
            {
                RunStage18PlayModeSmoke();
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

        public static void RunStage18PlayModeSmoke()
        {
            RedErrors.Clear();
            Application.logMessageReceived += CaptureRedError;
            try
            {
                Stage18SceneValidator.ValidateStage18Scene();
                ValidateBootScreens();
                ValidateStage16Runtime();

                if (RedErrors.Count > 0)
                    throw new InvalidOperationException("Red console errors were produced during Stage 18 smoke validation: " + string.Join(" | ", RedErrors.ToArray()));

                Debug.Log("Stage 18 play mode smoke validation passed.");
            }
            finally
            {
                Application.logMessageReceived -= CaptureRedError;
                RedErrors.Clear();
            }
        }

        static void ValidateBootScreens()
        {
            var bootScene = EditorSceneManager.OpenScene(Stage16_5BuildFlowConfigurator.BootScenePath);
            if (!bootScene.IsValid())
                throw new InvalidOperationException("Stage 18 boot scene did not open for smoke validation.");

            var controller = Require<GameBootController>("GameBootController");
            var menu = Require<MainMenuHud>("MainMenuHud");
            var controls = Require<ControlsHelpHud>("ControlsHelpHud");
            var options = Require<OptionsMenuHud>("OptionsMenuHud");

            if (!menu.visible || controls.visible || options.visible)
                throw new InvalidOperationException("Stage 18 boot menu default visibility is incorrect.");

            controller.ShowControls();
            if (menu.visible || !controls.visible || options.visible)
                throw new InvalidOperationException("Stage 18 controls screen did not open.");

            controller.ShowOptions();
            if (menu.visible || controls.visible || !options.visible)
                throw new InvalidOperationException("Stage 18 options screen did not open.");

            controller.ShowMainMenu();
            if (!menu.visible || controls.visible || options.visible)
                throw new InvalidOperationException("Stage 18 main menu did not restore.");
        }

        static void ValidateStage16Runtime()
        {
            var scene = EditorSceneManager.OpenScene(Stage16SceneCreator.ScenePath);
            if (!scene.IsValid())
                throw new InvalidOperationException("Stage 18 Stage16 scene did not open for smoke validation.");

            var bootstrapper = RequireEnabled<RtsGameBootstrapper>("RtsGameBootstrapper");
            var driver = RequireEnabled<RtsSimulationDriver>("RtsSimulationDriver");
            var controller = RequireEnabled<VerticalSliceScenarioController>("VerticalSliceScenarioController");
            var debugActions = RequireEnabled<VerticalSliceDebugActions>("VerticalSliceDebugActions");
            var playerInitializer = RequireEnabled<PlayerBuildSceneInitializer>("PlayerBuildSceneInitializer");
            var debugVisibility = RequireEnabled<DebugHudVisibilityController>("DebugHudVisibilityController");
            var desktopHud = RequireEnabled<DesktopRtsHudRoot>("DesktopRtsHudRoot");
            var boardRenderer = RequireEnabled<BoardRenderer>("BoardRenderer");
            var actorRenderer = RequireEnabled<ActorRenderSystem>("ActorRenderSystem");
            var projectileRenderer = RequireEnabled<ProjectileRenderSystem>("ProjectileRenderSystem");
            var combatRenderer = RequireEnabled<CombatEventRenderSystem>("CombatEventRenderSystem");
            var resourceRenderer = RequireEnabled<ResourceFieldRenderSystem>("ResourceFieldRenderSystem");
            var fogRenderer = RequireEnabled<FogOverlayRenderer>("FogOverlayRenderer");
            var minimapRenderer = RequireEnabled<MinimapRenderSystem>("MinimapRenderSystem");
            var aiRenderer = RequireEnabled<AiIntentRenderSystem>("AiIntentRenderSystem");
            var terrainRenderer = RequireEnabled<TerrainDebugRenderer>("TerrainDebugRenderer");
            var bus = RequireEnabled<FeedbackEventBus>("FeedbackEventBus");
            var stats = RequireEnabled<RuntimePerformanceStats>("RuntimePerformanceStats");
            var matchObjective = RequireEnabled<MatchObjectiveHud>("MatchObjectiveHud");
            var playerObjective = RequireEnabled<PlayerObjectiveHud>("PlayerObjectiveHud");
            var checklist = RequireEnabled<VerticalSliceChecklistHud>("VerticalSliceChecklistHud");
            var promptSystem = RequireEnabled<PlayerPromptSystem>("PlayerPromptSystem");
            var promptHud = RequireEnabled<PlayerPromptHud>("PlayerPromptHud");
            var resultHud = RequireEnabled<MatchResultHud>("MatchResultHud");
            var progress = RequireEnabled<VerticalSliceProgressTracker>("VerticalSliceProgressTracker");
            var statusLog = Require<RtsStatusLog>("RtsStatusLog");

            bootstrapper.InitializeScene();
            desktopHud.Initialize();
            playerInitializer.ApplyPlayerFacingDefaults();
            debugVisibility.ApplyPlayerFacingDefaults();
            controller.Initialize(driver, matchObjective, RequireEnabled<IntegratedSystemsStatusHud>("IntegratedSystemsStatusHud"), debugActions);
            StepRuntime(driver, boardRenderer, actorRenderer, projectileRenderer, combatRenderer, resourceRenderer, fogRenderer, minimapRenderer, aiRenderer, terrainRenderer, bus, stats, 4, 0.05f);

            var boardRoot = GameObject.Find("BoardRoot");
            if (boardRoot == null || !boardRoot.activeInHierarchy || boardRoot.transform.childCount == 0)
                throw new InvalidOperationException("Stage 18 board visuals are missing.");
            if (actorRenderer.transform.childCount == 0)
                throw new InvalidOperationException("Stage 18 actor visuals are missing.");
            if (resourceRenderer.transform.childCount == 0)
                throw new InvalidOperationException("Stage 18 resource visuals are missing.");
            if (!playerObjective.visible || !checklist.visible || !promptSystem.visible || !promptHud.visible)
                throw new InvalidOperationException("Stage 18 player guidance HUDs are not visible.");
            if (!debugVisibility.AreDebugPanelsHiddenByDefault())
                throw new InvalidOperationException("Stage 18 debug panels are visible by default.");
            if (!debugVisibility.IsPlacementUiHiddenByDefault())
                throw new InvalidOperationException("Stage 18 placement UI is visible by default.");
            if (desktopHud.showDebugOverlay || statusLog.visible || statusLog.gameObject.activeInHierarchy)
                throw new InvalidOperationException("Stage 18 desktop status log/debug overlay is visible in player-facing mode.");
            if (playerObjective.area != PlayerHudLayout.ObjectiveArea ||
                checklist.area != PlayerHudLayout.ChecklistArea ||
                promptHud.area != PlayerHudLayout.PromptArea)
                throw new InvalidOperationException("Stage 18 HUD layout defaults changed at runtime.");

            var snapshot = RequireSnapshot(driver);
            if (snapshot.Match.Phase != MatchPhase.Running)
                throw new InvalidOperationException("Stage 18 match did not start running.");
            progress.Refresh();
            if (progress.enemyBaseDestroyed || progress.hasWon)
                throw new InvalidOperationException("Stage 18 startup incorrectly reports the enemy base as destroyed.");
            if (string.IsNullOrEmpty(promptSystem.GetPrompt()))
                throw new InvalidOperationException("Stage 18 prompt system produced an empty prompt.");

            RequireSuccess(driver.TrySelectFirstOwnedActorOfType("fabrication_hub"), "select fabrication hub");
            progress.Refresh();
            if (!progress.hasSelectedFabricationHub)
                throw new InvalidOperationException("Stage 18 progress tracker did not detect selected fabrication hub.");

            RequireSuccess(debugActions.QueueProduction("power_plant"), "queue power plant");
            StepRuntime(driver, boardRenderer, actorRenderer, projectileRenderer, combatRenderer, resourceRenderer, fogRenderer, minimapRenderer, aiRenderer, terrainRenderer, bus, stats, 3, 0.05f);
            progress.Refresh();
            if (string.IsNullOrEmpty(progress.currentChecklistPrompt))
                throw new InvalidOperationException("Stage 18 checklist prompt is empty after production progress.");

            RequireSuccess(debugActions.RevealMap(), "reveal map for smoke");
            StepRuntime(driver, boardRenderer, actorRenderer, projectileRenderer, combatRenderer, resourceRenderer, fogRenderer, minimapRenderer, aiRenderer, terrainRenderer, bus, stats, 2, 0.05f);
            progress.Refresh();
            if (!progress.hasDiscoveredEnemy)
                throw new InvalidOperationException("Stage 18 progress tracker did not detect scouted enemy actors.");

            RequireSuccess(debugActions.DestroyEnemyBase(), "enemy base destruction");
            snapshot = RequireSnapshot(driver);
            progress.Refresh();
            if (snapshot.Match.Phase != MatchPhase.Won || !progress.enemyBaseDestroyed || !progress.hasWon)
                throw new InvalidOperationException("Stage 18 victory/objective state is inconsistent.");
            if (!resultHud.HasResultToShow)
                throw new InvalidOperationException("Stage 18 result HUD did not show victory.");

            RequireSuccess(controller.ResetScenario(), "scenario reset");
            StepRuntime(driver, boardRenderer, actorRenderer, projectileRenderer, combatRenderer, resourceRenderer, fogRenderer, minimapRenderer, aiRenderer, terrainRenderer, bus, stats, 2, 0.05f);
            if (resultHud.HasResultToShow)
                throw new InvalidOperationException("Stage 18 result HUD stayed visible after restart.");

            RequireSuccess(debugActions.DestroyPlayerBase(), "player base destruction");
            snapshot = RequireSnapshot(driver);
            progress.Refresh();
            if (snapshot.Match.Phase != MatchPhase.Lost || !progress.hasLost)
                throw new InvalidOperationException("Stage 18 defeat/objective state is inconsistent.");
            if (!resultHud.HasResultToShow)
                throw new InvalidOperationException("Stage 18 result HUD did not show defeat.");
        }

        static void StepRuntime(
            RtsSimulationDriver driver,
            BoardRenderer boardRenderer,
            ActorRenderSystem actorRenderer,
            ProjectileRenderSystem projectileRenderer,
            CombatEventRenderSystem combatRenderer,
            ResourceFieldRenderSystem resourceRenderer,
            FogOverlayRenderer fogRenderer,
            MinimapRenderSystem minimapRenderer,
            AiIntentRenderSystem aiRenderer,
            TerrainDebugRenderer terrainRenderer,
            FeedbackEventBus bus,
            RuntimePerformanceStats stats,
            int frames,
            float deltaTime)
        {
            for (var i = 0; i < frames; i++)
            {
                driver.ManualUpdate(deltaTime);
                boardRenderer.UpdateHover(driver.HasHoveredCell ? (Int2?)driver.HoveredCell : null);
                PlacementPreviewSnapshot preview;
                boardRenderer.UpdatePlacementPreview(driver.TryGetPlacementPreview(out preview) ? preview : null);
                actorRenderer.RenderSnapshot(driver.LatestSnapshot, driver.SelectedActorIds, deltaTime);
                projectileRenderer.RenderSnapshot(driver.LatestSnapshot);
                combatRenderer.RenderSnapshot(driver.LatestSnapshot);
                resourceRenderer.RenderSnapshot(driver.LatestSnapshot);
                fogRenderer.RenderSnapshot(driver.LatestSnapshot);
                minimapRenderer.RenderSnapshot(driver.LatestSnapshot);
                aiRenderer.RenderSnapshot(driver.LatestSnapshot);
                terrainRenderer.RenderSnapshot(driver.LatestSnapshot);
                bus.RenderSnapshot(driver.LatestSnapshot);
                stats.RecordFrame(deltaTime);
            }
        }

        static WorldSnapshot RequireSnapshot(RtsSimulationDriver driver)
        {
            if (driver.LatestSnapshot == null)
                throw new InvalidOperationException("Stage 18 expected a simulation snapshot.");
            return driver.LatestSnapshot;
        }

        static void RequireSuccess(RtsCommandResult result, string label)
        {
            if (result == null || !result.Success)
                throw new InvalidOperationException("Stage 18 expected success for " + label + ": " + (result == null ? "null" : result.ToString()));
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

        static T RequireEnabled<T>(string label) where T : Behaviour
        {
            var component = Require<T>(label);
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
