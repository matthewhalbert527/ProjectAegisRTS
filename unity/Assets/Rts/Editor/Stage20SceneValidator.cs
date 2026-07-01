using System;
using ProjectAegisRTS.UnityClient.Art;
using ProjectAegisRTS.UnityClient.Art.Production;
using ProjectAegisRTS.UnityClient.UI.Common;
using ProjectAegisRTS.UnityClient.UI.Desktop;
using ProjectAegisRTS.UnityClient.UI.XR.LeftHand;
using ProjectAegisRTS.UnityClient.UI.XR.RightHand;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage20SceneValidator
    {
        public static void ValidateStage20SceneBatch()
        {
            try
            {
                ValidateStage20Scene();
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

        public static void ValidateStage20Scene()
        {
            Stage20ProductionVisualValidator.ValidateStage20ProductionVisuals();

            if (!System.IO.File.Exists(Stage20SceneCreator.ScenePath))
                throw new InvalidOperationException("Stage 20 scene is missing: " + Stage20SceneCreator.ScenePath);

            var scene = EditorSceneManager.OpenScene(Stage20SceneCreator.ScenePath);
            if (!scene.IsValid())
                throw new InvalidOperationException("Stage 20 scene did not open.");

            RequireObject("RtsGame");
            RequireObject("Main Camera");
            RequireObject("Directional Light");
            RequireObject("EventSystem");
            RequireObject("Canvas");
            RequireObject("Stage20 MVP Production Visual Showcase");

            var definitions = RequireEnabled<ActorVisualDefinitionLibrary>("ActorVisualDefinitionLibrary");
            var standards = RequireEnabled<ProductionVisualStandardLibrary>("ProductionVisualStandardLibrary");
            var resolver = RequireEnabled<ActorVisualPrefabResolver>("ActorVisualPrefabResolver");
            var showcase = RequireEnabled<Stage20MvpVisualShowcaseController>("Stage20MvpVisualShowcaseController");
            RequireEnabled<EventSystem>("EventSystem");
            RequireEnabled<Canvas>("Canvas");

            definitions.EnsureInitialized();
            standards.EnsureInitialized();
            if (definitions.DefinitionCount != Stage8ActorCatalog.ExpectedActorCount)
                throw new InvalidOperationException("Stage 20 visual definition count mismatch: " + definitions.DefinitionCount);
            if (standards.StandardCount != Stage20MvpVisualActorSet.ActorTypeIds.Length)
                throw new InvalidOperationException("Stage 20 standard library count mismatch: " + standards.StandardCount);
            if (resolver.definitionLibrary == null)
                throw new InvalidOperationException("Stage 20 resolver is not linked to ActorVisualDefinitionLibrary.");

            showcase.EnsureShowcase();
            if (showcase.MvpProxyCount != Stage20MvpVisualActorSet.ActorTypeIds.Length)
                throw new InvalidOperationException("Stage 20 showcase did not display every MVP proxy.");
            if (showcase.SocketValidatedCount != Stage20MvpVisualActorSet.ActorTypeIds.Length)
                throw new InvalidOperationException("Stage 20 showcase socket validation count mismatch.");
            if (showcase.ViewCoverageValidatedCount != Stage20MvpVisualActorSet.ActorTypeIds.Length)
                throw new InvalidOperationException("Stage 20 showcase view coverage count mismatch.");

            var camera = RequireEnabled<Camera>("Main Camera");
            if (!camera.orthographic)
                throw new InvalidOperationException("Stage 20 camera must be orthographic.");
            if (camera.nearClipPlane > 0.11f || camera.farClipPlane < 999f)
                throw new InvalidOperationException("Stage 20 camera clipping planes are not safe for showcase view.");

            ValidateStage16UiModes();
            Debug.Log("Stage 20 scene validation passed.");
        }

        public static void ValidateStage16UiModes()
        {
            if (!System.IO.File.Exists(Stage16SceneCreator.ScenePath))
                throw new InvalidOperationException("Stage 16 player-facing scene is missing: " + Stage16SceneCreator.ScenePath);

            var scene = EditorSceneManager.OpenScene(Stage16SceneCreator.ScenePath);
            if (!scene.IsValid())
                throw new InvalidOperationException("Stage 16 scene did not open for Stage 20 UI split validation.");

            var mode = RequireComponent<PlayerFacingUiModeController>("PlayerFacingUiModeController");
            var desktopHud = RequireComponent<DesktopRtsHudRoot>("DesktopRtsHudRoot");
            RequireComponent<LeftHandBuildMenuController>("LeftHandBuildMenuController");
            RequireComponent<LeftHandRadialMenuView>("LeftHandRadialMenuView");
            RequireComponent<LeftHandPlacementPanel>("LeftHandPlacementPanel");
            RequireComponent<LeftHandSelectionPanel>("LeftHandSelectionPanel");
            RequireComponent<RightHandCommandRouter>("RightHandCommandRouter");
            RequireComponent<RightHandCommandHud>("RightHandCommandHud");
            RequireComponent<RightHandStatusPanel>("RightHandStatusPanel");

            mode.desktopHud = desktopHud;
            mode.ApplyPcDesktopMode();
            if (!mode.WindowsPlayerDefaultsToPcDesktop())
                throw new InvalidOperationException("Stage 20 requires Windows player builds to default to PCDesktop mode.");
            if (!mode.IsPcSidebarVisibleForDesktop())
                throw new InvalidOperationException("Stage 20 PCDesktop mode did not show the right-side sidebar/minimap layout.");
            if (!mode.AreXrBuildMenusHiddenForPc())
                throw new InvalidOperationException("Stage 20 PCDesktop mode did not hide Quest fallback menus.");

            mode.ApplyQuestXrMode();
            if (!mode.AreQuestLeftHandControlsAvailable() || !mode.AreQuestControlsActiveForQuest())
                throw new InvalidOperationException("Stage 20 QuestXR mode did not preserve left-hand build/selection controls.");
            if (!mode.AreQuestRightHandControlsAvailable())
                throw new InvalidOperationException("Stage 20 QuestXR mode did not preserve right-hand tactical controls.");
            if (!mode.IsPcSidebarHiddenForQuest())
                throw new InvalidOperationException("Stage 20 QuestXR mode did not hide the PC right sidebar.");

            mode.ApplyPcDesktopMode();
        }

        static GameObject RequireObject(string objectName)
        {
            var obj = GameObject.Find(objectName);
            if (obj == null)
                throw new InvalidOperationException("Missing GameObject: " + objectName);
            return obj;
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
    }
}
