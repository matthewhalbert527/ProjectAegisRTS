using System;
using System.Collections.Generic;
using ProjectAegisRTS.Core;
using ProjectAegisRTS.Data;
using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.UnityClient.Board;
using ProjectAegisRTS.UnityClient.Bootstrap;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Rendering;
using ProjectAegisRTS.UnityClient.Rendering.Buildings;
using ProjectAegisRTS.UnityClient.UI.Common;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage7PlayModeSmokeValidator
    {
        static readonly List<string> RedErrors = new List<string>();

        public static void RunStage7PlayModeSmokeBatch()
        {
            try
            {
                RunStage7PlayModeSmoke();
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

        public static void RunStage7PlayModeSmoke()
        {
            RedErrors.Clear();
            Application.logMessageReceived += CaptureRedError;
            try
            {
                Stage7SceneValidator.ValidateStage7Scene();
                var scene = EditorSceneManager.OpenScene(Stage7SceneCreator.ScenePath);
                if (!scene.IsValid())
                    throw new InvalidOperationException("Stage 7 scene did not open.");

                var bootstrapper = RequireEnabled<RtsGameBootstrapper>("RtsGameBootstrapper");
                var driver = RequireEnabled<RtsSimulationDriver>("RtsSimulationDriver");
                var boardRenderer = RequireEnabled<BoardRenderer>("BoardRenderer");
                var actorRenderer = RequireEnabled<ActorRenderSystem>("ActorRenderSystem");
                var library = RequireEnabled<BuildingVisualProfileLibrary>("BuildingVisualProfileLibrary");
                var demo = RequireEnabled<BuildingPowerDemoController>("BuildingPowerDemoController");
                RequireEnabled<BuildingAnimationDebugHud>("BuildingAnimationDebugHud");

                bootstrapper.InitializeScene();
                library.EnsureInitialized();
                StepRuntime(driver, boardRenderer, actorRenderer, 8, 0.1f);

                if (driver.LatestSnapshot == null || driver.LatestSnapshot.Tick <= 0)
                    throw new InvalidOperationException("Stage 7 tick did not advance.");
                if (actorRenderer.ActorVisualCount < 2)
                    throw new InvalidOperationException("Stage 7 actor visuals were not generated.");
                if (actorRenderer.BuildingVisualCount <= 0)
                    throw new InvalidOperationException("Stage 7 no building visual controllers were reported.");

                ActorViewBehaviour buildingView;
                if (!actorRenderer.TryGetDebugBuildingView(out buildingView) || buildingView == null || buildingView.BuildingVisual == null)
                    throw new InvalidOperationException("Stage 7 could not find a building actor view with BuildingVisualStateController.");

                var building = buildingView.BuildingVisual;
                if (!building.HasGeneratedParts)
                    throw new InvalidOperationException("Stage 7 building placeholder parts were not generated.");
                if (building.Lights == null || building.Machinery == null || building.Production == null || building.Door == null || building.Damage == null)
                    throw new InvalidOperationException("Stage 7 one or more building visual child controllers did not initialize.");

                RequireProfile(library, "power_plant");
                RequireProfile(library, "barracks");
                RequireProfile(library, "war_factory");
                RequireProfile(library, "refinery");
                RequireProfile(library, "gun_tower");
                RequireProfile(library, "fabrication_hub");

                demo.TriggerLowPowerDemo();
                StepRuntime(driver, boardRenderer, actorRenderer, 4, 0.1f);
                if (actorRenderer.LowPowerBuildingCount <= 0 && building.PowerVisualState != BuildingPowerVisualState.LowPower)
                    throw new InvalidOperationException("Stage 7 low-power demo did not visually affect a building controller.");

                demo.ClearLowPowerDemo();
                StepRuntime(driver, boardRenderer, actorRenderer, 4, 0.1f);

                demo.QueuePowerPlantDemo();
                StepRuntime(driver, boardRenderer, actorRenderer, 6, 0.1f);
                if (actorRenderer.ProducingBuildingCount <= 0)
                {
                    demo.ForceProductionVisualDemo();
                    StepRuntime(driver, boardRenderer, actorRenderer, 2, 0.1f);
                    if (building.AnimationVisualState != BuildingAnimationVisualState.Producing)
                        throw new InvalidOperationException("Stage 7 production visual state was not observable or forceable.");
                }

                building.SetDebugForcedState(BuildingAnimationVisualState.Damaged);
                building.TickVisual(0.1f);
                if (building.Damage == null || !building.Damage.IsDamaged)
                    throw new InvalidOperationException("Stage 7 damage placeholder did not activate.");
                building.SetDebugForcedState(null);

                if (RedErrors.Count > 0)
                    throw new InvalidOperationException("Red console errors were produced during Stage 7 smoke validation: " + string.Join(" | ", RedErrors.ToArray()));

                Debug.Log("Stage 7 play mode smoke validation passed.");
            }
            finally
            {
                Application.logMessageReceived -= CaptureRedError;
                RedErrors.Clear();
            }
        }

        static void RequireProfile(BuildingVisualProfileLibrary library, string typeId)
        {
            if (library.GetProfile(typeId, null) == null)
                throw new InvalidOperationException("Missing Stage 7 profile for " + typeId);
        }

        static void StepRuntime(RtsSimulationDriver driver, BoardRenderer boardRenderer, ActorRenderSystem actorRenderer, int frames, float deltaTime)
        {
            for (var i = 0; i < frames; i++)
            {
                driver.ManualUpdate(deltaTime);
                boardRenderer.UpdateHover(driver.HasHoveredCell ? (Int2?)driver.HoveredCell : null);
                PlacementPreviewSnapshot preview;
                boardRenderer.UpdatePlacementPreview(driver.TryGetPlacementPreview(out preview) ? preview : null);
                actorRenderer.RenderSnapshot(driver.LatestSnapshot, driver.SelectedActorIds, deltaTime);
            }
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
