using System;
using System.Collections.Generic;
using ProjectAegisRTS.Core;
using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.UnityClient.Bootstrap;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Feedback;
using ProjectAegisRTS.UnityClient.Performance;
using ProjectAegisRTS.UnityClient.Rendering;
using ProjectAegisRTS.UnityClient.Rendering.Combat;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage15PlayModeSmokeValidator
    {
        static readonly List<string> RedErrors = new List<string>();

        public static void RunStage15PlayModeSmokeBatch()
        {
            try
            {
                RunStage15PlayModeSmoke();
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

        public static void RunStage15PlayModeSmoke()
        {
            RedErrors.Clear();
            Application.logMessageReceived += CaptureRedError;
            try
            {
                Stage15SceneValidator.ValidateStage15Scene();
                var scene = EditorSceneManager.OpenScene(Stage15SceneCreator.ScenePath);
                if (!scene.IsValid())
                    throw new InvalidOperationException("Stage 15 scene did not open.");

                var bootstrapper = RequireEnabled<RtsGameBootstrapper>("RtsGameBootstrapper");
                var driver = RequireEnabled<RtsSimulationDriver>("RtsSimulationDriver");
                var boardRenderer = RequireEnabled<BoardRenderer>("BoardRenderer");
                var actorRenderer = RequireEnabled<ActorRenderSystem>("ActorRenderSystem");
                var projectileRenderer = RequireEnabled<ProjectileRenderSystem>("ProjectileRenderSystem");
                var bus = RequireEnabled<FeedbackEventBus>("FeedbackEventBus");
                var vfx = RequireEnabled<VfxFeedbackController>("VfxFeedbackController");
                var pool = RequireEnabled<ObjectPoolService>("ObjectPoolService");
                var budgets = RequireEnabled<PerformanceBudgetLibrary>("PerformanceBudgetLibrary");
                var stats = RequireEnabled<RuntimePerformanceStats>("RuntimePerformanceStats");
                var complexity = RequireEnabled<SceneComplexityReporter>("SceneComplexityReporter");
                var quality = RequireEnabled<QualityProfileApplier>("QualityProfileApplier");
                var quest = RequireEnabled<QuestBuildReadinessReporter>("QuestBuildReadinessReporter");
                var pc = RequireEnabled<PcBuildReadinessReporter>("PcBuildReadinessReporter");
                RequireEnabled<RenderStatsHud>("RenderStatsHud");

                bootstrapper.InitializeScene();
                budgets.EnsureInitialized();
                quality.ApplySelectedProfile();
                if (quality.AppliedTargetFrameRate <= 0)
                    throw new InvalidOperationException("Stage 15 quality profile did not apply a target frame rate.");

                driver.TryCreateCombatDemoWorld();
                StepRuntime(driver, boardRenderer, actorRenderer, projectileRenderer, bus, stats, 3, 0.016f);
                var snapshot = RequireSnapshot(driver);
                var towerId = FindActor(snapshot, "gun_tower", 1);
                var enemyId = FindActor(snapshot, "rifle_infantry", 2);
                driver.SetSelectedActorIds(new[] { towerId });
                RequireSuccess(driver.TryIssueAttackSelectedToActor(enemyId), "attack command");

                var createdBeforeAttack = pool.CreatedCount;
                for (var i = 0; i < 160 && pool.ReleasedCount == 0; i++)
                    StepRuntime(driver, boardRenderer, actorRenderer, projectileRenderer, bus, stats, 1, 0.05f);

                if (pool.CreatedCount <= createdBeforeAttack)
                    throw new InvalidOperationException("Stage 15 pool did not create any visual objects during combat/feedback smoke.");

                var releasedBeforeMarker = pool.ReleasedCount;
                bus.EmitManual(FeedbackEventType.MoveCommand, "Stage 15 pooled feedback marker smoke.");
                StepRuntime(driver, boardRenderer, actorRenderer, projectileRenderer, bus, stats, 1, 0.05f);
                AdvanceAllFeedbackMarkers(2f);
                stats.RefreshSnapshotCounts();
                if (pool.ReleasedCount <= releasedBeforeMarker)
                    throw new InvalidOperationException("Stage 15 feedback marker was not released to the object pool.");

                var reusedBeforeMarker = pool.ReusedCount;
                bus.EmitManual(FeedbackEventType.MoveCommand, "Stage 15 pooled feedback marker reuse smoke.");
                StepRuntime(driver, boardRenderer, actorRenderer, projectileRenderer, bus, stats, 1, 0.05f);
                if (pool.ReusedCount <= reusedBeforeMarker)
                    throw new InvalidOperationException("Stage 15 object pool did not reuse a released feedback marker.");
                AdvanceAllFeedbackMarkers(2f);

                complexity.Refresh();
                quest.RefreshReport();
                pc.RefreshReport();
                stats.RecordFrame(0.016f);
                stats.RefreshSnapshotCounts();

                var questProfile = budgets.GetProfile("quest");
                if (!stats.IsWithinBudget(questProfile))
                    throw new InvalidOperationException("Stage 15 runtime stats exceeded the placeholder Quest budget.");
                if (!complexity.IsWithinBudget(questProfile))
                    throw new InvalidOperationException("Stage 15 scene complexity exceeded the placeholder Quest budget.");
                if (!quest.reportGenerated || !pc.reportGenerated)
                    throw new InvalidOperationException("Stage 15 build readiness reports were not refreshed.");
                if (stats.FrameCount <= 0 || stats.SmoothedFps <= 0f)
                    throw new InvalidOperationException("Stage 15 runtime performance stats did not record frames.");
                if (vfx.SpawnedMarkerCount <= 0)
                    throw new InvalidOperationException("Stage 15 VFX controller did not spawn pooled feedback markers.");

                if (RedErrors.Count > 0)
                    throw new InvalidOperationException("Red console errors were produced during Stage 15 smoke validation: " + string.Join(" | ", RedErrors.ToArray()));

                Debug.Log("Stage 15 play mode smoke validation passed.");
            }
            finally
            {
                Application.logMessageReceived -= CaptureRedError;
                RedErrors.Clear();
            }
        }

        static void StepRuntime(RtsSimulationDriver driver, BoardRenderer boardRenderer, ActorRenderSystem actorRenderer, ProjectileRenderSystem projectileRenderer, FeedbackEventBus bus, RuntimePerformanceStats stats, int frames, float deltaTime)
        {
            for (var i = 0; i < frames; i++)
            {
                driver.ManualUpdate(deltaTime);
                boardRenderer.UpdateHover(driver.HasHoveredCell ? (Int2?)driver.HoveredCell : null);
                PlacementPreviewSnapshot preview;
                boardRenderer.UpdatePlacementPreview(driver.TryGetPlacementPreview(out preview) ? preview : null);
                actorRenderer.RenderSnapshot(driver.LatestSnapshot, driver.SelectedActorIds, deltaTime);
                projectileRenderer.RenderSnapshot(driver.LatestSnapshot);
                bus.RenderSnapshot(driver.LatestSnapshot);
                stats.RecordFrame(deltaTime);
            }
        }

        static void AdvanceAllFeedbackMarkers(float deltaTime)
        {
            var markers = UnityEngine.Object.FindObjectsByType<FeedbackVisualMarker>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            for (var i = 0; i < markers.Length; i++)
                if (markers[i] != null)
                    markers[i].AdvanceLifetime(deltaTime);
        }

        static WorldSnapshot RequireSnapshot(RtsSimulationDriver driver)
        {
            if (driver.LatestSnapshot == null)
                throw new InvalidOperationException("Stage 15 expected a simulation snapshot.");
            return driver.LatestSnapshot;
        }

        static int FindActor(WorldSnapshot snapshot, string typeId, int ownerId)
        {
            for (var i = 0; i < snapshot.Actors.Count; i++)
                if (snapshot.Actors[i].TypeId == typeId && snapshot.Actors[i].OwnerId == ownerId && !snapshot.Actors[i].IsDestroyed)
                    return snapshot.Actors[i].ActorId;
            throw new InvalidOperationException("Missing actor " + typeId + " for owner " + ownerId + ".");
        }

        static void RequireSuccess(RtsCommandResult result, string label)
        {
            if (result == null || !result.Success)
                throw new InvalidOperationException("Stage 15 " + label + " failed: " + (result == null ? "null result" : result.ToString()));
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
