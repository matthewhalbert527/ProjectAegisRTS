using System;
using System.Collections.Generic;
using ProjectAegisRTS.UnityClient.Boot;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Rendering;
using ProjectAegisRTS.UnityClient.Rendering.Economy;
using ProjectAegisRTS.UnityClient.UI.Common;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage17PlayModeSmokeValidator
    {
        static readonly List<string> RedErrors = new List<string>();

        public static void RunStage17PlayModeSmokeBatch()
        {
            try
            {
                RunStage17PlayModeSmoke();
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

        public static void RunStage17PlayModeSmoke()
        {
            RedErrors.Clear();
            Application.logMessageReceived += CaptureRedError;
            try
            {
                Stage17SceneValidator.ValidateStage17Scene();
                ValidateBootScreens();

                Stage16PlayModeSmokeValidator.RunStage16PlayModeSmoke();
                ValidatePlayerFacingRuntimeAfterStage16Smoke();

                if (RedErrors.Count > 0)
                    throw new InvalidOperationException("Red console errors were produced during Stage 17 smoke validation: " + string.Join(" | ", RedErrors.ToArray()));

                Debug.Log("Stage 17 play mode smoke validation passed.");
            }
            finally
            {
                Application.logMessageReceived -= CaptureRedError;
                RedErrors.Clear();
            }
        }

        static void ValidateBootScreens()
        {
            var bootScene = EditorSceneManager.OpenScene(Stage16_5BuildFlowConfigurator.BootScenePath);
            if (!bootScene.IsValid())
                throw new InvalidOperationException("Stage 17 boot scene did not open for smoke validation.");

            var controller = Require<GameBootController>("GameBootController");
            var menu = Require<MainMenuHud>("MainMenuHud");
            var controls = Require<ControlsHelpHud>("ControlsHelpHud");
            var options = Require<OptionsMenuHud>("OptionsMenuHud");

            if (!menu.visible || controls.visible || options.visible)
                throw new InvalidOperationException("Stage 17 boot menu default visibility is incorrect.");

            controller.ShowControls();
            if (menu.visible || !controls.visible || options.visible)
                throw new InvalidOperationException("Stage 17 controls screen did not open.");

            controller.ShowOptions();
            if (menu.visible || controls.visible || !options.visible)
                throw new InvalidOperationException("Stage 17 options screen did not open.");

            controller.ShowMainMenu();
            if (!menu.visible || controls.visible || options.visible)
                throw new InvalidOperationException("Stage 17 main menu did not restore.");
        }

        static void ValidatePlayerFacingRuntimeAfterStage16Smoke()
        {
            var boardRoot = GameObject.Find("BoardRoot");
            if (boardRoot == null || !boardRoot.activeInHierarchy || boardRoot.transform.childCount == 0)
                throw new InvalidOperationException("Stage 17 board visuals are missing after smoke validation.");

            var actorRenderer = Require<ActorRenderSystem>("ActorRenderSystem");
            if (actorRenderer.transform.childCount == 0)
                throw new InvalidOperationException("Stage 17 actor visuals are missing after smoke validation.");

            var resourceRenderer = Require<ResourceFieldRenderSystem>("ResourceFieldRenderSystem");
            if (resourceRenderer.transform.childCount == 0)
                throw new InvalidOperationException("Stage 17 resource visuals are missing after smoke validation.");

            var driver = Require<RtsSimulationDriver>("RtsSimulationDriver");
            var objective = Require<PlayerObjectiveHud>("PlayerObjectiveHud");
            var prompt = Require<PlayerPromptHud>("PlayerPromptHud");
            var controls = Require<PlayerControlsOverlay>("PlayerControlsOverlay");
            var result = Require<MatchResultHud>("MatchResultHud");
            var debugVisibility = Require<DebugHudVisibilityController>("DebugHudVisibilityController");

            if (driver.LatestSnapshot == null || driver.LatestSnapshot.Actors.Count == 0)
                throw new InvalidOperationException("Stage 17 runtime snapshot is missing actors.");
            if (!objective.visible || !prompt.visible)
                throw new InvalidOperationException("Stage 17 player HUD is hidden after smoke validation.");
            if (controls.visible)
                throw new InvalidOperationException("Stage 17 controls overlay is visible by default.");
            if (!result.HasResultToShow)
                throw new InvalidOperationException("Stage 17 match result HUD did not appear after the smoke validator triggered defeat.");
            if (!debugVisibility.AreDebugPanelsHiddenByDefault())
                throw new InvalidOperationException("Stage 17 debug panels are visible by default.");
            if (!debugVisibility.IsPlacementUiHiddenByDefault())
                throw new InvalidOperationException("Stage 17 placement UI is visible by default.");
            if (!debugVisibility.IsPlayerHudVisible())
                throw new InvalidOperationException("Stage 17 player HUD is not visible by default.");
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
