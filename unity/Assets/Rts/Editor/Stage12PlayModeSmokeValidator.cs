using System;
using System.Collections.Generic;
using ProjectAegisRTS.Core;
using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.UnityClient.Bootstrap;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Rendering;
using ProjectAegisRTS.UnityClient.Rendering.Ai;
using ProjectAegisRTS.UnityClient.UI.Common;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage12PlayModeSmokeValidator
    {
        static readonly List<string> RedErrors = new List<string>();

        public static void RunStage12PlayModeSmokeBatch()
        {
            try
            {
                RunStage12PlayModeSmoke();
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

        public static void RunStage12PlayModeSmoke()
        {
            RedErrors.Clear();
            Application.logMessageReceived += CaptureRedError;
            try
            {
                Stage12SceneValidator.ValidateStage12Scene();
                var scene = EditorSceneManager.OpenScene(Stage12SceneCreator.ScenePath);
                if (!scene.IsValid())
                    throw new InvalidOperationException("Stage 12 scene did not open.");

                var bootstrapper = RequireEnabled<RtsGameBootstrapper>("RtsGameBootstrapper");
                var driver = RequireEnabled<RtsSimulationDriver>("RtsSimulationDriver");
                var boardRenderer = RequireEnabled<BoardRenderer>("BoardRenderer");
                var actorRenderer = RequireEnabled<ActorRenderSystem>("ActorRenderSystem");
                var aiIntents = RequireEnabled<AiIntentRenderSystem>("AiIntentRenderSystem");
                var timeline = RequireEnabled<AiPlanTimelineView>("AiPlanTimelineView");
                RequireEnabled<AiDebugHud>("AiDebugHud");

                bootstrapper.InitializeScene();
                driver.TryCreateAiSkirmishDemoWorld();
                StepRuntime(driver, boardRenderer, actorRenderer, aiIntents, timeline, 16, 0.1f);

                var snapshot = driver.LatestSnapshot;
                if (snapshot == null)
                    throw new InvalidOperationException("Stage 12 did not produce a snapshot.");
                if (snapshot.Ai.Players.Count != 1)
                    throw new InvalidOperationException("Stage 12 expected one AI snapshot.");
                if (snapshot.Ai.Players[0].RecentIntents.Count < 5)
                    throw new InvalidOperationException("Stage 12 expected AI intents.");
                if (!HasAiIntent(snapshot, "Economy"))
                    throw new InvalidOperationException("Stage 12 economy intent missing.");
                if (!HasAiIntent(snapshot, "Production", "QueueProduction"))
                    throw new InvalidOperationException("Stage 12 production queue intent missing.");
                if (!HasAiIntent(snapshot, "Attack", "BasicAttackWave"))
                    throw new InvalidOperationException("Stage 12 attack-wave intent missing.");
                if (!HasAiIntent(snapshot, "Scouting"))
                    throw new InvalidOperationException("Stage 12 scouting placeholder intent missing.");
                if (!HasAiIntent(snapshot, "Defense"))
                    throw new InvalidOperationException("Stage 12 defense placeholder intent missing.");
                if (snapshot.Ai.Players[0].ConsecutiveInvalidCommands != 0)
                    throw new InvalidOperationException("Stage 12 AI issued invalid commands.");
                if (aiIntents.IntentCount <= 0 || aiIntents.IssuedCommandCount <= 0)
                    throw new InvalidOperationException("Stage 12 intent renderer did not read AI intents.");
                if (timeline.PlayerCount <= 0 || timeline.RecentIntentCount <= 0)
                    throw new InvalidOperationException("Stage 12 AI timeline did not read AI plan data.");

                if (RedErrors.Count > 0)
                    throw new InvalidOperationException("Red console errors were produced during Stage 12 smoke validation: " + string.Join(" | ", RedErrors.ToArray()));

                Debug.Log("Stage 12 play mode smoke validation passed.");
            }
            finally
            {
                Application.logMessageReceived -= CaptureRedError;
                RedErrors.Clear();
            }
        }

        static void StepRuntime(RtsSimulationDriver driver, BoardRenderer boardRenderer, ActorRenderSystem actorRenderer, AiIntentRenderSystem aiIntents, AiPlanTimelineView timeline, int frames, float deltaTime)
        {
            for (var i = 0; i < frames; i++)
            {
                driver.ManualUpdate(deltaTime);
                boardRenderer.UpdateHover(driver.HasHoveredCell ? (Int2?)driver.HoveredCell : null);
                PlacementPreviewSnapshot preview;
                boardRenderer.UpdatePlacementPreview(driver.TryGetPlacementPreview(out preview) ? preview : null);
                actorRenderer.RenderSnapshot(driver.LatestSnapshot, driver.SelectedActorIds, deltaTime);
                aiIntents.RenderSnapshot(driver.LatestSnapshot);
                timeline.RenderSnapshot(driver.LatestSnapshot);
            }
        }

        static bool HasAiIntent(WorldSnapshot snapshot, string kind)
        {
            return HasAiIntent(snapshot, kind, null);
        }

        static bool HasAiIntent(WorldSnapshot snapshot, string kind, string intentId)
        {
            for (var playerIndex = 0; playerIndex < snapshot.Ai.Players.Count; playerIndex++)
            {
                var player = snapshot.Ai.Players[playerIndex];
                for (var intentIndex = 0; intentIndex < player.RecentIntents.Count; intentIndex++)
                {
                    var intent = player.RecentIntents[intentIndex];
                    if (intent.Kind == kind && (intentId == null || intent.IntentId == intentId))
                        return true;
                }
            }

            return false;
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
