using System;
using System.Collections.Generic;
using ProjectAegisRTS.UnityClient.Boot;
using ProjectAegisRTS.UnityClient.Bootstrap;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Rendering;
using ProjectAegisRTS.UnityClient.UI.Common;
using ProjectAegisRTS.UnityClient.UI.Desktop;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage21_5PlayModeSmokeValidator
    {
        static readonly List<string> RedErrors = new List<string>();

        public static void RunStage21_5PlayModeSmokeBatch()
        {
            try
            {
                RunStage21_5PlayModeSmoke();
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

        public static void RunStage21_5PlayModeSmoke()
        {
            RedErrors.Clear();
            Application.logMessageReceived += CaptureRedError;
            try
            {
                Stage21_5DisplaySettingsValidator.ValidateDisplaySettings();
                ValidateBootRuntime();
                ValidateStage16Runtime();

                if (RedErrors.Count > 0)
                    throw new InvalidOperationException("Red console errors were produced during Stage 21.5 smoke validation: " + string.Join(" | ", RedErrors.ToArray()));

                Debug.Log("Stage 21.5 play mode smoke validation passed.");
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
                throw new InvalidOperationException("Stage 21.5 boot scene did not open for smoke validation.");

            var controller = Require<GameBootController>("GameBootController");
            var menu = Require<MainMenuHud>("MainMenuHud");
            var options = Require<OptionsMenuHud>("OptionsMenuHud");
            var display = Require<PlayerDisplaySettings>("PlayerDisplaySettings");

            if (!menu.visible || options.visible)
                throw new InvalidOperationException("Stage 21.5 boot menu default visibility is incorrect.");
            if (!options.displaySectionEnabled || options.displaySettings != display)
                throw new InvalidOperationException("Stage 21.5 options display section is not wired.");

            controller.ShowOptions();
            if (!options.visible || menu.visible)
                throw new InvalidOperationException("Stage 21.5 options screen did not open.");
            controller.ShowMainMenu();
        }

        static void ValidateStage16Runtime()
        {
            var scene = EditorSceneManager.OpenScene(Stage16SceneCreator.ScenePath);
            if (!scene.IsValid())
                throw new InvalidOperationException("Stage 21.5 Stage16 scene did not open for smoke validation.");

            var bootstrapper = Require<RtsGameBootstrapper>("RtsGameBootstrapper");
            var driver = Require<RtsSimulationDriver>("RtsSimulationDriver");
            var boardRenderer = Require<BoardRenderer>("BoardRenderer");
            var actorRenderer = Require<ActorRenderSystem>("ActorRenderSystem");
            var initializer = Require<PlayerBuildSceneInitializer>("PlayerBuildSceneInitializer");
            var desktopHud = Require<DesktopRtsHudRoot>("DesktopRtsHudRoot");
            var layout = Require<CncStyleSidebarLayout>("CncStyleSidebarLayout");
            var mode = Require<PlayerFacingUiModeController>("PlayerFacingUiModeController");
            var debugVisibility = Require<DebugHudVisibilityController>("DebugHudVisibilityController");
            var placementPanel = Require<PlacementModePanel>("PlacementModePanel");

            bootstrapper.InitializeScene();
            initializer.ApplyPlayerFacingDefaults();
            desktopHud.Initialize();
            layout.ApplyLayout();
            mode.ApplyModeDefaults();
            StepRuntime(driver, boardRenderer, actorRenderer, 4, 0.05f);

            var boardRoot = GameObject.Find("BoardRoot");
            if (boardRoot == null || !boardRoot.activeInHierarchy || boardRoot.transform.childCount == 0)
                throw new InvalidOperationException("Stage 21.5 board visuals are missing.");
            if (!layout.AreProductionPanelsInRightSidebar() || !layout.IsMinimapAboveProductionGrid())
                throw new InvalidOperationException("Stage 21.5 right sidebar/minimap layout is not active.");
            if (!mode.AreXrBuildMenusHiddenForPc())
                throw new InvalidOperationException("Stage 21.5 Quest/XR build menus are visible in PCDesktop mode.");
            if (!debugVisibility.AreDebugPanelsHiddenByDefault())
                throw new InvalidOperationException("Stage 21.5 debug panels are not hidden by default.");
            if (placementPanel.gameObject.activeInHierarchy)
                throw new InvalidOperationException("Stage 21.5 placement UI is visible by default.");
            if (driver.LatestSnapshot == null || driver.LatestSnapshot.Actors.Count == 0)
                throw new InvalidOperationException("Stage 21.5 expected actors in the runtime snapshot.");

            ValidateReadableScreenSizes();
        }

        static void ValidateReadableScreenSizes()
        {
            var display = Require<PlayerDisplaySettings>("PlayerDisplaySettings");
            if (display.minimumWindowWidth > 1280 || display.minimumWindowHeight > 720)
                throw new InvalidOperationException("Stage 21.5 minimum resolution should allow 1280x720.");
            if (display.defaultWindowWidth < 1600 || display.defaultWindowHeight < 900)
                throw new InvalidOperationException("Stage 21.5 default resolution must cover 1600x900.");
            ValidateCanvasScalers();
        }

        static void ValidateCanvasScalers()
        {
            var canvases = UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (canvases.Length == 0)
                throw new InvalidOperationException("Stage 21.5 expected at least one Canvas.");
            for (var i = 0; i < canvases.Length; i++)
            {
                var enforcer = canvases[i].GetComponent<ResponsiveCanvasScalerEnforcer>();
                if (enforcer == null)
                    throw new InvalidOperationException("Stage 21.5 missing ResponsiveCanvasScalerEnforcer on " + canvases[i].name + ".");
                enforcer.Enforce();
            }
        }

        static void StepRuntime(RtsSimulationDriver driver, BoardRenderer boardRenderer, ActorRenderSystem actorRenderer, int frames, float deltaTime)
        {
            for (var i = 0; i < frames; i++)
            {
                driver.ManualUpdate(deltaTime);
                boardRenderer.UpdateHover(driver.HasHoveredCell ? (ProjectAegisRTS.Core.Int2?)driver.HoveredCell : null, driver.HoveredCellIsPlacementCell);
                ProjectAegisRTS.Snapshots.PlacementPreviewSnapshot preview;
                boardRenderer.UpdatePlacementPreview(driver.TryGetPlacementPreview(out preview) ? preview : null);
                actorRenderer.RenderSnapshot(driver.LatestSnapshot, driver.SelectedActorIds, deltaTime);
            }
        }

        static T Require<T>(string label) where T : Component
        {
            var component = UnityEngine.Object.FindFirstObjectByType<T>();
            if (component == null)
            {
                var all = Resources.FindObjectsOfTypeAll<T>();
                for (var i = 0; i < all.Length; i++)
                {
                    if (all[i] != null && all[i].gameObject != null && all[i].gameObject.scene.IsValid())
                    {
                        component = all[i];
                        break;
                    }
                }
            }
            if (component == null)
                throw new InvalidOperationException("Missing component: " + label);
            return component;
        }

        static void CaptureRedError(string condition, string stackTrace, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
                RedErrors.Add(condition);
        }
    }
}
