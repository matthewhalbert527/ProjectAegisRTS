using System;
using ProjectAegisRTS.UnityClient.Bootstrap;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Rendering;
using ProjectAegisRTS.UnityClient.Rendering.Buildings;
using ProjectAegisRTS.UnityClient.Rendering.Motion;
using ProjectAegisRTS.UnityClient.UI.Common;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage7SceneValidator
    {
        public static void ValidateStage7SceneBatch()
        {
            try
            {
                ValidateStage7Scene();
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

        public static void ValidateStage7Scene()
        {
            if (!System.IO.File.Exists(Stage7SceneCreator.ScenePath))
                throw new InvalidOperationException("Stage 7 scene is missing: " + Stage7SceneCreator.ScenePath);

            EditorSceneManager.OpenScene(Stage7SceneCreator.ScenePath);
            RequireObject("RtsGame");
            RequireObject("BoardRoot");
            RequireObject("Main Camera");
            RequireObject("Directional Light");
            RequireObject("EventSystem");
            RequireObject("Canvas");
            RequireObject("Stage7 Placeholder Buildings");

            RequireEnabled<RtsGameBootstrapper>("RtsGameBootstrapper");
            RequireEnabled<BoardRenderer>("BoardRenderer");
            var actorRenderer = RequireEnabled<ActorRenderSystem>("ActorRenderSystem");
            RequireEnabled<RtsStatusLog>("RtsStatusLog");
            RequireEnabled<VisualMotionProfileLibrary>("VisualMotionProfileLibrary");
            var buildingLibrary = RequireEnabled<BuildingVisualProfileLibrary>("BuildingVisualProfileLibrary");
            RequireEnabled<BuildingAnimationDebugHud>("BuildingAnimationDebugHud");
            RequireEnabled<BuildingPowerDemoController>("BuildingPowerDemoController");
            RequireEnabled<EventSystem>("EventSystem");
            RequireEnabled<Canvas>("Canvas");

            RequireEnabled<BuildingVisualStateController>("BuildingVisualStateController");
            RequireEnabled<BuildingLightVisualController>("BuildingLightVisualController");
            RequireEnabled<BuildingMachineryVisualController>("BuildingMachineryVisualController");
            RequireEnabled<BuildingProductionVisualController>("BuildingProductionVisualController");
            RequireEnabled<BuildingDoorVisualController>("BuildingDoorVisualController");
            RequireEnabled<BuildingDamageVisualController>("BuildingDamageVisualController");
            RequireEnabled<BuildingSpecificLoopController>("BuildingSpecificLoopController");

            if (actorRenderer.buildingProfileLibrary == null)
                throw new InvalidOperationException("ActorRenderSystem is not linked to BuildingVisualProfileLibrary.");

            buildingLibrary.EnsureInitialized();
            if (buildingLibrary.ProfileCount < 16)
                throw new InvalidOperationException("Stage 7 building profile library has too few profiles: " + buildingLibrary.ProfileCount);

            RequireProfile(buildingLibrary, "power_plant");
            RequireProfile(buildingLibrary, "barracks");
            RequireProfile(buildingLibrary, "war_factory");
            RequireProfile(buildingLibrary, "refinery");
            RequireProfile(buildingLibrary, "gun_tower");
            RequireProfile(buildingLibrary, "fabrication_hub");

            RequireSceneFile("Assets/Rts/Scenes/Stage1_DesktopBoard.unity");
            RequireSceneFile(Stage2SceneCreator.ScenePath);
            RequireSceneFile(Stage3SceneCreator.ScenePath);
            RequireSceneFile(Stage4SceneCreator.ScenePath);
            RequireSceneFile(Stage5SceneCreator.ScenePath);
            RequireSceneFile(Stage6SceneCreator.ScenePath);

            var camera = RequireEnabled<Camera>("Main Camera");
            if (!camera.orthographic)
                throw new InvalidOperationException("Stage 7 camera must be orthographic.");
            if (Mathf.Abs(camera.orthographicSize - 28f) > 0.01f)
                throw new InvalidOperationException("Stage 7 camera orthographic size should be 28.");
            if (camera.nearClipPlane > 0.11f || camera.farClipPlane < 999f)
                throw new InvalidOperationException("Stage 7 camera clipping planes are not safe for board view.");

            Debug.Log("Stage 7 scene validation passed.");
        }

        static void RequireProfile(BuildingVisualProfileLibrary library, string typeId)
        {
            var profile = library.GetProfile(typeId, null);
            if (profile == null)
                throw new InvalidOperationException("Missing Stage 7 building profile for " + typeId);
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
