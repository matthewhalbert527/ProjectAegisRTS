using System;
using ProjectAegisRTS.UnityClient.Art;
using ProjectAegisRTS.UnityClient.Art.Production;
using ProjectAegisRTS.UnityClient.UI.Common;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage21SceneValidator
    {
        public static void ValidateStage21SceneBatch()
        {
            try
            {
                ValidateStage21Scene();
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

        public static void ValidateStage21Scene()
        {
            Stage21MvpVisualQaValidator.ValidateMvpVisualQa();
            Stage21MvpSocketPivotScaleValidator.ValidateMvpSocketPivotScale();

            if (!System.IO.File.Exists(Stage21SceneCreator.ScenePath))
                throw new InvalidOperationException("Stage 21 scene is missing: " + Stage21SceneCreator.ScenePath);

            var scene = EditorSceneManager.OpenScene(Stage21SceneCreator.ScenePath);
            if (!scene.IsValid())
                throw new InvalidOperationException("Stage 21 scene did not open.");

            RequireObject("RtsGame");
            RequireObject("Main Camera");
            RequireObject("Side Inspection Camera");
            RequireObject("Directional Light");
            RequireObject("EventSystem");
            RequireObject("Canvas");
            RequireObject("Stage21 MVP Visual QA Showcase");

            var definitions = RequireEnabled<ActorVisualDefinitionLibrary>("ActorVisualDefinitionLibrary");
            var standards = RequireEnabled<ProductionVisualStandardLibrary>("ProductionVisualStandardLibrary");
            var resolver = RequireEnabled<ActorVisualPrefabResolver>("ActorVisualPrefabResolver");
            var runner = RequireEnabled<MvpVisualQaRunner>("MvpVisualQaRunner");
            var showcase = RequireEnabled<Stage21MvpVisualQaShowcaseController>("Stage21MvpVisualQaShowcaseController");
            var hud = RequireEnabled<MvpVisualQaHud>("MvpVisualQaHud");
            RequireEnabled<EventSystem>("EventSystem");
            RequireEnabled<Canvas>("Canvas");

            definitions.EnsureInitialized();
            standards.EnsureInitialized();
            if (definitions.DefinitionCount != Stage8ActorCatalog.ExpectedActorCount)
                throw new InvalidOperationException("Stage 21 visual definition count mismatch: " + definitions.DefinitionCount);
            if (standards.StandardCount != Stage20MvpVisualActorSet.ActorTypeIds.Length)
                throw new InvalidOperationException("Stage 21 standard library count mismatch: " + standards.StandardCount);
            if (resolver.definitionLibrary == null)
                throw new InvalidOperationException("Stage 21 resolver is not linked to ActorVisualDefinitionLibrary.");

            runner.RunAll();
            if (runner.FailCount > 0)
                throw new InvalidOperationException("Stage 21 scene runner reports failing QA actors.");

            showcase.EnsureShowcase();
            if (showcase.DisplayedActorCount != Stage20MvpVisualActorSet.ActorTypeIds.Length)
                throw new InvalidOperationException("Stage 21 showcase did not display every MVP proxy.");
            if (showcase.FailCount > 0)
                throw new InvalidOperationException("Stage 21 showcase reports failing MVP visual QA.");
            if (!hud.visible)
                throw new InvalidOperationException("Stage 21 QA HUD must be visible in the review scene.");

            var camera = RequireObject("Main Camera").GetComponent<Camera>();
            if (camera == null || !camera.enabled)
                throw new InvalidOperationException("Stage 21 main camera is missing or disabled.");
            if (!camera.orthographic)
                throw new InvalidOperationException("Stage 21 main camera must be orthographic.");
            if (camera.nearClipPlane > 0.11f || camera.farClipPlane < 999f)
                throw new InvalidOperationException("Stage 21 camera clipping planes are not safe for showcase view.");

            ValidateBuildSettings();
            Stage20SceneValidator.ValidateStage16UiModes();
            Debug.Log("Stage 21 scene validation passed.");
        }

        static void ValidateBuildSettings()
        {
            var scenes = EditorBuildSettings.scenes;
            if (scenes.Length < 2 || !scenes[0].enabled || scenes[0].path != Stage16_5BuildFlowConfigurator.BootScenePath)
                throw new InvalidOperationException("Stage 21 requires the boot scene first in Build Settings.");
            if (!scenes[1].enabled || scenes[1].path != Stage16SceneCreator.ScenePath)
                throw new InvalidOperationException("Stage 21 requires Stage16 as the second Build Settings scene.");
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
    }
}
