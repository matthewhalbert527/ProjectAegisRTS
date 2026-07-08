using System;
using System.Collections.Generic;
using ProjectAegisRTS.UnityClient.Bootstrap;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Rendering.Ai;
using ProjectAegisRTS.UnityClient.UI.Common;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage12SceneCreator
    {
        public const string ScenePath = "Assets/Rts/Scenes/Stage12_AISkirmishFoundation.unity";

        [MenuItem("ProjectAegisRTS/Stage 12/Create AI Skirmish Foundation Scene")]
        public static void CreateStage12SceneMenu()
        {
            CreateOrUpdateStage12Scene();
        }

        public static void CreateStage12SceneBatch()
        {
            try
            {
                CreateOrUpdateStage12Scene();
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

        public static void CreateOrUpdateStage12Scene()
        {
            if (System.IO.File.Exists(Stage11SceneCreator.ScenePath))
                EditorSceneManager.OpenScene(Stage11SceneCreator.ScenePath);
            else
                Stage11SceneCreator.CreateOrUpdateStage11Scene();

            var scene = EditorSceneManager.GetActiveScene();
            var game = RequireObject("RtsGame");
            ConfigureCamera(Camera.main != null ? Camera.main : UnityEngine.Object.FindFirstObjectByType<Camera>());

            var driver = GetOrAdd<RtsSimulationDriver>(game);
            driver.UseAiSkirmishDemoWorld = true;
            driver.UseFogRadarDemoWorld = false;
            driver.UseEconomyDemoWorld = false;
            driver.UseCombatDemoWorld = false;
            driver.UsePlayerPerspectiveSnapshot = false;

            var mapper = UnityEngine.Object.FindFirstObjectByType<BoardCoordinateMapper>();
            var aiIntents = GetOrAdd<AiIntentRenderSystem>(game);
            var timeline = GetOrAdd<AiPlanTimelineView>(game);
            var hud = GetOrAdd<AiDebugHud>(game);
            var bootstrapper = GetOrAdd<RtsGameBootstrapper>(game);

            aiIntents.driver = driver;
            aiIntents.mapper = mapper;
            timeline.driver = driver;
            hud.driver = driver;
            hud.aiIntentRenderSystem = aiIntents;
            hud.aiPlanTimelineView = timeline;
            hud.visible = true;

            bootstrapper.simulationDriver = driver;
            bootstrapper.aiIntentRenderSystem = aiIntents;
            bootstrapper.aiPlanTimelineView = timeline;
            bootstrapper.startPaused = false;

            EditorSceneManager.SaveScene(scene, ScenePath);
            UpdateBuildScenes();
            AssetDatabase.SaveAssets();
            Debug.Log("Created Stage 12 scene at " + ScenePath);
        }

        static void ConfigureCamera(Camera camera)
        {
            if (camera == null)
                return;
            camera.orthographic = true;
            camera.orthographicSize = 28f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 1000f;
            camera.transform.position = new Vector3(16f, 38f, -26f);
            camera.transform.rotation = Quaternion.Euler(60f, 0f, 0f);
        }

        static void UpdateBuildScenes()
        {
            var paths = new[]
            {
                "Assets/Rts/Scenes/Stage1_DesktopBoard.unity",
                "Assets/Rts/Scenes/Stage2_PCSidebar.unity",
                "Assets/Rts/Scenes/Stage3_XRBoardPlacement.unity",
                "Assets/Rts/Scenes/Stage4_LeftHandBuildSelection.unity",
                "Assets/Rts/Scenes/Stage5_DualHandCommand.unity",
                "Assets/Rts/Scenes/Stage6_MovementVisualization.unity",
                Stage7SceneCreator.ScenePath,
                Stage8SceneCreator.ScenePath,
                Stage9SceneCreator.ScenePath,
                Stage10SceneCreator.ScenePath,
                Stage11SceneCreator.ScenePath,
                ScenePath
            };

            var scenes = new List<EditorBuildSettingsScene>();
            for (var i = 0; i < paths.Length; i++)
                if (System.IO.File.Exists(paths[i]))
                    scenes.Add(new EditorBuildSettingsScene(paths[i], true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        static GameObject RequireObject(string name)
        {
            var obj = GameObject.Find(name);
            if (obj == null)
                throw new InvalidOperationException("Missing GameObject: " + name);
            return obj;
        }

        static T GetOrAdd<T>(GameObject target) where T : Component
        {
            var component = target.GetComponent<T>();
            return component != null ? component : target.AddComponent<T>();
        }
    }
}
