using System;
using ProjectAegisRTS.UnityClient.Bootstrap;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Rendering;
using ProjectAegisRTS.UnityClient.Rendering.Ai;
using ProjectAegisRTS.UnityClient.UI.Common;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage12SceneValidator
    {
        public static void ValidateStage12SceneBatch()
        {
            try
            {
                ValidateStage12Scene();
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

        public static void ValidateStage12Scene()
        {
            if (!System.IO.File.Exists(Stage12SceneCreator.ScenePath))
                throw new InvalidOperationException("Stage 12 scene missing: " + Stage12SceneCreator.ScenePath);

            var scene = EditorSceneManager.OpenScene(Stage12SceneCreator.ScenePath);
            if (!scene.IsValid())
                throw new InvalidOperationException("Stage 12 scene did not open.");

            RequireObject("RtsGame");
            RequireObject("BoardRoot");
            if (Camera.main == null && UnityEngine.Object.FindFirstObjectByType<Camera>() == null)
                throw new InvalidOperationException("Stage 12 camera missing.");
            if (UnityEngine.Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
                throw new InvalidOperationException("Stage 12 EventSystem missing.");
            if (UnityEngine.Object.FindFirstObjectByType<Canvas>() == null)
                throw new InvalidOperationException("Stage 12 Canvas missing.");

            Require<RtsGameBootstrapper>("RtsGameBootstrapper");
            Require<BoardRenderer>("BoardRenderer");
            Require<ActorRenderSystem>("ActorRenderSystem");
            var driver = Require<RtsSimulationDriver>("RtsSimulationDriver");
            Require<AiIntentRenderSystem>("AiIntentRenderSystem");
            Require<AiPlanTimelineView>("AiPlanTimelineView");
            Require<AiDebugHud>("AiDebugHud");

            if (!driver.UseAiSkirmishDemoWorld)
                throw new InvalidOperationException("Stage 12 scene must use the AI skirmish demo world.");
            if (!System.IO.File.Exists(Stage11SceneCreator.ScenePath))
                throw new InvalidOperationException("Previous stage scene missing: " + Stage11SceneCreator.ScenePath);

            Debug.Log("Stage 12 scene validation passed.");
        }

        static void RequireObject(string name)
        {
            if (GameObject.Find(name) == null)
                throw new InvalidOperationException("Missing GameObject: " + name);
        }

        static T Require<T>(string label) where T : Component
        {
            var component = UnityEngine.Object.FindFirstObjectByType<T>();
            if (component == null)
                throw new InvalidOperationException("Missing component: " + label);
            return component;
        }
    }
}
