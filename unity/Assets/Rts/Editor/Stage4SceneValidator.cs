using System;
using ProjectAegisRTS.UnityClient.Bootstrap;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.InputControls.Desktop;
using ProjectAegisRTS.UnityClient.InputControls.XR;
using ProjectAegisRTS.UnityClient.Rendering;
using ProjectAegisRTS.UnityClient.Selection;
using ProjectAegisRTS.UnityClient.UI.Common;
using ProjectAegisRTS.UnityClient.UI.XR.LeftHand;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage4SceneValidator
    {
        public static void ValidateStage4SceneBatch()
        {
            try
            {
                ValidateStage4Scene();
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

        public static void ValidateStage4Scene()
        {
            if (!System.IO.File.Exists(Stage4SceneCreator.ScenePath))
                throw new InvalidOperationException("Stage 4 scene is missing: " + Stage4SceneCreator.ScenePath);

            EditorSceneManager.OpenScene(Stage4SceneCreator.ScenePath);
            RequireObject("RtsGame");
            RequireObject("BoardRoot");
            RequireObject("Main Camera");
            RequireObject("Directional Light");
            RequireObject("EventSystem");
            RequireObject("Stage4 Canvas");
            RequireObject("Simulated Left Hand Rig");
            RequireObject("Left Hand Wrist Canvas");
            RequireObject("Stage4 Left Hand Controllers");

            RequireEnabled<RtsGameBootstrapper>("RtsGameBootstrapper");
            RequireEnabled<BoardRenderer>("BoardRenderer");
            RequireEnabled<ActorRenderSystem>("ActorRenderSystem");
            RequireEnabled<SimulatedLeftHandRig>("SimulatedLeftHandRig");
            RequireEnabled<DesktopLeftHandInputSource>("DesktopLeftHandInputSource");
            RequireEnabled<XrLeftHandInputAdapter>("XrLeftHandInputAdapter");
            RequireEnabled<LeftHandBuildMenuController>("LeftHandBuildMenuController");
            RequireEnabled<LeftHandRadialMenuView>("LeftHandRadialMenuView");
            RequireEnabled<LeftHandPlacementPanel>("LeftHandPlacementPanel");
            RequireEnabled<LeftHandSelectionPanel>("LeftHandSelectionPanel");
            RequireEnabled<LeftHandStatusHud>("LeftHandStatusHud");
            RequireEnabled<LeftHandSelectionController>("LeftHandSelectionController");
            RequireEnabled<LeftHandLassoSelectionController>("LeftHandLassoSelectionController");
            RequireEnabled<LeftHandCommandRouter>("LeftHandCommandRouter");
            RequireEnabled<Stage4ModeCoordinator>("Stage4ModeCoordinator");
            RequireEnabled<RtsStatusLog>("RtsStatusLog");
            RequireEnabled<EventSystem>("EventSystem");
            RequireEnabled<Canvas>("Canvas");

            var camera = RequireEnabled<Camera>("Main Camera");
            if (!camera.orthographic)
                throw new InvalidOperationException("Stage 4 fallback camera must be orthographic.");
            if (Mathf.Abs(camera.orthographicSize - 28f) > 0.01f)
                throw new InvalidOperationException("Stage 4 camera orthographic size should be 28.");
            if (camera.nearClipPlane > 0.11f || camera.farClipPlane < 999f)
                throw new InvalidOperationException("Stage 4 camera clipping planes are not safe for board view.");

            Debug.Log("Stage 4 scene validation passed.");
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
