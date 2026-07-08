using System;
using ProjectAegisRTS.UnityClient.Bootstrap;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.InputControls.Desktop;
using ProjectAegisRTS.UnityClient.InputControls.XR;
using ProjectAegisRTS.UnityClient.Rendering;
using ProjectAegisRTS.UnityClient.UI.XR.LeftHand;
using ProjectAegisRTS.UnityClient.UI.XR.RightHand;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage5SceneValidator
    {
        public static void ValidateStage5SceneBatch()
        {
            try
            {
                ValidateStage5Scene();
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

        public static void ValidateStage5Scene()
        {
            if (!System.IO.File.Exists(Stage5SceneCreator.ScenePath))
                throw new InvalidOperationException("Stage 5 scene is missing: " + Stage5SceneCreator.ScenePath);

            EditorSceneManager.OpenScene(Stage5SceneCreator.ScenePath);
            RequireObject("RtsGame");
            RequireObject("BoardRoot");
            RequireObject("Main Camera");
            RequireObject("Directional Light");
            RequireObject("EventSystem");
            RequireObject("Stage4 Canvas");
            RequireObject("Simulated Left Hand Rig");
            RequireObject("Simulated Right Hand Rig");
            RequireObject("Stage4 Left Hand Controllers");
            RequireObject("Stage5 Right Hand Controllers");
            RequireObject("Right Hand Wrist Canvas");
            RequireObject("Right Hand Command HUD");
            RequireObject("Right Hand Status Panel");

            RequireEnabled<RtsGameBootstrapper>("RtsGameBootstrapper");
            RequireEnabled<BoardRenderer>("BoardRenderer");
            RequireEnabled<ActorRenderSystem>("ActorRenderSystem");
            RequireEnabled<CommandPreviewRenderer>("CommandPreviewRenderer");
            RequireEnabled<Stage4ModeCoordinator>("Stage4ModeCoordinator");
            RequireEnabled<SimulatedRightHandRig>("SimulatedRightHandRig");
            RequireEnabled<DesktopRightHandInputSource>("DesktopRightHandInputSource");
            RequireEnabled<XrRightHandInputAdapter>("XrRightHandInputAdapter");
            RequireEnabled<RightHandCommandRouter>("RightHandCommandRouter");
            RequireEnabled<RightHandCommandHud>("RightHandCommandHud");
            RequireEnabled<RightHandCommandReticle>("RightHandCommandReticle");
            RequireEnabled<RightHandStatusPanel>("RightHandStatusPanel");
            RequireEnabled<Stage5DualHandModeCoordinator>("Stage5DualHandModeCoordinator");
            RequireEnabled<EventSystem>("EventSystem");
            RequireEnabled<Canvas>("Canvas");

            var camera = RequireEnabled<Camera>("Main Camera");
            if (!camera.orthographic)
                throw new InvalidOperationException("Stage 5 fallback camera must be orthographic.");
            if (camera.nearClipPlane > 0.11f || camera.farClipPlane < 999f)
                throw new InvalidOperationException("Stage 5 camera clipping planes are not safe for board view.");

            Debug.Log("Stage 5 scene validation passed.");
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
