using System;
using ProjectAegisRTS.UnityClient.Bootstrap;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Rendering;
using ProjectAegisRTS.UnityClient.Rendering.Visibility;
using ProjectAegisRTS.UnityClient.UI.Common;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage11SceneValidator
    {
        public static void ValidateStage11SceneBatch()
        {
            try
            {
                ValidateStage11Scene();
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

        public static void ValidateStage11Scene()
        {
            if (!System.IO.File.Exists(Stage11SceneCreator.ScenePath))
                throw new InvalidOperationException("Stage 11 scene missing: " + Stage11SceneCreator.ScenePath);

            var scene = EditorSceneManager.OpenScene(Stage11SceneCreator.ScenePath);
            if (!scene.IsValid())
                throw new InvalidOperationException("Stage 11 scene did not open.");

            RequireObject("RtsGame");
            RequireObject("BoardRoot");
            if (Camera.main == null && UnityEngine.Object.FindFirstObjectByType<Camera>() == null)
                throw new InvalidOperationException("Stage 11 camera missing.");
            if (UnityEngine.Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
                throw new InvalidOperationException("Stage 11 EventSystem missing.");
            if (UnityEngine.Object.FindFirstObjectByType<Canvas>() == null)
                throw new InvalidOperationException("Stage 11 Canvas missing.");

            Require<RtsGameBootstrapper>("RtsGameBootstrapper");
            Require<BoardRenderer>("BoardRenderer");
            Require<ActorRenderSystem>("ActorRenderSystem");
            var driver = Require<RtsSimulationDriver>("RtsSimulationDriver");
            Require<FogOverlayRenderer>("FogOverlayRenderer");
            Require<VisibilityDebugRenderer>("VisibilityDebugRenderer");
            Require<RadarSnapshotAdapter>("RadarSnapshotAdapter");
            Require<MinimapRenderSystem>("MinimapRenderSystem");
            Require<FogDebugHud>("FogDebugHud");

            if (!driver.UseFogRadarDemoWorld)
                throw new InvalidOperationException("Stage 11 scene must use the fog/radar demo world.");
            if (!driver.UsePlayerPerspectiveSnapshot)
                throw new InvalidOperationException("Stage 11 scene must use player-perspective snapshots.");
            if (!System.IO.File.Exists(Stage10SceneCreator.ScenePath))
                throw new InvalidOperationException("Previous stage scene missing: " + Stage10SceneCreator.ScenePath);

            Debug.Log("Stage 11 scene validation passed.");
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
