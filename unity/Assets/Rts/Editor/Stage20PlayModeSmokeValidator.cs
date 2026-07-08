using System;
using System.Collections.Generic;
using ProjectAegisRTS.Core;
using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.UnityClient.Art;
using ProjectAegisRTS.UnityClient.Art.Production;
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
    public static class Stage20PlayModeSmokeValidator
    {
        static readonly List<string> RedErrors = new List<string>();

        public static void RunStage20PlayModeSmokeBatch()
        {
            try
            {
                RunStage20PlayModeSmoke();
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

        public static void RunStage20PlayModeSmoke()
        {
            RedErrors.Clear();
            Application.logMessageReceived += CaptureRedError;
            try
            {
                Stage20SceneValidator.ValidateStage20Scene();
                ValidateShowcaseRuntime();
                ValidateStage16RuntimeResolution();

                if (RedErrors.Count > 0)
                    throw new InvalidOperationException("Red console errors were produced during Stage 20 smoke validation: " + string.Join(" | ", RedErrors.ToArray()));

                Debug.Log("Stage 20 play mode smoke validation passed.");
            }
            finally
            {
                Application.logMessageReceived -= CaptureRedError;
                RedErrors.Clear();
            }
        }

        static void ValidateShowcaseRuntime()
        {
            var scene = EditorSceneManager.OpenScene(Stage20SceneCreator.ScenePath);
            if (!scene.IsValid())
                throw new InvalidOperationException("Stage 20 showcase scene did not open for smoke validation.");

            var showcase = RequireEnabled<Stage20MvpVisualShowcaseController>("Stage20MvpVisualShowcaseController");
            showcase.EnsureShowcase();
            if (showcase.MvpProxyCount != Stage20MvpVisualActorSet.ActorTypeIds.Length)
                throw new InvalidOperationException("Stage 20 showcase runtime proxy count mismatch.");
            if (showcase.MissingProxyCount != 0)
                throw new InvalidOperationException("Stage 20 showcase runtime reports missing proxies.");
        }

        static void ValidateStage16RuntimeResolution()
        {
            var scene = EditorSceneManager.OpenScene(Stage16SceneCreator.ScenePath);
            if (!scene.IsValid())
                throw new InvalidOperationException("Stage 16 scene did not open for Stage 20 runtime smoke.");

            var bootstrapper = RequireEnabled<RtsGameBootstrapper>("RtsGameBootstrapper");
            var driver = RequireEnabled<RtsSimulationDriver>("RtsSimulationDriver");
            var boardRenderer = RequireEnabled<BoardRenderer>("BoardRenderer");
            var actorRenderer = RequireEnabled<ActorRenderSystem>("ActorRenderSystem");
            var visualLibrary = RequireEnabled<ActorVisualDefinitionLibrary>("ActorVisualDefinitionLibrary");
            var resolver = RequireEnabled<ActorVisualPrefabResolver>("ActorVisualPrefabResolver");
            var desktopHud = RequireEnabled<DesktopRtsHudRoot>("DesktopRtsHudRoot");
            var mode = RequireEnabled<PlayerFacingUiModeController>("PlayerFacingUiModeController");
            var debugVisibility = RequireEnabled<DebugHudVisibilityController>("DebugHudVisibilityController");
            var placementPanel = RequireComponent<PlacementModePanel>("PlacementModePanel");

            bootstrapper.InitializeScene();
            visualLibrary.EnsureInitialized();
            resolver.definitionLibrary = visualLibrary;
            desktopHud.Initialize();
            mode.ApplyPcDesktopMode();
            StepRuntime(driver, boardRenderer, actorRenderer, 8, 0.05f);

            if (!mode.IsPcSidebarVisibleForDesktop())
                throw new InvalidOperationException("Stage 20 smoke: PCDesktop sidebar is not visible.");
            if (!mode.AreXrBuildMenusHiddenForPc())
                throw new InvalidOperationException("Stage 20 smoke: PCDesktop left-hand menus are not hidden.");
            if (!debugVisibility.AreDebugPanelsHiddenByDefault())
                throw new InvalidOperationException("Stage 20 smoke: debug panels are not hidden by default.");
            if (placementPanel.gameObject.activeInHierarchy)
                throw new InvalidOperationException("Stage 20 smoke: placement UI is visible before placement mode.");

            ValidateMvpResolver(resolver);
            ValidateRuntimeMvpActorViews(driver, actorRenderer);
            Stage20SceneValidator.ValidateStage16UiModes();
        }

        static void ValidateMvpResolver(ActorVisualPrefabResolver resolver)
        {
            var ids = Stage20MvpVisualActorSet.ActorTypeIds;
            for (var i = 0; i < ids.Length; i++)
            {
                ActorVisualDefinition definition;
                GameObject prefab;
                if (!resolver.ResolvePrefab(ids[i], out definition, out prefab) || !Stage20ProductionVisualValidator.IsMvpDefinitionUsingProductionProxy(definition))
                    throw new InvalidOperationException("Stage 20 resolver did not return the MVP production proxy for " + ids[i]);
            }
        }

        static void ValidateRuntimeMvpActorViews(RtsSimulationDriver driver, ActorRenderSystem actorRenderer)
        {
            var snapshot = driver.LatestSnapshot;
            if (snapshot == null || snapshot.Actors.Count == 0)
                throw new InvalidOperationException("Stage 20 smoke expected a live Stage16 snapshot.");

            var checkedCount = 0;
            for (var i = 0; i < snapshot.Actors.Count; i++)
            {
                var actor = snapshot.Actors[i];
                if (!Stage20MvpVisualActorSet.Contains(actor.TypeId))
                    continue;

                ActorViewBehaviour view;
                if (!actorRenderer.TryGetActorView(actor.ActorId, out view) || view == null)
                    throw new InvalidOperationException("Stage 20 smoke missing actor view for " + actor.TypeId);
                if (!Stage20ProductionVisualValidator.IsMvpDefinitionUsingProductionProxy(view.ActiveVisualDefinition))
                    throw new InvalidOperationException("Stage 20 smoke actor view did not use production proxy for " + actor.TypeId);
                checkedCount++;
            }

            if (checkedCount == 0)
                throw new InvalidOperationException("Stage 20 smoke did not encounter any MVP actors in Stage16 runtime.");
        }

        static void StepRuntime(RtsSimulationDriver driver, BoardRenderer boardRenderer, ActorRenderSystem actorRenderer, int frames, float deltaTime)
        {
            for (var i = 0; i < frames; i++)
            {
                driver.ManualUpdate(deltaTime);
                boardRenderer.UpdateHover(driver.HasHoveredCell ? (Int2?)driver.HoveredCell : null, driver.HoveredCellIsPlacementCell);
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
            if (!component.isActiveAndEnabled)
                throw new InvalidOperationException("Component is disabled: " + label);
            return component;
        }

        static T RequireComponent<T>(string label) where T : Component
        {
            var component = UnityEngine.Object.FindFirstObjectByType<T>();
            if (component == null)
            {
                var all = Resources.FindObjectsOfTypeAll<T>();
                for (var i = 0; i < all.Length; i++)
                    if (all[i] != null && all[i].gameObject != null && all[i].gameObject.scene.IsValid())
                        return all[i];
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
