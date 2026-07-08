using System;
using System.Collections.Generic;
using ProjectAegisRTS.UnityClient.Bootstrap;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Rendering.Visibility;
using ProjectAegisRTS.UnityClient.UI.Common;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage11SceneCreator
    {
        public const string ScenePath = "Assets/Rts/Scenes/Stage11_FogRadarMinimap.unity";

        [MenuItem("ProjectAegisRTS/Stage 11/Create Fog Radar Minimap Scene")]
        public static void CreateStage11SceneMenu()
        {
            CreateOrUpdateStage11Scene();
        }

        public static void CreateStage11SceneBatch()
        {
            try
            {
                CreateOrUpdateStage11Scene();
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

        public static void CreateOrUpdateStage11Scene()
        {
            if (System.IO.File.Exists(Stage10SceneCreator.ScenePath))
                EditorSceneManager.OpenScene(Stage10SceneCreator.ScenePath);
            else
                Stage10SceneCreator.CreateOrUpdateStage10Scene();

            var scene = EditorSceneManager.GetActiveScene();
            var game = RequireObject("RtsGame");
            ConfigureCamera(Camera.main != null ? Camera.main : UnityEngine.Object.FindFirstObjectByType<Camera>());

            var driver = GetOrAdd<RtsSimulationDriver>(game);
            driver.UseFogRadarDemoWorld = true;
            driver.UseEconomyDemoWorld = false;
            driver.UseCombatDemoWorld = false;
            driver.UsePlayerPerspectiveSnapshot = true;

            var mapper = UnityEngine.Object.FindFirstObjectByType<BoardCoordinateMapper>();
            var fogOverlay = GetOrAdd<FogOverlayRenderer>(game);
            var visibilityDebug = GetOrAdd<VisibilityDebugRenderer>(game);
            var radar = GetOrAdd<RadarSnapshotAdapter>(game);
            var minimap = GetOrAdd<MinimapRenderSystem>(game);
            var hud = GetOrAdd<FogDebugHud>(game);
            var bootstrapper = GetOrAdd<RtsGameBootstrapper>(game);

            fogOverlay.driver = driver;
            fogOverlay.mapper = mapper;
            visibilityDebug.driver = driver;
            visibilityDebug.mapper = mapper;
            radar.driver = driver;
            minimap.driver = driver;
            minimap.mapper = mapper;
            hud.driver = driver;
            hud.fogOverlayRenderer = fogOverlay;
            hud.visibilityDebugRenderer = visibilityDebug;
            hud.radarSnapshotAdapter = radar;
            hud.minimapRenderSystem = minimap;
            hud.visible = true;

            bootstrapper.simulationDriver = driver;
            bootstrapper.fogOverlayRenderer = fogOverlay;
            bootstrapper.visibilityDebugRenderer = visibilityDebug;
            bootstrapper.radarSnapshotAdapter = radar;
            bootstrapper.minimapRenderSystem = minimap;
            bootstrapper.startPaused = false;

            EditorSceneManager.SaveScene(scene, ScenePath);
            UpdateBuildScenes();
            AssetDatabase.SaveAssets();
            Debug.Log("Created Stage 11 scene at " + ScenePath);
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
