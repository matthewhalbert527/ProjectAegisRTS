using System;
using System.Collections.Generic;
using ProjectAegisRTS.Core;
using ProjectAegisRTS.Data;
using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.UnityClient.Board;
using ProjectAegisRTS.UnityClient.Bootstrap;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Rendering;
using ProjectAegisRTS.UnityClient.Rendering.Motion;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage6PlayModeSmokeValidator
    {
        static readonly List<string> RedErrors = new List<string>();

        public static void RunStage6PlayModeSmokeBatch()
        {
            try
            {
                RunStage6PlayModeSmoke();
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

        public static void RunStage6PlayModeSmoke()
        {
            RedErrors.Clear();
            Application.logMessageReceived += CaptureRedError;
            try
            {
                Stage6SceneValidator.ValidateStage6Scene();
                var scene = EditorSceneManager.OpenScene(Stage6SceneCreator.ScenePath);
                if (!scene.IsValid())
                    throw new InvalidOperationException("Stage 6 scene did not open.");

                var bootstrapper = RequireEnabled<RtsGameBootstrapper>("RtsGameBootstrapper");
                var driver = RequireEnabled<RtsSimulationDriver>("RtsSimulationDriver");
                var boardRenderer = RequireEnabled<BoardRenderer>("BoardRenderer");
                var actorRenderer = RequireEnabled<ActorRenderSystem>("ActorRenderSystem");
                var mapper = RequireEnabled<BoardCoordinateMapper>("BoardCoordinateMapper");
                var profileLibrary = RequireEnabled<VisualMotionProfileLibrary>("VisualMotionProfileLibrary");
                var commandPreview = RequireEnabled<CommandPreviewRenderer>("CommandPreviewRenderer");
                var pathPreview = RequireEnabled<MovementPathPreview>("MovementPathPreview");
                var showcase = RequireEnabled<Stage6MotionShowcase>("Stage6MotionShowcase");

                bootstrapper.InitializeScene();
                profileLibrary.EnsureInitialized();
                pathPreview.Initialize(mapper);
                showcase.EnsureShowcase();
                StepRuntime(driver, boardRenderer, actorRenderer, 8, 0.1f);

                if (actorRenderer.ActorVisualCount < 2)
                    throw new InvalidOperationException("Stage 6 did not generate the expected demo actor visuals.");

                var mobile = FindMobileActor(driver);
                var selectResult = driver.SetSelectedActorIds(new[] { mobile.ActorId });
                if (!selectResult.Success || driver.SelectedActorIds.Count == 0)
                    throw new InvalidOperationException("Stage 6 could not select a mobile actor: " + selectResult);

                var target = new Int2(16, 10);
                var moveResult = driver.TryIssueMoveSelectedToCell(target);
                if (!moveResult.Success)
                    throw new InvalidOperationException("Stage 6 move command failed: " + moveResult);

                commandPreview.ShowMovePath(mobile.CellPosition, target);
                StepRuntime(driver, boardRenderer, actorRenderer, 16, 0.1f);

                ActorViewBehaviour view;
                if (!actorRenderer.TryGetActorView(mobile.ActorId, out view) || view == null)
                    throw new InvalidOperationException("Stage 6 could not find the selected actor view.");
                if (view.ActorVisualMotion == null || view.ActorVisualMotion.ActiveProfile == null)
                    throw new InvalidOperationException("Stage 6 selected actor is missing base visual motion.");
                if (view.VehicleMotion == null || !view.VehicleMotion.enabled)
                    throw new InvalidOperationException("Stage 6 selected vehicle is missing vehicle motion details.");
                if (actorRenderer.VehicleMotionControllerCount <= 0)
                    throw new InvalidOperationException("Stage 6 did not report any vehicle motion controllers.");
                if (!pathPreview.HasPreview)
                    throw new InvalidOperationException("Stage 6 movement path preview did not render.");

                for (var i = 0; i < 8; i++)
                    showcase.TickShowcase(0.1f);

                if (showcase.VehicleMotion == null || showcase.InfantryMotion == null || showcase.AircraftMotion == null)
                    throw new InvalidOperationException("Stage 6 showcase missing vehicle, infantry, or aircraft motion.");
                if (showcase.VehicleDetails == null || showcase.InfantryDetails == null || showcase.AircraftDetails == null || showcase.TurretAim == null)
                    throw new InvalidOperationException("Stage 6 showcase missing detail controllers.");

                var pause = driver.TogglePause();
                if (!pause.Success || !driver.IsPaused)
                    throw new InvalidOperationException("Stage 6 pause failed: " + pause);
                var step = driver.StepOneTick();
                if (!step.Success)
                    throw new InvalidOperationException("Stage 6 single-step failed: " + step);
                var resume = driver.TogglePause();
                if (!resume.Success || driver.IsPaused)
                    throw new InvalidOperationException("Stage 6 resume failed: " + resume);

                var lowPower = driver.TryForceLowPowerOrCreateLowPowerDemoCondition();
                if (!lowPower.Success)
                    throw new InvalidOperationException("Stage 6 low-power visual demo toggle failed: " + lowPower);
                StepRuntime(driver, boardRenderer, actorRenderer, 4, 0.1f);

                if (RedErrors.Count > 0)
                    throw new InvalidOperationException("Red console errors were produced during Stage 6 smoke validation: " + string.Join(" | ", RedErrors.ToArray()));

                Debug.Log("Stage 6 play mode smoke validation passed.");
            }
            finally
            {
                Application.logMessageReceived -= CaptureRedError;
                RedErrors.Clear();
            }
        }

        static ActorSnapshot FindMobileActor(RtsSimulationDriver driver)
        {
            for (var i = 0; i < driver.LatestSnapshot.Actors.Count; i++)
            {
                var actor = driver.LatestSnapshot.Actors[i];
                ActorDefinition definition;
                if (driver.TryGetDefinition(actor.TypeId, out definition) && definition is UnitDefinition)
                    return actor;
            }

            throw new InvalidOperationException("Stage 6 smoke could not find a mobile unit actor.");
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
