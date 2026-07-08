using System;
using System.Collections.Generic;
using ProjectAegisRTS.Core;
using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.UnityClient.Art;
using ProjectAegisRTS.UnityClient.Art.Production;
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
    public static class Stage21PlayModeSmokeValidator
    {
        static readonly List<string> RedErrors = new List<string>();

        public static void RunStage21PlayModeSmokeBatch()
        {
            try
            {
                RunStage21PlayModeSmoke();
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

        public static void RunStage21PlayModeSmoke()
        {
            RedErrors.Clear();
            Application.logMessageReceived += CaptureRedError;
            try
            {
                Stage21SceneValidator.ValidateStage21Scene();
                ValidateShowcaseRuntime();
                ValidateBootRuntime();
                ValidateStage16RuntimeVisuals();

                Stage4SceneValidator.ValidateStage4Scene();
                Stage5SceneValidator.ValidateStage5Scene();

                if (RedErrors.Count > 0)
                    throw new InvalidOperationException("Red console errors were produced during Stage 21 smoke validation: " + string.Join(" | ", RedErrors.ToArray()));

                Debug.Log("Stage 21 play mode smoke validation passed.");
            }
            finally
            {
                Application.logMessageReceived -= CaptureRedError;
                RedErrors.Clear();
            }
        }

        static void ValidateShowcaseRuntime()
        {
            var scene = EditorSceneManager.OpenScene(Stage21SceneCreator.ScenePath);
            if (!scene.IsValid())
                throw new InvalidOperationException("Stage 21 showcase scene did not open for smoke validation.");

            var showcase = RequireEnabled<Stage21MvpVisualQaShowcaseController>("Stage21MvpVisualQaShowcaseController");
            showcase.EnsureShowcase();
            if (showcase.DisplayedActorCount != Stage20MvpVisualActorSet.ActorTypeIds.Length)
                throw new InvalidOperationException("Stage 21 showcase runtime proxy count mismatch.");
            if (showcase.FailCount > 0)
                throw new InvalidOperationException("Stage 21 showcase runtime reports failing QA actors.");
        }

        static void ValidateBootRuntime()
        {
            var scene = EditorSceneManager.OpenScene(Stage16_5BuildFlowConfigurator.BootScenePath);
            if (!scene.IsValid())
                throw new InvalidOperationException("Stage 21 boot scene did not open for smoke validation.");

            var menu = RequireEnabled<MainMenuHud>("MainMenuHud");
            RequireEnabled<GameBootController>("GameBootController");
            if (!menu.visible)
                throw new InvalidOperationException("Stage 21 boot menu is not visible.");
        }

        static void ValidateStage16RuntimeVisuals()
        {
            var scene = EditorSceneManager.OpenScene(Stage16SceneCreator.ScenePath);
            if (!scene.IsValid())
                throw new InvalidOperationException("Stage 16 scene did not open for Stage 21 runtime smoke.");

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

            if (boardRenderer.FineGridLineCount <= 0)
                throw new InvalidOperationException("Stage 21 smoke: fine grid is not visible/readable.");
            if (!mode.IsPcSidebarVisibleForDesktop())
                throw new InvalidOperationException("Stage 21 smoke: PCDesktop right sidebar is not visible.");
            if (!mode.AreXrBuildMenusHiddenForPc())
                throw new InvalidOperationException("Stage 21 smoke: PCDesktop left-hand menus are not hidden.");
            if (!debugVisibility.AreDebugPanelsHiddenByDefault())
                throw new InvalidOperationException("Stage 21 smoke: debug panels are not hidden by default.");
            if (placementPanel.gameObject.activeInHierarchy)
                throw new InvalidOperationException("Stage 21 smoke: placement UI is visible before placement mode.");

            ValidateRuntimeMvpActorViews(driver, actorRenderer);
            ValidateSelectionAndHealthMarkers(driver, boardRenderer, actorRenderer);
            ValidatePlacementPreview(driver, boardRenderer, actorRenderer);
            Stage20SceneValidator.ValidateStage16UiModes();
        }

        static void ValidateRuntimeMvpActorViews(RtsSimulationDriver driver, ActorRenderSystem actorRenderer)
        {
            var snapshot = RequireSnapshot(driver);
            var checkedCount = 0;
            for (var i = 0; i < snapshot.Actors.Count; i++)
            {
                var actor = snapshot.Actors[i];
                if (!Stage20MvpVisualActorSet.Contains(actor.TypeId))
                    continue;

                ActorViewBehaviour view;
                if (!actorRenderer.TryGetActorView(actor.ActorId, out view) || view == null)
                    throw new InvalidOperationException("Stage 21 smoke missing actor view for " + actor.TypeId);
                if (!Stage20ProductionVisualValidator.IsMvpDefinitionUsingProductionProxy(view.ActiveVisualDefinition))
                    throw new InvalidOperationException("Stage 21 smoke actor view did not use production proxy for " + actor.TypeId);
                if (view.ActivePrefabDescriptor == null)
                    throw new InvalidOperationException("Stage 21 smoke actor view missing ActivePrefabDescriptor for " + actor.TypeId);
                if (view.ActivePrefabDescriptor.GetComponentInChildren<ProductionVisualValidationTag>(true) == null)
                    throw new InvalidOperationException("Stage 21 smoke actor view missing production validation tag for " + actor.TypeId);
                checkedCount++;
            }

            if (checkedCount == 0)
                throw new InvalidOperationException("Stage 21 smoke did not encounter MVP actors in Stage16 runtime.");
        }

        static void ValidateSelectionAndHealthMarkers(RtsSimulationDriver driver, BoardRenderer boardRenderer, ActorRenderSystem actorRenderer)
        {
            RequireSuccess(driver.TrySelectFirstOwnedActorOfType("light_tank"), "select light tank");
            StepRuntime(driver, boardRenderer, actorRenderer, 2, 0.05f);

            var selected = driver.SelectedActorIds.Count > 0 ? driver.SelectedActorIds[0] : -1;
            ActorViewBehaviour view;
            if (selected < 0 || !actorRenderer.TryGetActorView(selected, out view) || view == null)
                throw new InvalidOperationException("Stage 21 smoke could not find selected actor view.");
            if (view.transform.Find("Selection Marker") == null)
                throw new InvalidOperationException("Stage 21 smoke: selection ring marker is missing.");
            if (view.transform.Find("Health Bar") == null)
                throw new InvalidOperationException("Stage 21 smoke: health bar marker is missing.");
        }

        static void ValidatePlacementPreview(RtsSimulationDriver driver, BoardRenderer boardRenderer, ActorRenderSystem actorRenderer)
        {
            RequireSuccess(driver.TryQueueProduction("power_plant"), "queue power plant");
            StepUntilPendingPlacement(driver, boardRenderer, actorRenderer, "power_plant");
            RequireSuccess(driver.TryEnterPlacementModeForFirstPending(), "enter placement mode");
            var validCell = FindValidPlacementCell(driver, boardRenderer, actorRenderer);
            driver.SetHoveredCell(validCell, true);
            StepRuntime(driver, boardRenderer, actorRenderer, 1, 0.05f);
            PlacementPreviewSnapshot preview;
            if (!driver.TryGetPlacementPreview(out preview) || !preview.CanPlace || preview.FootprintCells.Count == 0)
                throw new InvalidOperationException("Stage 21 smoke: building placement preview is broken.");
            boardRenderer.UpdatePlacementPreview(preview);
            RequireSuccess(driver.TryCancelPlacement(), "cancel placement");
        }

        static void StepUntilPendingPlacement(RtsSimulationDriver driver, BoardRenderer boardRenderer, ActorRenderSystem actorRenderer, string typeId)
        {
            for (var i = 0; i < 500; i++)
            {
                if (HasPendingPlacement(driver, typeId))
                    return;
                StepRuntime(driver, boardRenderer, actorRenderer, 1, 0.1f);
            }

            throw new InvalidOperationException("Timed out waiting for pending placement: " + typeId);
        }

        static bool HasPendingPlacement(RtsSimulationDriver driver, string typeId)
        {
            var player = driver.GetLocalPlayerSnapshot();
            if (player == null)
                return false;

            for (var i = 0; i < player.Production.Count; i++)
                if (player.Production[i].TypeId == typeId && player.Production[i].State == "CompletedPendingPlacement")
                    return true;
            return false;
        }

        static Int2 FindValidPlacementCell(RtsSimulationDriver driver, BoardRenderer boardRenderer, ActorRenderSystem actorRenderer)
        {
            var snapshot = RequireSnapshot(driver);
            for (var y = 0; y < snapshot.Map.PlacementHeight; y++)
            {
                for (var x = 0; x < snapshot.Map.PlacementWidth; x++)
                {
                    var candidate = new Int2(x, y);
                    driver.SetHoveredCell(candidate, true);
                    StepRuntime(driver, boardRenderer, actorRenderer, 1, 0.02f);
                    PlacementPreviewSnapshot preview;
                    if (driver.TryGetPlacementPreview(out preview) && preview.CanPlace)
                        return candidate;
                }
            }

            throw new InvalidOperationException("No valid Stage 21 placement cell was found.");
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

        static WorldSnapshot RequireSnapshot(RtsSimulationDriver driver)
        {
            if (driver.LatestSnapshot == null)
                throw new InvalidOperationException("Stage 21 expected a simulation snapshot.");
            return driver.LatestSnapshot;
        }

        static void RequireSuccess(RtsCommandResult result, string label)
        {
            if (result == null || !result.Success)
                throw new InvalidOperationException("Stage 21 expected success for " + label + ": " + (result == null ? "null" : result.ToString()));
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
