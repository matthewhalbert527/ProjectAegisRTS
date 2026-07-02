using System;
using System.Collections.Generic;
using ProjectAegisRTS.Core;
using ProjectAegisRTS.Snapshots;
using ProjectAegisRTS.UnityClient.Bootstrap;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Rendering;
using ProjectAegisRTS.UnityClient.UI.Common;
using ProjectAegisRTS.UnityClient.UI.Desktop;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage24PlayModeSmokeValidator
    {
        static readonly List<string> RedErrors = new List<string>();

        public static void RunStage24PlayModeSmokeBatch()
        {
            try
            {
                RunStage24PlayModeSmoke();
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

        public static void RunStage24PlayModeSmoke()
        {
            RedErrors.Clear();
            Application.logMessageReceived += CaptureRedError;
            try
            {
                Stage24TechSupportValidator.ValidateStage24TechSupport();
                ValidateStage16RuntimeSupportPowers();

                if (RedErrors.Count > 0)
                    throw new InvalidOperationException("Red console errors were produced during Stage 24 smoke validation: " + string.Join(" | ", RedErrors.ToArray()));

                Debug.Log("Stage 24 play mode smoke validation passed.");
            }
            finally
            {
                Application.logMessageReceived -= CaptureRedError;
                RedErrors.Clear();
            }
        }

        static void ValidateStage16RuntimeSupportPowers()
        {
            var scene = EditorSceneManager.OpenScene(Stage16SceneCreator.ScenePath);
            if (!scene.IsValid())
                throw new InvalidOperationException("Stage 24 Stage16 scene did not open for runtime support-power smoke.");

            var bootstrapper = Require<RtsGameBootstrapper>("RtsGameBootstrapper");
            var driver = Require<RtsSimulationDriver>("RtsSimulationDriver");
            var boardRenderer = Require<BoardRenderer>("BoardRenderer");
            var actorRenderer = Require<ActorRenderSystem>("ActorRenderSystem");
            var desktopHud = Require<DesktopRtsHudRoot>("DesktopRtsHudRoot");
            var layout = Require<CncStyleSidebarLayout>("CncStyleSidebarLayout");
            var supportPanel = Require<SupportPowerPanelController>("SupportPowerPanelController");
            var debugVisibility = Require<DebugHudVisibilityController>("DebugHudVisibilityController");

            bootstrapper.InitializeScene();
            desktopHud.Initialize();
            layout.ApplyLayout();
            StepRuntime(driver, boardRenderer, actorRenderer, 4, 0.05f);

            if (!debugVisibility.AreDebugPanelsHiddenByDefault())
                throw new InvalidOperationException("Stage 24 debug panels became visible during smoke validation.");
            if (supportPanel.GetComponentsInChildren<Button>(true).Length < 5)
                throw new InvalidOperationException("Stage 24 support panel lost its buttons during runtime smoke.");

            var before = FindSupportPower(FindPlayer(driver, 1), "reveal_scan");
            if (before == null || !before.IsReady)
                throw new InvalidOperationException("Stage 24 Reveal Scan was not ready before smoke activation.");

            var target = new Int2(25, 25);
            RequireSuccess(driver.TryActivateSupportPowerAtCell("reveal_scan", target), "activate Reveal Scan");
            StepRuntime(driver, boardRenderer, actorRenderer, 1, 0.05f);

            var after = FindSupportPower(FindPlayer(driver, 1), "reveal_scan");
            if (after == null || after.ActivationCount <= before.ActivationCount || after.CooldownRemainingTicks <= 0)
                throw new InvalidOperationException("Stage 24 Reveal Scan did not update support-power snapshot cooldown/activation.");

            var cooldown = driver.TryActivateSupportPowerAtCell("reveal_scan", new Int2(20, 20));
            if (cooldown.Success || cooldown.Code != "SupportPowerCooldown")
                throw new InvalidOperationException("Stage 24 Reveal Scan should reject repeated activation during cooldown.");

            var lockedStrike = driver.TryActivateSupportPowerAtCell("precision_strike", new Int2(20, 20));
            if (lockedStrike.Success || lockedStrike.Code != "MissingPrerequisite")
                throw new InvalidOperationException("Stage 24 precision strike placeholder should remain gated by tech_center.");

            StepRuntime(driver, boardRenderer, actorRenderer, 2, 0.05f);
        }

        static void StepRuntime(RtsSimulationDriver driver, BoardRenderer boardRenderer, ActorRenderSystem actorRenderer, int steps, float deltaSeconds)
        {
            for (var i = 0; i < steps; i++)
            {
                driver.ManualUpdate(deltaSeconds);
                boardRenderer.UpdateHover(driver.HasHoveredCell ? (Int2?)driver.HoveredCell : null, driver.HoveredCellIsPlacementCell);
                PlacementPreviewSnapshot preview;
                boardRenderer.UpdatePlacementPreview(driver.TryGetPlacementPreview(out preview) ? preview : null);
                actorRenderer.RenderSnapshot(driver.LatestSnapshot, driver.SelectedActorIds, deltaSeconds);
            }
        }

        static PlayerSnapshot FindPlayer(RtsSimulationDriver driver, int playerId)
        {
            var snapshot = RequireSnapshot(driver);
            for (var i = 0; i < snapshot.Players.Count; i++)
                if (snapshot.Players[i].PlayerId == playerId)
                    return snapshot.Players[i];

            throw new InvalidOperationException("Stage 24 player snapshot not found: " + playerId);
        }

        static SupportPowerSnapshot FindSupportPower(PlayerSnapshot player, string powerId)
        {
            if (player == null)
                return null;

            for (var i = 0; i < player.SupportPowers.Count; i++)
                if (player.SupportPowers[i].PowerId == powerId)
                    return player.SupportPowers[i];

            return null;
        }

        static WorldSnapshot RequireSnapshot(RtsSimulationDriver driver)
        {
            if (driver == null || driver.LatestSnapshot == null)
                throw new InvalidOperationException("Stage 24 expected a current runtime snapshot.");
            return driver.LatestSnapshot;
        }

        static void RequireSuccess(RtsCommandResult result, string action)
        {
            if (result == null || !result.Success)
                throw new InvalidOperationException("Stage 24 failed to " + action + ": " + (result == null ? "no result" : result.ToString()));
        }

        static void CaptureRedError(string condition, string stackTrace, LogType type)
        {
            if (type == LogType.Exception || type == LogType.Error || type == LogType.Assert)
                RedErrors.Add(condition);
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
    }
}
