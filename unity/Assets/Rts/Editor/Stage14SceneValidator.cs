using System;
using ProjectAegisRTS.UnityClient.Bootstrap;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Feedback;
using ProjectAegisRTS.UnityClient.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage14SceneValidator
    {
        public static void ValidateStage14SceneBatch()
        {
            try
            {
                ValidateStage14Scene();
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

        public static void ValidateStage14Scene()
        {
            if (!System.IO.File.Exists(Stage14SceneCreator.ScenePath))
                throw new InvalidOperationException("Stage 14 scene missing: " + Stage14SceneCreator.ScenePath);

            var scene = EditorSceneManager.OpenScene(Stage14SceneCreator.ScenePath);
            if (!scene.IsValid())
                throw new InvalidOperationException("Stage 14 scene did not open.");

            RequireObject("RtsGame");
            RequireObject("BoardRoot");
            if (Camera.main == null && UnityEngine.Object.FindFirstObjectByType<Camera>() == null)
                throw new InvalidOperationException("Stage 14 camera missing.");
            if (UnityEngine.Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
                throw new InvalidOperationException("Stage 14 EventSystem missing.");
            if (UnityEngine.Object.FindFirstObjectByType<Canvas>() == null)
                throw new InvalidOperationException("Stage 14 Canvas missing.");

            Require<RtsGameBootstrapper>("RtsGameBootstrapper");
            Require<BoardRenderer>("BoardRenderer");
            Require<ActorRenderSystem>("ActorRenderSystem");
            var driver = Require<RtsSimulationDriver>("RtsSimulationDriver");
            var profileLibrary = Require<FeedbackProfileLibrary>("FeedbackProfileLibrary");
            Require<FeedbackEventBus>("FeedbackEventBus");
            Require<AudioFeedbackController>("AudioFeedbackController");
            Require<VfxFeedbackController>("VfxFeedbackController");
            Require<UiFeedbackController>("UiFeedbackController");
            Require<HapticFeedbackAdapter>("HapticFeedbackAdapter");
            Require<FeedbackDebugHud>("FeedbackDebugHud");

            profileLibrary.EnsureInitialized();
            if (profileLibrary.ProfileCount < Enum.GetValues(typeof(FeedbackEventType)).Length)
                throw new InvalidOperationException("Stage 14 feedback profile library does not include all event types.");
            if (!driver.UseCombatDemoWorld)
                throw new InvalidOperationException("Stage 14 scene should default to the combat demo world.");
            if (!System.IO.File.Exists(Stage13SceneCreator.ScenePath))
                throw new InvalidOperationException("Previous stage scene missing: " + Stage13SceneCreator.ScenePath);

            Debug.Log("Stage 14 scene validation passed.");
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
