using System;
using ProjectAegisRTS.UnityClient.Bootstrap;
using ProjectAegisRTS.UnityClient.CameraControls;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Rendering;
using ProjectAegisRTS.UnityClient.UI.Common;
using ProjectAegisRTS.UnityClient.UI.Desktop;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage2SceneValidator
    {
        public static void ValidateStage2SceneBatch()
        {
            try
            {
                ValidateStage2Scene();
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

        public static void ValidateStage2Scene()
        {
            if (!System.IO.File.Exists(Stage2SceneCreator.ScenePath))
                throw new InvalidOperationException("Stage 2 scene is missing: " + Stage2SceneCreator.ScenePath);

            EditorSceneManager.OpenScene(Stage2SceneCreator.ScenePath);
            RequireObject("RtsGame");
            RequireObject("BoardRoot");
            RequireObject("Main Camera");
            RequireObject("Directional Light");
            RequireObject("EventSystem");
            RequireObject("Stage2 Canvas");

            RequireEnabled<RtsGameBootstrapper>("RtsGameBootstrapper");
            RequireEnabled<RtsSimulationDriver>("RtsSimulationDriver");
            RequireEnabled<BoardRenderer>("BoardRenderer");
            RequireEnabled<ActorRenderSystem>("ActorRenderSystem");
            RequireEnabled<DesktopRtsHudRoot>("DesktopRtsHudRoot");
            RequireEnabled<DesktopSidebarController>("DesktopSidebarController");
            RequireEnabled<ProductionCategoryTabs>("ProductionCategoryTabs");
            RequireEnabled<ProductionGridController>("ProductionGridController");
            RequireEnabled<ProductionQueuePanel>("ProductionQueuePanel");
            RequireEnabled<PlacementModePanel>("PlacementModePanel");
            RequireEnabled<SelectionPanelController>("SelectionPanelController");
            RequireEnabled<CommandBarController>("CommandBarController");
            RequireEnabled<MinimapPlaceholderController>("MinimapPlaceholderController");
            RequireEnabled<RtsStatusLog>("RtsStatusLog");
            RequireEnabled<DesktopUiCommandRouter>("DesktopUiCommandRouter");
            RequireEnabled<EventSystem>("EventSystem component");
            RequireEnabled<Canvas>("Canvas");

            var camera = RequireEnabled<Camera>("Main Camera component");
            if (!camera.orthographic)
                throw new InvalidOperationException("Main Camera must be orthographic.");
            if (Mathf.Abs(camera.orthographicSize - 28f) > 0.01f)
                throw new InvalidOperationException("Main Camera orthographic size must be 28.");
            if (Mathf.Abs(camera.nearClipPlane - 0.1f) > 0.01f || Mathf.Abs(camera.farClipPlane - 1000f) > 0.01f)
                throw new InvalidOperationException("Main Camera clipping planes must be 0.1 / 1000.");
            if (Vector3.Distance(camera.transform.position, new Vector3(16f, 38f, -26f)) > 0.01f)
                throw new InvalidOperationException("Main Camera position is not the Stage 1.6 safe framing.");

            RequireEnabled<RtsCameraController>("RtsCameraController");
            Debug.Log("Stage 2 scene validation passed.");
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
