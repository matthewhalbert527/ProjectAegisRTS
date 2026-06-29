using System;
using ProjectAegisRTS.UnityClient.Board;
using ProjectAegisRTS.UnityClient.Bootstrap;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.InputControls.Desktop;
using ProjectAegisRTS.UnityClient.InputControls.XR;
using ProjectAegisRTS.UnityClient.Rendering;
using ProjectAegisRTS.UnityClient.UI.XR;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage3SceneValidator
    {
        public static void ValidateStage3SceneBatch()
        {
            try
            {
                ValidateStage3Scene();
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

        public static void ValidateStage3Scene()
        {
            if (!System.IO.File.Exists(Stage3SceneCreator.ScenePath))
                throw new InvalidOperationException("Stage 3 scene is missing: " + Stage3SceneCreator.ScenePath);

            EditorSceneManager.OpenScene(Stage3SceneCreator.ScenePath);
            RequireObject("RtsGame");
            RequireObject("BoardRoot");
            RequireObject("Main Camera");
            RequireObject("Directional Light");
            RequireObject("EventSystem");
            RequireObject("Stage3 Canvas");

            RequireEnabled<RtsGameBootstrapper>("RtsGameBootstrapper");
            RequireEnabled<BoardRenderer>("BoardRenderer");
            RequireEnabled<ActorRenderSystem>("ActorRenderSystem");
            RequireEnabled<BoardPlacementController>("BoardPlacementController");
            RequireEnabled<BoardPlacementHud>("BoardPlacementHud");
            RequireEnabled<DesktopBoardPlacementInput>("DesktopBoardPlacementInput");
            RequireObject("Stage3 XR Rig Placeholder");
            RequireEnabled<Stage3XrRigPlaceholder>("Stage3XrRigPlaceholder");
            RequireEnabled<XrBoardPlacementInputAdapter>("XrBoardPlacementInputAdapter");
            RequireEnabled<EventSystem>("EventSystem");
            RequireEnabled<Canvas>("Canvas");

            var camera = RequireEnabled<Camera>("Main Camera");
            if (!camera.orthographic)
                throw new InvalidOperationException("Stage 3 fallback camera must be orthographic.");
            if (camera.nearClipPlane > 0.11f || camera.farClipPlane < 999f)
                throw new InvalidOperationException("Stage 3 camera clipping planes are not safe for board view.");

            Debug.Log("Stage 3 scene validation passed.");
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
