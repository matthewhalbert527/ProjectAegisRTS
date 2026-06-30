using System;
using ProjectAegisRTS.UnityClient.Bootstrap;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Rendering;
using ProjectAegisRTS.UnityClient.Rendering.Combat;
using ProjectAegisRTS.UnityClient.UI.Common;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage9SceneValidator
    {
        public static void ValidateStage9SceneBatch()
        {
            try
            {
                ValidateStage9Scene();
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

        public static void ValidateStage9Scene()
        {
            if (!System.IO.File.Exists(Stage9SceneCreator.ScenePath))
                throw new InvalidOperationException("Stage 9 scene missing: " + Stage9SceneCreator.ScenePath);

            var scene = EditorSceneManager.OpenScene(Stage9SceneCreator.ScenePath);
            if (!scene.IsValid())
                throw new InvalidOperationException("Stage 9 scene did not open.");

            RequireObject("RtsGame");
            RequireObject("BoardRoot");
            var camera = Camera.main != null ? Camera.main : UnityEngine.Object.FindFirstObjectByType<Camera>();
            if (camera == null || !camera.orthographic || Mathf.Abs(camera.orthographicSize - 28f) > 0.01f)
                throw new InvalidOperationException("Stage 9 camera is missing or not framed.");
            if (UnityEngine.Object.FindFirstObjectByType<Light>() == null)
                throw new InvalidOperationException("Stage 9 directional light missing.");
            if (UnityEngine.Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
                throw new InvalidOperationException("Stage 9 EventSystem missing.");
            if (UnityEngine.Object.FindFirstObjectByType<Canvas>() == null)
                throw new InvalidOperationException("Stage 9 Canvas missing.");

            Require<RtsGameBootstrapper>("RtsGameBootstrapper");
            Require<BoardRenderer>("BoardRenderer");
            Require<ActorRenderSystem>("ActorRenderSystem");
            Require<RtsSimulationDriver>("RtsSimulationDriver");
            Require<CombatVisualProfileLibrary>("CombatVisualProfileLibrary");
            Require<ProjectileRenderSystem>("ProjectileRenderSystem");
            Require<CombatEventRenderSystem>("CombatEventRenderSystem");
            Require<CombatDebugHud>("CombatDebugHud");

            var profiles = UnityEngine.Object.FindFirstObjectByType<CombatVisualProfileLibrary>();
            profiles.EnsureInitialized();
            if (profiles.ProfileCount < 6)
                throw new InvalidOperationException("Stage 9 combat visual profile count is incomplete.");

            var previousScenes = new[]
            {
                "Assets/Rts/Scenes/Stage1_DesktopBoard.unity",
                "Assets/Rts/Scenes/Stage2_PCSidebar.unity",
                "Assets/Rts/Scenes/Stage3_XRBoardPlacement.unity",
                "Assets/Rts/Scenes/Stage4_LeftHandBuildSelection.unity",
                "Assets/Rts/Scenes/Stage5_DualHandCommand.unity",
                "Assets/Rts/Scenes/Stage6_MovementVisualization.unity",
                Stage7SceneCreator.ScenePath,
                Stage8SceneCreator.ScenePath
            };

            for (var i = 0; i < previousScenes.Length; i++)
                if (!System.IO.File.Exists(previousScenes[i]))
                    throw new InvalidOperationException("Previous stage scene missing: " + previousScenes[i]);

            Debug.Log("Stage 9 scene validation passed.");
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
