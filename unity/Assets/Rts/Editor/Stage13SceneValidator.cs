using System;
using ProjectAegisRTS.UnityClient.Bootstrap;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Rendering;
using ProjectAegisRTS.UnityClient.Rendering.Map;
using ProjectAegisRTS.UnityClient.UI.Common;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage13SceneValidator
    {
        public static void ValidateStage13SceneBatch()
        {
            try
            {
                ValidateStage13Scene();
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

        public static void ValidateStage13Scene()
        {
            if (!System.IO.File.Exists(Stage13SceneCreator.ScenePath))
                throw new InvalidOperationException("Stage 13 scene missing: " + Stage13SceneCreator.ScenePath);

            var scene = EditorSceneManager.OpenScene(Stage13SceneCreator.ScenePath);
            if (!scene.IsValid())
                throw new InvalidOperationException("Stage 13 scene did not open.");

            RequireObject("RtsGame");
            RequireObject("BoardRoot");
            if (Camera.main == null && UnityEngine.Object.FindFirstObjectByType<Camera>() == null)
                throw new InvalidOperationException("Stage 13 camera missing.");
            if (UnityEngine.Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
                throw new InvalidOperationException("Stage 13 EventSystem missing.");
            if (UnityEngine.Object.FindFirstObjectByType<Canvas>() == null)
                throw new InvalidOperationException("Stage 13 Canvas missing.");

            Require<RtsGameBootstrapper>("RtsGameBootstrapper");
            Require<BoardRenderer>("BoardRenderer");
            Require<ActorRenderSystem>("ActorRenderSystem");
            var driver = Require<RtsSimulationDriver>("RtsSimulationDriver");
            Require<TerrainDebugRenderer>("TerrainDebugRenderer");
            Require<PathDebugRenderer>("PathDebugRenderer");
            Require<MapAuthoringOverlay>("MapAuthoringOverlay");
            Require<MapValidationDebugHud>("MapValidationDebugHud");

            if (!driver.UseMapTerrainDemoWorld)
                throw new InvalidOperationException("Stage 13 scene must use the map terrain demo world.");
            if (!System.IO.File.Exists(Stage12SceneCreator.ScenePath))
                throw new InvalidOperationException("Previous stage scene missing: " + Stage12SceneCreator.ScenePath);

            Debug.Log("Stage 13 scene validation passed.");
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
