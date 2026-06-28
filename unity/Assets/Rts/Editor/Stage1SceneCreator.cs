using System;
using ProjectAegisRTS.UnityClient.Bootstrap;
using ProjectAegisRTS.UnityClient.CameraControls;
using ProjectAegisRTS.UnityClient.CoreBridge;
using ProjectAegisRTS.UnityClient.InputControls;
using ProjectAegisRTS.UnityClient.Rendering;
using ProjectAegisRTS.UnityClient.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectAegisRTS.UnityClient.EditorTools
{
    public static class Stage1SceneCreator
    {
        const string ScenePath = "Assets/Rts/Scenes/Stage1_DesktopBoard.unity";

        [MenuItem("ProjectAegisRTS/Create Stage 1 Desktop Board Scene")]
        public static void CreateStage1SceneMenu()
        {
            CreateOrUpdateStage1Scene();
        }

        public static void CreateStage1SceneBatch()
        {
            try
            {
                CreateOrUpdateStage1Scene();
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

        public static void CreateOrUpdateStage1Scene()
        {
            EnsureFolders();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Stage1_DesktopBoard";

            var boardRoot = new GameObject("BoardRoot");
            var mapper = boardRoot.AddComponent<BoardCoordinateMapper>();
            var boardRenderer = boardRoot.AddComponent<BoardRenderer>();

            var game = new GameObject("RtsGame");
            var bootstrapper = game.AddComponent<RtsGameBootstrapper>();
            var driver = game.AddComponent<RtsSimulationDriver>();
            var actorRenderer = game.AddComponent<ActorRenderSystem>();
            var input = game.AddComponent<RtsDesktopInputController>();
            var hud = game.AddComponent<RtsDebugHud>();

            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 28f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 1000f;
            cameraObject.transform.position = new Vector3(16f, 38f, -26f);
            cameraObject.transform.rotation = Quaternion.Euler(60f, 0f, 0f);
            var cameraController = cameraObject.AddComponent<RtsCameraController>();

            var lightObject = new GameObject("Directional Light");
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.15f;
            lightObject.transform.rotation = Quaternion.Euler(50f, -35f, 0f);

            bootstrapper.boardRoot = boardRoot.transform;
            bootstrapper.coordinateMapper = mapper;
            bootstrapper.boardRenderer = boardRenderer;
            bootstrapper.actorRenderSystem = actorRenderer;
            bootstrapper.simulationDriver = driver;
            bootstrapper.inputController = input;
            bootstrapper.debugHud = hud;
            bootstrapper.sceneCamera = camera;
            bootstrapper.cameraController = cameraController;

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            Debug.Log("Created Stage 1 scene at " + ScenePath);
        }

        static void EnsureFolders()
        {
            EnsureFolder("Assets", "Rts");
            EnsureFolder("Assets/Rts", "Scenes");
            EnsureFolder("Assets/Rts", "Materials");
            EnsureFolder("Assets/Rts", "Prefabs");
            EnsureFolder("Assets/Rts", "ScriptableObjects");
            EnsureFolder("Assets/Rts", "Plugins");
            EnsureFolder("Assets/Rts/Plugins", "RtsCore");
        }

        static void EnsureFolder(string parent, string child)
        {
            var path = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(path))
                AssetDatabase.CreateFolder(parent, child);
        }
    }
}
