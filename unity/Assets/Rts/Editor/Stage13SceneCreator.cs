using System;
using System.Collections.Generic;
using ProjectAegisRTS.UnityClient.Bootstrap;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Rendering.Map;
using ProjectAegisRTS.UnityClient.UI.Common;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage13SceneCreator
    {
        public const string ScenePath = "Assets/Rts/Scenes/Stage13_MapTerrainPathing.unity";

        [MenuItem("ProjectAegisRTS/Stage 13/Create Map Terrain Pathing Scene")]
        public static void CreateStage13SceneMenu()
        {
            CreateOrUpdateStage13Scene();
        }

        public static void CreateStage13SceneBatch()
        {
            try
            {
                CreateOrUpdateStage13Scene();
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

        public static void CreateOrUpdateStage13Scene()
        {
            if (System.IO.File.Exists(Stage12SceneCreator.ScenePath))
                EditorSceneManager.OpenScene(Stage12SceneCreator.ScenePath);
            else
                Stage12SceneCreator.CreateOrUpdateStage12Scene();

            var scene = EditorSceneManager.GetActiveScene();
            var game = RequireObject("RtsGame");
            ConfigureCamera(Camera.main != null ? Camera.main : UnityEngine.Object.FindFirstObjectByType<Camera>());

            var driver = GetOrAdd<RtsSimulationDriver>(game);
            driver.UseMapTerrainDemoWorld = true;
            driver.UseAiSkirmishDemoWorld = false;
            driver.UseFogRadarDemoWorld = false;
            driver.UseEconomyDemoWorld = false;
            driver.UseCombatDemoWorld = false;
            driver.UsePlayerPerspectiveSnapshot = false;

            var mapper = UnityEngine.Object.FindFirstObjectByType<BoardCoordinateMapper>();
            var terrain = GetOrAdd<TerrainDebugRenderer>(game);
            var path = GetOrAdd<PathDebugRenderer>(game);
            var authoring = GetOrAdd<MapAuthoringOverlay>(game);
            var hud = GetOrAdd<MapValidationDebugHud>(game);
            var bootstrapper = GetOrAdd<RtsGameBootstrapper>(game);

            terrain.driver = driver;
            terrain.mapper = mapper;
            path.driver = driver;
            path.mapper = mapper;
            authoring.driver = driver;
            authoring.mapper = mapper;
            hud.driver = driver;
            hud.terrainDebugRenderer = terrain;
            hud.pathDebugRenderer = path;
            hud.mapAuthoringOverlay = authoring;
            hud.visible = true;

            bootstrapper.simulationDriver = driver;
            bootstrapper.terrainDebugRenderer = terrain;
            bootstrapper.pathDebugRenderer = path;
            bootstrapper.mapAuthoringOverlay = authoring;
            bootstrapper.startPaused = false;

            EditorSceneManager.SaveScene(scene, ScenePath);
            UpdateBuildScenes();
            AssetDatabase.SaveAssets();
            Debug.Log("Created Stage 13 scene at " + ScenePath);
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
                Stage12SceneCreator.ScenePath,
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
