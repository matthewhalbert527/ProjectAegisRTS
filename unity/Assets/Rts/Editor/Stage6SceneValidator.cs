using System;
using ProjectAegisRTS.UnityClient.Bootstrap;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Rendering;
using ProjectAegisRTS.UnityClient.Rendering.Motion;
using ProjectAegisRTS.UnityClient.UI.Common;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage6SceneValidator
    {
        public static void ValidateStage6SceneBatch()
        {
            try
            {
                ValidateStage6Scene();
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

        public static void ValidateStage6Scene()
        {
            if (!System.IO.File.Exists(Stage6SceneCreator.ScenePath))
                throw new InvalidOperationException("Stage 6 scene is missing: " + Stage6SceneCreator.ScenePath);

            EditorSceneManager.OpenScene(Stage6SceneCreator.ScenePath);
            RequireObject("RtsGame");
            RequireObject("BoardRoot");
            RequireObject("Main Camera");
            RequireObject("Directional Light");
            RequireObject("EventSystem");
            RequireObject("Canvas");
            RequireObject("Stage6 Motion Showcase");
            RequireObject("Stage6 Vehicle Motion Placeholder");
            RequireObject("Stage6 Infantry Motion Placeholder");
            RequireObject("Stage6 Aircraft Motion Placeholder");

            RequireEnabled<RtsGameBootstrapper>("RtsGameBootstrapper");
            RequireEnabled<BoardRenderer>("BoardRenderer");
            var actorRenderer = RequireEnabled<ActorRenderSystem>("ActorRenderSystem");
            RequireEnabled<RtsStatusLog>("RtsStatusLog");
            RequireEnabled<CommandPreviewRenderer>("CommandPreviewRenderer");
            var profileLibrary = RequireEnabled<VisualMotionProfileLibrary>("VisualMotionProfileLibrary");
            RequireEnabled<MovementPathPreview>("MovementPathPreview");
            RequireEnabled<MovementDebugHud>("MovementDebugHud");
            var showcase = RequireEnabled<Stage6MotionShowcase>("Stage6MotionShowcase");
            RequireEnabled<EventSystem>("EventSystem");
            RequireEnabled<Canvas>("Canvas");

            if (actorRenderer.motionProfileLibrary == null)
                throw new InvalidOperationException("ActorRenderSystem is not linked to the Stage 6 motion profile library.");

            profileLibrary.EnsureInitialized();
            if (profileLibrary.ProfileCount < 12)
                throw new InvalidOperationException("Stage 6 motion profile library has too few profiles: " + profileLibrary.ProfileCount);

            RequireProfile(profileLibrary, "scout_rover", VisualMotionCategory.Vehicle, "wheeled_scout");
            RequireProfile(profileLibrary, "rifle_infantry", VisualMotionCategory.Infantry, "infantry_basic");
            RequireProfile(profileLibrary, "harvester", VisualMotionCategory.Harvester, "wheeled_heavy");
            RequireProfile(profileLibrary, "attack_aircraft", VisualMotionCategory.Aircraft, "aircraft_attack");
            RequireProfile(profileLibrary, "fabrication_hub", VisualMotionCategory.Building, "building_static");
            RequireProfile(profileLibrary, "gun_tower", VisualMotionCategory.Defense, "default_defense");

            showcase.EnsureShowcase();
            if (showcase.VehicleMotion == null || showcase.InfantryMotion == null || showcase.AircraftMotion == null || showcase.TurretAim == null)
                throw new InvalidOperationException("Stage 6 motion showcase is missing one or more motion controllers.");

            var camera = RequireEnabled<Camera>("Main Camera");
            if (!camera.orthographic)
                throw new InvalidOperationException("Stage 6 camera must be orthographic.");
            if (Mathf.Abs(camera.orthographicSize - 28f) > 0.01f)
                throw new InvalidOperationException("Stage 6 camera orthographic size should be 28.");
            if (camera.nearClipPlane > 0.11f || camera.farClipPlane < 999f)
                throw new InvalidOperationException("Stage 6 camera clipping planes are not safe for board view.");

            Debug.Log("Stage 6 scene validation passed.");
        }

        static void RequireProfile(VisualMotionProfileLibrary library, string typeId, VisualMotionCategory category, string profileId)
        {
            var profile = library.GetProfile(typeId, category, profileId);
            if (profile == null)
                throw new InvalidOperationException("Missing Stage 6 profile for " + typeId + " / " + profileId);
            if (profile.category != category)
                throw new InvalidOperationException("Stage 6 profile category mismatch for " + profileId + ": " + profile.category + " expected " + category);
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
