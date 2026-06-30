using System;
using System.Collections.Generic;
using ProjectAegisRTS.Core;
using ProjectAegisRTS.Match;
using ProjectAegisRTS.Snapshots;
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
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage16PlayModeSmokeValidator
    {
        static readonly List<string> RedErrors = new List<string>();

        public static void RunStage16PlayModeSmokeBatch()
        {
            try
            {
                RunStage16PlayModeSmoke();
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

        public static void RunStage16PlayModeSmoke()
        {
            RedErrors.Clear();
            Application.logMessageReceived += CaptureRedError;
            try
            {
                Stage16SceneValidator.ValidateStage16Scene();
                var scene = EditorSceneManager.OpenScene(Stage16SceneCreator.ScenePath);
                if (!scene.IsValid())
                    throw new InvalidOperationException("Stage 16 scene did not open.");

                var bootstrapper = RequireEnabled<RtsGameBootstrapper>("RtsGameBootstrapper");
                var driver = RequireEnabled<RtsSimulationDriver>("RtsSimulationDriver");
                var controller = RequireEnabled<VerticalSliceScenarioController>("VerticalSliceScenarioController");
                var objectiveHud = RequireEnabled<MatchObjectiveHud>("MatchObjectiveHud");
                var systemsHud = RequireEnabled<IntegratedSystemsStatusHud>("IntegratedSystemsStatusHud");
                var debugActions = RequireEnabled<VerticalSliceDebugActions>("VerticalSliceDebugActions");
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

                bootstrapper.InitializeScene();
                controller.Initialize(driver, objectiveHud, systemsHud, debugActions);
                StepRuntime(driver, boardRenderer, actorRenderer, projectileRenderer, combatRenderer, resourceRenderer, fogRenderer, minimapRenderer, aiRenderer, terrainRenderer, bus, stats, 4, 0.05f);

                var snapshot = RequireSnapshot(driver);
                if (snapshot.Match.Phase != MatchPhase.Running)
                    throw new InvalidOperationException("Stage 16 match did not start running.");
                RequireActor(snapshot, "fabrication_hub", 1);
                RequireWorldActor(driver, "fabrication_hub", 2);
                RequireActor(snapshot, "harvester", 1);
                if (snapshot.Economy.Resources.Count == 0 || snapshot.Economy.Harvesters.Count == 0 || snapshot.Economy.Refineries.Count == 0)
                    throw new InvalidOperationException("Stage 16 economy snapshot is incomplete.");
                if (snapshot.Fog.Cells.Count == 0 || snapshot.Minimap.ActorDots.Count == 0)
                    throw new InvalidOperationException("Stage 16 fog/minimap snapshot is incomplete.");
                if (snapshot.Ai.Players.Count == 0)
                    throw new InvalidOperationException("Stage 16 AI snapshot is missing.");
                if (snapshot.Map.TerrainCells.Count == 0)
                    throw new InvalidOperationException("Stage 16 terrain snapshot is missing.");
                if (snapshot.Scenario.Objectives.Count < 2)
                    throw new InvalidOperationException("Stage 16 objective snapshot is incomplete.");

                RequireSuccess(debugActions.IssueHarvest(), "harvest command");
                StepRuntime(driver, boardRenderer, actorRenderer, projectileRenderer, combatRenderer, resourceRenderer, fogRenderer, minimapRenderer, aiRenderer, terrainRenderer, bus, stats, 3, 0.05f);
                RequireSuccess(debugActions.QueueProduction("power_plant"), "production command");
                StepRuntime(driver, boardRenderer, actorRenderer, projectileRenderer, combatRenderer, resourceRenderer, fogRenderer, minimapRenderer, aiRenderer, terrainRenderer, bus, stats, 3, 0.05f);
                RequireSuccess(debugActions.IssueAttack(), "attack command");
                StepRuntime(driver, boardRenderer, actorRenderer, projectileRenderer, combatRenderer, resourceRenderer, fogRenderer, minimapRenderer, aiRenderer, terrainRenderer, bus, stats, 12, 0.05f);

                RequireSuccess(debugActions.DestroyEnemyBase(), "enemy base destruction");
                snapshot = RequireSnapshot(driver);
                if (snapshot.Match.Phase != MatchPhase.Won)
                    throw new InvalidOperationException("Stage 16 enemy base destruction did not trigger victory.");

                RequireSuccess(controller.ResetScenario(), "scenario reset");
                StepRuntime(driver, boardRenderer, actorRenderer, projectileRenderer, combatRenderer, resourceRenderer, fogRenderer, minimapRenderer, aiRenderer, terrainRenderer, bus, stats, 2, 0.05f);
                RequireSuccess(debugActions.DestroyPlayerBase(), "player base destruction");
                snapshot = RequireSnapshot(driver);
                if (snapshot.Match.Phase != MatchPhase.Lost)
                    throw new InvalidOperationException("Stage 16 player base destruction did not trigger defeat.");

                if (RedErrors.Count > 0)
                    throw new InvalidOperationException("Red console errors were produced during Stage 16 smoke validation: " + string.Join(" | ", RedErrors.ToArray()));

                Debug.Log("Stage 16 play mode smoke validation passed.");
            }
            finally
            {
                Application.logMessageReceived -= CaptureRedError;
                RedErrors.Clear();
            }
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
                throw new InvalidOperationException("Stage 16 expected a simulation snapshot.");
            return driver.LatestSnapshot;
        }

        static void RequireActor(WorldSnapshot snapshot, string typeId, int ownerId)
        {
            for (var i = 0; i < snapshot.Actors.Count; i++)
                if (snapshot.Actors[i].TypeId == typeId && snapshot.Actors[i].OwnerId == ownerId && !snapshot.Actors[i].IsDestroyed)
                    return;

            throw new InvalidOperationException("Missing actor " + typeId + " for owner " + ownerId + ".");
        }

        static void RequireWorldActor(RtsSimulationDriver driver, string typeId, int ownerId)
        {
            int actorId;
            if (!driver.TryFindAliveActorOfType(typeId, ownerId, out actorId))
                throw new InvalidOperationException("Missing world actor " + typeId + " for owner " + ownerId + ".");
        }

        static void RequireSuccess(RtsCommandResult result, string label)
        {
            if (result == null || !result.Success)
                throw new InvalidOperationException("Stage 16 " + label + " failed: " + (result == null ? "null result" : result.ToString()));
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
