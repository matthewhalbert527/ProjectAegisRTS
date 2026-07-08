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

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage27PlayModeSmokeValidator
    {
        static readonly List<string> RedErrors = new List<string>();

        public static void RunStage27PlayModeSmokeBatch()
        {
            try
            {
                RunStage27PlayModeSmoke();
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

        public static void RunStage27PlayModeSmoke()
        {
            RedErrors.Clear();
            Application.logMessageReceived += CaptureRedError;
            try
            {
                Stage27SkirmishPlayabilityValidator.ValidateStage27SkirmishPlayability();
                ValidateStage16RuntimeSkirmish();

                if (RedErrors.Count > 0)
                    throw new InvalidOperationException("Red console errors were produced during Stage 27 smoke validation: " + string.Join(" | ", RedErrors.ToArray()));

                Debug.Log("Stage 27 play mode smoke validation passed.");
            }
            finally
            {
                Application.logMessageReceived -= CaptureRedError;
                RedErrors.Clear();
            }
        }

        static void ValidateStage16RuntimeSkirmish()
        {
            var previousDifficulty = PlayerPrefs.GetString(RtsSimulationDriver.SkirmishDifficultyPlayerPrefsKey, "normal");
            try
            {
                PlayerPrefs.SetString(RtsSimulationDriver.SkirmishDifficultyPlayerPrefsKey, "hard");
                PlayerPrefs.Save();

                var scene = EditorSceneManager.OpenScene(Stage16SceneCreator.ScenePath);
                if (!scene.IsValid())
                    throw new InvalidOperationException("Stage 27 Stage16 scene did not open for runtime smoke.");

                var bootstrapper = Require<RtsGameBootstrapper>("RtsGameBootstrapper");
                var driver = Require<RtsSimulationDriver>("RtsSimulationDriver");
                var boardRenderer = Require<BoardRenderer>("BoardRenderer");
                var actorRenderer = Require<ActorRenderSystem>("ActorRenderSystem");
                var objectiveHud = Require<PlayerObjectiveHud>("PlayerObjectiveHud");
                var desktopHud = Require<DesktopRtsHudRoot>("DesktopRtsHudRoot");
                var layout = Require<CncStyleSidebarLayout>("CncStyleSidebarLayout");
                var debugVisibility = Require<DebugHudVisibilityController>("DebugHudVisibilityController");

                bootstrapper.InitializeScene();
                desktopHud.Initialize();
                layout.ApplyLayout();
                StepRuntime(driver, boardRenderer, actorRenderer, 4, 0.05f);

                var snapshot = RequireSnapshot(driver);
                var ai = FirstAi(snapshot);
                if (ai == null || ai.DifficultyId != "hard")
                    throw new InvalidOperationException("Stage 27 smoke did not load Hard difficulty from player preferences.");
                if (!objectiveHud.visible)
                    throw new InvalidOperationException("Stage 27 player-facing objective HUD became hidden.");
                if (!layout.AreProductionPanelsInRightSidebar())
                    throw new InvalidOperationException("Stage 27 right sidebar layout was not preserved during smoke.");
                if (!debugVisibility.AreDebugPanelsHiddenByDefault())
                    throw new InvalidOperationException("Stage 27 debug panels became visible during smoke validation.");

                StepRuntime(driver, boardRenderer, actorRenderer, 40, 0.1f);
                snapshot = RequireSnapshot(driver);
                ai = FirstAi(snapshot);
                if (ai == null || ai.AttackWaveSequence < 1 || !HasSuccessfulAiIntent(snapshot, "Attack", "BasicAttackWave"))
                    throw new InvalidOperationException("Stage 27 Hard AI did not issue a live attack wave during smoke validation.");

                var easyResult = driver.SetSkirmishDifficulty("easy", true);
                if (!easyResult.Success)
                    throw new InvalidOperationException("Stage 27 difficulty restart command failed: " + easyResult);
                driver.TryStartMatch();
                snapshot = RequireSnapshot(driver);
                ai = FirstAi(snapshot);
                if (ai == null || ai.DifficultyId != "easy")
                    throw new InvalidOperationException("Stage 27 restart did not apply Easy difficulty.");
            }
            finally
            {
                PlayerPrefs.SetString(RtsSimulationDriver.SkirmishDifficultyPlayerPrefsKey, previousDifficulty);
                PlayerPrefs.Save();
            }
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

        static WorldSnapshot RequireSnapshot(RtsSimulationDriver driver)
        {
            if (driver == null || driver.LatestSnapshot == null)
                throw new InvalidOperationException("Stage 27 expected a current runtime snapshot.");
            return driver.LatestSnapshot;
        }

        static AiPlayerSnapshot FirstAi(WorldSnapshot snapshot)
        {
            if (snapshot == null || snapshot.Ai == null || snapshot.Ai.Players.Count == 0)
                return null;
            return snapshot.Ai.Players[0];
        }

        static bool HasSuccessfulAiIntent(WorldSnapshot snapshot, string kind, string intentId)
        {
            var ai = FirstAi(snapshot);
            if (ai == null)
                return false;

            for (var i = 0; i < ai.RecentIntents.Count; i++)
            {
                var intent = ai.RecentIntents[i];
                if (intent.Kind == kind && intent.IntentId == intentId && intent.WasCommandIssued && intent.CommandSucceeded)
                    return true;
            }

            return false;
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
