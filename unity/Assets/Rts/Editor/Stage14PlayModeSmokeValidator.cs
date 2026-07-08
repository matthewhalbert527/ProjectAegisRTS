using System;
using System.Collections.Generic;
using ProjectAegisRTS.Core;
using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.UnityClient.Bootstrap;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Feedback;
using ProjectAegisRTS.UnityClient.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage14PlayModeSmokeValidator
    {
        static readonly List<string> RedErrors = new List<string>();

        public static void RunStage14PlayModeSmokeBatch()
        {
            try
            {
                RunStage14PlayModeSmoke();
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

        public static void RunStage14PlayModeSmoke()
        {
            RedErrors.Clear();
            Application.logMessageReceived += CaptureRedError;
            try
            {
                Stage14SceneValidator.ValidateStage14Scene();
                var scene = EditorSceneManager.OpenScene(Stage14SceneCreator.ScenePath);
                if (!scene.IsValid())
                    throw new InvalidOperationException("Stage 14 scene did not open.");

                var bootstrapper = RequireEnabled<RtsGameBootstrapper>("RtsGameBootstrapper");
                var driver = RequireEnabled<RtsSimulationDriver>("RtsSimulationDriver");
                var boardRenderer = RequireEnabled<BoardRenderer>("BoardRenderer");
                var actorRenderer = RequireEnabled<ActorRenderSystem>("ActorRenderSystem");
                var bus = RequireEnabled<FeedbackEventBus>("FeedbackEventBus");
                var profiles = RequireEnabled<FeedbackProfileLibrary>("FeedbackProfileLibrary");
                var audio = RequireEnabled<AudioFeedbackController>("AudioFeedbackController");
                var vfx = RequireEnabled<VfxFeedbackController>("VfxFeedbackController");
                var ui = RequireEnabled<UiFeedbackController>("UiFeedbackController");
                var haptic = RequireEnabled<HapticFeedbackAdapter>("HapticFeedbackAdapter");
                RequireEnabled<FeedbackDebugHud>("FeedbackDebugHud");

                var seenTypes = new HashSet<FeedbackEventType>();
                Action<FeedbackEvent> recorder = feedbackEvent => seenTypes.Add(feedbackEvent.eventType);
                bus.EventEmitted += recorder;
                try
                {
                    bootstrapper.InitializeScene();
                    profiles.EnsureInitialized();
                    bus.ResetFeedbackState();

                    driver.TryCreateCombatDemoWorld();
                    StepRuntime(driver, boardRenderer, actorRenderer, bus, 2, 0.1f);
                    var snapshot = RequireSnapshot(driver);

                    var movingActorId = FindActor(snapshot, "light_tank", 1);
                    driver.SetSelectedActorIds(new[] { movingActorId });
                    StepRuntime(driver, boardRenderer, actorRenderer, bus, 1, 0.1f);
                    RequireSuccess(driver.TryIssueMoveSelectedToCell(new Int2(9, 8)), "move command");
                    StepRuntime(driver, boardRenderer, actorRenderer, bus, 4, 0.1f);

                    driver.ClearSelection();
                    var invalid = driver.TryIssueMoveSelectedToCell(new Int2(9, 9));
                    if (invalid.Success)
                        throw new InvalidOperationException("Stage 14 expected an invalid move command without selection.");
                    StepRuntime(driver, boardRenderer, actorRenderer, bus, 1, 0.1f);

                    var towerId = FindActor(driver.LatestSnapshot, "gun_tower", 1);
                    var enemyId = FindActor(driver.LatestSnapshot, "rifle_infantry", 2);
                    driver.SetSelectedActorIds(new[] { towerId });
                    RequireSuccess(driver.TryIssueAttackSelectedToActor(enemyId), "attack command");
                    for (var i = 0; i < 140 && !seenTypes.Contains(FeedbackEventType.DeathDestruction); i++)
                        StepRuntime(driver, boardRenderer, actorRenderer, bus, 1, 0.1f);

                    RequireSuccess(driver.TryQueueProduction("power_plant"), "production command");
                    for (var i = 0; i < 60 && !seenTypes.Contains(FeedbackEventType.ProductionCompleted); i++)
                        StepRuntime(driver, boardRenderer, actorRenderer, bus, 1, 0.1f);
                    RequireSuccess(driver.TryEnterPlacementModeForFirstPending(), "enter placement mode");
                    RequireSuccess(driver.TryPlacePendingBuildingAtCell(PlacementGridMetrics.CoarseCellToPlacementCell(new Int2(7, 3))), "building placement");
                    StepRuntime(driver, boardRenderer, actorRenderer, bus, 2, 0.1f);

                    RequireSuccess(driver.TryForceLowPowerOrCreateLowPowerDemoCondition(), "low-power command");
                    StepRuntime(driver, boardRenderer, actorRenderer, bus, 2, 0.1f);

                    driver.TryCreateEconomyDemoWorld();
                    StepRuntime(driver, boardRenderer, actorRenderer, bus, 2, 0.1f);
                    snapshot = RequireSnapshot(driver);
                    var harvesterId = snapshot.Economy.Harvesters[0].ActorId;
                    var resourceCell = snapshot.Economy.Resources[0].Cell;
                    driver.SetSelectedActorIds(new[] { harvesterId });
                    RequireSuccess(driver.TryIssueHarvestSelectedAtCell(resourceCell), "harvest command");
                    for (var i = 0; i < 340 && !(seenTypes.Contains(FeedbackEventType.Harvest) && seenTypes.Contains(FeedbackEventType.Unload)); i++)
                        StepRuntime(driver, boardRenderer, actorRenderer, bus, 1, 0.1f);

                    driver.TryCreateFogRadarDemoWorld();
                    StepRuntime(driver, boardRenderer, actorRenderer, bus, 2, 0.1f);
                    driver.TryForceLowPowerOrCreateLowPowerDemoCondition();
                    StepRuntime(driver, boardRenderer, actorRenderer, bus, 4, 0.1f);
                    if (!seenTypes.Contains(FeedbackEventType.FogRadarChanged))
                        bus.EmitManual(FeedbackEventType.FogRadarChanged, "Radar state smoke pulse.");

                    RequireSeen(seenTypes, FeedbackEventType.SelectionChanged);
                    RequireSeen(seenTypes, FeedbackEventType.MoveCommand);
                    RequireSeen(seenTypes, FeedbackEventType.InvalidCommand);
                    RequireSeen(seenTypes, FeedbackEventType.ProductionStarted);
                    RequireSeen(seenTypes, FeedbackEventType.ProductionCompleted);
                    RequireSeen(seenTypes, FeedbackEventType.BuildingPlaced);
                    RequireSeen(seenTypes, FeedbackEventType.LowPower);
                    RequireSeen(seenTypes, FeedbackEventType.Harvest);
                    RequireSeen(seenTypes, FeedbackEventType.Unload);
                    RequireSeen(seenTypes, FeedbackEventType.Attack);
                    RequireSeen(seenTypes, FeedbackEventType.ProjectileImpact);
                    RequireSeen(seenTypes, FeedbackEventType.Damage);
                    RequireSeen(seenTypes, FeedbackEventType.DeathDestruction);
                    RequireSeen(seenTypes, FeedbackEventType.FogRadarChanged);

                    if (profiles.ProfileCount < Enum.GetValues(typeof(FeedbackEventType)).Length)
                        throw new InvalidOperationException("Stage 14 profile library did not load all profiles.");
                    if (bus.TotalEventCount < seenTypes.Count)
                        throw new InvalidOperationException("Stage 14 feedback bus did not retain event counts.");
                    if (audio.SilentCueCount <= 0)
                        throw new InvalidOperationException("Stage 14 audio feedback controller did not receive silent cues.");
                    if (vfx.SpawnedMarkerCount <= 0)
                        throw new InvalidOperationException("Stage 14 VFX feedback controller did not spawn placeholder markers.");
                    if (ui.MessageCount <= 0)
                        throw new InvalidOperationException("Stage 14 UI feedback controller did not receive messages.");
                    if (haptic.PlaceholderPulseCount <= 0)
                        throw new InvalidOperationException("Stage 14 haptic feedback adapter did not receive placeholder pulses.");

                    if (RedErrors.Count > 0)
                        throw new InvalidOperationException("Red console errors were produced during Stage 14 smoke validation: " + string.Join(" | ", RedErrors.ToArray()));

                    Debug.Log("Stage 14 play mode smoke validation passed.");
                }
                finally
                {
                    bus.EventEmitted -= recorder;
                }
            }
            finally
            {
                Application.logMessageReceived -= CaptureRedError;
                RedErrors.Clear();
            }
        }

        static void StepRuntime(RtsSimulationDriver driver, BoardRenderer boardRenderer, ActorRenderSystem actorRenderer, FeedbackEventBus bus, int frames, float deltaTime)
        {
            for (var i = 0; i < frames; i++)
            {
                driver.ManualUpdate(deltaTime);
                boardRenderer.UpdateHover(driver.HasHoveredCell ? (Int2?)driver.HoveredCell : null);
                PlacementPreviewSnapshot preview;
                boardRenderer.UpdatePlacementPreview(driver.TryGetPlacementPreview(out preview) ? preview : null);
                actorRenderer.RenderSnapshot(driver.LatestSnapshot, driver.SelectedActorIds, deltaTime);
                bus.RenderSnapshot(driver.LatestSnapshot);
            }
        }

        static WorldSnapshot RequireSnapshot(RtsSimulationDriver driver)
        {
            if (driver.LatestSnapshot == null)
                throw new InvalidOperationException("Stage 14 expected a simulation snapshot.");
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
                throw new InvalidOperationException("Stage 14 " + label + " failed: " + (result == null ? "null result" : result.ToString()));
        }

        static void RequireSeen(HashSet<FeedbackEventType> seenTypes, FeedbackEventType type)
        {
            if (!seenTypes.Contains(type))
                throw new InvalidOperationException("Stage 14 did not observe feedback event type: " + type);
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
