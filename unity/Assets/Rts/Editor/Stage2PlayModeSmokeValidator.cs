using System;
using System.Collections.Generic;
using ProjectAegisRTS.UnityClient.Bootstrap;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Rendering;
using ProjectAegisRTS.UnityClient.UI.Common;
using ProjectAegisRTS.UnityClient.UI.Desktop;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage2PlayModeSmokeValidator
    {
        static readonly List<string> RedErrors = new List<string>();

        public static void RunStage2PlayModeSmokeBatch()
        {
            try
            {
                RunStage2PlayModeSmoke();
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

        public static void RunStage2PlayModeSmoke()
        {
            RedErrors.Clear();
            Application.logMessageReceived += CaptureRedError;
            try
            {
                Stage2SceneValidator.ValidateStage2Scene();
                var scene = EditorSceneManager.OpenScene(Stage2SceneCreator.ScenePath);
                if (!scene.IsValid())
                    throw new InvalidOperationException("Stage 2 scene did not open.");

                RequireObject("RtsGame");
                var boardRoot = RequireObject("BoardRoot");
                RequireObject("Main Camera");
                RequireObject("Directional Light");
                RequireObject("EventSystem");
                RequireEnabled<EventSystem>("EventSystem");
                RequireEnabled<Canvas>("Canvas");

                var camera = RequireEnabled<Camera>("Main Camera");
                if (!camera.orthographic || Mathf.Abs(camera.orthographicSize - 28f) > 0.01f)
                    throw new InvalidOperationException("Stage 2 camera is not in the expected orthographic framing.");

                var bootstrapper = RequireEnabled<RtsGameBootstrapper>("RtsGameBootstrapper");
                var driver = RequireEnabled<RtsSimulationDriver>("RtsSimulationDriver");
                var boardRenderer = RequireEnabled<BoardRenderer>("BoardRenderer");
                var actorRenderer = RequireEnabled<ActorRenderSystem>("ActorRenderSystem");
                var hud = RequireEnabled<DesktopRtsHudRoot>("DesktopRtsHudRoot");
                var sidebar = RequireEnabled<DesktopSidebarController>("DesktopSidebarController");
                RequireEnabled<ProductionCategoryTabs>("ProductionCategoryTabs");
                RequireEnabled<ProductionGridController>("ProductionGridController");
                RequireEnabled<ProductionQueuePanel>("ProductionQueuePanel");
                RequireEnabled<PlacementModePanel>("PlacementModePanel");
                var selectionPanel = RequireEnabled<SelectionPanelController>("SelectionPanelController");
                RequireEnabled<CommandBarController>("CommandBarController");
                RequireEnabled<MinimapPlaceholderController>("MinimapPlaceholderController");
                RequireEnabled<RtsStatusLog>("RtsStatusLog");
                var router = RequireEnabled<DesktopUiCommandRouter>("DesktopUiCommandRouter");

                bootstrapper.InitializeScene();
                hud.Initialize();
                StepRuntime(bootstrapper, driver, boardRenderer, actorRenderer, 8, 0.1f);

                if (boardRoot.transform.Find("Board Surface") == null)
                    throw new InvalidOperationException("Board visuals were not generated.");

                var actorViews = GameObject.Find("Actor Views");
                if (actorViews == null || actorViews.transform.childCount == 0)
                    throw new InvalidOperationException("Actor visuals were not generated.");

                if (driver.LatestSnapshot == null)
                    throw new InvalidOperationException("Simulation driver did not publish a snapshot.");
                if (driver.LatestSnapshot.Actors.Count == 0)
                    throw new InvalidOperationException("Simulation snapshot has no actors.");

                var tickBeforePause = driver.LatestSnapshot.Tick;
                router.TogglePause();
                StepRuntime(bootstrapper, driver, boardRenderer, actorRenderer, 5, 0.25f);
                if (driver.LatestSnapshot.Tick != tickBeforePause)
                    throw new InvalidOperationException("Paused Stage 2 simulation advanced ticks.");

                router.StepTick();
                if (driver.LatestSnapshot.Tick != tickBeforePause + 1)
                    throw new InvalidOperationException("Single-step did not advance exactly one deterministic tick.");

                router.TogglePause();
                StepRuntime(bootstrapper, driver, boardRenderer, actorRenderer, 4, 0.1f);
                if (driver.LatestSnapshot.Tick <= tickBeforePause + 1)
                    throw new InvalidOperationException("Unpaused Stage 2 simulation did not advance ticks.");

                var productionResult = router.QueueProduction("power_plant");
                if (productionResult == null)
                    throw new InvalidOperationException("Production command path returned no result.");

                var lowPowerResult = router.TriggerLowPowerDemo();
                if (lowPowerResult == null)
                    throw new InvalidOperationException("Low-power command path returned no result.");

                var firstActor = driver.LatestSnapshot.Actors[0];
                router.SelectAtCell(firstActor.CellPosition);
                selectionPanel.Initialize(driver, router);
                StepRuntime(bootstrapper, driver, boardRenderer, actorRenderer, 2, 0.05f);

                if (!sidebar.isActiveAndEnabled)
                    throw new InvalidOperationException("Stage 2 sidebar became inactive during smoke validation.");

                if (RedErrors.Count > 0)
                    throw new InvalidOperationException("Red console errors were produced during Stage 2 smoke validation: " + string.Join(" | ", RedErrors.ToArray()));

                Debug.Log("Stage 2 play mode smoke validation passed.");
            }
            finally
            {
                Application.logMessageReceived -= CaptureRedError;
                RedErrors.Clear();
            }
        }

        static void StepRuntime(
            RtsGameBootstrapper bootstrapper,
            RtsSimulationDriver driver,
            BoardRenderer boardRenderer,
            ActorRenderSystem actorRenderer,
            int frames,
            float deltaTime)
        {
            for (var i = 0; i < frames; i++)
            {
                driver.ManualUpdate(deltaTime);
                boardRenderer.UpdateHover(driver.HasHoveredCell ? (ProjectAegisRTS.Core.Int2?)driver.HoveredCell : null);
                PlacementPreviewSnapshotAdapter.UpdatePreview(boardRenderer, driver);
                actorRenderer.RenderSnapshot(driver.LatestSnapshot, driver.SelectedActorIds, deltaTime);
            }
        }

        static GameObject RequireObject(string objectName)
        {
            var obj = GameObject.Find(objectName);
            if (obj == null)
                throw new InvalidOperationException("Missing GameObject: " + objectName);
            return obj;
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

        static class PlacementPreviewSnapshotAdapter
        {
            public static void UpdatePreview(BoardRenderer renderer, RtsSimulationDriver driver)
            {
                ProjectAegisRTS.Snapshots.PlacementPreviewSnapshot preview;
                renderer.UpdatePlacementPreview(driver.TryGetPlacementPreview(out preview) ? preview : null);
            }
        }
    }
}
