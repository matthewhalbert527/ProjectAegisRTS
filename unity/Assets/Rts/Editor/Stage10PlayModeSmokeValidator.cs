using System;
using System.Collections.Generic;
using ProjectAegisRTS.Core;
using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.UnityClient.Bootstrap;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Rendering;
using ProjectAegisRTS.UnityClient.Rendering.Economy;
using ProjectAegisRTS.UnityClient.UI.Common;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage10PlayModeSmokeValidator
    {
        static readonly List<string> RedErrors = new List<string>();

        public static void RunStage10PlayModeSmokeBatch()
        {
            try
            {
                RunStage10PlayModeSmoke();
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

        public static void RunStage10PlayModeSmoke()
        {
            RedErrors.Clear();
            Application.logMessageReceived += CaptureRedError;
            try
            {
                Stage10SceneValidator.ValidateStage10Scene();
                var scene = EditorSceneManager.OpenScene(Stage10SceneCreator.ScenePath);
                if (!scene.IsValid())
                    throw new InvalidOperationException("Stage 10 scene did not open.");

                var bootstrapper = RequireEnabled<RtsGameBootstrapper>("RtsGameBootstrapper");
                var driver = RequireEnabled<RtsSimulationDriver>("RtsSimulationDriver");
                var boardRenderer = RequireEnabled<BoardRenderer>("BoardRenderer");
                var actorRenderer = RequireEnabled<ActorRenderSystem>("ActorRenderSystem");
                var resourceRenderer = RequireEnabled<ResourceFieldRenderSystem>("ResourceFieldRenderSystem");
                var cargoRenderer = RequireEnabled<HarvesterCargoVisualController>("HarvesterCargoVisualController");
                var dockRenderer = RequireEnabled<RefineryDockVisualController>("RefineryDockVisualController");
                var eventRenderer = RequireEnabled<EconomyEventRenderSystem>("EconomyEventRenderSystem");
                RequireEnabled<EconomyDebugHud>("EconomyDebugHud");

                bootstrapper.InitializeScene();
                driver.TryCreateEconomyDemoWorld();
                StepRuntime(driver, boardRenderer, actorRenderer, resourceRenderer, cargoRenderer, dockRenderer, eventRenderer, 4, 0.1f);

                var snapshot = driver.LatestSnapshot;
                if (snapshot == null || snapshot.Economy.Resources.Count == 0 || snapshot.Economy.Harvesters.Count == 0 || snapshot.Economy.Refineries.Count == 0)
                    throw new InvalidOperationException("Stage 10 economy demo world did not create resources, harvester, and refinery.");

                var harvesterId = snapshot.Economy.Harvesters[0].ActorId;
                var resourceCell = snapshot.Economy.Resources[0].Cell;
                var startResource = snapshot.Economy.Resources[0].Amount;
                var startCredits = snapshot.Players[0].Credits;
                driver.SetSelectedActorIds(new[] { harvesterId });
                var harvest = driver.TryIssueHarvestSelectedAtCell(resourceCell);
                if (!harvest.Success)
                    throw new InvalidOperationException("Stage 10 harvest command failed: " + harvest);

                var sawCargo = false;
                var sawResourceDecrease = false;
                var sawUnload = false;
                var sawCreditIncrease = false;
                for (var i = 0; i < 320; i++)
                {
                    StepRuntime(driver, boardRenderer, actorRenderer, resourceRenderer, cargoRenderer, dockRenderer, eventRenderer, 1, 0.1f);
                    snapshot = driver.LatestSnapshot;
                    var harvester = snapshot.Economy.Harvesters[0];
                    if (harvester.CargoAmount > 0 || cargoRenderer.LastObservedCargoAmount > 0)
                        sawCargo = true;
                    if (snapshot.Economy.Resources[0].Amount < startResource)
                        sawResourceDecrease = true;
                    if (HasEconomyEvent(snapshot, "HarvesterUnloaded") || eventRenderer.LastEventType == "HarvesterUnloaded")
                        sawUnload = true;
                    if (snapshot.Players[0].Credits > startCredits)
                        sawCreditIncrease = true;
                    if (sawCargo && sawResourceDecrease && sawUnload && sawCreditIncrease)
                        break;
                }

                if (driver.LatestSnapshot.Tick <= 0)
                    throw new InvalidOperationException("Stage 10 tick did not advance.");
                if (actorRenderer.ActorVisualCount < 3)
                    throw new InvalidOperationException("Stage 10 actor visuals were not generated.");
                if (resourceRenderer.ResourceVisualCount <= 0)
                    throw new InvalidOperationException("Stage 10 resource visuals were not generated.");
                if (dockRenderer.DockVisualCount <= 0)
                    throw new InvalidOperationException("Stage 10 refinery dock visual was not generated.");
                if (!sawCargo)
                    throw new InvalidOperationException("Stage 10 harvester cargo did not increase.");
                if (!sawResourceDecrease)
                    throw new InvalidOperationException("Stage 10 resource amount did not decrease.");
                if (!sawUnload)
                    throw new InvalidOperationException("Stage 10 unload event did not appear.");
                if (!sawCreditIncrease)
                    throw new InvalidOperationException("Stage 10 credits did not increase after unload.");
                if (eventRenderer.PlayedEventCount <= 0)
                    throw new InvalidOperationException("Stage 10 economy event renderer did not play events.");

                if (RedErrors.Count > 0)
                    throw new InvalidOperationException("Red console errors were produced during Stage 10 smoke validation: " + string.Join(" | ", RedErrors.ToArray()));

                Debug.Log("Stage 10 play mode smoke validation passed.");
            }
            finally
            {
                Application.logMessageReceived -= CaptureRedError;
                RedErrors.Clear();
            }
        }

        static void StepRuntime(RtsSimulationDriver driver, BoardRenderer boardRenderer, ActorRenderSystem actorRenderer, ResourceFieldRenderSystem resourceRenderer, HarvesterCargoVisualController cargoRenderer, RefineryDockVisualController dockRenderer, EconomyEventRenderSystem eventRenderer, int frames, float deltaTime)
        {
            for (var i = 0; i < frames; i++)
            {
                driver.ManualUpdate(deltaTime);
                boardRenderer.UpdateHover(driver.HasHoveredCell ? (Int2?)driver.HoveredCell : null);
                PlacementPreviewSnapshot preview;
                boardRenderer.UpdatePlacementPreview(driver.TryGetPlacementPreview(out preview) ? preview : null);
                actorRenderer.RenderSnapshot(driver.LatestSnapshot, driver.SelectedActorIds, deltaTime);
                resourceRenderer.RenderSnapshot(driver.LatestSnapshot);
                cargoRenderer.RenderSnapshot(driver.LatestSnapshot);
                dockRenderer.RenderSnapshot(driver.LatestSnapshot);
                eventRenderer.RenderSnapshot(driver.LatestSnapshot);
            }
        }

        static bool HasEconomyEvent(WorldSnapshot snapshot, string eventType)
        {
            for (var i = 0; i < snapshot.Economy.Events.Count; i++)
                if (snapshot.Economy.Events[i].EventType == eventType)
                    return true;
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
