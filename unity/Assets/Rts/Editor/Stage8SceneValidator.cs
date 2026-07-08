using System;
using ProjectAegisRTS.UnityClient.Art;
using ProjectAegisRTS.UnityClient.Bootstrap;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Rendering;
using ProjectAegisRTS.UnityClient.UI.Common;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage8SceneValidator
    {
        public static void ValidateStage8SceneBatch()
        {
            try
            {
                ValidateStage8Scene();
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

        public static void ValidateStage8Scene()
        {
            if (!System.IO.File.Exists(Stage8SceneCreator.ScenePath))
                throw new InvalidOperationException("Stage 8 scene is missing: " + Stage8SceneCreator.ScenePath);

            EditorSceneManager.OpenScene(Stage8SceneCreator.ScenePath);
            RequireObject("RtsGame");
            RequireObject("BoardRoot");
            RequireObject("Main Camera");
            RequireObject("Directional Light");
            RequireObject("EventSystem");
            RequireObject("Canvas");
            RequireObject("Stage8 Art Pipeline Showcase");

            RequireEnabled<RtsGameBootstrapper>("RtsGameBootstrapper");
            RequireEnabled<BoardRenderer>("BoardRenderer");
            var actorRenderer = RequireEnabled<ActorRenderSystem>("ActorRenderSystem");
            RequireEnabled<RtsStatusLog>("RtsStatusLog");
            var visualLibrary = RequireEnabled<ActorVisualDefinitionLibrary>("ActorVisualDefinitionLibrary");
            var resolver = RequireEnabled<ActorVisualPrefabResolver>("ActorVisualPrefabResolver");
            var conceptLibrary = RequireEnabled<ConceptArtReferenceLibrary>("ConceptArtReferenceLibrary");
            RequireEnabled<ArtPipelineDebugHud>("ArtPipelineDebugHud");
            var showcase = RequireEnabled<ArtPipelineShowcaseController>("ArtPipelineShowcaseController");
            RequireEnabled<EventSystem>("EventSystem");
            RequireEnabled<Canvas>("Canvas");

            if (actorRenderer.actorVisualDefinitionLibrary == null)
                throw new InvalidOperationException("ActorRenderSystem is not linked to ActorVisualDefinitionLibrary.");
            if (actorRenderer.actorVisualPrefabResolver == null)
                throw new InvalidOperationException("ActorRenderSystem is not linked to ActorVisualPrefabResolver.");
            if (resolver.definitionLibrary == null)
                throw new InvalidOperationException("ActorVisualPrefabResolver is not linked to the definition library.");

            visualLibrary.EnsureInitialized();
            conceptLibrary.EnsureInitialized();
            if (visualLibrary.DefinitionCount != Stage8ActorCatalog.ExpectedActorCount)
                throw new InvalidOperationException("Stage 8 visual definition count mismatch: " + visualLibrary.DefinitionCount);
            if (conceptLibrary.ReferenceCount != Stage8ActorCatalog.ExpectedActorCount)
                throw new InvalidOperationException("Stage 8 concept reference count mismatch: " + conceptLibrary.ReferenceCount);

            var validation = Stage8PrefabSocketValidator.ValidatePrefabs();
            if (validation.blockoutPrefabCount != Stage8ActorCatalog.ExpectedActorCount)
                throw new InvalidOperationException("Stage 8 blockout prefab count mismatch: " + validation.blockoutPrefabCount);
            if (validation.iconCount != Stage8ActorCatalog.ExpectedActorCount)
                throw new InvalidOperationException("Stage 8 icon count mismatch: " + validation.iconCount);

            RequireSceneFile("Assets/Rts/Scenes/Stage1_DesktopBoard.unity");
            RequireSceneFile(Stage2SceneCreator.ScenePath);
            RequireSceneFile(Stage3SceneCreator.ScenePath);
            RequireSceneFile(Stage4SceneCreator.ScenePath);
            RequireSceneFile(Stage5SceneCreator.ScenePath);
            RequireSceneFile(Stage6SceneCreator.ScenePath);
            RequireSceneFile(Stage7SceneCreator.ScenePath);

            var camera = RequireEnabled<Camera>("Main Camera");
            if (!camera.orthographic)
                throw new InvalidOperationException("Stage 8 camera must be orthographic.");
            if (Mathf.Abs(camera.orthographicSize - 28f) > 0.01f)
                throw new InvalidOperationException("Stage 8 camera orthographic size should be 28.");
            if (camera.nearClipPlane > 0.11f || camera.farClipPlane < 999f)
                throw new InvalidOperationException("Stage 8 camera clipping planes are not safe for board view.");

            showcase.EnsureShowcase();
            if (showcase.DefinitionCount != Stage8ActorCatalog.ExpectedActorCount)
                throw new InvalidOperationException("Stage 8 showcase did not enumerate all definitions.");

            Debug.Log("Stage 8 scene validation passed.");
        }

        static void RequireSceneFile(string scenePath)
        {
            if (!System.IO.File.Exists(scenePath))
                throw new InvalidOperationException("Required previous-stage scene is missing: " + scenePath);
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
