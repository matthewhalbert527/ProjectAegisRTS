using System;
using ProjectAegisRTS.UnityClient.Boot;
using ProjectAegisRTS.UnityClient.Art;
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
using ProjectAegisRTS.UnityClient.UI.XR.LeftHand;
using ProjectAegisRTS.UnityClient.UI.XR.RightHand;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage16SceneValidator
    {
        public static void ValidateStage16SceneBatch()
        {
            try
            {
                ValidateStage16Scene();
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

        public static void ValidateStage16Scene()
        {
            if (!System.IO.File.Exists(Stage16SceneCreator.ScenePath))
                throw new InvalidOperationException("Stage 16 scene missing: " + Stage16SceneCreator.ScenePath);

            var scene = EditorSceneManager.OpenScene(Stage16SceneCreator.ScenePath);
            if (!scene.IsValid())
                throw new InvalidOperationException("Stage 16 scene did not open.");

            RequireObject("RtsGame");
            RequireObject("BoardRoot");
            if (Camera.main == null && UnityEngine.Object.FindFirstObjectByType<Camera>() == null)
                throw new InvalidOperationException("Stage 16 camera missing.");
            if (UnityEngine.Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
                throw new InvalidOperationException("Stage 16 EventSystem missing.");
            if (UnityEngine.Object.FindFirstObjectByType<Canvas>() == null)
                throw new InvalidOperationException("Stage 16 Canvas missing.");

            var bootstrapper = Require<RtsGameBootstrapper>("RtsGameBootstrapper");
            var driver = Require<RtsSimulationDriver>("RtsSimulationDriver");
            var controller = Require<VerticalSliceScenarioController>("VerticalSliceScenarioController");
            var objectiveHud = Require<MatchObjectiveHud>("MatchObjectiveHud");
            var statusHud = Require<IntegratedSystemsStatusHud>("IntegratedSystemsStatusHud");
            var debugActions = Require<VerticalSliceDebugActions>("VerticalSliceDebugActions");
            var initializer = Require<PlayerBuildSceneInitializer>("PlayerBuildSceneInitializer");
            var debugVisibility = Require<DebugHudVisibilityController>("DebugHudVisibilityController");

            Require<BoardRenderer>("BoardRenderer");
            Require<ActorRenderSystem>("ActorRenderSystem");
            Require<DesktopRtsHudRoot>("DesktopRtsHudRoot");
            Require<DesktopUiCommandRouter>("DesktopUiCommandRouter");
            Require<DesktopSidebarController>("DesktopSidebarController");
            Require<Stage4ModeCoordinator>("Stage4ModeCoordinator");
            Require<Stage5DualHandModeCoordinator>("Stage5DualHandModeCoordinator");
            Require<ActorVisualPrefabResolver>("ActorVisualPrefabResolver");
            Require<ProjectileRenderSystem>("ProjectileRenderSystem");
            Require<CombatEventRenderSystem>("CombatEventRenderSystem");
            Require<ResourceFieldRenderSystem>("ResourceFieldRenderSystem");
            Require<FogOverlayRenderer>("FogOverlayRenderer");
            Require<MinimapRenderSystem>("MinimapRenderSystem");
            Require<AiIntentRenderSystem>("AiIntentRenderSystem");
            Require<TerrainDebugRenderer>("TerrainDebugRenderer");
            Require<PathDebugRenderer>("PathDebugRenderer");
            Require<FeedbackEventBus>("FeedbackEventBus");
            Require<RuntimePerformanceStats>("RuntimePerformanceStats");
            Require<RenderStatsHud>("RenderStatsHud");

            if (!driver.UseVerticalSliceDemoWorld)
                throw new InvalidOperationException("Stage 16 scene must use the vertical slice demo world.");
            if (!driver.UsePlayerPerspectiveSnapshot)
                throw new InvalidOperationException("Stage 16 scene must use player-perspective snapshots for fog/minimap.");
            if (bootstrapper.verticalSliceScenarioController != controller ||
                bootstrapper.matchObjectiveHud != objectiveHud ||
                bootstrapper.integratedSystemsStatusHud != statusHud ||
                bootstrapper.verticalSliceDebugActions != debugActions)
                throw new InvalidOperationException("Stage 16 bootstrapper scenario references are incomplete.");
            if (!initializer.frameCameraOnStart || !initializer.startScenarioOnLoad || !initializer.hideDebugPanelsOnStart || !initializer.cancelPlacementOnStart)
                throw new InvalidOperationException("Stage 16 player build initializer defaults are incomplete.");
            if (debugVisibility.showDebugPanelsByDefault)
                throw new InvalidOperationException("Stage 16 debug panels must be hidden by default.");
            if (!objectiveHud.visible)
                throw new InvalidOperationException("Stage 16 objective HUD must be visible by default.");
            if (objectiveHud.showDebugActions)
                throw new InvalidOperationException("Stage 16 objective HUD debug actions must be hidden by default.");
            if (statusHud.visible)
                throw new InvalidOperationException("Stage 16 integrated systems debug HUD must be hidden by default.");
            ValidateCameraDefaults();
            if (!System.IO.File.Exists(Stage15SceneCreator.ScenePath))
                throw new InvalidOperationException("Previous stage scene missing: " + Stage15SceneCreator.ScenePath);

            Debug.Log("Stage 16 scene validation passed.");
        }

        static void ValidateCameraDefaults()
        {
            var camera = Camera.main != null ? Camera.main : UnityEngine.Object.FindFirstObjectByType<Camera>();
            if (camera == null)
                throw new InvalidOperationException("Stage 16 camera missing.");
            if (!camera.orthographic)
                throw new InvalidOperationException("Stage 16 camera must be orthographic.");
            if (Math.Abs(camera.orthographicSize - 28f) > 0.01f)
                throw new InvalidOperationException("Stage 16 camera orthographic size must be 28.");
            if (camera.clearFlags != CameraClearFlags.SolidColor)
                throw new InvalidOperationException("Stage 16 camera must use a solid player-readable background.");
            if (Math.Abs(camera.nearClipPlane - 0.1f) > 0.01f || Math.Abs(camera.farClipPlane - 1000f) > 0.01f)
                throw new InvalidOperationException("Stage 16 camera clipping planes are incorrect.");
        }

        static void RequireObject(string name)
        {
            if (GameObject.Find(name) == null)
                throw new InvalidOperationException("Missing GameObject: " + name);
        }

        static T Require<T>(string label) where T : Component
        {
            var component = UnityEngine.Object.FindFirstObjectByType<T>();
            if (component == null)
                throw new InvalidOperationException("Missing component: " + label);
            return component;
        }
    }
}
