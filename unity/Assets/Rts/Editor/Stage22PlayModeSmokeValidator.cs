using System;
using System.Collections.Generic;
using ProjectAegisRTS.Core;
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
    public static class Stage22PlayModeSmokeValidator
    {
        static readonly List<string> RedErrors = new List<string>();

        public static void RunStage22PlayModeSmokeBatch()
        {
            try
            {
                RunStage22PlayModeSmoke();
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

        public static void RunStage22PlayModeSmoke()
        {
            RedErrors.Clear();
            Application.logMessageReceived += CaptureRedError;
            try
            {
                Stage22CommandMatrixValidator.ValidateStage22CommandMatrix();
                ValidateStage16RuntimeCommands();

                if (RedErrors.Count > 0)
                    throw new InvalidOperationException("Red console errors were produced during Stage 22 smoke validation: " + string.Join(" | ", RedErrors.ToArray()));

                Debug.Log("Stage 22 play mode smoke validation passed.");
            }
            finally
            {
                Application.logMessageReceived -= CaptureRedError;
                RedErrors.Clear();
            }
        }

        static void ValidateStage16RuntimeCommands()
        {
            var scene = EditorSceneManager.OpenScene(Stage16SceneCreator.ScenePath);
            if (!scene.IsValid())
                throw new InvalidOperationException("Stage 22 Stage16 scene did not open for runtime command smoke.");

            var bootstrapper = Require<RtsGameBootstrapper>("RtsGameBootstrapper");
            var driver = Require<RtsSimulationDriver>("RtsSimulationDriver");
            var boardRenderer = Require<BoardRenderer>("BoardRenderer");
            var actorRenderer = Require<ActorRenderSystem>("ActorRenderSystem");
            var desktopHud = Require<DesktopRtsHudRoot>("DesktopRtsHudRoot");
            var router = Require<DesktopUiCommandRouter>("DesktopUiCommandRouter");
            var layout = Require<CncStyleSidebarLayout>("CncStyleSidebarLayout");
            var placementPanel = Require<PlacementModePanel>("PlacementModePanel");
            var debugVisibility = Require<DebugHudVisibilityController>("DebugHudVisibilityController");

            bootstrapper.InitializeScene();
            desktopHud.Initialize();
            layout.ApplyLayout();
            StepRuntime(driver, boardRenderer, actorRenderer, 4, 0.05f);

            if (!debugVisibility.AreDebugPanelsHiddenByDefault())
                throw new InvalidOperationException("Stage 22 debug panels became visible during smoke validation.");
            if (placementPanel.gameObject.activeInHierarchy)
                throw new InvalidOperationException("Stage 22 placement UI became visible during command smoke.");

            var select = driver.TrySelectOwnedCombatGroup();
            RequireSuccess(select, "select owned mobile combat group");
            var selectedActorId = driver.SelectedActorIds[0];
            RequireSuccess(driver.SetSelectedActorIds(new[] { selectedActorId }), "narrow command smoke selection");
            var actor = FindSnapshotActor(driver, selectedActorId);
            var commandCell = actor.CellPosition;

            router.SetAttackMoveMode();
            if (router.CurrentMode != DesktopCommandMode.AttackMove)
                throw new InvalidOperationException("Stage 22 router did not enter attack-move mode.");
            RequireSuccess(router.IssueAttackMoveToCell(commandCell), "attack move");

            RequireSuccess(router.GuardSelected(), "guard");

            router.SetPatrolMode();
            if (router.CurrentMode != DesktopCommandMode.Patrol)
                throw new InvalidOperationException("Stage 22 router did not enter patrol mode.");
            RequireSuccess(router.IssuePatrolToCell(commandCell), "patrol");

            RequireSuccess(router.ScatterSelected(), "scatter");
            RequireSuccess(router.DeploySelected(), "deploy");
            RequireSuccess(router.StopSelected(), "stop");

            StepRuntime(driver, boardRenderer, actorRenderer, 2, 0.05f);
        }

        static ProjectAegisRTS.Snapshots.ActorSnapshot FindSnapshotActor(RtsSimulationDriver driver, int actorId)
        {
            ProjectAegisRTS.Snapshots.ActorSnapshot actor;
            if (!driver.TryGetActorSnapshot(actorId, out actor))
                throw new InvalidOperationException("Stage 22 selected actor was not present in the snapshot.");
            return actor;
        }

        static void RequireSuccess(RtsCommandResult result, string label)
        {
            if (result == null || !result.Success)
                throw new InvalidOperationException("Stage 22 command failed during smoke validation: " + label + " -> " + (result == null ? "null" : result.ToString()));
        }

        static void StepRuntime(RtsSimulationDriver driver, BoardRenderer boardRenderer, ActorRenderSystem actorRenderer, int frames, float deltaTime)
        {
            for (var i = 0; i < frames; i++)
            {
                driver.ManualUpdate(deltaTime);
                boardRenderer.UpdateHover(driver.HasHoveredCell ? (Int2?)driver.HoveredCell : null, driver.HoveredCellIsPlacementCell);
                ProjectAegisRTS.Snapshots.PlacementPreviewSnapshot preview;
                boardRenderer.UpdatePlacementPreview(driver.TryGetPlacementPreview(out preview) ? preview : null);
                actorRenderer.RenderSnapshot(driver.LatestSnapshot, driver.SelectedActorIds, deltaTime);
            }
        }

        static T Require<T>(string label) where T : Component
        {
            var active = UnityEngine.Object.FindFirstObjectByType<T>();
            if (active != null)
                return active;

            var all = Resources.FindObjectsOfTypeAll<T>();
            for (var i = 0; i < all.Length; i++)
                if (all[i] != null && all[i].gameObject != null && all[i].gameObject.scene.IsValid())
                    return all[i];

            throw new InvalidOperationException("Missing component: " + label);
        }

        static void CaptureRedError(string condition, string stackTrace, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
                RedErrors.Add(condition);
        }
    }
}
