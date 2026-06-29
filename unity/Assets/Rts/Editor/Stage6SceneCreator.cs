using System;
using System.Collections.Generic;
using ProjectAegisRTS.UnityClient.Bootstrap;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.Rendering;
using ProjectAegisRTS.UnityClient.Rendering.Motion;
using ProjectAegisRTS.UnityClient.UI.Common;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage6SceneCreator
    {
        public const string ScenePath = "Assets/Rts/Scenes/Stage6_MovementVisualization.unity";

        [MenuItem("ProjectAegisRTS/Create Stage 6 Movement Visualization Scene")]
        public static void CreateStage6SceneMenu()
        {
            CreateOrUpdateStage6Scene();
        }

        public static void CreateStage6SceneBatch()
        {
            try
            {
                CreateOrUpdateStage6Scene();
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

        public static void CreateOrUpdateStage6Scene()
        {
            EnsureFolders();
            var profiles = Stage6MotionProfileAssetCreator.CreateOrUpdateProfileAssets();

            if (System.IO.File.Exists(Stage5SceneCreator.ScenePath))
                EditorSceneManager.OpenScene(Stage5SceneCreator.ScenePath, OpenSceneMode.Single);
            else
                Stage5SceneCreator.CreateOrUpdateStage5Scene();

            var scene = EditorSceneManager.GetActiveScene();

            var game = RequireObject("RtsGame");
            var boardRoot = RequireObject("BoardRoot");
            var mapper = UnityEngine.Object.FindFirstObjectByType<BoardCoordinateMapper>();
            var actorRenderer = UnityEngine.Object.FindFirstObjectByType<ActorRenderSystem>();
            var bootstrapper = UnityEngine.Object.FindFirstObjectByType<RtsGameBootstrapper>();
            var previewRenderer = UnityEngine.Object.FindFirstObjectByType<CommandPreviewRenderer>();
            var camera = Camera.main != null ? Camera.main : UnityEngine.Object.FindFirstObjectByType<Camera>();

            ConfigureCamera(camera);
            EnsureCanvasName();

            var profileLibrary = GetOrAdd<VisualMotionProfileLibrary>(game);
            profileLibrary.includeGeneratedDefaults = true;
            profileLibrary.profiles = profiles;
            profileLibrary.EnsureInitialized();

            var pathPreview = GetOrAdd<MovementPathPreview>(game);
            pathPreview.mapper = mapper;
            pathPreview.Initialize(mapper);

            if (previewRenderer == null)
                previewRenderer = game.AddComponent<CommandPreviewRenderer>();
            previewRenderer.mapper = mapper;
            previewRenderer.movementPathPreview = pathPreview;

            var movementHud = GetOrAdd<MovementDebugHud>(game);
            movementHud.actorRenderSystem = actorRenderer;
            movementHud.visible = true;

            if (actorRenderer != null)
                actorRenderer.motionProfileLibrary = profileLibrary;

            var showcaseObject = GameObject.Find("Stage6 Motion Showcase");
            if (showcaseObject == null)
                showcaseObject = new GameObject("Stage6 Motion Showcase");
            showcaseObject.transform.SetParent(game.transform, false);
            var showcase = GetOrAdd<Stage6MotionShowcase>(showcaseObject);
            showcase.profileLibrary = profileLibrary;
            showcase.movementPathPreview = pathPreview;
            showcase.EnsureShowcase();

            if (bootstrapper != null)
                bootstrapper.actorRenderSystem = actorRenderer;

            EditorSceneManager.SaveScene(scene, ScenePath);
            UpdateBuildScenes();
            AssetDatabase.SaveAssets();
            Debug.Log("Created Stage 6 scene at " + ScenePath);
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

        static void EnsureCanvasName()
        {
            var canvas = GameObject.Find("Canvas");
            if (canvas != null)
                return;

            var stageCanvas = GameObject.Find("Stage4 Canvas");
            if (stageCanvas != null)
                stageCanvas.name = "Canvas";
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

        static void EnsureFolders()
        {
            EnsureFolder("Assets", "Rts");
            EnsureFolder("Assets/Rts", "Scenes");
            EnsureFolder("Assets/Rts", "ScriptableObjects");
            EnsureFolder("Assets/Rts/ScriptableObjects", "MotionProfiles");
        }

        static void EnsureFolder(string parent, string child)
        {
            var path = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(path))
                AssetDatabase.CreateFolder(parent, child);
        }
    }
}
