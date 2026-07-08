using System;
using System.Collections.Generic;
using ProjectAegisRTS.UnityClient.Boot;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Rendering;
using ProjectAegisRTS.UnityClient.UI.Common;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage16_5PlayModeSmokeValidator
    {
        static readonly List<string> RedErrors = new List<string>();

        public static void RunBuildFlowSmokeBatch()
        {
            try
            {
                RunBuildFlowSmoke();
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

        public static void RunBuildFlowSmoke()
        {
            RedErrors.Clear();
            Application.logMessageReceived += CaptureRedError;
            try
            {
                Stage16_5BuildFlowValidator.ValidateBuildFlow();
                var bootScene = EditorSceneManager.OpenScene(Stage16_5BuildFlowConfigurator.BootScenePath);
                if (!bootScene.IsValid())
                    throw new InvalidOperationException("Stage 16.5 boot scene did not open for smoke validation.");

                var boot = Require<GameBootController>("GameBootController");
                var menu = Require<MainMenuHud>("MainMenuHud");
                var controls = Require<ControlsHelpHud>("ControlsHelpHud");
                var options = Require<OptionsMenuHud>("OptionsMenuHud");
                if (!menu.visible || controls.visible || options.visible)
                    throw new InvalidOperationException("Stage 16.5 boot menu/default controls visibility is incorrect.");
                if (boot.verticalSliceSceneName != "Stage16_PlayableVerticalSlice")
                    throw new InvalidOperationException("Stage 16.5 Start path does not target Stage16_PlayableVerticalSlice.");

                boot.ShowControls();
                if (menu.visible || !controls.visible || options.visible)
                    throw new InvalidOperationException("Stage 16.5 controls screen did not open cleanly.");
                boot.ShowOptions();
                if (menu.visible || controls.visible || !options.visible)
                    throw new InvalidOperationException("Stage 16.5 options screen did not open cleanly.");
                boot.ShowMainMenu();
                if (!menu.visible || controls.visible || options.visible)
                    throw new InvalidOperationException("Stage 16.5 main menu did not restore cleanly.");

                Stage16PlayModeSmokeValidator.RunStage16PlayModeSmoke();
                ValidateStage16PresentationAfterSmoke();

                if (RedErrors.Count > 0)
                    throw new InvalidOperationException("Red console errors were produced during Stage 16.5 build-flow smoke validation: " + string.Join(" | ", RedErrors.ToArray()));

                Debug.Log("Stage 16.5 build flow smoke validation passed.");
            }
            finally
            {
                Application.logMessageReceived -= CaptureRedError;
                RedErrors.Clear();
            }
        }

        static void ValidateStage16PresentationAfterSmoke()
        {
            var boardRoot = GameObject.Find("BoardRoot");
            if (boardRoot == null || !boardRoot.activeInHierarchy || boardRoot.transform.childCount == 0)
                throw new InvalidOperationException("Stage 16 board visuals were not generated.");

            var actorRenderer = Require<ActorRenderSystem>("ActorRenderSystem");
            if (actorRenderer.transform.childCount == 0)
                throw new InvalidOperationException("Stage 16 actor visuals were not generated.");

            var driver = Require<RtsSimulationDriver>("RtsSimulationDriver");
            if (driver.LatestSnapshot == null || driver.LatestSnapshot.Actors.Count == 0)
                throw new InvalidOperationException("Stage 16 runtime snapshot has no actors.");

            var objectiveHud = Require<MatchObjectiveHud>("MatchObjectiveHud");
            var playerObjectiveHud = Require<PlayerObjectiveHud>("PlayerObjectiveHud");
            var playerPromptHud = Require<PlayerPromptHud>("PlayerPromptHud");
            var controlsOverlay = Require<PlayerControlsOverlay>("PlayerControlsOverlay");
            var matchResultHud = Require<MatchResultHud>("MatchResultHud");
            var systemsHud = Require<IntegratedSystemsStatusHud>("IntegratedSystemsStatusHud");
            if (!objectiveHud.visible || !playerObjectiveHud.visible || !playerPromptHud.visible)
                throw new InvalidOperationException("Stage 16 player-facing objective HUD is hidden.");
            if (controlsOverlay.visible)
                throw new InvalidOperationException("Stage 16 controls overlay is visible by default.");
            if (!matchResultHud.visible)
                throw new InvalidOperationException("Stage 16 match result HUD is disabled.");
            if (objectiveHud.showDebugActions || systemsHud.visible)
                throw new InvalidOperationException("Stage 16 debug panels are visible by default.");
            if (driver.HasPlacementMode || AnyPlacementPanelActive())
                throw new InvalidOperationException("Stage 16 placement UI is visible by default.");
        }

        static bool AnyPlacementPanelActive()
        {
            var behaviours = Resources.FindObjectsOfTypeAll<MonoBehaviour>();
            for (var i = 0; i < behaviours.Length; i++)
            {
                var behaviour = behaviours[i];
                if (behaviour == null || behaviour.gameObject == null || !behaviour.gameObject.scene.IsValid())
                    continue;

                var name = behaviour.GetType().Name;
                if ((name == "BoardPlacementHud" || name == "PlacementModePanel" || name == "LeftHandPlacementPanel") && behaviour.gameObject.activeInHierarchy)
                    return true;
            }

            return false;
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
