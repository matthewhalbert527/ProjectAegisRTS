using System;
using System.Collections.Generic;
using ProjectAegisRTS.Core;
using ProjectAegisRTS.Data;
using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.UnityClient.Board;
using ProjectAegisRTS.UnityClient.Bootstrap;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.InputControls.XR;
using ProjectAegisRTS.UnityClient.Rendering;
using ProjectAegisRTS.UnityClient.UI.XR.LeftHand;
using ProjectAegisRTS.UnityClient.UI.XR.RightHand;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage5PlayModeSmokeValidator
    {
        static readonly List<string> RedErrors = new List<string>();

        public static void RunStage5PlayModeSmokeBatch()
        {
            try
            {
                RunStage5PlayModeSmoke();
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

        public static void RunStage5PlayModeSmoke()
        {
            RedErrors.Clear();
            Application.logMessageReceived += CaptureRedError;
            try
            {
                Stage5SceneValidator.ValidateStage5Scene();
                var scene = EditorSceneManager.OpenScene(Stage5SceneCreator.ScenePath);
                if (!scene.IsValid())
                    throw new InvalidOperationException("Stage 5 scene did not open.");

                var bootstrapper = RequireEnabled<RtsGameBootstrapper>("RtsGameBootstrapper");
                var driver = RequireEnabled<RtsSimulationDriver>("RtsSimulationDriver");
                var boardRenderer = RequireEnabled<BoardRenderer>("BoardRenderer");
                var actorRenderer = RequireEnabled<ActorRenderSystem>("ActorRenderSystem");
                var mapper = RequireEnabled<BoardCoordinateMapper>("BoardCoordinateMapper");
                var leftMenu = RequireEnabled<LeftHandBuildMenuController>("LeftHandBuildMenuController");
                var rightRig = RequireEnabled<SimulatedRightHandRig>("SimulatedRightHandRig");
                var preview = RequireEnabled<CommandPreviewRenderer>("CommandPreviewRenderer");
                var router = RequireEnabled<RightHandCommandRouter>("RightHandCommandRouter");
                var coordinator = RequireEnabled<Stage5DualHandModeCoordinator>("Stage5DualHandModeCoordinator");
                var boardPlacement = RequireEnabled<BoardPlacementController>("BoardPlacementController");

                bootstrapper.InitializeScene();
                rightRig.EnsureRig();
                preview.Initialize(mapper);
                coordinator.InitializeIfNeeded();
                StepRuntime(driver, boardRenderer, actorRenderer, 8, 0.1f);

                var actorViews = GameObject.Find("Actor Views");
                if (actorViews == null || actorViews.transform.childCount == 0)
                    throw new InvalidOperationException("Stage 5 actor visuals were not generated.");

                leftMenu.OpenMenu();
                if (!leftMenu.IsOpen || leftMenu.GetActiveCategoryItems().Count == 0)
                    throw new InvalidOperationException("Stage 5 left-hand build menu did not remain functional.");

                var mobile = FindMobileActor(driver);
                var selectResult = driver.SetSelectedActorIds(new[] { mobile.ActorId });
                if (!selectResult.Success || driver.SelectedActorIds.Count == 0)
                    throw new InvalidOperationException("Stage 5 could not select a mobile actor: " + selectResult);

                router.EnterMoveMode();
                var move = router.IssueMoveToCell(new Int2(16, 10));
                if (!move.Success)
                    throw new InvalidOperationException("Stage 5 move command failed: " + move);
                if (!preview.HasPreview || preview.PreviewKind != "move")
                    throw new InvalidOperationException("Stage 5 move preview marker did not appear.");

                router.EnterAttackMode();
                var attack = router.IssueAttackPlaceholderAtCell(new Int2(12, 10), false);
                if (!attack.Success || !preview.HasPreview || preview.PreviewKind != "attack")
                    throw new InvalidOperationException("Stage 5 attack placeholder failed: " + attack);

                router.EnterForceAttackMode();
                var force = router.IssueAttackPlaceholderAtCell(new Int2(13, 10), true);
                if (!force.Success)
                    throw new InvalidOperationException("Stage 5 force-attack placeholder failed: " + force);

                coordinator.SetMode(RightHandCommandMode.BoardManipulation);
                if (coordinator.CurrentMode != RightHandCommandMode.BoardManipulation)
                    throw new InvalidOperationException("Stage 5 board manipulation mode did not activate.");

                boardPlacement.SetPlacementMode(true);
                if (!coordinator.IsGameplaySuppressed())
                    throw new InvalidOperationException("Stage 5 did not suppress gameplay commands during board placement.");
                boardPlacement.CancelPlacement();

                router.CancelCommandMode();
                if (router.CurrentMode != RightHandCommandMode.Idle)
                    throw new InvalidOperationException("Stage 5 command cancellation did not return to idle.");

                if (RedErrors.Count > 0)
                    throw new InvalidOperationException("Red console errors were produced during Stage 5 smoke validation: " + string.Join(" | ", RedErrors.ToArray()));

                Debug.Log("Stage 5 play mode smoke validation passed.");
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

            throw new InvalidOperationException("Stage 5 smoke could not find a mobile unit actor.");
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
